using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using TestStack.White;
using XPFriend.Prompit.Core.JUnit4;
using XPFriend.Prompit.Properties;

namespace XPFriend.Prompit.Core
{
    internal class MainWindowCore
    {
        private string workspacePath;
        private string scenarioFilePath;
        private string xmlReportFilePath;
        private string htmlReportFilePath;
        private string imagesDirectoryPath;
        private ReportWriter reportWriter;
        private Steps steps;

        internal Steps Steps { get { return steps; } }
        internal testsuite Testsuite 
        { 
            get 
            {
                return (this.reportWriter != null) ? this.reportWriter.Testsuite : null;
            } 
        }
        internal string ScenarioFilePath { get { return this.scenarioFilePath; } }
        internal string WorkspacePath { get { return this.workspacePath; } }
        internal string XmlReportFilePath
        {
            get
            {
                if (this.xmlReportFilePath == null)
                {
                    string directory;
                    string scenarioFileName;
                    GetScenarioFilePath(out directory, out scenarioFileName);
                    this.xmlReportFilePath = 
                        Path.GetFullPath(Path.Combine(directory, "TestResults", scenarioFileName + ".xml"));
                }
                return this.xmlReportFilePath;
            }
        }

        private void GetScenarioFilePath(out string directory, out string scenarioFileName)
        {
            directory = workspacePath;
            scenarioFileName = "st";
            if (scenarioFilePath != null)
            {
                scenarioFileName = Path.GetFileNameWithoutExtension(scenarioFilePath);
                directory = Path.GetDirectoryName(scenarioFilePath);
            }
        }

        internal string HtmlReportFilePath 
        {
            get
            {
                if (this.htmlReportFilePath == null)
                {
                    string directory;
                    string scenarioFileName;
                    GetScenarioFilePath(out directory, out scenarioFileName);
                    this.htmlReportFilePath =
                        Path.GetFullPath(Path.Combine(directory, "TestResults", scenarioFileName, "index.html"));
                }
                return this.htmlReportFilePath;
            }
        }

        internal string ImagesDirectoryPath
        {
            get
            {
                if(imagesDirectoryPath == null) 
                {
                    string results = Path.GetDirectoryName(XmlReportFilePath);
                    string scenario = Path.GetFileNameWithoutExtension(XmlReportFilePath);
                    imagesDirectoryPath = Path.Combine(results, scenario, "images");
                }
                return imagesDirectoryPath;
            }
        }

        internal MainWindowCore()
            : this(GetDefaultDirectory()) { }

        internal MainWindowCore(string myDocumentsPath)
        {
            this.workspacePath = GetWorkspacePath(myDocumentsPath);
        }

        internal static string GetDefaultDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        private static string GetWorkspacePath(string myDocumentsPath)
        {
            string workspacePath = Path.Combine(myDocumentsPath, "XPFriend\\Prompit");
            if (!File.Exists(workspacePath))
            {
                Directory.CreateDirectory(workspacePath);
            }
            return workspacePath;
        }

        internal Steps ReadScenarioFile(string scenarioFilePath, int index)
        {
            InitPaths(scenarioFilePath, index);
            string html = ReadScenarioFileAsHtml(scenarioFilePath);
            HtmlParser htmlParser = new HtmlParser(html);
            Release release = htmlParser.Release;
            Dictionary<string, testcase> testcases = new ReportReader().ReadXml(this.XmlReportFilePath, index);
            UpdateStepsByTestcases(release.Steps, testcases);

            this.reportWriter = new ReportWriter(release, htmlParser.HtmlReport);
            this.steps = new Steps(release.Steps);
            CleanImages(release.Steps, index);
            return this.steps;
        }

        private void InitPaths(string scenarioFilePath, int index)
        {
            this.scenarioFilePath = scenarioFilePath;
            this.xmlReportFilePath = null;
            this.imagesDirectoryPath = null;
        }

        private void CleanImages(List<Step> steps, int index)
        {
            if (index > 0)
            {
                DeleteImageFiles(steps, index);
            }
            else
            {
                CleanImagesDirectory();
            }
        }

