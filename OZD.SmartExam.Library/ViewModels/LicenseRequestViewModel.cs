using System.Windows;
using Caliburn.Micro;

namespace OZD.SmartExam.Library.ViewModels
{
    /// <summary>
    /// The viewmodel for LicenseRequestView.
    /// </summary>
    public class LicenseRequestViewModel : Screen
    {
        /// <summary>
        /// The constructore.
        /// </summary>
        public LicenseRequestViewModel()
        {
            this.CopyMachineIdCommand = new DelegateCommand(this.CopyMachineIdCommandExecute);
        }

        /// <summary>
        /// The machine identifier.
        /// </summary>
        private string machineId;

        /// <summary>
        /// Wrapper for machineId
        /// </summary>
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

        /// <summary>
        /// Copy the machine ID to clipboard.
        /// </summary>
        public DelegateCommand CopyMachineIdCommand { get; private set; }

        /// <summary>
        /// Execute the copy machine id command.
        /// </summary>
        /// <param name="param">The default parameter.</param>
        public void CopyMachineIdCommandExecute(object param)
        {
            Clipboard.SetText(this.MachineId);
        }
    }
}
