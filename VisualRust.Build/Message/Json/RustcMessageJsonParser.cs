using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VisualRust.Build.Message.Json
{
    public static class RustcMessageJsonParser
    {
        private static readonly JsonSerializer serializer = new JsonSerializer();

        public static IEnumerable<RustcMessageJson> Parse(String output)
        {
            var reader = new JsonTextReader(new StringReader(output)) {SupportMultipleContent = true};

            while (reader.Read())
                yield return serializer.Deserialize<RustcMessageJson>(reader);
        }
    }
}
