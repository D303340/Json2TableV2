using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Json2TableV2.ViewModel.Commands
{
    internal class OpenFileCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private Action _execute;
        public OpenFileCommand(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute.Invoke();
        }
    }
}
