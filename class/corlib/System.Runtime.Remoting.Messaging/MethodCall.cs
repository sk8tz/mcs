//
// System.Runtime.Remoting.Messaging.MethodCall.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//         Lluis Sanchez Gual (lluis@ideary.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.Messaging {

	[Serializable] [CLSCompliant (false)]
	public class MethodCall : IMethodCallMessage, IMethodMessage, IMessage, ISerializable, IInternalMessage
	{
		string _uri;
		string _typeName;
		string _methodName;
		object[] _args;
		Type[] _methodSignature;
		MethodBase _methodBase;
		LogicalCallContext _callContext;
		ArgInfo _inArgInfo;
		Identity _targetIdentity;

		protected IDictionary ExternalProperties;
		protected IDictionary InternalProperties;

		public MethodCall (Header [] headers)
		{
			Init();

			if (headers == null || headers.Length == 0) return;

			foreach (Header header in headers)
				InitMethodProperty (header.Name, header.Value);

			ResolveMethod ();
		}

		internal MethodCall (SerializationInfo info, StreamingContext context)
		{
			Init();

			foreach (SerializationEntry entry in info)
				InitMethodProperty ((string)entry.Name, entry.Value);

			ResolveMethod ();
		}

		internal MethodCall (CADMethodCallMessage msg) 
		{
			_typeName = msg.TypeName;
			_uri = msg.Uri;
			_methodName = msg.MethodName;
			
			// Get unmarshalled arguments
			ArrayList args = msg.GetArguments ();

			_args = msg.GetArgs (args);
			_methodSignature = (Type []) msg.GetMethodSignature (args);
	
			ResolveMethod ();
			Init();

			if (msg.PropertiesCount > 0)
				CADMessageBase.UnmarshalProperties (Properties, msg.PropertiesCount, args);
		}

		public MethodCall (IMessage msg)
		{
			if (msg is IMethodMessage)
				CopyFrom ((IMethodMessage) msg);
			else
			{
				IDictionary dic = msg.Properties;
				foreach (DictionaryEntry entry in msg.Properties)
					InitMethodProperty ((String) entry.Key, entry.Value);
				Init();
    		}
		}

		internal MethodCall (string uri, string typeName, string methodName, object[] args)
		{
			_uri = uri;
			_typeName = typeName;
			_methodName = methodName;
			_args = args;

			Init();
			ResolveMethod();
		}

		internal MethodCall ()
		{
		}
		
		internal void CopyFrom (IMethodMessage call)
		{
			_uri = call.Uri;
			_typeName = call.TypeName;
			_methodName = call.MethodName;
			_args = call.Args;
			_methodSignature = (Type[]) call.MethodSignature;
			_methodBase = call.MethodBase;
			_callContext = call.LogicalCallContext;

			Init();
		}
		
		internal virtual void InitMethodProperty(string key, object value)
		{
			switch (key)
			{
				case "__TypeName" : _typeName = (string) value; return;
				case "__MethodName" : _methodName = (string) value; return;
				case "__MethodSignature" : _methodSignature = (Type[]) value; return;
				case "__Args" : _args = (object[]) value; return;
				case "__CallContext" : _callContext = (LogicalCallContext) value; return;
				case "__Uri" : _uri = (string) value; return;
				default: Properties[key] = value; return;
			}
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("__TypeName", _typeName);
			info.AddValue ("__MethodName", _methodName);
			info.AddValue ("__MethodSignature", _methodSignature);
			info.AddValue ("__Args", _args);
			info.AddValue ("__CallContext", _callContext);
			info.AddValue ("__Uri", _uri);

			if (InternalProperties != null) {
				foreach (DictionaryEntry entry in InternalProperties)
					info.AddValue ((string) entry.Key, entry.Value);
			}
		} 

		public int ArgCount {
			get { return _args.Length; }
		}

		public object[] Args {
			get { return _args; }
		}
		
		[MonoTODO]
		public bool HasVarArgs {
			get { throw new NotImplementedException (); }
		}

		public int InArgCount 
		{
			get 
			{ 
				if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
				return _inArgInfo.GetInOutArgCount();
			}
		}

		public object[] InArgs 
		{
			get 
			{ 
				if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
				return _inArgInfo.GetInOutArgs (_args);
			}
		}
		
		public LogicalCallContext LogicalCallContext {
			get { return _callContext; }
		}
		
		public MethodBase MethodBase {
			get {
				if (_methodBase == null)
					ResolveMethod ();
					
				return _methodBase;
			}
		}

		public string MethodName {
			get { return _methodName; }
		}

		public object MethodSignature {
			get { 
				if (_methodSignature == null && _methodBase != null)
				{
					ParameterInfo[] parameters = _methodBase.GetParameters();
					_methodSignature = new Type[parameters.Length];
					for (int n=0; n<parameters.Length; n++)
						_methodSignature[n] = parameters[n].ParameterType;
				}
				return _methodSignature;
			}
		}

		public virtual IDictionary Properties {
			get 
			{ 
				if (ExternalProperties == null) InitDictionary ();
				return ExternalProperties; 
			}
		}

		internal virtual void InitDictionary()
		{
			MethodCallDictionary props = new MethodCallDictionary (this);
			ExternalProperties = props;
			InternalProperties = props.GetInternalProperties();
		}

		public string TypeName 
		{
			get { return _typeName; }
		}

		public string Uri {
			get { return _uri; }
			set { _uri = value; }
		}

		public object GetArg (int argNum)
		{
			return _args[argNum];
		}

		public string GetArgName (int index)
		{
			return _methodBase.GetParameters()[index].Name;
		}

		public object GetInArg (int argNum)
		{
			if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
			return _args[_inArgInfo.GetInOutArgIndex (argNum)];
		}

		public string GetInArgName (int index)
		{
			if (_inArgInfo == null) _inArgInfo = new ArgInfo (_methodBase, ArgInfoType.In);
			return _inArgInfo.GetInOutArgName(index);
		}

		[MonoTODO]
		public virtual object HeaderHandler (Header[] h)
		{
			throw new NotImplementedException ();
		}

		public virtual void Init ()
		{
		}

		public void ResolveMethod ()
		{
			if (_uri != null)
			{
				Type type = RemotingServices.GetServerTypeForUri (_uri);

				int i = _typeName.IndexOf(",");
				string clientTypeName = (i != -1) ? _typeName.Substring (0,i).Trim() : _typeName;

				if (clientTypeName == type.FullName)
				{
					BindingFlags bflags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
					if (_methodSignature == null) _methodBase = type.GetMethod (_methodName, bflags);
					else _methodBase = type.GetMethod (_methodName, bflags, null, _methodSignature, null);
					return;
				}
			}
			_methodBase = RemotingServices.GetMethodBaseFromMethodMessage (this);
		}

		[MonoTODO]
		public void RootSetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		Identity IInternalMessage.TargetIdentity
		{
			get { return _targetIdentity; }
			set { _targetIdentity = value; }
		}
	}
}
