//
// Mono.Xml.DTDObjectModel
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml.Schema;
using Mono.Xml.Native;

namespace Mono.Xml
{
	public class DTDObjectModel
	{
		DTDElementDeclarationCollection elementDecls;
		DTDAttListDeclarationCollection attListDecls;
		DTDEntityDeclarationCollection entityDecls;
		DTDNotationDeclarationCollection notationDecls;
		ArrayList validationErrors;
		XmlResolver resolver;

		public DTDObjectModel ()
		{
			elementDecls = new DTDElementDeclarationCollection (this);
			attListDecls = new DTDAttListDeclarationCollection (this);
			entityDecls = new DTDEntityDeclarationCollection (this);
			notationDecls = new DTDNotationDeclarationCollection (this);
			factory = new DTDAutomataFactory (this);
			validationErrors = new ArrayList ();
		}

		public string BaseURI;

		public string Name;
		
		public string PublicId;
		
		public string SystemId;
		
		public string InternalSubset;

		public bool InternalSubsetHasPEReference;
		
		public string ResolveEntity (string name)
		{
			DTDEntityDeclaration decl = EntityDecls [name] 
				as DTDEntityDeclaration;
			return decl.EntityValue;
		}

		internal XmlResolver Resolver {
			get { return resolver; }
		}

		public XmlResolver XmlResolver {
			set { resolver = value; }
		}

		private DTDAutomataFactory factory;
		public DTDAutomataFactory Factory {
			get { return factory; }
		}

		public DTDElementDeclaration RootElement {
			get { return ElementDecls [Name]; }
		}

		public DTDElementDeclarationCollection ElementDecls {
			get { return elementDecls; }
		}

		public DTDAttListDeclarationCollection AttListDecls {
			get { return attListDecls; }
		}

		public DTDEntityDeclarationCollection EntityDecls {
			get { return entityDecls; }
		}

		public DTDNotationDeclarationCollection NotationDecls {
			get { return notationDecls; }
		}

		DTDElementAutomata rootAutomata;
		public DTDAutomata RootAutomata {
			get {
				if (rootAutomata == null)
					rootAutomata = new DTDElementAutomata (this, this.Name);
				return rootAutomata;
			}
		}

		DTDEmptyAutomata emptyAutomata;
		public DTDEmptyAutomata Empty {
			get {
				if (emptyAutomata == null)
					emptyAutomata = new DTDEmptyAutomata (this);
				return emptyAutomata;
			}
		}

		DTDAnyAutomata anyAutomata;
		public DTDAnyAutomata Any {
			get {
				if (anyAutomata == null)
					anyAutomata = new DTDAnyAutomata (this);
				return anyAutomata;
			}
		}

		DTDInvalidAutomata invalidAutomata;
		public DTDInvalidAutomata Invalid {
			get {
				if (invalidAutomata == null)
					invalidAutomata = new DTDInvalidAutomata (this);
				return invalidAutomata;
			}
		}

		public XmlSchemaException [] Errors {
			get { return validationErrors.ToArray (typeof (XmlSchemaException)) as XmlSchemaException []; }
		}

		public void AddError (XmlSchemaException ex)
		{
			validationErrors.Add (ex);
		}
	}

	public class DTDElementDeclarationCollection
	{
		Hashtable elementDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDElementDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDElementDeclaration this [string name] {
			get { return elementDecls [name] as DTDElementDeclaration; }
		}

		public void Add (string name, DTDElementDeclaration decl)
		{
			if (elementDecls [name] != null) {
				this.root.AddError (new XmlSchemaException (String.Format (
					"Element declaration for {0} was already added.",
					name), null));
				return;
			}
			decl.SetRoot (root);
			elementDecls.Add (name, decl);
		}

		public ICollection Keys {
			get { return elementDecls.Keys; }
		}

		public ICollection Values {
			get { return elementDecls.Values; }
		}
	}

	public class DTDAttListDeclarationCollection
	{
		Hashtable attListDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDAttListDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDAttListDeclaration this [string name] {
			get { return attListDecls [name] as DTDAttListDeclaration; }
		}

