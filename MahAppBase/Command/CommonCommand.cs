using System;
using System.Windows.Input;

namespace MahAppBase.Command
{
    /// <summary>
    /// 有參數共用Command
    /// </summary>
    public class CommonCommand : ICommand
    {
        #region Declarations
        private readonly Action<object> _execute;
        public event EventHandler CanExecuteChanged;
        #endregion

        #region Memberfunction
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }

        public CommonCommand(Action<object> execute)
        {
            _execute = execute;
        }
        #endregion
    }
}