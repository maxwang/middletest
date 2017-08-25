using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmsTask.Framework.Models;

namespace SmsTask.Framework.Repository
{
    public interface ITaskRepository
    {
        IEnumerable<PortalOrder> PortalOrders { get; }
        IEnumerable<PortalCustomer> PortalCustomers { get; }
        IEnumerable<PortalOrderCustomer> PortalOrderCustomers { get; }

        IEnumerable<PortalOrderLineItem> PortalOrderLineItem { get; }
        Task<IList<PortalOrderLineItem>> GetOrderLineItemsAsync(int orderId);
        Task<PortalOrderLineItem> GetOrderLineItemAsync(int orderLineItemId);

        IEnumerable<OrderTask> OrderTasks { get; }
        IEnumerable<OrderConfiguration> OrderConfigurations { get; }
        IEnumerable<ZohoProduct> ZohoProducts { get; }
        /// <summary>
        /// Gets the product by code asynchronous.
        /// Normally we shold get only one product,but the data from external source, hard to control, the caller should check it
        /// </summary>
        /// <param name="productCode">The product code.</param>
        /// <returns></returns>
        Task<List<ZohoProduct>> GetProductByCodeAsync(string productCode);

        IEnumerable<ZohoAccount> ZohoAccounts { get; }
        IEnumerable<BdConfiguration> BdConfigurations { get; }

        
        Task<OrderTask> GetTaskAsync(int taskId);
        Task<int> InsertOrUpdateTaskAsync(OrderTask orderTask);
        Task<int> InsertOrderTaskAsync(OrderTask orderTask);
        Task<int> InsertTaskActionAsync(OrderTaskAction orderTaskAction);
        Task<int> InsertOrderActionLogAsync(OrderActionLog ordderActionLog);

        Task<int> InsertOrderBdLineItemKeys(List<PortalBdLineItemKey> keys);
        Task<int> InsertBdLineItemKeyHistory(List<BdLicenseKeyHistory> keys);
        

        Task<bool> AnyOrderActionLogAsync(int order);
        Task<PortalCustomer> GetOrderCustomerAsync(int orderId);
        Task<List<OrderTaskAction>> GetTaskActionsAsync(int taskId, string actionName);
        Task<OrderTaskAction> GetTaskActionAsync(int taskActionId);
        Task<List<OrderTask>> GetOrderTasksAsync(int orderId, string taskName);
        Task<bool> AnyZohoAccontAsync(string accountId);
        Task<ZohoAccount> GetZohoAccountAsync(string accountId);
        Task<bool> AnyOrderTaskAsync(int orderId, string taskName);
        Task<bool> AnyTaskActionAsync(int taskId, string taskActionName);
        Task<bool> AnyTaskActionAsync(int taskId, int orderLineItemId, string taskActionName);
        Task<bool> AnyFollowingTaskActionAsync(int taskId, string actionPath);
        Task<bool> UpdateTaskStatusAsync(int taskId, string taskStatus, string updatedBy);
        Task<bool> UpdateTaskActionStatusAsync(int taskActionId, string taskStatus, string requstData,
            string responseData, string updatedBy);
        Task<bool> UpdateOrderStatusAsync(int orderId, string orderStatus, string updatedBy);
        
        Task<PortalOrder> GetNextOrderAsync();
        Task<PortalOrder> GetNextOrderAsync(string orderType);
    }
}
