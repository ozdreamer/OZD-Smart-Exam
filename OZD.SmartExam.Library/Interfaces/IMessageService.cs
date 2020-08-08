// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMessageService.cs" company="RPM Software Pty Ltd">
//   © Copyright RPM Software Pty Ltd, a wholly owned subsidiary of RPMGlobal Holdings Limited. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OZD.SmartExam.Library.Interfaces
{
    /// <summary>
    /// Documentation for IMessageService.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="message">The message.</param>
        void ShowDialog(string caption, string message);

        /// <summary>
        /// Shows the confirm dialog.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="message">The message.</param>
        /// <returns>True, if selected Yes, False otherwise.</returns>
        bool ShowConfirmDialog(string caption, string message);
    }
}
