// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Shell;

namespace VisualRust.ProjectSystem.FileSystemMirroring.Package.Registration {
    public sealed class ProvideCpsProjectFactoryAttribute : RegistrationAttribute {
        public string ProjectTypeGuid { get; }
        public string LanguageVsTemplate { get; }
        public string PossibleProjectExtensions { get; }

        private bool? _disableAsynchronousSolutionLoadValue;
        public bool DisableAsynchronousSolutionLoad {
            get { return _disableAsynchronousSolutionLoadValue ?? false; }
            set { _disableAsynchronousSolutionLoadValue = value; }
        }

        public ProvideCpsProjectFactoryAttribute(string projectTypeGuid, string languageVsTemplate, string possibleProjectExtensions) {
            ProjectTypeGuid = projectTypeGuid;
            LanguageVsTemplate = languageVsTemplate;
            PossibleProjectExtensions = possibleProjectExtensions;
        }

        public override void Register(RegistrationContext context) {
            Build().Register(context);
        }

        public override void Unregister(RegistrationContext context) {
            Build().Unregister(context);
        }

        private RegistrationAttributeBuilder Build() {
            var builder = new RegistrationAttributeBuilder();
            builder.Key("Projects")
                .GuidSubKey(ProjectTypeGuid)
                    .PackageGuidValue("Package")
                    .StringValue("Language(VsTemplate)", LanguageVsTemplate)
                    .GuidValue("ProjectFactoryPackage", "3347BEE8-D7A1-4082-95E4-38A439553CC2")
                    .BoolValue("DisableAsynchronousSolutionLoad", DisableAsynchronousSolutionLoad)
                    .StringValue("PossibleProjectExtensions", PossibleProjectExtensions);
            return builder;
        }
    }
}