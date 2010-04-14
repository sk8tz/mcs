//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;

namespace System.Xaml
{
	public class XamlObjectReader : XamlReader
	{
		class NSList : List<NamespaceDeclaration>
		{
			public NSList (XamlNodeType ownerType, params NamespaceDeclaration [] nsdecls)
				: base (nsdecls)
			{
				OwnerType = ownerType;
			}
			
			public XamlNodeType OwnerType { get; set; }

			public IEnumerator<NamespaceDeclaration> GetEnumerator ()
			{
				return new NSEnumerator (this, base.GetEnumerator ());
			}
		}

		class NSEnumerator : IEnumerator<NamespaceDeclaration>
		{
			NSList list;
			IEnumerator<NamespaceDeclaration> e;

			public NSEnumerator (NSList list, IEnumerator<NamespaceDeclaration> e)
			{
				this.list= list;
				this.e = e;
			}
			
			public XamlNodeType OwnerType {
				get { return list.OwnerType; }
			}

			public void Dispose ()
			{
			}

			public bool MoveNext ()
			{
				return e.MoveNext ();
			}

			public NamespaceDeclaration Current {
				get { return e.Current; }
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public void Reset ()
			{
				throw new NotSupportedException ();
			}
		}
	
		public XamlObjectReader (object instance)
			: this (instance, new XamlSchemaContext (null, null), null)
		{
		}

		public XamlObjectReader (object instance, XamlObjectReaderSettings settings)
			: this (instance, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlObjectReader (object instance, XamlSchemaContext schemaContext)
			: this (instance, schemaContext, null)
		{
		}

		public XamlObjectReader (object instance, XamlSchemaContext schemaContext, XamlObjectReaderSettings settings)
		{
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			if (instance != null) {
				// check type validity. Note that some checks are done at Read() phase.
				var type = instance.GetType ();
				if (!type.IsPublic)
					throw new XamlObjectReaderException (String.Format ("instance type '{0}' must be public and non-nested.", type));
			}
			this.instance = instance;
			sctx = schemaContext;
			this.settings = settings;
		}

		object instance;
		XamlSchemaContext sctx;
		XamlObjectReaderSettings settings;

		Stack<XamlType> types = new Stack<XamlType> ();
		Stack<object> objects = new Stack<object> ();
		Stack<IEnumerator<XamlMember>> members_stack = new Stack<IEnumerator<XamlMember>> ();
		IEnumerator<NamespaceDeclaration> namespaces;
		XamlNodeType node_type = XamlNodeType.None;
		bool is_eof;

		public object Instance {
			get { return instance; }
		}

		public override bool IsEof {
			get { return is_eof; }
		}

		public override XamlMember Member {
			get { return NodeType == XamlNodeType.StartMember ? members_stack.Peek ().Current : null; }
		}

		public override NamespaceDeclaration Namespace {
			get { return NodeType == XamlNodeType.NamespaceDeclaration ? namespaces.Current : null; }
		}

		public override XamlNodeType NodeType {
			get { return node_type; }
		}

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public override XamlType Type {
			get { return NodeType == XamlNodeType.StartObject ? types.Peek () : null; }
		}

		public override object Value {
			get { return NodeType == XamlNodeType.Value ? objects.Peek () : null; }
		}
		
		public override bool Read ()
		{
			if (IsDisposed)
				throw new ObjectDisposedException ("reader");
			if (IsEof)
				return false;
			IEnumerator<XamlMember> members;
			switch (NodeType) {
			case XamlNodeType.None:
			default:
				// -> namespaces
				namespaces = new NSList (XamlNodeType.StartObject, new NamespaceDeclaration (XamlLanguage.Xaml2006Namespace, "x")).GetEnumerator ();
				namespaces.MoveNext ();
				node_type = XamlNodeType.NamespaceDeclaration;
				return true;

			case XamlNodeType.NamespaceDeclaration:
				if (namespaces.MoveNext ())
					return true;
				node_type = ((NSEnumerator) namespaces).OwnerType; // StartObject or StartMember
				if (node_type == XamlNodeType.StartObject)
					StartNextObject ();
				else
					StartNextMemberOrNamespace ();
				return true;

			case XamlNodeType.StartObject:
				var xt = types.Peek ();
				members = xt.GetAllReadWriteMembers ().GetEnumerator ();
				if (members.MoveNext ()) {
					members_stack.Push (members);
					StartNextMemberOrNamespace ();
					return true;
				}
				else
					node_type = XamlNodeType.EndObject;
				return true;

			case XamlNodeType.StartMember:
				members = members_stack.Peek ();
				var obj = members.Current.GetPropertyOrFieldValue (objects.Peek ());
				objects.Push (obj);
				node_type = XamlNodeType.Value;
				return true;

			case XamlNodeType.Value:
				objects.Pop ();
				node_type = XamlNodeType.EndMember;
				return true;

			case XamlNodeType.GetObject:
				// how do we get here?
				throw new NotImplementedException ();

			case XamlNodeType.EndMember:
				members = members_stack.Peek ();
				if (members.MoveNext ()) {
					members_stack.Push (members);
					StartNextMemberOrNamespace ();
				} else {
					members_stack.Pop ();
					node_type = XamlNodeType.EndObject;
				}
				return true;

			case XamlNodeType.EndObject:
				// It might be either end of the entire object tree or just the end of an object value.
				types.Pop ();
				objects.Pop ();
				if (objects.Count == 0) {
					node_type = XamlNodeType.None;
					is_eof = true;
					return false;
				}
				members = members_stack.Peek ();
				if (members.MoveNext ()) {
					StartNextMemberOrNamespace ();
					return true;
				}
				// then, move to the end of current object.
				members_stack.Pop ();
				node_type = XamlNodeType.EndObject;
				return true;
			}
		}

		// This assumes that the next member is already on current position on current iterator.
		void StartNextMemberOrNamespace ()
		{
			// FIXME: there might be NamespaceDeclarations.
			node_type = XamlNodeType.StartMember;
		}

		void StartNextObject ()
		{
			var xm = members_stack.Count > 0 ? members_stack.Peek ().Current : null;
			var obj = xm != null ? xm.GetPropertyOrFieldValue (objects.Peek ()) : instance;
			var xt = obj != null ? SchemaContext.GetXamlType (obj.GetType ()) : XamlLanguage.Null;

			// FIXME: enable these lines.
			// FIXME: if there is an applicable instance descriptor, then it could be still valid.
			//var type = xt.UnderlyingType;
			//if (type.GetConstructor (System.Type.EmptyTypes) == null)
			//	throw new XamlObjectReaderException (String.Format ("Type {0} has no default constructor or an instance descriptor.", type));

			objects.Push (obj);
			types.Push (xt);
			node_type = XamlNodeType.StartObject;
		}
	}
}
