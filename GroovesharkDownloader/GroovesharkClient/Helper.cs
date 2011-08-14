using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;


namespace Helper
{
    using System.Security.Cryptography;
    using System.Text;

    public static class Hash
    {
        private static string GetMD5Hash(byte[] strBytes)
        {
            Contract.Requires(strBytes != null);

            var buffer = new MD5CryptoServiceProvider().ComputeHash(strBytes);
            var builder = new StringBuilder();
            foreach (var num in buffer)
            {
                builder.Append(num.ToString("x2"));
            }
            return builder.ToString();
        }

        public static string ToMD5Hash(this string str)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(str));

            return GetMD5Hash(Encoding.ASCII.GetBytes(str));
        }

        private static string GetSHA1Hash(byte[] strBytes)
        {
            byte[] buffer = new SHA1CryptoServiceProvider().ComputeHash(strBytes);
            var builder = new StringBuilder();
            foreach (var num in buffer)
            {
                builder.Append(num.ToString("x2"));
            }
            return builder.ToString();
        }

        public static string ToSHA1Hash(this string str)
        {
            return GetSHA1Hash(Encoding.ASCII.GetBytes(str));
        }
    }

    public static class ClassExtensions
    {
        public static void ThrowIfArgumentIsNull<T>(this T obj) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException();
        }
    }
}

