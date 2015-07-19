using System;
using System.IO;
using Microsoft.Build.Exceptions;
using Microsoft.VisualStudioTools.Project;

namespace VisualRust.Project.Configuration.MsBuild
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;

    class UserProjectConfig
    {
        private const string emptyUserConfig =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""12.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
</Project>";

        private readonly ProjectNode proj;
        private Project msBuildProj;

        public ProjectRootElement Xml { get { return msBuildProj.Xml; } }

        public UserProjectConfig(ProjectNode node)
        {
            string userConfigPath = node.Url + ".user";
            try
            {
                LoadMsBuildProject(userConfigPath);
            }
            catch (InvalidProjectFileException)
            {
                File.WriteAllText(userConfigPath, emptyUserConfig);
                LoadMsBuildProject(userConfigPath);
            }
            this.proj = node;
        }

        private void LoadMsBuildProject(string userConfigPath)
        {
            this.msBuildProj = new Project(userConfigPath);
        }

        public ProjectInstance CreateProjectInstance(string config, string platform)
        {
            msBuildProj.SetGlobalProperty(ProjectFileConstants.Configuration, config);
            msBuildProj.SetGlobalProperty(ProjectFileConstants.Platform, platform);
            msBuildProj.ReevaluateIfNecessary();
            ProjectInstance result = msBuildProj.CreateProjectInstance();
            msBuildProj.SetGlobalProperty(ProjectFileConstants.Configuration, GetActiveConfigurationName());
            msBuildProj.SetGlobalProperty(ProjectFileConstants.Platform, GetActivePlatformName());
            return result;
        }

        string GetActiveConfigurationName()
        {
            var automationObject = (EnvDTE.Project)proj.GetAutomationObject();
            return Utilities.GetActiveConfigurationName(automationObject);
        }

        string GetActivePlatformName()
        {
            var automationObject = (EnvDTE.Project)proj.GetAutomationObject();
            return automationObject.ConfigurationManager.ActiveConfiguration.PlatformName;
        }
    }
}
