// THIS FILE AUTOMATICALLY GENERATED BY xpidl2cs.pl
// EDITING IS PROBABLY UNWISE
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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mono.Mozilla {

	[Guid ("a6cf9074-15b3-11d2-932e-00805f8add32")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface nsIDOMDOMImplementation {

#region nsIDOMDOMImplementation
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int hasFeature (
				   /*DOMString*/ HandleRef feature,
				   /*DOMString*/ HandleRef version, out bool ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createDocumentType (
				   /*DOMString*/ HandleRef qualifiedName,
				   /*DOMString*/ HandleRef publicId,
				   /*DOMString*/ HandleRef systemId,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMDocumentType ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createDocument (
				   /*DOMString*/ HandleRef namespaceURI,
				   /*DOMString*/ HandleRef qualifiedName,
				[MarshalAs (UnmanagedType.Interface)]   nsIDOMDocumentType doctype,[MarshalAs (UnmanagedType.Interface)]  out nsIDOMDocument ret);

#endregion
	}


	internal class nsDOMDOMImplementation {
		public static nsIDOMDOMImplementation GetProxy (Mono.WebBrowser.IWebBrowser control, nsIDOMDOMImplementation obj)
		{
			object o = Base.GetProxyForObject (control, typeof(nsIDOMDOMImplementation).GUID, obj.GetType (), obj);
			return o as nsIDOMDOMImplementation;
		}
	}
}