		public void Add (string name, DTDAttListDeclaration decl)
		{
			DTDAttListDeclaration existing = this [name];
			if (existing != null) {
				// It should be valid and 
				// has effect of additive declaration.
				foreach (DTDAttributeDefinition def in decl.Definitions)
					if (decl.Get (def.Name) == null)
						existing.Add (def);
			} else {
				decl.SetRoot (root);
				attListDecls.Add (name, decl);
			}
		}

		public ICollection Keys {
			get { return attListDecls.Keys; }
		}

		public ICollection Values {
			get { return attListDecls.Values; }
		}
	}

	public class DTDEntityDeclarationCollection
	{
		Hashtable entityDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDEntityDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDEntityDeclaration this [string name] {
			get { return entityDecls [name] as DTDEntityDeclaration; }
		}

		public void Add (string name, DTDEntityDeclaration decl)
		{
			if (entityDecls [name] != null)
				throw new InvalidOperationException (String.Format (
					"Entity declaration for {0} was already added.",
					name));
			decl.SetRoot (root);
			entityDecls.Add (name, decl);
		}

		public ICollection Keys {
			get { return entityDecls.Keys; }
		}

		public ICollection Values {
			get { return entityDecls.Values; }
		}
	}

	public class DTDNotationDeclarationCollection
	{
		Hashtable notationDecls = new Hashtable ();
		DTDObjectModel root;

		public DTDNotationDeclarationCollection (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDNotationDeclaration this [string name] {
			get { return notationDecls [name] as DTDNotationDeclaration; }
		}

		public void Add (string name, DTDNotationDeclaration decl)
		{
			if (notationDecls [name] != null)
				throw new InvalidOperationException (String.Format (
					"Notation declaration for {0} was already added.",
					name));
			decl.SetRoot (root);
			notationDecls.Add (name, decl);
		}

		public ICollection Keys {
			get { return notationDecls.Keys; }
		}

		public ICollection Values {
			get { return notationDecls.Values; }
		}
	}

	public class DTDContentModel
	{
		private DTDObjectModel root;
		DTDAutomata compiledAutomata;

		private string ownerElementName;
		public string ElementName;
		public DTDContentOrderType OrderType = DTDContentOrderType.None;
		public DTDContentModelCollection ChildModels 
			= new DTDContentModelCollection ();
		public DTDOccurence Occurence = DTDOccurence.One;

		internal DTDContentModel (DTDObjectModel root, string ownerElementName)
		{
			this.root = root;
			this.ownerElementName = ownerElementName;
		}

		public DTDElementDeclaration ElementDecl {
			get {
			      return root.ElementDecls [ownerElementName];
			}
		}

		public DTDAutomata GetAutomata ()
		{
			if (compiledAutomata == null)
				Compile ();
			return compiledAutomata;
		}

		public DTDAutomata Compile ()
		{
			compiledAutomata = CompileInternal ();
			return compiledAutomata;
		}

		private DTDAutomata CompileInternal ()
		{
			if (ElementDecl.IsAny)
				return root.Any;
			if (ElementDecl.IsEmpty)
				return root.Empty;

			DTDAutomata basis = GetBasicContentAutomata ();
			switch (Occurence) {
			case DTDOccurence.One:
				return basis;
			case DTDOccurence.Optional:
				return Choice (root.Empty, basis);
			case DTDOccurence.OneOrMore:
				return new DTDOneOrMoreAutomata (root, basis);
			case DTDOccurence.ZeroOrMore:
				return Choice (root.Empty, new DTDOneOrMoreAutomata (root, basis));
			}
			throw new InvalidOperationException ();
		}

		private DTDAutomata GetBasicContentAutomata ()
		{
			if (ElementName != null)
				return new DTDElementAutomata (root, ElementName);
			switch (ChildModels.Count) {
			case 0:
				return root.Empty;
			case 1:
				return ChildModels [0].GetAutomata ();
			}

			DTDAutomata current = null;
			int childCount = ChildModels.Count;
			switch (OrderType) {
			case DTDContentOrderType.Seq:
				current = Sequence (
					ChildModels [childCount - 2].GetAutomata (),
					ChildModels [childCount - 1].GetAutomata ());
				for (int i = childCount - 2; i > 0; i--)
					current = Sequence (
						ChildModels [i - 1].GetAutomata (), current);
				return current;
			case DTDContentOrderType.Or:
				current = Choice (
					ChildModels [childCount - 2].GetAutomata (),
					ChildModels [childCount - 1].GetAutomata ());
				for (int i = childCount - 2; i > 0; i--)
					current = Choice (
						ChildModels [i - 1].GetAutomata (), current);
				return current;
			default:
				throw new InvalidOperationException ("Invalid pattern specification");
			}
		}

