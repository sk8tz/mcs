// dmcs -d:DOTNET -r:System.Xaml -debug System.Xaml/XamlXmlWriter-new.cs System.Xaml/TypeExtensionMethods.cs System.Xaml/XamlWriterStateManager.cs System.Xaml/XamlNameResolver.cs System.Xaml/PrefixLookup.cs ../../build/common/MonoTODOAttribute.cs Test/System.Xaml/TestedTypes.cs

#define USE_NEW
#if USE_NEW




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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;

//
// XamlWriter expects write operations in premised orders.
// The most basic one is:
//
//	[NamespaceDeclaration]* -> StartObject -> [ StartMember -> Value | StartObject ... EndObject -> EndMember ]* -> EndObject
//
// For collections:
//	[NamespaceDeclaration]* -> StartObject -> (members)* -> StartMember XamlLanguage.Items -> [ StartObject ... EndObject ]* -> EndMember -> EndObject
//
// For MarkupExtension with PositionalParameters:
//
//	[NamespaceDeclaration]* -> StartObject -> StartMember XamlLanguage.PositionalParameters -> [Value]* -> EndMember -> ... -> EndObject
//

#if DOTNET
namespace Mono.Xaml
#else
namespace System.Xaml
#endif
{
	public class XamlXmlWriter : XamlWriter
	{
		public XamlXmlWriter (Stream stream, XamlSchemaContext schemaContext)
			: this (stream, schemaContext, null)
		{
		}
		
		public XamlXmlWriter (Stream stream, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
			: this (XmlWriter.Create (stream), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (TextWriter textWriter, XamlSchemaContext schemaContext)
			: this (XmlWriter.Create (textWriter), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (TextWriter textWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
			: this (XmlWriter.Create (textWriter), schemaContext, null)
		{
		}
		
		public XamlXmlWriter (XmlWriter xmlWriter, XamlSchemaContext schemaContext)
			: this (xmlWriter, schemaContext, null)
		{
		}
		
		public XamlXmlWriter (XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
		{
			if (xmlWriter == null)
				throw new ArgumentNullException ("xmlWriter");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			this.w = xmlWriter;
			this.sctx = schemaContext;
			this.settings = settings ?? new XamlXmlWriterSettings ();
			var manager = new XamlWriterStateManager<XamlXmlWriterException, InvalidOperationException> (true);
			intl = new XamlXmlWriterInternal (xmlWriter, sctx, manager);
		}

		XmlWriter w;
		XamlSchemaContext sctx;
		XamlXmlWriterSettings settings;

		XamlXmlWriterInternal intl;

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public XamlXmlWriterSettings Settings {
			get { return settings; }
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			intl.CloseAll ();

			if (settings.CloseOutput)
				w.Close ();
		}

		public void Flush ()
		{
			w.Flush ();
		}

		public override void WriteGetObject ()
		{
			intl.WriteGetObject ();
		}

		public override void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			intl.WriteNamespace (namespaceDeclaration);
		}

		public override void WriteStartObject (XamlType xamlType)
		{
			intl.WriteStartObject (xamlType);
		}
		
		public override void WriteValue (object value)
		{
			if (value != null && !(value is string))
				throw new ArgumentException ("Non-string value cannot be written.");

			intl.WriteValue (value);
		}
		
		public override void WriteStartMember (XamlMember property)
		{
			intl.WriteStartMember (property);
		}
		
		public override void WriteEndObject ()
		{
			intl.WriteEndObject ();
		}

		public override void WriteEndMember ()
		{
			intl.WriteEndMember ();
		}
	}

	internal abstract class XamlWriterInternalBase
	{
		static readonly BindingFlags static_flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		public XamlWriterInternalBase (XamlSchemaContext schemaContext, XamlWriterStateManager manager)
		{
			this.sctx = schemaContext;
			this.manager = manager;
			prefix_lookup = new PrefixLookup (sctx) { IsCollectingNamespaces = true }; // it does not raise unknown namespace error.
			service_provider = new XamlWriterInternalServiceProvider (namespaces, schemaContext);
		}

		XamlSchemaContext sctx;
		XamlWriterStateManager manager;

		bool member_had_namespaces = false;
		
		IServiceProvider service_provider;

		internal Stack<ObjectState> object_states = new Stack<ObjectState> ();
		internal PrefixLookup prefix_lookup;

		List<NamespaceDeclaration> namespaces {
			get { return prefix_lookup.Namespaces; }
		}

		internal class ObjectState
		{
			public XamlType Type;
			public object Value;
			public List<object> Contents = new List<object> ();
			public List<MemberAndValue> WrittenProperties = new List<MemberAndValue> ();
			public bool IsInstantiated;
			public bool IsGetObject;
			public int PositionalParameterIndex = -1;

			public string FactoryMethod;
			public List<object> Arguments = new List<object> ();
		}
		
		internal class MemberAndValue
		{
			public MemberAndValue (XamlMember xm)
			{
				Member = xm;
			}

			public XamlMember Member;
			public object Value;
		}

		public void CloseAll ()
		{
			while (object_states.Count > 0) {
				switch (manager.State) {
				case XamlWriteState.MemberDone:
				case XamlWriteState.ObjectStarted: // StartObject without member
					WriteEndObject ();
					break;
				case XamlWriteState.ValueWritten:
				case XamlWriteState.ObjectWritten:
				case XamlWriteState.MemberStarted: // StartMember without content
					manager.OnClosingItem ();
					WriteEndMember ();
					break;
				default:
					throw new NotImplementedException (manager.State.ToString ()); // there shouldn't be anything though
				}
			}
		}

		internal string GetPrefix (string ns)
		{
			foreach (var nd in namespaces)
				if (nd.Namespace == ns)
					return nd.Prefix;
			return null;
		}

		protected XamlMember CurrentMember {
			get {
				var mv = object_states.Count > 0 ? object_states.Peek ().WrittenProperties.LastOrDefault () : null;
				return mv != null ? mv.Member : null;
			}
		}

		public void WriteGetObject ()
		{
			manager.GetObject ();

			var xm = CurrentMember;

			// FIXME: see GetObjectOnNonNullString() test. Below is invalid.
			if (!xm.Type.IsCollection)
				throw new InvalidOperationException (String.Format ("WriteGetObject method can be invoked only when current member '{0}' is of collection type", xm.Name));

/*
			var instance = xm.Invoker.GetValue (object_states.Peek ().Value);
			if (instance == null)
				throw new XamlObjectWriterException (String.Format ("The value  for '{0}' property is null", xm.Name));

			var state = new ObjectState () {Type = sctx.GetXamlType (instance.GetType ()), Value = instance, IsInstantiated = true, IsGetObject = true};
*/
			var state = new ObjectState () {Type = xm.Type, IsGetObject = true};

			object_states.Push (state);

			OnWriteGetObject ();
		}

		public void WriteNamespace (NamespaceDeclaration namespaceDeclaration)
		{
			if (namespaceDeclaration == null)
				throw new ArgumentNullException ("namespaceDeclaration");

			manager.Namespace ();

			namespaces.Add (namespaceDeclaration);
			OnWriteNamespace (namespaceDeclaration);
		}

		public void WriteStartObject (XamlType xamlType)
		{
			if (xamlType == null)
				throw new ArgumentNullException ("xamlType");

			manager.StartObject ();

			var xm = CurrentMember;
//			var pstate = xm != null ? object_states.Peek () : null;
//			var wpl = xm != null && xm != XamlLanguage.Items ? pstate.WrittenProperties : null;
//			if (wpl != null && wpl.Any (wp => wp.Member == xm))
//				throw new XamlDuplicateMemberException (String.Format ("Property '{0}' is already set to this '{1}' object", xm, pstate.Type));

			var cstate = new ObjectState () {Type = xamlType, IsInstantiated = false};
			object_states.Push (cstate);

			OnWriteStartObject (xamlType);
		}
		
		public void WriteValue (object value)
		{
			if (value != null && !(value is string))
				throw new ArgumentException ("Non-string value cannot be written.");

			manager.Value ();

			var xm = CurrentMember;
			var state = object_states.Peek ();

//			var wpl = xm != null && xm != XamlLanguage.Items ? state.WrittenProperties : null;
//			if (wpl != null && wpl.Any (wp => wp.Member == xm))
//				throw new XamlDuplicateMemberException (String.Format ("Property '{0}' is already set to this '{1}' object", xm, state.Type));

			if (xm == XamlLanguage.Initialization ||
			    xm == state.Type.ContentProperty) {
				value = GetCorrectlyTypedValue (state.Type, value);
				state.Value = value;
				state.IsInstantiated = true;
			}
//			else if (xm.Type.IsCollection)
			else if (xm == XamlLanguage.Items) // FIXME: am not sure which is good yet.
				state.Contents.Add (GetCorrectlyTypedValue (xm.Type.ItemType, value));
			else
				state.Contents.Add (GetCorrectlyTypedValue (xm.Type, value));
			OnWriteValue (xm, value);
		}
		
		public void WriteStartMember (XamlMember property)
		{
			if (property == null)
				throw new ArgumentNullException ("property");

			if (manager.HasNamespaces)
				member_had_namespaces = true;

			manager.StartMember ();
			if (property == XamlLanguage.PositionalParameters)
				// this is an exception that indicates the state manager to accept more than values within this member.
				manager.AcceptMultipleValues = true;

			var state = object_states.Peek ();
			var wpl = state.WrittenProperties;
			// FIXME: enable this. Duplicate property check should
			// be differentiate from duplicate contents (both result
			// in XamlDuplicateMemberException though).
			// Now it is done at WriteStartObject/WriteValue, but
			// it is simply wrong.
			if (wpl.Any (wp => wp.Member == property))
				throw new XamlDuplicateMemberException (String.Format ("Property '{0}' is already set to this '{1}' object", property, object_states.Peek ().Type));
			wpl.Add (new MemberAndValue (property));
			if (property == XamlLanguage.PositionalParameters)
				state.PositionalParameterIndex = 0;

			OnWriteStartMember (property);
		}
		
		public void WriteEndObject ()
		{
			manager.EndObject (object_states.Count > 1);

			//InitializeObjectIfRequired (false); // this is required for such case that there was no StartMember call.

			OnWriteEndObject ();

			var state = object_states.Pop ();
/*
			var obj = GetCorrectlyTypedValue (state.Type, state.Value);
			if (CurrentMember != null) {
				var pstate = object_states.Peek ();
				pstate.Contents.Add (obj);
				pstate.WrittenProperties.Add (new MemberAndValue (CurrentMember));
			}
*/
		}

		public void WriteEndMember ()
		{
			manager.EndMember ();
			
			var state = object_states.Peek ();
			if (CurrentMember == XamlLanguage.PositionalParameters) {
				// this is an exception that indicates the state manager to accept more than values within this member.
				manager.AcceptMultipleValues = false;
				state.PositionalParameterIndex = -1;
			}
			var xm = CurrentMember;
			var contents = state.Contents;

			if (xm == XamlLanguage.PositionalParameters)
				state.PositionalParameterIndex = -1;

/*
			if (xm == XamlLanguage.FactoryMethod) {
				if (contents.Count != 1 || !(contents [0] is string))
					throw new XamlObjectWriterException (String.Format ("FactoryMethod must be non-empty string name. {0} value exists.", contents.Count > 0 ? contents [0] : "0"));
				state.FactoryMethod = (string) contents [0];
			} else if (xm == XamlLanguage.Arguments) {
				if (state.FactoryMethod != null) {
					var mi = state.Type.UnderlyingType.GetMethods (static_flags).FirstOrDefault (mii => mii.Name == state.FactoryMethod && mii.GetParameters ().Length == contents.Count);
					if (mi == null)
						throw new XamlObjectWriterException (String.Format ("Specified static factory method '{0}' for type '{1}' was not found", state.FactoryMethod, state.Type));
					state.Value = mi.Invoke (null, contents.ToArray ());
				}
				else
					throw new NotImplementedException ();
			} else if (xm == XamlLanguage.Initialization) {
				// ... and no need to do anything. The object value to pop *is* the return value.
			} else if (xm == XamlLanguage.Items) {
				var coll = state.Value;
				foreach (var content in contents)
					xm.Type.Invoker.AddToCollection (coll, content);
			} else if (xm.Type.IsDictionary) {
				throw new NotImplementedException ();
			} else {
				if (contents.Count > 1)
					throw new XamlDuplicateMemberException (String.Format ("Property '{0}' is already set to this '{1}' object", xm, state.Type));
				if (contents.Count == 1) {
					var value = contents [0];
					if (!xm.Type.IsCollection || !xm.IsReadOnly) // exclude read-only object.
						OnWriteValue (xm, value);
				}
			}
*/

			contents.Clear ();

//			if (object_states.Count > 0)
//				object_states.Peek ().WrittenProperties.Add (new MemberAndValue (xm));
			//written_properties_stack.Peek ().Add (xm);

			OnWriteEndMember ();
			member_had_namespaces = false;
		}

		protected abstract void OnWriteEndObject ();

		protected abstract void OnWriteEndMember ();

		protected abstract void OnWriteStartObject (XamlType xamlType);

		protected abstract void OnWriteGetObject ();

		protected abstract void OnWriteStartMember (XamlMember xm);

		protected abstract void OnWriteValue (XamlMember xm, object value);

		protected abstract void OnWriteNamespace (NamespaceDeclaration nd);


		void InitializeObjectIfRequired (bool isStart)
		{
			var state = object_states.Peek ();
			if (state.IsInstantiated)
				return;

			// FIXME: "The default techniques in absence of a factory method are to attempt to find a default constructor, then attempt to find an identified type converter on type, member, or destination type."
			// http://msdn.microsoft.com/en-us/library/system.xaml.xamllanguage.factorymethod%28VS.100%29.aspx
			object obj;
			if (state.FactoryMethod != null) // FIXME: it must be implemented and verified with tests.
				throw new NotImplementedException ();
			else
				obj = state.Type.Invoker.CreateInstance (null);
			state.Value = obj;
			state.IsInstantiated = true;
		}

		bool IsAllowedType (XamlType xt, object value)
		{
			// FIXME: not sure if it is correct
			if (value is string)
				return true;

			return  xt == null ||
				xt.UnderlyingType == null ||
				xt.UnderlyingType.IsInstanceOfType (value) ||
				value == null && xt == XamlLanguage.Null ||
				xt.IsMarkupExtension && IsAllowedType (xt.MarkupExtensionReturnType, value);
		}

		object GetCorrectlyTypedValue (XamlType xt, object value)
		{
			// FIXME: this could be generalized by some means, but I cannot find any.
			if (xt.UnderlyingType == typeof (Type))
				xt = XamlLanguage.Type;
			if (xt == XamlLanguage.Type && value is string)
				value = new TypeExtension ((string) value);
			
			if (value is MarkupExtension)
				value = ((MarkupExtension) value).ProvideValue (service_provider);

			if (IsAllowedType (xt, value))
				return value;

			if (xt.TypeConverter != null && value != null) {
				var tc = xt.TypeConverter.ConverterInstance;
				if (tc != null && tc.CanConvertFrom (value.GetType ()))
					value = tc.ConvertFrom (value);
				if (IsAllowedType (xt, value))
					return value;
			}

			throw new XamlObjectWriterException (String.Format ("Value '{1}' (of type {2}) is not of or convertible to type {0}", xt, value, value != null ? (object) value.GetType () : "(null)"));
		}
	}
	
	// specific implementation
	class XamlXmlWriterInternal : XamlWriterInternalBase
	{
		const string Xmlns2000Namespace = "http://www.w3.org/2000/xmlns/";

		public XamlXmlWriterInternal (XmlWriter w, XamlSchemaContext schemaContext, XamlWriterStateManager manager)
			: base (schemaContext, manager)
		{
			this.w = w;
			this.sctx = schemaContext;
		}
		
		XmlWriter w;
		XamlSchemaContext sctx;
		
		// Here's a complication.
		// - local_nss holds namespace declarations that are written *before* current element.
		// - local_nss2 holds namespace declarations that are wrtten *after* current element.
		//   (current element == StartObject or StartMember)
		// - When the next element or content is being written, local_nss items are written *within* current element, BUT after all attribute members are written. Hence I had to preserve all those nsdecls at such late.
		// - When current *start* element is closed, then copy local_nss2 items into local_nss.
		// - When there was no children i.e. end element immediately occurs, local_nss should be written at this stage too, and local_nss2 are *ignored*.
		List<NamespaceDeclaration> local_nss = new List<NamespaceDeclaration> ();
		List<NamespaceDeclaration> local_nss2 = new List<NamespaceDeclaration> ();
		bool inside_toplevel_positional_parameter;
		bool inside_attribute_object;

		protected override void OnWriteEndObject ()
		{
			var state = object_states.Count > 0 ? object_states.Peek () : null;
			if (state != null && state.IsGetObject) {
				// do nothing
				state.IsGetObject = false;
			} else if (w.WriteState == WriteState.Attribute) {
				w.WriteString ("}");
				inside_attribute_object = false;
			} else {
				WritePendingNamespaces ();
				w.WriteEndElement ();
			}
		}

		protected override void OnWriteEndMember ()
		{
			var member = CurrentMember;
			if (member == XamlLanguage.Initialization)
				return;
			if (member == XamlLanguage.Items)
				return;
			if (member != null && member.Type.IsCollection && member.IsReadOnly)
				return;

			if (inside_toplevel_positional_parameter) {
				w.WriteEndAttribute ();
				inside_toplevel_positional_parameter = false;
			} else if (inside_attribute_object) {
				// do nothing. It didn't open this attribute.
			} else {
				var state = object_states.Peek ();
				if (IsAttribute (state.Type, member)) {
					w.WriteEndAttribute ();
				} else {
					WritePendingNamespaces ();
					w.WriteEndElement ();
				}
			}
		}
		
		protected override void OnWriteStartObject (XamlType xamlType)
		{
			string ns = xamlType.PreferredXamlNamespace;
			string prefix = GetPrefix (ns); // null prefix is not rejected...

			if (w.WriteState == WriteState.Attribute) {
				// MarkupExtension
				w.WriteString ("{");
				if (!String.IsNullOrEmpty (prefix)) {
					w.WriteString (prefix);
					w.WriteString (":");
				}
				string name = ns == XamlLanguage.Xaml2006Namespace ? xamlType.GetInternalXmlName () : xamlType.Name;
				w.WriteString (name);
				// space between type and first member (if any).
				if (xamlType.IsMarkupExtension && xamlType.GetSortedConstructorArguments ().GetEnumerator ().MoveNext ())
					w.WriteString (" ");
			} else {
				WritePendingNamespaces ();
				w.WriteStartElement (prefix, xamlType.GetInternalXmlName (), xamlType.PreferredXamlNamespace);
				var l = xamlType.TypeArguments;
				if (l != null) {
					w.WriteStartAttribute ("x", "TypeArguments", XamlLanguage.Xaml2006Namespace);
					for (int i = 0; i < l.Count; i++) {
						if (i > 0)
							w.WriteString (", ");
						w.WriteString (new XamlTypeName (l [i]).ToString (prefix_lookup));
					}
					w.WriteEndAttribute ();
				}
			}
		}

		protected override void OnWriteGetObject ()
		{
			// nothing to do.
		}
		
		protected override void OnWriteStartMember (XamlMember member)
		{
			if (member == XamlLanguage.Initialization)
				return;
			if (member == XamlLanguage.Items)
				return;
			if (member != null && member.Type.IsCollection && member.IsReadOnly)
				return;

			var state = object_states.Peek ();
			
			// Top-level positional parameters are somehow special.
			// - If it has only one parameter, it is written as an
			//   attribute using the actual argument's member name.
			// - If there are more than one, then it is treated as
			var posprms = object_states.Count == 1 && state.Type.HasPositionalParameters () ? state.Type.GetSortedConstructorArguments ().GetEnumerator () : null;
			if (posprms != null) {
				if (inside_toplevel_positional_parameter)
					throw new XamlXmlWriterException (String.Format ("The XAML reader input has more than one positional parameter values within a top-level object {0}. While XamlObjectReader can read such an object, XamlXmlWriter cannot write such an object to XML.", state.Type));
				posprms.MoveNext ();
				var arg = posprms.Current;
				w.WriteStartAttribute (arg.Name);
				inside_toplevel_positional_parameter = true;
			}

			if (w.WriteState == WriteState.Attribute) {
				inside_attribute_object = true;
				if (state.PositionalParameterIndex < 0) {
					w.WriteString (" ");
					w.WriteString (member.Name);
					w.WriteString ("=");
				}
			} else if (IsAttribute (state.Type, member))
				OnWriteStartMemberAttribute (state.Type, member);
			else
				OnWriteStartMemberElement (state.Type, member);
		}

		bool IsAttribute (XamlType xt, XamlMember xm)
		{
			if (xm == XamlLanguage.Initialization)
				return false;
			if (xm.Type.HasPositionalParameters ())
				return true;
			if (w.WriteState == WriteState.Content)
				return false;
			var xd = xm as XamlDirective;
			if (xd != null && (xd.AllowedLocation & AllowedMemberLocations.Attribute) == 0)
				return false;
			if (xm.TypeConverter != null && xm.TypeConverter.ConverterInstance.CanConvertTo (typeof (string)))
				return true;
			return false;
		}

		void OnWriteStartMemberElement (XamlType xt, XamlMember xm)
		{
			string prefix = GetPrefix (xm.PreferredXamlNamespace);
			string name = xm.IsDirective ? xm.Name : String.Concat (xt.GetInternalXmlName (), ".", xm.Name);
			WritePendingNamespaces ();
			w.WriteStartElement (prefix, name, xm.PreferredXamlNamespace);
		}
		
		void OnWriteStartMemberAttribute (XamlType xt, XamlMember xm)
		{
			if (xt.PreferredXamlNamespace == xm.PreferredXamlNamespace &&
			    !(xm is XamlDirective)) // e.g. x:Key inside x:Int should not be written as Key.
				w.WriteStartAttribute (xm.Name);
			else {
				string prefix = GetPrefix (xm.PreferredXamlNamespace);
				w.WriteStartAttribute (prefix, xm.Name, xm.PreferredXamlNamespace);
			}
		}

		protected override void OnWriteValue (XamlMember xm, object value)
		{
			var xt = value == null ? XamlLanguage.Null : sctx.GetXamlType (value.GetType ());

			var vs = xt.TypeConverter;
			var c = vs != null ? vs.ConverterInstance : null;

			if (w.WriteState != WriteState.Attribute)
				WritePendingNamespaces ();

			string s;
			if (c != null && c.CanConvertTo (typeof (string)))
				s = c.ConvertToInvariantString (value);
			else
				throw new XamlXmlWriterException (String.Format ("Value type is '{0}' but it must be either string or any type that is convertible to string indicated by TypeConverterAttribute.", value != null ? value.GetType () : null));

			var state = object_states.Peek ();
			switch (state.PositionalParameterIndex) {
			case -1:
				break;
			case 0:
				state.PositionalParameterIndex++;
				break;
			default:
				state.PositionalParameterIndex++;
				w.WriteString (", ");
				break;
			}
			w.WriteString (s);
		}

		protected override void OnWriteNamespace (NamespaceDeclaration nd)
		{
			local_nss2.Add (nd);
		}
		
		void WritePendingNamespaces ()
		{
// FIXME: remove it
w.Flush ();

			foreach (var nd in local_nss) {
				if (String.IsNullOrEmpty (nd.Prefix))
					w.WriteAttributeString ("xmlns", nd.Namespace);
				else
					w.WriteAttributeString ("xmlns", nd.Prefix, Xmlns2000Namespace, nd.Namespace);
			}
			local_nss.Clear ();
			local_nss.AddRange (local_nss2);
			local_nss2.Clear ();
		}

		string ToMarkupString (XamlType xt, MarkupExtension m)
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ('{');
			sb.Append (new XamlTypeName (xt).ToString (prefix_lookup));
			foreach (var xm in xt.GetConstructorArguments ()) {
				sb.Append (' ').Append (xm.Name).Append ('=');
				// FIXME: incomplete
				sb.Append (xm.Invoker.GetValue (m));
			}
			sb.Append ('}');

			return sb.ToString ();
		}
	}

	// service provider and resolvers
	
	internal class XamlWriterInternalServiceProvider : IServiceProvider
	{
		XamlNameResolver name_resolver = new XamlNameResolver ();
		XamlTypeResolver type_resolver;
		NamespaceResolver namespace_resolver;

		public XamlWriterInternalServiceProvider (IList<NamespaceDeclaration> namespaces, XamlSchemaContext schemaContext)
		{
			namespace_resolver = new NamespaceResolver (namespaces);
			type_resolver = new XamlTypeResolver (namespace_resolver, schemaContext);
		}

		public object GetService (Type serviceType)
		{
			if (serviceType == typeof (IXamlNamespaceResolver))
				return namespace_resolver;
			if (serviceType == typeof (IXamlNameResolver))
				return name_resolver;
			if (serviceType == typeof (IXamlTypeResolver))
				return type_resolver;
			return null;
		}
	}

	internal class XamlTypeResolver : IXamlTypeResolver
	{
		NamespaceResolver ns_resolver;
		XamlSchemaContext schema_context;

		public XamlTypeResolver (NamespaceResolver namespaceResolver, XamlSchemaContext schemaContext)
		{
			ns_resolver = namespaceResolver;
			schema_context = schemaContext;
		}

		public Type Resolve (string typeName)
		{
			var tn = XamlTypeName.Parse (typeName, ns_resolver);
			var xt = schema_context.GetXamlType (tn);
			return xt != null ? xt.UnderlyingType : null;
		}
	}

	internal class NamespaceResolver : IXamlNamespaceResolver
	{
		public NamespaceResolver (IList<NamespaceDeclaration> source)
		{
			this.source = source;
		}
	
		IList<NamespaceDeclaration> source;
	
		public string GetNamespace (string prefix)
		{
			foreach (var nsd in source)
				if (nsd.Prefix == prefix)
					return nsd.Namespace;
			return null;
		}
	
		public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes ()
		{
			return source;
		}
	}

#if DOTNET
	internal static class TypeExtensionMethods2
	{
		static TypeExtensionMethods2 ()
		{
			SpecialNames = new SpecialTypeNameList ();
		}

		public static string GetInternalXmlName (this XamlType type)
		{
			if (type.IsMarkupExtension && type.Name.EndsWith ("Extension", StringComparison.Ordinal))
				return type.Name.Substring (0, type.Name.Length - 9);
			var stn = SpecialNames.FirstOrDefault (s => s.Type == type);
			return stn != null ? stn.Name : type.Name;
		}

		// FIXME: I'm not sure if these "special names" should be resolved like this. I couldn't find any rule so far.
		internal static readonly SpecialTypeNameList SpecialNames;

		internal class SpecialTypeNameList : List<SpecialTypeName>
		{
			internal SpecialTypeNameList ()
			{
				Add (new SpecialTypeName ("Member", XamlLanguage.Member));
				Add (new SpecialTypeName ("Property", XamlLanguage.Property));
			}

			public XamlType Find (string name, string ns)
			{
				if (ns != XamlLanguage.Xaml2006Namespace)
					return null;
				var stn = this.FirstOrDefault (s => s.Name == name);
				return stn != null ? stn.Type : null;
			}
		}

		internal class SpecialTypeName
		{
			public SpecialTypeName (string name, XamlType type)
			{
				Name = name;
				Type = type;
			}
			
			public string Name { get; private set; }
			public XamlType Type { get; private set; }
		}
	}
#endif
}
#endif
