///////////////////////////////////////////////////////////////////////////////
// CapStructures
//
// This software is released into the public domain.  You are free to use it
// in any way you like, except that you may not sell this source code.
//
// This software is provided "as is" with no expressed or implied warranty.
// I accept no liability for any damage or loss of business that this software
// may cause.
// 
// This source code is originally written by Tamir Khason (see http://blogs.microsoft.co.il/blogs/tamir
// or http://www.codeplex.com/wpfcap).
// 
// Modifications are made by Geert van Horrik (CatenaLogic, see http://blog.catenalogic.com) 
// 
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WpfCap
{
    [ComVisible(false)]
    internal enum PinDirection
    {
        Input,
        Output
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    internal class AMMediaType : IDisposable
    {
        public Guid MajorType;

        public Guid SubType;

        [MarshalAs(UnmanagedType.Bool)]
        public bool FixedSizeSamples = true;

        [MarshalAs(UnmanagedType.Bool)]
        public bool TemporalCompression;

        public int SampleSize = 1;

        public Guid FormatType;

        public IntPtr unkPtr;

        public int FormatSize;

        public IntPtr FormatPtr;

        ~AMMediaType()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // remove me from the Finalization queue 
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (FormatSize != 0)
                Marshal.FreeCoTaskMem(FormatPtr);
            if (unkPtr != IntPtr.Zero)
                Marshal.Release(unkPtr);
        }
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    internal class PinInfo
    {
        public IBaseFilter Filter;

        public PinDirection Direction;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Name;
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    internal struct VideoInfoHeader
    {
        public RECT SrcRect;

        public RECT TargetRect;

        public int BitRate;

        public int BitErrorRate;

        public long AverageTimePerFrame;

        public BitmapInfoHeader BmiHeader;
    }
    
    [ComVisible(false), StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct BitmapInfoHeader
    {
        public int Size;

        public int Width;

        public int Height;

        public short Planes;

        public short BitCount;

        public int Compression;

        public int ImageSize;

        public int XPelsPerMeter;

        public int YPelsPerMeter;

        public int ColorsUsed;

        public int ColorsImportant;
    }

    [ComVisible(false), StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }
}
