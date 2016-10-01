// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.ReflectionModel;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Microsoft.Common.Core.Extensions {
    public static partial class CompositionBatchExtensions {
        public static CompositionBatch AddValue<TContract>(this CompositionBatch batch, TContract value) {
            string contractName = AttributedModelServices.GetContractName(typeof(TContract));
            return batch.AddValue(contractName, value);
        }

        public static CompositionBatch AddValue<T>(this CompositionBatch batch, string contractName, T value) {
            var contractExport = CreateExportDefinition(contractName, typeof(T));
            var partDefinition = CreatePartDefinition(Enumerable.Empty<ImportDefinition>(), contractExport, typeof(T));
            var part = AttributedModelServices.CreatePart(partDefinition, value);

            batch.AddPart(part);
            return batch;
        }

        public static CompositionBatch AddValueFactory(this CompositionBatch batch, string contractName, ComposablePartDefinition ctorDefinition, Type type, Delegate factory) {
            var contractExport = CreateExportDefinition(contractName, type);
            var partDefinition = CreatePartDefinition(ctorDefinition.ImportDefinitions, contractExport, type);
            var part = new FactoryReflectionComposablePart(partDefinition, factory);

            batch.AddPart(part);
            return batch;
        }

        private static ExportDefinition CreateExportDefinition(string contractName, Type type) {
            LazyMemberInfo memberInfo = new LazyMemberInfo(MemberTypes.TypeInfo, type);

            Lazy<IDictionary<string, object>> metadata = new Lazy<IDictionary<string, object>>(() => {
                string typeIdentity = AttributedModelServices.GetTypeIdentity(type);
                return new Dictionary<string, object> {
                    {CompositionConstants.ExportTypeIdentityMetadataName, typeIdentity}
                };
            });

            ExportDefinition exportDefinition = ReflectionModelServices.CreateExportDefinition(memberInfo, contractName, metadata, null);
            return exportDefinition;
        }


        private static ComposablePartDefinition CreatePartDefinition(IEnumerable<ImportDefinition> ctorImports, ExportDefinition contractExport, Type type) {
            ComposablePartDefinition originalPartDefinition = AttributedModelServices.CreatePartDefinition(type, null);
            if (originalPartDefinition == null) {
                throw new InvalidOperationException();
            }

            IList<ImportDefinition> imports = originalPartDefinition.ImportDefinitions
                .Where(idef => !ReflectionModelServices.IsImportingParameter(idef))
                .Concat(ctorImports)
                .ToList();

            IList<ExportDefinition> exports = originalPartDefinition.ExportDefinitions
                .Append(contractExport)
                .ToList();

            IDictionary<string, object> metadata = originalPartDefinition.Metadata;

            return CreatePartDefinition(type, imports, exports, metadata);
        }

        private static ComposablePartDefinition CreatePartDefinition(Type type, IList<ImportDefinition> imports, IList<ExportDefinition> exports, IDictionary<string, object> metadata) {
            return ReflectionModelServices.CreatePartDefinition(
                new Lazy<Type>(() => type, LazyThreadSafetyMode.PublicationOnly),
                false,
                new Lazy<IEnumerable<ImportDefinition>>(() => imports),
                new Lazy<IEnumerable<ExportDefinition>>(() => exports),
                new Lazy<IDictionary<string, object>>(() => metadata),
                null);
        }


    }
}
