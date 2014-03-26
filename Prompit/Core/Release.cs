using System.Collections.Generic;
using System.Linq;

namespace XPFriend.Prompit.Core
{
    internal class Release
    {
        private List<Feature> features = new List<Feature>();
        private List<Step> steps;

        internal string Name { get; set; }
        internal string Description { get; set; }
        internal List<Feature> Features { get { return features; } }

        internal List<Step> Steps
        {
            get
            {
                if (steps == null)
                {
                    steps = features.SelectMany(feature => feature.Steps).ToList();
                }
                return steps;
            }
        }

        internal Release()
        {
            Name = "";
            Description = "";
        }
    }
}
