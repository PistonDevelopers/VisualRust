/********************************************************************************************

Copyright (c) Microsoft Corporation 
All rights reserved. 

Microsoft Public License: 

This license governs use of the accompanying software. If you use the software, you 
accept this license. If you do not accept the license, do not use the software. 

1. Definitions 
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the 
same meaning here as under U.S. copyright law. 
A "contribution" is the original software, or any additions or changes to the software. 
A "contributor" is any person that distributes its contribution under this license. 
"Licensed patents" are a contributor's patent claims that read directly on its contribution. 

2. Grant of Rights 
(A) Copyright Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free copyright license to reproduce its contribution, prepare derivative works of 
its contribution, and distribute its contribution or any derivative works that you create. 
(B) Patent Grant- Subject to the terms of this license, including the license conditions 
and limitations in section 3, each contributor grants you a non-exclusive, worldwide, 
royalty-free license under its licensed patents to make, have made, use, sell, offer for 
sale, import, and/or otherwise dispose of its contribution in the software or derivative 
works of the contribution in the software. 

3. Conditions and Limitations 
(A) No Trademark License- This license does not grant you rights to use any contributors' 
name, logo, or trademarks. 
(B) If you bring a patent claim against any contributor over patents that you claim are 
infringed by the software, your patent license from such contributor to the software ends 
automatically. 
(C) If you distribute any portion of the software, you must retain all copyright, patent, 
trademark, and attribution notices that are present in the software. 
(D) If you distribute any portion of the software in source code form, you may do so only 
under this license by including a complete copy of this license with your distribution. 
If you distribute any portion of the software in compiled or object code form, you may only 
do so under a license that complies with this license. 
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give 
no express warranties, guarantees or conditions. You may have additional consumer rights 
under your local laws which this license cannot change. To the extent permitted under your 
local laws, the contributors exclude the implied warranties of merchantability, fitness for 
a particular purpose and non-infringement.

********************************************************************************************/

using System;
using System.Reflection;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Security.Permissions;

