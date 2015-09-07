using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace MumbleLink_CSharp
{

    public class MumbleLink : IDisposable
    {
        private const string Name = "MumbleLink";
        private const float MeterToinch = 39.3701f;


        [SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MumbleLinkedMem
        {
            public readonly UInt32 UiVersion;
            public readonly UInt32 UiTick;
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

        #region Win32

        [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFileMapping(IntPtr hFile, IntPtr lpAttributes, FileMapProtection flProtect, Int32 dwMaxSizeHi, Int32 dwMaxSizeLow, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenFileMapping(FileMapAccess DesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("Kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr hFileMapping, FileMapAccess dwDesiredAccess, Int32 dwFileOffsetHigh, Int32 dwFileOffsetLow, Int32 dwNumberOfBytesToMap);

        [Flags]
        private enum FileMapAccess : uint
        {
            FileMapCopy = 0x0001,
            FileMapWrite = 0x0002,
            FileMapRead = 0x0004,
            FileMapAllAccess = 0x001f,
            fileMapExecute = 0x0020,
        }

        [Flags]
        private enum FileMapProtection : uint
        {
            PageReadonly = 0x02,
            PageReadWrite = 0x04,
            PageWriteCopy = 0x08,
            PageExecuteRead = 0x20,
            PageExecuteReadWrite = 0x40,
            SectionCommit = 0x8000000,
            SectionImage = 0x1000000,
            SectionNoCache = 0x10000000,
            SectionReserve = 0x4000000,
        }

        [DllImport("kernel32", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hFile);

        [DllImport("kernel32")]
        private static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);


        #endregion

        private readonly int _memSize;


        private IntPtr _mappedFile;
        private IntPtr _mapView;
        private readonly byte[] _buffer;
        private GCHandle _bufferHandle;
        private readonly UnmanagedMemoryStream _unmanagedStream;


        // Constructor
        public MumbleLink()
        {
            unsafe
            {
                _memSize = Marshal.SizeOf(typeof(MumbleLinkedMem));

                _mappedFile = OpenFileMapping(FileMapAccess.FileMapRead, false, Name);

                if (_mappedFile == IntPtr.Zero)
                {
                    _mappedFile = CreateFileMapping(IntPtr.Zero, IntPtr.Zero, FileMapProtection.PageReadWrite, 0,
                        _memSize, Name);
                    if (_mappedFile == IntPtr.Zero)
                    {
                        throw new Exception("Unable to create file Mapping");
                    }
                }

                _mapView = MapViewOfFile(_mappedFile, FileMapAccess.FileMapRead, 0, 0, _memSize);

                if (_mapView == IntPtr.Zero)
                {
                    throw new Exception("Unable to map view of file");
                }

                _buffer = new byte[_memSize];

                _bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);

                byte* p = (byte*)_mapView.ToPointer();

                _unmanagedStream = new UnmanagedMemoryStream(p, _memSize, _memSize, FileAccess.Read);

            }
        }


        public MumbleLinkedMem Read()
        {
            _unmanagedStream.Position = 0;
            _unmanagedStream.Read(_buffer, 0, _memSize);
            return
                (MumbleLinkedMem)Marshal.PtrToStructure(_bufferHandle.AddrOfPinnedObject(), typeof(MumbleLinkedMem));
        }

        public void Dispose()
        {
            if (_unmanagedStream != null)
                _unmanagedStream.Dispose();
            if (_bufferHandle != null)
                _bufferHandle.Free();
            if (_mapView != IntPtr.Zero)
            {
                UnmapViewOfFile(_mapView);
                _mapView = IntPtr.Zero;
            }
            if (_mappedFile != IntPtr.Zero)
            {
                CloseHandle(_mappedFile);
                _mappedFile = IntPtr.Zero;
            }
        }
    }
}