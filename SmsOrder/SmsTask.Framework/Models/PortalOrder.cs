using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_Orders")]
    public class PortalOrder
    {
        [Key]
        public int OrderId { get; set; }

        public string ResellerId { get; set; }
        public string CustomerId { get; set; }
        public string PotentialId { get; set; }

        public string Status { get; set; }
        public string OrderType { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
