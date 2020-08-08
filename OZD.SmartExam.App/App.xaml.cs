namespace OZD.SmartExam
{
    using DevExpress.LookAndFeel;
    using DevExpress.Skins;
    using DevExpress.Xpf.Core;
    using OZD.SmartExam.UI.Views;
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs" /> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SetupExceptionHandling();

            SkinManager.EnableFormSkins();
            UserLookAndFeel.Default.SetStyle(LookAndFeelStyle.Skin, false, false);

            new MainView().Show();
        }
        /// <summary>
        /// Setupt the unhandled exceptions.
        /// </summary>
        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        /// <summary>
        /// Log the unhandled exception.
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <param name="source">Source where the exception was generated from</param>
        private void LogUnhandledException(Exception exception, string source)
        {
            try
            {
                AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
                var message = string.Format($"Unhandled exception in {assemblyName.Name} v{assemblyName.Version} - {exception.Message}");
                DXMessageBox.Show(message, "Unhandled Exception");
            }
            catch (Exception ex)
            {
                //DXMessageBox.Show(ex.Message, "Unhandled Exception");
            }
        }
    }
}