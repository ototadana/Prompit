using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TestStack.White;
using XPFriend.Prompit.Core;
using XPFriend.Prompit.Core.JUnit4;
using XPFriend.Prompit.Properties;

namespace XPFriend.Prompit
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Html = "<!doctype html><html><head><style>body,p,ul,ol,li{{font-size:9pt;font-family:Meiryo;}} ul{{list-style-type:none;}} ul li {{text-indent:-26px;margin-left:20px;}} ul li input[type=checkbox] {{margin-right:6px;}} td{{margin:8px}} .num{{text-align:right;}} .nb,.eb,.fb{{padding-left:3px;padding-top:3px;color:white;font-size:16px;width:23px;height:23px;}} .nb{{color:black;background-color:lightgreen}} .eb{{background-color:darkred}} .fb{{background-color:indigo}} code{{background-color:#fafafa;}} pre{{background-color:#fafafa;border:1px solid #f0f0f0;margin:20px 4px;padding:1em 4px 0em;font-size:9pt;}} td,th{{border:solid 1px gray;padding:2px;}} table{{border-collapse:collapse;}}</style></head><body>{0}</body></html>";
        private System.Windows.Forms.WebBrowser stepPanel;
        private WindowsFormsHost stepPanelHost;
        private Brush ErrorColor;
        private Brush NormalColor;
        private Brush BlueBrush;
        private Brush WhiteBrush;
        private MainWindowCore core;

        public MainWindow()
        {
            InitializeComponent();
            this.ErrorColor = (Brush)this.Resources["ErrorColor"];
            this.NormalColor = (Brush)this.Resources["NormalColor"];
            this.WhiteBrush = (Brush)this.Resources["WhiteBrush"];
            this.BlueBrush = (Brush)this.Resources["BlueBrush"];

            AddStep();
            Settings.Default.RestoreWindowPosition(this);
            this.core = new MainWindowCore();

            this.comment.GotKeyboardFocus += (sender, e) => DoWithTimer(() => this.comment.SelectAll(), 0.2);
            this.Activated += (sender, e) => DoWithTimer(() => this.comment.Focus(), 0.2);
            this.KeyUp += (sender, e) => this.NextByKeyUp(e.Key);
            this.KeyDown += (sender, e) => this.NextByKeyDown(e.Key);

            this.previousButton.Click += (sender, e) => Previous();
            this.nextButton.Click += (sender, e) => Next();
            this.screenShotButton.Click += (sender, e) => TakeScreenShot(() => Save());
            this.reloadButton.Click += (sender, e) =>
            {
                this.core.SaveLastStep();
                ReadScenarioFileAndUpdateWindow(this.core.ScenarioFilePath);
            };
            this.finishButton.Click += (sender, e) => Close();
            this.Closing += (sender, e) => Save();
            this.Loaded += (sender, e) => OpenScenarioFile();

            this.messageButton.Click += (sender, e) => HideMessage();
            this.messageText.MouseLeftButtonDown += (sender, e) =>
                this.core.OpenLogFile(this.messageText.ToolTip.ToString());

            this.KeyDown += (sender, e) => ChangeButtonIcon();
            this.KeyUp += (sender, e) => SetDefaultButtonIcon();
            this.stepPanel.DocumentCompleted += (sender, e) => AddCheckboxEvent();

        }

        private void AddCheckboxEvent()
        {
            foreach (System.Windows.Forms.HtmlElement element in
                this.stepPanel.Document.GetElementsByTagName("input"))
            {
                if (element.GetAttribute("type") == "checkbox")
                {
                    element.Click += (sender, e) =>
                    {
                        if (this.comment.Text == Properties.Resources.Untested)
                        {
                            this.comment.Text = "";
                        }
                    };
                }
            }
        }

        private void SetDefaultButtonIcon()
        {
            this.previousButton.Content = "\uf060";
            this.nextButton.Content = "\uf061";
        }

        private void ChangeButtonIcon()
        {
            if (IsShiftKeyDown())
            {
                this.previousButton.Content = "\uf049";
                this.nextButton.Content = "\uf050";
            }
            else if (IsCtrlKeyDown())
            {
                this.previousButton.Content = "\uf048";
                this.nextButton.Content = "\uf051";
            }
        }

        private void TakeScreenShot(Action action)
        {
            DoWithTimer(() =>
            {
                TakeScreenShotInternal();
                DoAction(action);
            });
        }

        private void TakeScreenShotInternal()
        {
            var windows = Desktop.Instance.Windows();
            if (windows.Count > 1)
            {
                windows[1].Focus();
                Thread.Sleep(100);
            }
            core.TakeScreenShot();
            windows[0].Focus();
        }

        private void ShowWaitingStatus()
        {
            Mouse.OverrideCursor = Cursors.Wait;
        }

        private void HideWaitingStatus()
        {
            Mouse.OverrideCursor = null;
        }

        private void Save()
        {
            if (this.core.Steps != null)
            {
                UpdateReport();
                Settings.Default.SaveWindowPosition(this);
            }
        }

        private void NextByKeyDown(Key key)
        {
            if (IsAltKeyDown())
            {
                Next(key);
            }
        }

        private void NextByKeyUp(Key key)
        {
            if (!this.comment.IsKeyboardFocused ||
                string.IsNullOrEmpty(this.comment.Text))
            {
                Next(key);
            }
        }

        private void Next(Key key)
        {
            Button button;
            if (key == Key.Left || Keyboard.IsKeyDown(Key.Left))
            {
                button = previousButton;
            }
            else if (key == Key.Right || Keyboard.IsKeyDown(Key.Right))
            {
                button = nextButton;
            }
            else
            {
                return;
            }
            if (!button.IsEnabled)
            {
                return;
            }
            ButtonAutomationPeer peer = new ButtonAutomationPeer(button);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }

        private void OpenScenarioFile()
        {
            string fileName = GetScenarioFileName();
            ReadScenarioFileAndUpdateWindow(fileName);
        }

        private string GetScenarioFileName()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                Settings.Default.SaveLastStep(args[1], 0);
                return args[1];
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Settings.Default.GetScenarioFileDirectory();
            dialog.Filter = XPFriend.Prompit.Properties.Resources.ScenarioFileDialogFilter;
            if (dialog.ShowDialog() != true)
            {
                Close();
                return null;
            }
            return dialog.FileName;
        }

        private void ReadScenarioFileAndUpdateWindow(string filePath)
        {
            string message = XPFriend.Prompit.Properties.Resources.Reading;
            this.scenario.Text = string.Format(message, Path.GetFileName(filePath));
            DoWithTimer(() =>
            {
                int index = Settings.Default.GetLastStepIndex(filePath);
                Steps steps = this.core.ReadScenarioFile(filePath, index);
                index = MainWindowCore.ToValidIndex(index, steps);
                if (!steps.Move(index) && !steps.Move(0))
                {
                    HideWaitingStatus();
                    MessageBox.Show(XPFriend.Prompit.Properties.Resources.InvalidFileFormat);
                    Close();
                    return;
                }
                this.stepPanel.DocumentCompleted += stepPanel_DocumentCompleted;
                UpdateWindow(steps);
            });
        }

        void stepPanel_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            this.stepPanel.DocumentCompleted -= stepPanel_DocumentCompleted;
            UpdateReport();
        }

        private void UpdateReport()
        {
            this.core.UpdateReport(
                this.comment.Text, GetCheckedStates(), this.stepPanel.Document.Body.InnerHtml);
        }

        private List<bool> GetCheckedStates()
        {
            List<bool> list = new List<bool>();
            foreach (System.Windows.Forms.HtmlElement element in 
                this.stepPanel.Document.GetElementsByTagName("input"))
            {
                if (element.GetAttribute("type") == "checkbox")
                {
                    list.Add(bool.Parse(element.GetAttribute("checked")));
                }
            }
            return list;
        }

        private void DoWithTimer(Action action, double time = 0.1)
        {
            ShowWaitingStatus();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(time);
            timer.Tick += (sender, e) =>
            {
                try
                {
                    timer.Stop();
                    action();
                }
                finally
                {
                    HideWaitingStatus();
                }
            };
            timer.Start();
        }

        private void RestoreEditorRowDefinition()
        {
            this.container.RowDefinitions[2].Height =
                new GridLength(this.container.RowDefinitions[2].Height.Value, GridUnitType.Auto);
        }

        private void UpdateProgress(Steps steps)
        {
            this.counter.Content = steps.CurrentIndex + "/" + steps.Count;
            this.progress.Maximum = steps.Count;
            this.progress.Value = steps.CurrentIndex;
            this.progress.Foreground = steps.HasError ? ErrorColor : NormalColor;
        }

        private void Next()
        {
            if (this.screenShotButtonBorder.BorderBrush == BlueBrush)
            {
                NextInternal(() => TakeScreenShotInternal());
            }
            else
            {
                NextInternal(null);
            }

        }

        private void NextInternal(Action action)
        {
            bool isShiftKeyDown = IsShiftKeyDown();
            bool isCtrlKeyDown = IsCtrlKeyDown();

            DoWithTimer(() =>
            {
                Steps steps = this.core.Steps;
                if (isShiftKeyDown)
                {
                    DoActionAndUpdateReport(action);
                    Move(steps, steps.Count - 1);
                    return;
                }

                if (isCtrlKeyDown)
                {
                    int index = steps.FindNextFailure();
                    if (index >= 0)
                    {
                        DoActionAndUpdateReport(action);
                        Move(steps, index);
                    }
                    return;
                }

                DoActionAndUpdateReport(action);
                if (steps.Next())
                {
                    UpdateWindow(steps);
                }
                else
                {
                    ShowTestSummary();
                }
            });
        }

        private void DoActionAndUpdateReport(Action action)
        {
            DoAction(action);
            UpdateReport();
        }
        
        private void DoAction(Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        private void Move(Steps steps, int index)
        {
            steps.Move(index);
            UpdateWindow(steps);
        }

        private void Previous()
        {
            bool isShiftKeyDown = IsShiftKeyDown();
            bool isCtrlKeyDown = IsCtrlKeyDown();

            DoWithTimer(() =>
            {
                Steps steps = this.core.Steps;
                UpdateReport();
                if (isShiftKeyDown)
                {
                    Move(steps, 0);
                    return;
                }

                if (isCtrlKeyDown)
                {
                    int index = steps.FindPreviousFailure();
                    if (index >= 0)
                    {
                        Move(steps, index);
                    }
                    return;
                }

                if (steps.Previous())
                {
                    UpdateWindow(steps);
                }
            });
        }

        private bool IsShiftKeyDown()
        {
            return Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift);
        }

        private bool IsCtrlKeyDown()
        {
            return Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl);
        }

        private bool IsAltKeyDown()
        {
            return Keyboard.IsKeyDown(Key.RightAlt) || Keyboard.IsKeyDown(Key.LeftAlt);
        }

        private void UpdateWindow(Steps steps)
        {
            UpdateProgress(steps);

            RestoreEditorRowDefinition();

            this.finishButton.Visibility = Visibility.Collapsed;
            this.reloadButton.Visibility = Visibility.Visible;
            this.comment.Visibility = Visibility.Visible;
            this.splitter.Visibility = Visibility.Visible;
            this.previousButton.IsEnabled = steps.CurrentIndex > 0;
            this.nextButton.IsEnabled = true;

            Step step = steps.CurrentStep;
            this.Title = step.Release.Name + " - " + step.Feature.Name;
            this.scenario.Text = step.Scenario.Name;
            this.scenario.ToolTip =
                string.IsNullOrWhiteSpace(step.Scenario.Description) ? null : step.Scenario.Description;
            this.stepPanel.DocumentText = string.Format(Html, step.Description);
            this.comment.Text = step.FailureComment;
            if (!string.IsNullOrEmpty(this.comment.Text))
            {
                this.comment.SelectAll();
                this.comment.Focus();
            }

            if (step.Description.IndexOf("[*]") > -1)
            {
                this.screenShotButton.IsEnabled = false;
                this.screenShotButtonBorder.BorderBrush = BlueBrush;
            }
            else
            {
                this.screenShotButton.IsEnabled = true;
                this.screenShotButtonBorder.BorderBrush = WhiteBrush;
            }
            step.Start();
        }

        private void ShowTestSummary()
        {
            Steps steps = this.core.Steps;
            UpdateProgress(steps);

            RestoreEditorRowDefinition();

            this.finishButton.Visibility = Visibility.Visible;
            this.reloadButton.Visibility = Visibility.Collapsed;
            this.comment.Visibility = Visibility.Collapsed;
            this.splitter.Visibility = Visibility.Collapsed;
            this.previousButton.IsEnabled = steps.CurrentIndex > 0;
            this.nextButton.IsEnabled = false;
            this.screenShotButton.IsEnabled = false;
            this.screenShotButtonBorder.BorderBrush = WhiteBrush;

            this.Title = steps.CurrentStep.Release.Name;
            this.comment.Text = "";
            this.scenario.Text = XPFriend.Prompit.Properties.Resources.TestResult;
            this.scenario.ToolTip = null;

            string htmlReportUrl = new Uri(core.HtmlReportFilePath).AbsoluteUri;

            testsuite suite = this.core.Testsuite;
            this.stepPanel.DocumentText = string.Format(Html,
                string.Format(XPFriend.Prompit.Properties.Resources.TestSummary,
                suite.tests, suite.errors, suite.failures, FormatTime(suite.time),
                htmlReportUrl));
        }

        private string FormatTime(string time)
        {
            return Util.Format(TimeSpan.FromSeconds(double.Parse(time)));
        }

        private void AddStep()
        {
            this.stepPanel = new System.Windows.Forms.WebBrowser();
            WindowsFormsHost host = new WindowsFormsHost();
            host.Child = stepPanel;
            this.container.Children.Add(host);
            Grid.SetRow(host, 1);
            this.stepPanelHost = host;
        }

        internal void ShowMessage(string message, string errorLogFile)
        {
            this.messageText.Text = message;
            this.messageText.ToolTip = errorLogFile;

            this.scenario.Visibility = Visibility.Collapsed;
            this.stepPanelHost.Visibility = Visibility.Collapsed;
            this.splitter.Visibility = Visibility.Collapsed;
            this.editor.Visibility = Visibility.Collapsed;
            this.messageText.Visibility = Visibility.Visible;
            this.messageButton.Visibility = Visibility.Visible;
        }

        private void HideMessage()
        {
            if (this.core == null || this.core.Steps == null || this.core.Steps.Count == 0)
            {
                this.Close();
                return;
            }

            this.messageText.Text = "";
            this.messageText.ToolTip = "";

            this.messageText.Visibility = Visibility.Collapsed;
            this.messageButton.Visibility = Visibility.Collapsed;

            this.scenario.Visibility = Visibility.Visible;
            this.stepPanelHost.Visibility = Visibility.Visible;
            this.splitter.Visibility = Visibility.Visible;
            this.editor.Visibility = Visibility.Visible;
        }
    }
}
