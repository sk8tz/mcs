//
// System.Drawing.Region.cs
//
// Author:
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002 Ximian, Inc
//
using System;
using System.Drawing.Drawing2D;
namespace System.Drawing
{
	namespace Win32Impl {

		/// <summary>
		/// Summary description for Region.
		/// </summary>
		/// 
		//[ComVisible(false)]
		public sealed class Region : IRegion //, MarshalByRefObject, IDisposable
		{
			public Region() 
			{

			}

			public Region( Rectangle rect) 
			{

			}
			//[comVisible(false)]
			//public Region(GraphicsPath path) {
			//}
		}
	}
}
