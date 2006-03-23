//
// System.Drawing.Text.FontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//		Sanjay Gupta (gsanjay@novell.com)
//		Peter Dennis Bartok (pbartok@novell.com)
//
//
// Copyright (C) 2004 - 2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Text {

	public abstract class FontCollection : IDisposable {
		
		internal IntPtr nativeFontCollection = IntPtr.Zero;
				
		internal FontCollection ()
		{
		}
        
		internal FontCollection (IntPtr ptr)
		{
			nativeFontCollection = ptr;
		}

		// methods
		public void Dispose()
		{
			Dispose (true);
			GC.SuppressFinalize (true);
		}

		protected virtual void Dispose (bool disposing)
		{		
			OperatingSystem osInfo = Environment.OSVersion;

			if (nativeFontCollection != IntPtr.Zero) {
				if ((int) osInfo.Platform == 128 || (int) osInfo.Platform == 4) {
					GDIPlus.GdipDeletePrivateFontCollection (ref nativeFontCollection);
					nativeFontCollection = IntPtr.Zero;
				}       
			}
		}

		// properties
		public FontFamily[] Families
		{
			get { 
				int found;
				int returned;
				Status status;
				FontFamily[] families;
				IntPtr[] result;
				
				status = GDIPlus.GdipGetFontCollectionFamilyCount (nativeFontCollection, out found);
				GDIPlus.CheckStatus (status);
				
				result = new IntPtr[found];
				status = GDIPlus.GdipGetFontCollectionFamilyList(nativeFontCollection, found, result, out returned);
				   
				families = new FontFamily [returned];
				for ( int i = 0; i < returned ; i++) {
					families[i] = new FontFamily(result[i]);
				}
           
				return families;               
			}
		}

		~FontCollection()
		{
			Dispose (false);
		}

	}

}
