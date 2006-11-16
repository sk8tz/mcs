//
// System.Drawing.PageSettings.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Herve Poussineau (hpoussineau@fr.st)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Runtime.InteropServices;

namespace System.Drawing.Printing
{
#if NET_2_0
	[Serializable]
#else
	[ComVisible (false)]
#endif
	public class PageSettings : ICloneable
	{
		internal bool _Color;
		internal bool _Landscape;
		internal PaperSize _PaperSize;
		internal PaperSource _PaperSource;
		internal PrinterResolution _PrinterResolution;
		float _HardMarginX;
		float _HardMarginY;
		RectangleF _PrintableArea;
		// create a new default Margins object (is 1 inch for all margins)
		Margins _Margins = new Margins();
		PrinterSettings _PrinterSettings;
		
		public PageSettings() : this(new PrinterSettings())
		{
		}
		
		public PageSettings(PrinterSettings printerSettings)
		{
			PrinterSettings = printerSettings;
			
			Color = printerSettings.DefaultPageSettings._Color;
			Landscape = printerSettings.DefaultPageSettings._Landscape;
			PaperSize = printerSettings.DefaultPageSettings._PaperSize;
			PaperSource = printerSettings.DefaultPageSettings._PaperSource;
			PrinterResolution = printerSettings.DefaultPageSettings._PrinterResolution;
		}
		
		// used by PrinterSettings.DefaultPageSettings
		internal PageSettings(PrinterSettings printerSettings, bool color, bool landscape, PaperSize paperSize, PaperSource paperSource, PrinterResolution printerResolution)
		{
			PrinterSettings = printerSettings;
			
			_Color = color;
			_Landscape = landscape;
			_PaperSize = paperSize;
			_PaperSource = paperSource;
			_PrinterResolution = printerResolution;
		}

		//props
		public Rectangle Bounds{
			get{
				int width = this.PaperSize.Width;
				int height = this.PaperSize.Height;
				
				width -= this.Margins.Left + this.Margins.Right;
				height -= this.Margins.Top + this.Margins.Bottom;
				
				if (this.Landscape) {
					// swap width and height
					int tmp = width;
					width = height;
					height = tmp;
				}
				return new Rectangle (Margins.Left, Margins.Top, width, height);
			}
		}
		
		public bool Color{
			get{
				if (!this._PrinterSettings.IsValid)
					throw new InvalidPrinterException(this._PrinterSettings);
				return _Color;
			}
			set{
				_Color = value;
			}
		}
		
		public bool Landscape {
			get{
				if (!this._PrinterSettings.IsValid)
					throw new InvalidPrinterException(this._PrinterSettings);
				return _Landscape;
			}
			set{
				_Landscape = value;
			}
		}
		
		public Margins Margins{
			get{
				if (!this._PrinterSettings.IsValid)
					throw new InvalidPrinterException(this._PrinterSettings);
				return _Margins;
			}
			set{
				_Margins = value;
			}
		}
		
		public PaperSize PaperSize{
			get{
				if (!this._PrinterSettings.IsValid)
					throw new InvalidPrinterException(this._PrinterSettings);
				return _PaperSize;
			}
			set{
				_PaperSize = value;
			}
		}
		
		public PaperSource PaperSource{
			get{
				if (!this._PrinterSettings.IsValid)
					throw new InvalidPrinterException(this._PrinterSettings);
				return _PaperSource;
			}
			set{
				_PaperSource = value;
			}
		}
		
		public PrinterResolution PrinterResolution{
			get{
				if (!this._PrinterSettings.IsValid)
					throw new InvalidPrinterException(this._PrinterSettings);
				return _PrinterResolution;
			}
			set{
				_PrinterResolution = value;
			}
		}
		
		public PrinterSettings PrinterSettings{
			get{
				return _PrinterSettings;
			}
			set{
				_PrinterSettings = value;
			}
		}		
#if NET_2_0
		public float HardMarginX {
			get {
				return _HardMarginX;
			}
		}
		
		public float HardMarginY {
			get {
				return _HardMarginY;
			}
		}
		
		public RectangleF PrintableArea {
			get {
				return _PrintableArea;
			}
		}
#endif


		public object Clone ()
		{
			// We do a deep copy
			PrinterResolution pres = new PrinterResolution (_PrinterResolution.X, _PrinterResolution.Y, _PrinterResolution.Kind);
			PaperSource psource = new PaperSource (_PaperSource.SourceName, _PaperSource.Kind);
			PaperSize psize = new PaperSize (_PaperSize.PaperName, _PaperSize.Width, _PaperSize.Height);
			psize.SetKind (_PaperSize.Kind);

			PageSettings ps = new PageSettings (PrinterSettings, Color, Landscape,
					psize, psource, pres);
			ps.Margins = (Margins) _Margins.Clone ();
			return ps;
		}


		[MonoTODO("PageSettings.CopyToHdevmode")]
		public void CopyToHdevmode (IntPtr hdevmode){
			throw new NotImplementedException ();
		}


		[MonoTODO("PageSettings.SetHdevmode")]
		public void SetHdevmode (IntPtr hdevmode){
			throw new NotImplementedException ();
		}	

		public override string ToString(){
			string ret = "[PageSettings: Color={0}";
			ret += ", Landscape={1}";
			ret += ", Margins={2}";
			ret += ", PaperSize={3}";
			ret += ", PaperSource={4}";
			ret += ", PrinterResolution={5}";
			ret += "]";
			
			return String.Format(ret, this.Color, this.Landscape, this.Margins, this.PaperSize, this.PaperSource, this.PrinterResolution);
		}
	}
}
