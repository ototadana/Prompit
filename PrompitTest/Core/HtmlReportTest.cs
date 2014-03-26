using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XPFriend.Prompit.Core.JUnit4;

namespace XPFriend.Prompit.Core
{
    [TestClass]
    public class HtmlReportTest
    {
        private const string prefix = @"<!DOCTYPE html>
<!-- saved from url=(0013)about:internet -->
<html>
<head>
<meta charset=""utf-8"">
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>st</title>
<link href=""css/bootstrap.min.css"" rel=""stylesheet"">
<link href=""css/bootstrap-theme.min.css"" rel=""stylesheet"">
<link href=""css/index.css"" rel=""stylesheet"">
<!--[if lt IE 9]>
<script src=""https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js""></script>
<script src=""https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js""></script>
<![endif]-->
</head>
<body>
<div class=""timestamp"">";

        private const string suffix = @"</div>
<h1>あ</h1>
<script src=""js/jquery.min.js""></script>
<script src=""js/bootstrap.min.js""></script>
<script>
$('h1').click(function(){
  $('.collapse.in').removeClass('in').css('height', '0px');
  var state = $(this).data('state');
  if(state == 0) {
    $(this).data('state', 1);
  } else if(state == 1) {
    $('.collapse').addClass('in').css('height', 'auto');
    $(this).data('state', 2);
  } else {
    $('.alert').parent('.collapse').addClass('in').css('height', 'auto');
    $(this).data('state', 0);
  }
}).data('state', 0);
</script>
</body>
</html>";



        [TestMethod]
        public void TestConstructor()
        {
            // when
            HtmlReport report = new HtmlReport("<h1>title</h1>");
            // then
            HtmlNode node = report.HtmlDocument.DocumentNode.FirstChild;
            Assert.AreEqual("h1", node.Name);
            Assert.AreEqual("title", node.InnerText);
        }

        [TestMethod]
        public void TestH1()
        {
            // setup
            HtmlReport report = new HtmlReport("<h1>title</h1>");
            HtmlNode node = report.HtmlDocument.DocumentNode.FirstChild;
            // when
            report.H1 = node;
            // then
            Assert.AreSame(node, report.H1);
        }

        [TestMethod]
        public void TestUpdateHtmlNodeAsScenario()
        {
            // setup
            HtmlReport report = new HtmlReport("<h1>title</h1>");
            HtmlNode node = report.HtmlDocument.DocumentNode.FirstChild;

            // when
            report.UpdateHtmlNodeAsScenario(node);
            // then
            Assert.AreEqual("list-group", node.GetAttributeValue("class", "x"));
            Assert.AreEqual("scenario-001", node.Id);

            //
            report.UpdateHtmlNodeAsScenario(node);
            // then
            Assert.AreEqual("scenario-002", node.Id);
        }

        [TestMethod]
        public void TestUpdateHtmlNodeAsStep()
        {
            // setup
            HtmlReport report = new HtmlReport("<h1>title</h1>");
            HtmlNode node = report.HtmlDocument.DocumentNode.FirstChild;

            // when
            report.UpdateHtmlNodeAsStep(node, "scenario-001", "002", "abc[*]de");
            // then
            Assert.AreEqual("list-group-item", node.GetAttributeValue("class", "x"));
            Assert.AreEqual("<a data-toggle=\"collapse\" data-parent=\"#scenario-001\" href=\"#scenario-001-002\"><span class=\"glyphicon glyphicon-ok\"></span> abcde <span></span></a>"+
                            "<span class=\"time\"></span>" +
                            "<div id=\"scenario-001-002\" class=\"collapse\"><pre></pre><pre></pre><div></div><div class=\"description\">title</div></div>",
                node.InnerHtml);
        }

