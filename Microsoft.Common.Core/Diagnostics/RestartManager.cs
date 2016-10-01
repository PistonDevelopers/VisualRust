// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Common.Core.Diagnostics {
    public static class RestartManager {
        /// <summary>
        /// Given a list of filenames with absolute path, returns enumerable of process currently locking those files.
        /// </summary>
        /// <param name="filePaths">Filenames with absolute path.</param>
        /// <returns>Enumerable of processes locking the files. Empty if it encounters any error.</returns>
        public static IEnumerable<Process> GetProcessesUsingFiles(string[] filePaths) {
            uint sessionHandle;
            int error = NativeMethods.RmStartSession(out sessionHandle, 0, Guid.NewGuid().ToString("N"));
            if (error == 0) {
                try {
                    error = NativeMethods.RmRegisterResources(sessionHandle, (uint)filePaths.Length, filePaths, 0, null, 0, null);
                    if (error == 0) {
                        RM_PROCESS_INFO[] processInfo = null;
                        uint pnProcInfoNeeded = 0, pnProcInfo = 0, lpdwRebootReasons = RmRebootReasonNone;
                        error = NativeMethods.RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);
                        while (error == ERROR_MORE_DATA) {
                            processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                            pnProcInfo = (uint)processInfo.Length;
                            error = NativeMethods.RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref lpdwRebootReasons);
                        }

                        if (error == 0 && processInfo != null) {
                            for (var i = 0; i < pnProcInfo; i++) {
                                RM_PROCESS_INFO procInfo = processInfo[i];
                                Process proc = null;
                                try {
                                    proc = Process.GetProcessById(procInfo.Process.dwProcessId);
                                } catch (ArgumentException) {
                                    // Eat exceptions for processes which are no longer running.
                                }

                                if (proc != null) {
                                    yield return proc;
                                }
                            }
                        }
                    }
                } finally {
                    NativeMethods.RmEndSession(sessionHandle);
                }
            }
        }

        private const int RmRebootReasonNone = 0;
        private const int CCH_RM_MAX_APP_NAME = 255;
        private const int CCH_RM_MAX_SVC_NAME = 63;
        private const int ERROR_MORE_DATA = 234;

        [StructLayout(LayoutKind.Sequential)]
        private struct RM_UNIQUE_PROCESS {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RM_PROCESS_INFO {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public string strServiceShortName;
            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        private enum RM_APP_TYPE {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [SuppressUnmanagedCodeSecurity]
        private static class NativeMethods {
            /// <summary>
            /// Starts a new Restart Manager session.
            /// </summary>
            /// <param name="pSessionHandle">A pointer to the handle of a Restart Manager session. The session handle can be passed in subsequent calls to the Restart Manager API.</param>
            /// <param name="dwSessionFlags">Reserved must be 0.</param>
            /// <param name="strSessionKey">A null-terminated string that contains the session key to the new session. A GUID will work nicely.</param>
            /// <returns>Error code. 0 is successful.</returns>
            [DllImport("RSTRTMGR.DLL", CharSet = CharSet.Unicode)]
            public static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

            /// <summary>
            /// Ends the Restart Manager session.
            /// </summary>
            /// <param name="pSessionHandle">A handle to an existing Restart Manager session.</param>
            /// <returns>Error code. 0 is successful.</returns>
            [DllImport("RSTRTMGR.DLL")]
            public static extern int RmEndSession(uint pSessionHandle);

            /// <summary>
            /// Registers resources to a Restart Manager session. 
            /// </summary>
            /// <param name="pSessionHandle">A handle to an existing Restart Manager session.</param>
            /// <param name="nFiles">The number of files being registered.</param>
            /// <param name="rgsFilenames">An array of strings of full filename paths.</param>
            /// <param name="nApplications">The number of processes being registered.</param>
            /// <param name="rgApplications">An array of RM_UNIQUE_PROCESS structures. </param>
            /// <param name="nServices">The number of services to be registered.</param>
            /// <param name="rgsServiceNames">An array of null-terminated strings of service short names.</param>
            /// <returns>Error code. 0 is successful.</returns>
            [DllImport("RSTRTMGR.DLL", CharSet = CharSet.Unicode)]
            public static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames, uint nApplications, [In] RM_UNIQUE_PROCESS[] rgApplications, uint nServices, string[] rgsServiceNames);

            /// <summary>
            /// Gets a list of all applications and services that are currently using resources that have been registered with the Restart Manager session.
            /// </summary>
            /// <param name="dwSessionHandle">A handle to an existing Restart Manager session.</param>
            /// <param name="pnProcInfoNeeded">A pointer to an array size necessary to receive RM_PROCESS_INFO structures</param>
            /// <param name="pnProcInfo">A pointer to the total number of RM_PROCESS_INFO structures in an array and number of structures filled.</param>
            /// <param name="rgAffectedApps">An array of RM_PROCESS_INFO structures that list the applications and services using resources that have been registered with the session.</param>
            /// <param name="lpdwRebootReasons">Pointer to location that receives a value of the RM_REBOOT_REASON enumeration that describes the reason a system restart is needed.</param>
            /// <returns>Error code. 0 is successful.</returns>
            [DllImport("RSTRTMGR.DLL")]
            public static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps, ref uint lpdwRebootReasons);
        }
    }
}
