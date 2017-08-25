using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_OrderCustomer")]
    public class PortalOrderCustomer
    {
        [Key, Column(Order = 0)]
        public int OrderId { get; set; }

        [Key, Column(Order = 1)]
        public int CustomerId { get; set; }

        [ForeignKey("OrderId")]
        public PortalOrder Order { get; set; }

        [ForeignKey("CustomerId")]
        public PortalCustomer Customer { get; set; }
    }
}
