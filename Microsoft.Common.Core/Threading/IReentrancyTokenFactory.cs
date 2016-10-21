namespace Microsoft.Common.Core.Threading {
    internal interface IReentrancyTokenFactory<TSource> where TSource : class {
        TSource GetSource(ReentrancyToken token);
        ReentrancyToken Create(TSource source);
    }
}