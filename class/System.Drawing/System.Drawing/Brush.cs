//
// System.Drawing.Brush.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing;

namespace System.Drawing {

	public abstract class Brush : MarshalByRefObject, ICloneable, IDisposable {

		internal IntPtr nativeObject;
		abstract public object Clone ();

                internal Brush ()
                { }
        
		internal Brush (IntPtr ptr)
		{
                        nativeObject = ptr;
		}
		
		internal IntPtr NativeObject{
			get{
				return nativeObject;
			}
			set	{
				nativeObject = value;
			}
		}
	

                internal Brush CreateBrush (IntPtr brush, System.Drawing.BrushType type)
                {
                        switch (type) {

                        case BrushType.BrushTypeSolidColor:
                                return new SolidBrush (brush);

                        default:
                                throw new NotImplementedException ();
                        }
                }

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// Nothing for now.
		}

		~Brush ()
		{
			Dispose (false);
		}
	}
}

