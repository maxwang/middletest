using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_OrderActionLogs")]
    public class OrderActionLog
    {
        [Key]
        public int ActionLogId { get; set; }

        public int OrderId { get; set; }
        public int OrderTaskId { get; set; }
        public int OrderTaskActionId { get; set; }
        public string Comment { get; set; }
        public string Details { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }

    }
}
