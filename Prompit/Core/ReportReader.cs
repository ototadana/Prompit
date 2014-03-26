using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using XPFriend.Prompit.Core.JUnit4;

namespace XPFriend.Prompit.Core
{
    internal class ReportReader
    {
        internal Dictionary<string, testcase> ReadXml(string filePath, int index)
        {
            if (!File.Exists(filePath))
            {
                return new Dictionary<string, testcase>();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(testsuite));
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                testsuite suite = (testsuite)serializer.Deserialize(stream);
                return CreateDictionary(suite, index);
            }
        }

        private Dictionary<string, testcase> CreateDictionary(testsuite suite, int index)
        {
            Dictionary<string, testcase> dictionary = new Dictionary<string, testcase>();
            for (int i = 0; i < suite.testcase.Length && i < index; i++)
            {
                testcase tcase = suite.testcase[i];
                string key = Step.CreateKey(tcase.classname, tcase.name);
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, tcase);
                }
            }
            return dictionary;
        }
    }
}
