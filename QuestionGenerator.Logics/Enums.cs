namespace QuestionGenerator.UI
{
    /// <summary>
    /// The operating mode
    /// </summary>
    public enum OperatingMode
    {
        None = 0,

        Test = 1,

        Review = 2,
    }

    /// <summary>
    /// Difficulty level
    /// </summary>
    public enum DifficultyLevel
    {
        Easy = 1,

        Medium = 2,

        Hard = 3,

        Complex = 4,

        Impossible = 5
    }

    /// <summary>
    /// 
    /// </summary>
    public enum InputSelectionType
    {
        None,

        IncompleteTest,

        PastExam,

        RandomTest,

        ReviewTest,

        Newsletter
    }
}
