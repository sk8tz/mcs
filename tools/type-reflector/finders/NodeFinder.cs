//
// NodeFinder.cs: Finds sub-nodes for a given NodeInfo object.
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Diagnostics;
using System.Reflection;

namespace Mono.TypeReflector
{
	public delegate void BaseTypeEventHandler (object sender, BaseTypeEventArgs e);
	public delegate void TypeEventHandler (object sender, TypeEventArgs e);
	public delegate void InterfacesEventHandler (object sender, InterfacesEventArgs e);
	public delegate void FieldsEventHandler (object sender, FieldsEventArgs e);
	public delegate void PropertiesEventHandler (object sender, PropertiesEventArgs e);
	public delegate void EventsEventHandler (object sender, EventsEventArgs e);
	public delegate void ConstructorsEventHandler (object sender, ConstructorsEventArgs e);
	public delegate void MethodsEventHandler (object sender, MethodsEventArgs e);

	public class NodeFoundEventArgs : EventArgs {
		private NodeInfo _node;

		internal NodeFoundEventArgs (NodeInfo node)
		{
			_node = node;
		}

		public NodeInfo NodeInfo {
			get {return _node;}
		}
	}

	public class BaseTypeEventArgs : NodeFoundEventArgs {

		private Type _base;

		internal BaseTypeEventArgs (NodeInfo node, Type type)
			: base(node)
		{
			_base = type;
		}

		public Type BaseType {
			get {return _base;}
		}
	}

	public class TypeEventArgs : NodeFoundEventArgs {

		private Type _type;

		internal TypeEventArgs (NodeInfo node, Type type)
			: base(node)
		{
			_type = type;
		}

		public Type Type {
			get {return _type;}
		}
	}

	public class InterfacesEventArgs : NodeFoundEventArgs {

		private Type[] _interfaces;

		internal InterfacesEventArgs (NodeInfo node, Type[] interfaces)
			: base(node)
		{
			_interfaces = interfaces ;
		}

		public Type[] Interfaces {
			get {return _interfaces;}
		}
	}

	public class FieldsEventArgs : NodeFoundEventArgs {
		private FieldInfo[] _fields;

		internal FieldsEventArgs (NodeInfo node, FieldInfo[] fields)
			: base(node)
		{
			_fields = fields;
		}

		public FieldInfo[] Fields {
			get {return _fields;}
		}
	}

	public class PropertiesEventArgs : NodeFoundEventArgs {

		private PropertyInfo[] _props;

		internal PropertiesEventArgs (NodeInfo node, PropertyInfo[] properties)
			: base(node)
		{
			_props = properties;
		}

		public PropertyInfo[] Properties {
			get {return _props;}
		}
	}

	public class EventsEventArgs : NodeFoundEventArgs {

		private EventInfo[] _events;

		internal EventsEventArgs (NodeInfo node, EventInfo[] events)
			: base(node)
		{
			_events = events;
		}

		public EventInfo[] Events {
			get {return _events;}
		}
	}

	public class ConstructorsEventArgs : NodeFoundEventArgs {

		private ConstructorInfo[] _ctors;

		internal ConstructorsEventArgs (NodeInfo node, ConstructorInfo[] ctors)
			: base(node)
		{
			_ctors = ctors;
		}

		public ConstructorInfo[] Constructors {
			get {return _ctors;}
		}
	}

	public class MethodsEventArgs : NodeFoundEventArgs {

		private MethodInfo[] _methods;

		internal MethodsEventArgs (NodeInfo node, MethodInfo[] methods)
			: base(node)
		{
			_methods = methods;
		}

		public MethodInfo[] Methods {
			get {return _methods;}
		}
	}

	public abstract class NodeFinder : INodeFinder {

		private static BooleanSwitch info = 
			new BooleanSwitch ("node-finder", "NodeFinder messages");

		private bool showBase = false;
		private bool showConstructors = false;
		private bool showEvents = false;
		private bool showFields = false;
		private bool showInterfaces = false;
		private bool showMethods = false;
		private bool showProperties = false;
		private bool showTypeProperties = false;
		private bool verboseOutput = false;
		private bool showMonoBroken = false;

		// `ReflectionTypeDisplayer.PrintTypeProperties' is recursive, but refrains 
    // from printing duplicates.  Despite duplicate removal, the output for 
    // printing the Properties of System.Type is > 800K of text.
		//
		// 3 levels permits viewing Attribute values, but not the attributes of
		// those attribute values.
		//
		// For example, 3 levels permits:
		// 		class		System.Type                           {depth 0}
		// 			Properties:                                 {depth 1}
		// 				System.Reflection.MemberTypes MemberType  {depth 2}
		// 					- CanRead=True                          {depth 3}
		// 					- CanWrite=False                        {depth 3}
		// 					...
		private int maxDepth = 3;

		public int MaxDepth {
			get {return maxDepth;}
			set {maxDepth = value;}
		}

		public bool ShowBase {
			get {return showBase;}
			set {showBase = value;}
		}

		public bool ShowConstructors {
			get {return showConstructors;}
			set {showConstructors = value;}
		}

		public bool ShowEvents {
			get {return showEvents;}
			set {showEvents = value;}
		}

		public bool ShowFields {
			get {return showFields;}
			set {showFields = value;}
		}

		public bool ShowInterfaces {
			get {return showInterfaces;}
			set {showInterfaces = value;}
		}

		public bool ShowMethods {
			get {return showMethods;}
			set {showMethods = value;}
		}

		public bool ShowProperties {
			get {return showProperties;}
			set {showProperties = value;}
		}

