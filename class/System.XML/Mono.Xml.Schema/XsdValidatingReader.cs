//
// Mono.Xml.Schema.XsdValidatingReader.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
// Note:
//
// This class doesn't support set_XmlResolver, since it isn't common to XmlReader interface. 
// Try to set that of xml reader which is used to construct this object.
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml;

namespace Mono.Xml.Schema
{
	public class XsdValidatingReader : XmlReader, IXmlLineInfo, IHasXmlSchemaInfo, IHasXmlParserContext
	{
		char [] wsChars = new char [] {' ', '\t', '\n', '\r'};

		XmlReader reader;
		XmlValidatingReader xvReader;
		IXmlLineInfo readerLineInfo;
		bool laxElementValidation = true;
		bool reportNoValidationError;
		XmlSchemaCollection schemas = new XmlSchemaCollection ();
		bool namespaces = true;

		ArrayList idList = new ArrayList ();
		ArrayList missingIDReferences = new ArrayList ();
		string thisElementId;

		ArrayList keyTables = new ArrayList ();
		ArrayList currentKeyFieldConsumers = new ArrayList ();

		XsdValidationStateManager stateManager = new XsdValidationStateManager ();
		XsdValidationContext context = new XsdValidationContext ();
		XsdValidationState childParticleState;

		int xsiNilDepth = -1;
		StringBuilder storedCharacters = new StringBuilder ();
		bool shouldValidateCharacters;
		int skipValidationDepth = -1;

		XmlSchemaAttribute [] defaultAttributes = new XmlSchemaAttribute [0];
		int currentDefaultAttribute = -1;
		XmlQualifiedName currentQName;

		ArrayList elementQNameStack = new ArrayList ();
		bool popContext;

		// Property Cache.
		int nonDefaultAttributeCount;
		bool defaultAttributeConsumed;

		// Validation engine cached object
		ArrayList defaultAttributesCache = new ArrayList ();
		ArrayList tmpKeyrefPool = new ArrayList ();

#region .ctor
		public XsdValidatingReader (XmlReader reader)
			: this (reader, null)
		{
		}
		
		public XsdValidatingReader (XmlReader reader, XmlReader validatingReader)
		{
			this.reader = reader;
			xvReader = validatingReader as XmlValidatingReader;
			if (xvReader != null) {
				if (xvReader.ValidationType == ValidationType.None)
					reportNoValidationError = true;
			}
			readerLineInfo = reader as IXmlLineInfo;
		}
#endregion
		// Provate Properties
		private XmlQualifiedName CurrentQName {
			get {
				if (currentQName == null)
					currentQName = new XmlQualifiedName (LocalName, NamespaceURI);
				return currentQName;
			}
		}

		internal ArrayList CurrentKeyFieldConsumers {
			get { return currentKeyFieldConsumers; }
		}

		// Public Non-overrides

		public bool Namespaces {
			get { return namespaces; }
			set { namespaces = value; }
		}

		public XmlReader Reader {
			get { return reader; }
		}

		// This should be changed before the first Read() call.
		public XmlSchemaCollection Schemas {
			get { return schemas; }
		}

		public object SchemaType {
			get {
				if (ReadState != ReadState.Interactive)
					return null;

				switch (NodeType) {
				case XmlNodeType.Element:
					if (context.ActualType != null)
						return context.ActualType;
					else if (context.Element != null)
						return context.Element.ElementType;
					else
						return null;
				case XmlNodeType.Attribute:
					// TODO: Default attribute support
					XmlSchemaComplexType ct = context.ActualType as XmlSchemaComplexType;
					if (ct != null) {
						XmlSchemaAttribute attdef = ct.AttributeUses [CurrentQName] as XmlSchemaAttribute;
						if (attdef !=null)
							return attdef.AttributeType;
					}
					return null;
				default:
					return null;
				}
			}
		}

		// This property is never used in Mono.
		public ValidationType ValidationType {
			get {
				if (reportNoValidationError)
					return ValidationType.None;
				else
					return ValidationType.Schema;
			}
		}

		// It is used only for independent XmlReader use, not for XmlValidatingReader.
		public object ReadTypedValue ()
		{
			XmlSchemaDatatype dt = SchemaType as XmlSchemaDatatype;
			XmlSchemaSimpleType st = SchemaType as XmlSchemaSimpleType;
			if (st != null)
				dt = st.Datatype;
			if (dt == null)
				return null;

			switch (NodeType) {
			case XmlNodeType.Element:
				if (IsEmptyElement)
					return null;

				storedCharacters.Length = 0;
				bool loop = true;
				do {
					Read ();
					switch (NodeType) {
					case XmlNodeType.SignificantWhitespace:
					case XmlNodeType.Text:
					case XmlNodeType.CDATA:
						storedCharacters.Append (Value);
						break;
					case XmlNodeType.Comment:
						break;
					default:
						loop = false;
						break;
					}
				} while (loop && !EOF);
				return dt.ParseValue (storedCharacters.ToString (), NameTable, ParserContext.NamespaceManager);
			case XmlNodeType.Attribute:
				return dt.ParseValue (Value, NameTable, ParserContext.NamespaceManager);
			}
			return null;
		}

		public ValidationEventHandler ValidationEventHandler;

		// Public Overrided Properties

		public override int AttributeCount {
			get {
				/*
				if (NodeType == XmlNodeType.Element)
					return attributeCount;
				else if (IsDefault)
					return 0;
				else
					return reader.AttributeCount;
				*/
				return nonDefaultAttributeCount + defaultAttributes.Length;
			}
		}

		public override string BaseURI {
			get { return reader.BaseURI; }
		}

		// If this class is used to implement XmlValidatingReader,
		// it should be left to DTDValidatingReader. In other cases,
		// it depends on the reader's ability.
		public override bool CanResolveEntity {
			get { return reader.CanResolveEntity; }
		}

		public override int Depth {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Depth;
				if (this.defaultAttributeConsumed)
					return reader.Depth + 2;
				return reader.Depth + 1;
			}
		}

		public override bool EOF {
			get { return reader.EOF; }
		}

		public override bool HasValue {
			get {
				if (currentDefaultAttribute < 0)
					return reader.HasValue;
				return true;
			}
		}

		public override bool IsDefault {
			get {
				if (currentDefaultAttribute < 0)
					return reader.IsDefault;
				return true;
			}
		}

		public override bool IsEmptyElement {
			get {
				if (currentDefaultAttribute < 0)
					return reader.IsEmptyElement;
				return false;
			}
		}

		public override string this [int i] {
			get { return GetAttribute (i); }
		}

		public override string this [string name] {
			get { return GetAttribute (name); }
		}

		public override string this [string localName, string ns] {
			get { return GetAttribute (localName, ns); }
		}

