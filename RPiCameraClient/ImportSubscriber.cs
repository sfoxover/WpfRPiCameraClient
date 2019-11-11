using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RPiCameraClient
{
    public class ImportSubscriber
    {
        [DllImport("RPiClientWrapperLib.dll", EntryPoint = "#1", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCurrentImage(ref IntPtr buffer, ref int size);

        [DllImport("RPiClientWrapperLib.dll", EntryPoint = "#2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCurrentSensorData(ref IntPtr buffer, ref int size);

        [DllImport("RPiClientWrapperLib.dll", EntryPoint = "#3", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartSubscription();
        
        [DllImport("RPiClientWrapperLib.dll", EntryPoint = "#4", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StopSubscription();
    }
}
