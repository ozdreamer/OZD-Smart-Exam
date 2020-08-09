using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OZD.SmartExam.Library.ViewModels
{
    /// <summary>
    /// The viewmodel for LicenseRequestView.
    /// </summary>
    public class LicenseRequestViewModel : PropertyChangedBase
    {
        private string machineId;

        public string MachineId
        {
            get 
            {
                return this.machineId;
            }

            set
            { 
                if (this.machineId != value)
                {
                    this.machineId = value;
                    this.NotifyOfPropertyChange(() => this.MachineId);
                }
            }
        }
    }
}
