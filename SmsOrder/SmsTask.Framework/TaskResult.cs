using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SmsTask.Framework.Models;

namespace SmsTask.Framework
{
    public class TaskResult
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public SmsTaskStatus ResultStatus { get; set; }

        public string Message { get; set; }

        public string RequestData { get; set; }
        public string ResponseData { get; set; }
    }
}
