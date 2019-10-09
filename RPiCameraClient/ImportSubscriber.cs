﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace RPiCameraClient
{
    public class ImportSubscriber
    {
        [DllImport("RPiClientLib.dll", EntryPoint = "#2", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StartSubscription();

        [DllImport("RPiClientLib.dll", EntryPoint = "#3", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool StopSubscription();

        [DllImport("RPiClientLib.dll", EntryPoint = "#1", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetCurrentImage(out byte[] buffer, ref int size);

    }
}
