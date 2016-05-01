using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualRust.Cargo
{
    public static class Rust
    {
        [StructLayout(LayoutKind.Sequential)]
        struct EXCEPTION_POINTERS
        {
            public IntPtr ExceptionRecord;
            public IntPtr ContextRecord;
        }

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct EXCEPTION_RECORD
        {
            public uint ExceptionCode;
            public uint ExceptionFlags;
            public IntPtr ExceptionRecord;
            public IntPtr ExceptionAddress;
            public uint NumberParameters;
            public IntPtr ExceptionInformation;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct FFIPanicInfo
        {
            public Utf8String Message;
            public Utf8String File;
            public int Line;
        }

        public static void Invoke(Action a)
        {
            try
            {
                a();
            }
            catch (SEHException exc)
            {
                unsafe
                {
                    var exceptionPointers = (EXCEPTION_POINTERS*)Marshal.GetExceptionPointers();
                    var exceptionRecord = (EXCEPTION_RECORD*)(*exceptionPointers).ExceptionRecord;
                    if ((*exceptionRecord).ExceptionCode != RustException.PanicCode)
                        throw;
                    var panicInfo = *(FFIPanicInfo*)(*exceptionRecord).ExceptionInformation;
                    string message = panicInfo.Message.ToString();
                    string file = panicInfo.File.ToString();
                    string fullMessage = FormatError(message, file, panicInfo);
                    throw new RustException(fullMessage, exc);
                }
            }
        }

        public static void Invoke<T>(Action<T> a, T t)
        {
            Invoke(() => a(t));
        }

        public static T Call<T>(Func<T> f)
        {
            try
            {
                return f();
            }
            catch (SEHException exc)
            {
                unsafe
                {
                    var exceptionPointers = (EXCEPTION_POINTERS*)Marshal.GetExceptionPointers();
                    var exceptionRecord = (EXCEPTION_RECORD*)(*exceptionPointers).ExceptionRecord;
                    if ((*exceptionRecord).ExceptionCode != RustException.PanicCode)
                        throw;
                    var panicInfo = *(FFIPanicInfo*)(*exceptionRecord).ExceptionInformation;
                    string message = panicInfo.Message.ToString();
                    string file = panicInfo.File.ToString();
                    string fullMessage = FormatError(message, file, panicInfo);
                    throw new RustException(fullMessage, exc);
                }
            }
        }
        public static TResult Call<T1, TResult>(Func<T1, TResult> f, T1 t1)
        {
            return Call(() => f(t1));
        }

        public static TResult Call<T1, T2, TResult>(Func<T1, T2, TResult> f, T1 t1, T2 t2)
        {
            return Call(() => f(t1, t2));
        }

        private static string FormatError(string message, string file, FFIPanicInfo panicInfo)
        {
            if (message != null)
            {
                if (file != null)
                {
                    return String.Format("Panic `{0}` at {1}:{2}", message, file, panicInfo.Line);
                }
                return String.Format("Panic `{0}`", message);
            }
            if (file != null)
            {
                return String.Format("Panic at {0}:{1}", file, panicInfo.Line);
            }
            return null;
        }
    }
}
