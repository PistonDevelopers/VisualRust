// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Microsoft.Common.Core.Shell {
    public interface ICompositionCatalog {
        /// <summary>
        /// Host application MEF composition service.
        /// </summary>
        ICompositionService CompositionService { get; }

        /// <summary>
        /// Visual Studio MEF export provider.
        /// </summary>
        ExportProvider ExportProvider { get; }
    }
}
