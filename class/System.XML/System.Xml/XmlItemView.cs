//
// XmlItemView.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
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
#if NET_2_0

using System;
using System.ComponentModel;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlItemView : ICustomTypeDescriptor, IXPathNavigable
	{
		[MonoTODO]
		public XPathNavigator CreateNavigator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual AttributeCollection GetAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetClassName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual string GetComponentName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual TypeConverter GetConverter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptor GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PropertyDescriptor GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptorCollection GetEvents ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual EventDescriptorCollection GetEvents (Attribute [] attributes)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PropertyDescriptorCollection GetProperties ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual PropertyDescriptorCollection GetProperties (Attribute [] attrs)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual object this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual object this [string fieldName] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual XmlItemViewCollection XmlItemViewCollection {
			get { throw new NotImplementedException (); }
		}
	}
}
#endif
