namespace QuestionGenerator.Library.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;

    using Caliburn.Micro;
    using DevExpress.Xpf.Core;
    using QuestionGenerator.Library.Data;
    using QuestionGenerator.Library.DataModels;
    using QuestionGenerator.Library.Interfaces;
    using QuestionGenerator.Library.License;
    using DelegateCommand = QuestionGenerator.Library.DelegateCommand;

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

        /// <summary>
        /// The message service
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// The floating question view model
        /// </summary>
        private FloatingQuestionViewModel floatingQuestionViewModel;

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
        /// Gets or sets the SelectedSkin.
        /// </summary>
        /// <value>The SelectedSkin.</value>
        public string SelectedSkin
        {
            get
            {
                return SettingsManager.GetValueFromAppSettings("SelectedSkin") ?? "Office2016WhiteSE";
            }

            set
            {
                SettingsManager.SetValueToAppSettings("SelectedSkin", value);
                this.NotifyOfPropertyChange(() => this.SelectedSkin);
            }
        }


        /// <summary>
        /// Stores the value for the <see cref="IsLicenseValid" /> property.
        /// </summary>
        private bool isLicenseValid = false;

        /// <summary>
        /// Gets or sets the IsLicenseValid.
        /// </summary>
        /// <value>Flat to check whether the license is valid or not.</value>
        public bool IsLicenseValid
        {
            get
            {
                return this.isLicenseValid;
            }

            set
            {
                if (this.isLicenseValid != value)
                {
                    this.isLicenseValid = value;
                    this.NotifyOfPropertyChange(() => this.IsLicenseValid);
                }
            }
        }

        /// <summary>
        /// Gets the window service.
        /// </summary>
        /// <value>
        /// The window service.
        /// </value>
        //private IWindowService WindowService => this.GetService<IWindowService>();

        #region Primary Data

        /// <summary>
        /// Stores the value for the <see cref="ExamTypes" /> property.
        /// </summary>
        private IEnumerable<ExamType> examTypes;

        /// <summary>
        /// Gets or sets the ExamTypes.
        /// </summary>
        /// <value>The ExamTypes.</value>
        public IEnumerable<ExamType> ExamTypes
        {
            get
            {
                return this.examTypes;
            }

            set
            {
                if (this.examTypes != value)
                {
                    this.examTypes = value;
                    this.NotifyOfPropertyChange(() => this.ExamTypes);
                }
            }
        }

        /// <summary>
        /// The selected exam type.
        /// </summary>
        private ExamType selectedExamType;

        /// <summary>
        /// Gets or sets the SelectedExamType.
        /// </summary>
        /// <value>The SelectedExamType.</value>
        public ExamType SelectedExamType
        {
            get
            {
                if (this.selectedExamType == null)
                {
                    var examTypeId = int.TryParse(SettingsManager.GetValueFromAppSettings("SelectedExamType"), out int id) && id > 0 ? id : this.ExamTypes?.FirstOrDefault()?.Id ?? 0;
                    this.selectedExamType = this.ExamTypes?.FirstOrDefault(x => x.Id == examTypeId);
                }

                return this.selectedExamType;
            }

            set
            {
                if (this.selectedExamType != value)
                {
                    if (value != null)
                    {
                        SettingsManager.SetValueToAppSettings("SelectedExamType", value.Id.ToString());
                    }

                    this.selectedExamType = value;
                    this.NotifyOfPropertyChange(() => this.SelectedExamType);

                    this.LoadExamsAndTests();
                }
            }
        }

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
        /// The selected subject
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
                if (this.selectedSubject != null)
                {
                    var subjectId = int.TryParse(SettingsManager.GetValueFromAppSettings("SelectedSubject"), out int id) && id > 0 ? id : this.Subjects?.FirstOrDefault()?.Id ?? 0;
                    this.selectedSubject = this.Subjects?.FirstOrDefault(x => x.Id == subjectId);
                }

                return this.selectedSubject;
            }

            set
            {
                if (this.selectedSubject != value)
                {
                    if (value != null)
                    {
                        SettingsManager.SetValueToAppSettings("SelectedSubject", value.Id.ToString());
                    }

                    this.selectedSubject = value;
                    this.NotifyOfPropertyChange(() => this.SelectedSubject);

                    this.LoadExamsAndTests();
                }
            }
        }


        /// <summary>
        /// Stores the value for the <see cref="Grades" /> property.
        /// </summary>
        private IEnumerable<int> grades;

        /// <summary>
        /// Gets or sets the Grades.
        /// </summary>
        /// <value>The Grades.</value>
        public IEnumerable<int> Grades
        {
            get
            {
                return this.grades;
            }

            set
            {
                if (this.grades != value)
                {
                    this.grades = value;
                    this.NotifyOfPropertyChange(() => this.Grades);
                }
            }
        }

        /// <summary>
        /// The selected grade
        /// </summary>
        private int? selectedGrade;

        /// <summary>
        /// Gets or sets the SelectedGrade.
        /// </summary>
        /// <value>The SelectedGrade.</value>
        public int SelectedGrade
        {
            get
            {
                if (!this.selectedGrade.HasValue)
                {
                    this.selectedGrade = int.TryParse(SettingsManager.GetValueFromAppSettings("SelectedGrade"), out int grade) ? grade : 3;
                }

                return this.selectedGrade.Value;
            }

            set
            {
                if (this.selectedGrade.Value != value)
                {
                    if (value > 0)
                    {
                        SettingsManager.SetValueToAppSettings("SelectedGrade", value.ToString());
                    }

                    this.selectedGrade = value;
                    this.NotifyOfPropertyChange(() => this.SelectedGrade);

                    this.LoadExamsAndTests();
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
                        this.AnswerGiven = this.answers.ContainsKey(value.Id) ? this.answers[value.Id] : string.Empty;
                    }

                    // this.DisplayAdditionalQuestion();
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

        /// <summary>
        /// Stores the value of AnswerGiven
        /// </summary>
        private string answerGiven;

        /// <summary>
        /// The answer given by the user.
        /// </summary>
        public string AnswerGiven
        {
            get
            {
                return this.answerGiven;
            }

            set
            {
                if (this.answerGiven != value)
                {
                    this.answerGiven = value;
                    this.NotifyOfPropertyChange(() => this.AnswerGiven);
                }
            }
        }

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
        private bool isResultVisible = false;

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
                    this.NotifyOfPropertyChange(() => this.ResultTotalQuestionText);
                    this.NotifyOfPropertyChange(() => this.ResultCorrectQuestionText);
                    this.NotifyOfPropertyChange(() => this.ResultWrongQuestionText);
                    this.NotifyOfPropertyChange(() => this.ResultElapsedTimeText);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="IsTestInProgress" /> property.
        /// </summary>
        private bool isTestInProgress = false;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is test in progress.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is test in progress; otherwise, <c>false</c>.
        /// </value>
        public bool IsTestInProgress
        {
            get
            {
                return this.isTestInProgress;
            }

            set
            {
                if (this.isTestInProgress != value)
                {
                    this.isTestInProgress = value;
                    this.NotifyOfPropertyChange(() => this.IsTestInProgress);
                }
            }
        }

        /// <summary>
        /// Gets the total question text.
        /// </summary>
        /// <value>
        /// The total question text.
        /// </value>
        public string ResultTotalQuestionText => $"Total: {this.questions?.Count() ?? 0}";

        /// <summary>
        /// Gets the right question text.
        /// </summary>
        /// <value>
        /// The right question text.
        /// </value>
        public string ResultCorrectQuestionText => $"Correct: {this.CurrentTest?.Score ?? 0}";

        /// <summary>
        /// Gets the wrong question text.
        /// </summary>
        /// <value>
        /// The wrong question text.
        /// </value>
        public string ResultWrongQuestionText => $"Wrong: {(this.questions?.Count() ?? 0) - (this.CurrentTest?.Score ?? 0)}";

        /// <summary>
        /// Gets the result elapsed time text.
        /// </summary>
        /// <value>
        /// The result elapsed time text.
        /// </value>
        public string ResultElapsedTimeText => $"Time: {this.CurrentTest?.ElapsedTime.ToString(@"h\:mm\:ss")}";

        /// <summary>
        /// Stores the value for the <see cref="DonutChartData" /> property.
        /// </summary>
        private IEnumerable<ChartSeriesItem> donutChartData;

        /// <summary>
        /// Gets or sets the donut chart data.
        /// </summary>
        /// <value>
        /// The donut chart data.
        /// </value>
        public IEnumerable<ChartSeriesItem> DonutChartData
        {
            get
            {
                return this.donutChartData;
            }

            set
            {
                if (this.donutChartData != value)
                {
                    this.donutChartData = value;
                    this.NotifyOfPropertyChange(() => this.DonutChartData);
                }
            }
        }

        /// <summary>
        /// Stores the value for the <see cref="StackedChartData" /> property.
        /// </summary>
        private IEnumerable<HistoryDataModel> stackedChartData;

        /// <summary>
        /// Gets or sets the StackedChartData.
        /// </summary>
        /// <value>The StackedChartData.</value>
        public IEnumerable<HistoryDataModel> StackedChartData
        {
            get
            {
                return this.stackedChartData;
            }

            set
            {
                if (this.stackedChartData != value)
                {
                    this.stackedChartData = value;
                    this.NotifyOfPropertyChange(() => this.StackedChartData);
                    this.NotifyOfPropertyChange(() => this.HasHistoricalData);
                }
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance has historical data.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has historical data; otherwise, <c>false</c>.
        /// </value>
        public bool HasHistoricalData => this.StackedChartData != null && this.StackedChartData.Any() && this.StackedChartData.ElementAt(0).Data.Count() > 1;

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

        /// <summary>
        /// Stores the value for the <see cref="PastResults" /> property.
        /// </summary>
        private IEnumerable<TestDetailsDataModel> pastResults;

        /// <summary>
        /// Gets or sets the PastResults.
        /// </summary>
        /// <value>The PastResults.</value>
        public IEnumerable<TestDetailsDataModel> PastResults
        {
            get
            {
                return this.pastResults;
            }

            set
            {
                if (this.pastResults != value)
                {
                    this.pastResults = value;
                    this.NotifyOfPropertyChange(() => this.PastResults);
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

        /// <summary>
        /// Gets the delete test command.
        /// </summary>
        /// <value>
        /// The delete test command.
        /// </value>
        public DelegateCommand DeleteTestCommand { get; private set; }

        #endregion

        /// <summary>
        /// Gets or sets the message service.
        /// </summary>
        /// <value>
        /// The message service.
        /// </value>
        public IMessageService MessageService { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel" /> class.
        /// </summary>
        /// <param name="messageService">The message service.</param>
        public MainViewModel(/*IMessageService messageService*/)
        {
            //this.messageService = messageService;
            this.dataManager = new DataManager();
            this.floatingQuestionViewModel = new FloatingQuestionViewModel();

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
            this.DeleteTestCommand = new DelegateCommand(this.DeleteTestCommandExecute, this.DeleteTestCommandCanExecute);

            this.imageDirectory = Path.GetFullPath(ConfigurationManager.AppSettings["ImageDirectory"].ToString());
            this.InputSelectionType = InputSelectionType.IncompleteTest;
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        public void Load()
        {
            //LicenseManager.TempWriteLicenseFile();
            try
            {
                this.IsLicenseValid = LicenseManager.IsLicenseValid(out LicenseInfo licenseInfo);
            }
            catch (Exception ex)
            {
                DXMessageBox.Show($"Invalid license. {ex.Message}");
                return;
            }

            try
            {
                this.dataManager.Initialize();

                this.Grades = this.dataManager.GetGrades();
                var gradeFromSettings = SettingsManager.GetValueFromAppSettings("SelectedGrade");
                this.SelectedGrade = int.TryParse(gradeFromSettings, out int grade) && this.Grades.Contains(grade) ? grade : this.Grades.FirstOrDefault();

                if (this.SelectedGrade > 0)
                {
                    this.ExamTypes = this.dataManager.GetAllExamTypes(this.SelectedGrade);
                    var examTypeFromSettings = SettingsManager.GetValueFromAppSettings("SelectedExamType");
                    this.SelectedExamType = int.TryParse(examTypeFromSettings, out int examTypeId) ? this.ExamTypes.FirstOrDefault(x => x.Id == examTypeId) : this.ExamTypes.FirstOrDefault();

                    this.Subjects = this.dataManager.GetAllSubjects(this.SelectedGrade);
                    var subjectFromSettings = SettingsManager.GetValueFromAppSettings("SelectedSubject");
                    this.SelectedSubject = int.TryParse(subjectFromSettings, out int subject) ? this.Subjects.FirstOrDefault(x => x.Id == subject) : this.Subjects.FirstOrDefault();
                }
            }
            catch (FileNotFoundException ex)
            {
                this.MessageService.ShowDialog("Error", ex.Message);
            }
        }

        /// <summary>
        /// Loads the available exams.
        /// </summary>
        private void LoadExamsAndTests()
        {
            this.AvailableExams = this.SelectedGrade > 0 && this.SelectedExamType != null && this.SelectedSubject != null ? this.dataManager.GetAvailableExams(this.SelectedExamType.Id, this.SelectedSubject.Id, this.SelectedGrade) : new List<Exam>();
            this.SelectedExam = this.AvailableExams.FirstOrDefault();

            this.LoadTests();
        }

        /// <summary>
        /// Loads the completed tests.
        /// </summary>
        private void LoadTests()
        {
            this.IncompleteTests = new List<Test>();
            this.CompletedTests = new List<Test>();

            if (this.SelectedGrade > 0 && this.SelectedExamType != null && this.SelectedSubject != null)
            {
                this.IncompleteTests = this.dataManager.GetIncompleteTests(this.SelectedExamType.Id, this.SelectedSubject.Id, this.SelectedGrade);
                this.CompletedTests = this.dataManager.GetCompletedTests(this.SelectedExamType.Id, this.SelectedSubject.Id, this.SelectedGrade);
            }

            this.SelectedIncompleteTest = this.IncompleteTests.FirstOrDefault();
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
                    var question = this.dataManager.GetQuestion(e.Key, test.Grade);
                    if (question != null)
                    {
                        r.Add(question);
                    }

                    return r;
                });
        }

        /// <summary>
        /// Displays the additional question.
        /// </summary>
        private void DisplayAdditionalQuestion()
        {
            //if (!string.IsNullOrWhiteSpace(this.SelectedQuestion?.AdditionalImage) && this.QuestionImage != null)
            //{
            //    var directory = Path.GetDirectoryName(this.QuestionImage);
            //    this.floatingQuestionViewModel.QuestionImage = Path.Combine(directory, this.SelectedQuestion.AdditionalImage);
            //    //this.WindowService.Show("FloatingQuestionView", this.floatingQuestionViewModel);
            //}
            //else
            //{
            //    // this.WindowService.Close();
            //}
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
            this.CurrentTest.ElapsedTime += new TimeSpan(0, 0, 1);
            this.SetRemainingText();
        }

        /// <summary>
        /// Sets the remaining text.
        /// </summary>
        /// <param name="remainingTime">The remaining time.</param>
        private void SetRemainingText()
        {
            var remainingTime = this.CurrentTest.TotalTime - this.CurrentTest.ElapsedTime;
            this.RemainingTimeText = remainingTime < TimeSpan.Zero ? $"-{remainingTime.ToString(@"h\:mm\:ss")}" : remainingTime.ToString(@"hh\:mm\:ss");
        }

        /// <summary>
        /// Sets the examp progress text.
        /// </summary>
        private void SetExamProgressText() => this.ExamProgressText = $"{this.CurrentIndex + 1} of {this.CurrentTest.TotalQuestions}";

        /// <summary>
        /// Keys the pressed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="modifierKeys">The modifier keys.</param>
        public void KeyPressed(Key key, ModifierKeys modifierKeys)
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
        /// Creates the chart result.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <returns>Chart result data.</returns>
        private TestDetailsDataModel CreateChartResult(Test test)
        {
            var answers = Helper.StringToAnswer(test.Answers);
            var questions = this.GetQuestionFromTest(test);
            var reviewResults = GetReviewResults(questions, answers);

            return CreateChartResult(test, reviewResults);
        }

        /// <summary>
        /// Creates the chart result.
        /// </summary>
        /// <param name="test">The test.</param>
        /// <param name="reviewResults">The review results.</param>
        /// <returns>Chart result data.</returns>
        private static TestDetailsDataModel CreateChartResult(Test test, IEnumerable<ReviewResultDataModel> reviewResults)
        {
            var chartResult = new TestDetailsDataModel
            {
                Date = test.EndDate,
                Time = test.EndTime,
                Score = test.Score,
                TotalQuestions = test.TotalQuestions,
                TimeTaken = test.ElapsedTime
            };

            chartResult.Skipped = reviewResults.Count(x => x.IsNotAnswered);
            chartResult.Wrong = reviewResults.Count(x => !x.IsNotAnswered && !x.IsCorrect);
            chartResult.Impossible = reviewResults.Count(x => x.IsCorrect && x.Level == DifficultyLevel.Impossible);
            chartResult.Complex = reviewResults.Count(x => x.IsCorrect && x.Level == DifficultyLevel.Complex);
            chartResult.Hard = reviewResults.Count(x => x.IsCorrect && x.Level == DifficultyLevel.Hard);
            chartResult.Medium = reviewResults.Count(x => x.IsCorrect && x.Level == DifficultyLevel.Medium);
            chartResult.Easy = reviewResults.Count(x => x.IsCorrect && x.Level == DifficultyLevel.Easy);

            return chartResult;
        }
        /// <summary>
        /// Adds the chat series item.
        /// </summary>
        /// <param name="getClosure">The get closure.</param>
        /// <param name="title">The title.</param>
        /// <param name="foreground">The foreground.</param>
        /// <param name="chartData">The chart data.</param>
        private static void AddChatSeriesItem(Func<int> getClosure, string title, Color foreground, IList<ChartSeriesItem> chartData)
        {
            if (getClosure.Invoke() > 0)
            {
                chartData.Add(new ChartSeriesItem { Title = title, Value = getClosure.Invoke(), Foreground = foreground });
            }
        }

        /// <summary>
        /// Generates the chart data.
        /// </summary>
        private void GenerateChartData()
        {
            if (this.CurrentTest != null && this.ReviewResults != null)
            {
                var chartResult = CreateChartResult(this.CurrentTest, this.ReviewResults);
                var chartData = new List<ChartSeriesItem>();

                AddChatSeriesItem(() => chartResult.Skipped, "Skipped", Colors.Gray, chartData);
                AddChatSeriesItem(() => chartResult.Wrong, "Wrong", Colors.Pink, chartData);
                AddChatSeriesItem(() => chartResult.Impossible, DifficultyLevel.Impossible.ToString(), Helper.GetColorForDifficultyLevel(DifficultyLevel.Impossible), chartData);
                AddChatSeriesItem(() => chartResult.Complex, DifficultyLevel.Complex.ToString(), Helper.GetColorForDifficultyLevel(DifficultyLevel.Complex), chartData);
                AddChatSeriesItem(() => chartResult.Hard, DifficultyLevel.Hard.ToString(), Helper.GetColorForDifficultyLevel(DifficultyLevel.Hard), chartData);
                AddChatSeriesItem(() => chartResult.Medium, DifficultyLevel.Medium.ToString(), Helper.GetColorForDifficultyLevel(DifficultyLevel.Medium), chartData);
                AddChatSeriesItem(() => chartResult.Easy, DifficultyLevel.Easy.ToString(), Helper.GetColorForDifficultyLevel(DifficultyLevel.Easy), chartData);

                this.DonutChartData = chartData;
            }
        }

        /// <summary>
        /// Gets the results for exam.
        /// </summary>
        /// <param name="examId">The exam identifier.</param>
        /// <param name="grade">The grade.</param>
        /// <returns>Collection of chart results.</returns>
        private IEnumerable<TestDetailsDataModel> GetResultsForExam(int examId, int grade)
        {
            var pastTests = this.dataManager.GetCompletedTestsByExam(examId, grade);
            return pastTests.Select(this.CreateChartResult);
        }

        /// <summary>
        /// Generates the historical data.
        /// </summary>
        /// <param name="examId">The exam identifier.</param>
        /// <param name="grade">The grade.</param>
        private void GenerateHistoricalData()
        {
            IList<HistoryDataModel> historicalResults = null;
            if (this.CurrentTest.ExamId.HasValue && this.CurrentTest.ExamId.Value > 0)
            {
                var results = this.GetResultsForExam(this.CurrentTest.ExamId.Value, this.CurrentTest.Grade);
                historicalResults = new List<HistoryDataModel>();

                var correctResult = new HistoryDataModel { Title = "Correct" };
                var correctChartItems = new List<ChartSeriesItem>();
                foreach (var result in results)
                {
                    var title = $"{result.Date.ToString("dd-MMM-yy")} {result.Time.ToString(@"hh\:mm")}";
                    AddChatSeriesItem(() => result.TotalQuestions - result.Skipped - result.Wrong, title, Colors.LightGreen, correctChartItems);
                }

                correctResult.Data = correctChartItems;
                historicalResults.Add(correctResult);

                var wrongResult = new HistoryDataModel { Title = "Wrong" };
                var wrongChartItems = new List<ChartSeriesItem>();
                foreach (var result in results)
                {
                    var title = $"{result.Date.ToString("dd-MMM-yy")} {result.Time.ToString(@"hh\:mm")}";
                    AddChatSeriesItem(() => result.Wrong, title, Colors.Pink, wrongChartItems);
                }

                wrongResult.Data = wrongChartItems;
                historicalResults.Add(wrongResult);

                var skippedResult = new HistoryDataModel { Title = "Skipped" };
                var skippedChartItems = new List<ChartSeriesItem>();
                foreach (var result in results)
                {
                    var title = $"{result.Date.ToString("dd-MMM-yy")} {result.Time.ToString(@"hh\:mm")}";
                    AddChatSeriesItem(() => result.Skipped, title, Colors.Gray, skippedChartItems);
                }

                skippedResult.Data = skippedChartItems;
                historicalResults.Add(skippedResult);
            }

            this.StackedChartData = historicalResults;
        }

        /// <summary>
        /// Closeds this instance.
        /// </summary>
        public void Closed()
        {
            // Save the applicaiton settings.
        }

        /// <summary>
        /// Starts the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void StartTestCommandExecute(object param)
        {
            var questions = this.dataManager.GetQuestionsForExam(this.SelectedExam.Id, this.SelectedGrade);
            var answers = questions.Aggregate(new Dictionary<int, string>(), (res, x) =>
            {
                res.Add(x.Id, null);
                return res;
            });

            var test = new Test
            {
                ExamTypeId = this.SelectedExamType.Id,
                SubjectId = this.SelectedSubject.Id,
                ExamId = this.SelectedExam.Id,
                Grade = this.SelectedGrade,
                StartDate = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay,
                TotalTime = new TimeSpan(0, 45, 0),
                ElapsedTime = TimeSpan.Zero,
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
            this.OperatingMode = OperatingMode.Test;

            this.CurrentIndex = currentIndex;
            this.questions = questions;
            this.answers = Helper.StringToAnswer(test.Answers);

            this.CurrentTest = test;

            this.SetExamProgressText();
            this.SetRemainingText();

            this.SelectedQuestion = this.questions.ElementAtOrDefault(this.CurrentIndex);

            this.StartTimer();

            this.IsResultVisible = false;
            this.IsTestInProgress = true;
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
            this.answers[this.SelectedQuestion.Id] = answer;

            this.CurrentTest.LastIndex = this.CurrentIndex;
            this.CurrentTest.Answers = Helper.AnswerToString(this.answers);

            this.dataManager.UpdatetTest(this.CurrentTest);

            this.AnswerCommand?.RaiseCanExecuteChanged();

            // Last question or the final question to answer.
            if ((this.CurrentIndex == this.questions.Count() - 1 || this.answers.All(x => x.Value != null)) && (this.FinishTestCommand?.CanExecute(null) ?? false))
            {
                this.FinishTestCommand?.Execute(null);
            }

            // Navigate to the next question.
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
        private bool AnswerCommandCanExecute(object param) => this.SelectedQuestion != null;

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

                this.IsTestInProgress = false;

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
            this.InputSelectionType = InputSelectionType.ReviewTest;
            this.OperatingMode = OperatingMode.Review;

            this.answers = Helper.StringToAnswer(this.SelectedCompletedTest.Answers);
            this.questions = this.GetQuestionFromTest(this.SelectedCompletedTest);

            this.ReviewResults = GetReviewResults(this.questions, this.answers);
            this.SelectedReviewResult = null;

            this.CurrentTest = this.SelectedCompletedTest;

            this.GenerateChartData();
            this.GenerateHistoricalData();

            this.IsResultVisible = true;
        }

        /// <summary>
        /// Gets the review results.
        /// </summary>
        /// <param name="questions">The questions.</param>
        /// <param name="answers">The answers.</param>
        /// <returns>Collection of review results.</returns>
        private static IEnumerable<ReviewResultDataModel> GetReviewResults(IEnumerable<Question> questions, IDictionary<int, string> answers)
        {
            int index = 1;
            return questions.Aggregate(new List<ReviewResultDataModel>(), (r, e) =>
            {
                if (answers.ContainsKey(e.Id))
                {
                    var answer = answers[e.Id];
                    var result = new ReviewResultDataModel
                    {
                        Index = index++,
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

            var exam = this.dataManager.GetExam(this.SelectedCompletedTest.ExamId.Value, this.SelectedCompletedTest.Grade);
            var test = new Test
            {
                ExamTypeId = this.SelectedCompletedTest.ExamTypeId,
                SubjectId = this.SelectedCompletedTest.SubjectId,
                ExamId = null,
                Grade = this.SelectedCompletedTest.Grade,
                StartDate = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay,
                TotalTime = new TimeSpan(0, this.ReviewTestTime.Value, 0),
                ElapsedTime = TimeSpan.Zero,
                TotalQuestions = questions.Count(),
                LastIndex = -1,
                Score = 0,
                Answers = Helper.AnswerToString(answers),
                Name = $"{exam?.Year} (R)-{DateTime.Today.ToShortDateString()}-{DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}",
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
        private bool RetakeTestCommandCanExecute(object param) => this.SelectedCompletedTest != null && this.SelectedCompletedTest.ExamId != null && this.ReviewTestTime.HasValue && this.ReviewTestTime.Value > 0;

        /// <summary>
        /// Generates the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void GenerateTestCommandExecute(object param)
        {
            var questionRatios = new Dictionary<DifficultyLevel, double>
            {
                { DifficultyLevel.Easy, 0.3 },
                { DifficultyLevel.Medium, 0.25 },
                { DifficultyLevel.Hard, 0.2 },
                { DifficultyLevel.Complex, 0.15 },
                { DifficultyLevel.Impossible, 0.1 },
            };

            var chosenQuestions = new List<Question>();
            var rand = new Random();
            var availableQuestions = this.dataManager.GetAvailableQuestions(this.SelectedExamType.Id, this.SelectedSubject.Id, this.SelectedGrade).OrderBy(x => x.Level).GroupBy(x => x.Level);
            foreach (var questionGroup in availableQuestions)
            {
                var questionSubset = questionGroup.Select(x => x).ToList();
                var count = Math.Round(questionRatios[questionGroup.Key] * this.TotalQuestions.Value);
                if (count >= questionSubset.Count)
                {
                    chosenQuestions.AddRange(questionSubset);
                    continue;
                }

                count = Math.Min(count, this.TotalQuestions.Value - chosenQuestions.Count);
                var usedQuestionIds = new HashSet<int>();
                for (int i = 0; i < count; i++)
                {
                    var index = rand.Next(0, questionSubset.Count - 1);
                    if (!usedQuestionIds.Add(index))
                    {
                        i--;
                        continue;
                    }

                    chosenQuestions.Add(questionSubset[index]);
                }
            }

            var questions = chosenQuestions;
            var answers = questions.Aggregate(new Dictionary<int, string>(), (res, x) =>
            {
                res.Add(x.Id, null);
                return res;
            });

            var test = new Test
            {
                ExamTypeId = this.SelectedExamType.Id,
                SubjectId = this.SelectedSubject.Id,
                ExamId = 0,
                Grade = this.SelectedGrade,
                StartDate = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay,
                TotalTime = new TimeSpan(0, 45, 0),
                ElapsedTime = TimeSpan.Zero,
                TotalQuestions = questions.Count(),
                LastIndex = -1,
                Score = 0,
                Answers = Helper.AnswerToString(answers),
                Name = $"RND-{DateTime.Today.ToShortDateString()}-{DateTime.Now.Hour:D2}:{DateTime.Now.Minute:D2}",
                Status = "Incomplete",
            };

            this.dataManager.InsertTest(test);

            this.StartSelectedTest(test, questions, 0);
        }

        /// <summary>
        /// Generates the test command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns>True if can execute, False otherwise.</returns>
        private bool GenerateTestCommandCanExecute(object param) => this.SelectedSubject != null && this.TotalQuestions.HasValue && this.TotalQuestions.Value > 0;

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

        /// <summary>
        /// Deletes the test command execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void DeleteTestCommandExecute(object param)
        {
            if (param is Test testToDelete)
            {
                if (DXMessageBox.Show("Are you sure you want to delete this test?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.dataManager.DeleteTest(testToDelete.Id);
                    this.LoadTests();
                }
            }
        }

        /// <summary>
        /// Deletes the test command can execute.
        /// </summary>
        /// <param name="param">The parameter.</param>
        /// <returns></returns>
        private bool DeleteTestCommandCanExecute(object param) => true;
    }
}