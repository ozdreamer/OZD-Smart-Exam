namespace QuestionGenerator.Library
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    /// <summary>
    /// Helper class to facilitate the library.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Answers to string.
        /// </summary>
        /// <param name="answers">The answers.</param>
        /// <returns>The answer string.</returns>
        public static string AnswerToString(IDictionary<int, string> answers)
        {
            return answers.Aggregate(string.Empty, (r, e) =>
            {
                r += $"{e.Key},{e.Value ?? string.Empty};";
                return r;
            });
        }

        /// <summary>
        /// Strings to answer.
        /// </summary>
        /// <param name="answerString">The answer string.</param>
        /// <returns></returns>
        public static IDictionary<int, string> StringToAnswer(string answerString)
        {
            return answerString.Split(';').Aggregate(new Dictionary<int, string>(), (r, e) =>
            {
                var tokens = e.Split(',');
                if (tokens.Length == 2 && int.TryParse(tokens[0], out int questionId))
                {
                    r[questionId] = tokens[1] == string.Empty ? null : tokens[1];
                }
                return r;
            });
        }

        /// <summary>
        /// Gets the color for difficulty level.
        /// </summary>
        /// <param name="difficultyLevel">The difficulty level.</param>
        /// <returns>The color.</returns>
        public static Color GetColorForDifficultyLevel(DifficultyLevel difficultyLevel)
        {
            switch (difficultyLevel)
            {
                case DifficultyLevel.Easy:
                    return Colors.DarkGreen;

                case DifficultyLevel.Medium:
                    return Colors.Orange;

                case DifficultyLevel.Hard:
                    return Colors.DodgerBlue;

                case DifficultyLevel.Complex:
                    return Colors.Red;

                case DifficultyLevel.Impossible:
                    return Colors.DarkRed;

                default:
                    return Colors.Black;
            }
        }
    }
}