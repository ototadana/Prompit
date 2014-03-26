using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace XPFriend.Prompit
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                HandleException(e.Exception);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    GetMessage(e.Exception),
                    "",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                App.Current.Shutdown();
            }
            finally
            {
                e.Handled = true;
            }
        }

        internal void HandleException(Exception exception)
        {
            string errorLogFile = GetErrorLogFile();
            WriteErrorLog(exception, errorLogFile);
            MainWindow window = App.Current.MainWindow as MainWindow;
            window.ShowMessage(GetMessage(exception), errorLogFile);
        }

        private string GetErrorLogFile()
        {
            string errorLogFolder = Path.Combine(GetApplicationPath(), "Logs");
            if (!Directory.Exists(errorLogFolder))
            {
                Directory.CreateDirectory(errorLogFolder);
            }
            return Path.Combine(
                errorLogFolder,
                "error" + DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss") + ".txt");
        }

        private static string GetApplicationPath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string applicationPath = Path.Combine(appData, "XPFriend\\Prompit");
            if (!File.Exists(applicationPath))
            {
                Directory.CreateDirectory(applicationPath);
            }
            return applicationPath;
        }

        private static string GetMessage(Exception e)
        {
            Stack<string> stack = new Stack<string>();
            stack.Push(XPFriend.Prompit.Properties.Resources.ExceptionOccurred + "\r\n" + e.Message);

            Exception innerException = null;
            while ((innerException = e.InnerException) != null && innerException != e)
            {
                stack.Push(innerException.Message);
                e = innerException;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(stack.Pop());
            while (stack.Count > 0)
            {
                sb.Append(Environment.NewLine).Append("  <-- ");
                sb.Append(stack.Pop());
            }
            return sb.ToString();
        }

        private static void WriteErrorLog(Exception e, string errorLogFile)
        {
            try
            {
                File.WriteAllText(errorLogFile,
                    errorLogFile + Environment.NewLine +
                    "---" + Environment.NewLine +
                    e.ToString());
            }
            catch (Exception)
            {
            }
        }
    }
}
