///////////////////////////////////////////////////////////////////////////////
// CapInterfaces
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
using System.Runtime.InteropServices.ComTypes;

namespace WpfCap
{
    [ComImport, Guid("56A868A9-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IGraphBuilder
    {
        [PreserveSig]
        int AddFilter([In] IBaseFilter filter, [In, MarshalAs(UnmanagedType.LPWStr)] string name);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter filter);

        [PreserveSig]
        int EnumFilters([Out] out IntPtr enumerator);

        [PreserveSig]
        int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string name, [Out] out IBaseFilter filter);

        [PreserveSig]
        int ConnectDirect([In] IPin pinOut, [In] IPin pinIn, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int Reconnect([In] IPin pin);

        [PreserveSig]
        int Disconnect([In] IPin pin);

        [PreserveSig]
        int SetDefaultSyncSource();

        [PreserveSig]
        int Connect([In] IPin pinOut, [In] IPin pinIn);

        [PreserveSig]
        int Render([In] IPin pinOut);

        [PreserveSig]
        int RenderFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string file,
            [In, MarshalAs(UnmanagedType.LPWStr)] string playList);

        [PreserveSig]
        int AddSourceFilter(
            [In, MarshalAs(UnmanagedType.LPWStr)] string fileName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string filterName,
            [Out] out IBaseFilter filter);

        [PreserveSig]
        int SetLogFile(IntPtr hFile);

        [PreserveSig]
        int Abort();

        [PreserveSig]
        int ShouldOperationContinue();
    }

