// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Security {
    public interface ISecurityService {
        Task<Credentials> GetUserCredentialsAsync(string authority, bool invalidateStoredCredentials);
        Task<bool> ValidateX509CertificateAsync(X509Certificate certificate, string message);
    }
}
