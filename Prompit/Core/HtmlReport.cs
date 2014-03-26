using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using XPFriend.Prompit.Core.JUnit4;
using XPFriend.Prompit.Properties;

namespace XPFriend.Prompit.Core
{
    internal class HtmlReport
    {
        private const string HtmlTemplate =
@"<!DOCTYPE html>
<!-- saved from url=(0013)about:internet -->
<html>
<head>
<meta charset=""utf-8"">
<meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>{0}</title>
<link href=""css/bootstrap.min.css"" rel=""stylesheet"">
<link href=""css/bootstrap-theme.min.css"" rel=""stylesheet"">
<link href=""css/index.css"" rel=""stylesheet"">
<!--[if lt IE 9]>
<script src=""https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js""></script>
<script src=""https://oss.maxcdn.com/libs/respond.js/1.4.2/respond.min.js""></script>
<![endif]-->
</head>
<body>
<div class=""timestamp"">{1}</div>
{2}
<script src=""js/jquery.min.js""></script>
<script src=""js/bootstrap.min.js""></script>
<script>
$('h1').click(function(){{
  $('.collapse.in').removeClass('in').css('height', '0px');
  var state = $(this).data('state');
  if(state == 0) {{
    $(this).data('state', 1);
  }} else if(state == 1) {{
    $('.collapse').addClass('in').css('height', 'auto');
    $(this).data('state', 2);
  }} else {{
    $('.alert').parent('.collapse').addClass('in').css('height', 'auto');
    $(this).data('state', 0);
  }}
}}).data('state', 0);
</script>
</body>
</html>";

        private int scenarioNumber = 1;

        private HtmlDocument htmlDocument;
        private List<HtmlNode> steps = new List<HtmlNode>();

        internal HtmlDocument HtmlDocument { get { return htmlDocument; } }
        internal HtmlNode H1 { get; set; }

        public HtmlReport(string html)
        {
            this.htmlDocument = ToHtmlDocument(html);
        }

        private static HtmlDocument ToHtmlDocument(string html)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            return htmlDocument;
        }

        internal string UpdateHtmlNodeAsScenario(HtmlNode node)
        {
            node.SetAttributeValue("class", "list-group");
            string scenarioId = "scenario-" + (scenarioNumber++).ToString("000");
            node.SetAttributeValue("id", scenarioId);
            return scenarioId;
        }

        internal void UpdateHtmlNodeAsStep(HtmlNode node, string scenarioId, string stepNumber, string stepName)
        {
            string stepId = scenarioId + "-" + stepNumber;
            node.SetAttributeValue("class", "list-group-item");
            string description = node.InnerHtml;
            node.RemoveAllChildren();
            node.AppendChild(HtmlNode.CreateNode(string.Format(
                "<a data-toggle=\"collapse\" data-parent=\"#{0}\" href=\"#{1}\"><span class=\"glyphicon glyphicon-ok\"></span> {2} <span></span></a>",
                scenarioId, stepId, stepName.Replace("[*]", ""))));
            node.AppendChild(HtmlNode.CreateNode("<span class=\"time\"></span>"));
            node.AppendChild(HtmlNode.CreateNode(string.Format(
                "<div id=\"{0}\" class=\"collapse\"><pre></pre><pre></pre><div></div><div class=\"description\">{1}</div></div>",
                stepId, description)));
            steps.Add(node);
        }

