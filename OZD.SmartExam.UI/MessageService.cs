namespace OZD.SmartExam.UI
{
    using System.Windows;

    using DevExpress.Xpf.Core;

    using OZD.SmartExam.Library.Interfaces;

    /// <summary>
    /// Service to show messages.
    /// </summary>
    /// <seealso cref="OZD.SmartExam.Library.Interfaces.IMessageService" />
    public class MessageService : IMessageService
    {
        /// <summary>
        /// Shows the confirm dialog.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="message">The message.</param>
        /// <returns>
        /// True, if selected Yes, False otherwise.
        /// </returns>
        public bool ShowConfirmDialog(string caption, string message) => DXMessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="message">The message.</param>
        public void ShowDialog(string caption, string message) => DXMessageBox.Show(message, caption);
    }
}