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
            StartAction action;
            return Enum.TryParse(p, out action) ? action : (StartAction?)null;
        }

        private static string StartActionQToString(StartAction? action)
        {
            return action.HasValue ? action.Value.ToString() : null;
        }
    }
}
