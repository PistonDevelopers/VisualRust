using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test.Integration.Fake
{
    class FakeIVsSolution : IVsSolution
    {
        public int AddVirtualProject(IVsHierarchy pHierarchy, uint grfAddVPFlags)
        {
            return VSConstants.S_OK;
        }

        public int AddVirtualProjectEx(IVsHierarchy pHierarchy, uint grfAddVPFlags, ref Guid rguidProjectID)
        {
            return VSConstants.S_OK;
        }

        public int AdviseSolutionEvents(IVsSolutionEvents pSink, out uint pdwCookie)
        {
            pdwCookie = 0;
            return VSConstants.S_OK;
        }

        public int CanCreateNewProjectAtLocation(int fCreateNewSolution, string pszFullProjectFilePath, out int pfCanCreate)
        {
            pfCanCreate = 0;
            return VSConstants.S_OK;
        }

        public int CloseSolutionElement(uint grfCloseOpts, IVsHierarchy pHier, uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int CreateNewProjectViaDlg(string pszExpand, string pszSelect, uint dwReserved)
        {
            return VSConstants.S_OK;
        }

        public int CreateProject(ref Guid rguidProjectType, string lpszMoniker, string lpszLocation, string lpszName, uint grfCreateFlags, ref Guid iidProject, out IntPtr ppProject)
        {
            ppProject = IntPtr.Zero;
            return VSConstants.S_OK;
        }

        public int CreateSolution(string lpszLocation, string lpszName, uint grfCreateFlags)
        {
            return VSConstants.S_OK;
        }

        public int GenerateNextDefaultProjectName(string pszBaseName, string pszLocation, out string pbstrProjectName)
        {
            pbstrProjectName = null;
            return VSConstants.S_OK;
        }

        public int GenerateUniqueProjectName(string lpszRoot, out string pbstrProjectName)
        {
            pbstrProjectName = null;
            return VSConstants.S_OK;
        }

        public int GetGuidOfProject(IVsHierarchy pHierarchy, out Guid pguidProjectID)
        {
            pguidProjectID = Guid.Empty;
            return VSConstants.S_OK;
        }

        public int GetItemInfoOfProjref(string pszProjref, int propid, out object pvar)
        {
            pvar = 0;
            return VSConstants.S_OK;
        }

        public int GetItemOfProjref(string pszProjref, out IVsHierarchy ppHierarchy, out uint pitemid, out string pbstrUpdatedProjref, VSUPDATEPROJREFREASON[] puprUpdateReason)
        {
            ppHierarchy = null;
            pitemid = 0;
            pbstrUpdatedProjref = null;
            return VSConstants.S_OK;
        }

        public int GetProjectEnum(uint grfEnumFlags, ref Guid rguidEnumOnlyThisType, out IEnumHierarchies ppenum)
        {
            ppenum = null;
            return VSConstants.S_OK;
        }

        public int GetProjectFactory(uint dwReserved, Guid[] pguidProjectType, string pszMkProject, out IVsProjectFactory ppProjectFactory)
        {
            ppProjectFactory = null;
            return VSConstants.S_OK;
        }

        public int GetProjectFilesInSolution(uint grfGetOpts, uint cProjects, string[] rgbstrProjectNames, out uint pcProjectsFetched)
        {
            pcProjectsFetched = 0;
            return VSConstants.S_OK;
        }

        public int GetProjectInfoOfProjref(string pszProjref, int propid, out object pvar)
        {
            pvar = null;
            return VSConstants.S_OK;
        }

        public int GetProjectOfGuid(ref Guid rguidProjectID, out IVsHierarchy ppHierarchy)
        {
            ppHierarchy = null;
            return VSConstants.S_OK;
        }

        public int GetProjectOfProjref(string pszProjref, out IVsHierarchy ppHierarchy, out string pbstrUpdatedProjref, VSUPDATEPROJREFREASON[] puprUpdateReason)
        {
            ppHierarchy = null;
            pbstrUpdatedProjref = null;
            return VSConstants.S_OK;
        }

        public int GetProjectOfUniqueName(string pszUniqueName, out IVsHierarchy ppHierarchy)
        {
            ppHierarchy = null;
            return VSConstants.S_OK;
        }

        public int GetProjectTypeGuid(uint dwReserved, string pszMkProject, out Guid pguidProjectType)
        {
            pguidProjectType = Guid.Empty;
            return VSConstants.S_OK;
        }

        public int GetProjrefOfItem(IVsHierarchy pHierarchy, uint itemid, out string pbstrProjref)
        {
            pbstrProjref = null;
            return VSConstants.S_OK;
        }

        public int GetProjrefOfProject(IVsHierarchy pHierarchy, out string pbstrProjref)
        {
            pbstrProjref = null;
            return VSConstants.S_OK;
        }

        public int GetProperty(int propid, out object pvar)
        {
            pvar = null;
            return VSConstants.S_OK;
        }

        public int GetSolutionInfo(out string pbstrSolutionDirectory, out string pbstrSolutionFile, out string pbstrUserOptsFile)
        {
            pbstrSolutionDirectory = null;
            pbstrSolutionFile = null;
            pbstrUserOptsFile = null;
            return VSConstants.S_OK;
        }

        public int GetUniqueNameOfProject(IVsHierarchy pHierarchy, out string pbstrUniqueName)
        {
            pbstrUniqueName = null;
            return VSConstants.S_OK;
        }

        public int GetVirtualProjectFlags(IVsHierarchy pHierarchy, out uint pgrfAddVPFlags)
        {
            pgrfAddVPFlags = 0;
            return VSConstants.S_OK;
        }

        public int OnAfterRenameProject(IVsProject pProject, string pszMkOldName, string pszMkNewName, uint dwReserved)
        {
            return VSConstants.S_OK;
        }

        public int OpenSolutionFile(uint grfOpenOpts, string pszFilename)
        {
            return VSConstants.S_OK;
        }

        public int OpenSolutionViaDlg(string pszStartDirectory, int fDefaultToAllProjectsFilter)
        {
            return VSConstants.S_OK;
        }

        public int QueryEditSolutionFile(out uint pdwEditResult)
        {
            pdwEditResult = 0;
            return VSConstants.S_OK;
        }

        public int QueryRenameProject(IVsProject pProject, string pszMkOldName, string pszMkNewName, uint dwReserved, out int pfRenameCanContinue)
        {
            pfRenameCanContinue = 0;
            return VSConstants.S_OK;
        }

        public int RemoveVirtualProject(IVsHierarchy pHierarchy, uint grfRemoveVPFlags)
        {
            return VSConstants.S_OK;
        }

        public int SaveSolutionElement(uint grfSaveOpts, IVsHierarchy pHier, uint docCookie)
        {
            return VSConstants.S_OK;
        }

        public int SetProperty(int propid, object var)
        {
            return VSConstants.S_OK;
        }

        public int UnadviseSolutionEvents(uint dwCookie)
        {
            return VSConstants.S_OK;
        }
    }
}
