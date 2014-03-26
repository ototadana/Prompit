using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using XPFriend.Prompit.Core;
namespace XPFriend.Prompit.Properties {
    
    internal sealed partial class Settings {

        private Dictionary<string, int> lastSteps = new Dictionary<string, int>();
        
        public Settings() {
            if (this.UpgradeRequired)
            {
                this.Upgrade();
                this.UpgradeRequired = false;
            }
            RestoreLastSteps();
        }

        internal void RestoreLastSteps()
        {
            lastSteps.Clear();
            try
            {
                string[] parts = this.LastSteps.Split('|', '/');
                for(int i = 0; i < parts.Length; i++)
                {
                    lastSteps[parts[i]] = int.Parse(parts[++i]);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
        }

        internal string GetScenarioFileDirectory()
        {
            string filePath = this.LastScenarioFilePath;
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return MainWindowCore.GetDefaultDirectory();
            }
            return Path.GetDirectoryName(filePath);
        }

        internal void SaveScenarioFilePath(string fileName)
        {
            this.LastScenarioFilePath = fileName;
            this.Save();
        }

        internal void SaveLastStep(string fileName, int index)
        {
            lastSteps[fileName] = index;
            this.LastSteps = GetLastStepsAsText();
            this.Save();
        }

        private string GetLastStepsAsText()
        {
            StringBuilder text = new StringBuilder();
            foreach (string key in lastSteps.Keys)
            {
                text.Append(key).Append('|').Append(lastSteps[key]).Append('/');
            }
            return text.ToString();
        }

        internal int GetLastStepIndex(string scenarioFilePath)
        {
            int lastStep;
            if (this.lastSteps.TryGetValue(scenarioFilePath, out lastStep))
            {
                return lastStep;
            }
            return 0;
        }

        internal void SaveWindowPosition(Window window)
        {
            this.WindowLeft = window.Left;
            this.WindowTop = window.Top;
            this.WindowWidth = window.Width;
            this.WindowHeight = window.Height;
            this.Save();
        }

        internal void RestoreWindowPosition(Window window)
        {
            window.Width = GetValue(this.WindowWidth, 330);
            window.Height = GetValue(this.WindowHeight, 460.0);
            window.Left = GetValue(this.WindowLeft, SystemParameters.PrimaryScreenWidth - 410.0);
            window.Top = GetValue(this.WindowTop, 10.0);
        }

        private static double GetValue(double value, double defaultValue)
        {
            if (value < 0.0)
            {
                return defaultValue;
            }
            return value;
        }
    }
}
