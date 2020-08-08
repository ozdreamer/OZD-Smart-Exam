namespace OZD.SmartExam.Library.DataModels
{
    using OZD.SmartExam.Library.Data;

    public class ReviewResultDataModel
    {
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        /// <value>
        /// The answer.
        /// </value>
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the correct answer.
        /// </summary>
        /// <value>
        /// The correct answer.
        /// </value>
        public string CorrectAnswer { get; set; }

        /// <summary>
        /// Gets or sets the level.
        /// </summary>
        /// <value>
        /// The level.
        /// </value>
        public DifficultyLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public int Category { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is correct.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is correct; otherwise, <c>false</c>.
        /// </value>
        public bool IsCorrect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not answered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is not answered; otherwise, <c>false</c>.
        /// </value>
        public bool IsNotAnswered { get; set; }

        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        /// <value>
        /// The question.
        /// </value>
        public Question Question { get; set; }
    }
}
