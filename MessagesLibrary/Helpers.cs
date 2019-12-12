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
    }
}
