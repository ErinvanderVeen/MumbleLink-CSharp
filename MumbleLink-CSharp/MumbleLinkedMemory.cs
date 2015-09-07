using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MumbleLink_CSharp
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MumbleLinkedMemory
    {
        public readonly uint UiVersion;
        public readonly uint UiTick;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly float[] FAvatarPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly float[] FAvatarFront;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly float[] FAvatarTop;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public readonly char[] Name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly float[] FCameraPosition;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly float[] FCameraFront;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly float[] FCameraTop;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public readonly char[] Identity;
        public readonly uint ContextLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public readonly byte[] Context;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2048)]
        public readonly char[] Description;

        public override string ToString()
        {
            return string.Format("UiVersion: {0}, UiTick: {1}, FAvatarPosition: {2}, FAvatarFront: {3}, FAvatarTop: {4}, Name: {5}, FCameraPosition: {6}, FCameraFront: {7}, FCameraTop: {8}, Identity: {9}, ContextLen: {10}, Context: {11}, Description: {12}", UiVersion, UiTick, FAvatarPosition, FAvatarFront, FAvatarTop, Name, FCameraPosition, FCameraFront, FCameraTop, Identity, ContextLen, Context, Description);
        }
    }
}