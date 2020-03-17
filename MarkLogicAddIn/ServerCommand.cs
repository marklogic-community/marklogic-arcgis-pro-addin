using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MarkLogic.Esri.ArcGISPro.AddIn
{
    public class ServerCommand : ICommand
    {
        public ServerCommand(Func<object, Task> execute, Func<object, bool> canExecute = null, Action<Exception> error = null)
        {
            ExecuteCallback = execute ?? throw new ArgumentNullException("execute");
            CanExecuteCallback = canExecute;
            ErrorCallback = error;
        }

        protected ServerCommand()
        {
        }

        protected Func<object, Task> ExecuteCallback { get; set; }

        protected Func<object, bool> CanExecuteCallback { get; set; }

        protected Action<Exception> ErrorCallback { get; set; }

        public virtual event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteCallback?.Invoke(parameter) ?? true;
        }

        public virtual async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            var retry = true;
            while (retry)
            {
                try
                {
                    await ExecuteCallback(parameter);
                    retry = false;
                }
                catch (AuthorizationRequiredException e)
                {
                    retry = (bool)Dispatcher.CurrentDispatcher.Invoke(new ResolveCredentialsDelegate(ResolveCredentials), e);
                    if (!retry)
                        ErrorCallback?.Invoke(e);
                }
                catch (Exception e)
                {
                    e.HandleAsUserNotification();
                    retry = false;
                    ErrorCallback?.Invoke(e);
                }
            }
        }

        public delegate bool ResolveCredentialsDelegate(AuthorizationRequiredException e);

        private bool ResolveCredentials(AuthorizationRequiredException e)
        {
            CredentialsWindow dlg = new CredentialsWindow
            {
                ConnectionProfile = e.ConnectionProfile,
                Owner = Application.Current.MainWindow
            };
            var resolved = dlg.ShowDialog().GetValueOrDefault(false);
            if (resolved)
                ConnectionService.Instance.SetCredentials(e.ConnectionProfile, dlg.Username, dlg.Password, e.Scheme, e.Domain);
            return resolved;
        }
    }
}
