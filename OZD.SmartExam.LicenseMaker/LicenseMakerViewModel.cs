using Caliburn.Micro;
using DevExpress.Xpf.Core;
using Microsoft.Win32;
using OZD.SmartExam.Library.License;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using DelegateCommand = OZD.SmartExam.Library.DelegateCommand;

namespace OZD.SmartExam.LicenseMaker
{
    public class LicenseMakerViewModel : Caliburn.Micro.Screen
    {
        public WindowManager WindowManager { get; set; }

        public DelegateCommand BrowseDirectoryCommand { get; set; }

        public DelegateCommand CreateLicenseCommand { get; set; }

        private string machineId;

        public string MachineId
        {
            get => this.machineId;
            set 
            {
                if (this.machineId != value)
                {
                    this.machineId = value;
                    this.NotifyOfPropertyChange(() => this.MachineId);
                    this.CreateLicenseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private DateTime expiryDate;

        public DateTime ExpiryDate
        {
            get => this.expiryDate;
            set
            {
                if (this.expiryDate != value)
                {
                    this.expiryDate = value;
                    this.NotifyOfPropertyChange(() => this.ExpiryDate);
                    this.CreateLicenseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string directory;

        public string Directory
        {
            get => this.directory;
            set
            {
                if (this.directory != value)
                {
                    this.directory = value;
                    this.NotifyOfPropertyChange(() => this.Directory);
                    this.CreateLicenseCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public void BrowseDirectoryCommandExecuted(object param)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK && System.IO.Directory.Exists(dialog.SelectedPath))
                {
                    this.Directory = dialog.SelectedPath;
                }
            }
        }

        public LicenseMakerViewModel()
        {
            this.BrowseDirectoryCommand = new DelegateCommand(this.BrowseDirectoryCommandExecuted);
            this.CreateLicenseCommand = new DelegateCommand(this.CreateLicenseCommandExecuted, this.CreateLicenseCommandCanExecute);
            this.ExpiryDate = DateTime.Today.AddYears(1);
        }

        public bool CreateLicenseCommandCanExecute(object param) => !string.IsNullOrEmpty(this.MachineId) && !string.IsNullOrEmpty(this.Directory) && this.ExpiryDate != null;

        public void CreateLicenseCommandExecuted(object param)
        {
            var licenseFile = Path.Combine(this.Directory, "license.lic");
            try
            {
                LicenseManager.WriteLicenseFile(licenseFile, this.MachineId, this.ExpiryDate);
                DXMessageBox.Show(messageBoxText: "License created successfully", caption: "Success", button: MessageBoxButton.OK, icon: MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show(messageBoxText: ex.Message, caption: "Fail", button: MessageBoxButton.OK, icon: MessageBoxImage.Error);

            }
        }
    }
}
