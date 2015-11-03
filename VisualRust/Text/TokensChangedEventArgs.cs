using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace VisualRust.Text
{
    public class TokensChangedEventArgs : EventArgs
    {
        public IReadOnlyList<TrackingToken> NewTokens { get; private set; }

        public TokensChangedEventArgs(IReadOnlyList<TrackingToken> newT)
        {
            NewTokens = newT;
        }
    }
}