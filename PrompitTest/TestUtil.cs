using System.IO;
using XPFriend.Prompit.Core;

namespace XPFriend.Prompit
{
    internal class TestUtil
    {
        private const string topDirectory = @"..\..";
        private const string workingDirectory = @"..\test";

        static TestUtil()
        {
            if (!Directory.Exists(workingDirectory))
            {
                Directory.CreateDirectory(workingDirectory);
            }
        }

        internal static string GetApplicationData(string name)
        {
            string applicationData = Path.GetFullPath(Path.Combine(workingDirectory, name));
            if (Directory.Exists(applicationData))
            {
                Directory.Delete(applicationData, true);
            }
            return applicationData;
        }

        internal static string WorkingDirectory { get { return Path.GetFullPath(workingDirectory); } }

        internal static MainWindowCore CreateMainWindowCore(string name)
        {
            return new MainWindowCore(GetApplicationData(name));
        }

        internal static string GetTestResourcePath(string file)
        {
            return Path.Combine(topDirectory, file);
        }
    }
}
