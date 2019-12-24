using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MessagesLibrary
{
    public static class ExtensionMethods
    {
        // Append byte[] to another byte[], buffer can be null.
        public static byte[] AppendBytes(this byte[] buffer, byte[] appendData)
        {
            int endPos = 0;
            byte[] result = buffer;
            if (buffer != null)
            {
                endPos = buffer.Length;
            }
            Array.Resize(ref result, endPos + appendData.Length);
            System.Buffer.BlockCopy(appendData, 0, result, endPos, appendData.Length);
            return result;
        }

        // Apend dictionary
        public static void AddDictionary<T, S>(this Dictionary<T, S> source, Dictionary<T, S> collection)
        {
            foreach (var item in collection)
            {
                Debug.Assert(!source.ContainsKey(item.Key));
                source.Add(item.Key, item.Value);
            }
        }
    }
}