        [TestMethod]
        public void TestUpdateByStep()
        {
            // setup
            HtmlReport report = new HtmlReport("<h1>title</h1>");
            HtmlNode node = report.HtmlDocument.DocumentNode.FirstChild;
            report.UpdateHtmlNodeAsStep(node, "scenario-001", "002", "abc[*]de");

            // when
            report.UpdateByStep(0, new Step(null));
            Assert.AreEqual("list-group-item", node.GetAttributeValue("class", "x"));
            Assert.AreEqual("<a data-toggle=\"collapse\" data-parent=\"#scenario-001\" href=\"#scenario-001-002\"><span class=\"glyphicon glyphicon-ok\"></span> abcde <span class=\"\"></span></a>" +
                            "<span class=\"time\">00:00</span>" +
                            "<div id=\"scenario-001-002\" class=\"collapse\"><pre class=\"hidden\"></pre><pre class=\"hidden\"></pre><div></div><div class=\"description\">title</div></div>",
                node.InnerHtml);

            {
                // when
                Step step = new Step(null)
                {
                    ImageFileName = "xx",
                    FailureComment = "aa",
                    ErrorMessage = "bb",
                    ErrorType = "cc",
                    ErrorText = "dd",
                    Time = 120000000L
                };
                report.UpdateByStep(0, step);
                // then
                Assert.AreEqual("list-group-item list-group-item-danger", node.GetAttributeValue("class", "x"));
                Assert.AreEqual("<a data-toggle=\"collapse\" data-parent=\"#scenario-001\" href=\"#scenario-001-002\"><span class=\"glyphicon glyphicon-exclamation-sign\"></span> abcde <span class=\"glyphicon glyphicon-camera\"></span></a>" +
                                "<span class=\"time\">00:12</span>" +
                                "<div id=\"scenario-001-002\" class=\"collapse in\"><pre class=\"alert alert-warning\">bb\r\ndd</pre><pre class=\"alert alert-warning\">aa</pre><div><img src=\"./images/xx\" class=\"img-responsive img-thumbnail\"></div><div class=\"description\">title</div></div>",
                    node.InnerHtml);
            }

            {
                // when
                Step step = new Step(null)
                {
                    ImageFileName = "xx",
                    ErrorMessage = "bb",
                    ErrorType = "cc",
                    ErrorText = "dd",
                    Time = 120000000L
                };
                report.UpdateByStep(0, step);
                // then
                Assert.AreEqual("list-group-item list-group-item-danger", node.GetAttributeValue("class", "x"));
                Assert.AreEqual("<a data-toggle=\"collapse\" data-parent=\"#scenario-001\" href=\"#scenario-001-002\"><span class=\"glyphicon glyphicon-exclamation-sign\"></span> abcde <span class=\"glyphicon glyphicon-camera\"></span></a>" +
                                "<span class=\"time\">00:12</span>" +
                                "<div id=\"scenario-001-002\" class=\"collapse in\"><pre class=\"alert alert-warning\">bb\r\ndd</pre><pre class=\"hidden\"></pre><div><img src=\"./images/xx\" class=\"img-responsive img-thumbnail\"></div><div class=\"description\">title</div></div>",
                    node.InnerHtml);
            }

            {
                // when
                Step step = new Step(null)
                {
                    ImageFileName = "xx",
                    FailureComment = "aa",
                    ErrorType = "cc",
                    ErrorText = "dd",
                    Time = 120000000L
                };
                report.UpdateByStep(0, step);
                // then
                Assert.AreEqual("list-group-item list-group-item-danger", node.GetAttributeValue("class", "x"));
                Assert.AreEqual("<a data-toggle=\"collapse\" data-parent=\"#scenario-001\" href=\"#scenario-001-002\"><span class=\"glyphicon glyphicon-exclamation-sign\"></span> abcde <span class=\"glyphicon glyphicon-camera\"></span></a>" +
                                "<span class=\"time\">00:12</span>" +
                                "<div id=\"scenario-001-002\" class=\"collapse in\"><pre class=\"hidden\"></pre><pre class=\"alert alert-warning\">aa</pre><div><img src=\"./images/xx\" class=\"img-responsive img-thumbnail\"></div><div class=\"description\">title</div></div>",
                    node.InnerHtml);
            }
        }

        [TestMethod]
        public void TestUpdateResultNode()
        {
            // setup
            HtmlReport report = new HtmlReport("<p>xxx<ul><li><input type='checkbox'>aaa</li><li><input type='checkbox'>bbb</li></ul></p>");
            HtmlNode node = report.HtmlDocument.DocumentNode.FirstChild;
            report.UpdateHtmlNodeAsStep(node, "scenario-001", "002", "abc[*]de");

            {
                // when
                Step step = new Step(null);
                step.CheckedStates.AddRange(new bool[] { true, true });
                report.UpdateByStep(0, step);
                // then
                List<HtmlNode> list = node.Descendants("input").ToList();
                Assert.AreEqual(2, list.Count);
                Assert.IsNotNull(list[0].Attributes["checked"]);
                Assert.IsNotNull(list[1].Attributes["checked"]);
            }
            {
                // when
                Step step = new Step(null);
                step.CheckedStates.AddRange(new bool[] { true, false });
                report.UpdateByStep(0, step);
                // then
                List<HtmlNode> list = node.Descendants("input").ToList();
                Assert.AreEqual(2, list.Count);
                Assert.IsNotNull(list[0].Attributes["checked"]);
                Assert.IsNull(list[1].Attributes["checked"]);
            }
            {
                // when
                Step step = new Step(null);
                step.CheckedStates.AddRange(new bool[] { false, false });
                report.UpdateByStep(0, step);
                // then
                List<HtmlNode> list = node.Descendants("input").ToList();
                Assert.AreEqual(2, list.Count);
                Assert.IsNull(list[0].Attributes["checked"]);
                Assert.IsNull(list[1].Attributes["checked"]);
            }
        }

