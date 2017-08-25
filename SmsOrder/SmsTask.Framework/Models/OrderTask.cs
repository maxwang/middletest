using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_OrderTasks")]
    public class OrderTask
    {
        
            [Key]
            public int OrderTaskId { get; set; }

            public int OrderId { get; set; }
            

            public string TaskName { get; set; }

            public string Status { get; set; }

            public string CreatedBy { get; set; }
            public DateTime CreatedTime { get; set; }

            public string LastUpdatedBy { get; set; }
            public DateTime LastUpdatedTime { get; set; }

        
    }
}