namespace Microsoft.VisualStudio.Project
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class SRDescriptionAttribute : DescriptionAttribute
    {
        private bool replaced;

        public SRDescriptionAttribute(string description)
            : base(description)
        {
        }

        public override string Description
        {
            get
            {
                if(!replaced)
                {
                    replaced = true;
                    DescriptionValue = SR.GetString(base.Description, CultureInfo.CurrentUICulture);
                }
                return base.Description;
            }
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public sealed class SRCategoryAttribute : CategoryAttribute
    {

        public SRCategoryAttribute(string category)
            : base(category)
        {
        }

        protected override string GetLocalizedString(string value)
        {
            return SR.GetString(value, CultureInfo.CurrentUICulture);
        }
    }
    public sealed class SR
    {
        public const string AddReferenceDialogTitle = "AddReferenceDialogTitle";
        public const string AddToNullProjectError = "AddToNullProjectError";
        public const string Advanced = "Advanced";
        public const string AssemblyReferenceAlreadyExists = "AssemblyReferenceAlreadyExists";
        public const string AttributeLoad = "AttributeLoad";
        public const string BuildAction = "BuildAction";
        public const string BuildActionDescription = "BuildActionDescription";
        public const string BuildCaption = "BuildCaption";
        public const string BuildVerbosity = "BuildVerbosity";
        public const string BuildVerbosityDescription = "BuildVerbosityDescription";
        public const string BuildEventError = "BuildEventError";
        public const string CancelQueryEdit = "CancelQueryEdit";
        public const string CannotAddFileThatIsOpenInEditor = "CannotAddFileThatIsOpenInEditor";
        public const string CanNotSaveFileNotOpeneInEditor = "CanNotSaveFileNotOpeneInEditor";
        public const string cli1 = "cli1";
        public const string Compile = "Compile";
        public const string ConfirmExtensionChange = "ConfirmExtensionChange";
        public const string Content = "Content";
        public const string CopyToLocal = "CopyToLocal";
        public const string CopyToLocalDescription = "CopyToLocalDescription";
        public const string EmbedInteropTypes = "EmbedInteropTypes";
        public const string EmbedInteropTypesDescription = "EmbedInteropTypesDescription";
        public const string CustomTool = "CustomTool";
        public const string CustomToolDescription = "CustomToolDescription";
        public const string CustomToolNamespace = "CustomToolNamespace";
        public const string CustomToolNamespaceDescription = "CustomToolNamespaceDescription";
        public const string DetailsImport = "DetailsImport";
        public const string DetailsUserImport = "DetailsUserImport";
        public const string DetailsItem = "DetailsItem";
        public const string DetailsItemLocation = "DetailsItemLocation";
        public const string DetailsProperty = "DetailsProperty";
        public const string DetailsTarget = "DetailsTarget";
        public const string DetailsUsingTask = "DetailsUsingTask";
        public const string Detailed = "Detailed";
        public const string Diagnostic = "Diagnostic";
        public const string DirectoryExistError = "DirectoryExistError";
        public const string EditorViewError = "EditorViewError";
        public const string EmbeddedResource = "EmbeddedResource";
        public const string Error = "Error";
        public const string ErrorInvalidFileName = "ErrorInvalidFileName";
        public const string ErrorInvalidProjectName = "ErrorInvalidProjectName";
        public const string ErrorReferenceCouldNotBeAdded = "ErrorReferenceCouldNotBeAdded";
        public const string ErrorMsBuildRegistration = "ErrorMsBuildRegistration";
        public const string ErrorSaving = "ErrorSaving";
        public const string Exe = "Exe";
        public const string ExpectedObjectOfType = "ExpectedObjectOfType";
        public const string FailedToGetService = "FailedToGetService";
        public const string FailedToRetrieveProperties = "FailedToRetrieveProperties";
        public const string FileNameCannotContainALeadingPeriod = "FileNameCannotContainALeadingPeriod";
        public const string FileCannotBeRenamedToAnExistingFile = "FileCannotBeRenamedToAnExistingFile";
        public const string FileAlreadyExistsAndCannotBeRenamed = "FileAlreadyExistsAndCannotBeRenamed";
        public const string FileAlreadyExists = "FileAlreadyExists";
        public const string FileAlreadyExistsCaption = "FileAlreadyExistsCaption";
        public const string FileAlreadyInProject = "FileAlreadyInProject";
        public const string FileAlreadyInProjectCaption = "FileAlreadyInProjectCaption";
        public const string FileCopyError = "FileCopyError";
        public const string FileName = "FileName";
        public const string FileNameDescription = "FileNameDescription";
        public const string FileOrFolderAlreadyExists = "FileOrFolderAlreadyExists";
        public const string FileOrFolderCannotBeFound = "FileOrFolderCannotBeFound";
        public const string FileProperties = "FileProperties";
        public const string FolderName = "FolderName";
        public const string FolderNameDescription = "FolderNameDescription";
        public const string FolderProperties = "FolderProperties";
        public const string FullPath = "FullPath";
        public const string FullPathDescription = "FullPathDescription";
        public const string ItemDoesNotExistInProjectDirectory = "ItemDoesNotExistInProjectDirectory";
        public const string InvalidAutomationObject = "InvalidAutomationObject";
        public const string InvalidLoggerType = "InvalidLoggerType";
        public const string InvalidParameter = "InvalidParameter";
        public const string Library = "Library";
        public const string LinkedItemsAreNotSupported = "LinkedItemsAreNotSupported";
        public const string Minimal = "Minimal";
        public const string Misc = "Misc";
        public const string None = "None";
        public const string Normal = "Normal";
        public const string NestedProjectFailedToReload = "NestedProjectFailedToReload";
        public const string OutputPath = "OutputPath";
        public const string OutputPathDescription = "OutputPathDescription";
        public const string PasteFailed = "PasteFailed";
        public const string ParameterMustBeAValidGuid = "ParameterMustBeAValidGuid";
        public const string ParameterMustBeAValidItemId = "ParameterMustBeAValidItemId";
        public const string ParameterCannotBeNullOrEmpty = "ParameterCannotBeNullOrEmpty";
        public const string PathTooLong = "PathTooLong";
        public const string ProjectContainsCircularReferences = "ProjectContainsCircularReferences";
        public const string Program = "Program";
        public const string Project = "Project";
        public const string ProjectFile = "ProjectFile";
        public const string ProjectFileDescription = "ProjectFileDescription";
        public const string ProjectFolder = "ProjectFolder";
        public const string ProjectFolderDescription = "ProjectFolderDescription";
        public const string ProjectProperties = "ProjectProperties";
        public const string Quiet = "Quiet";
        public const string QueryReloadNestedProject = "QueryReloadNestedProject";
        public const string ReferenceAlreadyExists = "ReferenceAlreadyExists";
        public const string ReferencesNodeName = "ReferencesNodeName";
        public const string ReferenceProperties = "ReferenceProperties";
        public const string RefName = "RefName";
        public const string RefNameDescription = "RefNameDescription";
        public const string RenameFolder = "RenameFolder";
        public const string RTL = "RTL";
        public const string SaveCaption = "SaveCaption";
        public const string SaveModifiedDocuments = "SaveModifiedDocuments";
        public const string SaveOfProjectFileOutsideCurrentDirectory = "SaveOfProjectFileOutsideCurrentDirectory";
        public const string StandardEditorViewError = "StandardEditorViewError";
        public const string Settings = "Settings";
        public const string URL = "URL";
        public const string UseOfDeletedItemError = "UseOfDeletedItemError";
        public const string Warning = "Warning";
        public const string WinExe = "WinExe";
        public const string CannotLoadUnknownTargetFrameworkProject = "CannotLoadUnknownTargetFrameworkProject";
        public const string ReloadPromptOnTargetFxChanged = "ReloadPromptOnTargetFxChanged";
        public const string ReloadPromptOnTargetFxChangedCaption = "ReloadPromptOnTargetFxChangedCaption";

        static SR loader;
        ResourceManager resources;

        private static Object s_InternalSyncObject;
        private static Object InternalSyncObject
        {
            get
            {
                if(s_InternalSyncObject == null)
                {
                    Object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal SR()
        {
            resources = new System.Resources.ResourceManager("Microsoft.VisualStudio.Project", this.GetType().Assembly);
        }

        private static SR GetLoader()
        {
            if(loader == null)
            {
                lock(InternalSyncObject)
                {
                    if(loader == null)
                    {
                        loader = new SR();
                    }
                }
            }

            return loader;
        }

        private static CultureInfo Culture
        {
            get { return null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }

        public static string GetString(string name, params object[] args)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            string res = sys.resources.GetString(name, SR.Culture);

            if(args != null && args.Length > 0)
            {
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static string GetString(string name)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            return sys.resources.GetString(name, SR.Culture);
        }

        public static string GetString(string name, CultureInfo culture)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            return sys.resources.GetString(name, culture);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static object GetObject(string name)
        {
            SR sys = GetLoader();
            if(sys == null)
                return null;
            return sys.resources.GetObject(name, SR.Culture);
        }
    }
}
