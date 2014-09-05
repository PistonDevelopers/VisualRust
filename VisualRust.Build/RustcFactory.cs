using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Build
{
    public class RustcFactory : ITaskFactory
    {
        private TaskPropertyInfo[] parameters;

        public void CleanupTask(ITask task)
        {
        }

        public ITask CreateTask(IBuildEngine taskFactoryLoggingHost)
        {
            return new Rustc();
        }

        public string FactoryName
        {
            get { return "VisualRust.Build.RustcFactory"; }
        }

        public TaskPropertyInfo[] GetTaskParameters()
        {
            parameters = parameters ?? typeof(Rustc)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select((prop) => 
                    new TaskPropertyInfo(
                        prop.Name,
                        prop.PropertyType,
                        prop.GetCustomAttributes().OfType<OutputAttribute>().Any(),
                        prop.GetCustomAttributes().OfType<RequiredAttribute>().Any()))
                .ToArray();
            return parameters;
        }

        public bool Initialize(string taskName, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost)
        {
            return true;
        }

        public Type TaskType
        {
            get { return typeof(Rustc); }
        }
    }
}
