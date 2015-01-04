using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Test.Integration
{
    internal static class AssertEx
    {
        public static void IsThrown<T>(Action action) where T : Exception
        {
            try
            {
                action.Invoke();
                Assert.Fail("Exception of type {0} should be thrown.", typeof(T));
            }
            catch (Exception exc)
            {
                Assert.IsInstanceOfType(exc, typeof(T));
            }
        }
    }
}
