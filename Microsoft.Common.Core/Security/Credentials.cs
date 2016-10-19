// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Security;

namespace Microsoft.Common.Core.Security {
    public sealed class Credentials : ICredentials {
        // Although NetworkCredential does support SecureString, it still exposes
        // plain text password via property and it is visible in a debugger.
        public string UserName { get; set; }
        public SecureString Password { get; set; }

        public NetworkCredential GetCredential(Uri uri, string authType) {
            return new NetworkCredential(UserName, Password);
        }
    }
}
