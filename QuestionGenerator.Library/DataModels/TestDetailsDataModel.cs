namespace QuestionGenerator.Library.DataModels
{
    using System;

    public class TestDetailsDataModel
    {
        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        public int Score { get; set; }

        public int TotalQuestions { get; set; }

        public TimeSpan TimeTaken {get;set;}

        public int Skipped { get; set; }

        public int Wrong { get; set; }

        public int Impossible { get; set; }

        public int Complex { get; set; }

        public int Hard { get; set; }

        public int Medium { get; set; }

        public int Easy { get; set; }
    }
}