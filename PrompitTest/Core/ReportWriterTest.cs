using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.RegularExpressions;
using XPFriend.Prompit.Core.JUnit4;

namespace XPFriend.Prompit.Core
{
    [TestClass]
    public class ReportWriterTest
    {
        [TestMethod]
        public void TestConstructor()
        {
            // setup
            Release release = CreateRelease();
            HtmlReport htmlReport = CreateHtmlReport();

            // when
            ReportWriter writer = new ReportWriter(release, htmlReport);

            // then
            testsuite suite = writer.Testsuite;
            AssertTestsuite(suite);
            Assert.AreEqual("0", suite.errors);
            Assert.AreEqual("0", suite.failures);
            Assert.AreEqual("0.000", suite.testcase[0].time);
            Assert.AreEqual(null, suite.testcase[0].failure);
            Assert.AreEqual(null, suite.testcase[0].error);
            Assert.AreEqual("0.000", suite.testcase[1].time);
            Assert.AreEqual(null, suite.testcase[1].failure);
            Assert.AreEqual(null, suite.testcase[1].error);
        }

        private HtmlReport CreateHtmlReport()
        {
            string html = MainWindowCore.ReadScenarioFileAsHtml(TestUtil.GetTestResourcePath(@"Core\HtmlParserTest_03_Full.txt"));
            return new HtmlParser(html).HtmlReport;
        }

        private static void AssertTestsuite(testsuite suite)
        {
            Assert.AreEqual("2", suite.tests);
            Assert.AreEqual("R1", suite.name);
            Assert.AreEqual(2, suite.testcase.Length);
            Assert.AreEqual("F1.S1", suite.testcase[0].classname);
            Assert.AreEqual("s1", suite.testcase[0].name);
            Assert.AreEqual(1, suite.testcase[0].systemout.Length);
            Assert.AreEqual("d1", suite.testcase[0].systemout[0]);
            Assert.AreEqual("F1.S1", suite.testcase[1].classname);
            Assert.AreEqual("s2", suite.testcase[1].name);
            Assert.AreEqual(1, suite.testcase[1].systemout.Length);
            Assert.AreEqual("d2", suite.testcase[1].systemout[0]);
        }

        private Release CreateRelease()
        {
            Release release = new Release() { Name = "R1" };
            release.Features.Add(new Feature(release) { Name = "F1" });
            release.Features[0].Scenarios.Add(new Scenario(release.Features[0]) { Name = "S1" });
            release.Features[0].Scenarios[0].Steps.Add(new Step(release.Features[0].Scenarios[0])
            {
                Name = "s1",
                Description = "d1"
            });
            release.Features[0].Scenarios[0].Steps.Add(new Step(release.Features[0].Scenarios[0])
            {
                Name = "s2",
                Description = "d2"
            });

            return release;
        }

