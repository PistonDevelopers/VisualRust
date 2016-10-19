using System;

namespace Microsoft.Common.Core.Threading {
    public interface IAsyncReaderWriterLockToken : IDisposable {
        ReentrancyToken Reentrancy { get; }
    }
}