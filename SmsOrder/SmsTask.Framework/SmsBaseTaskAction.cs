using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsTask.Framework.Models;
using SmsTask.Framework.Repository;

namespace SmsTask.Framework
{


    public abstract class SmsBaseTaskAction 
    {
        public event EventHandler<MessageEventArgs> DisplayMessage;

        protected readonly ITaskRepository TaskRepository;
        protected abstract Task<TaskResult> ProcessTaskActionAsync(int actionId);
        public string TaskActionName { get; set; }
        protected List<string> FollowingActionNames;
        protected List<string> DependencyTaskActions;

        protected readonly string LogUserName;

        protected SmsBaseTaskAction(ITaskRepository taskRepository)
        {
            TaskRepository = taskRepository;
            LogUserName = "OrderTask";
            FollowingActionNames = new List<string>();
            DependencyTaskActions = new List<string>();
        }

        #region Order Task Action three main functions

        protected virtual async Task<TaskResult> TaskActionPrecheckAsync(int taskid)
        {
            var result = await CheckDependenciesAsync(taskid);

            if (result.ResultStatus == SmsTaskStatus.WaitForDependency)
            {
                return result;
            }
            
            return await CreateInitTaskActionsIfNeededAsync(taskid);
        }
        

        protected virtual async Task<TaskResult> TaskActionPostcheckAsync(int taskid)
        {

            return await Task.FromResult(new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success
            });
        }

        public async Task<TaskResult> ProcessAsync(int taskId)
        {
            
            var task = await TaskRepository.GetTaskAsync(taskId);

            if (task == null)
            {
                OnDisplayMessage($"[Task:{taskId}]Could not find Task information");
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Error,
                    Message = $"[Task:{taskId}]Could not find Task information"
                };
            }

            OnDisplayMessage($"[{task.OrderId}.{task.OrderTaskId}]start processing {TaskActionName}");

            TaskResult result = await TaskActionPrecheckAsync(taskId);

