using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace XPFriend.Prompit.Core
{
    internal class Step
    {
        private Scenario scenario;
        private long startTime = DateTime.Now.Ticks;
        private List<bool> checkedStates = new List<bool>();

        internal string Name { get; set; }
        internal string ParentName { get { return Feature.Name + "." + Scenario.Name; } }
        internal string Key { get { return CreateKey(ParentName, Name); } }
        internal string Description { get; set; }
        internal Release Release { get { return scenario.Release; } }
        internal Feature Feature { get { return scenario.Feature; } }
        internal Scenario Scenario { get { return scenario; } }

        internal List<bool> CheckedStates { get { return checkedStates; } }
        internal string FailureComment { get; set; }
        internal string ErrorMessage { get; set; }
        internal string ErrorType { get; set; }
        internal string ErrorText { get; set; }

        internal long Time { get; set; }
        internal string ImageFileName { get; set; }

        internal Step(Scenario scenario)
        {
            this.scenario = scenario;
        }

        internal static string CreateKey(string parentName, string name)
        {
            return parentName + "." + RemoveNumber(name);
        }

        private static string RemoveNumber(string name)
        {
            int separatorIndex = name.IndexOf(' ') + 1;
            return name.Substring(separatorIndex);
        }

        internal void Start()
        {
            this.startTime = DateTime.Now.Ticks;
        }

        internal void Stop()
        {
            this.Time += DateTime.Now.Ticks - this.startTime;
        }

        internal static string GetImageFileName(int index)
        {
            return "step-" + (index + 1).ToString("000") + ".png";
        }

        internal string GetCheckedStatesAsString()
        {
            StringBuilder text = new StringBuilder(CheckedStates.Count);
            foreach (bool state in CheckedStates)
            {
                text.Append((state) ? "+" : "-");
            }
            return text.ToString();
        }

        internal void UpdateCheckedStatesByString(string text)
        {
            if (text == null)
            {
                return;
            }

            if (text.Length != CheckedStates.Count)
            {
                return;
            }

            List<bool> states = new List<bool>(text.Length);
            foreach (char c in text)
            {
                if (c == '+')
                {
                    states.Add(true);
                }
                else if (c == '-')
                {
                    states.Add(false);
                }
                else
                {
                    return;
                }
            }
            this.checkedStates = states;
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(Description);
            UpdateCheckboxes(htmlDocument.DocumentNode);
            Description = htmlDocument.DocumentNode.WriteContentTo();
        }

        internal void UpdateCheckboxes(HtmlNode node)
        {
            int statesIndex = 0;
            foreach (HtmlNode child in node.Descendants("input"))
            {
                if (child.GetAttributeValue("type", "x") == "checkbox")
                {
                    if (checkedStates[statesIndex++])
                    {
                        child.SetAttributeValue("checked", "checked");
                    }
                    else
                    {
                        HtmlAttribute attribute = child.Attributes["checked"];
                        if (attribute != null)
                        {
                            attribute.Remove();
                        }
                    }
                }
            }
        }
    }
}
