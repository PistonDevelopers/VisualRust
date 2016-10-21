// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using static Microsoft.Common.Core.NativeMethods;

namespace Microsoft.Common.Core.Security {
    [Export(typeof(ISecurityService))]
    internal class SecurityService : ISecurityService {
        private readonly Lazy<ICoreShell> _coreShellLazy;

        [ImportingConstructor]
        public SecurityService([Import(AllowDefault = true)] Lazy<ICoreShell> coreShellLazy) {
            _coreShellLazy = coreShellLazy;
        }

        public async Task<Credentials> GetUserCredentialsAsync(string authority, bool invalidateStoredCredentials) {
            var showDialog = invalidateStoredCredentials;
            var credentials = new Credentials();

            var passwordStorage = IntPtr.Zero;

            try {
                var userNameBuilder = new StringBuilder(CREDUI_MAX_USERNAME_LENGTH + 1);
                var save = false;
                var flags = CREDUI_FLAGS_EXCLUDE_CERTIFICATES | CREDUI_FLAGS_PERSIST | CREDUI_FLAGS_EXPECT_CONFIRMATION | CREDUI_FLAGS_GENERIC_CREDENTIALS;

                if(showDialog) {
                    flags |= CREDUI_FLAGS_ALWAYS_SHOW_UI;
                }

                await _coreShellLazy.Value.SwitchToMainThreadAsync();
                var credui = new CREDUI_INFO {
                    cbSize = Marshal.SizeOf(typeof(CREDUI_INFO)),
                    hwndParent = _coreShellLazy.Value.AppConstants.ApplicationWindowHandle
                };

                // For password, use native memory so it can be securely freed.
                passwordStorage = SecurityUtilities.CreatePasswordBuffer();
                var err = CredUIPromptForCredentials(ref credui, authority, IntPtr.Zero, 0, userNameBuilder, userNameBuilder.Capacity, passwordStorage, CREDUI_MAX_PASSWORD_LENGTH, ref save, flags);
                if (err != 0) {
                    throw new OperationCanceledException();
                }

                credentials.UserName = userNameBuilder.ToString();
                credentials.Password = SecurityUtilities.SecureStringFromNativeBuffer(passwordStorage);
                credentials.Password.MakeReadOnly();
            } finally {
                if (passwordStorage != IntPtr.Zero) {
                    Marshal.ZeroFreeGlobalAllocUnicode(passwordStorage);
                }
            }

            return credentials;
        }

        public async Task<bool> ValidateX509CertificateAsync(X509Certificate certificate, string message) {
            var certificate2 = certificate as X509Certificate2;
            Debug.Assert(certificate2 != null);
            if (certificate2 == null || !certificate2.Verify()) {
                await _coreShellLazy.Value.SwitchToMainThreadAsync();
                if (_coreShellLazy.Value.ShowMessage(message, MessageButtons.OKCancel, MessageType.Warning) == MessageButtons.OK) {
                    certificate2.Reset();
                    return true;
                }
            }
            return false;
        }
    }
}
