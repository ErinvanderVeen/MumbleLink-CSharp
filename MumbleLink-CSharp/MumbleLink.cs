using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MumbleLink_CSharp
{

    public class MumbleLink : IDisposable
    {
        private const string Name = "MumbleLink";

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
                _memSize = Marshal.SizeOf(typeof(MumbleLinkedMemory));

                _mappedFile = NativeMethods.OpenFileMapping(FileMapAccess.FileMapRead, false, Name);

                if (_mappedFile == IntPtr.Zero)
                {
                    _mappedFile = NativeMethods.CreateFileMapping(IntPtr.Zero, IntPtr.Zero, FileMapProtection.PageReadWrite, 0,
                        _memSize, Name);
                    if (_mappedFile == IntPtr.Zero)
                    {
                        throw new Exception("Unable to create file Mapping");
                    }
                }

                _mapView = NativeMethods.MapViewOfFile(_mappedFile, FileMapAccess.FileMapRead, 0, 0, _memSize);

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


        public MumbleLinkedMemory Read()
        {
            _unmanagedStream.Position = 0;
            _unmanagedStream.Read(_buffer, 0, _memSize);
            return
                (MumbleLinkedMemory)Marshal.PtrToStructure(_bufferHandle.AddrOfPinnedObject(), typeof(MumbleLinkedMemory));
        }


        //Context is not outputted, because, it is a binary blob that should be treated as a distinct structure
        public override string ToString()
        {
            var linkedMemory = Read();


            var str = new StringBuilder();

            str.AppendLine("UiVersion : " + linkedMemory.UiVersion);
            str.AppendLine("UiTick : " + linkedMemory.UiTick);
            str.AppendFormat("FAvatarPosition : [{0}, {1}, {2}]\n", linkedMemory.FAvatarPosition[0],
                linkedMemory.FAvatarPosition[1], linkedMemory.FAvatarPosition[2]);
            str.AppendFormat("FAvatarFront : [{0}, {1}, {2}]\n", linkedMemory.FAvatarFront[0],
                linkedMemory.FAvatarFront[1], linkedMemory.FAvatarFront[2]);
            str.AppendFormat("FAvatarTop : [{0}, {1}, {2}]\n", linkedMemory.FAvatarTop[0],
                linkedMemory.FAvatarTop[1], linkedMemory.FAvatarTop[2]);
            str.AppendLine("Name : " + new string(linkedMemory.Name));
            str.AppendFormat("FCameraPosition : [{0}, {1}, {2}]\n", linkedMemory.FCameraPosition[0],
                linkedMemory.FCameraPosition[1], linkedMemory.FCameraPosition[2]);
            str.AppendFormat("FCameraFront : [{0}, {1}, {2}]\n", linkedMemory.FCameraFront[0],
                linkedMemory.FCameraFront[1], linkedMemory.FCameraFront[2]);
            str.AppendFormat("FCameraTop : [{0}, {1}, {2}]\n", linkedMemory.FCameraTop[0],
                linkedMemory.FCameraTop[1], linkedMemory.FCameraTop[2]);

            str.AppendLine("Identity : " + new string(linkedMemory.Identity));

            str.AppendLine("ContextLen : " + linkedMemory.ContextLen);

            str.AppendLine("Description : " + new string(linkedMemory.Description));

            return str.ToString();
        }

        public void Dispose()
        {
            if (_unmanagedStream != null)
                _unmanagedStream.Dispose();
            if (_bufferHandle != null)
                _bufferHandle.Free();
            if (_mapView != IntPtr.Zero)
            {
                NativeMethods.UnmapViewOfFile(_mapView);
                _mapView = IntPtr.Zero;
            }
            if (_mappedFile != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(_mappedFile);
                _mappedFile = IntPtr.Zero;
            }
        }
    }
}