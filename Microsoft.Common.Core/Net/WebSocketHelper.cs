// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core {
    public class WebSocketHelper {
        private static async Task DoWebSocketReceiveSendAsync(WebSocket receiver, WebSocket sender, CancellationToken ct) {
            if (receiver == null || sender == null) {
                return;
            }

            ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[65335]);
            while (receiver.State == WebSocketState.Open && sender.State == WebSocketState.Open) {
                if (ct.IsCancellationRequested) {
                    receiver.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None).SilenceException<WebSocketException>().DoNotWait();
                    sender.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", CancellationToken.None).SilenceException<WebSocketException>().DoNotWait();
                    return;
                }

                WebSocketReceiveResult result = await receiver.ReceiveAsync(receiveBuffer, ct);

                byte[] data = await receiveBuffer.ToByteArrayAsync(result.Count);
                ArraySegment<byte> sendBuffer = new ArraySegment<byte>(data);
                await sender.SendAsync(sendBuffer, result.MessageType, result.EndOfMessage, ct);

                if (result.MessageType == WebSocketMessageType.Close) {
                    await receiver.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal closure", ct);
                }
            }
        }

        public static Task SendReceiveAsync(WebSocket ws1, WebSocket ws2, CancellationToken ct) {
            return Task.WhenAll(
                DoWebSocketReceiveSendAsync(ws1, ws2, ct),
                DoWebSocketReceiveSendAsync(ws2, ws1, ct));
        }
    }
}
