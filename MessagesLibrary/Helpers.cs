using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MessagesLibrary
{
    public class Helpers
    {
        // Append file or folder to run directory
        public static string AppendToRunPath(string path)
        {
            var basePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var result = Path.Combine(basePath, path);
            return result;
        }

        // Find an byte[] in a byte[]
        public static int FindInArray(byte[] data, byte[] value)
        {
            int pos = -1;
            for(int n=0; n<=data.Length-value.Length; n++)
            {
                pos = n;
                for(int n2=0;n2<value.Length;n2++)
                {
                    if (data[n + n2] != value[n2])
                    {
                        pos = -1;
                        break;
                    }
                }
                if (pos != -1)
                    break;
            }
            return pos;
        }

        // Format bandwidth into formatted string
        public static string FormatBandwidth(Int32 bytesPerSec, int decimalPlaces = 1)
        {
            if(bytesPerSec <= 0)
            {
                return "0 bytes/sec";
            }
            string[] SizeSuffixes = { "bytes/s", "kB/s", "MB/s", "GB/s", "TB/s", "PB/s", "EB/s", "ZB/s", "YB/s" };

            if (bytesPerSec < 0)
            { 
                return "-" + FormatBandwidth(-bytesPerSec); 
            }
            if (bytesPerSec == 0) 
            { 
                return string.Format("{0:n" + decimalPlaces + "} bytes", 0); 
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(bytesPerSec, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)bytesPerSec / (1L << (mag * 10));

            // make adjustment when the value is large enough that it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            string result = string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, SizeSuffixes[mag]);
            return result;
        }
    }
}
