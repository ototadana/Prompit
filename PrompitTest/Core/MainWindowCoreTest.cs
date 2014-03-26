using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using XPFriend.Prompit.Core.JUnit4;

namespace XPFriend.Prompit.Core
{
    [TestClass]
    public class MainWindowCoreTest
    {
        [TestInitialize]
        public void Setup()
        {
            string directory = TestUtil.GetTestResourcePath(@"Core\TestResults");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string resultFile = TestUtil.GetTestResourcePath(@"Core\TestResults\MainWindowCoreTest_01.xml");
            if (!File.Exists(resultFile))
            {
                string resultFileSource = TestUtil.GetTestResourcePath(@"Core\MainWindowCoreTest_01.xml");
                File.Copy(resultFileSource, resultFile);
            }
        }


        [TestMethod]
        public void TestConstructor()
        {
            // when
            MainWindowCore core = TestUtil.CreateMainWindowCore(GetType().Name);

            // then
            Assert.IsTrue(core.WorkspacePath.EndsWith(@"test\MainWindowCoreTest\XPFriend\Prompit"));
            Assert.IsTrue(core.XmlReportFilePath.EndsWith(@"test\MainWindowCoreTest\XPFriend\Prompit\TestResults\st.xml"));
            Assert.IsTrue(core.HtmlReportFilePath.EndsWith(@"test\MainWindowCoreTest\XPFriend\Prompit\TestResults\st\index.html"));
            Assert.IsTrue(core.ImagesDirectoryPath.EndsWith(@"test\MainWindowCoreTest\XPFriend\Prompit\TestResults\st\images"));
            Assert.IsNull(core.ScenarioFilePath);
            Assert.IsNull(core.Testsuite);
        }

        [TestMethod]
        public void TestReadScenarioFile()
        {
            {
                // setup
                string imageDirectoryPath = TestUtil.GetTestResourcePath(@"Core\TestResults\MainWindowCoreTest_01\images");
                string imageFilePath001 = Path.Combine(imageDirectoryPath, "step-001.png");
                string imageFilePath002 = Path.Combine(imageDirectoryPath, "step-002.png");
                if (!Directory.Exists(imageDirectoryPath))
                {
                    Directory.CreateDirectory(imageDirectoryPath);
                }
                File.WriteAllBytes(imageFilePath001, new byte[] { 1 });
                File.WriteAllBytes(imageFilePath002, new byte[] { 1 });

                // when
                MainWindowCore core = TestUtil.CreateMainWindowCore(GetType().Name);
                string path = TestUtil.GetTestResourcePath(@"Core\MainWindowCoreTest_01.txt");
                Steps steps = core.ReadScenarioFile(path, 5);
                // then
                Assert.IsTrue(steps.Next());
                HtmlParserTest.AssertFull(steps.CurrentStep.Release);
                AssertComment(steps.CurrentStep.Release.Steps);
                Assert.AreEqual(path, core.ScenarioFilePath);
                Assert.IsTrue(core.XmlReportFilePath.EndsWith(@"\Core\TestResults\MainWindowCoreTest_01.xml"));
                Assert.IsTrue(core.HtmlReportFilePath.EndsWith(@"\Core\TestResults\MainWindowCoreTest_01\index.html"));
                Assert.IsTrue(core.ImagesDirectoryPath.EndsWith(@"\Core\TestResults\MainWindowCoreTest_01\images"));
                Assert.IsNotNull(core.Testsuite);
                Assert.IsTrue(File.Exists(imageFilePath001));
                Assert.IsTrue(File.Exists(imageFilePath002));

                core.ReadScenarioFile(path, 2);
                Assert.IsTrue(File.Exists(imageFilePath001));
                Assert.IsTrue(File.Exists(imageFilePath002));

                core.ReadScenarioFile(path, 1);
                Assert.IsTrue(File.Exists(imageFilePath001));
                Assert.IsTrue(!File.Exists(imageFilePath002));

                core.ReadScenarioFile(path, 0);
                Assert.IsTrue(!File.Exists(imageFilePath001));
                Assert.IsTrue(!File.Exists(imageFilePath002));
            }

            {
                // when
                MainWindowCore core = TestUtil.CreateMainWindowCore(GetType().Name);
                string path = TestUtil.GetTestResourcePath(@"Core\MainWindowCoreTest_02.txt");
                Steps steps = core.ReadScenarioFile(path, 0);

                // then
                Assert.IsTrue(steps.Next());
                HtmlParserTest.AssertFull(steps.CurrentStep.Release);
                foreach (Step step in steps.CurrentStep.Release.Steps)
                {
                    Assert.AreEqual(Properties.Resources.Untested, step.FailureComment);
                }
                Assert.AreEqual(path, core.ScenarioFilePath);
                Assert.IsTrue(core.XmlReportFilePath.EndsWith(@"\Core\TestResults\MainWindowCoreTest_02.xml"));
                Assert.IsTrue(core.HtmlReportFilePath.EndsWith(@"\Core\TestResults\MainWindowCoreTest_02\index.html"));
                Assert.IsTrue(core.ImagesDirectoryPath.EndsWith(@"\Core\TestResults\MainWindowCoreTest_02\images"));
                Assert.IsNotNull(core.Testsuite);
                AssertCleanDirectory(core.ImagesDirectoryPath);

                // when
                Directory.CreateDirectory(core.ImagesDirectoryPath);
                core.ReadScenarioFile(path, 0);
                // then
                AssertCleanDirectory(core.ImagesDirectoryPath);

                // setup
                if (!Directory.Exists(core.ImagesDirectoryPath))
                {
                    Directory.CreateDirectory(core.ImagesDirectoryPath);
                }
                string imageFile = Path.Combine(core.ImagesDirectoryPath, "test1.png");
                using (FileStream stream = new FileStream(imageFile, FileMode.Create))
                {
                    stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
                    stream.Flush();
                }
                Assert.IsTrue(File.Exists(imageFile));
                Assert.IsTrue(new FileInfo(imageFile).Length > 0);
                // when
                core.ReadScenarioFile(path, 0);
                // then
                AssertCleanDirectory(core.ImagesDirectoryPath);
            }
        }

