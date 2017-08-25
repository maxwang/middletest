using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SmsTask.Framework;
using SmsTask.Framework.Models;
using SmsTask.Framework.Repository;

namespace SmsTask.Framework
{
    /// <summary>
    /// Real Order Process, we try to put order to service provider
    /// </summary>
    /// <seealso cref="SmsTask.Framework.SmsBaseTask" />
    public class SmsOrderBaseTask : SmsBaseTask
    {
        private readonly Dictionary<string, string> _apiMethodToTaskAction;
        protected string ApiMethod;

        public SmsOrderBaseTask(ITaskRepository taskRepository, CancellationToken token) : base(taskRepository, token)
        {
            TaskName = "Order.New";
            _apiMethodToTaskAction = new Dictionary<string, string>
            {
                { "BD PAN", "Order.New.Pan"},
                { "BD WEBGEN", "Order.New.Webgen"}
            };
        }

        protected override async Task<TaskResult> TaskPrecheckAsync(int orderId)
        {
            var result = await base.TaskPrecheckAsync(orderId);

            if (result.ResultStatus != SmsTaskStatus.Success && result.ResultStatus != SmsTaskStatus.CouldRunNext)
            {
                return result;
            }

            var lineItems = await TaskRepository.GetOrderLineItemsAsync(orderId);

            if (lineItems == null || lineItems.Count < 1)
            {
                return new TaskResult
                {
                    ResultStatus = SmsTaskStatus.Error,
                    Message = "Could not find any order line item"
                };
            }

            bool error = false;

            var message = new StringBuilder();

            foreach (var item in lineItems)
            {
                if (string.IsNullOrEmpty(item.SKU))
                {
                    error = true;
                    message.AppendLine($"[Order:{orderId}][LineItem:{item.OrderLineItemId}] SKU is empty");
                }
                else
                {
                    var products = await TaskRepository.GetProductByCodeAsync(item.SKU);
                    if (products == null || products.Count == 0)
                    {
                        error = true;
                        message.AppendLine(
                            $"[Order:{orderId}][LineItem:{item.OrderLineItemId}] Could not find product for SKU:{item.SKU}");
                    }
                    else if (products.Count > 1)
                    {
                        error = true;
                        message.AppendLine(
                            $"[Order:{orderId}][LineItem:{item.OrderLineItemId}] More than one products were found for SKU:{item.SKU}");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(products[0].ApiMethod))
                        {
                            error = true;
                            message.AppendLine(
                                $"[Order:{orderId}][LineItem:{item.OrderLineItemId}] Could not find product API method for SKU:{item.SKU}");
                        }
                        else if (!_apiMethodToTaskAction.ContainsKey(products[0].ApiMethod.Trim().ToUpper()))
                        {
                            error = true;
                            message.AppendLine(
                                $"[Order:{orderId}][LineItem:{item.OrderLineItemId}] Not supported API method {products[0].ApiMethod} for SKU:{item.SKU}");
                        }
                    }

                }
            }

            return new TaskResult
            {
                ResultStatus = error ? SmsTaskStatus.Error : SmsTaskStatus.Success,
                Message = message.ToString()
            };
        }

        //protected override async Task<TaskResult> CreateInitTasksIfNeededAsync(int orderId)
        //{
        //    var lineItems = await TaskRepository.GetOrderLineItemsAsync(orderId);
        //    TaskResult result = new TaskResult
        //    {
        //        ResultStatus = SmsTaskStatus.Success,
        //        Message = string.Empty
        //    };

        //    foreach (var item in lineItems)
        //    {
        //        if (!string.IsNullOrEmpty(item.SKU))
        //        {
        //            var products = await TaskRepository.GetProductByCodeAsync(item.SKU);
        //            var product = products[0];

        //            if (product.ApiMethod.Trim().ToUpper().Equals(ApiMethod))
        //            {
        //                result = await CreateOrderTaskAsync(orderId, item.OrderLineItemId);
        //                if (result.ResultStatus != SmsTaskStatus.Success)
        //                {
        //                    return result;
        //                }
        //            }

        //        }
        //    }

        //    return result;


        //}

       
    }
}
