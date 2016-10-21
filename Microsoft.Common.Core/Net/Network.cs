// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Net {
    public static class NetworkExtensions {
        /// <summary>
        /// Checks machine status via ping.
        /// </summary>
        /// <returns>Empty string if machine is online, exception message if ping failed.</returns>
        public static async Task<string> GetMachineOnlineStatusAsync(this Uri url, int timeout = 3000) {
            if (url.IsFile) {
                return string.Empty;
            }

            try {
                var ping = new Ping();
                var reply = await ping.SendPingAsync(url.Host, timeout);
                if (reply.Status != IPStatus.Success) {
                    return reply.Status.ToString();
                }
            } catch (PingException pex) {
                var pingMessage = pex.InnerException?.Message ?? pex.Message;
                if (!string.IsNullOrEmpty(pingMessage)) {
                    return pingMessage;
                }
            } catch (SocketException sx) {
                return sx.Message;
            }
            return string.Empty;
        }

        public static bool IsHttps(this Uri url) {
            return url.Scheme.EqualsIgnoreCase("https");
        }
    }
}
