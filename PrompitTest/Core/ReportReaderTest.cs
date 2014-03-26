using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using XPFriend.Prompit.Core.JUnit4;

namespace XPFriend.Prompit.Core
{
    [TestClass]
    public class ReportReaderTest
    {
        [TestMethod]
        public void TestReadXmlWhenNoFile()
        {
            // setup
            ReportReader reader = new ReportReader();

            // when
            Dictionary<string, testcase> dictionary = reader.ReadXml("xxx", 0);

            // then
            Assert.AreEqual(0, dictionary.Count);
        }

        [TestMethod]
        public void TestReadXml()
        {
            // setup
            ReportReader reader = new ReportReader();
            string xml = TestUtil.GetTestResourcePath("Core\\ReportReaderTest.xml");
            {
                // when
                Dictionary<string, testcase> dictionary = reader.ReadXml(xml, 3);
                // then
                Assert.AreEqual(2, dictionary.Count);
                Assert.AreEqual("001. STEP-01", dictionary["Release-01.Feature-01.STEP-01"].name);
                Assert.AreEqual("002. STEP-02", dictionary["Release-01.Feature-01.STEP-02"].name);
            }
            {
                // when
                Dictionary<string, testcase> dictionary = reader.ReadXml(xml, 2);
                // then
                Assert.AreEqual(2, dictionary.Count);
                Assert.AreEqual("001. STEP-01", dictionary["Release-01.Feature-01.STEP-01"].name);
                Assert.AreEqual("002. STEP-02", dictionary["Release-01.Feature-01.STEP-02"].name);
            }
            {
                // when
                Dictionary<string, testcase> dictionary = reader.ReadXml(xml, 1);
                // then
                Assert.AreEqual(1, dictionary.Count);
                Assert.AreEqual("001. STEP-01", dictionary["Release-01.Feature-01.STEP-01"].name);
            }
        }
    }
}
