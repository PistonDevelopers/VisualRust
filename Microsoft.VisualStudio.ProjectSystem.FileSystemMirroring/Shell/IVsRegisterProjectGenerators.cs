// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell {
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("9FDECA99-9A9D-44A8-98AF-C5A285EEFB47")]
    [ComImport]
    public interface IVsRegisterProjectGenerators {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void RegisterProjectGenerator([ComAliasName("OLE.REFGUID"), In] ref Guid rguidProjGenerator, [MarshalAs(UnmanagedType.Interface), In] IVsProjectGenerator pProjectGenerator, [ComAliasName("EnvDTE.ULONG_PTR")] out uint pdwCookie);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void UnregisterProjectGenerator([ComAliasName("EnvDTE.ULONG_PTR"), In] uint dwCookie);
    }
}
