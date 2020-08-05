namespace QuestionGenerator.ExpertApp
{
    using DevExpress.LookAndFeel;
    using DevExpress.Skins;
    using QuestionGenerator.ExpertUI.Views;
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

            SkinManager.EnableFormSkins();
            UserLookAndFeel.Default.SetStyle(LookAndFeelStyle.Skin, false, false);

            new MainView().Show();
        }
    }
}