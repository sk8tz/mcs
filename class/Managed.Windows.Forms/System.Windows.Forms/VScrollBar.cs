//
// System.Windows.Forms.HScrollBar.cs
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
// Copyright (C) 2004, Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez	jordi@ximian.com
//
//
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: VScrollBar.cs,v $
// Revision 1.2  2004/08/23 20:10:03  jordi
// fixes properties and methods
//
// Revision 1.1  2004/07/13 15:33:46  jordi
// vertical and hort. classes commit
//
//

using System.Drawing;

namespace System.Windows.Forms 
{

	public class VScrollBar : ScrollBar 
	{
		public new event EventHandler RightToLeftChanged;

		public VScrollBar()
		{			
			vert = true;
		}

		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set {
				if (RightToLeft == value)
					return;

				RightToLeft = value;

				if (RightToLeftChanged != null)
					RightToLeftChanged (this, EventArgs.Empty);

			}
		}

		protected override Size DefaultSize {
			get { return new Size (13,80); }
		}	

		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}		
	}
}
