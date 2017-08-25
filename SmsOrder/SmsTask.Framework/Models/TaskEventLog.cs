using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{

    [Table("task_EventLog")]
    public class TaskEventLog
    {
        [Key]
        public int Id { get; set; }

        public int TaskId { get; set; }
        public int TaskActionId { get; set; }

        [Column(TypeName = "text")]
        public string RequestData { get; set; }

        [Column(TypeName = "text")]
        public string ResponseData { get; set; }
        
        [Column(TypeName = "text")]
        public string Comment { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; }
    }
}
