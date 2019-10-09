using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RPiCameraClient
{
    public class ImportSubscriber
    {
        [DllImport("RPiClientLib.dll", EntryPoint = "#2", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartSubscription();

    }
}
