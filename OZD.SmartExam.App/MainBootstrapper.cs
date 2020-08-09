using Caliburn.Micro;
using OZD.SmartExam.Library.ViewModels;
using OZD.SmartExam.UI.Views;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace OZD.SmartExam
{
    public class MainBootstrapper : BootstrapperBase
    {
        public MainBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<MainViewModel>();
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();
            assemblies.Add(typeof(MainView).GetTypeInfo().Assembly);
            assemblies.Add(typeof(MainViewModel).GetTypeInfo().Assembly);
            return assemblies;
        }
    }
}