		private DTDAutomata Sequence (DTDAutomata l, DTDAutomata r)
		{
			return root.Factory.Sequence (l, r);
		}

		private DTDAutomata Choice (DTDAutomata l, DTDAutomata r)
		{
			return l.MakeChoice (r);
		}

	}

	public class DTDContentModelCollection
	{
		ArrayList contentModel = new ArrayList ();

		public DTDContentModelCollection ()
		{
		}

		public DTDContentModel this [int i] {
			get { return contentModel [i] as DTDContentModel; }
		}

		public int Count {
			get { return contentModel.Count; }
		}

		public void Add (DTDContentModel model)
		{
			contentModel.Add (model);
		}
	}

	public abstract class DTDNode
	{
		private DTDObjectModel root;
		public string BaseURI;
		public int LineNumber;
		public int LinePosition;

		internal void SetRoot (DTDObjectModel root)
		{
			this.root = root;
			if (BaseURI == null)
				this.BaseURI = root.BaseURI;
		}

		protected DTDObjectModel Root {
			get { return root; }
		}
	}

	public class DTDElementDeclaration : DTDNode // : ICloneable
	{
		public string Name;
		public bool IsEmpty;
		public bool IsAny;
		public bool IsMixedContent;
		public DTDContentModel contentModel;
		DTDObjectModel root;

		internal DTDElementDeclaration (DTDObjectModel root)
		{
			this.root = root;
		}

		public DTDContentModel ContentModel {
			get {
				if (contentModel == null)
					contentModel = new DTDContentModel (root, Name);
				return contentModel;
			}
		}

		public DTDAttListDeclaration Attributes {
			get {
				return Root.AttListDecls [Name];
			}
		}

//		public object Clone ()
//		{
//			return this.MemberwiseClone ();
//		}
	}

	public class DTDAttributeDefinition : DTDNode// : ICloneable
	{
		public string Name;
		public XmlSchemaDatatype Datatype;
		// entity reference inside enumerated values are not allowed,
		// but on the other hand, they are allowed inside default value.
		// Then I decided to use string ArrayList for enumerated values,
		// and unresolved string value for DefaultValue.
		public ArrayList EnumeratedAttributeDeclaration = new ArrayList ();
		public string UnresolvedDefaultValue = null;
		public ArrayList EnumeratedNotations = new ArrayList();
		public DTDAttributeOccurenceType OccurenceType = DTDAttributeOccurenceType.None;
		private string resolvedDefaultValue;
		private string resolvedNormalizedDefaultValue;

		internal DTDAttributeDefinition () {}

		public string DefaultValue {
			get {
				if (resolvedDefaultValue == null)
					resolvedDefaultValue = ComputeDefaultValue ();
				return resolvedDefaultValue;
			}
		}

		public string NormalizedDefaultValue {
			get {
				if (resolvedNormalizedDefaultValue == null) {
					object o = Datatype.ParseValue (ComputeDefaultValue (), null, null);
					resolvedNormalizedDefaultValue = 
						(o is string []) ? 
						String.Join (" ", (string []) o) :
						o.ToString ();
				}
				return resolvedNormalizedDefaultValue;
			}
		}

