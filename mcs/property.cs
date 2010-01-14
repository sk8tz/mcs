//
// property.cs: Property based handlers
//
// Authors: Miguel de Icaza (miguel@gnu.org)
//          Martin Baulig (martin@ximian.com)
//          Marek Safar (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004-2008 Novell, Inc
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

#if NET_2_1
using XmlElement = System.Object;
#else
using System.Xml;
#endif

using Mono.CompilerServices.SymbolWriter;

namespace Mono.CSharp
{
	// It is used as a base class for all property based members
	// This includes properties, indexers, and events
	public abstract class PropertyBasedMember : InterfaceMemberBase
	{
		public PropertyBasedMember (DeclSpace parent, GenericMethod generic,
			FullNamedExpression type, Modifiers mod, Modifiers allowed_mod,
			MemberName name, Attributes attrs)
			: base (parent, generic, type, mod, allowed_mod, name, attrs)
		{
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			if (!AttributeTester.IsClsCompliant (MemberType)) {
				Report.Warning (3003, 1, Location, "Type of `{0}' is not CLS-compliant",
					GetSignatureForError ());
			}
			return true;
		}

	}

	//
	// `set' and `get' accessors are represented with an Accessor.
	// 
	public class Accessor {
		//
		// Null if the accessor is empty, or a Block if not
		//
		public const Modifiers AllowedModifiers = 
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE;
		
		public ToplevelBlock Block;
		public Attributes Attributes;
		public Location Location;
		public Modifiers ModFlags;
		public ParametersCompiled Parameters;
		
		public Accessor (ToplevelBlock b, Modifiers mod, Attributes attrs, ParametersCompiled p, Location loc)
		{
			Block = b;
			Attributes = attrs;
			Location = loc;
			Parameters = p;
			ModFlags = ModifiersExtensions.Check (AllowedModifiers, mod, 0, loc, RootContext.ToplevelTypes.Compiler.Report);
		}
	}

	public class PropertySpec : MemberSpec
	{
		PropertyInfo info;

		public PropertySpec (MemberKind kind, IMemberDefinition definition, PropertyInfo info, Modifiers modifiers)
			: base (kind, definition, info.Name, modifiers)
		{
			this.info = info;
		}

		public override Type DeclaringType {
			get { return info.DeclaringType; }
		}

		public PropertyInfo MetaInfo {
			get { return info; }
		}

		public Type PropertyType {
			get { return info.PropertyType; }
		}
	}

	//
	// Properties and Indexers both generate PropertyBuilders, we use this to share 
	// their common bits.
	//
	abstract public class PropertyBase : PropertyBasedMember {

		public class GetMethod : PropertyMethod
		{
			static string[] attribute_targets = new string [] { "method", "return" };

			public GetMethod (PropertyBase method):
				base (method, "get_")
			{
			}

			public GetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "get_")
			{
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				base.Define (parent);

				if (IsDummy)
					return null;
				
				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return method.MemberType;
				}
			}

			public override ParametersCompiled ParameterInfo {
				get {
					return ParametersCompiled.EmptyReadOnlyParameters;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		public class SetMethod : PropertyMethod {

			static string[] attribute_targets = new string [] { "method", "param", "return" };
			ImplicitParameter param_attr;
			protected ParametersCompiled parameters;

			public SetMethod (PropertyBase method) :
				base (method, "set_")
			{
				parameters = new ParametersCompiled (Compiler,
					new Parameter (method.type_name, "value", Parameter.Modifier.NONE, null, Location));
			}

			public SetMethod (PropertyBase method, Accessor accessor):
				base (method, accessor, "set_")
			{
				this.parameters = accessor.Parameters;
			}

			protected override void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb, pa);
					return;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			public override ParametersCompiled ParameterInfo {
			    get {
			        return parameters;
			    }
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				parameters.Resolve (this);
				
				base.Define (parent);

				if (IsDummy)
					return null;

				method_data = new MethodData (method, ModFlags, flags, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				return method_data.MethodBuilder;
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}
		}

		static string[] attribute_targets = new string [] { "property" };

		public abstract class PropertyMethod : AbstractPropertyEventMethod
		{
			protected readonly PropertyBase method;
			protected MethodAttributes flags;

			public PropertyMethod (PropertyBase method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE);				
			}

			public PropertyMethod (PropertyBase method, Accessor accessor,
					       string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = accessor.ModFlags | (method.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE));

				if (accessor.ModFlags != 0 && RootContext.Version == LanguageVersion.ISO_1) {
					Report.FeatureIsNotAvailable (Location, "access modifiers on properties");
				}
			}

			public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.IsInternalMethodImplAttribute) {
					method.is_external_implementation = true;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				CheckForDuplications ();

				if (IsDummy) {
					if (method.InterfaceType != null && parent.PartialContainer.PendingImplementations != null) {
						MethodInfo mi = parent.PartialContainer.PendingImplementations.IsInterfaceMethod (
							MethodName.Name, method.InterfaceType, new MethodData (method, ModFlags, flags, this));
						if (mi != null) {
							Report.SymbolRelatedToPreviousError (mi);
							Report.Error (551, Location, "Explicit interface implementation `{0}' is missing accessor `{1}'",
								method.GetSignatureForError (), TypeManager.CSharpSignature (mi, true));
						}
					}
					return null;
				}

				TypeContainer container = parent.PartialContainer;

				//
				// Check for custom access modifier
				//
				if ((ModFlags & Modifiers.AccessibilityMask) == 0) {
					ModFlags |= method.ModFlags;
					flags = method.flags;
				} else {
					if (container.Kind == Kind.Interface)
						Report.Error (275, Location, "`{0}': accessibility modifiers may not be used on accessors in an interface",
							GetSignatureForError ());

					if ((method.ModFlags & Modifiers.ABSTRACT) != 0 && (ModFlags & Modifiers.PRIVATE) != 0) {
						Report.Error (442, Location, "`{0}': abstract properties cannot have private accessors", GetSignatureForError ());
					}

					CheckModifiers (ModFlags);
					ModFlags |= (method.ModFlags & (~Modifiers.AccessibilityMask));
					ModFlags |= Modifiers.PROPERTY_CUSTOM;
					flags = ModifiersExtensions.MethodAttr (ModFlags);
					flags |= (method.flags & (~MethodAttributes.MemberAccessMask));
				}

				CheckAbstractAndExtern (block != null);
				CheckProtectedModifier ();

				if (block != null && block.IsIterator)
					Iterator.CreateIterator (this, Parent.PartialContainer, ModFlags, Compiler);

				return null;
			}

