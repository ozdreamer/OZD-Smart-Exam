namespace OZD.SmartExam.UI.Views
{
    using System.Windows;
    using System.Windows.Input;
    using DevExpress.Xpf.Core;

    using OZD.SmartExam.Library.ViewModels;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : DXWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainView" /> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public MainView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public MainViewModel ViewModel => this.DataContext as MainViewModel;

        /// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Load();
        }

        /// <summary>
        /// Called when [key up].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            this.ViewModel.KeyPressed(e.Key, e.KeyboardDevice.Modifiers);
        }

        /// <summary>
        /// Called when [closed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnClosed(object sender, System.EventArgs e)
        {
            this.ViewModel.Closed();
        }
    }
}