using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace XPFriend.Prompit.Core
{
    [TestClass]
    public class StepsTest
    {
        [TestMethod]
        public void TestCount()
        {
            // when
            List<Step> steps = new List<Step>(); 

            // then
            Assert.AreEqual(0, new Steps(steps).Count);

            // when
            steps.Add(new Step(null));

            // then
            Assert.AreEqual(1, new Steps(steps).Count);
        }

        [TestMethod]
        public void TestNextPrevious()
        {
            // setup
            List<Step> stepList = new List<Step>();
            stepList.Add(new Step(null) { Name = "0" });
            stepList.Add(new Step(null) { Name = "1" });

            // when
            Steps steps = new Steps(stepList);
            // then
            Assert.AreEqual(-1, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);

            // when
            Assert.IsFalse(steps.Previous());
            // then
            Assert.AreEqual(-1, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);

            // when
            Assert.IsTrue(steps.Next());
            // then
            Assert.AreEqual(0, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);

            // when
            Assert.IsTrue(steps.Next());
            // then
            Assert.AreEqual(1, steps.CurrentIndex);
            Assert.AreEqual("1", steps.CurrentStep.Name);

            // when
            Assert.IsFalse(steps.Next());
            // then
            Assert.AreEqual(2, steps.CurrentIndex);
            Assert.AreEqual("1", steps.CurrentStep.Name);

            // when
            Assert.IsFalse(steps.Next());
            // then
            Assert.AreEqual(2, steps.CurrentIndex);
            Assert.AreEqual("1", steps.CurrentStep.Name);

            // when
            Assert.IsTrue(steps.Previous());
            // then
            Assert.AreEqual(1, steps.CurrentIndex);
            Assert.AreEqual("1", steps.CurrentStep.Name);

            // when
            Assert.IsTrue(steps.Previous());
            // then
            Assert.AreEqual(0, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);

            // when
            Assert.IsFalse(steps.Previous());
            // then
            Assert.AreEqual(-1, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);

            // when
            Assert.IsFalse(steps.Previous());
            // then
            Assert.AreEqual(-1, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);
        }

        [TestMethod]
        public void TestMove()
        {
            // setup
            List<Step> stepList = new List<Step>();
            stepList.Add(new Step(null) { Name = "0" });
            stepList.Add(new Step(null) { Name = "1" });
            Steps steps = new Steps(stepList);

            // when
            Assert.IsFalse(steps.Move(-1));
            // then
            Assert.AreEqual(0, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);

            // when
            Assert.IsTrue(steps.Move(0));
            // then
            Assert.AreEqual(0, steps.CurrentIndex);
            Assert.AreEqual("0", steps.CurrentStep.Name);

            // when
            Assert.IsTrue(steps.Move(1));
            // then
            Assert.AreEqual(1, steps.CurrentIndex);
            Assert.AreEqual("1", steps.CurrentStep.Name);

            // when
            Assert.IsFalse(steps.Move(2));
            // then
            Assert.AreEqual(1, steps.CurrentIndex);
            Assert.AreEqual("1", steps.CurrentStep.Name);

            // when
            Assert.IsFalse(steps.Move(3));
            // then
            Assert.AreEqual(1, steps.CurrentIndex);
            Assert.AreEqual("1", steps.CurrentStep.Name);
        }

        [TestMethod]
        public void TestMoveWithEmptySteps()
        {
            // setup
            Steps steps = new Steps(new List<Step>());

            // when
            Assert.IsFalse(steps.Move(0));
            // then
            Assert.AreEqual(-1, steps.CurrentIndex);
        }

        [TestMethod]
        public void TestHasError()
        {
            // setup
            List<Step> stepList = new List<Step>();
            stepList.Add(new Step(null) { Name = "0" });
            stepList.Add(new Step(null) { Name = "1" });
            stepList.Add(new Step(null) { Name = "2" });
            Steps steps = new Steps(stepList);

            // expect
            Assert.IsFalse(steps.HasError);

            // when
            stepList[1].FailureComment = "x";
            // then
            Assert.IsFalse(steps.HasError);

            // when
            steps.Next();
            // then
            Assert.IsFalse(steps.HasError);

            // when
            steps.Next();
            // then
            Assert.IsFalse(steps.HasError);

            // when
            steps.Next();
            // then
            Assert.IsTrue(steps.HasError);

            // when
            steps.Previous();
            // then
            Assert.IsFalse(steps.HasError);

            // when
            steps.Next();
            // then
            Assert.IsTrue(steps.HasError);

            // when
            stepList[1].FailureComment = "";
            // then
            Assert.IsFalse(steps.HasError);

            // when
            stepList[2].ErrorMessage = "x";
            // then
            Assert.IsFalse(steps.HasError);

            // when
            steps.Next();
            // then
            Assert.IsTrue(steps.HasError);

            // when
            stepList[2].ErrorMessage = " ";
            // then
            Assert.IsFalse(steps.HasError);
        }

        [TestMethod]
        public void TestFindNextFailure()
        {
            // setup
            List<Step> stepList = new List<Step>();
            stepList.Add(new Step(null) { Name = "0" });
            stepList.Add(new Step(null) { Name = "1" });
            stepList.Add(new Step(null) { Name = "2" });
            stepList.Add(new Step(null) { Name = "3" });
            stepList.Add(new Step(null) { Name = "4" });
            stepList[2].FailureComment = "x";
            stepList[3].FailureComment = "x";
            Steps steps = new Steps(stepList);

            // expect
            Assert.AreEqual(2, steps.FindNextFailure());

            // when
            steps.Move(2);
            // then
            Assert.AreEqual(3, steps.FindNextFailure());

            // when
            steps.Move(3);
            // then
            Assert.AreEqual(-1, steps.FindNextFailure());
        }

        [TestMethod]
        public void TestFindPreviousFailure()
        {
            // setup
            List<Step> stepList = new List<Step>();
            stepList.Add(new Step(null) { Name = "0" });
            stepList.Add(new Step(null) { Name = "1" });
            stepList.Add(new Step(null) { Name = "2" });
            stepList.Add(new Step(null) { Name = "3" });
            stepList.Add(new Step(null) { Name = "4" });
            stepList[2].FailureComment = "x";
            stepList[3].FailureComment = "x";
            Steps steps = new Steps(stepList);

            // when
            steps.Move(4);
            // then
            Assert.AreEqual(3, steps.FindPreviousFailure());

            // when
            steps.Move(3);
            // then
            Assert.AreEqual(2, steps.FindPreviousFailure());

            // when
            steps.Move(2);
            // then
            Assert.AreEqual(-1, steps.FindPreviousFailure());
        }

        private Scenario CreateScenario()
        {
            return new Scenario(new Feature(new Release()));
        }
    }
}
