using System;

namespace VisualRust
{
    static class GuidList
    {
        public const string guidVisualRustPkgString = "40c1d2b5-528b-4966-a7b1-1974e3568abe";
        public static Guid VisualRustCommandSet = new Guid("{91C8967B-EB9D-4904-AB07-4ACCA9C0ECFE}");

        public const string CpsProjectFactoryGuidString = "c7cbdbed-50ca-46fc-be3b-1d7809d42a0a";
        public static readonly Guid CpsProjectFactoryGuid = new Guid(CpsProjectFactoryGuidString);
        public const string ProjectFileGenerationGuidString = "04486994-cf85-4394-b3cd-53ddc27698f5";
        public static readonly Guid VisualRustPkgGuid = new Guid(guidVisualRustPkgString);
    };
}