using System.Collections.Generic;

namespace XPFriend.Prompit.Core
{
    internal class Steps
    {
        private List<Step> steps;
        private int index = -1;

        internal Steps(List<Step> steps)
        {
            this.steps = steps;
        }

        internal int Count { get { return steps.Count; } }
        internal int CurrentIndex { get { return this.index; } }
        internal Step CurrentStep 
        { 
            get 
            {
                if (this.index < 0)
                {
                    return this.steps[0];
                }
                if (this.index >= this.steps.Count)
                {
                    return this.steps[this.steps.Count - 1];
                }
                return this.steps[index]; 
            } 
        }

        internal bool HasError
        {
            get
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    if (i >= this.CurrentIndex)
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(steps[i].FailureComment))
                    {
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(steps[i].ErrorMessage))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        internal bool Next()
        {
            if (this.index >= this.steps.Count - 1)
            {
                this.index = this.steps.Count;
                return false;
            }
            this.index++;
            return true;
        }

        internal bool Move(int index)
        {
            if (index < 0)
            {
                this.index = 0;
                return false;
            }

            if (index > this.steps.Count - 1)
            {
                this.index = this.steps.Count - 1;
                return false;
            }

            this.index = index;
            return true;
        }

        internal bool Previous()
        {
            if (this.index <= 0)
            {
                this.index = -1;
                return false;
            }
            this.index--;
            return true;
        }

        internal int FindNextFailure()
        {
            for (int i = this.index + 1; i < this.steps.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(this.steps[i].FailureComment))
                {
                    return i;
                }
            }
            return -1;
        }

        internal int FindPreviousFailure()
        {
            for (int i = this.index - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(this.steps[i].FailureComment))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
