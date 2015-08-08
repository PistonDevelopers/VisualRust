using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Runtime.Serialization;

namespace VisualRust
{
    public class Downloader
    {
        private static String[] Sha256Fingerprints =
        {
            "0C:0B:AC:4C:96:D9:F2:2C:8D:7A:00:9F:2F:48:3D:7B:46:FE:2C:60:0B:52:19:5B:B4:80:47:36:7C:03:E9:41",
            // all the known static.rust-lang.org cert fingerprints should be specified here. they expire every
            // 2 years.
        };

        private static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                return false;
            }

            var cert2 = new X509Certificate2(certificate);
            var time = System.DateTime.Now;

            if (time > cert2.NotAfter || time < cert2.NotBefore)
            {
                // expiry
                return false;
            }

            var der_encoded = certificate.Export(X509ContentType.Cert);
            var hash = SHA256.Create().ComputeHash(der_encoded);
            var received_fingerprint = BitConverter.ToString(hash).Replace('-', ':');

            foreach (String fingerprint in Sha256Fingerprints)
            {
                if (fingerprint == received_fingerprint) { return true; }
            }

            return false;
        }

        private static WebResponse RawDownload(String relative_path)
        {
            HttpWebRequest wc = (HttpWebRequest)WebRequest.Create("https://static.rust-lang.org/dist/" + relative_path);
            wc.ServerCertificateValidationCallback = ValidateCertificate;
            return wc.GetResponse();
        }

        /// <summary>
        /// Download and verify the signature of a file rust-lang.org, writing the contents into write_into.
        /// </summary>
        /// <exception cref="VerificationException">
        /// Thrown when the signature of the downloaded file could not be verified with Rust's signing key.
        /// Note that the stream will still contain the contents of the file even if verification failed. But,
        /// if verification fails, the contents should not be trusted.
        /// </exception>
        /// <param name="relative_path">Appended to https://static.rust-lang.org/dist/ to determine the file to download</param>
        /// <param name="write_into">The contents of the downloaded file (but not the signature) will be written into this stream.</param>
        public static void Download(String relative_path, Stream write_into)
        {
            var saved_pos = write_into.Position;
            MemoryStream sig = new MemoryStream();
            RawDownload(relative_path).GetResponseStream().CopyTo(write_into);
            RawDownload(relative_path + ".asc").GetResponseStream().CopyTo(sig);
            write_into.Position = 0;
            sig.Position = 0;
            var res = GPG.Gpg.Instance.Verify(write_into, sig);
            if (!res.Item1)
            {
                File.WriteAllText("C:/Users/Elaine/Desktop/gpg_log.txt", res.Item2);
                throw new VerificationException(res.Item2);
            }
        }
    }

    [Serializable]
    public class VerificationException : Exception
    {
        public VerificationException()
        {
        }

        public VerificationException(string message) : base(message)
        {
        }

        public VerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VerificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}