        internal void Write(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            CreateSupportFiles(directory);
            string html = string.Format(HtmlTemplate, 
                Path.GetFileName(directory),
                DateTime.Now.ToString(),
                this.HtmlDocument.DocumentNode.WriteContentTo());
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.Write(html);
                writer.Flush();
            }
        }

        private void CreateSupportFiles(string directory)
        {
            string css = Path.Combine(directory, "css");
            if (CreateDirectory(css))
            {
                File.WriteAllBytes(Path.Combine(css, "bootstrap-theme.min.css"), Resources.bootstrap_theme_min_css);
                File.WriteAllBytes(Path.Combine(css, "bootstrap.min.css"), Resources.bootstrap_min_css);
                File.WriteAllBytes(Path.Combine(css, "index.css"), Resources.index_css);
            }

            string fonts = Path.Combine(directory, "fonts");
            if (CreateDirectory(fonts))
            {
                File.WriteAllBytes(Path.Combine(fonts, "glyphicons-halflings-regular.eot"), Resources.glyphicons_halflings_regular_eot);
                File.WriteAllBytes(Path.Combine(fonts, "glyphicons-halflings-regular.svg"), Resources.glyphicons_halflings_regular_svg);
                File.WriteAllBytes(Path.Combine(fonts, "glyphicons-halflings-regular.ttf"), Resources.glyphicons_halflings_regular_ttf);
                File.WriteAllBytes(Path.Combine(fonts, "glyphicons-halflings-regular.woff"), Resources.glyphicons_halflings_regular_woff);
            }

            string js = Path.Combine(directory, "js");
            if (CreateDirectory(js))
            {
                File.WriteAllBytes(Path.Combine(js, "bootstrap.min.js"), Resources.bootstrap_min_js);
                File.WriteAllBytes(Path.Combine(js, "jquery.min.js"), Resources.jquery_min_js);
            }
        }

        private bool CreateDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                return false;
            }
            Directory.CreateDirectory(directory);
            return true;
        }

        internal void UpdateByTestsuite(testsuite suite)
        {
            if (this.H1 == null)
            {
                return;
            }
            string h1Class = "btn-info";
            if (suite.errors != "0" || suite.failures != "0")
            {
                h1Class = "btn-danger";
            }
            this.H1.SetAttributeValue("class", h1Class);
        }

        internal void UpdateByStep(int index, Step step)
        {
            HtmlNode stepNode = this.steps[index];
            HtmlNode resultNode = stepNode.LastChild;
            bool hasImage = UpdateImageNode(step, resultNode);
            bool hasError = UpdateErrorNode(step, resultNode);
            bool hasFailure = UpdateFailureNode(step, resultNode);
            bool hasErrorOrFailure = hasError || hasFailure;
            UpdateResultNode(step, resultNode, hasErrorOrFailure);

            stepNode.FirstChild.FirstChild.SetAttributeValue("class", 
                hasErrorOrFailure ? "glyphicon glyphicon-exclamation-sign" : "glyphicon glyphicon-ok");
            stepNode.SetAttributeValue("class", 
                hasErrorOrFailure ? "list-group-item list-group-item-danger" : "list-group-item");
            UpdateTimeNode(step, stepNode);
            UpdateTitleNode(step, stepNode, hasImage);
        }

        private void UpdateResultNode(Step step, HtmlNode resultNode, bool hasErrorOrFailure)
        {
            resultNode.SetAttributeValue("class", hasErrorOrFailure ? "collapse in" : "collapse");
            step.UpdateCheckboxes(resultNode);
        }

        private bool UpdateImageNode(Step step, HtmlNode resultNode)
        {
            HtmlNode node = resultNode.ChildNodes[2];
            node.RemoveAllChildren();
            if (step.ImageFileName != null)
            {
                node.AppendChild(HtmlNode.CreateNode(string.Format(
                    "<img src=\"./images/{0}\" class=\"img-responsive img-thumbnail\"/>",
                    step.ImageFileName)));
                return true;
            }
            return false;
        }

        private bool UpdateFailureNode(Step step, HtmlNode resultNode)
        {
            HtmlNode node = resultNode.ChildNodes[1];
            return UpdateMessageNode(node, ToString(step.FailureComment, null));
        }

        private bool UpdateErrorNode(Step step, HtmlNode resultNode)
        {
            HtmlNode node = resultNode.ChildNodes[0];
            return UpdateMessageNode(node, ToString(step.ErrorMessage, step.ErrorText));
        }

        private static bool UpdateMessageNode(HtmlNode node, string message)
        {
            node.InnerHtml = message;
            bool hasMessage = !string.IsNullOrWhiteSpace(message);
            node.SetAttributeValue("class", hasMessage ? "alert alert-warning" : "hidden");
            return hasMessage;
        }

        private string ToString(string message, string text)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return HttpUtility.HtmlEncode(message.Trim());
            }
            return message.Trim() + "\r\n" + text.Trim();
        }

        private static void UpdateTimeNode(Step step, HtmlNode stepNode)
        {
            HtmlNode timeNode = stepNode.ChildNodes[1];
            TimeSpan timeSpan = TimeSpan.FromTicks(step.Time);
            timeNode.InnerHtml = Util.Format(timeSpan);
        }

        private static void UpdateTitleNode(Step step, HtmlNode stepNode, bool hasImage)
        {
            HtmlNode titleNode = stepNode.ChildNodes[0];
            titleNode.LastChild.SetAttributeValue("class", hasImage ? "glyphicon glyphicon-camera" : "");
        }

    }
}
