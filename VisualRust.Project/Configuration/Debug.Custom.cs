using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Project.Configuration
{
    public enum StartAction
    {
        Project,
        Program
    }

    partial class Debug
    {
        public StartAction StartAction
        {
            get { return StartActionQ ?? StartAction.Project; }
            set { StartActionQ = value; }
        }

        private static StartAction? StartActionQFromString(string p)
        {
            try {
                return (StartAction)Enum.Parse(typeof(StartAction), p);
            } catch (ArgumentException) {
                return null;
            }
        }

        private string StartActionQToString(StartAction? action)
        {
            if (!action.HasValue)
                return null;
            return action.Value.ToString();
        }
    }
}