            if (result.ResultStatus != SmsTaskStatus.WaitForDependency)
            {

                //check dependency only need save it once, otherwise too many check dependency logs
                await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                {
                    OrderId = task.OrderId,
                    OrderTaskId = taskId,
                    Comment = $"{TaskActionName} Precheck result: {result.ResultStatus.ToString()}",
                    Details = result.Message,
                    CreatedTime = DateTime.Now,
                    CreatedBy = LogUserName
                });

            }

            if (result.ResultStatus != SmsTaskStatus.Success && result.ResultStatus != SmsTaskStatus.CouldRunNext)
            {
                return result;
            }

            var actions = await TaskRepository.GetTaskActionsAsync(taskId, TaskActionName);

            //
            OnDisplayMessage($"[Order:{task.OrderId}, Task:{taskId}]{TaskActionName} :start processing");
            StringBuilder message = new StringBuilder();

            var lastActionStatus = SmsTaskStatus.Success;

            foreach (var action in actions)
            {
                if (await PreviouseRunAsync(taskId))
                {
                    continue;
                }

                TaskResult actionResult = await ProcessTaskActionAsync(action.OrderTaskActionId);

                await TaskRepository.UpdateTaskActionStatusAsync(action.OrderTaskActionId, actionResult.ResultStatus.ToString(),
                    actionResult.RequestData, actionResult.ResponseData, LogUserName);

                await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                {
                    OrderId = task.OrderId,
                    OrderTaskId = taskId,
                    OrderTaskActionId = action.OrderTaskActionId,
                    Comment = JsonConvert.SerializeObject(actionResult),
                    Details = result.Message,
                    CreatedTime = DateTime.Now,
                    CreatedBy = LogUserName
                });


                lastActionStatus = actionResult.ResultStatus;

                var addNextActions = await CreateFollowingTaskActionsIfNeededAsync(action);

                await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                {
                    OrderId = task.OrderId,
                    OrderTaskId = taskId,
                    OrderTaskActionId = action.OrderTaskActionId,
                    Comment = $"Create following tasks:{addNextActions.ResultStatus.ToString()}",
                    Details = addNextActions.Message,
                    CreatedTime = DateTime.Now,
                    CreatedBy = LogUserName
                });

                if (lastActionStatus != SmsTaskStatus.Success && lastActionStatus != SmsTaskStatus.CouldRunNext)
                {
                    message.AppendLine(actionResult.Message);
                    break;
                }
            }

            if (lastActionStatus == SmsTaskStatus.Success || lastActionStatus == SmsTaskStatus.CouldRunNext)
            {
                result = await TaskActionPostcheckAsync(taskId);
                lastActionStatus = result.ResultStatus;
                message.AppendLine(result.Message);

                await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                {
                    OrderId = task.OrderId,
                    OrderTaskId = taskId,
                    Comment = $"{TaskActionName} Postcheck result: {result.ResultStatus.ToString()}",
                    Details = result.Message,
                    CreatedTime = DateTime.Now,
                    CreatedBy = LogUserName
                });

            }

            result.ResultStatus = lastActionStatus;
            result.Message = message.ToString();
            return result;
        }

        private async Task<bool> PreviouseRunAsync(int taskId)
        {
            
            var taskAction = await TaskRepository.GetTaskActionAsync(taskId);

            //if error, we still retry, sometimes we try to resume
            if (taskAction != null)
            {
                if (taskAction.Status.Equals(SmsTaskStatus.Success.ToString(),
                    StringComparison.CurrentCultureIgnoreCase) || taskAction.Status.Equals(SmsTaskStatus.CouldRunNext.ToString(),
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;


        }

        #endregion

        protected virtual async Task<TaskResult> CreateFollowingTaskActionsIfNeededAsync(OrderTaskAction action)
        {
            if (FollowingActionNames == null || FollowingActionNames.Count < 1)
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Success,
                    Message = "No Following Task"
                };
            }

            var followingActionPath = string.IsNullOrEmpty(action.ActionPath)
                ? action.OrderTaskActionId.ToString()
                : $"{action.ActionPath}-{action.OrderTaskActionId}";
            if (await TaskRepository.AnyFollowingTaskActionAsync(action.OrderTaskId, followingActionPath))
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Success,
                    Message = "Task Actions created before, do not need create it this time"
                };
            }

            var messsage = new StringBuilder();

            foreach (var actionName in FollowingActionNames)
            {
                var actionId = await TaskRepository.InsertTaskActionAsync(new OrderTaskAction
                {
                    OrderId = action.OrderId,
                    OrderTaskId = action.OrderTaskId,
                    ActionName = actionName,
                    InitData = action.OrderTaskActionId.ToString(),
                    Status = SmsTaskStatus.New.ToString(),
                    CreatedBy = LogUserName,
                    CreatedTime = DateTime.Now,
                    LastUpdatedBy = LogUserName,
                    LastUpdatedTime = DateTime.Now,
                    ActionPath = followingActionPath
                });

                messsage.AppendLine($"Task Action:{action.OrderTaskActionId} fowwing task action :{actionName} created qirh id{actionId}");
            }

            return new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success,
                Message = messsage.ToString()
            };
        }

        protected virtual async Task<TaskResult> CreateInitTaskActionsIfNeededAsync(int taskid)
        {
            if (await TaskRepository.AnyTaskActionAsync(taskid, TaskActionName))
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Success,
                    Message = "Task created before, do not need create it this time"
                };
            }

            var task = await TaskRepository.GetTaskAsync(taskid);

            if (task == null)
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Error,
                    Message = $"[Task:{taskid}]Could not find Task information"
                };
            }

            var taskId = await TaskRepository.InsertTaskActionAsync(new OrderTaskAction
            {
                OrderId = task.OrderId,
                OrderTaskId = taskid,
                ActionName = TaskActionName,
                InitData = task.OrderId.ToString(),
                Status = SmsTaskStatus.New.ToString(),
                CreatedBy = LogUserName,
                CreatedTime = DateTime.Now,
                LastUpdatedBy = LogUserName,
                LastUpdatedTime = DateTime.Now,
                ActionPath = string.Empty
            });

            return new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success,
                Message = $"Task Created:{taskId}"
            };
        }

        protected virtual async Task<TaskResult> CheckDependenciesAsync(int taskid)
        {
            TaskResult taskResult = new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success
            };

            if (DependencyTaskActions == null || DependencyTaskActions.Count == 0)
            {
                return taskResult;
            }

            foreach (var dependency in DependencyTaskActions)
            {
                taskResult = await CheckDependencyAsync(taskid, dependency);
                if (taskResult.ResultStatus == SmsTaskStatus.WaitForDependency)
                {
                    break;
                }
            }

            return taskResult;
        }

        protected void OnDisplayMessage(string message)
        {
            DisplayMessage?.Invoke(this, new MessageEventArgs { Message = message });
        }

        protected virtual async Task<TaskResult> CheckDependencyAsync(int taskId, string dependencyTaskActionName)
        {
            var tasks = await TaskRepository.GetTaskActionsAsync(taskId, dependencyTaskActionName);
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
                Message = $"{taskId}:{TaskActionName}]: wait for {dependencyTaskActionName}"
            });
        }

    }
}
