//
// System.Windows.Forms.ErrorProvider
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	 Dennis Hayes(dennish@raytek.com)
// (C) 2002 Ximian, Inc
//

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
using System.Drawing;
using System.Runtime.Remoting;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>
using System.ComponentModel;
	public class ErrorProvider : Component, IExtenderProvider {
		internal string dataMember;
		ContainerControl parentControl;
		//
		//  --- Constructor
		//
		[MonoTODO]
		public ErrorProvider(ContainerControl parentControl)
		{
			dataMember = "";
			this.parentControl = parentControl;
		}

		[MonoTODO]
		public ErrorProvider() {
			dataMember = "";
			this.parentControl = null;
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override ISite  Site {
			set {
				//FIXME:
				base.Site = value;
			}
		}

		[MonoTODO]
		public int BlinkRate {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public ErrorBlinkStyle BlinkStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		internal ContainerControl cc;//FIXME: just to get it to run
		[MonoTODO]
		public ContainerControl ContainerControl {
			get {
				 return cc;
			}
			set {
				cc = value;
			}
		}

		[MonoTODO]
		public string DataMember {
			get {
				return dataMember;
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public object DataSource {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public Icon Icon {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		//
		//  --- Protected Methods
		//
		
		[MonoTODO]
		protected override void Dispose(bool disposing) { // .NET V1.1 Beta
			base.Dispose(disposing);
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void BindToDataAndErrors(object newDataSource, string newDataMember)
		{
			//FIXME:
		}

		[MonoTODO]
		public bool CanExtend(object extendee)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string GetError(Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ErrorIconAlignment GetIconAlignment(Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int GetIconPadding(Control control)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetError(Control control,string value)
		{
			//FIXME:
		}

		[MonoTODO]
		public void SetIconAlignment(Control control, ErrorIconAlignment value)
		{
			//FIXME:
		}

		[MonoTODO]
		public void SetIconPadding(Control control, int padding)
		{
			//FIXME:
		}

		[MonoTODO]
		public void UpdateBinding()
		{
			//FIXME:
		}
	 }
}
