using System;

namespace QuestionGenerator.UI.Data
{
    public class Test
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SubjectId { get; set; }
        public int? ExamId { get; set; }
        public int Grade { get; set; }
        public int TotalQuestions { get; set; }
        public int LastIndex { get; set; }
        public int Score { get; set; }
        public string Answers { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public string Status { get; set; }
    }
}
