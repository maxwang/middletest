using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsTask.Framework.Repository
{
    public interface IEmailSender
    {
        Task<bool> SendEmailAsync(string subject, string message);
    }
}
