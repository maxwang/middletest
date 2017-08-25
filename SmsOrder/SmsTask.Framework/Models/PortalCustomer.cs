using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    [Table("portal_Customers")]
    public class PortalCustomer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

       

        //keep the same as Zoho defination
        [MaxLength(200)]
        public string CustomerName { get; set; }

        [MaxLength(250)]
        public string BillingStreet { get; set; }

        [MaxLength(30)]
        public string BillingCity { get; set; }

        [MaxLength(30)]
        public string BillingState { get; set; }

        [MaxLength(30)]
        public string BillingCode { get; set; }

        [MaxLength(30)]
        public string BillingCountry { get; set; }

        [MaxLength(48)]
        public string Industry { get; set; }

        [MaxLength(48)]
        public string PositionClassification { get; set; }


        [MaxLength(255)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(40)]
        public string ContactFirstName { get; set; }

        [MaxLength(80)]
        public string ContactLastName { get; set; }

        [MaxLength(80)]
        public string Phone { get; set; }

        [MaxLength(30)]
        public string Mobile { get; set; }

        [MaxLength(250)]
        public string Email { get; set; }
    }
}
