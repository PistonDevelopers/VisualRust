// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace VisualRust.ProjectSystem.FileSystemMirroring.Project {
    /// <summary>
    /// Allows creation of nested items in the project. Exported via MEF.
    /// </summary>
    public interface IProjectItemDependencyProvider {
        /// <summary>
        /// For a given child file returns master file (root of the nested set). 
        /// For example, returns Script.R for Script.R.sql.
        string GetMasterFile(string childFilePath);
    }
}
