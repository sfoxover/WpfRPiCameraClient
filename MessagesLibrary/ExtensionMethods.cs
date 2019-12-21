using System;
using System.Collections.Generic;
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
    }
}