        [TestMethod]
        public void TestWriteReport()
        {
            // setup
            MainWindowCore core = TestUtil.CreateMainWindowCore(GetType().Name);
            Release release = CreateRelease();
            HtmlReport htmlReport = CreateHtmlReport();
            ReportWriter writer = new ReportWriter(release, htmlReport);
            string xmlFilePath = Delete(core.XmlReportFilePath);
            string htmlFilePath = Delete(core.HtmlReportFilePath);

            // when
            release.Steps[0].Time = 12340000L;
            writer.WriteReport(xmlFilePath, htmlFilePath);

            // then
            Assert.IsTrue(File.Exists(xmlFilePath));
            Assert.IsTrue(File.Exists(htmlFilePath));
            testsuite suite = writer.Testsuite;
            AssertTestsuite(suite);
            Assert.AreEqual("0", suite.errors);
            Assert.AreEqual("0", suite.failures);
            Assert.AreEqual("1.234", suite.testcase[0].time);
            Assert.AreEqual(null, suite.testcase[0].failure);
            Assert.AreEqual(null, suite.testcase[0].error);
            Assert.AreEqual("0.000", suite.testcase[1].time);
            Assert.AreEqual(null, suite.testcase[1].failure);
            Assert.AreEqual(null, suite.testcase[1].error);

            Assert.IsTrue(new Regex(@"[0-9]\.[0-9][0-9][0-9]", RegexOptions.None).IsMatch(suite.time));
            Assert.IsTrue(new Regex(@"2[0-1][0-9][0-9]-[0-1][0-9]-[0-3][0-9] [0-2][0-9]:[0-5][0-9]:[0-5][0-9]", RegexOptions.None).IsMatch(suite.timestamp));

            // when
            release.Steps[0].Time = 23450000L;
            release.Steps[1].Time = 12340000L;
            release.Steps[0].FailureComment = "a ";
            writer.WriteReport(xmlFilePath, htmlFilePath);

            // then
            AssertTestsuite(suite);
            Assert.AreEqual("0", suite.errors);
            Assert.AreEqual("1", suite.failures);
            Assert.AreEqual("2.345", suite.testcase[0].time);
            Assert.AreEqual(1, suite.testcase[0].failure.Length);
            Assert.AreEqual("a", suite.testcase[0].failure[0].message);
            Assert.AreEqual("Failure", suite.testcase[0].failure[0].type);
            Assert.AreEqual(1, suite.testcase[0].failure[0].Text.Length);
            Assert.AreEqual("a ", suite.testcase[0].failure[0].Text[0]);
            Assert.AreEqual(null, suite.testcase[0].error);
            Assert.AreEqual("1.234", suite.testcase[1].time);
            Assert.AreEqual(null, suite.testcase[1].failure);
            Assert.AreEqual(null, suite.testcase[1].error);

            // when
            release.Steps[1].ErrorMessage = "x ";
            release.Steps[1].ErrorText = "xx ";
            writer.WriteReport(xmlFilePath, htmlFilePath);

            // then
            AssertTestsuite(suite);
            Assert.AreEqual("1", suite.errors);
            Assert.AreEqual("1", suite.failures);
            Assert.AreEqual("2.345", suite.testcase[0].time);
            Assert.AreEqual(1, suite.testcase[0].failure.Length);
            Assert.AreEqual("a", suite.testcase[0].failure[0].message);
            Assert.AreEqual("Failure", suite.testcase[0].failure[0].type);
            Assert.AreEqual(1, suite.testcase[0].failure[0].Text.Length);
            Assert.AreEqual("a ", suite.testcase[0].failure[0].Text[0]);
            Assert.AreEqual(null, suite.testcase[0].error);

            Assert.AreEqual("1.234", suite.testcase[1].time);
            Assert.AreEqual(null, suite.testcase[1].failure);
            Assert.AreEqual(1, suite.testcase[1].error.Length);
            Assert.AreEqual("x", suite.testcase[1].error[0].message);
            Assert.AreEqual("Error", suite.testcase[1].error[0].type);
            Assert.AreEqual(1, suite.testcase[1].error[0].Text.Length);
            Assert.AreEqual("xx ", suite.testcase[1].error[0].Text[0]);

            // when
            release.Steps[0].ErrorMessage = "z ";
            release.Steps[0].ErrorText = "zz ";
            release.Steps[0].ErrorType = "zzz";
            writer.WriteReport(xmlFilePath, htmlFilePath);

            // then
            AssertTestsuite(suite);
            Assert.AreEqual("2", suite.errors);
            Assert.AreEqual("1", suite.failures);
            Assert.AreEqual("2.345", suite.testcase[0].time);
            Assert.AreEqual(1, suite.testcase[0].failure.Length);
            Assert.AreEqual("a", suite.testcase[0].failure[0].message);
            Assert.AreEqual("Failure", suite.testcase[0].failure[0].type);
            Assert.AreEqual(1, suite.testcase[0].failure[0].Text.Length);
            Assert.AreEqual("a ", suite.testcase[0].failure[0].Text[0]);

            Assert.AreEqual(1, suite.testcase[0].error.Length);
            Assert.AreEqual("z", suite.testcase[0].error[0].message);
            Assert.AreEqual("zzz", suite.testcase[0].error[0].type);
            Assert.AreEqual(1, suite.testcase[0].error[0].Text.Length);
            Assert.AreEqual("zz ", suite.testcase[0].error[0].Text[0]);

            Assert.AreEqual("1.234", suite.testcase[1].time);
            Assert.AreEqual(null, suite.testcase[1].failure);
            Assert.AreEqual(1, suite.testcase[1].error.Length);
            Assert.AreEqual("x", suite.testcase[1].error[0].message);
            Assert.AreEqual("Error", suite.testcase[1].error[0].type);
            Assert.AreEqual(1, suite.testcase[1].error[0].Text.Length);
            Assert.AreEqual("xx ", suite.testcase[1].error[0].Text[0]);

        }

        private static string Delete(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            Assert.IsFalse(File.Exists(filePath));
            return filePath;
        }
    }
}
