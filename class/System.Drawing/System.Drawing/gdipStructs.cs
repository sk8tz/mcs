//
// System.Drawing.gdipStructs.cs
//
// Author: 
// Alexandre Pigolkine (pigolkine@gmx.de)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;

namespace System.Drawing {
	[StructLayout(LayoutKind.Sequential)]
	internal struct GdiplusStartupInput
	{
                uint 		GdiplusVersion;
                IntPtr 		DebugEventCallback;
                int             SuppressBackgroundThread;
                int 		SuppressExternalCodecs;
    
    	internal static GdiplusStartupInput MakeGdiplusStartupInput ()
    	{
    		GdiplusStartupInput result = new GdiplusStartupInput ();
        	result.GdiplusVersion = 1;
        	result.DebugEventCallback = IntPtr.Zero;
        	result.SuppressBackgroundThread = 0;
        	result.SuppressExternalCodecs = 0;
        	return result;
    	}
    	
        }
    
	[StructLayout(LayoutKind.Sequential)]
	internal struct GdiplusStartupOutput
	{
                internal IntPtr 	NotificationHook;
                internal IntPtr		NotificationUnhook;
                
    	internal static GdiplusStartupOutput MakeGdiplusStartupOutput ()
    	{
    		GdiplusStartupOutput result = new GdiplusStartupOutput ();
    		result.NotificationHook = result.NotificationUnhook = IntPtr.Zero;
        	return result;
    	}
	}
	
		
	[StructLayout(LayoutKind.Sequential)]
	internal struct GdiColorPalette
	{
   		internal int Flags;             // Palette flags
    		internal int Count;             // Number of color entries    		
		//internal int[] Entries;     		
    	}
    	
    	[StructLayout(LayoutKind.Sequential)]
    	internal struct  GdiColorMap 
    	{
    		internal int from;
    		internal int to;
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal  struct LOGFONTA  
	{
		internal int    lfHeight;
		internal uint   lfWidth;
		internal uint   lfEscapement;
		internal uint   lfOrientation;
		internal uint   lfWeight;
		internal byte   lfItalic;
		internal byte   lfUnderline;
		internal byte   lfStrikeOut;
		internal byte   lfCharSet;
		internal byte   lfOutPrecision;
		internal byte   lfClipPrecision;
		internal byte   lfQuality;
		internal byte   lfPitchAndFamily;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
		internal string lfFaceName;
	}  
}

