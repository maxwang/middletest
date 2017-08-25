using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_OrderBDLineItemKeys")]
    public class PortalBdLineItemKey
    {
        [Key]
        public int OrderLineItemKeyId { get; set; }
        public int OrderId { get; set; }
        public int OrderLineItemId { get; set; }

        [Column("BDProductId")]
        public int BdProductId { get; set; }
        
        [Column("BDProductName")]
        public string BdProductName { get; set; }

        [Column("BDProductKey")]
        public string BdProductKey { get; set; }
    }
}
