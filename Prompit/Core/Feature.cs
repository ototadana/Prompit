using System.Collections.Generic;
using System.Linq;

namespace XPFriend.Prompit.Core
{
    internal class Feature
    {
        private List<Scenario> scenarios = new List<Scenario>();
        private Release release;

        internal string Name { get; set; }
        internal string Description { get; set; }
        internal List<Scenario> Scenarios { get { return scenarios; } }
        internal Release Release { get { return release; } }

        internal IEnumerable<Step> Steps 
        {
            get
            {
                return scenarios.SelectMany(scenario => scenario.Steps);
            }
        }

        internal Feature(Release release)
        {
            this.release = release;
        }
    }
}
