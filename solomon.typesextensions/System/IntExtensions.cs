using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Solomon.TypesExtensions
{
    public static class IntExtensions
    {
        private const int alphaBase = 26;
        private const int digitMax = 7; // ceil(log26(Int32.Max))
        private const string digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string ToAlpha(this int Index)
        {
            if (Index <= 0)
                throw new IndexOutOfRangeException("Index must be a positive number");

            if (Index <= alphaBase)
                return digits[Index - 1].ToString();

            var sb = new StringBuilder().Append(' ', digitMax);
            var current = Index;
            var offset = digitMax;
            while (current > 0)
            {
                sb[--offset] = digits[--current % alphaBase];
                current /= alphaBase;
            }
            return sb.ToString(offset, digitMax - offset);
        }
    }
}