		public bool ShowTypeProperties {
			get {return showTypeProperties;}
			set {showTypeProperties = value;}
		}

		public bool ShowMonoBroken {
			get {return showMonoBroken;}
			set {showMonoBroken = value;}
		}

		public bool VerboseOutput {
			get {return verboseOutput;}
			set {verboseOutput = value;}
		}

		private BindingFlags bindingFlags = 
			BindingFlags.DeclaredOnly |
			BindingFlags.Public |
			BindingFlags.Instance |
			BindingFlags.Static;

		protected BindingFlags BindingFlags {
			get {return bindingFlags;}
		}

		public bool FlattenHierarchy {
			get {return (bindingFlags & BindingFlags.FlattenHierarchy) != 0;}
			set {
				if (value)
					bindingFlags |= BindingFlags.FlattenHierarchy;
				else
					bindingFlags &= ~BindingFlags.FlattenHierarchy;
			}
		}

		public bool ShowInheritedMembers {
			get {return (bindingFlags & BindingFlags.DeclaredOnly) != 0;}
			set {
				if (value)
					bindingFlags |= BindingFlags.FlattenHierarchy;
				else
					bindingFlags &= ~BindingFlags.FlattenHierarchy;
			}
		}

		public bool ShowNonPublic {
			get {return (bindingFlags & BindingFlags.NonPublic) != 0;}
			set {
				if (value)
					bindingFlags |= BindingFlags.NonPublic;
				else
					bindingFlags &= ~BindingFlags.NonPublic;
			}
		}

		public virtual NodeInfoCollection GetChildren (NodeInfo root)
		{
			Trace.WriteLineIf (info.Enabled, "NodeFinder.GetChildren");
			NodeInfoCollection c = new NodeInfoCollection ();

			// always handle NodeTypes.Type
			if (root.NodeType == NodeTypes.Type)
				GetTypeChildren (c, root, (Type) root.ReflectionObject);
			else if (VerboseOutput) {
				switch (root.NodeType) {
					case NodeTypes.BaseType:
						GetBaseTypeChildren (c, root, (Type) root.ReflectionObject);
						break;
					case NodeTypes.Interface:
						GetInterfaceChildren (c, root, (Type) root.ReflectionObject);
						break;
					case NodeTypes.Field:
						GetFieldChildren (c, root, (FieldInfo) root.ReflectionObject);
						break;
					case NodeTypes.Constructor:
						GetConstructorChildren (c, root, (ConstructorInfo) root.ReflectionObject);
						break;
					case NodeTypes.Method:
						GetMethodChildren (c, root, (MethodInfo) root.ReflectionObject);
						break;
					case NodeTypes.Parameter:
						GetParameterChildren (c, root, (ParameterInfo) root.ReflectionObject);
						break;
					case NodeTypes.Property:
						GetPropertyChildren (c, root, (PropertyInfo) root.ReflectionObject);
						break;
					case NodeTypes.Event:
						GetEventChildren (c, root, (EventInfo) root.ReflectionObject);
						break;
					case NodeTypes.ReturnValue:
						GetReturnValueChildren (c, root);
						break;
					case NodeTypes.Other:
					case NodeTypes.Alias:
						GetOtherChildren (c, root);
						break;
					default:
						GetUnhandledChildren (c, root);
						break;
				}
			}
			return c;
		}

		protected virtual void GetTypeChildren (NodeInfoCollection c, NodeInfo root, Type type)
		{
		}

		protected virtual void GetBaseTypeChildren (NodeInfoCollection c, NodeInfo root, Type baseType)
		{
		}

		protected virtual void GetInterfaceChildren (NodeInfoCollection c, NodeInfo root, Type iface)
		{
		}

		protected virtual void GetFieldChildren (NodeInfoCollection c, NodeInfo root, FieldInfo field)
		{
			Console.WriteLine ("** field children b");
		}

		protected virtual void GetConstructorChildren (NodeInfoCollection c, NodeInfo root, ConstructorInfo ctor)
		{
		}

		protected virtual void GetMethodChildren (NodeInfoCollection c, NodeInfo root, MethodInfo method)
		{
		}

		protected virtual void GetParameterChildren (NodeInfoCollection c, NodeInfo root, ParameterInfo param)
		{
		}

		protected virtual void GetPropertyChildren (NodeInfoCollection c, NodeInfo root, PropertyInfo property)
		{
		}

		protected virtual void GetEventChildren (NodeInfoCollection c, NodeInfo root, EventInfo e)
		{
		}

		protected virtual void GetReturnValueChildren (NodeInfoCollection c, NodeInfo root)
		{
			if (root.ReflectionObject != null)
				GetTypeChildren (c, root, (Type) root.ReflectionObject);
		}

		protected virtual void GetOtherChildren (NodeInfoCollection c, NodeInfo root)
		{
			if (root.Description is NodeGroup) {
				NodeGroup g = (NodeGroup) root.Description;
				g.Invoke (c, root);
			}
		}

		protected virtual void GetUnhandledChildren (NodeInfoCollection c, NodeInfo root)
		{
			c.Add (new NodeInfo (root, "Unhandled child: NodeType=" + root.NodeType));
		}

		public event TypeEventHandler         Types;
		public event BaseTypeEventHandler     BaseType;
		public event InterfacesEventHandler   Interfaces;
		public event FieldsEventHandler       Fields;
		public event PropertiesEventHandler   Properties;
		public event EventsEventHandler       Events;
		public event ConstructorsEventHandler Constructors;
		public event MethodsEventHandler      Methods;
	}
}

