using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_OrderConfigurations")]
    public class OrderConfiguration
    {
        [Key, Column(Order = 0)]
        public int OrderId { get; set; }
        [Key, Column(Order = 1)]
        public string ConfigurationTable { get; set; }
        [Key, Column(Order = 2)]
        public int ConfigurationId { get; set; }
    }
}
