using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OZD.SmartExam.LicenseMaker
{
    public class LicenseMakerBootstrapper : BootstrapperBase
    {
        public LicenseMakerBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<LicenseMakerViewModel>();
        }
    }
}
