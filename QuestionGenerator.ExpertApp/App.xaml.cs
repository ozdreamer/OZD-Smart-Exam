namespace QuestionGenerator.ExpertApp
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;

    using Autofac;
    using Autofac.Builder;

    using DevExpress.LookAndFeel;
    using DevExpress.Skins;
    using QuestionGenerator.ExpertUI.Views;

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

            SkinManager.EnableFormSkins();
            UserLookAndFeel.Default.SetStyle(LookAndFeelStyle.Skin, false, false);

            var container = this.BuildIoC();
            container.Resolve<MainView>().Show();
        }

        /// <summary>
        /// Builds the Autofac Container.
        /// </summary>
        private IContainer BuildIoC()
        {
            var builder = new ContainerBuilder();

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var directory = new DirectoryInfo(assemblyPath);
            var files = directory.GetFiles("*QuestionGenerator*.dll", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                var assembly = AppDomain.CurrentDomain.Load(assemblyName);
                builder.RegisterAssemblyModules(assembly);
            }

            return builder.Build(ContainerBuildOptions.None);
        }
    }
}