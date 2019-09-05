using QuestionGenerator.UI.Data;

namespace QuestionGenerator.UI.DataModels
{
    public class ReviewResultDataModel
    {
        public string Answer { get; set; }

        public string CorrectAnswer { get; set; }

        public DifficultyLevel Level { get; set; }

        public int Category { get; set; }

        public bool IsCorrect { get; set; }

        public bool IsNotAnswered { get; set; }

        public Question Question { get; set; }
    }
}