        private void AssertCleanDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            string [] files = Directory.GetFiles(path);
            if(files.Length == 0) 
            {
                return;
            }
            Assert.IsTrue(files.All(file => new FileInfo(file).Length == 0));
        }

        private void AssertComment(System.Collections.Generic.List<Step> steps)
        {
            Assert.AreEqual(null, steps[0].FailureComment);
            Assert.AreEqual("Unimplemented.", steps[1].FailureComment);
            Assert.AreEqual(null, steps[2].FailureComment);
            Assert.AreEqual("xxx", steps[3].FailureComment);
            Assert.AreEqual("Unimplemented.", steps[4].FailureComment);
        }

        [TestMethod]
        public void TestReadScenarioFileAsHtml()
        {
            // when
            string html = MainWindowCore.ReadScenarioFileAsHtml(TestUtil.GetTestResourcePath(@"Core\HtmlParserTest_03_Full.txt"));

            // then
            Console.WriteLine(html);
            Assert.IsTrue(html.StartsWith("<h1>"));
        }

        [TestMethod]
        public void TestUpdateReport()
        {
            // setup
            MainWindowCore core = TestUtil.CreateMainWindowCore(GetType().Name);
            string scenarioFilePath = CreateScenarioFile(core);
            string xmlReportFilePath = Path.Combine(core.WorkspacePath, "TestResults", Path.GetFileNameWithoutExtension(scenarioFilePath) + ".xml");
            string htmlReportFilePath = Path.Combine(core.WorkspacePath, "TestResults", Path.GetFileNameWithoutExtension(scenarioFilePath) + "\\index.html");
            core.ReadScenarioFile(scenarioFilePath, 0);
            Assert.IsFalse(File.Exists(xmlReportFilePath));
            Assert.IsFalse(File.Exists(htmlReportFilePath));

            // when
            core.Steps.Move(core.Steps.Count);
            core.Steps.Next();
            string description = core.Steps.CurrentStep.Description;
            core.UpdateReport("xx", null, "x");
            // then
            Assert.IsFalse(File.Exists(xmlReportFilePath));
            Assert.IsFalse(File.Exists(htmlReportFilePath));
            Assert.AreEqual(description, core.Steps.CurrentStep.Description);

            // when
            core.Steps.Move(0);
            core.Steps.CurrentStep.FailureComment = "...";
            core.Steps.CurrentStep.CheckedStates.AddRange(new bool[] { true, false });
            core.UpdateReport("zz", new List<bool> { false, false }, "z");
            // then
            Assert.IsTrue(File.Exists(xmlReportFilePath));
            Assert.IsTrue(File.Exists(htmlReportFilePath));
            Dictionary<string, testcase> dictionary = new ReportReader().ReadXml(xmlReportFilePath, 1);
            Assert.AreEqual("zz", core.Steps.CurrentStep.FailureComment);
            Assert.AreEqual("zz", dictionary["フィーチャー 1.シナリオ 1.ステップ その(1)"].failure[0].message);
            string html = File.ReadAllText(htmlReportFilePath);
            Assert.IsTrue(html.IndexOf("リリース") > -1);
            Assert.AreEqual("z", core.Steps.CurrentStep.Description);
            Assert.AreEqual(2, core.Steps.CurrentStep.CheckedStates.Count);
            Assert.AreEqual(false, core.Steps.CurrentStep.CheckedStates[0]);
            Assert.AreEqual(false, core.Steps.CurrentStep.CheckedStates[1]);
            Assert.AreEqual(scenarioFilePath + "|0/", Properties.Settings.Default.LastSteps);

            // when
            core.Steps.CurrentStep.FailureComment = "...";
            core.UpdateReport(null, new List<bool> { true, false }, "z");
            // then
            Assert.AreEqual(true, core.Steps.CurrentStep.CheckedStates[0]);
            Assert.AreEqual(false, core.Steps.CurrentStep.CheckedStates[1]);
            Assert.AreEqual(Properties.Resources.Unchecked, core.Steps.CurrentStep.FailureComment);

            // when
            core.Steps.CurrentStep.FailureComment = "...";
            core.UpdateReport(null, new List<bool> { true, true}, "z");
            // then
            Assert.AreEqual(true, core.Steps.CurrentStep.CheckedStates[0]);
            Assert.AreEqual(true, core.Steps.CurrentStep.CheckedStates[1]);
            Assert.AreEqual(null, core.Steps.CurrentStep.FailureComment);

            // when
            core.Steps.CurrentStep.FailureComment = "...";
            core.UpdateReport(Properties.Resources.Unchecked, new List<bool> { true, false }, "z");
            // then
            Assert.AreEqual(true, core.Steps.CurrentStep.CheckedStates[0]);
            Assert.AreEqual(false, core.Steps.CurrentStep.CheckedStates[1]);
            Assert.AreEqual(Properties.Resources.Unchecked, core.Steps.CurrentStep.FailureComment);

            // when
            core.Steps.CurrentStep.FailureComment = "...";
            core.UpdateReport(Properties.Resources.Unchecked, new List<bool> { true, true }, "z");
            // then
            Assert.AreEqual(true, core.Steps.CurrentStep.CheckedStates[0]);
            Assert.AreEqual(true, core.Steps.CurrentStep.CheckedStates[1]);
            Assert.AreEqual(null, core.Steps.CurrentStep.FailureComment);

            // when
            core.Steps.Move(1);
            core.UpdateReport("zz", new List<bool> { false, false }, "z");
            // then
            Assert.AreEqual(scenarioFilePath + "|1/", Properties.Settings.Default.LastSteps);

            // when
            int maxIndex = core.Steps.Count - 1;
            core.Steps.Move(maxIndex);
            core.UpdateReport("zz", new List<bool> { false, false }, "z");
            // then
            Assert.AreEqual(scenarioFilePath + "|" + maxIndex + "/", Properties.Settings.Default.LastSteps);

            // when
            core.Steps.Next();
            core.UpdateReport("zz", new List<bool> { false, false }, "z");
            // then
            Assert.AreEqual(scenarioFilePath + "|0/", Properties.Settings.Default.LastSteps);
        }

