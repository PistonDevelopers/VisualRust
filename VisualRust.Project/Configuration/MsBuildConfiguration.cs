using System;
using System.Globalization;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using VisualRust.Project.Configuration.MsBuild;

namespace VisualRust.Project.Configuration
{
    // This is more or less ripped form ProjectConfig
    class MsBuildConfiguration
    {
        private const string configString = "'$(Configuration)|$(Platform)' == '{0}|{1}'";

        private readonly UserProjectConfig proj;
        private readonly string config;
        private readonly string platform;
        private ProjectInstance snapshot;

        public MsBuildConfiguration(UserProjectConfig proj, string config, string platform)
        {
            this.platform = platform;
            this.config = config;
            this.proj = proj;
        }

        public void SetConfigurationProperty(string propertyName, string propertyValue)
        {
            string condition = String.Format(CultureInfo.InvariantCulture, configString, this.config, this.platform);
            SetPropertyUnderCondition(propertyName, propertyValue, condition);
            snapshot = null;
        }

        public virtual string GetConfigurationProperty(string propertyName, bool resetCache)
        {
            if (snapshot == null)
                snapshot = proj.CreateProjectInstance(config, platform);
            ProjectPropertyInstance property = snapshot.GetProperty(propertyName);
            if (property == null)
                return null;
            return property.EvaluatedValue;
        }

        private void SetPropertyUnderCondition(string propertyName, string propertyValue, string condition)
        {
            // New OM doesn't have a convenient equivalent for setting a property with a particular property group condition. 
            // So do it ourselves.
            ProjectPropertyGroupElement newGroup = null;

            foreach (ProjectPropertyGroupElement group in this.proj.Xml.PropertyGroups)
            {
                if (String.Equals(group.Condition.Trim(), condition, StringComparison.OrdinalIgnoreCase))
                {
                    newGroup = group;
                    break;
                }
            }

            if (newGroup == null)
            {
                newGroup = this.proj.Xml.AddPropertyGroup(); // Adds after last existing PG, else at start of project
                newGroup.Condition = condition;
            }

            foreach (ProjectPropertyElement property in newGroup.PropertiesReversed) // If there's dupes, pick the last one so we win
            {
                if (String.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase) && property.Condition.Length == 0)
                {
                    property.Value = propertyValue;
                    return;
                }
            }

            newGroup.AddProperty(propertyName, propertyValue);
        }
    }
}