    [ComImport, Guid("56A86895-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IBaseFilter
    {
        [PreserveSig]
        int GetClassID([Out] out Guid ClassID);

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Run(long start);

        [PreserveSig]
        int GetState(int milliSecsTimeout, [Out] out int filterState);

        [PreserveSig]
        int SetSyncSource([In] IntPtr clock);

        [PreserveSig]
        int GetSyncSource([Out] out IntPtr clock);

        [PreserveSig]
        int EnumPins([Out] out IEnumPins enumPins);

        [PreserveSig]
        int FindPin([In, MarshalAs(UnmanagedType.LPWStr)] string id, [Out] out IPin pin);

        [PreserveSig]
        int QueryFilterInfo([Out] FilterInfo filterInfo);

        [PreserveSig]
        int JoinFilterGraph([In] IFilterGraph graph, [In, MarshalAs(UnmanagedType.LPWStr)] string name);

        [PreserveSig]
        int QueryVendorInfo([Out, MarshalAs(UnmanagedType.LPWStr)] out string vendorInfo);
    }

    [ComImport, Guid("56A86891-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPin
    {
        [PreserveSig]
        int Connect([In] IPin receivePin, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int ReceiveConnection([In] IPin receivePin, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int Disconnect();

        [PreserveSig]
        int ConnectedTo([Out] out IPin pin);

        [PreserveSig]
        int ConnectionMediaType([Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int QueryPinInfo([Out, MarshalAs(UnmanagedType.LPStruct)] PinInfo pinInfo);

        [PreserveSig]
        int QueryDirection(out PinDirection pinDirection);

        [PreserveSig]
        int QueryId([Out, MarshalAs(UnmanagedType.LPWStr)] out string id);

        [PreserveSig]
        int QueryAccept([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int EnumMediaTypes(IntPtr enumerator);

        [PreserveSig]
        int QueryInternalConnections(IntPtr apPin, [In, Out] ref int nPin);

        [PreserveSig]
        int EndOfStream();

        [PreserveSig]
        int BeginFlush();

        [PreserveSig]
        int EndFlush();

        [PreserveSig]
        int NewSegment(long start, long stop, double rate);
    }

    [ComImport, Guid("56A86892-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IEnumPins
    {
        [PreserveSig]
        int Next([In] int cPins,
           [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IPin[] pins,
           [Out] out int pinsFetched);

        [PreserveSig]
        int Skip([In] int cPins);

        [PreserveSig]
        int Reset();

        [PreserveSig]
        int Clone([Out] out IEnumPins enumPins);
    }

    [ComImport,Guid("56A8689F-0AD4-11CE-B03A-0020AF0BA770"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFilterGraph
    {
        [PreserveSig]
        int AddFilter([In] IBaseFilter filter, [In, MarshalAs(UnmanagedType.LPWStr)] string name);

        [PreserveSig]
        int RemoveFilter([In] IBaseFilter filter);

        [PreserveSig]
        int EnumFilters([Out] out IntPtr enumerator);

        [PreserveSig]
        int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string name, [Out] out IBaseFilter filter);

        [PreserveSig]
        int ConnectDirect([In] IPin pinOut, [In] IPin pinIn, [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int Reconnect([In] IPin pin);

        [PreserveSig]
        int Disconnect([In] IPin pin);

        [PreserveSig]
        int SetDefaultSyncSource();
    }

    [ComImport,Guid("55272A00-42CB-11CE-8135-00AA004BB851"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyBag
    {
        [PreserveSig]
        int Read(
            [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
            [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pVar,
            [In] IntPtr pErrorLog);

         [PreserveSig]
        int Write(
            [In, MarshalAs(UnmanagedType.LPWStr)] string propertyName,
            [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
    }

    [ComImport,Guid("6B652FFF-11FE-4FCE-92AD-0266B5D7C78F"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISampleGrabber
    {
        [PreserveSig]
        int SetOneShot([In, MarshalAs(UnmanagedType.Bool)] bool oneShot);

        [PreserveSig]
        int SetMediaType([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int GetConnectedMediaType([Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType mediaType);

        [PreserveSig]
        int SetBufferSamples([In, MarshalAs(UnmanagedType.Bool)] bool bufferThem);

        [PreserveSig]
        int GetCurrentBuffer(ref int bufferSize, IntPtr buffer);

        [PreserveSig]
        int GetCurrentSample(IntPtr sample);

        [PreserveSig]
        int SetCallback(ISampleGrabberCB callback, int whichMethodToCallback);
    }

    [ComImport, Guid("0579154A-2B53-4994-B0D0-E773148EFF85"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISampleGrabberCB
    {
        [PreserveSig]
        int SampleCB(double sampleTime, IntPtr sample);

        [PreserveSig]
        int BufferCB(double sampleTime, IntPtr buffer, int bufferLen);
    }

    [ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICreateDevEnum
    {
        [PreserveSig]
        int CreateClassEnumerator([In] ref Guid type, [Out] out IEnumMoniker enumMoniker, [In] int flags);
    }

    [ComImport,Guid("56A868B4-0AD4-11CE-B03A-0020AF0BA770"),InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IVideoWindow
    {
        [PreserveSig]
        int put_Caption(string caption);

        [PreserveSig]
        int get_Caption([Out] out string caption);

         [PreserveSig]
        int put_WindowStyle(int windowStyle);

        [PreserveSig]
        int get_WindowStyle(out int windowStyle);

        [PreserveSig]
        int put_WindowStyleEx(int windowStyleEx);

        [PreserveSig]
        int get_WindowStyleEx(out int windowStyleEx);

        [PreserveSig]
        int put_AutoShow([In, MarshalAs(UnmanagedType.Bool)] bool autoShow);

        [PreserveSig]
        int get_AutoShow([Out, MarshalAs(UnmanagedType.Bool)] out bool autoShow);

        [PreserveSig]
        int put_WindowState(int windowState);

        [PreserveSig]
        int get_WindowState(out int windowState);

        [PreserveSig]
        int put_BackgroundPalette([In, MarshalAs(UnmanagedType.Bool)] bool backgroundPalette);

        [PreserveSig]
        int get_BackgroundPalette([Out, MarshalAs(UnmanagedType.Bool)] out bool backgroundPalette);

        [PreserveSig]
        int put_Visible([In, MarshalAs(UnmanagedType.Bool)] bool visible);

        [PreserveSig]
        int get_Visible([Out, MarshalAs(UnmanagedType.Bool)] out bool visible);

        [PreserveSig]
        int put_Left(int left);

        [PreserveSig]
        int get_Left(out int left);

        [PreserveSig]
        int put_Width(int width);

        [PreserveSig]
        int get_Width(out int width);

        [PreserveSig]
        int put_Top(int top);

        [PreserveSig]
        int get_Top(out int top);

        [PreserveSig]
        int put_Height(int height);

        [PreserveSig]
        int get_Height(out int height);

        [PreserveSig]
        int put_Owner(IntPtr owner);

        [PreserveSig]
        int get_Owner(out IntPtr owner);

        [PreserveSig]
        int put_MessageDrain(IntPtr drain);

        [PreserveSig]
        int get_MessageDrain(out IntPtr drain);

        [PreserveSig]
        int get_BorderColor(out int color);

        [PreserveSig]
        int put_BorderColor(int color);

        [PreserveSig]
        int get_FullScreenMode(
            [Out, MarshalAs(UnmanagedType.Bool)] out bool fullScreenMode);

        [PreserveSig]
        int put_FullScreenMode([In, MarshalAs(UnmanagedType.Bool)] bool fullScreenMode);

        [PreserveSig]
        int SetWindowForeground(int focus);

        [PreserveSig]
        int NotifyOwnerMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

        [PreserveSig]
        int SetWindowPosition(int left, int top, int width, int height);

        [PreserveSig]
        int GetWindowPosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int GetMinIdealImageSize(out int width, out int height);

        [PreserveSig]
        int GetMaxIdealImageSize(out int width, out int height);

        [PreserveSig]
        int GetRestorePosition(out int left, out int top, out int width, out int height);

        [PreserveSig]
        int HideCursor([In, MarshalAs(UnmanagedType.Bool)] bool hideCursor);

        [PreserveSig]
        int IsCursorHidden([Out, MarshalAs(UnmanagedType.Bool)] out bool hideCursor);
    }

    [ComImport, Guid("56A868B1-0AD4-11CE-B03A-0020AF0BA770"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    internal interface IMediaControl
    {
        [PreserveSig]
        int Run();

        [PreserveSig]
        int Pause();

        [PreserveSig]
        int Stop();

        [PreserveSig]
        int GetState(int timeout, out int filterState);

        [PreserveSig]
        int RenderFile(string fileName);

        [PreserveSig]
        int AddSourceFilter([In] string fileName, [Out, MarshalAs(UnmanagedType.IDispatch)] out object filterInfo);

        [PreserveSig]
        int get_FilterCollection(
            [Out, MarshalAs(UnmanagedType.IDispatch)] out object collection);

        [PreserveSig]
        int get_RegFilterCollection(
            [Out, MarshalAs(UnmanagedType.IDispatch)] out object collection);

        [PreserveSig]
        int StopWhenReady();
    }
}
