//
// System.Drawing.Text.FontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//			Sanjay Gupta (gsanjay@novell.com)
//
using System;
using System.Drawing;

namespace System.Drawing.Text {

	public abstract class FontCollection : IDisposable {
		
		//internal IFontCollection implementation;
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
			System.GC.SuppressFinalize (this);
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			//Nothing for now
		}

		// properties
		public FontFamily[] Families
		{
			get { 
				int found;
				int returned;
				Status status;
				
				Console.WriteLine("came to Families method of FontCollection");
				
				status = GDIPlus.GdipGetFontCollectionFamilyCount( nativeFontCollection, out found);
				if (status != Status.Ok){
					throw new Exception ("Error calling GDIPlus.GdipGetFontCollectionFamilyCount: " +status);
				}
				
				Console.WriteLine("FamilyFont count returned in Families method of FontCollection " + found);
				
				IntPtr [] family = new IntPtr [found];
				//FontFamily [] familyList = new FontFamily[found];
				
				status = GDIPlus.GdipGetFontCollectionFamilyList( nativeFontCollection, found, out family, out returned);
				if (status != Status.Ok){
					Console.WriteLine("Error calling GDIPlus.GdipGetFontCollectionFamilyList: " +status);
					throw new Exception ("Error calling GDIPlus.GdipGetFontCollectionFamilyList: " +status);					
				}
				
				FontFamily [] familyList = new FontFamily[returned];
				Console.WriteLine("No of FontFamilies returned in Families method of FontCollection " + returned);
				for( int i = 0 ; i < returned ; i++ )
					familyList [i] = new FontFamily( family[i] );
				//return implementation.Families;
				return familyList; 
			}
		}

		~FontCollection()
		{
			Dispose (false);
		}

	}

}
