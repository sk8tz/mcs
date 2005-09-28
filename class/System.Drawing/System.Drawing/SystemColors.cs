//
// System.Drawing.SystemColors.cs
//
// Copyright (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
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
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Peter Dennis Bartok (pbartok@novell.com)
//

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Drawing {
	public sealed class SystemColors
	{
		static private Color active_border = Color.FromArgbSystem (255, 131, 153, 177, "ActiveBorder", KnownColor.ActiveBorder);
		static private Color active_caption = Color.FromArgbSystem (255, 79, 101, 125, "ActiveCaption", KnownColor.ActiveCaption);
		static private Color active_caption_text = Color.FromArgbSystem (255, 255, 255, 255, "ActiveCaptionText", KnownColor.ActiveCaptionText);
		static private Color app_workspace = Color.FromArgbSystem (255, 128, 128, 128, "AppWorkspace", KnownColor.AppWorkspace);
		static private Color control = Color.FromArgbSystem (255, 192, 192, 192, "Control", KnownColor.Control);
		static private Color control_dark = Color.FromArgbSystem (255, 79, 101, 125, "ControlDark", KnownColor.ControlDark);
		static private Color control_dark_dark = Color.FromArgbSystem (255, 0, 0, 0, "ControlDarkDark", KnownColor.ControlDarkDark);
		static private Color control_light = Color.FromArgbSystem (255, 131, 153, 177, "ControlLight", KnownColor.ControlLight);
		static private Color control_light_light = Color.FromArgbSystem (255, 193, 204, 217, "ControlLightLight", KnownColor.ControlLightLight);
		static private Color control_text = Color.FromArgbSystem (255, 0, 0, 0, "ControlText", KnownColor.ControlText);
		static private Color desktop = Color.FromArgbSystem (255, 0, 0, 0, "Desktop", KnownColor.Desktop);
		static private Color gray_text = Color.FromArgbSystem (255, 79, 101, 125, "GrayText", KnownColor.GrayText);
		static private Color highlight = Color.FromArgbSystem (255, 0, 0, 128, "Highlight", KnownColor.Highlight);
		static private Color highlight_text = Color.FromArgbSystem (255, 255, 255, 255, "HighlightText", KnownColor.HighlightText);
		static private Color hot_track = Color.FromArgbSystem (255, 0, 0, 255, "HotTrack", KnownColor.HotTrack);
		static private Color inactive_border = Color.FromArgbSystem (255, 131, 153, 177, "InactiveBorder", KnownColor.InactiveBorder);
		static private Color inactive_caption = Color.FromArgbSystem (255, 128, 128, 128, "InactiveCaption", KnownColor.InactiveCaption);
		static private Color inactive_caption_text = Color.FromArgbSystem (255, 193, 204, 217, "InactiveCaptionText", KnownColor.InactiveCaptionText);
		static private Color info = Color.FromArgbSystem (255, 255, 255, 255, "Info", KnownColor.Info);
		static private Color info_text = Color.FromArgbSystem (255, 0, 0, 0, "InfoText", KnownColor.InfoText);
		static private Color menu = Color.FromArgbSystem (255, 131, 153, 177, "Menu", KnownColor.Menu);
		static private Color menu_text = Color.FromArgbSystem (255, 0, 0, 0, "MenuText", KnownColor.MenuText);
		static private Color scrollbar = Color.FromArgbSystem (255, 193, 204, 217, "ScrollBar", KnownColor.ScrollBar);
		static private Color window = Color.FromArgbSystem (255, 255, 255, 255, "Window", KnownColor.Window);
		static private Color window_frame = Color.FromArgbSystem (255, 0, 0, 0, "WindowFrame", KnownColor.WindowFrame);
		static private Color window_text = Color.FromArgbSystem (255, 0, 0, 0, "WindowText", KnownColor.WindowText);

		private enum GetSysColorIndex {
			COLOR_SCROLLBAR			= 0,
			COLOR_BACKGROUND		= 1,
			COLOR_ACTIVECAPTION		= 2,
			COLOR_INACTIVECAPTION		= 3,
			COLOR_MENU			= 4,
			COLOR_WINDOW			= 5,
			COLOR_WINDOWFRAME		= 6,
			COLOR_MENUTEXT			= 7,
			COLOR_WINDOWTEXT		= 8,
			COLOR_CAPTIONTEXT		= 9,
			COLOR_ACTIVEBORDER		= 10,
			COLOR_INACTIVEBORDER		= 11,
			COLOR_APPWORKSPACE		= 12,
			COLOR_HIGHLIGHT			= 13,
			COLOR_HIGHLIGHTTEXT		= 14,
			COLOR_BTNFACE			= 15,
			COLOR_BTNSHADOW			= 16,
			COLOR_GRAYTEXT			= 17,
			COLOR_BTNTEXT			= 18,
			COLOR_INACTIVECAPTIONTEXT	= 19,
			COLOR_BTNHIGHLIGHT		= 20,
			COLOR_3DDKSHADOW		= 21,
			COLOR_3DLIGHT			= 22,
			COLOR_INFOTEXT			= 23,
			COLOR_INFOBK			= 24,
			
			COLOR_HOTLIGHT			= 26,
			COLOR_GRADIENTACTIVECAPTION	= 27,
			COLOR_GRADIENTINACTIVECAPTION	= 28,
			COLOR_MENUHIGHLIGHT		= 29,
			COLOR_MENUBAR			= 30,

			COLOR_DESKTOP			= 1,
			COLOR_3DFACE			= 16,
			COLOR_3DSHADOW			= 16,
			COLOR_3DHIGHLIGHT		= 20,
			COLOR_3DHILIGHT			= 20,
			COLOR_BTNHILIGHT		= 20,

			COLOR_MAXVALUE			= 30,/* Maximum value */
		}       

		[DllImport ("user32.dll", EntryPoint="GetSysColor", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetSysColor(GetSysColorIndex index);

		private static Color GetSysColor(GetSysColorIndex index) {
			uint color;

			color = Win32GetSysColor(index);

			return Color.FromArgb((byte)(color & 0xff), (byte)((color >> 8) & 0xff), (byte)((color >> 16) & 0xff));
		}

		// When this method is called, we teach any new color(s) to the Color class
		private static void UpdateColors() {
			Color.UpdateKnownColor (active_border.A, active_border.R, active_border.G, active_border.B, "ActiveBorder", KnownColor.ActiveBorder);
			Color.UpdateKnownColor (active_caption.A, active_caption.R, active_caption.G, active_caption.B, "ActiveCaption", KnownColor.ActiveCaption);
			Color.UpdateKnownColor (active_caption_text.A, active_caption_text.R, active_caption_text.G, active_caption_text.B, "ActiveCaptionText", KnownColor.ActiveCaptionText);
			Color.UpdateKnownColor (app_workspace.A, app_workspace.R, app_workspace.G, app_workspace.B, "AppWorkspace", KnownColor.AppWorkspace);
			Color.UpdateKnownColor (control.A, control.R, control.G, control.B, "Control", KnownColor.Control);
			Color.UpdateKnownColor (control_dark.A, control_dark.R, control_dark.G, control_dark.B, "ControlDark", KnownColor.ControlDark);
			Color.UpdateKnownColor (control_dark_dark.A, control_dark_dark.R, control_dark_dark.G, control_dark_dark.B, "ControlDarkDark", KnownColor.ControlDarkDark);
			Color.UpdateKnownColor (control_light.A, control_light.R, control_light.G, control_light.B, "ControlLight", KnownColor.ControlLight);
			Color.UpdateKnownColor (control_light_light.A, control_light_light.R, control_light_light.G, control_light_light.B, "ControlLightLight", KnownColor.ControlLightLight);
			Color.UpdateKnownColor (control_text.A, control_text.R, control_text.G, control_text.B, "ControlText", KnownColor.ControlText);
			Color.UpdateKnownColor (desktop.A, desktop.R, desktop.G, desktop.B, "Desktop", KnownColor.Desktop);
			Color.UpdateKnownColor (gray_text.A, gray_text.R, gray_text.G, gray_text.B, "GrayText", KnownColor.GrayText);
			Color.UpdateKnownColor (highlight.A, highlight.R, highlight.G, highlight.B, "Highlight", KnownColor.Highlight);
			Color.UpdateKnownColor (highlight_text.A, highlight_text.R, highlight_text.G, highlight_text.B, "HighlightText", KnownColor.HighlightText);
			Color.UpdateKnownColor (hot_track.A, hot_track.R, hot_track.G, hot_track.B, "HotTrack", KnownColor.HotTrack);
			Color.UpdateKnownColor (inactive_border.A, inactive_border.R, inactive_border.G, inactive_border.B, "InactiveBorder", KnownColor.InactiveBorder);
			Color.UpdateKnownColor (inactive_caption.A, inactive_caption.R, inactive_caption.G, inactive_caption.B, "InactiveCaption", KnownColor.InactiveCaption);
			Color.UpdateKnownColor (inactive_caption_text.A, inactive_caption_text.R, inactive_caption_text.G, inactive_caption_text.B, "InactiveCaptionText", KnownColor.InactiveCaptionText);
			Color.UpdateKnownColor (info.A, info.R, info.G, info.B, "Info", KnownColor.Info);
			Color.UpdateKnownColor (info_text.A, info_text.R, info_text.G, info_text.B, "InfoText", KnownColor.InfoText);
			Color.UpdateKnownColor (menu.A, menu.R, menu.G, menu.B, "Menu", KnownColor.Menu);
			Color.UpdateKnownColor (menu_text.A, menu_text.R, menu_text.G, menu_text.B, "MenuText", KnownColor.MenuText);
			Color.UpdateKnownColor (scrollbar.A, scrollbar.R, scrollbar.G, scrollbar.B, "ScrollBar", KnownColor.ScrollBar);
			Color.UpdateKnownColor (window.A, window.R, window.G, window.B, "Window", KnownColor.Window);
			Color.UpdateKnownColor (window_frame.A, window_frame.R, window_frame.G, window_frame.B, "WindowFrame", KnownColor.WindowFrame);
			Color.UpdateKnownColor (window_text.A, window_text.R, window_text.G, window_text.B, "WindowText", KnownColor.WindowText);
		}

		static SystemColors () {
			// If we're on a Win32 platform we should behave like MS and pull the colors
			if (((int)Environment.OSVersion.Platform != 4) && ((int)Environment.OSVersion.Platform != 128)) {
				active_border = GetSysColor(GetSysColorIndex.COLOR_ACTIVEBORDER);
				active_caption = GetSysColor(GetSysColorIndex.COLOR_ACTIVECAPTION);
				active_caption_text = GetSysColor(GetSysColorIndex.COLOR_CAPTIONTEXT);
				app_workspace = GetSysColor(GetSysColorIndex.COLOR_APPWORKSPACE);
				control = GetSysColor(GetSysColorIndex.COLOR_BTNFACE);
				control_dark = GetSysColor(GetSysColorIndex.COLOR_BTNSHADOW);
				control_dark_dark = GetSysColor(GetSysColorIndex.COLOR_3DDKSHADOW);
				control_light = GetSysColor(GetSysColorIndex.COLOR_BTNHIGHLIGHT);
				control_light_light = GetSysColor(GetSysColorIndex.COLOR_3DLIGHT);
				control_text = GetSysColor(GetSysColorIndex.COLOR_BTNTEXT);
				desktop = GetSysColor(GetSysColorIndex.COLOR_DESKTOP);
				gray_text = GetSysColor(GetSysColorIndex.COLOR_GRAYTEXT);
				highlight = GetSysColor(GetSysColorIndex.COLOR_HIGHLIGHT);
				highlight_text = GetSysColor(GetSysColorIndex.COLOR_HIGHLIGHTTEXT);
				hot_track = GetSysColor(GetSysColorIndex.COLOR_HOTLIGHT);
				inactive_border = GetSysColor(GetSysColorIndex.COLOR_INACTIVEBORDER);
				inactive_caption = GetSysColor(GetSysColorIndex.COLOR_INACTIVECAPTION);
				inactive_caption_text = GetSysColor(GetSysColorIndex.COLOR_INACTIVECAPTIONTEXT);
				info = GetSysColor(GetSysColorIndex.COLOR_INFOBK);
				info_text = GetSysColor(GetSysColorIndex.COLOR_INFOTEXT);
				menu = GetSysColor(GetSysColorIndex.COLOR_MENU);
				menu_text = GetSysColor(GetSysColorIndex.COLOR_MENUTEXT);
				scrollbar = GetSysColor(GetSysColorIndex.COLOR_SCROLLBAR);
				window = GetSysColor(GetSysColorIndex.COLOR_WINDOW);
				window_frame = GetSysColor(GetSysColorIndex.COLOR_WINDOWFRAME);
				window_text = GetSysColor(GetSysColorIndex.COLOR_WINDOWTEXT);
			}
		}


		private SystemColors ()
		{
		}

		static public Color ActiveBorder
		{	
			get {
				return active_border;
			}
		}

		static public Color ActiveCaption
		{	
			get {
				return active_caption;
			}
		}

		static public Color ActiveCaptionText
		{	
			get {
				return active_caption_text;
			}
		}

		static public Color AppWorkspace
		{	
			get {
				return app_workspace;
			}
		}

		static public Color Control
		{	
			get {
				return control;
			}
		}

		static public Color ControlDark
		{	
			get {
				return control_dark;
			}
		}

		static public Color ControlDarkDark
		{	
			get {
				return control_dark_dark;
			}
		}

		static public Color ControlLight
		{	
			get {
				return control_light;
			}
		}

		static public Color ControlLightLight
		{	
			get {
				return control_light_light;
			}
		}

		static public Color ControlText
		{	
			get {
				return control_text;
			}
		}

		static public Color Desktop
		{	
			get {
				return desktop;
			}
		}

		static public Color GrayText
		{	
			get {
				return gray_text;
			}
		}

		static public Color Highlight
		{	
			get {
				return highlight;
			}
		}

		static public Color HighlightText
		{	
			get {
				return highlight_text;
			}
		}

		static public Color HotTrack
		{	
			get {
				return hot_track;
			}
		}

		static public Color InactiveBorder
		{	
			get {
				return inactive_border;
			}
		}

		static public Color InactiveCaption
		{	
			get {
				return inactive_caption;
			}
		}

		static public Color InactiveCaptionText
		{	
			get {
				return inactive_caption_text;
			}
		}

		static public Color Info
		{	
			get {
				return info;
			}
		}

		static public Color InfoText
		{	
			get {
				return info_text;
			}
		}

		static public Color Menu
		{	
			get {
				return menu;
			}
		}

		static public Color MenuText
		{	
			get {
				return menu_text;
			}
		}

		static public Color ScrollBar
		{	
			get {
				return scrollbar;
			}
		}

		static public Color Window
		{	
			get {
				return window;
			}
		}

		static public Color WindowFrame
		{	
			get {
				return window_frame;
			}
		}

		static public Color WindowText
		{	
			get {
				return window_text;
			}
		}
	}
}

