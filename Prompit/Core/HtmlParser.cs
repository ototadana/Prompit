using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text;

namespace XPFriend.Prompit.Core
{
    internal class HtmlParser
    {
        private static readonly string[] TitleNodeNames = { "h1", "h2", "h3" };
        private static readonly string[] LineSeparator = { "\r", "\n"};

        private Release release;
        private HtmlReport htmlReport;

        internal Release Release { get { return release; } }
        internal HtmlReport HtmlReport { get { return htmlReport; } }

        internal HtmlParser(string html)
        {
            this.htmlReport = new HtmlReport(html);
            this.release = CreateRelease(this.htmlReport);
        }

        private Release CreateRelease(HtmlReport htmlReport)
        {
            HtmlNode htmlNode = htmlReport.HtmlDocument.DocumentNode;
            Release release = new Release();
            HtmlNode h1 = htmlNode.SelectSingleNode("/h1");
            if (h1 != null)
            {
                release.Name = Trim(h1.InnerText);
                release.Description = GetHtml(h1);
            }
            htmlReport.H1 = h1;

            AddFeatures(release, htmlNode);
            return release;
        }

        private void AddFeatures(Release release, HtmlNode htmlNode)
        {
            HtmlNodeCollection h2s = htmlNode.SelectNodes("/h2");
            if (h2s != null)
            {
                AddNamedFeatures(release, h2s);
            }
            else
            {
                AddAnonymousFeature(release, htmlNode);
            }
        }

        private void AddNamedFeatures(Release release, HtmlNodeCollection h2s)
        {
            foreach (HtmlNode h2 in h2s)
            {
                StringBuilder sb = new StringBuilder();
                HtmlNode node = FindNode(sb, h2);

                Feature feature = new Feature(release)
                {
                    Name = Trim(h2.InnerText),
                    Description = sb.ToString().Trim()
                };
                release.Features.Add(feature);

                if (node != null && node.Name == "h3")
                {
                    AddScenarios(feature, node);
                }
            }
        }

        private void AddAnonymousFeature(Release release, HtmlNode htmlNode)
        {
            HtmlNode h3 = htmlNode.SelectSingleNode("/h3");
            if (h3 != null)
            {
                Feature feature = new Feature(release) { Name = "", Description = "" };
                release.Features.Add(feature);
                AddScenarios(feature, h3);
            }
        }

        private void AddScenarios(Feature feature, HtmlNode node)
        {
            Scenario scenario = null;
            StringBuilder sb = null;
            do
            {
                if (node.Name == "h3")
                {
                    AddDescription(scenario, sb);
                    sb = new StringBuilder();
                    scenario = new Scenario(feature) { Name = Trim(node.InnerText.Replace(".", " ")) };
                    feature.Scenarios.Add(scenario);
                }
                else if (node.Name == "ol")
                {
                    string scenarioId = this.htmlReport.UpdateHtmlNodeAsScenario(node);
                    AddSteps(scenario, node, scenarioId);
                }
                else if (node.Name != "h1" && node.Name != "h2")
                {
                    sb.Append(Trim(node.InnerText));
                }
                else
                {
                    break;
                }
            } while ((node = node.NextSibling) != null);

            AddDescription(scenario, sb);
        }

        private void AddSteps(Scenario scenario, HtmlNode node, string scenarioId)
        {
            HtmlNodeCollection children = node.ChildNodes;
            int counter = 1;
            foreach (HtmlNode child in children)
            {
                if (child.Name == "li")
                {
                    string stepNumber = (counter++).ToString("000");
                    string label = stepNumber + ". ";
                    Step step = new Step(scenario);
                    AddCheckbox(child, step);
                    UpdateATag(child);
                    step.Name = label + Shorten(child);
                    step.Description = ToDescription(label, child);
                    scenario.Steps.Add(step);
                    this.htmlReport.UpdateHtmlNodeAsStep(child, scenarioId, stepNumber, step.Name);
                }
            }
        }

        private string ToDescription(string label, HtmlNode node)
        {
            HtmlNode labelNode = HtmlNode.CreateNode(label);
            HtmlNode firstChild = GetFirstChild(node);
            if (firstChild.Name == "p")
            {
                firstChild.PrependChild(labelNode);
            }
            else
            {
                node.PrependChild(labelNode);
            }
            return Trim(node.WriteContentTo());
        }

        private HtmlNode GetFirstChild(HtmlNode node)
        {
            foreach (HtmlNode child in node.ChildNodes)
            {
                if (child.NodeType != HtmlNodeType.Text)
                {
                    return child;
                }
                if (!string.IsNullOrWhiteSpace(child.InnerText))
                {
                    return child;
                }
            }
            return node;
        }

        private void AddCheckbox(HtmlNode node, Step step)
        {
            foreach (HtmlNode li in node.Descendants("li"))
            {
                if (li.ParentNode.Name == "ul")
                {
                    li.PrependChild(HtmlNode.CreateNode("<input type='checkbox'/>"));
                    step.CheckedStates.Add(false);
                }
            }
        }

        private void UpdateATag(HtmlNode node)
        {
            foreach (HtmlNode a in node.Descendants("a"))
            {
                a.SetAttributeValue("target", "prompit");
            }
        }

        private static string Shorten(HtmlNode node)
        {
            string text = node.InnerText;
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }
            return text.Split(LineSeparator, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        }

        private static void AddDescription(Scenario scenario, StringBuilder sb)
        {
            if (scenario != null)
            {
                scenario.Description = sb.ToString().Trim();
            }
        }

        private static string GetHtml(HtmlNode startNode)
        {
            StringBuilder sb = new StringBuilder();
            FindNode(sb, startNode);
            return sb.ToString().Trim();
        }

        private static HtmlNode FindNode(StringBuilder sb, HtmlNode startNode)
        {
            while ((startNode = startNode.NextSibling) != null)
            {
                if (TitleNodeNames.Contains(startNode.Name))
                {
                    return startNode;
                }
                sb.Append(startNode.OuterHtml);
            }
            return null;
        }

        private static string Trim(string text)
        {
            if (text == null)
            {
                return "";
            }
            return text.Trim();
        }
    }
}