		private string ComputeDefaultValue ()
		{
			if (UnresolvedDefaultValue == null)
				return null;

			StringBuilder sb = new StringBuilder ();
			int pos = 0;
			int next = 0;
			string value = this.UnresolvedDefaultValue;
			while ((next = value.IndexOf ('&', pos)) >= 0) {
				int semicolon = value.IndexOf (';', next);
				if (value [next + 1] == '#') {
					// character reference.
					char c = value [next + 2];
					NumberStyles style = NumberStyles.Integer;
					string spec;
					if (c == 'x' || c == 'X') {
						spec = value.Substring (next + 3, semicolon - next - 3);
						style |= NumberStyles.HexNumber;
					}
					else
						spec = value.Substring (next + 2, semicolon - next - 2);
					sb.Append ((char) int.Parse (spec, style));
				} else {
					sb.Append (value.Substring (pos, next - 1));
					string name = value.Substring (pos + 1, semicolon - 1);
					char predefined = XmlChar.GetPredefinedEntity (name);
					if (predefined != 0)
						sb.Append (predefined);
					else
						sb.Append (Root.ResolveEntity (name));
				}
				pos = semicolon + 1;
			}
			sb.Append (value.Substring (pos));
			// strip quote chars
			string ret = sb.ToString (1, sb.Length - 2);
			sb.Length = 0;
			return ret;
		}

		public char QuoteChar {
			get {
				return UnresolvedDefaultValue.Length > 0 ?
					this.UnresolvedDefaultValue [0] :
					'"';
			}
		}

//		public object Clone ()
//		{
//			return this.MemberwiseClone ();
//		}
	}

	public class DTDAttListDeclaration : DTDNode // : ICloneable
	{
		public string Name;

		internal DTDAttListDeclaration (DTDObjectModel root)
		{
			SetRoot (root);
		}

		private Hashtable attributeOrders = new Hashtable ();
		private ArrayList attributes = new ArrayList ();

		public DTDAttributeDefinition this [int i] {
			get { return Get (i); }
		}

		public DTDAttributeDefinition this [string name] {
			get { return Get (name); }
		}

		public DTDAttributeDefinition Get (int i)
		{
			return attributes [i] as DTDAttributeDefinition;
		}

		public DTDAttributeDefinition Get (string name)
		{
			object o = attributeOrders [name];
			if (o != null)
				return attributes [(int) o] as DTDAttributeDefinition;
			else
				return null;
		}

		public ICollection Definitions {
			get { return attributes; }
		}

		public void Add (DTDAttributeDefinition def)
		{
			if (attributeOrders [def.Name] != null)
				throw new InvalidOperationException (String.Format (
					"Attribute definition for {0} was already added at element {1}.",
					def.Name, this.Name));
			def.SetRoot (Root);
			attributeOrders.Add (def.Name, attributes.Count);
			attributes.Add (def);
		}

		public int Count {
			get { return attributeOrders.Count; }
		}

//		public object Clone ()
//		{
//			return this.MemberwiseClone ();
//		}
	}

	public class DTDEntityDeclaration : DTDNode
	{
		string entityValue;

		public string Name;
		public string PublicId;
		public string SystemId;
		public string NotationName;
		public string LiteralEntityValue;
		public bool IsInternalSubset;
		public StringCollection ReferencingEntities = new StringCollection ();
		bool scanned;
		bool recursed;

		public string EntityValue {
			get {
				if (entityValue == null) {
					if (NotationName != null)
						entityValue = "";
					else if (SystemId == null)
						entityValue = LiteralEntityValue;
					else {
						// FIXME: should use specified XmlUrlResolver.
						entityValue = ResolveExternalEntity (Root.Resolver);
					}
					// Check illegal recursion.
					ScanEntityValue (new StringCollection ());
				}
				return entityValue;
			}
		}

		public void ScanEntityValue (StringCollection refs)
		{
			// To modify this code, beware nesting between this and EntityValue.
			string value = EntityValue;

			if (recursed)
				throw new XmlException ("Entity recursion was found.");
			recursed = true;

			if (scanned) {
				foreach (string referenced in refs)
					if (this.ReferencingEntities.Contains (referenced))
						throw new XmlException (String.Format (
							"Nested entity was found between {0} and {1}",
							referenced, Name));
				recursed = false;
				return;
			}

			int len = value.Length;
			int start = 0;
			for (int i=0; i<len; i++) {
				switch (value [i]) {
				case '&':
					start = i+1;
					break;
				case ';':
					if (start == 0)
						break;
					string name = value.Substring (start, i - start);
					this.ReferencingEntities.Add (name);
					DTDEntityDeclaration decl = Root.EntityDecls [name];
					if (decl != null) {
						refs.Add (Name);
						decl.ScanEntityValue (refs);
						foreach (string str in decl.ReferencingEntities)
							ReferencingEntities.Add (str);
						refs.Remove (Name);
					}
					start = 0;
					break;
				}
			}
			scanned = true;
			recursed = false;
		}

