using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionGenerator.UI
{
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
    }
}
