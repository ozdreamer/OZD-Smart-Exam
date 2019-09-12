using Caliburn.Micro;
using DevExpress.Mvvm;
using System.Windows.Controls;

namespace QuestionGenerator.Library.ViewModels
{
    public class FloatingQuestionViewModel : ViewModelBase
    {
        /// <summary>
        /// The question image
        /// </summary>
        private string questionImage;

        /// <summary>
        /// Gets or sets the question image.
        /// </summary>
        /// <value>
        /// The question image.
        /// </value>
        public string QuestionImage
        {
            get
            {
                return this.questionImage;
            }

            set
            {
                if (this.questionImage != value)
                {
                    this.questionImage = value;
                    this.RaisePropertiesChanged(nameof(this.QuestionImage));
                }
            }
        }
    }
}
