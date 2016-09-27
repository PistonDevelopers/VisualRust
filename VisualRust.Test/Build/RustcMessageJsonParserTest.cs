using System.Linq;
using NUnit.Framework;
using VisualRust.Build.Message;
using VisualRust.Build.Message.Json;

namespace VisualRust.Test.Build
{
    class RustcMessageJsonParserTest
    {
        [Test]
        public void WithoutError()
        {
            const string output = "";
            var messages = RustcMessageJsonParser.Parse(output).ToList();

            Assert.AreEqual(0, messages.Count);
        }

        [Test]
        public void FileNotFound()
        {
            const string output = "{\"message\":\"couldn't read \\\"main2.rs\\\": file" +
                                  " not found. (os error 2)\",\"code\":null,\"level\":\"error" +
                                  "\",\"spans\":[],\"children\":[],\"rendered\":null}\n";
            var messages = RustcMessageJsonParser.Parse(output).ToList();

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(RustcMessageType.Error, messages[0].GetLevelAsEnum());
        }

        [Test]
        public void NoMain()
        {
            const string output = "{\"message\":\"main function not found\",\"code\":null" + 
                                  ",\"level\":\"error\",\"spans\":[],\"children\":[],\"rendered\":null" + 
                                  "}\n{\"message\":\"aborting due to previous error\",\"code\":null,\"level" + 
                                  "\":\"error\",\"spans\":[],\"children\":[],\"rendered\":null}\n";
            var messages = RustcMessageJsonParser.Parse(output).ToList();

            Assert.AreEqual(2, messages.Count);
        }

        [Test]
        public void WithWarning()
        {
            const string output = "{\"message\":\"unused variable: `x`, #[warn" +
                                  "(unused_variables)] on by default\",\"code\":null,\"level\":\"war" +
                                  "ning\",\"spans\":[{\"file_name\":\"main.rs\",\"byte_start\":498,\"by" +
                                  "te_end\":499,\"line_start\":12,\"line_end\":12,\"column_start\":9,\"c" +
                                  "olumn_end\":10,\"is_primary\":true,\"text\":[{\"text\":\"    let x " +
                                  "= 42;\\r\",\"highlight_start\":9,\"highlight_end\":10}],\"label\":nu" +
                                  "ll,\"suggested_replacement\":null,\"expansion\":null}],\"chil" +
                                  "dren\":[],\"rendered\":null}\n";
            var messages = RustcMessageJsonParser.Parse(output).ToList();

            Assert.AreEqual(1, messages.Count);
            Assert.AreEqual(RustcMessageType.Warning, messages[0].GetLevelAsEnum());

            var span = messages[0].GetPrimarySpan();
            Assert.NotNull(span);
            Assert.True(span.is_primary);


        }
    }
}
