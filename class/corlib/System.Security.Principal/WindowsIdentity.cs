//
// System.Security.Principal.WindowsIdentity
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Security.Principal {

	[Serializable]
#if NET_1_0
	public class WindowsIdentity : IIdentity, IDeserializationCallback {
#else
	public class WindowsIdentity : IIdentity, IDeserializationCallback, ISerializable {
#endif
		private IntPtr _token;
		private string _type;
		private WindowsAccountType _account;
		private bool _authenticated;
		private string _name;
		private SerializationInfo _info;

		static private IntPtr invalidWindows = IntPtr.Zero;
		// that seems to be the value used for (at least) AIX and MacOSX
		static private IntPtr invalidPosix = (IntPtr) unchecked (-2);

		public WindowsIdentity (IntPtr userToken) 
			: this (userToken, null, WindowsAccountType.Normal, false)
		{
		}

		public WindowsIdentity (IntPtr userToken, string type) 
			: this (userToken, type, WindowsAccountType.Normal, false)
		{
		}

		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType)
			: this (userToken, type, acctType, false)
		{
		}

		public WindowsIdentity (IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated)
		{
			_type = type;
			_account = acctType;
			_authenticated = isAuthenticated;
			_name = null;
			// last - as it can override some fields
			SetToken (userToken);
		}
#if !NET_1_0
		public WindowsIdentity (string sUserPrincipalName) 
			: this (sUserPrincipalName, null)
		{
		}

		public WindowsIdentity (string sUserPrincipalName, string type)
		{
			if (sUserPrincipalName == null)
				throw new NullReferenceException ("sUserPrincipalName");

			// TODO: Windows 2003 compatibility should be done in runtime
			IntPtr token = GetUserToken (sUserPrincipalName);
			if ((!IsPosix) && (token == IntPtr.Zero)) {
				throw new ArgumentException ("only for Windows Server 2003 +");
			}

			_authenticated = true;
			_account = WindowsAccountType.Normal;
			_type = type;
			// last - as it can override some fields
			SetToken (token);
		}

		public WindowsIdentity (SerializationInfo info, StreamingContext context)
		{
			_info = info;
		}
#endif

		~WindowsIdentity ()
		{
			// clear our copy but don't close it
			// http://www.develop.com/kbrown/book/html/whatis_windowsprincipal.html
			_token = IntPtr.Zero;
		}

		// static methods

		public static WindowsIdentity GetAnonymous ()
		{
			WindowsIdentity id = null;
			if (IsPosix) {
				id = new WindowsIdentity ("nobody");
				// special case
				id._account = WindowsAccountType.Anonymous;
				id._authenticated = false;
				id._type = String.Empty;
			}
			else {
				id = new WindowsIdentity (IntPtr.Zero, String.Empty, WindowsAccountType.Anonymous, false);
				// special case (don't try to resolve the name)
				id._name = String.Empty;
			}
			return id;
		}

		public static WindowsIdentity GetCurrent ()
		{
			return new WindowsIdentity (GetCurrentToken (), null, WindowsAccountType.Normal, true);
		}

		// methods

		public virtual WindowsImpersonationContext Impersonate ()
		{
			return new WindowsImpersonationContext (_token);
		}

		public static WindowsImpersonationContext Impersonate (IntPtr userToken)
		{
			return new WindowsImpersonationContext (userToken);
		}

		// properties

		public virtual string AuthenticationType
		{
			get { return _type; }
		}

		public virtual bool IsAnonymous
		{
			get { return (_account == WindowsAccountType.Anonymous); }
		}

		public virtual bool IsAuthenticated
		{
			get { return _authenticated; }
		}

		public virtual bool IsGuest
		{
			get { return (_account == WindowsAccountType.Guest); }
		}

		public virtual bool IsSystem
		{
			get { return (_account == WindowsAccountType.System); }
		}

		public virtual string Name
		{
			get {
				if (_name == null) {
					// revolve name (runtime)
					_name = GetTokenName (_token);
				}
				return _name; 
			}
		}

		public virtual IntPtr Token
		{
			get { return _token; }
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
			_token = (IntPtr) _info.GetValue ("m_userToken", typeof (IntPtr));
			// can't trust this alone - we must validate the token
			_name = _info.GetString ("m_name");
			if (_name != null) {
				// validate token by comparing names
				string name = GetTokenName (_token);
				if (name != _name)
					throw new SerializationException ("Token-Name mismatch.");
			}
			else {
				// validate token by getting name
				_name = GetTokenName (_token);
				if ((_name == String.Empty) || (_name == null))
					throw new SerializationException ("Token doesn't match a user.");
			}
			_type = _info.GetString ("m_type");
			_account = (WindowsAccountType) _info.GetValue ("m_acctType", typeof (WindowsAccountType));
			_authenticated = _info.GetBoolean ("m_isAuthenticated");
		}
#if !NET_1_0
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) 
		{
			info.AddValue ("m_userToken", _token);
			// can be null when not resolved
			info.AddValue ("m_name", _name);
			info.AddValue ("m_type", _type);
			info.AddValue ("m_acctType", _account);
			info.AddValue ("m_isAuthenticated", _authenticated);
		}
#endif
		private static bool IsPosix {
			get { return ((int) Environment.Platform == 128); }
		}

		private void SetToken (IntPtr token) 
		{
			if (IsPosix) {
				if (token == invalidPosix)
					throw new ArgumentException ("Invalid token");

				_token = token;
				// apply defaults
				if (_type == null)
					_type = "POSIX";
				// override user choice in this specific case
				if (_token == IntPtr.Zero)
					_account = WindowsAccountType.System;
			}
			else {
				if ((token == invalidWindows) && (_account != WindowsAccountType.Anonymous))
					throw new ArgumentException ("Invalid token");

				_token = token;
				// apply defaults
				if (_type == null)
					_type = "NTLM";
			}
		}

		// see mono/mono/metadata/security.c for implementation

		// Many people use reflection to get a user's roles - so many 
		// that's it's hard to say it's an "undocumented" feature -
		// so we also implement it in Mono :-/
		// http://www.dotnet247.com/247reference/msgs/39/195403.aspx
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string[] _GetRoles (IntPtr token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static IntPtr GetCurrentToken ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string GetTokenName (IntPtr token);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static IntPtr GetUserToken (string username);
	}
}
