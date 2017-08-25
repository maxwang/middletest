using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmsTask.Framework.Models;
using SmsTask.Framework.Repository;

namespace SmsTask.Framework
{
    public abstract class SmsOrderBaseTaskAction : SmsBaseTaskAction
    {
        protected readonly Dictionary<string, string> ApiMethodToTaskAction;
        protected SmsOrderBaseTaskAction(ITaskRepository taskRepository) : base(taskRepository)
        {
            ApiMethodToTaskAction = new Dictionary<string, string>
            {
                { "BD PAN", "Order.New.Pan"},
                { "BD WEBGEN", "Order.New.Webgen"}
            };
        }
        
        protected override async Task<TaskResult> CreateInitTaskActionsIfNeededAsync(int taskId)
        {
            var task = await TaskRepository.GetTaskAsync(taskId);
            var orderId = task.OrderId;

            var lineItems = await TaskRepository.GetOrderLineItemsAsync(orderId);
            TaskResult result = new TaskResult
            {
                ResultStatus = SmsTaskStatus.Success,
                Message = string.Empty
            };

            foreach (var item in lineItems)
            {
                if (!string.IsNullOrEmpty(item.SKU))
                {
                    var products = await TaskRepository.GetProductByCodeAsync(item.SKU);
                    var product = products[0];

                    var key = product.ApiMethod.Trim().ToUpper();
                    //all validation was done at new order, do not need double handling it
                    if (ApiMethodToTaskAction.ContainsKey(key))
                    {
                        var taskName = ApiMethodToTaskAction[key];
                        if (taskName.Equals(TaskActionName))
                        {
                            result = await CreateOrderTaskActionAsync(orderId, taskId, item.OrderLineItemId);
                            if (result.ResultStatus != SmsTaskStatus.Success)
                            {
                                return result;
                            }

                        }
                    }
                    
                }

            }
            
            return result;
        }

        private async Task<TaskResult> CreateOrderTaskActionAsync(int orderId, int taskId, int orderLineItemId)
        {
            if (await TaskRepository.AnyTaskActionAsync(taskId, orderLineItemId, TaskActionName))
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Success,
                    Message =
                        $"Order {orderId} LineItem {taskId}:{orderLineItemId} Task created before, do not need create it this time"
                };
            }

            var taskActionId = await TaskRepository.InsertTaskActionAsync(new OrderTaskAction
            {
                OrderId = orderId,
                OrderTaskId = taskId,
                OrderLineItemId = orderLineItemId,
                ActionName = TaskActionName,
                InitData = orderLineItemId.ToString(),
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
                Message = $"Order LineItem {orderId}:{orderLineItemId} {TaskActionName} Created:{taskActionId}"
            };
        }

    }
}
