using SmsTask.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmsTask.Framework.Repository;

namespace SmsTask.Framework
{
    public abstract class SmsBaseOrderManager : IDisposable
    {

        public event EventHandler<MessageEventArgs> DisplayMessage;

        protected CancellationToken CancellationToken;
        protected readonly ITaskRepository TaskRepository;
        protected SmsBaseTask[] Tasks;
        protected readonly string LogUserName;
        protected readonly IEmailSender EmailSender;
        protected readonly string OrderType;
        private bool _disposed = false;

        protected void OnDisplayMessage(string message)
        {
            DisplayMessage?.Invoke(this, new MessageEventArgs {Message = $"[{OrderType}]{message}"});
        }

        protected SmsBaseOrderManager(ITaskRepository taskRepository, IEmailSender emailSender, CancellationToken token, string orderType = "")
        {
            LogUserName = "Order.System";
            CancellationToken = token;
            TaskRepository = taskRepository;
            EmailSender = emailSender;
            OrderType = orderType;
        }

        public virtual async Task StartProcessingOrderTasksAsync()
        {
            //await ResumOrderTasksAsync();

            await StartOrderTasksAsync();

        }

        protected virtual async Task<PortalOrder> GetNextOrderAsync()
        {
            return await TaskRepository.GetNextOrderAsync(OrderType);
        }

        protected virtual async Task StartOrderTasksAsync()
        {
            PortalOrder order = await GetNextOrderAsync();
            StringBuilder message = new StringBuilder();
            string taskName = string.Empty;
            while (order != null)
            {
                var lastTaskStatus = SmsTaskStatus.Success;

                message.Remove(0, message.Length);
                OnDisplayMessage($"[Order:{order.OrderId}]:start processing");
                try
                {
                    await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                    {
                        OrderId = order.OrderId,
                        Comment = "Start Processing Order",
                        CreatedTime = DateTime.Now,
                        CreatedBy = LogUserName
                    });

                    foreach (SmsBaseTask task in Tasks)
                    {
                        if (!CancellationToken.IsCancellationRequested)
                        {
                            taskName = task.TaskName;

                            await TaskRepository.UpdateOrderStatusAsync(order.OrderId,
                                $"SmsTaskStatus.Processing:{task.TaskName}",
                                LogUserName);

                            TaskResult result = await task.ProcessTaskActionsAsync(order.OrderId);
                            lastTaskStatus = result.ResultStatus;

                            if (result.ResultStatus != SmsTaskStatus.Success &&
                                result.ResultStatus != SmsTaskStatus.CouldRunNext)
                            {
                                message.AppendLine(JsonConvert.SerializeObject(result));
                                break;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    lastTaskStatus = SmsTaskStatus.Error;
                    message.Append($"{ex.Message} : \r\n {ex.StackTrace}");
                    if (ex.InnerException != null)
                        message.Append($"\r\n{ex.InnerException.Message}\r\n {ex.InnerException.StackTrace}");
                }

                OnDisplayMessage($"[Order:{order.OrderId}]:{lastTaskStatus.ToString()}");

                if (lastTaskStatus != SmsTaskStatus.WaitForDependency)
                {
                    await TaskRepository.InsertOrderActionLogAsync(new OrderActionLog
                    {
                        OrderId = order.OrderId,
                        Comment = $"Finished Processing Order, result:{lastTaskStatus}",
                        Details = message.ToString(),
                        CreatedTime = DateTime.Now,
                        CreatedBy = LogUserName
                    });
                }

                if (lastTaskStatus == SmsTaskStatus.Error)
                {
                    await EmailSender.SendEmailAsync(
                        $"[Order id: {order.OrderId}] Task Error: {taskName}",
                        message.ToString());
                }

                //update order status
                await TaskRepository.UpdateOrderStatusAsync(order.OrderId,
                    (lastTaskStatus == SmsTaskStatus.Error
                        ? $"{lastTaskStatus}:{taskName}"
                        : lastTaskStatus.ToString()), LogUserName);

                if (CancellationToken.IsCancellationRequested)
                {
                    break;
                }
                else
                {
                    order = await GetNextOrderAsync();
                }
            }
        }

        #region Old resume order task

        //protected virtual async Task ResumOrderTasksAsync()
        //{
        //    PortalOrder order = await TaskRepository.GetNextResumeOrderAysnc();
        //    SmsTaskStatus lastTaskStatus = SmsTaskStatus.Success;
        //    StringBuilder message = new StringBuilder();
        //    string taskName = string.Empty;
        //    while (order != null)
        //    {
        //        lastTaskStatus = SmsTaskStatus.Success;

        //        message.Remove(0, message.Length);

        //        foreach (var task in Tasks)
        //        {

        //            if (!CancellationToken.IsCancellationRequested)
        //            {
        //                taskName = task.TaskName;

        //                await TaskRepository.UpdateOrderStatusAsync(order.OrderId,
        //                    $"SmsTaskStatus.Processing.ToString():{task.TaskName}",
        //                    LogUserName);

        //                var result = await task.ProcessTaskActionsAsync(order.OrderId);

        //                if (result.ResultStatus != SmsTaskStatus.Success &&
        //                    result.ResultStatus != SmsTaskStatus.CouldRunNext &&
        //                    result.ResultStatus != SmsTaskStatus.OnHold)
        //                {
        //                    lastTaskStatus = result.ResultStatus;
        //                    message.Append(result.Message);
        //                    break;
        //                }
        //            }
        //        }

        //        //update order status

        //        OnDisplayMessage($"[Order:{order.OrderId}]:{lastTaskStatus.ToString()}");

        //        if (lastTaskStatus == SmsTaskStatus.Error)
        //        {
        //            await EmailSender.SendEmailAsync(
        //                $"[Order id: {order.OrderId}] Task Error: {taskName}",
        //                message.ToString());
        //        }

        //        //update order status
        //        await TaskRepository.UpdateOrderStatusAsync(order.OrderId,
        //            (lastTaskStatus == SmsTaskStatus.Error
        //                ? $"{taskName}:{lastTaskStatus.ToString()}"
        //                : lastTaskStatus.ToString()), LogUserName);


        //        if (CancellationToken.IsCancellationRequested)
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            order = await TaskRepository.GetNextResumeOrderAysnc();
        //        }

        //    }

        //}
        #endregion

        ~SmsBaseOrderManager()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free any other managed objects here.
                //

            }


            // free native resources here if there are any
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
