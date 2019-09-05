using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SQLite;

namespace QuestionGenerator.UI.Data
{
    public class DataManager : IDisposable
    {
        /// <summary>
        /// The connection
        /// </summary>
        private SQLiteConnection connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataManager"/> class.
        /// </summary>
        public DataManager()
        {
            var conn = ConfigurationManager.ConnectionStrings["SQLiteConnection"].ToString();
            this.connection = new SQLiteConnection(conn);
            this.connection.Open();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.connection != null)
            {
                this.connection.Close();
                this.connection.Dispose();
            }
        }

        /// <summary>
        /// Gets all subjects.
        /// </summary>
        /// <returns>List of subjects.</returns>
        public IEnumerable<Subject> GetAllSubjects()
        {
            var subjects = new List<Subject>();
            using (var command = new SQLiteCommand($"SELECT * FROM Subject", this.connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var subject = new Subject
                        {
                            Id = Convert.ToInt32(reader[nameof(Subject.Id)]),
                            Title = Convert.ToString(reader[nameof(Subject.Title)])
                        };

                        subjects.Add(subject);
                    }
                }
            }

            return subjects;
        }

        /// <summary>
        /// Gets the available exams.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>
        /// List of exams.
        /// </returns>
        public IEnumerable<Exam> GetAvailableExams(int subjectId, int grade)
        {
            var subjects = new List<Exam>();
            using (var command = new SQLiteCommand($"SELECT * FROM Exam Where SubjectId={subjectId} AND Grade={grade} ORDER BY Year DESC", this.connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var exam = new Exam
                        {
                            Id = Convert.ToInt32(reader[nameof(Exam.Id)]),
                            SubjectId = Convert.ToInt32(reader[nameof(Exam.SubjectId)]),
                            Grade = Convert.ToInt32(reader[nameof(Exam.Grade)]),
                            Year = Convert.ToInt32(reader[nameof(Exam.Year)]),
                        };

                        subjects.Add(exam);
                    }
                }
            }

            return subjects;
        }

        /// <summary>
        /// Gets the question.
        /// </summary>
        /// <param name="questionId">The question identifier.</param>
        /// <returns>The selected question.</returns>
        public Question GetQuestion(int questionId)
        {
            using (var command = new SQLiteCommand($"SELECT * FROM Question Where Id={questionId}", this.connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadQuestion(reader);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the questions for exam.
        /// </summary>
        /// <param name="examId">The exam identifier.</param>
        /// <returns>Collection of questions.</returns>
        public IEnumerable<Question> GetQuestionsForExam(int examId)
        {
            var questions = new List<Question>();
            using (var command = new SQLiteCommand($"SELECT * FROM Question Where ExamId={examId}", this.connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questions.Add(ReadQuestion(reader));
                    }
                }
            }

            return questions;
        }

        /// <summary>
        /// Gets the available questions.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>Collection of questions.</returns>
        public IEnumerable<Question> GetAvailableQuestions(int subjectId, int grade)
        {
            var questions = new List<Question>();
            using (var command = new SQLiteCommand($"SELECT * FROM Question Where SubjectId={subjectId} AND Grade={grade}", this.connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        questions.Add(ReadQuestion(reader));
                    }
                }
            }

            return questions;
        }

        /// <summary>
        /// Inserts the test.
        /// </summary>
        /// <param name="test">The test.</param>
        public void InsertTest(Test test)
        {
            using (var command = new SQLiteCommand($"INSERT INTO Test(Name,SubjectId,ExamId,Grade,TotalQuestions,LastIndex,Score,Answers,StartDate,StartTime,EndDate,EndTime,TotalTime,RemainingTime,Status) VALUES" +
                $"('{test.Name}',{test.SubjectId},{test.ExamId?.ToString() ?? "null"},{test.Grade},{test.TotalQuestions},{test.LastIndex},{test.Score},'{test.Answers}','{test.StartDate.ToString("yyyy-MM-dd")}','{test.StartTime}','{test.EndDate.ToString("yyyy-MM-dd")}','{test.EndTime}','{test.TotalTime}','{test.RemainingTime}','{test.Status}')", this.connection))
            {
                command.ExecuteNonQuery();

                using (var command2 = new SQLiteCommand("select last_insert_rowid()", this.connection))
                {
                    test.Id = (int)(long)command2.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// Updatets the test.
        /// </summary>
        /// <param name="test">The test.</param>
        public void UpdatetTest(Test test)
        {
            using (var command = new SQLiteCommand($"UPDATE Test SET Score={test.Score},LastIndex={test.LastIndex},Answers='{test.Answers}',EndDate='{test.EndDate.ToString("yyyy-MM-dd")}',EndTime='{test.EndTime}',RemainingTime='{test.RemainingTime}',Status='{test.Status}' WHERE Id={test.Id}", this.connection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the completed tests.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>Collection of tests.</returns>
        public IEnumerable<Test> GetCompletedTests(int subjectId, int grade)
        {
            var tests = new List<Test>();
            using (var command = new SQLiteCommand($"SELECT * FROM Test Where SubjectId={subjectId} AND Grade={grade} AND Status='Completed' ORDER BY EndDate DESC, EndTime DESC", this.connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tests.Add(ReadTest(reader));
                    }
                }
            }

            return tests;
        }


        /// <summary>
        /// Gets the completed tests.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>Collection of tests.</returns>
        public IEnumerable<Test> GetIncompleteTests(int subjectId, int grade)
        {
            var tests = new List<Test>();
            using (var command = new SQLiteCommand($"SELECT * FROM Test Where SubjectId={subjectId} AND Grade={grade} AND Status='Incomplete' ORDER BY EndDate DESC, EndTime DESC", this.connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tests.Add(ReadTest(reader));
                    }
                }
            }

            return tests;
        }


        /// <summary>
        /// Reads the question.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The question.</returns>
        private static Question ReadQuestion(SQLiteDataReader reader)
        {
            return new Question
            {
                Id = Convert.ToInt32(reader[nameof(Question.Id)]),
                ExamId = Convert.ToInt32(reader[nameof(Question.ExamId)]),
                Number = Convert.ToInt32(reader[nameof(Question.Number)]),
                Answer = Convert.ToString(reader[nameof(Question.Answer)]),
                Category = Convert.ToInt32(reader[nameof(Question.Category)]),
                Level = (DifficultyLevel)Convert.ToInt32(reader[nameof(Question.Level)]),
            };
        }

        /// <summary>
        /// Reads the test.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The test.</returns>
        private static Test ReadTest(SQLiteDataReader reader)
        {
            return new Test
            {
                Id = Convert.ToInt32(reader[nameof(Test.Id)]),
                Name = Convert.ToString(reader[nameof(Test.Name)]),
                ExamId = reader[nameof(Test.ExamId)] == DBNull.Value ? null : (int?)Convert.ToInt32(reader[nameof(Test.ExamId)]),
                SubjectId = Convert.ToInt32(reader[nameof(Test.SubjectId)]),
                Grade = Convert.ToInt32(reader[nameof(Test.Grade)]),
                TotalQuestions = Convert.ToInt32(reader[nameof(Test.TotalQuestions)]),
                LastIndex = Convert.ToInt32(reader[nameof(Test.LastIndex)]),
                Score = Convert.ToInt32(reader[nameof(Test.Score)]),
                Answers = Convert.ToString(reader[nameof(Test.Answers)]),
                StartDate = Convert.ToDateTime(reader[nameof(Test.StartDate)]),
                StartTime = Convert.ToDateTime(reader[nameof(Test.StartTime)]).TimeOfDay,
                EndDate = Convert.ToDateTime(reader[nameof(Test.EndDate)]),
                EndTime = Convert.ToDateTime(reader[nameof(Test.EndTime)]).TimeOfDay,
                TotalTime = Convert.ToDateTime(reader[nameof(Test.TotalTime)]).TimeOfDay,
                RemainingTime = Convert.ToDateTime(reader[nameof(Test.RemainingTime)]).TimeOfDay,
                Status = Convert.ToString(reader[nameof(Test.Status)]),
            };
        }
    }
}