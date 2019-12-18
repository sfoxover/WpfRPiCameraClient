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
    }
}