		private string ResolveExternalEntity (XmlResolver resolver)
		{
			if (resolver == null)
				return String.Empty;

			string baseUri = Root.BaseURI;
			if (baseUri == "")
				baseUri = null;
			Uri uri = resolver.ResolveUri (
				baseUri != null ? new Uri (baseUri) : null, SystemId);
			Stream stream = resolver.GetEntity (uri, null, typeof (Stream)) as Stream;
			XmlStreamReader reader = new XmlStreamReader (stream, false);

			StringBuilder sb = new StringBuilder ();

			bool checkTextDecl = true;
			while (reader.Peek () != -1) {
				sb.Append ((char) reader.Read ());
				if (checkTextDecl && sb.Length == 6) {
					if (sb.ToString () == "<?xml ") {
						// Skip Text declaration.
						sb.Length = 0;
						StringBuilder textdecl = new StringBuilder ();
						while (reader.Peek () != '>' && reader.Peek () != -1)
							textdecl.Append ((char) reader.Read ());
						if (textdecl.ToString ().IndexOf ("encoding") < 0)
							throw new XmlException ("Text declaration must have encoding specification: " + BaseURI);
						if (textdecl.ToString ().IndexOf ("standalone") >= 0)
							throw new XmlException ("Text declaration cannot have standalone declaration: " + BaseURI);
					}
					checkTextDecl = false;
				}
			}
			return sb.ToString ();
		}

		internal DTDEntityDeclaration (DTDObjectModel root)
		{
			this.SetRoot (root);
		}
	}

	public class DTDNotationDeclaration : DTDNode
	{
		public string Name;
		public string LocalName;
		public string Prefix;
		public string PublicId;
		public string SystemId;

		internal DTDNotationDeclaration () {}
	}

	public class DTDParameterEntityDeclaration : DTDNode
	{
		string resolvedValue;
		Exception loadException;

		public string Name;
		public string PublicId;
		public string SystemId;
		public string LiteralValue;
		public bool LoadFailed;

		public string Value {
			get {
				if (LiteralValue != null)
					return LiteralValue;
				if (resolvedValue == null)
					throw new InvalidOperationException ();
				return resolvedValue;
			}
		}

		public void Resolve (XmlResolver resolver)
		{
			if (resolver == null) {
				resolvedValue = String.Empty;
				LoadFailed = true;
				return;
			}

			Uri baseUri = null;
			try {
				baseUri = new Uri (BaseURI);
			} catch (UriFormatException) {
			}

			Uri absUri = resolver.ResolveUri (baseUri, SystemId);
			string absPath = absUri.ToString ();

			try {
				XmlStreamReader tw = new XmlStreamReader (absUri.ToString (), false, resolver, BaseURI);
				string s = tw.ReadToEnd ();
				if (s.StartsWith ("<?xml")) {
					int end = s.IndexOf (">") + 1;
					if (end < 0)
						throw new XmlException (this as IXmlLineInfo,
							"Inconsistent text declaration markup.");
					if (s.IndexOf ("encoding", 0, end) < 0)
						throw new XmlException (this as IXmlLineInfo,
							"Text declaration must not omit encoding specification.");
					if (s.IndexOf ("standalone", 0, end) >= 0)
						throw new XmlException (this as IXmlLineInfo,
							"Text declaration cannot have standalone declaration.");
					resolvedValue = s.Substring (end);
				}
				else
					resolvedValue = s;
			} catch (IOException ex) {
				loadException = ex;
				resolvedValue = String.Empty;
				LoadFailed = true;
			}
		}
	}

	public enum DTDContentOrderType
	{
		None,
		Seq,
		Or
	}

	public enum DTDAttributeOccurenceType
	{
		None,
		Required,
		Optional,
		Fixed
	}

	public enum DTDOccurence
	{
		One,
		Optional,
		ZeroOrMore,
		OneOrMore
	}
}
