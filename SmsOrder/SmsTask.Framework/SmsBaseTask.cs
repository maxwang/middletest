using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsTask.Framework.Models;
using SmsTask.Framework.Repository;

namespace SmsTask.Framework
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public abstract class SmsBaseTask : IDisposable
    {
        public event EventHandler<MessageEventArgs> DisplayMessage;

        public string TaskName { get; set; }

        //we could get from action
        protected string FirstTasActionName => (Actions.Length < 1 ? string.Empty : Actions[0].TaskActionName);
        protected bool Disposed = false;
        protected readonly CancellationToken CancellationToken;
        protected readonly ITaskRepository TaskRepository;
        
        protected SmsBaseTaskAction[] Actions;
        protected List<string> DependencyTasks;
        protected readonly string LogUserName;

        protected SmsBaseTask(ITaskRepository taskRepository, CancellationToken token)
        {
            CancellationToken = token;
            TaskRepository = taskRepository;
            LogUserName = "OrderTask";
            DependencyTasks = new List<string>();
        }

        #region Order Task three main functions 
        
        protected virtual async Task<TaskResult> TaskPrecheckAsync(int orderId)
        {
            var result = await CheckDependenciesAsync(orderId);

            if (result.ResultStatus == SmsTaskStatus.WaitForDependency)
            {
                return result;
            }

            return await CreateInitTasksIfNeededAsync(orderId);
        }


        protected virtual async Task<TaskResult> TaskPostcheckAsync(int orderId)
        {
            return await Task.FromResult(new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success
            });
        }

        public async Task<TaskResult> ProcessTaskActionsAsync(int orderId)
        {

            OnDisplayMessage($"[Order:{orderId}]start processing {TaskName}");

            if (CancellationToken.IsCancellationRequested)
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.TaskCancelled
                };
            }

            await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
            {
                OrderId = orderId,
                Comment = "Start Order Prechecking",
                CreatedTime = DateTime.Now,
                CreatedBy = LogUserName
            });

            TaskResult result = await TaskPrecheckAsync(orderId);
            
            if (result.ResultStatus == SmsTaskStatus.WaitForDependency)
            {
                if (!await TaskRepository.AnyOrderActionLogAsync(orderId))
                {
                    //check dependency only need save it once, otherwise too many check dependency logs
                    await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                    {
                        OrderId = orderId,
                        Comment = result.Message,
                        CreatedTime = DateTime.Now,
                        CreatedBy = LogUserName
                    });
                }
            }
            else
            {
                await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                {
                    OrderId = orderId,
                    Comment = "Start Order Prechecking",
                    Details = JsonConvert.SerializeObject(result),
                    CreatedTime = DateTime.Now,
                    CreatedBy = LogUserName
                });
            }

            if (result.ResultStatus != SmsTaskStatus.Success && result.ResultStatus != SmsTaskStatus.CouldRunNext)
            {
                return result;
            }

            var tasks = await TaskRepository.GetOrderTasksAsync(orderId, TaskName);

            var lastTaskStatus = SmsTaskStatus.Success;

            //update
            OnDisplayMessage($"[Order:{orderId}, Task:{TaskName}]:start processing");
            StringBuilder message = new StringBuilder();

            foreach (var task in tasks)
            {
                if (!CancellationToken.IsCancellationRequested)
                {
                    TaskResult actionResult = await ProcessTaskActionAsync(task);
                    lastTaskStatus = actionResult.ResultStatus;
                    if (lastTaskStatus != SmsTaskStatus.Success && lastTaskStatus != SmsTaskStatus.CouldRunNext)
                    {
                        message.AppendLine(actionResult.Message);
                        break;
                    }
                }    
            }

            if (lastTaskStatus == SmsTaskStatus.Success || lastTaskStatus == SmsTaskStatus.CouldRunNext)
            {
                result = await TaskPostcheckAsync(orderId);
                lastTaskStatus = result.ResultStatus;
                message.AppendLine(result.Message);

                await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                {
                    OrderId = orderId,
                    Details = result.Message,
                    Comment = $"PostCheck:{lastTaskStatus}",
                    CreatedTime = DateTime.Now,
                    CreatedBy = LogUserName
                });

            }

            return new TaskResult
            {
                ResultStatus = lastTaskStatus,
                Message = message.ToString()
            };
            
        }
        

        #endregion

        private async Task<TaskResult> ProcessTaskActionAsync(OrderTask task)
        {
            OnDisplayMessage($"[Order:{task.OrderId}, {TaskName} TaskId:{task.OrderTaskId}]: Start Processing");

            await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
            {
                OrderId = task.OrderId,
                OrderTaskId = task.OrderTaskId,
                Comment = "Start Processing",
                CreatedTime = DateTime.Now,
                CreatedBy = LogUserName
            });

            await TaskRepository.UpdateTaskStatusAsync(task.OrderTaskId, SmsTaskStatus.Processing.ToString(), LogUserName);

            TaskResult lastActionResult= new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success
            };

            foreach (var action in Actions)
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    OnDisplayMessage($"{TaskName}[{action.TaskActionName}] Cancelled");
                    lastActionResult.ResultStatus = SmsTaskStatus.TaskCancelled;
                    break;
                }
                
                lastActionResult = await action.ProcessAsync(task.OrderTaskId);

                if (lastActionResult.ResultStatus != SmsTaskStatus.Success && lastActionResult.ResultStatus != SmsTaskStatus.CouldRunNext)
                {
                    lastActionResult.Message = lastActionResult.Message;
                    break;
                }
            }

            OnDisplayMessage($"[Order:{task.OrderId}, {TaskName} TaskId:{task.OrderTaskId}]:{lastActionResult.ResultStatus}");

            await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
            {
                OrderId = task.OrderId,
                OrderTaskId = task.OrderTaskId,
                Comment = lastActionResult.ResultStatus.ToString(),
                Details = lastActionResult.Message,
                CreatedTime = DateTime.Now,
                CreatedBy = LogUserName
            });

            await TaskRepository.UpdateTaskStatusAsync(task.OrderTaskId, lastActionResult.ResultStatus.ToString(), LogUserName);

            return lastActionResult;
        }
        
        protected virtual async Task<TaskResult> CreateInitTasksIfNeededAsync(int orderId)
        {
            if (await TaskRepository.AnyOrderTaskAsync(orderId, TaskName))
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Success,
                    Message = "Task created before, do not need create it this time"
                };
            }

            var taskId = await TaskRepository.InsertOrderTaskAsync(new OrderTask
            {
                OrderId = orderId,
                Status = SmsTaskStatus.New.ToString(),
                CreatedBy = LogUserName,
                CreatedTime = DateTime.Now,
                LastUpdatedBy = LogUserName,
                LastUpdatedTime = DateTime.Now,
                TaskName = TaskName
            });

            return new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success,
                Message = $"Task Created:{taskId}"
            };
        }

        

        protected virtual async Task<TaskResult> CheckDependenciesAsync(int orderId)
        {
            TaskResult taskResult = new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success
            };

            if (DependencyTasks.Count == 0)
            {
                return taskResult;
            }

            foreach (var dependency in DependencyTasks)
            {
                taskResult = await CheckDependencyAsync(orderId, dependency);
                if (taskResult.ResultStatus == SmsTaskStatus.WaitForDependency)
                {
                    break;
                }
            }

            return taskResult;
        }

        protected virtual async Task<TaskResult> CheckDependencyAsync(int orderId, string dependencyTaskName)
        {
            var tasks = await TaskRepository.GetOrderTasksAsync(orderId, dependencyTaskName);
            if (tasks != null && tasks.Count > 0 &&
                tasks.TrueForAll(x => x.Status.Equals(SmsTaskStatus.Success.ToString(),
                                    StringComparison.CurrentCultureIgnoreCase) ||
                                x.Status.Equals(SmsTaskStatus.CouldRunNext.ToString(),
                                    StringComparison.CurrentCultureIgnoreCase)))
            {
                return await Task.FromResult(new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Success
                });
            }

            return await Task.FromResult(new TaskResult
            {
                ResultStatus = SmsTaskStatus.WaitForDependency,
                Message = $"[Order:{orderId},Task:{TaskName}]: wait for {dependencyTaskName}"
            });
        }

        

        

        protected void OnDisplayMessage(object sender, MessageEventArgs e)
        {
            OnDisplayMessage(e.Message);
        }

        private void OnDisplayMessage(string message)
        {
            DisplayMessage?.Invoke(this, new MessageEventArgs { Message = message });
        }
        

        

        ~SmsBaseTask()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free any other managed objects here.
                //

            }


            // free native resources here if there are any
            Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