		[MonoTODO ("Default values don't have line info.")]
		int IXmlLineInfo.LineNumber {
			get { return readerLineInfo != null ? readerLineInfo.LineNumber : 0; }
		}

		[MonoTODO ("Default values don't have line info.")]
		int IXmlLineInfo.LinePosition {
			get { return readerLineInfo != null ? readerLineInfo.LinePosition : 0; }
		}

		public override string LocalName {
			get {
				if (currentDefaultAttribute < 0)
					return reader.LocalName;
				if (defaultAttributeConsumed)
					return String.Empty;
				return defaultAttributes [currentDefaultAttribute].QualifiedName.Name;
			}
		}

		public override string Name {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Name;
				if (defaultAttributeConsumed)
					return String.Empty;

				XmlQualifiedName qname = defaultAttributes [currentDefaultAttribute].QualifiedName;
				string prefix = Prefix;
				if (prefix == String.Empty)
					return qname.Name;
				else
					return String.Concat (prefix, ":", qname.Name);
			}
		}

		public override string NamespaceURI {
			get {
				if (currentDefaultAttribute < 0)
					return reader.NamespaceURI;
				if (defaultAttributeConsumed)
					return String.Empty;
				return defaultAttributes [currentDefaultAttribute].QualifiedName.Namespace;
			}
		}

		public override XmlNameTable NameTable {
			get { return reader.NameTable; }
		}

		public override XmlNodeType NodeType {
			get {
				if (currentDefaultAttribute < 0)
					return reader.NodeType;
				if (defaultAttributeConsumed)
					return XmlNodeType.Text;
				return XmlNodeType.Attribute;
			}
		}

		public XmlParserContext ParserContext {
			get { return XmlSchemaUtil.GetParserContext (reader); }
		}

		public override string Prefix {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Prefix;
				if (defaultAttributeConsumed)
					return String.Empty;
				XmlQualifiedName qname = defaultAttributes [currentDefaultAttribute].QualifiedName;
				string prefix = this.ParserContext.NamespaceManager.LookupPrefix (qname.Namespace);
				if (prefix == null)
					return String.Empty;
				else
					return prefix;
			}
		}

		public override char QuoteChar {
			get { return reader.QuoteChar; }
		}

		public override ReadState ReadState {
			get { return reader.ReadState; }
		}

		public override string Value {
			get {
				if (currentDefaultAttribute < 0)
					return reader.Value;
				return defaultAttributes [currentDefaultAttribute].ValidatedDefaultValue;
			}
		}

		XmlQualifiedName qnameXmlLang = new XmlQualifiedName ("lang", XmlNamespaceManager.XmlnsXml);

		public override string XmlLang {
			get {
				string xmlLang = reader.XmlLang;
				if (xmlLang != null)
					return xmlLang;
				int idx = this.FindDefaultAttribute ("lang", XmlNamespaceManager.XmlnsXml);
				if (idx < 0)
					return null;
				return defaultAttributes [idx].ValidatedDefaultValue;
			}
		}

		public override XmlSpace XmlSpace {
			get {
				XmlSpace space = reader.XmlSpace;
				if (space != XmlSpace.None)
					return space;
				int idx = this.FindDefaultAttribute ("space", XmlNamespaceManager.XmlnsXml);
				if (idx < 0)
					return XmlSpace.None;
				return (XmlSpace) Enum.Parse (typeof (XmlSpace), defaultAttributes [idx].ValidatedDefaultValue, false);
			}
		}

		// Private Methods

		private XmlQualifiedName QualifyName (string name)
		{
			int colonAt = name.IndexOf (':');
			if (colonAt < 0)
				return new XmlQualifiedName (name, null);
			else
				return new XmlQualifiedName (name.Substring (colonAt + 1),
					LookupNamespace (name.Substring (0, colonAt)));
		}

		private void HandleError (string error)
		{
			HandleError (error, null);
		}

		private void HandleError (string error, Exception innerException)
		{
			if (reportNoValidationError)	// extra quick check
				return;

			XmlSchemaException schemaException = new XmlSchemaException (error, 
					this, this.BaseURI, null, innerException);
			HandleError (schemaException);
		}

		private void HandleError (XmlSchemaException schemaException)
		{
			if (reportNoValidationError)
				return;

			ValidationEventArgs e = new ValidationEventArgs (schemaException,
				schemaException.Message, XmlSeverityType.Error);

			if (this.ValidationEventHandler != null)
				this.ValidationEventHandler (this, e);
			else if (xvReader != null)
				xvReader.OnValidationEvent (this, e);
			else
#if NON_MONO_ENV
				this.xvReader.OnValidationEvent (this, e);
#else
				throw e.Exception;
#endif
		}

		private XmlSchemaElement FindElement (string name, string ns)
		{
			foreach (XmlSchema target in schemas) {
				XmlSchema matches = target.Schemas [reader.NamespaceURI];
				if (matches != null) {
					XmlSchemaElement result = target.Elements [new XmlQualifiedName (reader.LocalName, reader.NamespaceURI)] as XmlSchemaElement;
					if (result != null)
						return result;
				}
			}
			return null;
		}

		private XmlSchemaType FindType (XmlQualifiedName qname)
		{
			foreach (XmlSchema target in schemas) {
				XmlSchemaType type = target.SchemaTypes [qname] as XmlSchemaType;
				if (type != null)
					return type;
			}
			return null;
		}

		private void ValidateStartElementParticle ()
		{
			stateManager.CurrentElement = null;
			context.ParticleState = context.ParticleState.EvaluateStartElement (reader.LocalName, reader.NamespaceURI);
			if (context.ParticleState == XsdValidationState.Invalid)
				HandleError ("Invalid start element: " + reader.NamespaceURI + ":" + reader.LocalName);

			context.Element = stateManager.CurrentElement;
			if (context.Element != null)
				context.SchemaType = context.Element.ElementType;
		}

		private void ValidateEndElementParticle ()
		{
			if (childParticleState != null) {
				if (!childParticleState.EvaluateEndElement ()) {
					HandleError ("Invalid end element: " + reader.Name);
				}
			}
			context.PopScope (reader.Depth);
		}

		// Utility for missing validation completion related to child items.
		private void ValidateCharacters ()
		{
			// TODO: value context validation here.
			if (xsiNilDepth >= 0 && xsiNilDepth < reader.Depth)
				HandleError ("Element item appeared, while current element context is nil.");

			storedCharacters.Append (reader.Value);
		}

		// Utility for missing validation completion related to child items.
		private void ValidateEndCharacters ()
		{
			if (context.ActualType == null)
				return;

			string value = storedCharacters.ToString ();

			if (storedCharacters.Length == 0) {
				// 3.3.4 Element Locally Valid (Element) 5.1.2
				// TODO: check entire DefaultValid (3.3.6)
				if (context.Element != null && context.Element.ValidatedDefaultValue != null)
					value = context.Element.ValidatedDefaultValue;
			}

			XmlSchemaDatatype dt = context.ActualType as XmlSchemaDatatype;
			XmlSchemaSimpleType st = context.ActualType as XmlSchemaSimpleType;
			if (dt == null) {
				if (st != null) {
//					if (st.Variety == XmlSchemaDerivationMethod.Restriction)
					dt = st.Datatype;
				} else {
					XmlSchemaComplexType ct = context.ActualType as XmlSchemaComplexType;
					dt = ct.Datatype;
					switch (ct.ContentType) {
					case XmlSchemaContentType.ElementOnly:
					case XmlSchemaContentType.Empty:
						if (storedCharacters.Length > 0)
							HandleError ("Character content not allowed.");
						break;
					}
				}
			}
			if (dt != null) {
				// 3.3.4 Element Locally Valid (Element) :: 5.2.2.2. Fixed value constraints
				if (context.Element != null && context.Element.ValidatedFixedValue != null)
					if (value != context.Element.ValidatedFixedValue)
						HandleError ("Fixed value constraint was not satisfied.");
				AssessStringValid (st, dt, value);
			}

			// Identity field value
			while (this.currentKeyFieldConsumers.Count > 0) {
				XsdKeyEntryField field = this.currentKeyFieldConsumers [0] as XsdKeyEntryField;
				if (field.Identity != null)
					HandleError ("Two or more identical field was found. Former value is '" + field.Identity + "' .");
				object identity = null;
				if (dt != null) {
					try {
						identity = dt.ParseValue (value, NameTable, ParserContext.NamespaceManager);
					} catch (Exception ex) { // FIXME: This is bad manner ;-(
						// FIXME: Such exception handling is not a good idea.
						HandleError ("Identity value is invalid against its data type " + dt.TokenizedType, ex);
					}
				}
				if (identity == null)
					identity = value;
				
				if (!field.SetIdentityField (identity, dt as XsdAnySimpleType, this))
					HandleError ("Two or more identical key value was found: '" + value + "' .");
				this.currentKeyFieldConsumers.RemoveAt (0);
			}

			shouldValidateCharacters = false;
		}

		// 3.14.4 String Valid 
		private void AssessStringValid (XmlSchemaSimpleType st,
			XmlSchemaDatatype dt, string value)
		{
			XmlSchemaDatatype validatedDatatype = dt;
			if (st != null) {
				string normalized = validatedDatatype.Normalize (value);
				string [] values;
				XmlSchemaDatatype itemDatatype;
				XmlSchemaSimpleType itemSimpleType;
				switch (st.DerivedBy) {
				case XmlSchemaDerivationMethod.List:
					XmlSchemaSimpleTypeList listContent = st.Content as XmlSchemaSimpleTypeList;
					values = normalized.Split (wsChars);
					itemDatatype = listContent.ValidatedListItemType as XmlSchemaDatatype;
					itemSimpleType = listContent.ValidatedListItemType as XmlSchemaSimpleType;
					foreach (string each in values) {
						if (each == String.Empty)
							continue;
						// validate against ValidatedItemType
						if (itemDatatype != null) {
							try {
								itemDatatype.ParseValue (each, NameTable, ParserContext.NamespaceManager);
							} catch (Exception ex) { // FIXME: better exception handling ;-(
								HandleError ("List type value contains one or more invalid values.", ex);
								break;
							}
						}
						else
							AssessStringValid (itemSimpleType, itemSimpleType.Datatype, each);
					}
					break;
				case XmlSchemaDerivationMethod.Union:
					XmlSchemaSimpleTypeUnion union = st.Content as XmlSchemaSimpleTypeUnion;
//					values = normalized.Split (wsChars);
				{
string each = normalized;
//					foreach (string each in values) {
//						if (each == String.Empty)
//							continue;
						// validate against ValidatedItemType
						bool passed = false;
						foreach (object eachType in union.ValidatedTypes) {
							itemDatatype = eachType as XmlSchemaDatatype;
							itemSimpleType = eachType as XmlSchemaSimpleType;
							if (itemDatatype != null) {
								try {
									itemDatatype.ParseValue (each, NameTable, ParserContext.NamespaceManager);
								} catch (Exception) { // FIXME: better exception handling ;-(
									continue;
								}
							}
							else {
								try {
									AssessStringValid (itemSimpleType, itemSimpleType.Datatype, each);
								} catch (XmlSchemaException) {
									continue;
								}
							}
							passed = true;
							break;
						}
						if (!passed) {
							HandleError ("Union type value contains one or more invalid values.");
							break;
						}
					}
					break;
				// TODO: rest
				case XmlSchemaDerivationMethod.Restriction:
					XmlSchemaSimpleTypeRestriction str = st.Content as XmlSchemaSimpleTypeRestriction;
					// facet validation
					if (str != null) {
						if (!str.ValidateValueWithFacets (normalized, NameTable)) {
							HandleError ("Specified value was invalid against the facets.");
							break;
						}
					}
					validatedDatatype = st.Datatype;
					break;
				}
			}
			if (validatedDatatype != null) {
				try {
					validatedDatatype.ParseValue (value, NameTable, ParserContext.NamespaceManager);
				} catch (Exception ex) {	// FIXME: It is really bad design ;-P
					HandleError ("Invalidly typed data was specified.", ex);
				}
			}
		}

		private object GetLocalTypeDefinition (string name)
		{
			object xsiType = null;
			XmlQualifiedName typeQName = QualifyName (name);
			if (typeQName.Namespace == XmlSchema.Namespace) {
				if (typeQName.Name == "anyType")
					xsiType = XmlSchemaComplexType.AnyType;
				else
					xsiType = XmlSchemaDatatype.FromName (typeQName);
			}
			else
				xsiType = FindType (typeQName);
			return xsiType;
		}

		// It is common to ElementLocallyValid::4 and SchemaValidityAssessment::1.2.1.2.4
		private void AssessLocalTypeDerivationOK (object xsiType, object baseType, XmlSchemaDerivationMethod flag)
		{
			XmlSchemaType xsiSchemaType = xsiType as XmlSchemaType;
			XmlSchemaComplexType baseComplexType = baseType as XmlSchemaComplexType;
			XmlSchemaComplexType xsiComplexType = xsiSchemaType as XmlSchemaComplexType;
			if (xsiType != baseType) {
				// Extracted (not extraneous) check for 3.4.6 TypeDerivationOK.
				if (baseComplexType != null)
					flag |= baseComplexType.BlockResolved;
				if (flag == XmlSchemaDerivationMethod.All) {
					HandleError ("Prohibited element type substitution.");
					return;
				} else if (xsiSchemaType != null && (flag & xsiSchemaType.DerivedBy) != 0) {
					HandleError ("Prohibited element type substitution.");
					return;
				}
			}

			if (xsiComplexType != null)
				try {
					xsiComplexType.ValidateTypeDerivationOK (baseType, null, null);
				} catch (XmlSchemaException ex) {
//					HandleError ("Locally specified schema complex type derivation failed. " + ex.Message, ex);
					HandleError (ex);
				}
			else {
				XmlSchemaSimpleType xsiSimpleType = xsiType as XmlSchemaSimpleType;
				if (xsiSimpleType != null) {
					try {
						xsiSimpleType.ValidateTypeDerivationOK (baseType, null, null, true);
					} catch (XmlSchemaException ex) {
//						HandleError ("Locally specified schema simple type derivation failed. " + ex.Message, ex);
						HandleError (ex);
					}
				}
				else if (xsiType is XmlSchemaDatatype) {
					// do nothing
				}
				else
					HandleError ("Primitive data type cannot be derived type using xsi:type specification.");
			}
		}

		// Section 3.3.4 of the spec.
		// [MonoTODO ("normalize xsi:* attributes; supply correct type for TypeDerivationOK()")]
		private void AssessStartElementSchemaValidity ()
		{
			// If the reader is inside xsi:nil (and failed on validation),
			// then simply skip its content.
			if (xsiNilDepth >= 0 && xsiNilDepth < reader.Depth)
				HandleError ("Element item appeared, while current element context is nil.");

			context.Load (reader.Depth);
			if (childParticleState != null) {
				context.ParticleState = childParticleState;
				childParticleState = null;
			}

			// If validation state exists, then first assess particle validity.
			if (context.ParticleState != null) {
				ValidateStartElementParticle ();
			}

			string xsiNilValue = GetAttribute ("nil", XmlSchema.InstanceNamespace);
			bool isXsiNil = xsiNilValue == "true";
			if (isXsiNil && this.xsiNilDepth < 0)
				xsiNilDepth = reader.Depth;

			// [Schema Validity Assessment (Element) 1.2]
			// Evaluate "local type definition" from xsi:type.
			// (See spec 3.3.4 Schema Validity Assessment (Element) 1.2.1.2.3.
			// Note that Schema Validity Assessment(Element) 1.2 takes
			// precedence than 1.1 of that.

			// FIXME: xsi:type value should be normalized.
			string xsiTypeName = reader.GetAttribute ("type", XmlSchema.InstanceNamespace);
			if (xsiTypeName != null) {
				object xsiType = GetLocalTypeDefinition (xsiTypeName);
				if (xsiType == null)
					HandleError ("The instance type was not found: " + xsiTypeName + " .");
				else {
					XmlSchemaType xsiSchemaType = xsiType as XmlSchemaType;
					if (xsiSchemaType != null && this.context.Element != null) {
						XmlSchemaType elemBaseType = context.Element.ElementType as XmlSchemaType;
						if (elemBaseType != null && (xsiSchemaType.DerivedBy & elemBaseType.FinalResolved) != 0)
							HandleError ("The instance type is prohibited by the type of the context element.");
						if (elemBaseType != xsiType && (xsiSchemaType.DerivedBy & this.context.Element.BlockResolved) != 0)
							HandleError ("The instance type is prohibited by the context element.");
					}
					XmlSchemaComplexType xsiComplexType = xsiType as XmlSchemaComplexType;
					if (xsiComplexType != null && xsiComplexType.IsAbstract)
						HandleError ("The instance type is abstract: " + xsiTypeName + " .");
					else {
						// If current schema type exists, then this xsi:type must be
						// valid extension of that type. See 1.2.1.2.4.
						if (context.Element != null) {
							// FIXME: supply *correct* base type
							AssessLocalTypeDerivationOK (xsiType, context.Element.ElementType, context.Element.BlockResolved);
						}
						AssessStartElementLocallyValidType (xsiType);	// 1.2.2:
						context.LocalTypeDefinition = xsiType;
					}
				}
			}

			// Create Validation Root, if not exist.
			// [Schema Validity Assessment (Element) 1.1]
			if (context.Element == null)
				context.Element = FindElement (reader.LocalName, reader.NamespaceURI);
			if (xsiTypeName == null && context.Element != null) {
				context.SchemaType = context.Element.ElementType;
				AssessElementLocallyValidElement (context.Element, xsiNilValue);	// 1.1.2
// FIXME: recover it after xs:any validation implementation.
			} else {
				switch (stateManager.ProcessContents) {
				case XmlSchemaContentProcessing.Skip:
				case XmlSchemaContentProcessing.Lax:
					break;
				default:
					XmlSchema schema = schemas [reader.NamespaceURI];
					if (xsiTypeName == null && (schema != null && !schema.missedSubComponents)) {
						HandleError ("Element declaration for " + reader.LocalName + " is missing.");
					}
					break;
				}
			}

			if (stateManager.ProcessContents == XmlSchemaContentProcessing.Skip)
				skipValidationDepth = reader.Depth;

			// Finally, create child particle state.
			XmlSchemaComplexType xsComplexType = SchemaType as XmlSchemaComplexType;
			if (xsComplexType != null)
				childParticleState = stateManager.Create (xsComplexType.ContentTypeParticle);
			else if (stateManager.ProcessContents == XmlSchemaContentProcessing.Lax)
				childParticleState = stateManager.Create (XmlSchemaAny.AnyTypeContent);
			else
				childParticleState = stateManager.Create (XmlSchemaParticle.Empty);

			AssessStartIdentityConstraints ();

			context.PushScope (reader.Depth);
		}

		// 3.3.4 Element Locally Valid (Element)
		private void AssessElementLocallyValidElement (XmlSchemaElement element, string xsiNilValue)
		{
			XmlQualifiedName qname = new XmlQualifiedName (reader.LocalName, reader.NamespaceURI);
			// 1.
			if (element == null)
				HandleError ("Element declaration is required for " + qname);
			// 2.
			if (element.actualIsAbstract)
				HandleError ("Abstract element declaration was specified for " + qname);
			// 3.1.
			if (!element.actualIsNillable && xsiNilValue != null)
				HandleError ("This element declaration is not nillable: " + qname);
			// 3.2.
			// Note that 3.2.1 xsi:nil constraints are to be validated in
			else if (xsiNilValue == "true") {
				// AssessElementSchemaValidity() and ValidateCharacters()

				if (element.ValidatedFixedValue != null)
					HandleError ("Schema instance nil was specified, where the element declaration for " + qname + "has fixed value constraints.");
			}
			// 4.
			string xsiType = reader.GetAttribute ("type", XmlSchema.InstanceNamespace);
			if (xsiType != null) {
				context.LocalTypeDefinition = GetLocalTypeDefinition (xsiType);
				AssessLocalTypeDerivationOK (context.LocalTypeDefinition, element.ElementType, element.BlockResolved);
			}

			// 5 Not all things cannot be assessed here.
			// It is common to 5.1 and 5.2
			if (element.ElementType != null)
				AssessStartElementLocallyValidType (SchemaType);

			// 6. should be out from here.
			// See invokation of AssessStartIdentityConstraints().

			// 7 is going to be validated in Read() (in case of xmlreader's EOF).
		}

		// 3.3.4 Element Locally Valid (Type)
		private void AssessStartElementLocallyValidType (object schemaType)
		{
			if (schemaType == null) {	// 1.
				HandleError ("Schema type does not exist.");
				return;
			}
			XmlSchemaComplexType cType = schemaType as XmlSchemaComplexType;
			XmlSchemaSimpleType sType = schemaType as XmlSchemaSimpleType;
			if (sType != null) {
				// 3.1.1.
				while (reader.MoveToNextAttribute ()) {
					if (reader.NamespaceURI == XmlNamespaceManager.XmlnsXmlns)
						continue;
					if (reader.NamespaceURI != XmlSchema.InstanceNamespace)
						HandleError ("Current simple type cannot accept attributes other than schema instance namespace.");
					switch (reader.LocalName) {
					case "type":
					case "nil":
					case "schemaLocation":
					case "noNamespaceSchemaLocation":
						break;
					default:
						HandleError ("Unknown schema instance namespace attribute: " + reader.LocalName);
						break;
					}
				}
				reader.MoveToElement ();
				// 3.1.2 and 3.1.3 cannot be assessed here.
			} else if (cType != null) {
				if (cType.IsAbstract) {	// 2.
					HandleError ("Target complex type is abstract.");
					return;
				}
				// 3.2
				AssessElementLocallyValidComplexType (cType);
			}
		}

		// 3.4.4 Element Locally Valid (Complex Type)
		// TODO ("wild IDs constraints.")
		private void AssessElementLocallyValidComplexType (XmlSchemaComplexType cType)
		{
			// 1.
			if (cType.IsAbstract)
				HandleError ("Target complex type is abstract.");

			// 2 (xsi:nil and content prohibition)
			// See AssessStartElementSchemaValidity() and ValidateCharacters()

			string elementNs = reader.NamespaceURI;
			// 3. attribute uses and 
			// 5. wild IDs
			while (reader.MoveToNextAttribute ()) {
				if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
					continue;
				else if (reader.NamespaceURI == XmlSchema.InstanceNamespace)
					continue;
				XmlQualifiedName qname = new XmlQualifiedName (reader.LocalName, reader.NamespaceURI);
				object attMatch = FindAttributeDeclaration (cType, qname, elementNs);
				if (attMatch == null)
					HandleError ("Attribute declaration was not found for " + qname);
				else {
					XmlSchemaAttribute attdecl = attMatch as XmlSchemaAttribute;
					if (attdecl == null) { // i.e. anyAttribute
						XmlSchemaAnyAttribute anyAttrMatch = attMatch as XmlSchemaAnyAttribute;
					} else {
						AssessAttributeLocallyValidUse (attdecl);
						AssessAttributeLocallyValid (attdecl, true);
					}
				}
			}
			reader.MoveToElement ();

			// Collect default attributes.
			// 4.
			// FIXME: FixedValue check maybe extraneous.
			foreach (XmlSchemaAttribute attr in cType.AttributeUses) {
				if (reader [attr.QualifiedName.Name, attr.QualifiedName.Namespace] == null) {
					if (attr.Use == XmlSchemaUse.Required && 
						attr.ValidatedFixedValue == null)
						HandleError ("Required attribute " + attr.QualifiedName + " was not found.");
					else if (attr.ValidatedDefaultValue != null)
						defaultAttributesCache.Add (attr);
				}
			}
			defaultAttributes = (XmlSchemaAttribute []) 
				defaultAttributesCache.ToArray (typeof (XmlSchemaAttribute));
			context.DefaultAttributes = defaultAttributes;
			defaultAttributesCache.Clear ();
			// 5. wild IDs was already checked above.
		}

		// Spec 3.10.4 Item Valid (Wildcard)
		private bool AttributeWildcardItemValid (XmlSchemaAnyAttribute anyAttr, XmlQualifiedName qname)
		{
			if (anyAttr.HasValueAny)
				return true;
			if (anyAttr.HasValueOther && (anyAttr.TargetNamespace == "" || reader.NamespaceURI != anyAttr.TargetNamespace))
				return true;
			if (anyAttr.HasValueTargetNamespace && reader.NamespaceURI == anyAttr.TargetNamespace)
				return true;
			if (anyAttr.HasValueLocal && reader.NamespaceURI == "")
				return true;
			foreach (string ns in anyAttr.ResolvedNamespaces)
				if (ns == reader.NamespaceURI)
					return true;
			return false;
		}

		private XmlSchemaObject FindAttributeDeclaration (XmlSchemaComplexType cType,
			XmlQualifiedName qname, string elementNs)
		{
			XmlSchemaObject result = cType.AttributeUses [qname];
			if (result != null)
				return result;
			if (cType.AttributeWildcard == null)
				return null;

			if (!AttributeWildcardItemValid (cType.AttributeWildcard, qname))
				return null;

			if (cType.AttributeWildcard.ProcessContents == XmlSchemaContentProcessing.Skip)
				return cType.AttributeWildcard;
			foreach (XmlSchema schema in schemas) {
				foreach (XmlSchemaAttribute attr in schema.Attributes)
					if (attr.QualifiedName == qname)
						return attr;
			}
			if (cType.AttributeWildcard.ProcessContents == XmlSchemaContentProcessing.Lax)
				return cType.AttributeWildcard;
			else
				return null;
		}

		// 3.2.4 Attribute Locally Valid and 3.4.4 - 5.wildIDs
		// TODO
		private void AssessAttributeLocallyValid (XmlSchemaAttribute attr, bool checkWildIDs)
		{
			// 1.
			switch (reader.NamespaceURI) {
			case XmlNamespaceManager.XmlnsXml:
			case XmlNamespaceManager.XmlnsXmlns:
			case XmlSchema.InstanceNamespace:
				break;
			}
			// TODO 2. - 4.
			if (attr.AttributeType == null)
				HandleError ("Attribute type is missing for " + attr.QualifiedName);
			XmlSchemaDatatype dt = attr.AttributeType as XmlSchemaDatatype;
			if (dt == null)
				dt = ((XmlSchemaSimpleType) attr.AttributeType).Datatype;
			// It is a bit heavy process, so let's omit as long as possible ;-)
			if (dt != XmlSchemaSimpleType.AnySimpleType || attr.ValidatedFixedValue != null) {
				string normalized = dt.Normalize (reader.Value);
				object parsedValue = null;
				try {
					dt.ParseValue (normalized, reader.NameTable, this.ParserContext.NamespaceManager);
				} catch (Exception ex) {
					// FIXME: Such exception handling is not a good idea.
					HandleError ("Attribute value is invalid against its data type " + dt.TokenizedType, ex);
				}
				if (attr.ValidatedFixedValue != null && attr.ValidatedFixedValue != normalized)
					HandleError ("The value of the attribute " + attr.QualifiedName + " does not match with its fixed value.");
				// FIXME: this is extraneous checks in 3.2.4 Attribute Locally Valid.
				if (checkWildIDs)
					AssessEachAttributeIdentityConstraint (dt, normalized, parsedValue);
			}
		}

		private void AssessEachAttributeIdentityConstraint (XmlSchemaDatatype dt,
			string normalized, object parsedValue)
		{
			// Get normalized value and (if required) parsedValue if missing.
			switch (dt.TokenizedType) {
			case XmlTokenizedType.IDREFS:
				if (normalized == null)
					normalized = dt.Normalize (reader.Value);
				if (parsedValue == null)
					parsedValue = dt.ParseValue (normalized, reader.NameTable, ParserContext.NamespaceManager);
				break;
			case XmlTokenizedType.ID:
			case XmlTokenizedType.IDREF:
				if (normalized == null)
					normalized = dt.Normalize (reader.Value);
				break;
			}

			// Validate identity constraints.
			switch (dt.TokenizedType) {
			case XmlTokenizedType.ID:
				if (thisElementId != null)
					HandleError ("ID type attribute was already assigned in the containing element.");
				thisElementId = normalized;
				idList.Add (normalized);
				break;
			case XmlTokenizedType.IDREF:
				if (missingIDReferences.Contains (normalized))
					missingIDReferences.Remove (normalized);
				else
					missingIDReferences.Add (normalized);
				break;
			case XmlTokenizedType.IDREFS:
				foreach (string id in (string []) parsedValue) {
					if (missingIDReferences.Contains (id))
						missingIDReferences.Remove (id);
					else
						missingIDReferences.Add (id);
				}
				break;
			}
		}

		// TODO
		private void AssessAttributeLocallyValidUse (XmlSchemaAttribute attr)
		{
			// TODO: value constraint check
			// This is extra check than spec 3.5.4
			if (attr.Use == XmlSchemaUse.Prohibited)
				HandleError ("Attribute " + attr.QualifiedName + " is prohibited in this context.");
		}

		private void AssessEndElementSchemaValidity ()
		{
			if (childParticleState == null)
				childParticleState = context.ParticleState;
			ValidateEndElementParticle ();	// validate against childrens' state.

			context.Load (reader.Depth);

			// 3.3.4 Assess ElementLocallyValidElement 5: value constraints.
			// 3.3.4 Assess ElementLocallyValidType 3.1.3. = StringValid(3.14.4)
			// => ValidateEndCharacters().

			// Reset Identity constraints.
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable keyTable = this.keyTables [i] as XsdKeyTable;
				if (keyTable.StartDepth == reader.Depth) {
					EndIdentityValidation (keyTable);
				} else {
					for (int k = 0; k < keyTable.Entries.Count; k++) {
						XsdKeyEntry entry = keyTable.Entries [k] as XsdKeyEntry;
						// Remove finished (maybe key not found) entries.
						if (entry.StartDepth == reader.Depth) {
							if (entry.KeyFound)
								keyTable.FinishedEntries.Add (entry);
							else if (entry.KeySequence.SourceSchemaIdentity is XmlSchemaKey)
								HandleError ("Key sequence is missing.");
							keyTable.Entries.RemoveAt (k);
							k--;
						}
						// Pop validated key depth to find two or more fields.
						else {
							foreach (XsdKeyEntryField kf in entry.KeyFields) {
								if (!kf.FieldFound && kf.FieldFoundDepth == reader.Depth) {
									kf.FieldFoundDepth = 0;
									kf.FieldFoundPath = null;
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < keyTables.Count; i++) {
				XsdKeyTable keyseq = this.keyTables [i] as XsdKeyTable;
				if (keyseq.StartDepth == reader.Depth) {
//Console.WriteLine ("Finishing table.");
					keyTables.RemoveAt (i);
					i--;
				}
			}

			// Reset xsi:nil, if required.
			if (xsiNilDepth == reader.Depth)
				xsiNilDepth = -1;
		}

		// 3.11.4 Identity Constraint Satisfied
		// TODO
		private void AssessStartIdentityConstraints ()
		{
			tmpKeyrefPool.Clear ();
			if (context.Element != null && context.Element.Constraints.Count > 0) {
				// (a) Create new key sequences, if required.
				foreach (XmlSchemaIdentityConstraint ident in context.Element.Constraints) {
					XsdKeyTable seq = CreateNewKeyTable (ident);
					if (ident is XmlSchemaKeyref)
						tmpKeyrefPool.Add (seq);
				}
			}

			// (b) Evaluate current key sequences.
			foreach (XsdKeyTable seq in this.keyTables) {
				if (seq.SelectorMatches (this.elementQNameStack, reader) != null) {
					// creates and registers new entry.
					XsdKeyEntry entry = new XsdKeyEntry (seq, reader);
					seq./*NotFound*/Entries.Add (entry);
				}
			}

			// (c) Evaluate field paths.
			foreach (XsdKeyTable seq in this.keyTables) {
				// If possible, create new field entry candidates.
				for (int i = 0; i < seq./*NotFound*/Entries.Count; i++) {
					XsdKeyEntry entry = seq./*NotFound*/Entries [i] as XsdKeyEntry;
//					if (entry.KeyFound)
// FIXME: it should not be skipped for multiple key check!!
//						continue;
					try {
						entry.FieldMatches (this.elementQNameStack, this);
					} catch (Exception ex) {
						// FIXME: Such exception handling is not a good idea.
						HandleError ("Identity field value is invalid against its data type.", ex);
					}
				}
			}
		}

		private XsdKeyTable CreateNewKeyTable (XmlSchemaIdentityConstraint ident)
		{
			XsdKeyTable seq = new XsdKeyTable (ident, this);
			seq.StartDepth = reader.Depth;
			XmlSchemaKeyref keyref = ident as XmlSchemaKeyref;
			this.keyTables.Add (seq);
			return seq;
		}

		private void EndIdentityValidation (XsdKeyTable seq)
		{
			ArrayList errors = new ArrayList ();
			foreach (XsdKeyEntry entry in seq./*NotFound*/Entries) {
				if (entry.KeyFound)
					continue;
				if (seq.SourceSchemaIdentity is XmlSchemaKey)
					errors.Add ("line " + entry.SelectorLineNumber + "position " + entry.SelectorLinePosition);
			}
			if (errors.Count > 0)
				HandleError ("Invalid identity constraints were found. Key was not found. "
					+ String.Join (", ", errors.ToArray (typeof (string)) as string []));

			errors.Clear ();
			// Find reference target
			XmlSchemaKeyref xsdKeyref = seq.SourceSchemaIdentity as XmlSchemaKeyref;
			if (xsdKeyref != null) {
				for (int i = this.keyTables.Count - 1; i >= 0; i--) {
					XsdKeyTable target = this.keyTables [i] as XsdKeyTable;
					if (target.SourceSchemaIdentity == xsdKeyref.Target) {
						seq.ReferencedKey = target;
						foreach (XsdKeyEntry entry in seq.FinishedEntries) {
							foreach (XsdKeyEntry targetEntry in target.FinishedEntries) {
								if (entry.CompareIdentity (targetEntry)) {
									entry.KeyRefFound = true;
									break;
								}
							}
						}
					}
				}
				if (seq.ReferencedKey == null)
					HandleError ("Target key was not found.");
				foreach (XsdKeyEntry entry in seq.FinishedEntries) {
					if (!entry.KeyRefFound)
						errors.Add (" line " + entry.SelectorLineNumber + ", position " + entry.SelectorLinePosition);
				}
				if (errors.Count > 0)
					HandleError ("Invalid identity constraints were found. Referenced key was not found: "
						+ String.Join (" / ", errors.ToArray (typeof (string)) as string []));
			}
		}

		// Overrided Methods

		public override void Close ()
		{
			reader.Close ();
		}

		public override string GetAttribute (int i)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (i);
			}

			if (reader.AttributeCount > i)
				reader.GetAttribute (i);
			int defIdx = i - nonDefaultAttributeCount;
			if (i < AttributeCount)
				return defaultAttributes [defIdx].DefaultValue;

			throw new ArgumentOutOfRangeException ("i", i, "Specified attribute index is out of range.");
		}

		public override string GetAttribute (string name)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (name);
			}

			string value = reader.GetAttribute (name);
			if (value != null)
				return value;

			XmlQualifiedName qname = SplitQName (name);
			return GetDefaultAttribute (qname.Name, qname.Namespace);
		}

		private XmlQualifiedName SplitQName (string name)
		{
			if (!XmlChar.IsName (name))
				throw new ArgumentException ("Invalid name was specified.", "name");

			Exception ex = null;
			XmlQualifiedName qname = XmlSchemaUtil.ToQName (reader, name, out ex);
			if (ex != null)
				return XmlQualifiedName.Empty;
			else
				return qname;
		}

		public override string GetAttribute (string localName, string ns)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.GetAttribute (localName, ns);
			}

			string value = reader.GetAttribute (localName, ns);
			if (value != null)
				return value;

			return GetDefaultAttribute (localName, ns);
		}

		private string GetDefaultAttribute (string localName, string ns)
		{
			int idx = this.FindDefaultAttribute (localName, ns);
			if (idx >= 0)
				return defaultAttributes [idx].ValidatedDefaultValue;
			else
				return null;
		}

		private int FindDefaultAttribute (string localName, string ns)
		{
			for (int i = 0; i < this.defaultAttributes.Length; i++) {
				XmlSchemaAttribute attr = defaultAttributes [i];
				if (attr.QualifiedName.Name == localName &&
					attr.QualifiedName.Namespace == ns)
					return i;
			}
			return -1;
		}

		[MonoTODO ("When it is default attribute, does it works?")]
		bool IXmlLineInfo.HasLineInfo ()
		{
			return readerLineInfo != null && readerLineInfo.HasLineInfo ();
		}

		public override string LookupNamespace (string prefix)
		{
			return reader.LookupNamespace (prefix);
		}

		public override void MoveToAttribute (int i)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				reader.MoveToAttribute (i);
				return;
			}

			currentQName = null;
			if (i < this.nonDefaultAttributeCount) {
				reader.MoveToAttribute (i);
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
			}

			if (i < AttributeCount) {
				this.currentDefaultAttribute = i - nonDefaultAttributeCount;
				this.defaultAttributeConsumed = false;
			}
			else
				throw new ArgumentOutOfRangeException ("i", i, "Attribute index is out of range.");
		}

		public override bool MoveToAttribute (string name)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToAttribute (name);
			}

			currentQName = null;
			bool b = reader.MoveToAttribute (name);
			if (b) {
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
				return true;
			}

			XmlQualifiedName qname = SplitQName (name);
			return MoveToDefaultAttribute (qname.Name, qname.Namespace);
		}

		public override bool MoveToAttribute (string localName, string ns)
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToAttribute (localName, ns);
			}

			currentQName = null;
			bool b = reader.MoveToAttribute (localName, ns);
			if (b) {
				this.currentDefaultAttribute = -1;
				this.defaultAttributeConsumed = false;
				return true;
			}

			return MoveToDefaultAttribute (localName, ns);
		}

		private bool MoveToDefaultAttribute (string localName, string ns)
		{
			int idx = this.FindDefaultAttribute (localName, ns);
			if (idx < 0)
				return false;
			currentDefaultAttribute = idx;
			defaultAttributeConsumed = false;
			return true;
		}

		public override bool MoveToElement ()
		{
			currentDefaultAttribute = -1;
			defaultAttributeConsumed = false;
			currentQName = null;
			return reader.MoveToElement ();
		}

		public override bool MoveToFirstAttribute ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToFirstAttribute ();
			}

			currentQName = null;
			if (this.nonDefaultAttributeCount > 0) {
				bool b = reader.MoveToFirstAttribute ();
				if (b) {
					currentDefaultAttribute = -1;
					defaultAttributeConsumed = false;
				}
				return b;
			}

			if (this.defaultAttributes.Length > 0) {
				currentDefaultAttribute = 0;
				defaultAttributeConsumed = false;
				return true;
			}
			else
				return false;
		}

		public override bool MoveToNextAttribute ()
		{
			switch (reader.NodeType) {
			case XmlNodeType.XmlDeclaration:
			case XmlNodeType.DocumentType:
				return reader.MoveToNextAttribute ();
			}

			currentQName = null;
			if (currentDefaultAttribute >= 0) {
				if (defaultAttributes.Length == currentDefaultAttribute + 1)
					return false;
				currentDefaultAttribute++;
				defaultAttributeConsumed = false;
				return true;
			}

			bool b = reader.MoveToNextAttribute ();
			if (b) {
				currentDefaultAttribute = -1;
				defaultAttributeConsumed = false;
				return true;
			}

			if (defaultAttributes.Length > 0) {
				currentDefaultAttribute = 0;
				defaultAttributeConsumed = false;
				return true;
			}
			else
				return false;
		}

		private void ExamineAdditionalSchema ()
		{
			XmlSchema schema = null;
			string schemaLocation = reader.GetAttribute ("schemaLocation", XmlSchema.InstanceNamespace);
			if (schemaLocation != null) {
				string [] tmp = XmlSchemaDatatype.FromName ("NMTOKENS").ParseValue (schemaLocation, NameTable, null) as string [];
				if (tmp.Length % 2 != 0)
					HandleError ("Invalid schemaLocation attribute format.");
				for (int i = 0; i < tmp.Length; i += 2) {
					try {
						Uri absUri = new Uri ((this.BaseURI != "" ? new Uri (BaseURI) : null), tmp [i + 1]);
						XmlTextReader xtr = new XmlTextReader (absUri.ToString ());
						schema = XmlSchema.Read (xtr, null);
					} catch (Exception ex) {
						// FIXME: better exception handling...
//						HandleError ("Errors were occured when resolving schemaLocation specified schema document.", ex);
						continue;
					}
					if (schema.TargetNamespace == null)
						schema.TargetNamespace = tmp [i];
					else if (schema.TargetNamespace != tmp [i])
						HandleError ("Specified schema has different target namespace.");
				}
			}
			if (schema != null) {
				try {
					schemas.Add (schema);
				} catch (XmlSchemaException ex) {
					HandleError (ex);
				}
			}
			schema = null;
			string noNsSchemaLocation = reader.GetAttribute ("noNamespaceSchemaLocation", XmlSchema.InstanceNamespace);
			if (noNsSchemaLocation != null) {
				try {
					Uri absUri = new Uri ((this.BaseURI != "" ? new Uri (BaseURI) : null), noNsSchemaLocation);
					XmlTextReader xtr = new XmlTextReader (absUri.ToString ());
					schema = XmlSchema.Read (xtr, null);
				} catch (Exception ex) {
					// FIXME: better exception handling...
//					HandleError ("Errors were occured when resolving schemaLocation specified schema document.", ex);
				}
				if (schema != null && schema.TargetNamespace != null)
					HandleError ("Specified schema has different target namespace.");
			}
			if (schema != null) {
				try {
					schemas.Add (schema);
				} catch (XmlSchemaException ex) {
					HandleError (ex);
				}
			}
		}

		public override bool Read ()
		{
			nonDefaultAttributeCount = 0;
			currentDefaultAttribute = -1;
			defaultAttributeConsumed = false;
			currentQName = null;
			thisElementId = null;
			defaultAttributes = new XmlSchemaAttribute [0];
			if (popContext) {
				elementQNameStack.RemoveAt (elementQNameStack.Count - 1);
				popContext = false;
			}

			bool result = reader.Read ();
			// 3.3.4 ElementLocallyValidElement 7 = Root Valid.
			if (!result && missingIDReferences.Count > 0)
				HandleError ("There are missing ID references: " +
					String.Join (" ",
					this.missingIDReferences.ToArray (typeof (string)) as string []));

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				nonDefaultAttributeCount = reader.AttributeCount;

				if (reader.Depth == 0)
					ExamineAdditionalSchema ();

				this.elementQNameStack.Add (new XmlQualifiedName (reader.LocalName, reader.NamespaceURI));

				// If there is no schema information, then no validation is performed.
				if (schemas.Count == 0)
					break;

//				context.Load (reader.Depth);
				if (skipValidationDepth < 0 || reader.Depth <= skipValidationDepth) {
					if (shouldValidateCharacters) {
						ValidateEndCharacters ();
						shouldValidateCharacters = false;
					}
					AssessStartElementSchemaValidity ();
					storedCharacters.Length = 0;
				} else {
					context.Clear ();
				}

				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				else
					shouldValidateCharacters = true;
				break;
			case XmlNodeType.EndElement:
				if (reader.Depth == skipValidationDepth) {
					skipValidationDepth = -1;
					context.Clear ();
				} else {
//					context.Load (reader.Depth);
					if (shouldValidateCharacters) {
						ValidateEndCharacters ();
						shouldValidateCharacters = false;
					}
					AssessEndElementSchemaValidity ();
				}
				storedCharacters.Length = 0;
				childParticleState = null;
				popContext = true;
				break;

			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
			case XmlNodeType.Text:
				XmlSchemaComplexType ct = context.ActualType as XmlSchemaComplexType;
				if (ct != null && storedCharacters.Length > 0) {
					switch (ct.ContentType) {
					case XmlSchemaContentType.ElementOnly:
					case XmlSchemaContentType.Empty:
						HandleError ("Not allowed character content was found.");
						break;
					}
				}

				ValidateCharacters ();
				break;
			}

			return result;
		}

		public override bool ReadAttributeValue ()
		{
			if (currentDefaultAttribute < 0)
				return reader.ReadAttributeValue ();

			if (this.defaultAttributeConsumed)
				return false;

			defaultAttributeConsumed = true;
			return true;
		}