			public bool HasCustomAccessModifier {
				get {
					return (ModFlags & Modifiers.PROPERTY_CUSTOM) != 0;
				}
			}

			public PropertyBase Property {
				get {
					return method;
				}
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string GetSignatureForError()
			{
				return method.GetSignatureForError () + '.' + prefix.Substring (0, 3);
			}

			void CheckModifiers (Modifiers modflags)
			{
				modflags &= Modifiers.AccessibilityMask;
				Modifiers flags = 0;
				Modifiers mflags = method.ModFlags & Modifiers.AccessibilityMask;

				if ((mflags & Modifiers.PUBLIC) != 0) {
					flags |= Modifiers.PROTECTED | Modifiers.INTERNAL | Modifiers.PRIVATE;
				}
				else if ((mflags & Modifiers.PROTECTED) != 0) {
					if ((mflags & Modifiers.INTERNAL) != 0)
						flags |= Modifiers.PROTECTED | Modifiers.INTERNAL;

					flags |= Modifiers.PRIVATE;
				} else if ((mflags & Modifiers.INTERNAL) != 0)
					flags |= Modifiers.PRIVATE;

				if ((mflags == modflags) || (modflags & (~flags)) != 0) {
					Report.Error (273, Location,
						"The accessibility modifier of the `{0}' accessor must be more restrictive than the modifier of the property or indexer `{1}'",
						GetSignatureForError (), method.GetSignatureForError ());
				}
			}

			protected bool CheckForDuplications ()
			{
				if ((caching_flags & Flags.MethodOverloadsExist) == 0)
					return true;

				return Parent.MemberCache.CheckExistingMembersOverloads (this, Name, ParameterInfo, Report);
			}
		}

		public PropertyMethod Get, Set;
		public PropertyBuilder PropertyBuilder;
		public MethodBuilder GetBuilder, SetBuilder;

		protected bool define_set_first = false;

		public PropertyBase (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags,
				     Modifiers allowed_mod, MemberName name,
				     Attributes attrs, bool define_set_first)
			: base (parent, null, type, mod_flags, allowed_mod, name, attrs)
		{
			 this.define_set_first = define_set_first;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.HasSecurityAttribute) {
				a.Error_InvalidSecurityParent ();
				return;
			}

			if (a.Type == pa.Dynamic) {
				a.Error_MisusedDynamicAttribute ();
				return;
			}