        private static string CreateScenarioFile(MainWindowCore core)
        {
            string scenarioFilePath = Path.Combine(core.WorkspacePath, "MainWindowCoreTest_01.txt");
            File.Copy(
                TestUtil.GetTestResourcePath(@"Core\MainWindowCoreTest_01.txt"), scenarioFilePath);
            Console.WriteLine(scenarioFilePath);
            return scenarioFilePath;
        }

        [TestMethod]
        public void TestTakeScreenShot()
        {
            // setup
            MainWindowCore core = TestUtil.CreateMainWindowCore(GetType().Name);
            string scenarioFilePath = CreateScenarioFile(core);
            core.ReadScenarioFile(scenarioFilePath, 0);
            string imagesDirectoryPath = core.ImagesDirectoryPath;
            Assert.IsFalse(Directory.Exists(imagesDirectoryPath));

            // when
            core.Steps.Next();
            core.TakeScreenShot();
            // then
            Assert.IsTrue(Directory.Exists(imagesDirectoryPath));
            Assert.IsTrue(File.Exists(Path.Combine(imagesDirectoryPath, "step-001.png")));

            // when
            core.Steps.Next();
            core.TakeScreenShot();
            // then
            Assert.IsTrue(File.Exists(Path.Combine(imagesDirectoryPath, "step-002.png")));

            // when
            Thread.Sleep(1000);
            core.ReadScenarioFile(scenarioFilePath, 0);
            // then
            Assert.IsFalse(Directory.Exists(imagesDirectoryPath));
        }

        [TestMethod]
        public void TestToValidIndex()
        {
            // setup
            List<Step> stepList = new List<Step>();
            stepList.Add(new Step(new Scenario(new Feature(null))) { Name = "0" });
            stepList.Add(new Step(new Scenario(new Feature(null))) { Name = "1" });
            Steps steps = new Steps(stepList);

            // expect
            Assert.AreEqual(0, MainWindowCore.ToValidIndex(0, steps));
            Assert.AreEqual(1, MainWindowCore.ToValidIndex(1, steps));
            Assert.AreEqual(1, MainWindowCore.ToValidIndex(2, steps));
        }
    }
}
