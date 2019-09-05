using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionGenerator.UI.Data
{
    public class Question
    {
        public int Id { get; set; }

        public int ExamId { get; set; }

        public int Number { get; set; }

        public string Answer { get; set; }

        public int Category { get; set; }

        public DifficultyLevel Level { get; set; }
    }
}
