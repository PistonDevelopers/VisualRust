// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;

namespace VisualRust.ProjectSystem.FileSystemMirroring.Package.Registration {
    public sealed class ProvideProjectFileGeneratorAttribute : RegistrationAttribute {
        public Type GeneratorType { get; }
        public string FileNames { get; set; }
        public string FileExtensions { get; set; }
        public string[] PossibleGeneratedProjectTypes { get; }

        private int? _displayGeneratorFilter;
        public int DisplayGeneratorFilter {
            get { return _displayGeneratorFilter ?? 0; }
            set { _displayGeneratorFilter = value; }
        }

        public ProvideProjectFileGeneratorAttribute(Type projectFileGeneratorType, params string[] possibleGeneratedProjectTypes) {
            GeneratorType = projectFileGeneratorType;
            PossibleGeneratedProjectTypes = possibleGeneratedProjectTypes;
        }

        public override void Register(RegistrationContext context) {
            Build().Register(context);
        }

        public override void Unregister(RegistrationContext context) {
            Build().Unregister(context);
        }

        private RegistrationAttributeBuilder Build() {
            var builder = new RegistrationAttributeBuilder();
            builder.Key("ProjectGenerators")
                .GuidSubKey(GeneratorType)
                    .PackageGuidValue("Package")
                    .StringValue("FileNames", FileNames)
                    .StringValue("FileExtensions", FileExtensions)
                    .ResourceIdValue("DisplayGeneratorFilter", DisplayGeneratorFilter)
                    .GuidArrayValue("PossibleGeneratedProjectTypes", PossibleGeneratedProjectTypes);
            return builder;
        }
    }
}