// Guids.cs
// MUST match guids.h
using System;

namespace VisualRust
{
    static class GuidList
    {
        public const string guidVisualRustPkgString = "40c1d2b5-528b-4966-a7b1-1974e3568abe";
        public const string guidVisualRustCmdSetString = "c2ea7a57-3978-4bff-bcba-dea4c6638cad";

        public static readonly Guid guidVisualRustCmdSet = new Guid(guidVisualRustCmdSetString);
    };
}