        private void DeleteImageFiles(List<Step> steps, int index)
        {
            string directory = ImagesDirectoryPath;
            if (!Directory.Exists(directory))
            {
                return;
            }

            for (int i = 0; i < steps.Count && i < index; i++)
            {
                string imageFileName = Step.GetImageFileName(i);
                if (File.Exists(Path.Combine(directory, imageFileName)))
                {
                    steps[i].ImageFileName = imageFileName;
                }
            }

            for (int i = index; i < steps.Count; i++)
            {
                string imageFileName = Step.GetImageFileName(i);
                string imageFilePath= Path.Combine(directory, imageFileName);
                if (File.Exists(imageFilePath))
                {
                    DeleteFile(imageFilePath);
                }
            }
        }

        private void CleanImagesDirectory()
        {
            if (!Directory.Exists(ImagesDirectoryPath))
            {
                return;
            }

            try
            {
                Directory.Delete(ImagesDirectoryPath, true);
            }
            catch (Exception)
            {
                foreach (string file in Directory.GetFiles(ImagesDirectoryPath))
                {
                    DeleteFile(file);
                }
            }
        }

        private static void DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception)
            {
                File.WriteAllBytes(file, new byte[] { });
            }
        }

        internal static string ReadScenarioFileAsHtml(string scenarioFilePath)
        {
            Settings.Default.SaveScenarioFilePath(scenarioFilePath);
            string mdText = File.ReadAllText(scenarioFilePath);
            MarkdownDeep.Markdown md = new MarkdownDeep.Markdown();
            md.ExtraMode = true;
            return md.Transform(mdText);
        }

        private void UpdateStepsByTestcases(List<Step> steps, Dictionary<string, testcase> testcases)
        {
            foreach (Step step in steps)
            {
                testcase tcase;
                if (testcases.TryGetValue(step.Key, out tcase))
                {
                    if(tcase.failure != null && tcase.failure.Length > 0) 
                    {
                        step.FailureComment = tcase.failure[0].message;
                    }
                    if (tcase.systemerr != null && tcase.systemerr.Length > 0)
                    {
                        step.UpdateCheckedStatesByString(tcase.systemerr[0]);
                    }
                } 
                else 
                {
                    step.FailureComment = Properties.Resources.Untested;
                }
            }
        }

        internal void UpdateReport(string failureComment, List<bool> checkedStates, string description)
        {
            SaveLastStep();
            Steps steps = this.steps;
            if (steps.CurrentIndex >= steps.Count)
            {
                return;
            }
            Step currentStep = steps.CurrentStep;
            currentStep.Stop();
            currentStep.Description = description;
            currentStep.FailureComment =
                UpdateCheckedStatesAndGetFailureComment(currentStep, checkedStates, failureComment);
            this.WriteReport();
        }

        internal void SaveLastStep()
        {
            int index = (this.steps.CurrentIndex < this.steps.Count) ? this.steps.CurrentIndex : 0;
            Settings.Default.SaveLastStep(this.scenarioFilePath, index);
        }

        private string UpdateCheckedStatesAndGetFailureComment(
            Step currentStep, List<bool> checkedStates, string failureComment)
        {
            string comment = null;
            if (!string.IsNullOrWhiteSpace(failureComment) &&
                failureComment != Properties.Resources.Unchecked)
            {
                comment = failureComment;
            }

            for (int i = 0; i < currentStep.CheckedStates.Count; i++)
            {
                currentStep.CheckedStates[i] = checkedStates[i];
                if (comment == null && !checkedStates[i])
                {
                    comment = Properties.Resources.Unchecked;
                }
            }

            return comment;
        }

        private void WriteReport()
        {
            this.reportWriter.WriteReport(this.XmlReportFilePath, this.HtmlReportFilePath);
        }

        internal void OpenLogFile(string filePath)
        {
            Process.Start(filePath);
        }

        internal void TakeScreenShot()
        {
            this.steps.CurrentStep.ImageFileName = null;
            string imageFileName = GetImageFileName();
            string imageFilePath = Path.Combine(ImagesDirectoryPath, imageFileName);
            string directory = Path.GetDirectoryName(imageFilePath);
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            Desktop.TakeScreenshot(imageFilePath, ImageFormat.Png);
            this.steps.CurrentStep.ImageFileName = imageFileName;
        }

        private string GetImageFileName()
        {
            return Step.GetImageFileName(this.steps.CurrentIndex);
        }

        internal static int ToValidIndex(int index, Steps steps)
        {
            if (index >= steps.Count)
            {
                index = steps.Count - 1;
            }
            return index;
        }
    }
}
