using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XPFriend.Prompit.Core
{
    [TestClass]
    public class HtmlParserTest
    {
        [TestMethod]
        public void TestMinimum()
        {
            // setup
            string html = MainWindowCore.ReadScenarioFileAsHtml(TestUtil.GetTestResourcePath(@"Core\HtmlParserTest_01_Minimum.txt"));

            // when
            HtmlParser htmlParser = new HtmlParser(html);
            Release release = htmlParser.Release;

            // then
            AssertMinimum(release);
            List<HtmlNode> steps = GetField<List<HtmlNode>>(htmlParser.HtmlReport, "steps");
            Assert.AreEqual("list-group-item", steps[0].GetAttributeValue("class", "x"));
            Assert.AreEqual("list-group", steps[0].ParentNode.GetAttributeValue("class", "x"));
        }

        private T GetField<T>(HtmlReport report, string name)
        {
            return (T)GetField(typeof(HtmlReport), name).GetValue(report);
        }

        private FieldInfo GetField(Type type, string name)
        {
            return type.GetField(name,
                BindingFlags.GetField | BindingFlags.SetField |
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        private static void AssertMinimum(Release release) 
        {
            Assert.AreEqual("", release.Name);
            Assert.AreEqual("", release.Description);
            Assert.AreEqual(1, release.Features.Count);
            Assert.AreEqual("", release.Features[0].Name);
            Assert.AreEqual("", release.Features[0].Description);
            Assert.AreEqual(1, release.Features[0].Scenarios.Count);
            Assert.AreEqual("シナリオ 1", release.Features[0].Scenarios[0].Name);
            Assert.AreEqual("", release.Features[0].Scenarios[0].Description);
            Assert.AreEqual(1, release.Features[0].Scenarios[0].Steps.Count);
            Assert.AreEqual("001. ステップ その(1)", release.Features[0].Scenarios[0].Steps[0].Name);
            Assert.AreEqual("001. ステップ その(1)", release.Features[0].Scenarios[0].Steps[0].Description);
            Assert.AreEqual(".シナリオ 1.ステップ その(1)", release.Features[0].Scenarios[0].Steps[0].Key);
            Assert.AreEqual(".シナリオ 1", release.Features[0].Scenarios[0].Steps[0].ParentName);

            Assert.AreEqual(1, release.Steps.Count);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Steps[0]);
            Assert.AreEqual(1, release.Features[0].Steps.ToArray().Length);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Features[0].Steps.ToArray()[0]);
            Assert.AreEqual(1, release.Features[0].Scenarios[0].Steps.Count);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Features[0].Scenarios[0].Steps[0]);

            Assert.AreSame(release, release.Features[0].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Steps[0].Release);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Steps[0].Feature);
            Assert.AreSame(release.Features[0].Scenarios[0], release.Features[0].Scenarios[0].Steps[0].Scenario);
        }

        [TestMethod]
        public void TestStandard()
        {
            // setup
            string html = MainWindowCore.ReadScenarioFileAsHtml(TestUtil.GetTestResourcePath(@"Core\HtmlParserTest_02_Standard.txt"));

            // when
            Release release = new HtmlParser(html).Release;

            // then
            AssertStandard(release);
        }

        private static void AssertStandard(Release release)
        {
            Assert.AreEqual("リリース", release.Name);
            Assert.AreEqual("<p>リリースの説明</p>", release.Description);
            Assert.AreEqual(1, release.Features.Count);
            Assert.AreEqual("フィーチャー", release.Features[0].Name);
            Assert.AreEqual("<p>フィーチャーの説明</p>", release.Features[0].Description);
            Assert.AreEqual(1, release.Features[0].Scenarios.Count);
            Assert.AreEqual("シナリオ", release.Features[0].Scenarios[0].Name);
            Assert.AreEqual("シナリオの説明", release.Features[0].Scenarios[0].Description);
            Assert.AreEqual(1, release.Features[0].Scenarios[0].Steps.Count);
            Assert.AreEqual("<p>001. ステップ</p>\n<p>ステップの説明</p>", release.Features[0].Scenarios[0].Steps[0].Description);
            Assert.AreEqual("001. ステップ", release.Features[0].Scenarios[0].Steps[0].Name);
            Assert.AreEqual("フィーチャー.シナリオ.ステップ", release.Features[0].Scenarios[0].Steps[0].Key);
            Assert.AreEqual("フィーチャー.シナリオ", release.Features[0].Scenarios[0].Steps[0].ParentName);

            Assert.AreEqual(1, release.Steps.Count);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Steps[0]);
            Assert.AreEqual(1, release.Features[0].Steps.ToArray().Length);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Features[0].Steps.ToArray()[0]);
            Assert.AreEqual(1, release.Features[0].Scenarios[0].Steps.Count);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Features[0].Scenarios[0].Steps[0]);

            Assert.AreSame(release, release.Features[0].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Steps[0].Release);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Steps[0].Feature);
            Assert.AreSame(release.Features[0].Scenarios[0], release.Features[0].Scenarios[0].Steps[0].Scenario);
        }

        [TestMethod]
        public void TestFull()
        {
            // setup
            string html = MainWindowCore.ReadScenarioFileAsHtml(TestUtil.GetTestResourcePath(@"Core\HtmlParserTest_03_Full.txt"));

            // when
            Release release = new HtmlParser(html).Release;

            // then
            AssertFull(release);
        }

        internal static void AssertFull(Release release)
        {
            Assert.AreEqual("リリース", release.Name);
            Assert.AreEqual("<p>リリースの説明</p>", release.Description);
            Assert.AreEqual(3, release.Features.Count);
            Assert.AreEqual("フィーチャー 1", release.Features[0].Name);
            Assert.AreEqual("", release.Features[0].Description);
            Assert.AreEqual(3, release.Features[0].Scenarios.Count);
            Assert.AreEqual("シナリオ 1", release.Features[0].Scenarios[0].Name);
            Assert.AreEqual("", release.Features[0].Scenarios[0].Description);
            Assert.AreEqual(3, release.Features[0].Scenarios[0].Steps.Count);
            Assert.AreEqual(
                "001. ステップ その(1)<ol><li>ステップ実施手順 1</li><li>ステップ実施手順 2 <a href=\"http://example.com/\" target=\"prompit\">http://example.com/</a></li></ol>", 
                release.Features[0].Scenarios[0].Steps[0].Description.Replace("\n", ""));
            Assert.AreEqual("001. ステップ その(1)", release.Features[0].Scenarios[0].Steps[0].Name);
            Assert.AreEqual("フィーチャー 1.シナリオ 1.ステップ その(1)", release.Features[0].Scenarios[0].Steps[0].Key);
            Assert.AreEqual("002. ステップ その(2)", release.Features[0].Scenarios[0].Steps[1].Description);
            Assert.AreEqual("002. ステップ その(2)", release.Features[0].Scenarios[0].Steps[1].Name);
            Assert.AreEqual("フィーチャー 1.シナリオ 1.ステップ その(2)", release.Features[0].Scenarios[0].Steps[1].Key);
            Assert.AreEqual(
                "003. ステップ その(3)<ul><li><input type='checkbox'>確認事項1</li><li><input type='checkbox'>確認事項2</li></ul>",
                release.Features[0].Scenarios[0].Steps[2].Description.Replace("\n", ""));
            Assert.AreEqual("003. ステップ その(3)", release.Features[0].Scenarios[0].Steps[2].Name);
            Assert.AreEqual("フィーチャー 1.シナリオ 1.ステップ その(3)", release.Features[0].Scenarios[0].Steps[2].Key);
            Assert.AreEqual("フィーチャー 1.シナリオ 1", release.Features[0].Scenarios[0].Steps[0].ParentName);
            Assert.AreEqual("フィーチャー 1.シナリオ 1", release.Features[0].Scenarios[0].Steps[1].ParentName);
            Assert.AreEqual("フィーチャー 1.シナリオ 1", release.Features[0].Scenarios[0].Steps[2].ParentName);

            Assert.AreEqual("シナリオ 2", release.Features[0].Scenarios[1].Name);
            Assert.AreEqual("", release.Features[0].Scenarios[1].Description);
            Assert.AreEqual(0, release.Features[0].Scenarios[1].Steps.Count);
            Assert.AreEqual("シナリオ 3", release.Features[0].Scenarios[2].Name);
            Assert.AreEqual("", release.Features[0].Scenarios[2].Description);
            Assert.AreEqual(1, release.Features[0].Scenarios[2].Steps.Count);
            Assert.AreEqual("001. ステップ その(1)", release.Features[0].Scenarios[2].Steps[0].Description);
            Assert.AreEqual("001. ステップ その(1)", release.Features[0].Scenarios[2].Steps[0].Name);
            Assert.AreEqual("フィーチャー 1.シナリオ 3.ステップ その(1)", release.Features[0].Scenarios[2].Steps[0].Key);
            Assert.AreEqual("フィーチャー 1.シナリオ 3", release.Features[0].Scenarios[2].Steps[0].ParentName);

            Assert.AreEqual("フィーチャー 2", release.Features[1].Name);
            Assert.AreEqual(
                "<p>フィーチャー 2の説明1</p><ul><li>詳細1</li><li>詳細2</li></ul><p>フィーチャー 2の説明2</p><ol><li>説明1</li><li>説明2</li></ol>",
                release.Features[1].Description.Replace("\n", ""));
            Assert.AreEqual(1, release.Features[1].Scenarios.Count);
            Assert.AreEqual("シナリオ 1", release.Features[1].Scenarios[0].Name);
            Assert.AreEqual("シナリオ 1の説明", release.Features[1].Scenarios[0].Description);
            Assert.AreEqual(1, release.Features[1].Scenarios[0].Steps.Count);
            Assert.AreEqual("001. ステップ その(1)", release.Features[1].Scenarios[0].Steps[0].Description);
            Assert.AreEqual("001. ステップ その(1)", release.Features[1].Scenarios[0].Steps[0].Name);
            Assert.AreEqual("フィーチャー 2.シナリオ 1.ステップ その(1)", release.Features[1].Scenarios[0].Steps[0].Key);
            Assert.AreEqual("フィーチャー 2.シナリオ 1", release.Features[1].Scenarios[0].Steps[0].ParentName);

            Assert.AreEqual("フィーチャー 3", release.Features[2].Name);
            Assert.AreEqual("<p>フィーチャー 3の説明</p>", release.Features[2].Description);
            Assert.AreEqual(0, release.Features[2].Scenarios.Count);

            Assert.AreEqual(5, release.Steps.Count);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Steps[0]);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[1], release.Steps[1]);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[2], release.Steps[2]);
            Assert.AreSame(release.Features[0].Scenarios[2].Steps[0], release.Steps[3]);
            Assert.AreSame(release.Features[1].Scenarios[0].Steps[0], release.Steps[4]);

            Assert.AreEqual(4, release.Features[0].Steps.ToArray().Length);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Features[0].Steps.ToArray()[0]);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[1], release.Features[0].Steps.ToArray()[1]);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[2], release.Features[0].Steps.ToArray()[2]);
            Assert.AreSame(release.Features[0].Scenarios[2].Steps[0], release.Features[0].Steps.ToArray()[3]);
            Assert.AreEqual(3, release.Features[0].Scenarios[0].Steps.Count);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[0], release.Features[0].Scenarios[0].Steps[0]);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[1], release.Features[0].Scenarios[0].Steps[1]);
            Assert.AreSame(release.Features[0].Scenarios[0].Steps[2], release.Features[0].Scenarios[0].Steps[2]);
            Assert.AreEqual(0, release.Features[0].Scenarios[1].Steps.Count);
            Assert.AreEqual(1, release.Features[0].Scenarios[2].Steps.Count);
            Assert.AreEqual(1, release.Features[1].Steps.ToArray().Length);
            Assert.AreSame(release.Features[1].Scenarios[0].Steps[0], release.Features[1].Steps.ToArray()[0]);
            Assert.AreEqual(1, release.Features[1].Scenarios[0].Steps.Count);
            Assert.AreSame(release.Features[1].Scenarios[0].Steps[0], release.Features[1].Scenarios[0].Steps[0]);

            Assert.AreSame(release, release.Features[0].Release);
            Assert.AreSame(release, release.Features[1].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[1].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[2].Release);
            Assert.AreSame(release, release.Features[1].Scenarios[0].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Steps[0].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Steps[1].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[0].Steps[2].Release);
            Assert.AreSame(release, release.Features[0].Scenarios[2].Steps[0].Release);
            Assert.AreSame(release, release.Features[1].Scenarios[0].Steps[0].Release);

            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[1].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[2].Feature);
            Assert.AreSame(release.Features[1], release.Features[1].Scenarios[0].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Steps[0].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Steps[1].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[0].Steps[2].Feature);
            Assert.AreSame(release.Features[0], release.Features[0].Scenarios[2].Steps[0].Feature);
            Assert.AreSame(release.Features[1], release.Features[1].Scenarios[0].Steps[0].Feature);

            Assert.AreSame(release.Features[0].Scenarios[0], release.Features[0].Scenarios[0].Steps[0].Scenario);
            Assert.AreSame(release.Features[0].Scenarios[0], release.Features[0].Scenarios[0].Steps[1].Scenario);
            Assert.AreSame(release.Features[0].Scenarios[0], release.Features[0].Scenarios[0].Steps[2].Scenario);
            Assert.AreSame(release.Features[0].Scenarios[2], release.Features[0].Scenarios[2].Steps[0].Scenario);
            Assert.AreSame(release.Features[1].Scenarios[0], release.Features[1].Scenarios[0].Steps[0].Scenario);
        }
    }
}
