namespace QuestionGenerator.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    using Caliburn.Micro;

    using DevExpress.Xpf.Core;

    using QuestionGenerator.UI.Data;
    using QuestionGenerator.UI.DataModels;

    using DelegateCommand = QuestionGenerator.UI.DelegateCommand;

    public class MainViewModel : PropertyChangedBase
    {
        #region Generic Fields

        /// <summary>
        /// The event aggregator
        /// </summary>
        private DispatcherTimer timer;

        /// <summary>
        /// The data manager
        /// </summary>
        private readonly DataManager dataManager;

        /// <summary>
        /// The image directory
        /// </summary>
        private readonly string imageDirectory;

        #endregion

        /// <summary>
        /// Gets the skin collection.
        /// </summary>
        /// <value>
        /// The skin collection.
        /// </value>
        public IDictionary<string, string> SkinCollection => new Dictionary<string, string>
        {
            { "DXStyle", "Classic" },
            { "VS2017Light", "Light Grey" },
            { "Office2016White", "White" },
            { "Office2016WhiteSE", "White Plus" },
        };

        /// <summary>
        /// Stores the value for the <see cref="SelectedSkin" /> property.
        /// </summary>
        private string selectedSkin;

        /// <summary>
        /// Gets or sets the SelectedSkin.
        /// </summary>
        /// <value>The SelectedSkin.</value>
        public string SelectedSkin
        {
            get
            {
                return this.selectedSkin;
            }

            set
            {
                if (this.selectedSkin != value)
                {
                    this.selectedSkin = value;
                    this.NotifyOfPropertyChange(() => this.SelectedSkin);
                }
            }
        }

        #region Subject

        /// <summary>
        /// Gets or sets the subjects.
        /// </summary>
        /// <value>
        /// The subjects.
        /// </value>
        /// <summary>
        /// Stores the value for the <see cref="Subjects" /> property.
        /// </summary>
        private IEnumerable<Subject> subjects;

        /// <summary>
        /// Gets or sets the Subjects.
        /// </summary>
        /// <value>The Subjects.</value>
        public IEnumerable<Subject> Subjects
        {
            get
            {
                return this.subjects;
            }

            set
            {
                if (this.subjects != value)
                {
                    this.subjects = value;
                    this.NotifyOfPropertyChange(() => this.Subjects);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="SelectedSubject" /> property.
        /// </summary>
        private Subject selectedSubject;

        /// <summary>
        /// Gets or sets the SelectedSubject.
        /// </summary>
        /// <value>The SelectedSubject.</value>
        public Subject SelectedSubject
        {
            get
            {
                return this.selectedSubject;
            }

            set
            {
                if (this.selectedSubject != value)
                {
                    this.selectedSubject = value;
                    this.LoadAvailableData();
                    this.NotifyOfPropertyChange(() => this.SelectedSubject);
                }
            }
        }

        #endregion

        #region Grade

        /// <summary>
        /// Gets the grades.
        /// </summary>
        /// <value>
        /// The grades.
        /// </value>
        public IEnumerable<int> Grades => Enumerable.Range(1, 12);

        /// <summary>
        /// Stores the value for the <see cref="SelectedGrade" /> property.
        /// </summary>
        private int selectedGrade;

        /// <summary>
        /// Gets or sets the SelectedGrade.
        /// </summary>
        /// <value>The SelectedGrade.</value>
        public int SelectedGrade
        {
            get
            {
                return this.selectedGrade;
            }

            set
            {
                if (this.selectedGrade != value)
                {
                    this.selectedGrade = value;
                    this.LoadAvailableData();
                    this.NotifyOfPropertyChange(() => this.SelectedGrade);
                }
            }
        }

        #endregion

        #region Question

        /// <summary>
        /// Stores the value for the <see cref="TotalQuestions" /> property.
        /// </summary>
        private int? totalQuestions;

        /// <summary>
        /// Gets or sets the TotalQuestions.
        /// </summary>
        /// <value>The TotalQuestions.</value>
        public int? TotalQuestions
        {
            get
            {
                return this.totalQuestions;
            }

            set
            {
                if (this.totalQuestions != value)
                {
                    this.totalQuestions = value;
                    this.NotifyOfPropertyChange(() => this.TotalQuestions);
                }

                this.GenerateTestCommand?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="QuestionImage" /> property.
        /// </summary>
        private string questionImage;

        /// <summary>
        /// Gets or sets the QuestionImage.
        /// </summary>
        /// <value>The QuestionImage.</value>
        public string QuestionImage
        {
            get
            {
                return this.questionImage;
            }

            set
            {
                if (this.questionImage != value)
                {
                    this.questionImage = value;
                    this.NotifyOfPropertyChange(() => this.QuestionImage);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="SelectedQuestion" /> property.
        /// </summary>
        private Question selectedQuestion;

        /// <summary>
        /// Gets or sets the SelectedQuestion.
        /// </summary>
        /// <value>The SelectedQuestion.</value>
        public Question SelectedQuestion
        {
            get
            {
                return this.selectedQuestion;
            }

            set
            {
                if (this.selectedQuestion != value)
                {
                    this.selectedQuestion = value;
                    this.NotifyOfPropertyChange(() => this.SelectedQuestion);

                    if (value != null)
                    {
                        var exam = this.AvailableExams.FirstOrDefault(x => x.Id == value.ExamId);
                        this.QuestionImage = Path.Combine(this.imageDirectory, $"{this.SelectedSubject.Id}", $"{exam.Grade}", $"{exam.Year}", $"{value.Number}.png");
                    }
                }

                this.NextQuestionCommand?.RaiseCanExecuteChanged();
                this.PrevQuestionCommand?.RaiseCanExecuteChanged();
                this.AnswerCommand?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// The questions
        /// </summary>
        private IEnumerable<Question> questions;

        /// <summary>
        /// The answers
        /// </summary>
        private IDictionary<int, string> answers;

        #endregion

        #region Exam

        /// <summary>
        /// Stores the value for the <see cref="AvailableExams" /> property.
        /// </summary>
        private IEnumerable<Exam> availableExams;

        /// <summary>
        /// Gets or sets the AvailableExams.
        /// </summary>
        /// <value>The AvailableExams.</value>
        public IEnumerable<Exam> AvailableExams
        {
            get
            {
                return this.availableExams;
            }

            set
            {
                if (this.availableExams != value)
                {
                    this.availableExams = value;
                    this.NotifyOfPropertyChange(() => this.AvailableExams);
                }
            }
        }


        /// <summary>
        /// Stores the value for the <see cref="SelectedExam" /> property.
        /// </summary>
        private Exam selectedExam;

        /// <summary>
        /// Gets or sets the SelectedExam.
        /// </summary>
        /// <value>The SelectedExam.</value>
        public Exam SelectedExam
        {
            get
            {
                return this.selectedExam;
            }

            set
            {
                if (this.selectedExam != value)
                {
                    this.selectedExam = value;
                    this.NotifyOfPropertyChange(() => this.SelectedExam);

                    this.StartTestCommand?.RaiseCanExecuteChanged();
                    this.FinishTestCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        /// <summary>
        /// Stores the value for the <see cref="CurrentIndex" /> property.
        /// </summary>
        private int currentIndex;

        /// <summary>
        /// Gets or sets the CurrentIndex.
        /// </summary>
        /// <value>The CurrentIndex.</value>
        public int CurrentIndex
        {
            get
            {
                return this.currentIndex;
            }

            set
            {
                if (this.currentIndex != value)
                {
                    this.currentIndex = value;
                    this.NotifyOfPropertyChange(() => this.CurrentIndex);
                }
            }
        }


        /// <summary>
        /// Stores the value for the <see cref="CurrentTest" /> property.
        /// </summary>
        private Test currentTest;

        /// <summary>
        /// Gets or sets the CurrentTest.
        /// </summary>
        /// <value>The CurrentTest.</value>
        public Test CurrentTest
        {
            get
            {
                return this.currentTest;
            }

            set
            {
                if (this.currentTest != value)
                {
                    this.currentTest = value;
                    this.NotifyOfPropertyChange(() => this.CurrentTest);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="IsResultVisible" /> property.
        /// </summary>
        private bool isResultVisible;

        /// <summary>
        /// Gets or sets the IsResultVisible.
        /// </summary>
        /// <value>The IsResultVisible.</value>
        public bool IsResultVisible
        {
            get
            {
                return this.isResultVisible;
            }

            set
            {
                if (this.isResultVisible != value)
                {
                    this.isResultVisible = value;
                    this.NotifyOfPropertyChange(() => this.IsResultVisible);
                }

                if (value)
                {
                    this.NotifyOfPropertyChange(() => this.TotalQuestionText);
                    this.NotifyOfPropertyChange(() => this.CorrectQuestionText);
                    this.NotifyOfPropertyChange(() => this.WrongQuestionText);
                }
            }
        }

        /// <summary>
        /// Gets the total question text.
        /// </summary>
        /// <value>
        /// The total question text.
        /// </value>
        public string TotalQuestionText => $"Total: {this.questions?.Count() ?? 0}";

        /// <summary>
        /// Gets the right question text.
        /// </summary>
        /// <value>
        /// The right question text.
        /// </value>
        public string CorrectQuestionText => $"Correct: {this.CurrentTest?.Score ?? 0}";

        /// <summary>
        /// Gets the wrong question text.
        /// </summary>
        /// <value>
        /// The wrong question text.
        /// </value>
        public string WrongQuestionText => $"Wrong: {(this.questions?.Count() ?? 0) - (this.CurrentTest?.Score ?? 0)}";

        /// <summary>
        /// Stores the value for the <see cref="CompletedTests" /> property.
        /// </summary>
        private IEnumerable<Test> completedTests;

        /// <summary>
        /// Gets or sets the CompletedTests.
        /// </summary>
        /// <value>The CompletedTests.</value>
        public IEnumerable<Test> CompletedTests
        {
            get
            {
                return this.completedTests;
            }

            set
            {
                if (this.completedTests != value)
                {
                    this.completedTests = value;
                    this.NotifyOfPropertyChange(() => this.CompletedTests);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="SelectedCompletedTest" /> property.
        /// </summary>
        private Test selectedCompletedTest;

        /// <summary>
        /// Gets or sets the SelectedCompletedTest.
        /// </summary>
        /// <value>The SelectedCompletedTest.</value>
        public Test SelectedCompletedTest
        {
            get
            {
                return this.selectedCompletedTest;
            }

            set
            {
                if (this.selectedCompletedTest != value)
                {
                    this.selectedCompletedTest = value;
                    this.NotifyOfPropertyChange(() => this.SelectedCompletedTest);

                    this.ReviewTestCommand?.RaiseCanExecuteChanged();
                    this.RetakeTestCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="IncompleteTests" /> property.
        /// </summary>
        private IEnumerable<Test> incompleteTests;

        /// <summary>
        /// Gets or sets the IncompleteTests.
        /// </summary>
        /// <value>The IncompleteTests.</value>
        public IEnumerable<Test> IncompleteTests
        {
            get
            {
                return this.incompleteTests;
            }

            set
            {
                if (this.incompleteTests != value)
                {
                    this.incompleteTests = value;
                    this.NotifyOfPropertyChange(() => this.IncompleteTests);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="SelectedIncompleteTest" /> property.
        /// </summary>
        private Test selectedIncompleteTest;

        /// <summary>
        /// Gets or sets the SelectedIncompleteTest.
        /// </summary>
        /// <value>The SelectedIncompleteTest.</value>
        public Test SelectedIncompleteTest
        {
            get
            {
                return this.selectedIncompleteTest;
            }

            set
            {
                if (this.selectedIncompleteTest != value)
                {
                    this.selectedIncompleteTest = value;
                    this.NotifyOfPropertyChange(() => this.SelectedIncompleteTest);

                    this.ResumeExamCommand?.RaiseCanExecuteChanged();
                }
            }
        }


        /// <summary>
        /// Stores the value for the <see cref="ReviewResults" /> property.
        /// </summary>
        private IEnumerable<ReviewResultDataModel> reviewResults;

        /// <summary>
        /// Gets or sets the ReviewResults.
        /// </summary>
        /// <value>The ReviewResults.</value>
        public IEnumerable<ReviewResultDataModel> ReviewResults
        {
            get
            {
                return this.reviewResults;
            }

            set
            {
                if (this.reviewResults != value)
                {
                    this.reviewResults = value;
                    this.NotifyOfPropertyChange(() => this.ReviewResults);
                    this.NotifyOfPropertyChange(() => this.IsResultVisible);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="SelectedReviewResult" /> property.
        /// </summary>
        private ReviewResultDataModel selectedReviewReslt;

        /// <summary>
        /// Gets or sets the SelectedReviewResult.
        /// </summary>
        /// <value>The SelectedReviewResult.</value>
        public ReviewResultDataModel SelectedReviewResult
        {
            get
            {
                return this.selectedReviewReslt;
            }

            set
            {
                if (this.selectedReviewReslt != value)
                {
                    this.selectedReviewReslt = value;

                    if (value != null)
                    {
                        this.SelectedQuestion = value.Question;
                        this.IsResultVisible = false;
                    }

                    this.NotifyOfPropertyChange(() => this.SelectedReviewResult);
                }
            }
        }

        /// <summary>
        /// Gets the review test times.
        /// </summary>
        /// <value>
        /// The review test times.
        /// </value>
        public IEnumerable<int> ReviewTestTimes => Enumerable.Range(10, 35);

        /// <summary>
        /// Stores the value for the <see cref="ReviewTestTime" /> property.
        /// </summary>
        private int? reviewTestTime;

        /// <summary>
        /// Gets or sets the ReviewTestTime.
        /// </summary>
        /// <value>The ReviewTestTime.</value>
        public int? ReviewTestTime
        {
            get
            {
                return this.reviewTestTime;
            }

            set
            {
                if (this.reviewTestTime != value)
                {
                    this.reviewTestTime = value;
                    this.NotifyOfPropertyChange(() => this.ReviewTestTime);
                }

                this.RetakeTestCommand?.RaiseCanExecuteChanged();
            }
        }


        /// <summary>
        /// Stores the value for the <see cref="OperatingMode" /> property.
        /// </summary>
        private OperatingMode operatingMode;

        /// <summary>
        /// Gets or sets the OperatingMode.
        /// </summary>
        /// <value>The OperatingMode.</value>
        public OperatingMode OperatingMode
        {
            get
            {
                return this.operatingMode;
            }

            set
            {
                if (this.operatingMode != value)
                {
                    this.operatingMode = value;
                    this.NotifyOfPropertyChange(() => this.OperatingMode);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="InputSelectionType" /> property.
        /// </summary>
        private InputSelectionType inputSelectionType;

        /// <summary>
        /// Gets or sets the InputSelectionType.
        /// </summary>
        /// <value>The InputSelectionType.</value>
        public InputSelectionType InputSelectionType
        {
            get
            {
                return this.inputSelectionType;
            }

            set
            {
                if (this.inputSelectionType != value)
                {
                    this.inputSelectionType = value;
                    this.NotifyOfPropertyChange(() => this.InputSelectionType);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="RemainingTimeText" /> property.
        /// </summary>
        private string remainingTimeText;

        /// <summary>
        /// Gets or sets the RemainingTimeText.
        /// </summary>
        /// <value>The RemainingTimeText.</value>
        public string RemainingTimeText
        {
            get
            {
                return this.remainingTimeText;
            }

            set
            {
                if (this.remainingTimeText != value)
                {
                    this.remainingTimeText = value;
                    this.NotifyOfPropertyChange(() => this.RemainingTimeText);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="ExamProgressText" /> property.
        /// </summary>
        private string examProgressText;

        /// <summary>
        /// Gets or sets the ExamProgressText.
        /// </summary>
        /// <value>The ExamProgressText.</value>
        public string ExamProgressText
        {
            get
            {
                return this.examProgressText;
            }

            set
            {
                if (this.examProgressText != value)
                {
                    this.examProgressText = value;
                    this.NotifyOfPropertyChange(() => this.ExamProgressText);
                }
            }
        }

        #region Commands

        /// <summary>
        /// Gets or sets the start exam command.
        /// </summary>
        /// <value>
        /// The start exam command.
        /// </value>
        public DelegateCommand StartTestCommand { get; private set; }

        /// <summary>
        /// Gets the next question command.
        /// </summary>
        /// <value>
        /// The next question command.
        /// </value>
        public DelegateCommand NextQuestionCommand { get; private set; }

        /// <summary>
        /// Gets the previous question command.
        /// </summary>
        /// <value>
        /// The previous question command.
        /// </value>
        public DelegateCommand PrevQuestionCommand { get; private set; }

        /// <summary>
        /// Gets the answer command.
        /// </summary>
        /// <value>
        /// The answer command.
        /// </value>
        public DelegateCommand AnswerCommand { get; private set; }

        /// <summary>
        /// Gets the finish test command.
        /// </summary>
        /// <value>
        /// The finish test command.
        /// </value>
        public DelegateCommand FinishTestCommand { get; private set; }

        /// <summary>
        /// Gets the review test command.
        /// </summary>
        /// <value>
        /// The review test command.
        /// </value>
        public DelegateCommand ReviewTestCommand { get; private set; }

        /// <summary>
        /// Gets the resume exam command.
        /// </summary>
        /// <value>
        /// The resume exam command.
        /// </value>
        public DelegateCommand ResumeExamCommand { get; private set; }

        /// <summary>
        /// Gets the retake test command.
        /// </summary>
        /// <value>
        /// The retake test command.
        /// </value>
        public DelegateCommand RetakeTestCommand { get; private set; }

        /// <summary>
        /// Gets the generate test command.
        /// </summary>
        /// <value>
        /// The generate test command.
        /// </value>
        public DelegateCommand GenerateTestCommand { get; private set; }

        /// <summary>
        /// Gets the pause timer command.
        /// </summary>
        /// <value>
        /// The pause timer command.
        /// </value>
        public DelegateCommand PauseTimerCommand { get; private set; }

        /// <summary>
        /// Gets the play timer command.
        /// </summary>
        /// <value>
        /// The play timer command.
        /// </value>
        public DelegateCommand PlayTimerCommand { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        public MainViewModel()
        {
            this.dataManager = new DataManager();

            this.SelectedSkin = ConfigurationManager.AppSettings["SelectedSkin"].ToString();

            this.StartTestCommand = new DelegateCommand(this.StartTestCommandExecute, this.StartTestCommandCanExecute);
            this.NextQuestionCommand = new DelegateCommand(this.NextQuestionCommandExecute, this.NextQuestionCommandCanExecute);
            this.PrevQuestionCommand = new DelegateCommand(this.PrevQuestionCommandExecute, this.PrevQuestionCommandCanExecute);
            this.AnswerCommand = new DelegateCommand(this.AnswerCommandExecute, this.AnswerCommandCanExecute);
            this.FinishTestCommand = new DelegateCommand(this.FinishTestCommandExecute, this.FinishTestCommandCanExecute);
            this.ReviewTestCommand = new DelegateCommand(this.ReviewTestCommandExecute, this.ReviewTestCommandCanExecute);
            this.ResumeExamCommand = new DelegateCommand(this.ResumeExamCommandExecute, this.ResumeExamCommandCanExecute);
            this.RetakeTestCommand = new DelegateCommand(this.RetakeTestCommandExecute, this.RetakeTestCommandCanExecute);
            this.GenerateTestCommand = new DelegateCommand(this.GenerateTestCommandExecute, this.GenerateTestCommandCanExecute);
            this.PauseTimerCommand = new DelegateCommand(this.PauseTimerCommandExecute, this.PauseTimerCommandCanExecute);
            this.PlayTimerCommand = new DelegateCommand(this.PlayTimerCommandExecute, this.PlayTimerCommandCanExecute);

            this.imageDirectory = Path.GetFullPath(ConfigurationManager.AppSettings["ImageDirectory"].ToString());
            this.InputSelectionType = InputSelectionType.IncompleteTest;
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Load()
        {
            this.Subjects = this.dataManager.GetAllSubjects();
            this.SelectedSubject = this.Subjects.FirstOrDefault();
            this.SelectedGrade = 3;
        }

        /// <summary>
        /// Gets the exam by identifier.
        /// </summary>
        /// <param name="examId">The exam identifier.</param>
        /// <returns>The exam by Id.</returns>
        private Exam GetExamById(int examId) => this.AvailableExams.FirstOrDefault(x => x.Id == examId);

        /// <summary>
        /// Gets the question by identifier.
        /// </summary>
        /// <param name="questionId">The question identifier.</param>
        /// <returns>The question.</returns>
        private Question GetQuestionById(int questionId) => new Question { Id = 1, ExamId = 1, Answer = "B", Number = 14 };

        /// <summary>
        /// Loads the available exams.
        /// </summary>
        private void LoadAvailableData()
        {
            if (this.SelectedSubject != null && this.SelectedGrade > 0)
            {
                this.AvailableExams = this.dataManager.GetAvailableExams(this.SelectedSubject.Id, this.SelectedGrade);
                this.SelectedExam = this.AvailableExams.FirstOrDefault();

                this.LoadTests();
            }
        }

        /// <summary>
        /// Loads the completed tests.
        /// </summary>
        private void LoadTests()
        {
            this.IncompleteTests = this.dataManager.GetIncompleteTests(this.SelectedSubject.Id, this.SelectedGrade);
            this.SelectedIncompleteTest = this.IncompleteTests.FirstOrDefault();

            this.CompletedTests = this.dataManager.GetCompletedTests(this.SelectedSubject.Id, this.SelectedGrade);
            this.SelectedCompletedTest = this.CompletedTests.FirstOrDefault();
        }

        /// <summary>
        /// Gets the question from test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns>Collection of questions.</returns>
        private IEnumerable<Question> GetQuestionFromTest(Test test)
        {
            var answers = Helper.StringToAnswer(test.Answers);
            return answers.Aggregate(new List<Question>(), (r, e) =>
                {
                    var question = this.dataManager.GetQuestion(e.Key);
                    if (question != null)
                    {
                        r.Add(question);
                    }

                    return r;
                });
        }

        /// <summary>
        /// Runs the timer.
        /// </summary>
        private void StartTimer()
        {
            if (this.timer == null)
            {
                this.timer = new DispatcherTimer(
                        new TimeSpan(0, 0, 1),
                        DispatcherPriority.Background,
                        this.TimerTick,
                        Dispatcher.CurrentDispatcher);
            }

            this.timer.Start();

            this.PlayTimerCommand?.RaiseCanExecuteChanged();
            this.PauseTimerCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
            }

            this.PlayTimerCommand?.RaiseCanExecuteChanged();
            this.PauseTimerCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Timers the tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TimerTick(object sender, EventArgs e)
        {
            this.CurrentTest.RemainingTime -= new TimeSpan(0, 0, 1);
            this.SetRemainingText();

            if (this.CurrentTest.RemainingTime.TotalSeconds <= 0)
            {
                this.timer.Stop();
            }
        }

        /// <summary>
        /// Sets the remaining text.
        /// </summary>
        /// <param name="remainingTime">The remaining time.</param>
        private void SetRemainingText() => this.RemainingTimeText = $"{this.CurrentTest.RemainingTime.Hours}:{this.CurrentTest.RemainingTime.Minutes:D2}:{this.CurrentTest.RemainingTime.Seconds:D2}";

        /// <summary>
        /// Sets the examp progress text.
        /// </summary>
        private void SetExamProgressText() => this.ExamProgressText = $"{this.CurrentIndex + 1} of {this.CurrentTest.TotalQuestions}";

        /// <summary>
        /// Keys the pressed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="modifierKeys">The modifier keys.</param>
        internal void KeyPressed(Key key, ModifierKeys modifierKeys)
        {
            if (modifierKeys != ModifierKeys.None)
            {
                return;
            }

            switch (key)
            {
                case Key.A:
                    if (this.AnswerCommand?.CanExecute(null) ?? false)
                    {
                        this.AnswerCommand.Execute("A");
                    }

                    break;

                case Key.B:
                    if (this.AnswerCommand?.CanExecute(null) ?? false)
                    {
                        this.AnswerCommand.Execute("B");
                    }

                    break;

                case Key.C:
                    if (this.AnswerCommand?.CanExecute(null) ?? false)
                    {
                        this.AnswerCommand.Execute("C");
                    }

                    break;

                case Key.D:
                    if (this.AnswerCommand?.CanExecute(null) ?? false)
                    {
                        this.AnswerCommand.Execute("D");
                    }

                    break;

                case Key.OemComma:
                    if (this.PrevQuestionCommand?.CanExecute(null) ?? false)
                    {
                        this.PrevQuestionCommand.Execute(null);
                    }

                    break;

                case Key.OemPeriod:
                    if (this.NextQuestionCommand?.CanExecute(null) ?? false)
                    {
                        this.NextQuestionCommand.Execute(null);
                    }

                    break;
            }
        }

        /// <summary>
        /// Closeds this instance.
        /// </summary>
        internal void Closed()
        {
            // Save the applicaiton settings.
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["SelectedSkin"].Value = this.SelectedSkin;
            config.Save(ConfigurationSaveMode.Minimal);

            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Starts the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void StartTestCommandExecute(object param)
        {
            var questions = this.dataManager.GetQuestionsForExam(this.SelectedExam.Id);
            var answers = questions.Aggregate(new Dictionary<int, string>(), (res, x) =>
            {
                res.Add(x.Id, null);
                return res;
            });

            var test = new Test
            {
                SubjectId = this.SelectedSubject.Id,
                ExamId = this.SelectedExam.Id,
                Grade = this.SelectedGrade,
                StartDate = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay,
                TotalTime = new TimeSpan(0, 45, 0),
                RemainingTime = new TimeSpan(0, 45, 0),
                TotalQuestions = questions.Count(),
                LastIndex = -1,
                Score = 0,
                Answers = Helper.AnswerToString(answers),
                Name = $"{this.SelectedExam.Year}-{DateTime.Today.ToShortDateString()}-{DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}",
                Status = "Incomplete",
            };

            this.dataManager.InsertTest(test);

            this.StartSelectedTest(test, questions, 0);
        }

        /// <summary>
        /// Starts the selected test.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="questions">The questions.</param>
        /// <param name="currentIndex">Index of the current.</param>
        private void StartSelectedTest(Test test, IEnumerable<Question> questions, int currentIndex)
        {
            this.IsResultVisible = false;
            this.OperatingMode = OperatingMode.Test;

            this.CurrentIndex = currentIndex;
            this.questions = questions;
            this.answers = Helper.StringToAnswer(test.Answers);

            this.CurrentTest = test;

            this.SetExamProgressText();
            this.SetRemainingText();

            this.SelectedQuestion = this.questions.ElementAtOrDefault(this.CurrentIndex);

            this.StartTimer();
        }

        /// <summary>
        /// Determines whether this instance [can start test].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can start test]; otherwise, <c>false</c>.
        /// </returns>
        private bool StartTestCommandCanExecute(object param) => this.SelectedExam != null;

        /// <summary>
        /// Nexts the question command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void NextQuestionCommandExecute(object param)
        {
            this.SelectedQuestion = this.questions.ElementAt(++this.CurrentIndex);
            this.SetExamProgressText();
        }

        /// <summary>
        /// Nexts the question command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool NextQuestionCommandCanExecute(object param) => this.CurrentTest != null && this.CurrentIndex < this.CurrentTest.TotalQuestions - 1;

        /// <summary>
        /// Previouses the question command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void PrevQuestionCommandExecute(object param)
        {
            this.SelectedQuestion = this.questions.ElementAt(--this.CurrentIndex);
            this.SetExamProgressText();
        }

        /// <summary>
        /// Previouses the question command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool PrevQuestionCommandCanExecute(object param) => this.CurrentTest != null && this.CurrentIndex > 0;

        /// <summary>
        /// Answers the command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void AnswerCommandExecute(object param)
        {
            var answer = param as string;
            this.answers[this.SelectedQuestion.Id] = param as string;

            this.CurrentTest.LastIndex = this.CurrentIndex;
            this.CurrentTest.Answers = Helper.AnswerToString(this.answers);

            this.dataManager.UpdatetTest(this.CurrentTest);

            if (this.NextQuestionCommand?.CanExecute(null) ?? false)
            {
                this.NextQuestionCommand.Execute(null);
            }
        }

        /// <summary>
        /// Answers the command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool AnswerCommandCanExecute(object param) => this.SelectedQuestion != null && this.answers.ContainsKey(this.SelectedQuestion.Id) && this.answers[this.SelectedQuestion.Id] == null;

        /// <summary>
        /// Finishes the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void FinishTestCommandExecute(object param)
        {
            if (DXMessageBox.Show($"Are you sure your want to finish the test?", "Finish", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.CurrentTest.EndDate = DateTime.Today;
                this.CurrentTest.EndTime = DateTime.Now.TimeOfDay;
                this.CurrentTest.Answers = Helper.AnswerToString(this.answers);
                this.CurrentTest.Score = this.questions.Count(x => this.answers[x.Id] == x.Answer);
                this.CurrentTest.Status = "Completed";

                this.dataManager.UpdatetTest(this.CurrentTest);
                this.LoadTests();

                if (ReviewTestCommand?.CanExecute(null) ?? false)
                {
                    this.ReviewTestCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Finishes the test command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool FinishTestCommandCanExecute(object param) => this.SelectedExam != null;

        /// <summary>
        /// Reviews the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void ReviewTestCommandExecute(object param)
        {
            this.OperatingMode = OperatingMode.Review;

            this.answers = Helper.StringToAnswer(this.SelectedCompletedTest.Answers);
            this.questions = this.GetQuestionFromTest(this.SelectedCompletedTest);

            this.ReviewResults = questions.Aggregate(new List<ReviewResultDataModel>(), (r, e) =>
            {
                if (this.answers.ContainsKey(e.Id))
                {
                    var answer = this.answers[e.Id];
                    var result = new ReviewResultDataModel
                    {
                        Answer = answer,
                        CorrectAnswer = e.Answer,
                        Category = e.Category,
                        Level = e.Level,
                        IsCorrect = answer == e.Answer,
                        IsNotAnswered = string.IsNullOrWhiteSpace(answer),
                        Question = e
                    };

                    r.Add(result);
                }

                return r;
            });

            this.SelectedReviewResult = null;
            this.CurrentTest = this.SelectedCompletedTest;

            this.IsResultVisible = true;
        }

        /// <summary>
        /// Reviews the test command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool ReviewTestCommandCanExecute(object param) => this.SelectedCompletedTest != null;

        /// <summary>
        /// Resumes the exam command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void ResumeExamCommandExecute(object param)
        {
            var index = this.SelectedIncompleteTest.LastIndex + 1;
            if (index >= this.SelectedIncompleteTest.TotalQuestions)
            {
                DXMessageBox.Show("You have no more question to answer.", "Message", MessageBoxButton.OK, MessageBoxImage.Information);
                this.ReviewTestCommand?.Execute(null);
                return;
            }

            var questions = this.GetQuestionFromTest(this.SelectedIncompleteTest);
            this.StartSelectedTest(this.SelectedIncompleteTest, questions, index);
        }

        /// <summary>
        /// Resumes the exam command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool ResumeExamCommandCanExecute(object param) => this.SelectedIncompleteTest != null;

        /// <summary>
        /// Retakes the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void RetakeTestCommandExecute(object param)
        {
            var previousAnswers = Helper.StringToAnswer(this.SelectedCompletedTest.Answers);
            var questions = this.GetQuestionFromTest(this.SelectedCompletedTest).Aggregate(new List<Question>(), (r, e) =>
            {
                if (previousAnswers.ContainsKey(e.Id) && previousAnswers[e.Id] != e.Answer)
                {
                    r.Add(e);
                }

                return r;
            });

            var answers = questions.Aggregate(new Dictionary<int, string>(), (res, x) =>
            {
                res.Add(x.Id, null);
                return res;
            });

            var test = new Test
            {
                SubjectId = this.SelectedCompletedTest.SubjectId,
                ExamId = null,
                Grade = this.SelectedCompletedTest.Grade,
                StartDate = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay,
                TotalTime = new TimeSpan(0, this.ReviewTestTime.Value, 0),
                RemainingTime = new TimeSpan(0, this.ReviewTestTime.Value, 0),
                TotalQuestions = questions.Count(),
                LastIndex = -1,
                Score = 0,
                Answers = Helper.AnswerToString(answers),
                Name = $"{this.SelectedExam.Year} (R)-{DateTime.Today.ToShortDateString()}-{DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}",
                Status = "Incomplete",
            };

            this.dataManager.InsertTest(test);

            this.StartSelectedTest(test, questions, 0);
        }

        /// <summary>
        /// Retakes the test command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool RetakeTestCommandCanExecute(object param) => this.SelectedCompletedTest != null && this.ReviewTestTime.HasValue && this.ReviewTestTime.Value > 0;

        /// <summary>
        /// Generates the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void GenerateTestCommandExecute(object param)
        {

        }

        /// <summary>
        /// Generates the test command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool GenerateTestCommandCanExecute(object param) => this.TotalQuestions.HasValue && this.TotalQuestions.Value > 0;

        /// <summary>
        /// Pauses the timer command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void PauseTimerCommandExecute(object param)
        {
            this.StopTimer();
        }

        /// <summary>
        /// Pauses the timer command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool PauseTimerCommandCanExecute(object param) => this.timer != null && this.timer.IsEnabled;

        /// <summary>
        /// Plays the timer command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void PlayTimerCommandExecute(object param)
        {
            this.StartTimer();
        }

        /// <summary>
        /// Plays the timer command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool PlayTimerCommandCanExecute(object param) => this.timer != null && !this.timer.IsEnabled;
    }
}