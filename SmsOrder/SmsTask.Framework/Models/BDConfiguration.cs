using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmsTask.Framework.Models
{
    public class BdConfiguration
    {
        public int Id { get; set; }
        public string UserUUId { get; set; }
        public string AccountUUId { get; set; }
        public string LicenseType { get; set; }
        public string Duration { get; set; }
        public string ConsoleType { get; set; }
        public int PhysicalWorkStations { get; set; }
        public int VirtualWorkStations { get; set; }
        public int PhysicalServers { get; set; }
        public int VirtualServers { get; set; }
        public int MicrosoftExchange { get; set; }
        public int MobileDevices { get; set; }
        public string SelectionType { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string LastModifiedDate { get; set; }
        public string OrderUUId { get; set; }
        public string ShortDescription { get; set; }

    }
    
}
