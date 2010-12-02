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
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Xaml
{
	internal struct XamlObjectNodeIterator
	{
		static readonly XamlObject null_object = new XamlObject (XamlLanguage.Null, null);

		public XamlObjectNodeIterator (object root, XamlSchemaContext schemaContext, IValueSerializerContext vctx)
		{
			ctx = schemaContext;
			this.root = root;
			value_serializer_ctx = vctx;
		}
		
		XamlSchemaContext ctx;
		object root;
		IValueSerializerContext value_serializer_ctx;
		
		PrefixLookup PrefixLookup {
			get { return (PrefixLookup) value_serializer_ctx.GetService (typeof (INamespacePrefixLookup)); }
		}
		XamlNameResolver NameResolver {
			get { return (XamlNameResolver) value_serializer_ctx.GetService (typeof (IXamlNameResolver)); }
		}

		public XamlSchemaContext SchemaContext {
			get { return ctx; }
		}
		
		XamlType GetType (object obj)
		{
			return obj == null ? XamlLanguage.Null : ctx.GetXamlType (obj.GetType ());
		}
		
		// returns StartObject, StartMember, Value, EndMember and EndObject. (NamespaceDeclaration is not included)
		public IEnumerable<XamlNodeInfo> GetNodes ()
		{
			var xobj = new XamlObject (GetType (root), root);
			foreach (var node in GetNodes (null, xobj))
				yield return node;
		}
		
		IEnumerable<XamlNodeInfo> GetNodes (XamlMember xm, XamlObject xobj)
		{
			return GetNodes (xm, xobj, null, false);
		}

		IEnumerable<XamlNodeInfo> GetNodes (XamlMember xm, XamlObject xobj, XamlType overrideMemberType, bool partOfPositionalParameters)
		{
			// collection items: each item is exposed as a standalone object that has StartObject, EndObject and contents.
			if (xm == XamlLanguage.Items) {
				foreach (var xn in GetItemsNodes (xm, xobj))
					yield return xn;
				yield break;
			}
			
			// Arguments: each argument is written as a standalone object
			if (xm == XamlLanguage.Arguments) {
				foreach (var argm in xobj.Type.GetSortedConstructorArguments ()) {
					var argv = argm.Invoker.GetValue (xobj.GetRawValue ());
					var xarg = new XamlObject (argm.Type, argv);
					foreach (var cn in GetNodes (null, xarg))
						yield return cn;
				}
				yield break;
			}

			// PositionalParameters: items are from constructor arguments, written as Value node sequentially. Note that not all of them are in simple string value. Also, null values are not written as NullExtension
			if (xm == XamlLanguage.PositionalParameters) {
				foreach (var argm in xobj.Type.GetSortedConstructorArguments ()) {
					foreach (var cn in GetNodes (argm, new XamlObject (argm.Type, xobj.GetMemberValue (argm)), null, true))
						yield return cn;
				}
				yield break;
			}

			if (xm == XamlLanguage.Initialization) {
				yield return new XamlNodeInfo (TypeExtensionMethods.GetStringValue (xobj.Type, xm, xobj.GetRawValue (), value_serializer_ctx));
				yield break;
			}

			// Value - only for non-top-level node (thus xm != null)
			if (xm != null) {
				// overrideMemberType is (so far) used for XamlLanguage.Key.
				var xtt = overrideMemberType ?? xm.Type;
				if (!xtt.IsMarkupExtension && // this condition is to not serialize MarkupExtension whose type has TypeConverterAttribute (e.g. StaticExtension) as a string.
				    (xtt.IsContentValue (value_serializer_ctx) || xm.IsContentValue (value_serializer_ctx))) {
					// though null value is special: it is written as a standalone object.
					var val = xobj.GetRawValue ();
					if (val == null) {
						if (!partOfPositionalParameters)
							foreach (var xn in GetNodes (null, null_object))
								yield return xn;
						else
							yield return new XamlNodeInfo (String.Empty);
					}
					else
						yield return new XamlNodeInfo (TypeExtensionMethods.GetStringValue (xtt, xm, val, value_serializer_ctx));
					yield break;
				}
			}

			// collection items: return GetObject and Items.
			if (xm != null && xm.Type.IsCollection && !xm.IsWritePublic) {
				yield return new XamlNodeInfo (XamlNodeType.GetObject, xobj);
				// Write Items member only when there are items (i.e. do not write it if it is empty).
				var xnm = new XamlNodeMember (xobj, XamlLanguage.Items);
				var en = GetNodes (XamlLanguage.Items, xnm.Value).GetEnumerator ();
				if (en.MoveNext ()) {
					yield return new XamlNodeInfo (XamlNodeType.StartMember, xnm);
					do {
						yield return en.Current;
					} while (en.MoveNext ());
					yield return new XamlNodeInfo (XamlNodeType.EndMember, xnm);
				}
				yield return new XamlNodeInfo (XamlNodeType.EndObject, xobj);
			} else {
				// Object
				yield return new XamlNodeInfo (XamlNodeType.StartObject, xobj);
				foreach (var xn in GetObjectMemberNodes (xobj))
					yield return xn;
				yield return new XamlNodeInfo (XamlNodeType.EndObject, xobj);
			}
		}

		IEnumerable<XamlNodeInfo> GetObjectMemberNodes (XamlObject xobj)
		{
			var xce = xobj.Children (value_serializer_ctx).GetEnumerator ();
			while (xce.MoveNext ()) {
				// XamlLanguage.Items does not show up if the content is empty.
				if (xce.Current.Member == XamlLanguage.Items)
					if (!GetNodes (xce.Current.Member, xce.Current.Value).GetEnumerator ().MoveNext ())
						continue;

				// Other collections as well, but needs different iteration (as nodes contain GetObject and EndObject).
				if (!xce.Current.Member.IsWritePublic && xce.Current.Member.Type != null && xce.Current.Member.Type.IsCollection) {
					var e = GetNodes (xce.Current.Member, xce.Current.Value).GetEnumerator ();
					if (!(e.MoveNext () && e.MoveNext () && e.MoveNext ())) // GetObject, EndObject and more
						continue;
				}

				yield return new XamlNodeInfo (XamlNodeType.StartMember, xce.Current);
				foreach (var cn in GetNodes (xce.Current.Member, xce.Current.Value))
					yield return cn;
				yield return new XamlNodeInfo (XamlNodeType.EndMember, xce.Current);
			}
		}

		IEnumerable<XamlNodeInfo> GetItemsNodes (XamlMember xm, XamlObject xobj)
		{
			var obj = xobj.GetRawValue ();
			if (obj == null)
				yield break;
			var ie = xobj.Type.Invoker.GetItems (obj);
			while (ie.MoveNext ()) {
				var iobj = ie.Current;
				// If it is dictionary, then retrieve the key, and rewrite the item as the Value part.
				object ikey = null;
				if (xobj.Type.IsDictionary) {
					Type kvpType = iobj.GetType ();
					bool isNonGeneric = kvpType == typeof (DictionaryEntry);
					var kp = isNonGeneric ? null : kvpType.GetProperty ("Key");
					var vp = isNonGeneric ? null : kvpType.GetProperty ("Value");
					ikey = isNonGeneric ? ((DictionaryEntry) iobj).Key : kp.GetValue (iobj, null);
					iobj = isNonGeneric ? ((DictionaryEntry) iobj).Value : vp.GetValue (iobj, null);
				}

				var wobj = TypeExtensionMethods.GetExtensionWrapped (iobj);
				var xiobj = new XamlObject (GetType (wobj), wobj);
				if (ikey != null) {
					// Key member is written *inside* the item object.
					//
					// It is messy, but Key and Value are *sorted*. In most cases Key goes first, but for example PositionalParameters comes first.
					// To achieve this behavior, we compare XamlLanguage.Key and value's Member and returns in order. It's all nasty hack, but at least it could be achieved like this!

					var en = GetNodes (null, xiobj).ToArray ();
					yield return en [0]; // StartObject

					var xknm = new XamlNodeMember (xobj, XamlLanguage.Key);
					if (TypeExtensionMethods.CompareMembers (en [1].Member.Member, XamlLanguage.Key) < 0) { // en[1] is the StartMember of the first member.
						// value -> key -> endobject
						for (int i = 1; i < en.Length - 1; i++)
							yield return en [i];
						foreach (var kn in GetKeyNodes (ikey, xobj.Type.KeyType, xknm))
							yield return kn;
						yield return en [en.Length - 1];
					} else {
						// key -> value -> endobject
						foreach (var kn in GetKeyNodes (ikey, xobj.Type.KeyType, xknm))
							yield return kn;
						for (int i = 1; i < en.Length - 1; i++)
							yield return en [i];
						yield return en [en.Length - 1];
					}
				}
				else
					foreach (var xn in GetNodes (null, xiobj))
						yield return xn;
			}
		}

		IEnumerable<XamlNodeInfo> GetKeyNodes (object ikey, XamlType keyType, XamlNodeMember xknm)
		{
			yield return new XamlNodeInfo (XamlNodeType.StartMember, xknm);
			foreach (var xn in GetNodes (XamlLanguage.Key, new XamlObject (GetType (ikey), ikey), keyType, false))
				yield return xn;
			yield return new XamlNodeInfo (XamlNodeType.EndMember, xknm);
		}

		// Namespace and Reference retrieval.
		// It is iterated before iterating the actual object nodes,
		// and results are cached for use in XamlObjectReader.
		public void PrepareReading ()
		{
			PrefixLookup.IsCollectingNamespaces = true;
			foreach (var xn in GetNodes ()) {
				if (xn.NodeType == XamlNodeType.GetObject)
					continue; // it is out of consideration here.
				if (xn.NodeType == XamlNodeType.StartObject) {
					foreach (var ns in NamespacesInType (xn.Object.Type))
						PrefixLookup.LookupPrefix (ns);
				} else if (xn.NodeType == XamlNodeType.StartMember) {
					var xm = xn.Member.Member;
					// This filtering is done as a black list so far. There does not seem to be any usable property on XamlDirective.
					if (xm == XamlLanguage.Items || xm == XamlLanguage.PositionalParameters || xm == XamlLanguage.Initialization)
						continue;
					PrefixLookup.LookupPrefix (xn.Member.Member.PreferredXamlNamespace);
				} else {
					if (xn.NodeType == XamlNodeType.Value && xn.Value is Type)
						// this tries to lookup existing prefix, and if there isn't any, then adds a new declaration.
						TypeExtensionMethods.GetStringValue (XamlLanguage.Type, xn.Member.Member, xn.Value, value_serializer_ctx);
					continue;
				}
			}
			PrefixLookup.Namespaces.Sort ((nd1, nd2) => String.CompareOrdinal (nd1.Prefix, nd2.Prefix));
			PrefixLookup.IsCollectingNamespaces = false;
		}
		
		IEnumerable<string> NamespacesInType (XamlType xt)
		{
			yield return xt.PreferredXamlNamespace;
			if (xt.TypeArguments != null) {
				// It is for x:TypeArguments
				yield return XamlLanguage.Xaml2006Namespace;
				foreach (var targ in xt.TypeArguments)
					foreach (var ns in NamespacesInType (targ))
						yield return ns;
			}
		}
	}
}
