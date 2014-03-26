using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows;
using XPFriend.Prompit.Core;
using XPFriend.Prompit.Properties;

namespace XPFriend.Prompit
{
    [TestClass]
    public class SettingsTest
    {
        private MainWindowCore workspace;

        [TestInitialize]
        public void Setup()
        {
            new Settings().Reset();
            this.workspace = TestUtil.CreateMainWindowCore(GetType().Name);
        }

        [TestMethod]
        public void TestGetScenarioFileDirectory()
        {
            // setup
            Settings settings = new Settings();

            // expect
            Assert.AreEqual(MainWindowCore.GetDefaultDirectory(), settings.GetScenarioFileDirectory());
            
            // when
            settings.LastScenarioFilePath = @"c:\temp\test1.txt";
            // then
            Assert.AreEqual(@"c:\temp", settings.GetScenarioFileDirectory());
        }

        [TestMethod]
        public void TestSaveScenarioFilePath()
        {
            // when
            new Settings().SaveScenarioFilePath(@"c:\temp\test2.txt");

            // then
            Assert.AreEqual(@"c:\temp\test2.txt", new Settings().LastScenarioFilePath);
        }

        [TestMethod]
        public void TestGetLastStepIndex()
        {
            // setup
            Settings settings = new Settings();

            // when
            settings.LastSteps = null;
            settings.RestoreLastSteps();
            // then
            Assert.AreEqual(0, settings.GetLastStepIndex(@"c:\temp\test4.txt"));

            // when
            settings.SaveLastStep(@"c:\temp\test3.txt", 1);
            settings.RestoreLastSteps();
            // then
            Assert.AreEqual(@"c:\temp\test3.txt|1/", settings.LastSteps);
            Assert.AreEqual(1, settings.GetLastStepIndex(@"c:\temp\test3.txt"));
            Assert.AreEqual(0, settings.GetLastStepIndex(@"c:\temp\test4.txt"));

            // when
            settings.SaveLastStep(@"c:\temp\test4.txt", 2);
            settings.RestoreLastSteps();
            // then
            Assert.AreEqual(@"c:\temp\test3.txt|1/c:\temp\test4.txt|2/", settings.LastSteps);
            Assert.AreEqual(1, settings.GetLastStepIndex(@"c:\temp\test3.txt"));
            Assert.AreEqual(2, settings.GetLastStepIndex(@"c:\temp\test4.txt"));
        }

        [TestMethod]
        public void TestSaveWindowPosition()
        {
            // setup
            MainWindow window = new MainWindow();

            // when
            window.Top = 1;
            window.Left = 2;
            window.Width = 200;
            window.Height = 300;
            new Settings().SaveWindowPosition(window);

            // then
            Settings settings = new Settings();
            Assert.AreEqual(1.0, settings.WindowTop);
            Assert.AreEqual(2.0, settings.WindowLeft);
            Assert.AreEqual(200.0, settings.WindowWidth);
            Assert.AreEqual(300.0, settings.WindowHeight);
        }

        [TestMethod]
        public void TestRestoreWindowPosition()
        {
            // setup
            MainWindow window = new MainWindow();

            // when
            new Settings().Reset();
            new Settings().RestoreWindowPosition(window);
            // then
            Assert.AreEqual(330.0, window.Width);
            Assert.AreEqual(460.0, window.Height);
            Assert.AreEqual(SystemParameters.PrimaryScreenWidth - 410.0, window.Left);
            Assert.AreEqual(10.0, window.Top);

            // when
            Settings settings = new Settings();
            settings.WindowTop = 1;
            settings.WindowLeft = 2;
            settings.WindowWidth = 200;
            settings.WindowHeight = 300;
            settings.RestoreWindowPosition(window);
            //then
            Assert.AreEqual(1.0, window.Top);
            Assert.AreEqual(2.0, window.Left);
            Assert.AreEqual(200.0, window.Width);
            Assert.AreEqual(300.0, window.Height);
        }
    }
}
