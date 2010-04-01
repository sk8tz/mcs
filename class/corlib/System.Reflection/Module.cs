//
// System.Reflection/Module.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;

namespace System.Reflection {

	internal enum ResolveTokenError {
		OutOfRange,
		BadTable,
		Other
	};

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_Module))]
	[Serializable]
	[ClassInterfaceAttribute (ClassInterfaceType.None)]

#if NET_4_0
	public class Module : ISerializable, ICustomAttributeProvider, _Module {
#else
	public partial class Module : ISerializable, ICustomAttributeProvider, _Module {
#endif
		public static readonly TypeFilter FilterTypeName;
		public static readonly TypeFilter FilterTypeNameIgnoreCase;
	
#pragma warning disable 649	
		internal IntPtr _impl; /* a pointer to a MonoImage */
		internal Assembly assembly;
		internal string fqname;
		internal string name;
		internal string scopename;
		internal bool is_resource;
		internal int token;
#pragma warning restore 649		
	
		const BindingFlags defaultBindingFlags = 
			BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
		
		static Module () {
			FilterTypeName = new TypeFilter (filter_by_type_name);
			FilterTypeNameIgnoreCase = new TypeFilter (filter_by_type_name_ignore_case);
		}


#if NET_4_0
		protected
#else
		internal
#endif
		Module () {
		}

		public Assembly Assembly {
			get { return assembly; }
		}
	
		public virtual string FullyQualifiedName {
			get {
#if !NET_2_1
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, fqname).Demand ();
				}
#endif
				return fqname;
			}
		}

		// Note: we do not ask for PathDiscovery because no path is returned here.
		// However MS Fx requires it (see FDBK23572 for details).
		public string Name {
			get { return name; }
		}
	
		public string ScopeName {
			get { return scopename; }
		}

		public ModuleHandle ModuleHandle {
			get {
				return new ModuleHandle (_impl);
			}
		}

		public extern int MetadataToken {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		public int MDStreamVersion {
			get {
				if (_impl == IntPtr.Zero)
					throw new NotSupportedException ();
				return GetMDStreamVersion (_impl);
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern int GetMDStreamVersion (IntPtr module_handle);

		public FieldInfo GetField (string name) 
		{
			return GetField (name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
		}

		public FieldInfo[] GetFields () 
		{
			return GetFields (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
		}
	
		public MethodInfo GetMethod (string name) 
		{
			//FIXME this sure breaks since Type.GetMethod throws due to a null 'type' array. But it's what MS does
			return GetMethodImpl (name, defaultBindingFlags, null, CallingConventions.Any, null, null);
		}
	
		public MethodInfo GetMethod (string name, Type[] types) 
		{
			return GetMethodImpl (name, defaultBindingFlags, null, CallingConventions.Any, types, null);
		}
	
		public MethodInfo GetMethod (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) 
		{
			return GetMethodImpl (name, bindingAttr, binder, callConvention, types, modifiers);
		}
	
		public MethodInfo[] GetMethods () 
		{
			if (IsResource ())
				return new MethodInfo [0];

			Type globalType = GetGlobalType ();
			return (globalType != null) ? globalType.GetMethods () : new MethodInfo [0];
		}

		public MethodInfo[] GetMethods (BindingFlags bindingFlags) {
			if (IsResource ())
				return new MethodInfo [0];

			Type globalType = GetGlobalType ();
			return (globalType != null) ? globalType.GetMethods (bindingFlags) : new MethodInfo [0];
		}
	
		[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			UnitySerializationHolder.GetModuleData (this, info, context);
		}

#if !NET_2_1
		public X509Certificate GetSignerCertificate ()
		{
			try {
				return X509Certificate.CreateFromSignedFile (assembly.Location);
			}
			catch {
				return null;
			}
		}
#endif

		[ComVisible (true)]
		public virtual Type GetType(string className) 
		{
			return GetType (className, false, false);
		}

		[ComVisible (true)]
		public virtual Type GetType(string className, bool ignoreCase) 
		{
			return GetType (className, false, ignoreCase);
		}
	
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern Type[] InternalGetTypes ();
	
		public virtual Type[] GetTypes() 
		{
			return InternalGetTypes ();
		}
	
		public override string ToString () 
		{
			return name;
		}

		internal Guid MvId {
			get {
				return GetModuleVersionId ();
			}
		}

		public Guid ModuleVersionId {
			get {
				return GetModuleVersionId ();
			}
		}

		internal Exception resolve_token_exception (int metadataToken, ResolveTokenError error, string tokenType) {
			if (error == ResolveTokenError.OutOfRange)
				return new ArgumentOutOfRangeException ("metadataToken", String.Format ("Token 0x{0:x} is not valid in the scope of module {1}", metadataToken, name));
			else
				return new ArgumentException (String.Format ("Token 0x{0:x} is not a valid {1} token in the scope of module {2}", metadataToken, tokenType, name), "metadataToken");
		}

		internal IntPtr[] ptrs_from_types (Type[] types) {
			if (types == null)
				return null;
			else {
				IntPtr[] res = new IntPtr [types.Length];
				for (int i = 0; i < types.Length; ++i) {
					if (types [i] == null)
						throw new ArgumentException ();
					res [i] = types [i].TypeHandle.Value;
				}
				return res;
			}
		}

		public FieldInfo ResolveField (int metadataToken) {
			return ResolveField (metadataToken, null, null);
		}

		public MemberInfo ResolveMember (int metadataToken) {
			return ResolveMember (metadataToken, null, null);
		}

		public MemberInfo ResolveMember (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {

			ResolveTokenError error;

			MemberInfo m = ResolveMemberToken (_impl, metadataToken, ptrs_from_types (genericTypeArguments), ptrs_from_types (genericMethodArguments), out error);
			if (m == null)
				throw resolve_token_exception (metadataToken, error, "MemberInfo");
			else
				return m;
		}

		public MethodBase ResolveMethod (int metadataToken) {
			return ResolveMethod (metadataToken, null, null);
		}

		public MethodBase ResolveMethod (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			ResolveTokenError error;

			IntPtr handle = ResolveMethodToken (_impl, metadataToken, ptrs_from_types (genericTypeArguments), ptrs_from_types (genericMethodArguments), out error);
			if (handle == IntPtr.Zero)
				throw resolve_token_exception (metadataToken, error, "MethodBase");
			else
				return MethodBase.GetMethodFromHandleNoGenericCheck (new RuntimeMethodHandle (handle));
		}

		public string ResolveString (int metadataToken) {
			ResolveTokenError error;

			string s = ResolveStringToken (_impl, metadataToken, out error);
			if (s == null)
				throw resolve_token_exception (metadataToken, error, "string");
			else
				return s;
		}

		public Type ResolveType (int metadataToken) {
			return ResolveType (metadataToken, null, null);
		}

		public Type ResolveType (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments) {
			ResolveTokenError error;

			IntPtr handle = ResolveTypeToken (_impl, metadataToken, ptrs_from_types (genericTypeArguments), ptrs_from_types (genericMethodArguments), out error);
			if (handle == IntPtr.Zero)
				throw resolve_token_exception (metadataToken, error, "Type");
			else
				return Type.GetTypeFromHandle (new RuntimeTypeHandle (handle));
		}

		public byte[] ResolveSignature (int metadataToken) {
			ResolveTokenError error;

		    byte[] res = ResolveSignature (_impl, metadataToken, out error);
			if (res == null)
				throw resolve_token_exception (metadataToken, error, "signature");
			else
				return res;
		}

		internal static Type MonoDebugger_ResolveType (Module module, int token)
		{
			ResolveTokenError error;

			IntPtr handle = ResolveTypeToken (module._impl, token, null, null, out error);
			if (handle == IntPtr.Zero)
				return null;
			else
				return Type.GetTypeFromHandle (new RuntimeTypeHandle (handle));
		}

		// Used by mcs, the symbol writer, and mdb through reflection
		internal static Guid Mono_GetGuid (Module module)
		{
			return module.GetModuleVersionId ();
		}

		internal virtual Guid GetModuleVersionId ()
		{
			return new Guid (GetGuidInternal ());
		}

		private static bool filter_by_type_name (Type m, object filterCriteria) {
			string s = (string)filterCriteria;
			if (s.EndsWith ("*"))
				return m.Name.StartsWith (s.Substring (0, s.Length - 1));
			else
				return m.Name == s;
		}

		private static bool filter_by_type_name_ignore_case (Type m, object filterCriteria) {
			string s = (string)filterCriteria;
			if (s.EndsWith ("*"))
				return m.Name.ToLower ().StartsWith (s.Substring (0, s.Length - 1).ToLower ());
			else
				return String.Compare (m.Name, s, true) == 0;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern IntPtr GetHINSTANCE ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern string GetGuidInternal ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern Type GetGlobalType ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern IntPtr ResolveTypeToken (IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern IntPtr ResolveMethodToken (IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern IntPtr ResolveFieldToken (IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern string ResolveStringToken (IntPtr module, int token, out ResolveTokenError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern MemberInfo ResolveMemberToken (IntPtr module, int token, IntPtr[] type_args, IntPtr[] method_args, out ResolveTokenError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern byte[] ResolveSignature (IntPtr module, int metadataToken, out ResolveTokenError error);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern void GetPEKind (IntPtr module, out PortableExecutableKinds peKind, out ImageFileMachine machine);

		void _Module.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _Module.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Module.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Module.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}

#if NET_4_0
		static Exception CreateNIE ()
		{
			return new NotImplementedException ("Derived classes must implement it");
		}

		public virtual bool IsResource()
		{
			throw CreateNIE ();
		}

		public virtual Type[] FindTypes(TypeFilter filter, object filterCriteria) 
		{
			throw CreateNIE ();
		}

		public virtual object[] GetCustomAttributes(bool inherit)
		{
			throw CreateNIE ();
		}

		public virtual object[] GetCustomAttributes(Type attributeType, bool inherit) 
		{
			throw CreateNIE ();
		}

		public virtual IList<CustomAttributeData> GetCustomAttributesData ()
		{
			throw CreateNIE ();
		}

		public virtual FieldInfo GetField (string name, BindingFlags bindingAttr) 
		{
			throw CreateNIE ();
		}

		public virtual FieldInfo[] GetFields (BindingFlags bindingFlags)
		{
			throw CreateNIE ();
		}

		protected virtual MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) 
		{
			throw CreateNIE ();
		}

		public virtual void GetPEKind (out PortableExecutableKinds peKind, out ImageFileMachine machine)
		{
			throw CreateNIE ();
		}

		[ComVisible (true)]
		public virtual Type GetType(string className, bool throwOnError, bool ignoreCase) 
		{
			throw CreateNIE ();
		}

		public virtual bool IsDefined (Type attributeType, bool inherit) 
		{
			throw CreateNIE ();
		}

		public virtual FieldInfo ResolveField (int metadataToken, Type [] genericTypeArguments, Type [] genericMethodArguments)
		{
			throw CreateNIE ();
		}

#endif

	}
}