        [TestMethod]
        public void TestUpdateByTestsuite()
        {
            // setup
            HtmlReport report = new HtmlReport("<h1>title</h1>");
            report.H1 = report.HtmlDocument.DocumentNode.FirstChild;

            // when
            report.UpdateByTestsuite(new testsuite());
            // then
            Assert.AreEqual("btn-danger", report.H1.GetAttributeValue("class", "x"));

            // when
            report.UpdateByTestsuite(new testsuite() { errors = "0", failures = "0" });
            // then
            Assert.AreEqual("btn-info", report.H1.GetAttributeValue("class", "x"));

            // when
            report.UpdateByTestsuite(new testsuite() { errors = "1", failures = "0" });
            // then
            Assert.AreEqual("btn-danger", report.H1.GetAttributeValue("class", "x"));

            // when
            report.UpdateByTestsuite(new testsuite() { errors = "1", failures = "1" });
            // then
            Assert.AreEqual("btn-danger", report.H1.GetAttributeValue("class", "x"));

            // when
            report.UpdateByTestsuite(new testsuite() { errors = "0", failures = "1" });
            // then
            Assert.AreEqual("btn-danger", report.H1.GetAttributeValue("class", "x"));

            // when
            report.H1 = null;
            report.UpdateByTestsuite(new testsuite());
            // then
            Assert.AreEqual(null, report.H1);
        }

        [TestMethod]
        public void TestWrite()
        {
            // setup
            MainWindowCore core = TestUtil.CreateMainWindowCore(GetType().Name);
            Assert.IsFalse(File.Exists(core.HtmlReportFilePath));
            HtmlReport report = new HtmlReport("<h1>あ</h1>");
            string html = core.HtmlReportFilePath;
            string dir = Path.GetDirectoryName(html);
            string css = Path.Combine(dir, "css");
            string fonts = Path.Combine(dir, "fonts");
            string js= Path.Combine(dir, "js");

            // when
            report.Write(html);
            // then
            Assert.IsTrue(File.Exists(html));
            string text = File.ReadAllText(html);
            Assert.IsTrue(text.StartsWith(prefix));
            Assert.IsTrue(text.EndsWith(suffix));
            Assert.IsTrue(text.Contains(DateTime.Now.ToShortDateString()));

            Assert.IsTrue(Directory.Exists(css));
            Assert.AreEqual(3, Directory.GetFiles(css).Length);
            Assert.IsTrue(File.Exists(Path.Combine(css, "bootstrap-theme.min.css")));
            Assert.IsTrue(File.Exists(Path.Combine(css, "bootstrap.min.css")));
            Assert.IsTrue(File.Exists(Path.Combine(css, "index.css")));

            Assert.IsTrue(Directory.Exists(fonts));
            Assert.AreEqual(4, Directory.GetFiles(fonts).Length);
            Assert.IsTrue(File.Exists(Path.Combine(fonts, "glyphicons-halflings-regular.eot")));
            Assert.IsTrue(File.Exists(Path.Combine(fonts, "glyphicons-halflings-regular.svg")));
            Assert.IsTrue(File.Exists(Path.Combine(fonts, "glyphicons-halflings-regular.ttf")));
            Assert.IsTrue(File.Exists(Path.Combine(fonts, "glyphicons-halflings-regular.woff")));

            Assert.IsTrue(Directory.Exists(js));
            Assert.AreEqual(2, Directory.GetFiles(js).Length);
            Assert.IsTrue(File.Exists(Path.Combine(js, "bootstrap.min.js")));
            Assert.IsTrue(File.Exists(Path.Combine(js, "jquery.min.js")));
        }
    }
}
