using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using XPFriend.Prompit.Core.JUnit4;

namespace XPFriend.Prompit.Core
{
    internal class ReportWriter
    {
        private Release release;
        private HtmlReport htmlReport;
        private testsuite suite;

        internal testsuite Testsuite { get { return suite; } }

        internal ReportWriter(Release release, HtmlReport htmlReport)
        {
            this.release = release;
            this.htmlReport = htmlReport;
            this.suite = CreateTestsuite(release);
        }

        private static testsuite CreateTestsuite(Release release)
        {
            List<Step> steps = release.Steps;
            testsuite suite = new testsuite();
            suite.name = release.Name;
            suite.tests = steps.Count.ToString();
            suite.errors = "0";
            suite.failures = "0";
            suite.testcase = CreateTestcase(steps);
            return suite;
        }

        private static testcase[] CreateTestcase(List<Step> steps)
        {
            testcase[] tcases = new testcase[steps.Count];
            for (int i = 0; i < tcases.Length; i++)
            {
                Step step = steps[i];
                tcases[i] = new testcase()
                {
                    classname = step.ParentName,
                    name = step.Name,
                    systemout = new[] { step.Description },
                    time = "0.000"
                };
            }
            return tcases;
        }

        internal void WriteReport(string xmlReportFilePath, string htmlReportFilePath)
        {
            UpdateTestsuite();
            WriteXml(xmlReportFilePath);
            WriteHtml(htmlReportFilePath);
        }

        private void WriteXml(string filePath)
        {
            CreateDirectory(filePath);
            XmlSerializer serializer = new XmlSerializer(typeof(testsuite));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, this.suite);
            }
        }

        private static void CreateDirectory(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void UpdateTestsuite()
        {
            int errorCount = 0;
            int failureCount = 0;
            long time = 0;

            this.suite.timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

            List<Step> steps = this.release.Steps;
            testcase[] tcases = this.suite.testcase;
            for (int i = 0; i < tcases.Length; i++)
            {
                Step step = steps[i];
                testcase tcase = tcases[i];
                time += step.Time;
                tcase.time = ToString(step.Time);
                tcase.failure = null;
                tcase.error = null;

                if (!string.IsNullOrWhiteSpace(step.ErrorMessage))
                {
                    tcase.error = CreateError(step);
                    errorCount++;
                }

                if (!string.IsNullOrWhiteSpace(step.FailureComment))
                {
                    tcase.failure = CreateFailure(step);
                    failureCount++;
                }

                tcase.systemout = new string[] { step.Description };
                tcase.systemerr = new string[] { step.GetCheckedStatesAsString() };
                this.htmlReport.UpdateByStep(i, step);
            }

            this.suite.failures = failureCount.ToString();
            this.suite.errors = errorCount.ToString();
            this.suite.time = ToString(time);
            this.htmlReport.UpdateByTestsuite(this.suite);
        }

        private static failure[] CreateFailure(Step step)
        {
            return new[] { new failure() 
                    {
                        type = "Failure",
                        message = step.FailureComment.Trim(),
                        Text = new[] {step.FailureComment}
                    }};
        }

        private static error[] CreateError(Step step)
        {
            return new[] { new error() 
                    {
                        type = GetErrorType(step.ErrorType),
                        message = step.ErrorMessage.Trim(),
                        Text = new[] {step.ErrorText}
                    }};
        }

        private static string ToString(long time)
        {
            return (time / 10000000.0).ToString("0.000");
        }

        private static string GetErrorType(string errorType)
        {
            if (string.IsNullOrWhiteSpace(errorType))
            {
                return "Error";
            }
            else
            {
                return errorType;
            }
        }

        private void WriteHtml(string filePath)
        {
            CreateDirectory(filePath);
            htmlReport.Write(filePath);
        }
    }
}
