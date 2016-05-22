using System;
using System.Windows.Input;

namespace VisualRust.Project.Controls
{
    public class RelayCommand : ICommand
    {
        Action action;

        public RelayCommand(Action action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action();
        }
    }
}