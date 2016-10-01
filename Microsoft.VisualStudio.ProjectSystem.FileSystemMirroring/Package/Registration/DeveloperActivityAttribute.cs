// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Package.Registration {
    public sealed class DeveloperActivityAttribute : RegistrationAttribute {
        private readonly string _projectType;
        private readonly int _templateSet;
        private readonly string _developerActivity;
        private readonly int _sortPriority;

        public DeveloperActivityAttribute(string developerActivity, string projectPackageType, int sortPriority, int templateSet = 1) {
            _developerActivity = developerActivity;
            _projectType = projectPackageType;
            _templateSet = templateSet;
            _sortPriority = sortPriority;
        }

        public override void Register(RegistrationAttribute.RegistrationContext context) {
            Build().Register(context);
        }

        public override void Unregister(RegistrationAttribute.RegistrationContext context) {
            Build().Unregister(context);
        }

        private RegistrationAttributeBuilder Build() {
            var builder = new RegistrationAttributeBuilder();
            builder.Key("NewProjectTemplates\\TemplateDirs")
                .GuidSubKey(_projectType)
                    .SubKey("/" + _templateSet)
                        .StringValue("", _developerActivity)
                        .StringValue("TemplatesDir", "\\.\\NullPath")
                        .StringValue("DeveloperActivity", _developerActivity)
                        .IntValue("SortPriority", _sortPriority);
            return builder;
        }
    }
}
