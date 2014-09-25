using System.Collections.Generic;

namespace XPFriend.Prompit.Core
{
    internal class Scenario
    {
        private List<Step> steps = new List<Step>();
        private Feature feature;

        internal string Name { get; set; }
        internal string Description { get; set; }
        internal List<Step> Steps { get { return steps; } }
        internal Release Release { get { return feature.Release; } }
        internal Feature Feature { get { return feature; } }

        internal Scenario(Feature feature)
        {
            this.feature = feature;
        }

        internal void Add(Step step)
        {
            step.IndexInScenario = Steps.Count;
            Steps.Add(step);
        }
    }
}