			PropertyBuilder.SetCustomAttribute (cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Property;
			}
		}

		protected override void DoMemberTypeDependentChecks ()
		{
			base.DoMemberTypeDependentChecks ();

			IsTypePermitted ();
#if MS_COMPATIBLE
			if (MemberType.IsGenericParameter)
				return;
#endif

			if ((MemberType.Attributes & Class.StaticClassAttribute) == Class.StaticClassAttribute) {
				Report.Error (722, Location, Error722, TypeManager.CSharpName (MemberType));
			}
		}

		protected override void DoMemberTypeIndependentChecks ()
		{
			base.DoMemberTypeIndependentChecks ();

			//
			// Accessors modifiers check
			//
			if ((Get.ModFlags & Modifiers.AccessibilityMask) != 0 &&
				(Set.ModFlags & Modifiers.AccessibilityMask) != 0) {
				Report.Error (274, Location, "`{0}': Cannot specify accessibility modifiers for both accessors of the property or indexer",
						GetSignatureForError ());
			}

			if ((ModFlags & Modifiers.OVERRIDE) == 0 && 
				(Get.IsDummy && (Set.ModFlags & Modifiers.AccessibilityMask) != 0) ||
				(Set.IsDummy && (Get.ModFlags & Modifiers.AccessibilityMask) != 0)) {
				Report.Error (276, Location, 
					      "`{0}': accessibility modifiers on accessors may only be used if the property or indexer has both a get and a set accessor",
					      GetSignatureForError ());
			}
		}

		bool DefineGet ()
		{
			GetBuilder = Get.Define (Parent);
			return (Get.IsDummy) ? true : GetBuilder != null;
		}

		bool DefineSet (bool define)
		{
			if (!define)
				return true;

			SetBuilder = Set.Define (Parent);
			return (Set.IsDummy) ? true : SetBuilder != null;
		}

		protected bool DefineAccessors ()
		{
			return DefineSet (define_set_first) &&
				DefineGet () &&
				DefineSet (!define_set_first);
		}

		protected abstract PropertyInfo ResolveBaseProperty ();

		// TODO: rename to Resolve......
 		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
 		{
 			PropertyInfo base_property = ResolveBaseProperty ();
 			if (base_property == null)
 				return null;

 			base_ret_type = base_property.PropertyType;
			MethodInfo get_accessor = base_property.GetGetMethod (true);
			MethodInfo set_accessor = base_property.GetSetMethod (true);
			MethodAttributes get_accessor_access = 0, set_accessor_access = 0;

			//
			// Check base property accessors conflict
			//
			if ((ModFlags & (Modifiers.OVERRIDE | Modifiers.NEW)) == Modifiers.OVERRIDE) {
				if (get_accessor == null) {
					if (Get != null && !Get.IsDummy) {
						Report.SymbolRelatedToPreviousError (base_property);
						Report.Error (545, Location,
							"`{0}.get': cannot override because `{1}' does not have an overridable get accessor",
							GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
					}
				} else {
					get_accessor_access = get_accessor.Attributes & MethodAttributes.MemberAccessMask;

					if (!Get.IsDummy && !CheckAccessModifiers (
						ModifiersExtensions.MethodAttr (Get.ModFlags) & MethodAttributes.MemberAccessMask, get_accessor_access, get_accessor))
						Error_CannotChangeAccessModifiers (Get.Location, get_accessor, get_accessor_access, ".get");
				}

				if (set_accessor == null) {
					if (Set != null && !Set.IsDummy) {
						Report.SymbolRelatedToPreviousError (base_property);
						Report.Error (546, Location,
							"`{0}.set': cannot override because `{1}' does not have an overridable set accessor",
							GetSignatureForError (), TypeManager.GetFullNameSignature (base_property));
					}
				} else {
					set_accessor_access = set_accessor.Attributes & MethodAttributes.MemberAccessMask;

					if (!Set.IsDummy && !CheckAccessModifiers (
						ModifiersExtensions.MethodAttr (Set.ModFlags) & MethodAttributes.MemberAccessMask, set_accessor_access, set_accessor))
						Error_CannotChangeAccessModifiers (Set.Location, set_accessor, set_accessor_access, ".set");
				}
			}

			// When one accessor does not exist and property hides base one
			// we need to propagate this upwards
			if (set_accessor == null)
				set_accessor = get_accessor;

			//
			// Get the less restrictive access
			//
			return get_accessor_access > set_accessor_access ? get_accessor : set_accessor;
		}

		public override void Emit ()
		{
			//
			// The PropertyBuilder can be null for explicit implementations, in that
			// case, we do not actually emit the ".property", so there is nowhere to
			// put the attribute
			//
			if (PropertyBuilder != null) {
				if (OptAttributes != null)
					OptAttributes.Emit ();

				if (TypeManager.IsDynamicType (member_type)) {
					PredefinedAttributes.Get.Dynamic.EmitAttribute (PropertyBuilder);
				} else {
					var trans_flags = TypeManager.HasDynamicTypeUsed (member_type);
					if (trans_flags != null) {
						var pa = PredefinedAttributes.Get.DynamicTransform;
						if (pa.Constructor != null || pa.ResolveConstructor (Location, TypeManager.bool_type.MakeArrayType ())) {
							PropertyBuilder.SetCustomAttribute (
								new CustomAttributeBuilder (pa.Constructor, new object [] { trans_flags }));
						}
					}
				}
			}

			if (!Get.IsDummy)
				Get.Emit (Parent);

			if (!Set.IsDummy)
				Set.Emit (Parent);

			base.Emit ();
		}

		/// <summary>
		/// Tests whether accessors are not in collision with some method (CS0111)
		/// </summary>
		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Get.IsDuplicateImplementation (mc) || Set.IsDuplicateImplementation (mc);
		}

		public override bool IsUsed
		{
			get {
				if (IsExplicitImpl)
					return true;

				return Get.IsUsed | Set.IsUsed;
			}
		}

		protected override void SetMemberName (MemberName new_name)
		{
			base.SetMemberName (new_name);

			Get.UpdateName (this);
			Set.UpdateName (this);
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "P:"; }
		}
	}
			
	public class Property : PropertyBase
	{
		public sealed class BackingField : Field
		{
			readonly Property property;

			public BackingField (Property p)
				: base (p.Parent, p.type_name,
				Modifiers.BACKING_FIELD | Modifiers.COMPILER_GENERATED | Modifiers.PRIVATE | (p.ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
				new MemberName ("<" + p.GetFullName (p.MemberName) + ">k__BackingField", p.Location), null)
			{
				this.property = p;
			}

			public override string GetSignatureForError ()
			{
				return property.GetSignatureForError ();
			}
		}

		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.ABSTRACT |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.VIRTUAL;

		const Modifiers AllowedInterfaceModifiers =
			Modifiers.NEW;

		public Property (DeclSpace parent, FullNamedExpression type, Modifiers mod,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block, bool define_set_first)
			: this (parent, type, mod, name, attrs, get_block, set_block,
				define_set_first, null)
		{
		}
		
		public Property (DeclSpace parent, FullNamedExpression type, Modifiers mod,
				 MemberName name, Attributes attrs, Accessor get_block,
				 Accessor set_block, bool define_set_first, Block current_block)
			: base (parent, type, mod,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, define_set_first)
		{
			if (get_block == null)
				Get = new GetMethod (this);
			else
				Get = new GetMethod (this, get_block);

			if (set_block == null)
				Set = new SetMethod (this);
			else
				Set = new SetMethod (this, set_block);

			if (!IsInterface && (mod & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0 &&
				get_block != null && get_block.Block == null &&
				set_block != null && set_block.Block == null) {
				if (RootContext.Version <= LanguageVersion.ISO_2)
					Report.FeatureIsNotAvailable (Location, "automatically implemented properties");

				Get.ModFlags |= Modifiers.COMPILER_GENERATED;
				Set.ModFlags |= Modifiers.COMPILER_GENERATED;
			}
		}

		void CreateAutomaticProperty ()
		{
			// Create backing field
			Field field = new BackingField (this);
			if (!field.Define ())
				return;

			Parent.PartialContainer.AddField (field);

			FieldExpr fe = new FieldExpr (field, Location);
			if ((field.ModFlags & Modifiers.STATIC) == 0)
				fe.InstanceExpression = new CompilerGeneratedThis (fe.Type, Location);

			// Create get block
			Get.Block = new ToplevelBlock (Compiler, ParametersCompiled.EmptyReadOnlyParameters, Location);
			Return r = new Return (fe, Location);
			Get.Block.AddStatement (r);

			// Create set block
			Set.Block = new ToplevelBlock (Compiler, Set.ParameterInfo, Location);
			Assign a = new SimpleAssign (fe, new SimpleName ("value", Location));
			Set.Block.AddStatement (new StatementExpression (a));
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;

			if ((Get.ModFlags & Modifiers.COMPILER_GENERATED) != 0)
				CreateAutomaticProperty ();

			if (!DefineAccessors ())
				return false;

			if (!CheckBase ())
				return false;

			// FIXME - PropertyAttributes.HasDefault ?

			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				GetFullName (MemberName), PropertyAttributes.None, MemberType, null);

			if (!Get.IsDummy) {
				PropertyBuilder.SetGetMethod (GetBuilder);
				Parent.MemberCache.AddMember (GetBuilder, Get);
			}

			if (!Set.IsDummy) {
				PropertyBuilder.SetSetMethod (SetBuilder);
				Parent.MemberCache.AddMember (SetBuilder, Set);
			}
			
			TypeManager.RegisterProperty (PropertyBuilder, this);
			Parent.MemberCache.AddMember (PropertyBuilder, this);
			return true;
		}

		public override void Emit ()
		{
			if (((Set.ModFlags | Get.ModFlags) & (Modifiers.STATIC | Modifiers.COMPILER_GENERATED)) == Modifiers.COMPILER_GENERATED && Parent.PartialContainer.HasExplicitLayout) {
				Report.Error (842, Location,
					"Automatically implemented property `{0}' cannot be used inside a type with an explicit StructLayout attribute",
					GetSignatureForError ());
			}

			base.Emit ();
		}

		protected override PropertyInfo ResolveBaseProperty ()
		{
			return Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, ParametersCompiled.EmptyReadOnlyParameters, null, true) as PropertyInfo;
		}
	}

	/// </summary>
	///  Gigantic workaround  for lameness in SRE follows :
	///  This class derives from EventInfo and attempts to basically
	///  wrap around the EventBuilder so that FindMembers can quickly
	///  return this in it search for members
	/// </summary>
	public class MyEventBuilder : EventInfo {
		
		//
		// We use this to "point" to our Builder which is
		// not really a MemberInfo
		//
		EventBuilder MyBuilder;
		
		//
		// We "catch" and wrap these methods
		//
		MethodInfo raise, remove, add;

		EventAttributes attributes;
		Type declaring_type, reflected_type, event_type;
		string name;

		Event my_event;

		public MyEventBuilder (Event ev, TypeBuilder type_builder, string name, EventAttributes event_attr, Type event_type)
		{
			MyBuilder = type_builder.DefineEvent (name, event_attr, event_type);

			// And now store the values in our own fields.
			
			declaring_type = type_builder;

			reflected_type = type_builder;
			
			attributes = event_attr;
			this.name = name;
			my_event = ev;
			this.event_type = event_type;
		}
		
		//
		// Methods that you have to override.  Note that you only need 
		// to "implement" the variants that take the argument (those are
		// the "abstract" methods, the others (GetAddMethod()) are 
		// regular.
		//
		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			return add;
		}
		
		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			return remove;
		}
		
		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			return raise;
		}
		
		//
		// These methods make "MyEventInfo" look like a Builder
		//
		public void SetRaiseMethod (MethodBuilder raiseMethod)
		{
			raise = raiseMethod;
			MyBuilder.SetRaiseMethod (raiseMethod);
		}

		public void SetRemoveOnMethod (MethodBuilder removeMethod)
		{
			remove = removeMethod;
			MyBuilder.SetRemoveOnMethod (removeMethod);
		}

		public void SetAddOnMethod (MethodBuilder addMethod)
		{
			add = addMethod;
			MyBuilder.SetAddOnMethod (addMethod);
		}

		public void SetCustomAttribute (CustomAttributeBuilder cb)
		{
			MyBuilder.SetCustomAttribute (cb);
		}
		
		public override object [] GetCustomAttributes (bool inherit)
		{
			// FIXME : There's nothing which can be seemingly done here because
			// we have no way of getting at the custom attribute objects of the
			// EventBuilder !
			return null;
		}

		public override object [] GetCustomAttributes (Type t, bool inherit)
		{
			// FIXME : Same here !
			return null;
		}

		public override bool IsDefined (Type t, bool b)
		{
			return true;
		}

		public override EventAttributes Attributes {
			get {
				return attributes;
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override Type DeclaringType {
			get {
				return declaring_type;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type;
			}
		}

		public Type EventType {
			get {
				return event_type;
			}
		}
		
		public void SetUsed ()
		{
			if (my_event != null) {
//				my_event.SetAssigned ();
				my_event.SetIsUsed ();
			}
		}
	}
	
	/// <summary>
	/// For case when event is declared like property (with add and remove accessors).
	/// </summary>
	public class EventProperty: Event {
		abstract class AEventPropertyAccessor : AEventAccessor
		{
			protected AEventPropertyAccessor (EventProperty method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
			}

			public override MethodBuilder Define (DeclSpace ds)
			{
				CheckAbstractAndExtern (block != null);
				return base.Define (ds);
			}
			
			public override string GetSignatureForError ()
			{
				return method.GetSignatureForError () + "." + prefix.Substring (0, prefix.Length - 1);
			}
		}

		sealed class AddDelegateMethod: AEventPropertyAccessor
		{
			public AddDelegateMethod (EventProperty method, Accessor accessor):
				base (method, accessor, AddPrefix)
			{
			}
		}

		sealed class RemoveDelegateMethod: AEventPropertyAccessor
		{
			public RemoveDelegateMethod (EventProperty method, Accessor accessor):
				base (method, accessor, RemovePrefix)
			{
			}
		}


		static readonly string[] attribute_targets = new string [] { "event" }; // "property" target was disabled for 2.0 version

		public EventProperty (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags,
				      MemberName name,
				      Attributes attrs, Accessor add, Accessor remove)
			: base (parent, type, mod_flags, name, attrs)
		{
			Add = new AddDelegateMethod (this, add);
			Remove = new RemoveDelegateMethod (this, remove);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			SetIsUsed ();
			return true;
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	/// Event is declared like field.
	/// </summary>
	public class EventField : Event {
		abstract class EventFieldAccessor : AEventAccessor
		{
			protected EventFieldAccessor (EventField method, string prefix)
				: base (method, prefix)
			{
			}

			public override void Emit (DeclSpace parent)
			{
				if ((method.ModFlags & (Modifiers.ABSTRACT | Modifiers.EXTERN)) == 0) {
					if (parent is Class) {
						MethodBuilder mb = method_data.MethodBuilder;
						mb.SetImplementationFlags (mb.GetMethodImplementationFlags () | MethodImplAttributes.Synchronized);
					}

					var field_info = ((EventField) method).BackingField;
					FieldExpr f_expr = new FieldExpr (field_info, Location);
					if ((method.ModFlags & Modifiers.STATIC) == 0)
						f_expr.InstanceExpression = new CompilerGeneratedThis (field_info.Spec.FieldType, Location);

					block = new ToplevelBlock (Compiler, ParameterInfo, Location);
					block.AddStatement (new StatementExpression (
						new CompoundAssign (Operation,
							f_expr,
							block.GetParameterReference (ParameterInfo[0].Name, Location))));
				}

				base.Emit (parent);
			}

			protected abstract Binary.Operator Operation { get; }
		}

		sealed class AddDelegateMethod: EventFieldAccessor
		{
			public AddDelegateMethod (EventField method):
				base (method, AddPrefix)
			{
			}

			protected override Binary.Operator Operation {
				get { return Binary.Operator.Addition; }
			}
		}

		sealed class RemoveDelegateMethod: EventFieldAccessor
		{
			public RemoveDelegateMethod (EventField method):
				base (method, RemovePrefix)
			{
			}

			protected override Binary.Operator Operation {
				get { return Binary.Operator.Subtraction; }
			}
		}


		static readonly string[] attribute_targets = new string [] { "event", "field", "method" };
		static readonly string[] attribute_targets_interface = new string[] { "event", "method" };

		public Field BackingField;
		public Expression Initializer;

		public EventField (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base (parent, type, mod_flags, name, attrs)
		{
			Add = new AddDelegateMethod (this);
			Remove = new RemoveDelegateMethod (this);
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.Target == AttributeTargets.Field) {
				BackingField.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			if (a.Target == AttributeTargets.Method) {
				int errors = Report.Errors;
				Add.ApplyAttributeBuilder (a, cb, pa);
				if (errors == Report.Errors)
					Remove.ApplyAttributeBuilder (a, cb, pa);
				return;
			}

			base.ApplyAttributeBuilder (a, cb, pa);
		}

		public override bool Define()
		{
			if (!base.Define ())
				return false;

			if (Initializer != null && (ModFlags & Modifiers.ABSTRACT) != 0) {
				Report.Error (74, Location, "`{0}': abstract event cannot have an initializer",
					GetSignatureForError ());
			}

			if (!HasBackingField) {
				SetIsUsed ();
				return true;
			}

			// FIXME: We are unable to detect whether generic event is used because
			// we are using FieldExpr instead of EventExpr for event access in that
			// case.  When this issue will be fixed this hack can be removed.
			if (TypeManager.IsGenericType (MemberType) || Parent.IsGeneric)
				SetIsUsed ();

			if (Add.IsInterfaceImplementation)
				SetIsUsed ();

			TypeManager.RegisterEventField (EventBuilder, this);

			BackingField = new Field (Parent,
				new TypeExpression (MemberType, Location),
				Modifiers.BACKING_FIELD | Modifiers.COMPILER_GENERATED | Modifiers.PRIVATE | (ModFlags & (Modifiers.STATIC | Modifiers.UNSAFE)),
				MemberName, null);

			Parent.PartialContainer.AddField (BackingField);
			BackingField.Initializer = Initializer;
			BackingField.ModFlags &= ~Modifiers.COMPILER_GENERATED;

			// Call define because we passed fields definition
			return BackingField.Define ();
		}

		bool HasBackingField {
			get {
				return !IsInterface && (ModFlags & Modifiers.ABSTRACT) == 0;
			}
		}

		public override string[] ValidAttributeTargets 
		{
			get {
				return HasBackingField ? attribute_targets : attribute_targets_interface;
			}
		}
	}

	public abstract class Event : PropertyBasedMember {
		public abstract class AEventAccessor : AbstractPropertyEventMethod
		{
			protected readonly Event method;
			ImplicitParameter param_attr;

			static readonly string[] attribute_targets = new string [] { "method", "param", "return" };

			public const string AddPrefix = "add_";
			public const string RemovePrefix = "remove_";

			protected AEventAccessor (Event method, string prefix)
				: base (method, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags;
			}

			protected AEventAccessor (Event method, Accessor accessor, string prefix)
				: base (method, accessor, prefix)
			{
				this.method = method;
				this.ModFlags = method.ModFlags;
			}

			public bool IsInterfaceImplementation {
				get { return method_data.implementing != null; }
			}

			public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.IsInternalMethodImplAttribute) {
					method.is_external_implementation = true;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			protected override void ApplyToExtraTarget (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
			{
				if (a.Target == AttributeTargets.Parameter) {
					if (param_attr == null)
						param_attr = new ImplicitParameter (method_data.MethodBuilder);

					param_attr.ApplyAttributeBuilder (a, cb, pa);
					return;
				}

				base.ApplyAttributeBuilder (a, cb, pa);
			}

			public override AttributeTargets AttributeTargets {
				get {
					return AttributeTargets.Method;
				}
			}

			public override bool IsClsComplianceRequired ()
			{
				return method.IsClsComplianceRequired ();
			}

			public virtual MethodBuilder Define (DeclSpace parent)
			{
				method_data = new MethodData (method, method.ModFlags,
					method.flags | MethodAttributes.HideBySig | MethodAttributes.SpecialName, this);

				if (!method_data.Define (parent, method.GetFullName (MemberName), Report))
					return null;

				MethodBuilder mb = method_data.MethodBuilder;
				ParameterInfo.ApplyAttributes (mb);
				return mb;
			}

			public override Type ReturnType {
				get {
					return TypeManager.void_type;
				}
			}

			public override ObsoleteAttribute GetObsoleteAttribute ()
			{
				return method.GetObsoleteAttribute ();
			}

			public override string[] ValidAttributeTargets {
				get {
					return attribute_targets;
				}
			}

			public override ParametersCompiled ParameterInfo {
				get {
					return method.parameters;
				}
			}
		}


		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.STATIC |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.ABSTRACT |
			Modifiers.EXTERN;

		const Modifiers AllowedInterfaceModifiers =
			Modifiers.NEW;

		public AEventAccessor Add, Remove;
		public MyEventBuilder     EventBuilder;
		public MethodBuilder AddBuilder, RemoveBuilder;

		ParametersCompiled parameters;

		protected Event (DeclSpace parent, FullNamedExpression type, Modifiers mod_flags, MemberName name, Attributes attrs)
			: base (parent, null, type, mod_flags,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if ((a.HasSecurityAttribute)) {
				a.Error_InvalidSecurityParent ();
				return;
			}
			
			EventBuilder.SetCustomAttribute (cb);
		}

		public bool AreAccessorsDuplicateImplementation (MethodCore mc)
		{
			return Add.IsDuplicateImplementation (mc) || Remove.IsDuplicateImplementation (mc);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Event;
			}
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!TypeManager.IsDelegateType (MemberType)) {
				Report.Error (66, Location, "`{0}': event must be of a delegate type", GetSignatureForError ());
			}

			parameters = ParametersCompiled.CreateFullyResolved (
				new Parameter (null, "value", Parameter.Modifier.NONE, null, Location), MemberType);

			if (!CheckBase ())
				return false;

			if (TypeManager.delegate_combine_delegate_delegate == null) {
				TypeManager.delegate_combine_delegate_delegate = TypeManager.GetPredefinedMethod (
					TypeManager.delegate_type, "Combine", Location,
					TypeManager.delegate_type, TypeManager.delegate_type);
			}
			if (TypeManager.delegate_remove_delegate_delegate == null) {
				TypeManager.delegate_remove_delegate_delegate = TypeManager.GetPredefinedMethod (
					TypeManager.delegate_type, "Remove", Location,
					TypeManager.delegate_type, TypeManager.delegate_type);
			}

			//
			// Now define the accessors
			//

			AddBuilder = Add.Define (Parent);
			if (AddBuilder == null)
				return false;

			RemoveBuilder = Remove.Define (Parent);
			if (RemoveBuilder == null)
				return false;

			EventBuilder = new MyEventBuilder (this, Parent.TypeBuilder, Name, EventAttributes.None, MemberType);						
			EventBuilder.SetAddOnMethod (AddBuilder);
			EventBuilder.SetRemoveOnMethod (RemoveBuilder);

			Parent.MemberCache.AddMember (EventBuilder, this);
			Parent.MemberCache.AddMember (AddBuilder, Add);
			Parent.MemberCache.AddMember (RemoveBuilder, Remove);
			
			return true;
		}

		public override void Emit ()
		{
			if (OptAttributes != null) {
				OptAttributes.Emit ();
			}

			Add.Emit (Parent);
			Remove.Emit (Parent);

			base.Emit ();
		}

		protected override MethodInfo FindOutBaseMethod (ref Type base_ret_type)
		{
			MethodInfo mi = (MethodInfo) Parent.PartialContainer.BaseCache.FindBaseEvent (
				Parent.TypeBuilder, Name);

			if (mi == null)
				return null;

			AParametersCollection pd = TypeManager.GetParameterData (mi);
			base_ret_type = pd.Types [0];
			return mi;
		}

		//
		//   Represents header string for documentation comment.
		//
		public override string DocCommentHeader {
			get { return "E:"; }
		}
	}
 
	public class Indexer : PropertyBase
	{
		public class GetIndexerMethod : GetMethod
		{
			ParametersCompiled parameters;

			public GetIndexerMethod (Indexer method):
				base (method)
			{
				this.parameters = method.parameters;
			}

			public GetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
				parameters = accessor.Parameters;
			}

			public override MethodBuilder Define (DeclSpace parent)
			{
				parameters.Resolve (this);
				return base.Define (parent);
			}
			
			public override bool EnableOverloadChecks (MemberCore overload)
			{
				if (base.EnableOverloadChecks (overload)) {
					overload.caching_flags |= Flags.MethodOverloadsExist;
					return true;
				}

				return false;
			}			

			public override ParametersCompiled ParameterInfo {
				get {
					return parameters;
				}
			}
		}

		public class SetIndexerMethod: SetMethod
		{
			public SetIndexerMethod (Indexer method):
				base (method)
			{
				parameters = ParametersCompiled.MergeGenerated (Compiler, method.parameters, false, parameters [0], null);
			}

			public SetIndexerMethod (PropertyBase method, Accessor accessor):
				base (method, accessor)
			{
				parameters = method.Get.IsDummy ? accessor.Parameters : accessor.Parameters.Clone ();			
			}

			public override bool EnableOverloadChecks (MemberCore overload)
			{
				if (base.EnableOverloadChecks (overload)) {
					overload.caching_flags |= Flags.MethodOverloadsExist;
					return true;
				}

				return false;
			}
		}

		const Modifiers AllowedModifiers =
			Modifiers.NEW |
			Modifiers.PUBLIC |
			Modifiers.PROTECTED |
			Modifiers.INTERNAL |
			Modifiers.PRIVATE |
			Modifiers.VIRTUAL |
			Modifiers.SEALED |
			Modifiers.OVERRIDE |
			Modifiers.UNSAFE |
			Modifiers.EXTERN |
			Modifiers.ABSTRACT;

		const Modifiers AllowedInterfaceModifiers =
			Modifiers.NEW;

		public readonly ParametersCompiled parameters;

		public Indexer (DeclSpace parent, FullNamedExpression type, MemberName name, Modifiers mod,
				ParametersCompiled parameters, Attributes attrs,
				Accessor get_block, Accessor set_block, bool define_set_first)
			: base (parent, type, mod,
				parent.PartialContainer.Kind == Kind.Interface ? AllowedInterfaceModifiers : AllowedModifiers,
				name, attrs, define_set_first)
		{
			this.parameters = parameters;

			if (get_block == null)
				Get = new GetIndexerMethod (this);
			else
				Get = new GetIndexerMethod (this, get_block);

			if (set_block == null)
				Set = new SetIndexerMethod (this);
			else
				Set = new SetIndexerMethod (this, set_block);
		}

		protected override bool CheckForDuplications ()
		{
			return Parent.MemberCache.CheckExistingMembersOverloads (this, GetFullName (MemberName), parameters, Report);
		}
		
		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			if (!DefineParameters (parameters))
				return false;

			if (OptAttributes != null) {
				Attribute indexer_attr = OptAttributes.Search (PredefinedAttributes.Get.IndexerName);
				if (indexer_attr != null) {
					// Remove the attribute from the list because it is not emitted
					OptAttributes.Attrs.Remove (indexer_attr);

					string name = indexer_attr.GetIndexerAttributeValue ();
					if (name == null)
						return false;

					ShortName = name;

					if (IsExplicitImpl) {
						Report.Error (415, indexer_attr.Location,
							      "The `IndexerName' attribute is valid only on an " +
							      "indexer that is not an explicit interface member declaration");
						return false;
					}

					if ((ModFlags & Modifiers.OVERRIDE) != 0) {
						Report.Error (609, indexer_attr.Location,
							      "Cannot set the `IndexerName' attribute on an indexer marked override");
						return false;
					}
				}
			}

			if (InterfaceType != null) {
				string base_IndexerName = TypeManager.IndexerPropertyName (InterfaceType);
				if (base_IndexerName != Name)
					ShortName = base_IndexerName;
			}

			if (!Parent.PartialContainer.AddMember (this) ||
				!Parent.PartialContainer.AddMember (Get) || !Parent.PartialContainer.AddMember (Set))
				return false;

			flags |= MethodAttributes.HideBySig | MethodAttributes.SpecialName;
			
			if (!DefineAccessors ())
				return false;

			if (!CheckBase ())
				return false;

			//
			// Now name the parameters
			//
			PropertyBuilder = Parent.TypeBuilder.DefineProperty (
				GetFullName (MemberName), PropertyAttributes.None, MemberType, parameters.GetEmitTypes ());

			if (!Get.IsDummy) {
				PropertyBuilder.SetGetMethod (GetBuilder);
				Parent.MemberCache.AddMember (GetBuilder, Get);
			}

			if (!Set.IsDummy) {
				PropertyBuilder.SetSetMethod (SetBuilder);
				Parent.MemberCache.AddMember (SetBuilder, Set);
			}
				
			TypeManager.RegisterIndexer (PropertyBuilder, parameters);
			Parent.MemberCache.AddMember (PropertyBuilder, this);
			return true;
		}

		public override bool EnableOverloadChecks (MemberCore overload)
		{
			if (overload is Indexer) {
				caching_flags |= Flags.MethodOverloadsExist;
				return true;
			}

			return base.EnableOverloadChecks (overload);
		}

		public override string GetDocCommentName (DeclSpace ds)
		{
			return DocUtil.GetMethodDocCommentName (this, parameters, ds);
		}

		public override string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder (Parent.GetSignatureForError ());
			if (MemberName.Left != null) {
				sb.Append ('.');
				sb.Append (MemberName.Left.GetSignatureForError ());
			}

			sb.Append (".this");
			sb.Append (parameters.GetSignatureForError ().Replace ('(', '[').Replace (')', ']'));
			return sb.ToString ();
		}

		protected override PropertyInfo ResolveBaseProperty ()
		{
			return Parent.PartialContainer.BaseCache.FindMemberToOverride (
				Parent.TypeBuilder, Name, parameters, null, true) as PropertyInfo;
		}

		protected override bool VerifyClsCompliance ()
		{
			if (!base.VerifyClsCompliance ())
				return false;

			parameters.VerifyClsCompliance (this);
			return true;
		}
	}
}
