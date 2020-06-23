﻿using System.IO;
using System.Text;

namespace Bhbk.Lib.Aurora.Domain.Helpers
{
    public class HashHelper
    {
        /*
         * https://medium.com/eonian-technologies/file-name-hashing-creating-a-hashed-directory-structure-eabb03aa4091
         */
        public static string GenerateDirectoryHash(string input)
        {
            int hashCode = input.GetHashCode();

            int mask = 255;
            int level1 = hashCode & mask;
            int level2 = (hashCode >> 8) & mask;

            var hashPath = new StringBuilder()
                .Append(string.Format("{0:X}", level1))
                .Append(Path.DirectorySeparatorChar)
                .Append(string.Format("{0:X}", level2)).ToString();

            return hashPath;
        }

        public static string GetHexString(byte[] input)
        {
            var sb = new StringBuilder();

            foreach (byte b in input)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}
