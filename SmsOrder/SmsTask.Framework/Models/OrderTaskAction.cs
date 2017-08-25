using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_OrderTaskActions")]
    public class OrderTaskAction
    {
        [Key]
        public int OrderTaskActionId { get; set; }

        public int OrderTaskId { get; set; }
        public int OrderId { get; set; }
        public int OrderLineItemId { get; set; }
        public string ActionName { get; set; }
        public string InitData { get; set; }
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
        public string Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedTime { get; set; }
        public string LastUpdatedBy { get; set; }
        public string ActionPath { get; set; }
    }
}