#if NET_1_0
		public override string ReadInnerXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadInnerXml ();
		}

		public override string ReadOuterXml ()
		{
			// MS.NET 1.0 has a serious bug here. It skips validation.
			return reader.ReadOuterXml ();
		}
#endif

		// XmlReader.ReadString() should call derived this.Read().
		public override string ReadString ()
		{
#if NET_1_0
			return reader.ReadString ();
#else
			return base.ReadString ();
#endif
		}

		// This class itself does not have this feature.
		public override void ResolveEntity ()
		{
			reader.ResolveEntity ();
		}

		internal class XsdValidationContext
		{
			Hashtable contextStack;

			public XsdValidationContext ()
			{
				contextStack = new Hashtable ();
			}

			// Some of them might be missing (See the spec section 5.3, and also 3.3.4).
			public XmlSchemaElement Element;
			public XsdValidationState ParticleState;
			public XmlSchemaAttribute [] DefaultAttributes;

			// Some of them might be missing (See the spec section 5.3).
			public object SchemaType;

			public object LocalTypeDefinition;

			public object ActualType {
				get {
					if (LocalTypeDefinition != null)
						return LocalTypeDefinition;
					else
						return SchemaType;
				}
			}

			public void Clear ()
			{
				Element = null;
				SchemaType = null;
				ParticleState = null;
				LocalTypeDefinition = null;
			}

			public void PushScope (int depth)
			{
				contextStack [depth] = this.MemberwiseClone ();
			}

			public void PopScope (int depth)
			{
				Load (depth);
				contextStack.Remove (depth + 1);
			}

			public void Load (int depth)
			{
				Clear ();
				XsdValidationContext restored = (XsdValidationContext) contextStack [depth];
				if (restored != null) {
					this.Element = restored.Element;
					this.ParticleState = restored.ParticleState;
					this.SchemaType = restored.SchemaType;
					this.LocalTypeDefinition = restored.LocalTypeDefinition;
				}
			}
		}

		/*
		internal class XsdValidityState
		{
			ArrayList currentParticles = new ArrayList ();
			ArrayList occured = new ArrayList ();
			Hashtable xsAllConsumed = new Hashtable ();
			XmlSchemaParticle parciele;
			int particleDepth;

			public XsdValidityState (XmlSchemaParticle particle)
			{
				this.parciele = particle;
				currentParticles.Add (particle);
			}

		}
		*/
	}

}
