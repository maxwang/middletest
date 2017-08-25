using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Models
{
    public enum SmsTaskStatus
    {
        New = 1,
        Processing,
        Success,
        CouldRunNext,
        Error,
        TaskCancelled,
        OnHold,
        WaitForDependency
    }
   
}
