using System;

namespace VisualRust.Build
{
    class Version
    {
        public UInt32 VersionMajor { get; private set; }
        public UInt32 VersionMinor { get; private set; }
        public UInt32 VersionPatch { get; private set; }
        public string VersionVariant { get; private set; }

        public static Version ParseRustcOutput(String output)
        {
            var outputParts = output.Split(' ');
            if (outputParts.Length != 4)
                return null;

            var releaseParts = outputParts[1].Split('.', '-');
            if (releaseParts.Length != 4)
                return null;

            var result = new Version();
            try
            {
                result.VersionMajor = Convert.ToUInt32(releaseParts[0]);
                result.VersionMinor = Convert.ToUInt32(releaseParts[1]);
                result.VersionPatch = Convert.ToUInt32(releaseParts[2]);
                result.VersionVariant = releaseParts[3].Trim().ToLowerInvariant();
            }
            catch (Exception ex)
            {
                return null;
            }

            return result;
        }
    }
}
