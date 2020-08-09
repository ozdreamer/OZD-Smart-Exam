namespace OZD.SmartExam.Library.Data
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The database manager.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class DataManager : IDisposable
    {
        /// <summary>
        /// The connections
        /// </summary>
        private readonly IDictionary<int, SQLiteConnection> connections;

        /// <summary>
        /// The test connection
        /// </summary>
        private SQLiteConnection testConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataManager"/> class.
        /// </summary>
        public DataManager()
        {
            this.connections = new Dictionary<int, SQLiteConnection>();
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Initialize()
        {
            this.connections.Clear();

            var connectionStrings = ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>();
            var testConnectionString = connectionStrings.FirstOrDefault(x => x.Name == "TestConnection");
            if (testConnectionString == null)
            {
                throw new InvalidOperationException("Connection string for the Test database not found.");
            }

            var filePath = Regex.Replace(testConnectionString.ConnectionString.Trim(), "[D|d]ata [S|s]ource=*", string.Empty);
            if (!File.Exists(Path.GetFullPath(filePath)))
            {
                throw new FileNotFoundException("Test database not found.");
            }

            this.testConnection = new SQLiteConnection(testConnectionString.ConnectionString);
            this.testConnection.Open();

            foreach (var connectionString in connectionStrings)
            {
                var tokens = connectionString.Name.Split('.');
                if (tokens.Length == 2 && int.TryParse(tokens[1], out int grade))
                {
                    filePath = Regex.Replace(connectionString.ConnectionString.Trim(), "[D|d]ata [S|s]ource=*", string.Empty);
                    if (File.Exists(Path.GetFullPath(filePath)))
                    {
                        this.connections[grade] = new SQLiteConnection($"{connectionString.ConnectionString}");
                        this.connections[grade].Open();
                    }
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.testConnection != null)
            {
                this.testConnection.Close();
                this.testConnection.Dispose();
            }

            foreach (var connection in this.connections)
            {
                if (connection.Value != null)
                {
                    connection.Value.Close();
                    connection.Value.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets all subjects.
        /// </summary>
        /// <param name="grade">The grade.</param>
        /// <returns>
        /// List of subjects.
        /// </returns>
        public IEnumerable<Subject> GetAllSubjects(int grade)
        {
            var subjects = new List<Subject>();
            using (var command = new SQLiteCommand($"SELECT * FROM Subject", this.connections[grade]))
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
        /// Gets all exam types.
        /// </summary>
        /// <param name="grade">The grade.</param>
        /// <returns>The exam types.</returns>
        public IEnumerable<ExamType> GetAllExamTypes(int grade)
        {
            var examTypes = new List<ExamType>();
            using (var command = new SQLiteCommand($"SELECT * FROM ExamType", this.connections[grade]))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var examType = new ExamType
                        {
                            Id = Convert.ToInt32(reader[nameof(ExamType.Id)]),
                            Title = Convert.ToString(reader[nameof(ExamType.Title)])
                        };

                        examTypes.Add(examType);
                    }
                }
            }

            return examTypes;
        }

        /// <summary>
        /// Gets the available exams.
        /// </summary>
        /// <param name="examTypeId">Type of the exam.</param>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>List of exams.</returns>
        public IEnumerable<Exam> GetAvailableExams(int examTypeId, int subjectId, int grade)
        {
            var subjects = new List<Exam>();
            using (var command = new SQLiteCommand($"SELECT * FROM Exam WHERE TypeId={examTypeId} AND SubjectId={subjectId} AND Grade={grade} ORDER BY Year DESC", this.connections[grade]))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        subjects.Add(ReadExam(reader));
                    }
                }
            }

            return subjects;
        }

        /// <summary>
        /// Gets the exam.
        /// </summary>
        /// <param name="examId">The exam identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>The exam.</returns>
        public Exam GetExam(int examId, int grade)
        {
            using (var command = new SQLiteCommand($"SELECT * FROM Exam WHERE Id={examId}", this.connections[grade]))
            {
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return ReadExam(reader);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the question.
        /// </summary>
        /// <param name="questionId">The question identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>
        /// The selected question.
        /// </returns>
        public Question GetQuestion(int questionId, int grade)
        {
            using (var command = new SQLiteCommand($"SELECT * FROM Question Where Id={questionId}", this.connections[grade]))
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
        /// <param name="grade">The grade.</param>
        /// <returns>
        /// Collection of questions.
        /// </returns>
        public IEnumerable<Question> GetQuestionsForExam(int examId, int grade)
        {
            var questions = new List<Question>();
            using (var command = new SQLiteCommand($"SELECT * FROM Question Where ExamId={examId}", this.connections[grade]))
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
        /// <param name="examTypeId">The exam type identifier.</param>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>
        /// Collection of questions.
        /// </returns>
        public IEnumerable<Question> GetAvailableQuestions(int examTypeId, int subjectId, int grade)
        {
            var questions = new List<Question>();
            using (var command = new SQLiteCommand($"SELECT * FROM Question q INNER JOIN Exam e ON q.ExamId = e.Id WHERE e.TypeId={examTypeId} AND e.SubjectId={subjectId} AND e.Grade={grade}", this.connections[grade]))
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
            using (var command = new SQLiteCommand($"INSERT INTO Test(Name,ExamTypeId,SubjectId,ExamId,Grade,TotalQuestions,LastIndex,Score,Answers,StartDate,StartTime,EndDate,EndTime,TotalTime,ElapsedTime,Status) VALUES" +
                $"('{test.Name}',{test.ExamTypeId},{test.SubjectId},{test.ExamId?.ToString() ?? "null"},{test.Grade},{test.TotalQuestions},{test.LastIndex},{test.Score},'{test.Answers}','{test.StartDate.ToString("yyyy-MM-dd")}','{test.StartTime}','{test.EndDate.ToString("yyyy-MM-dd")}','{test.EndTime}','{test.TotalTime}','{test.ElapsedTime}','{test.Status}')", this.testConnection))
            {
                command.ExecuteNonQuery();

                using (var command2 = new SQLiteCommand("select last_insert_rowid()", this.testConnection))
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
            using (var command = new SQLiteCommand($"UPDATE Test SET Score={test.Score},LastIndex={test.LastIndex},Answers='{test.Answers}',EndDate='{test.EndDate.ToString("yyyy-MM-dd")}',EndTime='{test.EndTime}',ElapsedTime='{test.ElapsedTime}',Status='{test.Status}' WHERE Id={test.Id}", this.testConnection))
            {
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Gets the completed tests.
        /// </summary>
        /// <param name="examTypeId">The exam type identifier.</param>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>
        /// Collection of tests.
        /// </returns>
        public IEnumerable<Test> GetCompletedTests(int examTypeId, int subjectId, int grade)
        {
            var tests = new List<Test>();
            using (var command = new SQLiteCommand($"SELECT * FROM Test Where ExamTypeId={examTypeId} AND SubjectId={subjectId} AND Grade={grade} AND Status='Completed' ORDER BY EndDate DESC, EndTime DESC", this.testConnection))
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
        /// Gets the completed tests by exam.
        /// </summary>
        /// <param name="examId">The exam identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>Collection of tests.</returns>
        public IEnumerable<Test> GetCompletedTestsByExam(int examId, int grade)
        {
            var tests = new List<Test>();
            using (var command = new SQLiteCommand($"SELECT * FROM Test Where ExamId={examId} AND Grade={grade} AND Status='Completed' ORDER BY EndDate DESC, EndTime DESC", this.testConnection))
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
        /// <param name="examTypeId">The exam type identifier.</param>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>
        /// Collection of tests.
        /// </returns>
        public IEnumerable<Test> GetIncompleteTests(int examTypeId, int subjectId, int grade)
        {
            var tests = new List<Test>();
            using (var command = new SQLiteCommand($"SELECT * FROM Test Where ExamTypeId={examTypeId} AND SubjectId={subjectId} AND Grade={grade} AND Status='Incomplete' ORDER BY EndDate DESC, EndTime DESC", this.testConnection))
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
        /// Deletes the test.
        /// </summary>
        /// <param name="testId">The test identifier.</param>
        public void DeleteTest(int testId)
        {
            using (var command = new SQLiteCommand($"DELETE FROM Test Where Id={testId}", this.testConnection))
            {
                command.ExecuteNonQuery();
            }
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
                // AdditionalImage = Convert.ToString(reader[nameof(Question.AdditionalImage)]),
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
                ExamTypeId = Convert.ToInt32(reader[nameof(Test.ExamTypeId)]),
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
                ElapsedTime = Convert.ToDateTime(reader[nameof(Test.ElapsedTime)]).TimeOfDay,
                Status = Convert.ToString(reader[nameof(Test.Status)]),
            };
        }

        /// <summary>Reads the exam.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The exam.</returns>
        private static Exam ReadExam(SQLiteDataReader reader)
        {
            return new Exam
            {
                Id = Convert.ToInt32(reader[nameof(Exam.Id)]),
                TypeId = Convert.ToInt32(reader[nameof(Exam.TypeId)]),
                SubjectId = Convert.ToInt32(reader[nameof(Exam.SubjectId)]),
                Grade = Convert.ToInt32(reader[nameof(Exam.Grade)]),
                Year = Convert.ToInt32(reader[nameof(Exam.Year)]),
            };
        }

        /// <summary>
        /// Gets the grades.
        /// </summary>
        /// <returns>Collection of available grades.</returns>
        public IEnumerable<int> GetGrades() => this.connections.Keys.ToList();
    }
}