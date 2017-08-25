using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmsTask.Framework.Models;

namespace SmsTask.Framework
{
    public interface ITaskAction
    {
        string TaskActionName { get; set; }
        Task<TaskResult> ProcessTaskAction(int taskId);
    }
}
