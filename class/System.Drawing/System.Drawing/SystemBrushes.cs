//
// System.Drawing.SystemBrushes.cs
//
// Author:
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System;

namespace System.Drawing
{
	/// <summary>
	/// Summary description for SystemBrushes.
	/// </summary>
	public sealed class SystemBrushes
	{
		private SystemBrushes()
		{
		}

		public static Brush ActiveBorder
		{	
			get {
				return new SolidBrush(SystemColors.ActiveBorder);
			}
		}

		public static Brush ActiveCaption
		{	
			get {
				return new SolidBrush(SystemColors.ActiveCaption);
			}
		}

		public static Brush ActiveCaptionText
		{	
			get {
				return new SolidBrush(SystemColors.ActiveCaptionText);
			}
		}

		public static Brush AppWorkspace
		{	
			get {
				return new SolidBrush(SystemColors.AppWorkspace);
			}
		}

		public static Brush Control {
			get {
				return new SolidBrush(SystemColors.Control);
			}
		}
		
		public static Brush ControlLight {
			get {
				return new SolidBrush(SystemColors.ControlLight);
			}
		}
		
		public static Brush ControlLightLight {
			get {
				return new SolidBrush(SystemColors.ControlLightLight);
			}
		}

		public static Brush ControlText {
			get {
				return new SolidBrush(SystemColors.ControlText);
			}
		}

		public static Brush Highlight {
			get {
				return new SolidBrush(SystemColors.Highlight);
			}
		}

		public static Brush HighlightText {
			get {
				return new SolidBrush(SystemColors.HighlightText);
			}
		}

		public static Brush Window {
			get {
				return new SolidBrush(SystemColors.Window);
			}
		}
		public static Brush WindowText {
			get {
				return new SolidBrush(SystemColors.WindowText);
			}
		}
	}
}
