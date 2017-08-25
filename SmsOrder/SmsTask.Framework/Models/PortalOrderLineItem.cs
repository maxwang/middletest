using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using SmsTask.Framework.Models;

namespace SmsTask.Framework.Models
{
    [Table("portal_OrderLineItems")]
    public class PortalOrderLineItem
    {

        [Key]
        public int OrderLineItemId { get; set; }
        
        public int OrderId { get; set; }
        
        [ForeignKey("OrderId")]
        public PortalOrder Order { get; set; }

        public int RowNumber { get; set; }

        [MaxLength(255)]
        public string SKU { get; set; }

        public int Quantity { get; set; }

        public decimal MsrpPerUnit { get; set; }

        public decimal MsrpTotal { get; set; }

        public decimal PartnerPerUnit { get; set; }
        public decimal PartnerTotal { get; set; }

        [MaxLength(255)]
        public string Status { get; set; }

        [MaxLength(255)]
        public string CreatedBy { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [MaxLength(255)]
        public string ModifiedBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
