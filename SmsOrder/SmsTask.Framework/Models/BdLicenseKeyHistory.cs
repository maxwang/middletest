using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_BDLicenseKeyHistory")]
    public class BdLicenseKeyHistory
    {
        [Key]
        public int BdLicenseKeyHistoryId { get; set; }

        public string Sku { get; set; }
        public string BdLicenseKey { get; set; }
        public int Qty { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
