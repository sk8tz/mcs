// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software",, to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//


// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// X11 Version
namespace System.Windows.Forms {
	//
	// In the structures below, fields of type long are mapped to IntPtr.
	// This will work on all platforms where sizeof(long)==sizeof(void*), which
	// is almost all platforms except WIN64.
	//

	[StructLayout(LayoutKind.Sequential)]
	internal struct XAnyEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XKeyEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal IntPtr		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal int		state;
		internal int		keycode;
		internal bool		same_screen;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XButtonEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal IntPtr		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal int		state;
		internal int		button;
		internal bool		same_screen;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XMotionEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal IntPtr		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal int		state;
		internal byte		is_hint;
		internal bool		same_screen;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XCrossingEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		root;
		internal IntPtr		subwindow;
		internal IntPtr		time;
		internal int		x;
		internal int		y;
		internal int		x_root;
		internal int		y_root;
		internal NotifyMode	mode;
		internal NotifyDetail	detail;
		internal bool		same_screen;
		internal bool		focus;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XFocusChangeEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		mode;
		internal NotifyDetail	detail;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XKeymapEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal byte		key_vector0;
		internal byte		key_vector1;
		internal byte		key_vector2;
		internal byte		key_vector3;
		internal byte		key_vector4;
		internal byte		key_vector5;
		internal byte		key_vector6;
		internal byte		key_vector7;
		internal byte		key_vector8;
		internal byte		key_vector9;
		internal byte		key_vector10;
		internal byte		key_vector11;
		internal byte		key_vector12;
		internal byte		key_vector13;
		internal byte		key_vector14;
		internal byte		key_vector15;
		internal byte		key_vector16;
		internal byte		key_vector17;
		internal byte		key_vector18;
		internal byte		key_vector19;
		internal byte		key_vector20;
		internal byte		key_vector21;
		internal byte		key_vector22;
		internal byte		key_vector23;
		internal byte		key_vector24;
		internal byte		key_vector25;
		internal byte		key_vector26;
		internal byte		key_vector27;
		internal byte		key_vector28;
		internal byte		key_vector29;
		internal byte		key_vector30;
		internal byte		key_vector31;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XExposeEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		count;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XGraphicsExposeEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		drawable;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		count;
		internal int		major_code;
		internal int		minor_code;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XNoExposeEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		drawable;
		internal int		major_code;
		internal int		minor_code;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XVisibilityEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XCreateWindowEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		parent;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		border_width;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XDestroyWindowEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XUnmapEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal bool		from_configure;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XMapEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XMapRequestEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		parent;
		internal IntPtr		window;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XReparentEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal IntPtr		parent;
		internal int		x;
		internal int		y;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XConfigureEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		border_width;
		internal IntPtr		above;
		internal bool		override_redirect;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XGravityEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal int		x;
		internal int		y;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XResizeRequestEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		width;
		internal int		height;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XConfigureRequestEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		width;
		internal int		height;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XCirculateEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		xevent;
		internal IntPtr		window;
		internal int		place;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XCirculateRequestEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		parent;
		internal IntPtr		window;
		internal int		place;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XPropertyEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		atom;
		internal IntPtr		time;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionClearEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		selection;
		internal IntPtr		time;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionRequestEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		owner;
		internal IntPtr		requestor;
		internal IntPtr		selection;
		internal IntPtr		target;
		internal IntPtr		property;
		internal IntPtr		time;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSelectionEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		requestor;
		internal IntPtr		selection;
		internal IntPtr		target;
		internal IntPtr		property;
		internal IntPtr		time;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XColormapEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		colormap;
		internal bool		c_new;
		internal int		state;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XClientMessageEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal IntPtr		message_type;
		internal int		format;
		internal IntPtr		ptr1;
		internal IntPtr		ptr2;
		internal IntPtr		ptr3;
		internal IntPtr		ptr4;
		internal IntPtr		ptr5;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XMappingEvent {
		internal XEventName	type;
		internal IntPtr		serial;
		internal bool		send_event;
		internal IntPtr		display;
		internal IntPtr		window;
		internal int		request;
		internal int		first_keycode;
		internal int		count;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XErrorEvent {
		internal XEventName	type;
		internal IntPtr		display;
		internal IntPtr		resourceid;
		internal IntPtr		serial;
		internal byte		error_code;
		internal byte		request_code;
		internal byte		minor_code;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XTimerNotifyEvent {
		internal XEventName	type;
		internal EventHandler	handler;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XEventPad {
		internal IntPtr pad0;
		internal IntPtr pad1;
		internal IntPtr pad2;
		internal IntPtr pad3;
		internal IntPtr pad4;
		internal IntPtr pad5;
		internal IntPtr pad6;
		internal IntPtr pad7;
		internal IntPtr pad8;
		internal IntPtr pad9;
		internal IntPtr pad10;
		internal IntPtr pad11;
		internal IntPtr pad12;
		internal IntPtr pad13;
		internal IntPtr pad14;
		internal IntPtr pad15;
		internal IntPtr pad16;
		internal IntPtr pad17;
		internal IntPtr pad18;
		internal IntPtr pad19;
		internal IntPtr pad20;
		internal IntPtr pad21;
		internal IntPtr pad22;
		internal IntPtr pad23;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct XEvent {
		[ FieldOffset(0) ] internal XEventName type;
		[ FieldOffset(0) ] internal XAnyEvent AnyEvent;
		[ FieldOffset(0) ] internal XKeyEvent KeyEvent;
		[ FieldOffset(0) ] internal XButtonEvent ButtonEvent;
		[ FieldOffset(0) ] internal XMotionEvent MotionEvent;
		[ FieldOffset(0) ] internal XCrossingEvent CrossingEvent;
		[ FieldOffset(0) ] internal XFocusChangeEvent FocusChangeEvent;
		[ FieldOffset(0) ] internal XExposeEvent ExposeEvent;
		[ FieldOffset(0) ] internal XGraphicsExposeEvent GraphicsExposeEvent;
		[ FieldOffset(0) ] internal XNoExposeEvent NoExposeEvent;
		[ FieldOffset(0) ] internal XVisibilityEvent VisibilityEvent;
		[ FieldOffset(0) ] internal XCreateWindowEvent CreateWindowEvent;
		[ FieldOffset(0) ] internal XDestroyWindowEvent DestroyWindowEvent;
		[ FieldOffset(0) ] internal XUnmapEvent UnmapEvent;
		[ FieldOffset(0) ] internal XMapEvent MapEvent;
		[ FieldOffset(0) ] internal XMapRequestEvent MapRequestEvent;
		[ FieldOffset(0) ] internal XReparentEvent ReparentEvent;
		[ FieldOffset(0) ] internal XConfigureEvent ConfigureEvent;
		[ FieldOffset(0) ] internal XGravityEvent GravityEvent;
		[ FieldOffset(0) ] internal XResizeRequestEvent ResizeRequestEvent;
		[ FieldOffset(0) ] internal XConfigureRequestEvent ConfigureRequestEvent;
		[ FieldOffset(0) ] internal XCirculateEvent CirculateEvent;
		[ FieldOffset(0) ] internal XCirculateRequestEvent CirculateRequestEvent;
		[ FieldOffset(0) ] internal XPropertyEvent PropertyEvent;
		[ FieldOffset(0) ] internal XSelectionClearEvent SelectionClearEvent;
		[ FieldOffset(0) ] internal XSelectionRequestEvent SelectionRequestEvent;
		[ FieldOffset(0) ] internal XSelectionEvent SelectionEvent;
		[ FieldOffset(0) ] internal XColormapEvent ColormapEvent;
		[ FieldOffset(0) ] internal XClientMessageEvent ClientMessageEvent;
		[ FieldOffset(0) ] internal XMappingEvent MappingEvent;
		[ FieldOffset(0) ] internal XErrorEvent ErrorEvent;
		[ FieldOffset(0) ] internal XKeymapEvent KeymapEvent;
		[ FieldOffset(0) ] internal XTimerNotifyEvent TimerNotifyEvent;

		//[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=24)]
		//[ FieldOffset(0) ] internal int[] pad;
		[ FieldOffset(0) ] internal XEventPad Pad;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XSetWindowAttributes {
		internal IntPtr		background_pixmap;
		internal IntPtr		background_pixel;
		internal IntPtr		border_pixmap;
		internal IntPtr		border_pixel;
		internal Gravity	bit_gravity;
		internal Gravity	win_gravity;
		internal int		backing_store;
		internal IntPtr		backing_planes;
		internal IntPtr		backing_pixel;
		internal bool		save_under;
		internal IntPtr		event_mask;
		internal IntPtr		do_not_propagate_mask;
		internal bool		override_redirect;
		internal IntPtr		colormap;
		internal IntPtr		cursor;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XWindowAttributes {
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		border_width;
		internal int		depth;
		internal IntPtr		visual;
		internal IntPtr		root;
		internal int		c_class;
		internal Gravity	bit_gravity;
		internal Gravity	win_gravity;
		internal int		backing_store;
		internal IntPtr		backing_planes;
		internal IntPtr		backing_pixel;
		internal bool		save_under;
		internal IntPtr		colormap;
		internal bool		map_installed;
		internal MapState	map_state;
		internal IntPtr		all_event_masks;
		internal IntPtr		your_event_mask;
		internal IntPtr		do_not_propagate_mask;
		internal bool		override_direct;
		internal IntPtr		screen;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XTextProperty {
		internal string		value;
		internal IntPtr		encoding;
		internal int		format;
		internal IntPtr		nitems;
	}

	internal enum XWindowClass {
		InputOutput	= 1,
		InputOnly	= 2
	}

	internal enum XEventName {
		KeyPress                = 2,
		KeyRelease              = 3,
		ButtonPress             = 4,
		ButtonRelease           = 5,
		MotionNotify            = 6,
		EnterNotify             = 7,
		LeaveNotify             = 8,
		FocusIn                 = 9,
		FocusOut                = 10,
		KeymapNotify            = 11,
		Expose                  = 12,
		GraphicsExpose          = 13,
		NoExpose                = 14,
		VisibilityNotify        = 15,
		CreateNotify            = 16,
		DestroyNotify           = 17,
		UnmapNotify             = 18,
		MapNotify               = 19,
		MapRequest              = 20,
		ReparentNotify          = 21,
		ConfigureNotify         = 22,
		ConfigureRequest        = 23,
		GravityNotify           = 24,
		ResizeRequest           = 25,
		CirculateNotify         = 26,
		CirculateRequest        = 27,
		PropertyNotify          = 28,
		SelectionClear          = 29,
		SelectionRequest        = 30,
		SelectionNotify         = 31,
		ColormapNotify          = 32,
		ClientMessage		= 33,
		MappingNotify		= 34,
		TimerNotify		= 100,

		LASTEvent
	}

	[Flags]
	internal enum SetWindowValuemask {
		BackPixmap	= 1,
		BackPixel	= 2,
		BorderPixmap	= 4,
		BorderPixel	= 8,
		BitGravity	= 16,
		WinGravity	= 32,
		BackingStore	= 64,
		BackingPlanes	= 128,
		BackingPixel	= 256,
		OverrideRedirect = 512,
		SaveUnder	= 1024,
		EventMask	= 2048,
		DontPropagate	= 4096,
		ColorMap	= 8192,
		Cursor	= 16384
	}

	internal enum CreateWindowArgs {
		CopyFromParent	= 0,
		ParentRelative	= 1,
		InputOutput	= 1,
		InputOnly	= 2
	}

	internal enum Gravity {
		ForgetGravity	= 0,
		NorthWestGravity= 1,
		NorthGravity	= 2,
		NorthEastGravity= 3,
		WestGravity	= 4,
		CenterGravity	= 5,
		EastGravity	= 6,
		SouthWestGravity= 7,
		SouthGravity	= 8,
		SouthEastGravity= 9,
		StaticGravity	= 10
	}

	internal enum XKeySym {
		XK_BackSpace	= 0xFF08,
		XK_Tab		= 0xFF09,
		XK_Clear	= 0xFF0B,
		XK_Return	= 0xFF0D,
		XK_Home		= 0xFF50,
		XK_Left		= 0xFF51,
		XK_Up		= 0xFF52,
		XK_Right	= 0xFF53,
		XK_Down		= 0xFF54,
		XK_Page_Up	= 0xFF55,
		XK_Page_Down	= 0xFF56,
		XK_End		= 0xFF57,
		XK_Begin	= 0xFF58,
		XK_Menu		= 0xFF67,
		XK_Shift_L	= 0xFFE1,
		XK_Shift_R	= 0xFFE2,
		XK_Control_L	= 0xFFE3,
		XK_Control_R	= 0xFFE4,
		XK_Caps_Lock	= 0xFFE5,
		XK_Shift_Lock	= 0xFFE6,	
		XK_Meta_L	= 0xFFE7,
		XK_Meta_R	= 0xFFE8,
		XK_Alt_L	= 0xFFE9,
		XK_Alt_R	= 0xFFEA,
		XK_Super_L	= 0xFFEB,
		XK_Super_R	= 0xFFEC,
		XK_Hyper_L	= 0xFFED,
		XK_Hyper_R	= 0xFFEE,
	}

	[Flags]
	internal enum EventMask {
		NoEventMask		= 0,
		KeyPressMask		= 1<<0,
		KeyReleaseMask		= 1<<1,
		ButtonPressMask		= 1<<2,
		ButtonReleaseMask	= 1<<3,
		EnterWindowMask		= 1<<4,
		LeaveWindowMask		= 1<<5,
		PointerMotionMask	= 1<<6,
		PointerMotionHintMask	= 1<<7,
		Button1MotionMask	= 1<<8,
		Button2MotionMask	= 1<<9,
		Button3MotionMask	= 1<<10,
		Button4MotionMask	= 1<<11,
		Button5MotionMask	= 1<<12,
		ButtonMotionMask	= 1<<13,
		KeymapStateMask		= 1<<14,
		ExposureMask		= 1<<15,
		VisibilityChangeMask	= 1<<16,
		StructureNotifyMask	= 1<<17,
		ResizeRedirectMask	= 1<<18,
		SubstructureNotifyMask	= 1<<19,
		SubstructureRedirectMask= 1<<20,
		FocusChangeMask		= 1<<21,
		PropertyChangeMask	= 1<<22,
		ColormapChangeMask	= 1<<23,
		OwnerGrabButtonMask	= 1<<24
	}

	internal enum GrabMode {
		GrabModeSync		= 0,
		GrabModeAsync		= 1
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XStandardColormap {
		internal IntPtr		colormap;
		internal IntPtr		red_max;
		internal IntPtr		red_mult;
		internal IntPtr		green_max;
		internal IntPtr		green_mult;
		internal IntPtr		blue_max;
		internal IntPtr		blue_mult;
		internal IntPtr		base_pixel;
		internal IntPtr		visualid;
		internal IntPtr		killid;
	}

	[StructLayout(LayoutKind.Sequential, Pack=2)]
	internal struct XColor {
		internal IntPtr		pixel;
		internal ushort		red;
		internal ushort		green;
		internal ushort		blue;
		internal byte		flags;
		internal byte		pad;
	}

	internal enum Atom {
		AnyPropertyType		= 0,
		XA_PRIMARY		= 1,
		XA_SECONDARY		= 2,
		XA_ARC			= 3,
		XA_ATOM			= 4,
		XA_BITMAP		= 5,
		XA_CARDINAL		= 6,
		XA_COLORMAP		= 7,
		XA_CURSOR		= 8,
		XA_CUT_BUFFER0		= 9,
		XA_CUT_BUFFER1		= 10,
		XA_CUT_BUFFER2		= 11,
		XA_CUT_BUFFER3		= 12,
		XA_CUT_BUFFER4		= 13,
		XA_CUT_BUFFER5		= 14,
		XA_CUT_BUFFER6		= 15,
		XA_CUT_BUFFER7		= 16,
		XA_DRAWABLE		= 17,
		XA_FONT			= 18,
		XA_INTEGER		= 19,
		XA_PIXMAP		= 20,
		XA_POINT		= 21,
		XA_RECTANGLE		= 22,
		XA_RESOURCE_MANAGER	= 23,
		XA_RGB_COLOR_MAP	= 24,
		XA_RGB_BEST_MAP		= 25,
		XA_RGB_BLUE_MAP		= 26,
		XA_RGB_DEFAULT_MAP	= 27,
		XA_RGB_GRAY_MAP		= 28,
		XA_RGB_GREEN_MAP	= 29,
		XA_RGB_RED_MAP		= 30,
		XA_STRING		= 31,
		XA_VISUALID		= 32,
		XA_WINDOW		= 33,
		XA_WM_COMMAND		= 34,
		XA_WM_HINTS		= 35,
		XA_WM_CLIENT_MACHINE	= 36,
		XA_WM_ICON_NAME		= 37,
		XA_WM_ICON_SIZE		= 38,
		XA_WM_NAME		= 39,
		XA_WM_NORMAL_HINTS	= 40,
		XA_WM_SIZE_HINTS	= 41,
		XA_WM_ZOOM_HINTS	= 42,
		XA_MIN_SPACE		= 43,
		XA_NORM_SPACE		= 44,
		XA_MAX_SPACE		= 45,
		XA_END_SPACE		= 46,
		XA_SUPERSCRIPT_X	= 47,
		XA_SUPERSCRIPT_Y	= 48,
		XA_SUBSCRIPT_X		= 49,
		XA_SUBSCRIPT_Y		= 50,
		XA_UNDERLINE_POSITION	= 51,
		XA_UNDERLINE_THICKNESS	= 52,
		XA_STRIKEOUT_ASCENT	= 53,
		XA_STRIKEOUT_DESCENT	= 54,
		XA_ITALIC_ANGLE		= 55,
		XA_X_HEIGHT		= 56,
		XA_QUAD_WIDTH		= 57,
		XA_WEIGHT		= 58,
		XA_POINT_SIZE		= 59,
		XA_RESOLUTION		= 60,
		XA_COPYRIGHT		= 61,
		XA_NOTICE		= 62,
		XA_FONT_NAME		= 63,
		XA_FAMILY_NAME		= 64,
		XA_FULL_NAME		= 65,
		XA_CAP_HEIGHT		= 66,
		XA_WM_CLASS		= 67,
		XA_WM_TRANSIENT_FOR	= 68,

		XA_LAST_PREDEFINED	= 68
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XScreen {
		internal IntPtr		ext_data;
		internal IntPtr		display;
		internal IntPtr		root;
		internal int		width;
		internal int		height;
		internal int		mwidth;
		internal int		mheight;
		internal int		ndepths;
		internal IntPtr		depths;
		internal int		root_depth;
		internal IntPtr		root_visual;
		internal IntPtr		default_gc;
		internal IntPtr		cmap;
		internal IntPtr		white_pixel;
		internal IntPtr		black_pixel;
		internal int		max_maps;
		internal int		min_maps;
		internal int		backing_store;
		internal bool		save_unders;
		internal IntPtr	    root_input_mask;
	}

	[Flags]
	internal enum ChangeWindowFlags {
		CWX			= 1<<0,
		CWY			= 1<<1,
		CWWidth			= 1<<2,
		CWHeight		= 1<<3,
		CWBorderWidth		= 1<<4,
		CWSibling		= 1<<5,
		CWStackMode		= 1<<6
	}

	internal enum StackMode {
		Above			= 0,
		Below			= 1,
		TopIf			= 2,
		BottomIf		= 3,
		Opposite		= 4
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct XWindowChanges {
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal int		border_width;
		internal IntPtr		sibling;
		internal StackMode	stack_mode;
	}	

	[Flags]
	internal enum ColorFlags {
		DoRed			= 1<<0,
		DoGreen			= 1<<1,
		DoBlue			= 1<<2
	}

	internal enum NotifyMode {
		NotifyNormal		= 0,
		NotifyGrab		= 1,
		NotifyUngrab		= 2
	}

	internal enum NotifyDetail {
		NotifyAncestor		= 0,
		NotifyVirtual		= 1,
		NotifyInferior		= 2,
		NotifyNonlinear		= 3,
		NotifyNonlinearVirtual	= 4,
		NotifyPointer		= 5,
		NotifyPointerRoot	= 6,
		NotifyDetailNone	= 7
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MotifWmHints {
		internal IntPtr		flags;
		internal IntPtr		functions;
		internal IntPtr	    decorations;
		internal IntPtr		input_mode;
		internal IntPtr		status;
	}

	[Flags]
	internal enum MotifFlags {
		Functions		= 1,
		Decorations		= 2,
		InputMode		= 4,
		Status			= 8
	}

	[Flags]
	internal enum MotifFunctions {
		All			= 0x01,
		Resize			= 0x02,
		Move			= 0x04,
		Minimize		= 0x08,
		Maximize		= 0x10,
		Close			= 0x20
	}

	[Flags]
	internal enum MotifDecorations {
		All			= 0x01,
		Border			= 0x02,
		ResizeH			= 0x04,
		Title			= 0x08,
		Menu			= 0x10,
		Minimize		= 0x20,
		Maximize		= 0x40,
		
	}

	[Flags]
	internal enum MotifInputMode {
		Modeless		= 0,
		ApplicationModal	= 1,
		SystemModal		= 2,
		FullApplicationMondal	= 3
	}

        internal enum KeyMasks {
                ShiftMask               = (1 << 0),
                LockMask 		= (1 << 1),
                ControlMask		= (1 << 2)
        }

	[StructLayout (LayoutKind.Sequential)]
	internal struct XModifierKeymap {
		public int max_keypermod;
		public IntPtr modifiermap;
	} 

	internal enum PropertyMode {
		Replace			= 0,
		Prepend			= 1,
		Append			= 2
	}

	[StructLayout (LayoutKind.Sequential)]
	internal struct XKeyBoardState {
		int key_click_percent;
		int bell_percent;
		uint bell_pitch, bell_duration;
		IntPtr led_mask;
		int global_auto_repeat;
		AutoRepeats auto_repeats;

		[StructLayout (LayoutKind.Explicit)]
			struct AutoRepeats {
			[FieldOffset (0)]
			byte first;
				
			[FieldOffset (31)]
			byte last;
		}
	}

	[Flags]
	internal enum GCFunction {
		GCFunction              = 1<<0,
		GCPlaneMask             = 1<<1,
		GCForeground            = 1<<2,
		GCBackground            = 1<<3,
		GCLineWidth             = 1<<4,
		GCLineStyle             = 1<<5,
		GCCapStyle              = 1<<6,
		GCJoinStyle             = 1<<7,
		GCFillStyle             = 1<<8,
		GCFillRule              = 1<<9, 
		GCTile                  = 1<<10,
		GCStipple               = 1<<11,
		GCTileStipXOrigin       = 1<<12,
		GCTileStipYOrigin       = 1<<13,
		GCFont                  = 1<<14,
		GCSubwindowMode         = 1<<15,
		GCGraphicsExposures     = 1<<16,
		GCClipXOrigin           = 1<<17,
		GCClipYOrigin           = 1<<18,
		GCClipMask              = 1<<19,
		GCDashOffset            = 1<<20,
		GCDashList              = 1<<21,
		GCArcMode               = 1<<22
	}

	internal enum GCJoinStyle {
		JoinMiter		= 0,
		JoinRound		= 1,
		JoinBevel		= 2
	}

	internal enum GCLineStyle {
		LineSolid		= 0,
		LineOnOffDash		= 1,
		LineDoubleDash		= 2
	}

	internal enum GCCapStyle {
		CapNotLast		= 0,
		CapButt			= 1,
		CapRound		= 2,
		CapProjecting		= 3
	}

	internal enum GCFillStyle {
		FillSolid		= 0,
		FillTiled		= 1,
		FillStippled		= 2,
		FillOpaqueStppled	= 3
	}

	internal enum GCFillRule {
		EvenOddRule		= 0,
		WindingRule		= 1
	}

	internal enum GCArcMode {
		ArcChord		= 0,
		ArcPieSlice		= 1
	}

	internal enum GCSubwindowMode {
		ClipByChildren		= 0,
		IncludeInferiors	= 1
	}

	[StructLayout (LayoutKind.Sequential)]
	internal struct XGCValues {
		internal GXFunction		function;
		internal IntPtr			plane_mask;
		internal IntPtr			foreground;
		internal IntPtr			background;
		internal int			line_width;
		internal GCLineStyle		line_style;
		internal GCCapStyle		cap_style;
		internal GCJoinStyle		join_style;
		internal GCFillStyle		fill_style;
		internal GCFillRule		fill_rule;
		internal GCArcMode		arc_mode;
		internal IntPtr			tile;
		internal IntPtr			stipple;
		internal int			ts_x_origin;
		internal int			ts_y_origin;
		internal IntPtr			font;
		internal GCSubwindowMode	subwindow_mode;
		internal bool			graphics_exposures;
		internal int			clip_x_origin;
		internal int			clib_y_origin;
		internal IntPtr			clip_mask;
		internal int			dash_offset;
		internal byte			dashes;
	}

	internal enum GXFunction {
		GXclear				= 0x0,		/* 0 */
		GXand                   	= 0x1,		/* src AND dst */
		GXandReverse            	= 0x2,		/* src AND NOT dst */
		GXcopy                  	= 0x3,		/* src */
		GXandInverted           	= 0x4,		/* NOT src AND dst */
		GXnoop                  	= 0x5,		/* dst */
		GXxor                   	= 0x6,		/* src XOR dst */
		GXor                    	= 0x7,		/* src OR dst */
		GXnor                   	= 0x8,		/* NOT src AND NOT dst */
		GXequiv                 	= 0x9,		/* NOT src XOR dst */
		GXinvert                	= 0xa,		/* NOT dst */
		GXorReverse             	= 0xb,		/* src OR NOT dst */
		GXcopyInverted          	= 0xc,		/* NOT src */
		GXorInverted            	= 0xd,		/* NOT src OR dst */
		GXnand                  	= 0xe,		/* NOT src OR NOT dst */
		GXset                   	= 0xf		/* 1 */
	}

	internal enum NetWindowManagerState {
		Remove				= 0,
		Add				= 1,
		Toggle				= 2
	}

	internal enum RevertTo {
		None				= 0,
		PointerRoot			= 1,
		Parent				= 2
	}

	internal enum MapState {
		IsUnmapped			= 0,
		IsUnviewable			= 1,
		IsViewable			= 2
	}

	internal enum CursorFontShape {
		XC_X_cursor			= 0,
		XC_arrow			= 2,
		XC_based_arrow_down		= 4,
		XC_based_arrow_up		= 6,
		XC_boat				= 8,
		XC_bogosity			= 10,
		XC_bottom_left_corner		= 12,
		XC_bottom_right_corner		= 14,
		XC_bottom_side			= 16,
		XC_bottom_tee			= 18,
		XC_box_spiral			= 20,
		XC_center_ptr			= 22,

		XC_circle			= 24,
		XC_clock			= 26,
		XC_coffee_mug			= 28,
		XC_cross			= 30,
		XC_cross_reverse		= 32,
		XC_crosshair			= 34,
		XC_diamond_cross		= 36,
		XC_dot				= 38,
		XC_dotbox			= 40,
		XC_double_arrow			= 42,
		XC_draft_large			= 44,
		XC_draft_small			= 46,

		XC_draped_box			= 48,
		XC_exchange			= 50,
		XC_fleur			= 52,
		XC_gobbler			= 54,
		XC_gumby			= 56,
		XC_hand1			= 58,
		XC_hand2			= 60,
		XC_heart			= 62,
		XC_icon				= 64,
		XC_iron_cross			= 66,
		XC_left_ptr			= 68,
		XC_left_side			= 70,

		XC_left_tee			= 72,
		XC_left_button			= 74,
		XC_ll_angle			= 76,
		XC_lr_angle			= 78,
		XC_man				= 80,
		XC_middlebutton			= 82,
		XC_mouse			= 84,
		XC_pencil			= 86,
		XC_pirate			= 88,
		XC_plus				= 90,
		XC_question_arrow		= 92,
		XC_right_ptr			= 94,

		XC_right_side			= 96,
		XC_right_tee			= 98,
		XC_rightbutton			= 100,
		XC_rtl_logo			= 102,
		XC_sailboat			= 104,
		XC_sb_down_arrow		= 106,
		XC_sb_h_double_arrow		= 108,
		XC_sb_left_arrow		= 110,
		XC_sb_right_arrow		= 112,
		XC_sb_up_arrow			= 114,
		XC_sb_v_double_arrow		= 116,
		XC_sb_shuttle			= 118,

		XC_sizing			= 120,
		XC_spider			= 122,
		XC_spraycan			= 124,
		XC_star				= 126,
		XC_target			= 128,
		XC_tcross			= 130,
		XC_top_left_arrow		= 132,
		XC_top_left_corner		= 134,
		XC_top_right_corner		= 136,
		XC_top_side			= 138,
		XC_top_tee			= 140,
		XC_trek				= 142,

		XC_ul_angle			= 144,
		XC_umbrella			= 146,
		XC_ur_angle			= 148,
		XC_watch			= 150,
		XC_xterm			= 152,
		XC_num_glyphs			= 154
	}
}

