using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace XPFriend.Prompit.Core
{
    [TestClass]
    public class StepTest
    {
        [TestMethod]
        public void TestProperties()
        {
            // when
            Step step = new Step(null)
            {
                FailureComment = "aa",
                ErrorMessage = "bb",
                ErrorType = "cc",
                ErrorText = "dd",
                Time = 12L
            };

            // then
            Assert.AreEqual("aa", step.FailureComment);
            Assert.AreEqual("bb", step.ErrorMessage);
            Assert.AreEqual("cc", step.ErrorType);
            Assert.AreEqual("dd", step.ErrorText);
            Assert.AreEqual(12L, step.Time);
        }

        [TestMethod]
        public void TestStartStop()
        {
            // setup
            Step step = new Step(null);
            // when
            step.Start();
            Thread.Sleep(1100);
            step.Stop();
            Assert.AreEqual("01", TimeSpan.FromTicks(step.Time).ToString("ss"));
            Console.WriteLine(step.Time);
        }

        [TestMethod]
        public void TestGetImageFileName()
        {
            // expect
            Assert.AreEqual("step-001.png", Step.GetImageFileName(0));
            Assert.AreEqual("step-002.png", Step.GetImageFileName(1));
            Assert.AreEqual("step-999.png", Step.GetImageFileName(998));
            Assert.AreEqual("step-1000.png", Step.GetImageFileName(999));
        }

        [TestMethod]
        public void TestGetCheckedStatesAsString()
        {
            {
                // when
                Step step = new Step(null);
                // then
                Assert.AreEqual("", step.GetCheckedStatesAsString());
            }
            {
                // when
                Step step = new Step(null);
                step.CheckedStates.AddRange(new bool[] { true, false, false, true });
                // then
                Assert.AreEqual("+--+", step.GetCheckedStatesAsString());
            }
            {
                // when
                Step step = new Step(null);
                step.CheckedStates.AddRange(new bool[] { true, true, false});
                // then
                Assert.AreEqual("++-", step.GetCheckedStatesAsString());
            }
        }

        [TestMethod]
        public void TestUpdateCheckboxes()
        {
            // setup
            Step step = new Step(null);
            HtmlNode node = HtmlNode.CreateNode("<div><input type='checkbox'><input type='checkbox'><input type='checkbox'></div>");
            List<HtmlNode> c = node.Descendants("input").ToList();
            Assert.AreEqual(null, c[0].Attributes["checked"]);
            Assert.AreEqual(null, c[0].Attributes["checked"]);
            Assert.AreEqual(null, c[0].Attributes["checked"]);

            // when
            step.CheckedStates.AddRange(new bool[] { true, true, true });
            step.UpdateCheckboxes(node);
            // then
            Assert.AreEqual("checked", c[0].Attributes["checked"].Value);
            Assert.AreEqual("checked", c[0].Attributes["checked"].Value);
            Assert.AreEqual("checked", c[0].Attributes["checked"].Value);

            // when
            step.CheckedStates.Clear();
            step.CheckedStates.AddRange(new bool[] { false, false, false});
            step.UpdateCheckboxes(node);
            // then
            Assert.AreEqual(null, c[0].Attributes["checked"]);
            Assert.AreEqual(null, c[0].Attributes["checked"]);
            Assert.AreEqual(null, c[0].Attributes["checked"]);
        }

        [TestMethod]
        public void TestUpdateCheckedStatesByString()
        {
            // setup
            Step step = new Step(null);
            step.CheckedStates.AddRange(new bool[] { false, true });
            step.Description = "<div><input type='checkbox'><input type='checkbox'></div>";
            AssertAsInitial(step);

            // when
            step.UpdateCheckedStatesByString(null);
            // then
            AssertAsInitial(step);

            // when
            step.UpdateCheckedStatesByString("+");
            // then
            AssertAsInitial(step);

            // when
            step.UpdateCheckedStatesByString("+++");
            // then
            AssertAsInitial(step);

            // when
            step.UpdateCheckedStatesByString("+x");
            // then
            AssertAsInitial(step);

            // when
            step.UpdateCheckedStatesByString("+-");
            // then
            Assert.AreEqual(true, step.CheckedStates[0]);
            Assert.AreEqual(false, step.CheckedStates[1]);
            Assert.AreEqual("<div><input type='checkbox' checked=\"checked\"><input type='checkbox'></div>", step.Description);

            // when
            step.UpdateCheckedStatesByString("-+");
            // then
            Assert.AreEqual(false, step.CheckedStates[0]);
            Assert.AreEqual(true, step.CheckedStates[1]);
            Assert.AreEqual("<div><input type='checkbox'><input type='checkbox' checked=\"checked\"></div>", step.Description);
        }

        private static void AssertAsInitial(Step step)
        {
            Assert.AreEqual(false, step.CheckedStates[0]);
            Assert.AreEqual(true, step.CheckedStates[1]);
            Assert.AreEqual("<div><input type='checkbox'><input type='checkbox'></div>", step.Description);
        }
    }
}
