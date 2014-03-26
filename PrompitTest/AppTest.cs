using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace XPFriend.Prompit
{
    [TestClass]
    public class AppTest
    {
        [TestInitialize]
        public void Setup()
        {
            string dir = GetLogDirectory();
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
        }

        [TestMethod]
        public void TestHandleException()
        {
            // setup
            App app = GetApp();
            MainWindow window = new MainWindow();
            app.MainWindow = window;

            // when
            app.HandleException(new Exception("exception1", new Exception("exception2")));

            // then
            string errorLogFolder = GetLogDirectory();
            Assert.IsTrue(Directory.Exists(errorLogFolder));
            string[] logFiles = Directory.GetFiles(errorLogFolder);
            Assert.AreEqual(1, logFiles.Length);
            string logFile = logFiles[0];

            // assert file name
            Assert.IsTrue(Path.GetFileName(logFile).StartsWith("error_" + DateTime.Now.ToString("yyyy-MM-dd_HH")));
            Assert.IsTrue(logFile.EndsWith(".txt"));

            // assert file contents
            string logText = File.ReadAllText(logFile);
            Assert.IsTrue(logText.Contains("exception1"));
            Assert.IsTrue(logText.Contains("exception2"));
            Assert.IsTrue(logText.Contains(" ---> "));
            Assert.IsTrue(logText.Contains(logFile));
        }

        private static string GetLogDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "XPFriend\\Prompit\\Logs");
        }

        private static App GetApp()
        {
            if (App.Current != null)
            {
                return (App)App.Current;
            }
            return new App();
        }
    }
}
