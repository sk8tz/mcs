//
// System.Security.PermissionSet.cs
//
// Authors:
//	Nick Drochak(ndrochak@gol.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Nick Drochak
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
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

using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;

namespace System.Security {

	[Serializable]
	public class PermissionSet: ISecurityEncodable, ICollection, IEnumerable, IStackWalk, IDeserializationCallback {

		private static string tagName = "PermissionSet";
		private const int version = 1;
		private static object[] psNone = new object [1] { PermissionState.None };

		private PermissionState state;
		private ArrayList list;
		private int _hashcode;
		private PolicyLevel _policyLevel;
		private bool _declsec;

		// constructors

		// for PolicyLevel (to avoid validation duplication)
		internal PermissionSet () 
		{
			list = new ArrayList ();
		}

		public PermissionSet (PermissionState state) : this ()
		{
			if (!Enum.IsDefined (typeof (PermissionState), state))
				throw new System.ArgumentException ("state");
			this.state = state;
		}

		public PermissionSet (PermissionSet permSet) : this ()
		{
			// LAMESPEC: This would be handled by the compiler.  No way permSet is not a PermissionSet.
			//if (!(permSet is PermissionSet))
			//	throw new System.ArgumentException(); // permSet is not an instance of System.Security.PermissionSet.
			if (permSet == null)
				state = PermissionState.Unrestricted;
			else {
				state = permSet.state;
				foreach (IPermission p in permSet.list)
					list.Add (p);
			}
		}

		internal PermissionSet (string xml)
			: this ()
		{
			state = PermissionState.None;
			if (xml != null) {
				SecurityElement se = SecurityElement.FromString (xml);
				FromXml (se);
			}
		}

		// Light version for creating a (non unrestricted) PermissionSet with
		// a single permission. This allows to relax most validations.
		internal PermissionSet (IPermission perm)
			: this ()
		{
			if (perm != null) {
				// note: we do not copy IPermission like AddPermission
				list.Add (perm);
			}
		}

		// methods

		public virtual IPermission AddPermission (IPermission perm)
		{
			if (perm == null)
				return null;

			// we don't add to an unrestricted permission set unless...
			if (state == PermissionState.Unrestricted) {
				// we're adding identity permission as they don't support unrestricted
				if (perm is IUnrestrictedPermission) {
					// we return the union of the permission with unrestricted
					// which results in a permission of the same type initialized 
					// with PermissionState.Unrestricted
					object[] args = new object [1] { PermissionState.Unrestricted };
					return (IPermission) Activator.CreateInstance (perm.GetType (), args);
				}
			}

			// we can't add two permissions of the same type in a set
			// so we remove an existing one, union with it and add it back
			IPermission existing = RemovePermission (perm.GetType ());
			if (existing != null) {
				perm = perm.Union (existing);
			}

			// note: Add doesn't copy
			list.Add (perm);
			return perm;
		}

		[MonoTODO ("Imperative mode isn't supported")]
		public virtual void Assert ()
		{
			new SecurityPermission (SecurityPermissionFlag.Assertion).Demand ();

			int count = this.Count;

			// we (current frame) must have the permission to assert it to others
			// otherwise we don't assert (but we don't throw an exception)
			foreach (IPermission p in list) {
				// note: we ignore non-CAS permissions
				if (p is IStackWalk) {
					if (!SecurityManager.IsGranted (p)) {
						return;
					}
				} else
					count--;
			}

			// note: we must ignore the stack modifiers for the non-CAS permissions
			if (count > 0)
				throw new NotSupportedException ("Currently only declarative Assert are supported.");
		}

		internal void Clear () 
		{
			list.Clear ();
		}

		public virtual PermissionSet Copy ()
		{
			return new PermissionSet (this);
		}

		public virtual void CopyTo (Array array, int index)
		{
			if (null == array)
				throw new ArgumentNullException ("array");

			if (list.Count > 0) {
				if (array.Rank > 1) {
					throw new ArgumentException (Locale.GetText (
						"Array has more than one dimension"));
				}
				if (index < 0 || index >= array.Length) {
					throw new IndexOutOfRangeException ("index");
				}

				list.CopyTo (array, index);
			}
		}

		[MonoTODO ("Imperative Assert, Deny and PermitOnly aren't yet supported")]
		public virtual void Demand ()
		{
			// Note: SecurityEnabled only applies to CAS permissions
			// so we're not checking for it (yet)
			if (IsEmpty ())
				return;

			PermissionSet cas = this;
			// avoid copy (if possible)
			if (ContainsNonCodeAccessPermissions ()) {
				// non CAS permissions (e.g. PrincipalPermission) do not requires a stack walk
				cas = this.Copy ();
				foreach (IPermission p in list) {
					Type t = p.GetType ();
					if (!t.IsSubclassOf (typeof (CodeAccessPermission))) {
						p.Demand ();
						// we wont have to process this one in the stack walk
						cas.RemovePermission (t);
					}
				}
			}

			// don't start the stack walk if
			// - the permission set only contains non CAS permissions; or
			// - security isn't enabled (applis only to CAS!)
			if (!cas.IsEmpty () && SecurityManager.SecurityEnabled)
				CasOnlyDemand (_declsec ? 4 : 2);
		}

		// The number of frames to skip depends on who's calling
		// - CodeAccessPermission.Demand (imperative)
		// - PermissionSet.Demand (imperative)
		// - SecurityManager.InternalDemand (declarative)
		internal void CasOnlyDemand (int skip)
		{
			Assembly current = null;

			// skip ourself, Demand and other security runtime methods
			foreach (SecurityFrame sf in SecurityFrame.GetStack (skip)) {
				if (ProcessFrame (sf, ref current))
					return; // reached Assert
			}

			// Is there a CompressedStack to handle ?
			CompressedStack stack = Thread.CurrentThread.GetCompressedStack ();
			if ((stack != null) && !stack.IsEmpty ()) {
				foreach (SecurityFrame frame in stack.List) {
					if (ProcessFrame (frame, ref current))
						return; // reached Assert
				}
			}
		}

		[MonoTODO ("Imperative mode isn't supported")]
		public virtual void Deny ()
		{
			foreach (IPermission p in list) {
				// note: we ignore non-CAS permissions
				if (p is IStackWalk) {
					throw new NotSupportedException ("Currently only declarative Deny are supported.");
				}
			}
		}

		[MonoTODO ("adjust class version with current runtime - unification")]
		public virtual void FromXml (SecurityElement et)
		{
			if (et == null)
				throw new ArgumentNullException ("et");
			if (et.Tag != tagName) {
				string msg = String.Format ("Invalid tag {0} expected {1}", et.Tag, tagName);
				throw new ArgumentException (msg, "et");
			}

			if (CodeAccessPermission.IsUnrestricted (et))
				state = PermissionState.Unrestricted;
			else
				state = PermissionState.None;

			list.Clear ();
			if (et.Children != null) {
				foreach (SecurityElement se in et.Children) {
					string className = se.Attribute ("class");
					if (className == null) {
						throw new ArgumentException (Locale.GetText (
							"No permission class is specified."));
					}
					if (Resolver != null) {
						// policy class names do not have to be fully qualified
						className = Resolver.ResolveClassName (className);
					}
					// TODO: adjust class version with current runtime (unification)
					// http://blogs.msdn.com/shawnfa/archive/2004/08/05/209320.aspx
					Type classType = Type.GetType (className);
					if (classType != null) {
						IPermission p = (IPermission) Activator.CreateInstance (classType, psNone);
						p.FromXml (se);
						list.Add (p);
					}
#if !NET_2_0
					else {
						string msg = Locale.GetText ("Can't create an instance of permission class {0}.");
						throw new ArgumentException (String.Format (msg, se.Attribute ("class")));
					}
#endif
				}
			}
		}

		public virtual IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public virtual bool IsSubsetOf (PermissionSet target)
		{
			// if target is empty we must be empty too
			if ((target == null) || (target.IsEmpty ()))
				return this.IsEmpty ();

			// TODO - non CAS permissions must be evaluated for unrestricted

			// if target is unrestricted then we are a subset
			if (!this.IsUnrestricted () && target.IsUnrestricted ())
				return true;
			// else target isn't unrestricted.
			// so if we are unrestricted, the we can't be a subset
			if (this.IsUnrestricted () && !target.IsUnrestricted ())
				return false;

			// if each of our permission is (a) present and (b) a subset of target
			foreach (IPermission p in list) {
				// for every type in both list
				IPermission i = target.GetPermission (p.GetType ());
				if (i == null)
					return false; // not present (condition a)
				if (!p.IsSubsetOf (i))
					return false; // not a subset (condition b)
			}
			return true;
		}

		[MonoTODO ("Imperative mode isn't supported")]
		public virtual void PermitOnly ()
		{
			foreach (IPermission p in list) {
				// note: we ignore non-CAS permissions
				if (p is IStackWalk) {
					throw new NotSupportedException ("Currently only declarative Deny are supported.");
				}
			}
		}

		public bool ContainsNonCodeAccessPermissions () 
		{
			foreach (IPermission p in list) {
				if (! p.GetType ().IsSubclassOf (typeof (CodeAccessPermission)))
					return true;
			}
			return false;
		}

		[MonoTODO ("little documentation in Fx 2.0 beta 1")]
		public static byte[] ConvertPermissionSet (string inFormat, byte[] inData, string outFormat) 
		{
			if (inFormat == null)
				throw new ArgumentNullException ("inFormat");
			if (outFormat == null)
				throw new ArgumentNullException ("outFormat");
			if (inData == null)
				return null;

			if (inFormat == outFormat)
				return inData;

			PermissionSet ps = null;

			if (inFormat == "BINARY") {
				if (outFormat.StartsWith ("XML")) {
					using (MemoryStream ms = new MemoryStream (inData)) {
						BinaryFormatter formatter = new BinaryFormatter ();
						ps = (PermissionSet) formatter.Deserialize (ms);
						ms.Close ();
					}
					string xml = ps.ToString ();
					switch (outFormat) {
						case "XML":
						case "XMLASCII":
							return Encoding.ASCII.GetBytes (xml);
						case "XMLUNICODE":
							return Encoding.Unicode.GetBytes (xml);
					}
				}
			}
			else if (inFormat.StartsWith ("XML")) {
				if (outFormat == "BINARY") {
					string xml = null;
					switch (inFormat) {
						case "XML":
						case "XMLASCII":
							xml = Encoding.ASCII.GetString (inData);
							break;
						case "XMLUNICODE":
							xml = Encoding.Unicode.GetString (inData);
							break;
					}
					if (xml != null) {
						ps = new PermissionSet (PermissionState.None);
						ps.FromXml (SecurityElement.FromString (xml));

						MemoryStream ms = new MemoryStream ();
						BinaryFormatter formatter = new BinaryFormatter ();
						formatter.Serialize (ms, ps);
						ms.Close ();
						return ms.ToArray ();
					}
				}
				else if (outFormat.StartsWith ("XML")) {
					string msg = String.Format (Locale.GetText ("Can't convert from {0} to {1}"), inFormat, outFormat);
#if NET_2_0
					throw new XmlSyntaxException (msg);
#else
					throw new ArgumentException (msg);
#endif
				}
			}
			else {
				// unknown inFormat, returns null
				return null;
			}
			// unknown outFormat, throw
			throw new SerializationException (String.Format (Locale.GetText ("Unknown output format {0}."), outFormat));
		}

		public virtual IPermission GetPermission (Type permClass) 
		{
			foreach (object o in list) {
				if (o.GetType ().Equals (permClass))
					return (IPermission) o;
			}
			// it's normal to return null for unrestricted sets
			return null;
		}

		public virtual PermissionSet Intersect (PermissionSet other) 
		{
			// no intersection possible
			if ((other == null) || (other.IsEmpty ()) || (this.IsEmpty ()))
				return null;

			PermissionState state = PermissionState.None;
			if (this.IsUnrestricted () && other.IsUnrestricted ())
				state = PermissionState.Unrestricted;

			PermissionSet interSet = new PermissionSet (state);
			if (state == PermissionState.Unrestricted) {
				InternalIntersect (interSet, this, other, true);
				InternalIntersect (interSet, other, this, true);
			}
			else if (this.IsUnrestricted ()) {
				InternalIntersect (interSet, this, other, true);
			}
			else if (other.IsUnrestricted ()) {
				InternalIntersect (interSet, other, this, true);
			}
			else {
				InternalIntersect (interSet, this, other, false);
			}
			return interSet;
		}

		internal void InternalIntersect (PermissionSet intersect, PermissionSet a, PermissionSet b, bool unrestricted)
		{
			foreach (IPermission p in b.list) {
				// for every type in both list
				IPermission i = a.GetPermission (p.GetType ());
				if (i != null) {
					// add intersection for this type
					intersect.AddPermission (p.Intersect (i));
				}
				else if (unrestricted && (p is IUnrestrictedPermission)) {
					intersect.AddPermission (p);
				}
				// or reject!
			}
		}

		public virtual bool IsEmpty () 
		{
			// note: Unrestricted isn't empty
			if (state == PermissionState.Unrestricted)
				return false;
			if ((list == null) || (list.Count == 0))
				return true;
			// the set may include some empty permissions
			foreach (IPermission p in list) {
				// empty == fully restricted == IsSubsetOg(null) == true
				if (!p.IsSubsetOf (null))
					return false;
			}
			return true;
		}

		public virtual bool IsUnrestricted () 
		{
			return (state == PermissionState.Unrestricted);
		}

		public virtual IPermission RemovePermission (Type permClass) 
		{
			if (permClass == null)
				return null;

			foreach (object o in list) {
				if (o.GetType ().Equals (permClass)) {
					list.Remove (o);
					return (IPermission) o;
				}
			}
			return null;
		}

		public virtual IPermission SetPermission (IPermission perm) 
		{
			if (perm == null)
				return null;
			if (perm is IUnrestrictedPermission)
				state = PermissionState.None;
			RemovePermission (perm.GetType ());
			list.Add (perm);
			return perm;
		}

		public override string ToString ()
		{
			return ToXml ().ToString ();
		}

		public virtual SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement (tagName);
			se.AddAttribute ("class", GetType ().FullName);
			se.AddAttribute ("version", version.ToString ());
			if (state == PermissionState.Unrestricted)
				se.AddAttribute ("Unrestricted", "true");

			// required for permissions that do not implement IUnrestrictedPermission
			foreach (IPermission p in list) {
				se.AddChild (p.ToXml ());
			}
			return se;
		}

		public virtual PermissionSet Union (PermissionSet other)
		{
			if (other == null)
				return this.Copy ();

			PermissionSet copy = this.Copy ();
			if (this.IsUnrestricted () || other.IsUnrestricted ()) {
				// so we keep the "right" type
				copy.Clear ();
				copy.state = PermissionState.Unrestricted;
				// copy all permissions that do not implement IUnrestrictedPermission
				foreach (IPermission p in this.list) {
					if (!(p is IUnrestrictedPermission))
						copy.AddPermission (p);
				}
				foreach (IPermission p in other.list) {
					if (!(p is IUnrestrictedPermission))
						copy.AddPermission (p);
				}
			}
			else {
				// PermissionState.None -> copy all permissions
				foreach (IPermission p in other.list) {
					copy.AddPermission (p);
				}
			}
			return copy;
		}

		public virtual int Count {
			get { return list.Count; }
		}

		public virtual bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public virtual bool IsReadOnly {
			get { return false; } // always false
		}

		public virtual object SyncRoot {
			get { return this; }
		}

		internal bool DeclarativeSecurity {
			get { return _declsec; }
			set { _declsec = value; }
		}

		[MonoTODO()]
		void IDeserializationCallback.OnDeserialization (object sender) 
		{
		}

#if NET_2_0
		[ComVisible (false)]
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			PermissionSet ps = (obj as PermissionSet);
			if (ps == null)
				return false;
			if (list.Count != ps.Count)
				return false;

			for (int i=0; i < list.Count; i++) {
				bool found = false;
				for (int j=0; i < ps.list.Count; j++) {
					if (list [i].Equals (ps.list [j])) {
						found = true;
						break;
					}
				}
				if (!found)
					return false;
			}
			return true;
		}

		[ComVisible (false)]
		public override int GetHashCode ()
		{
			return (list.Count == 0) ? (int) state : base.GetHashCode ();
		}

		[MonoTODO ("what's it doing here?")]
		static public void RevertAssert ()
		{
			// FIXME: There's probably a reason this was added here ?
			CodeAccessPermission.RevertAssert ();
		}
#endif

		// internal

		internal PolicyLevel Resolver {
			get { return _policyLevel; }
			set { _policyLevel = value; }
		}

		internal bool ProcessFrame (SecurityFrame frame, ref Assembly current)
		{
			if (IsUnrestricted ()) {
				// we request unrestricted
				if (frame.Deny != null) {
					// but have restrictions (some denied permissions)
					CodeAccessPermission.ThrowSecurityException (this, "Deny", frame.Assembly, 
						frame.Method, SecurityAction.Demand, null);
				} else if (frame.PermitOnly != null) {
					// but have restrictions (onyl some permitted permissions)
					CodeAccessPermission.ThrowSecurityException (this, "PermitOnly", frame.Assembly,
						frame.Method, SecurityAction.Demand, null);
				}
			}

			foreach (CodeAccessPermission cap in list) {
				if (cap.ProcessFrame (frame, ref current))
					return true; // Assert reached - abort stack walk!
			}
			return false;
		}

		// 2.0 metadata format

		internal static PermissionSet CreateFromBinaryFormat (byte[] data)
		{
			if ((data == null) || (data [0] != 0x2E) || (data.Length < 2)) {
				string msg = Locale.GetText ("Invalid data in 2.0 metadata format.");
				throw new SecurityException (msg);
			}

			int pos = 1;
			int numattr = ReadEncodedInt (data, ref pos);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			for (int i = 0; i < numattr; i++) {
				IPermission p = ProcessAttribute (data, ref pos);
				if (p == null) {
					string msg = Locale.GetText ("Unsupported data found in 2.0 metadata format.");
					throw new SecurityException (msg);
				}
				ps.AddPermission (p);
			}
			return ps;
		}

		internal static int ReadEncodedInt (byte[] data, ref int position)
		{
			int len = 0;
			if ((data [position] & 0x80) == 0) {
				len = data [position];
				position ++;
			} else if ((data [position] & 0x40) == 0) {
				len = ((data [position] & 0x3f) << 8 | data [position + 1]);
				position += 2;
			} else {
				len = (((data [position] & 0x1f) << 24) | (data [position + 1] << 16) |
					(data [position + 2] << 8) | (data [position + 3]));
				position += 4;
			}
			return len;
		}

		static object[] action = new object [1] { (SecurityAction) 0 };

		// TODO: add support for arrays and enums
		internal static IPermission ProcessAttribute (byte[] data, ref int position)
		{
			int clen = ReadEncodedInt (data, ref position);
			string cnam = Encoding.UTF8.GetString (data, position, clen);
			position += clen;

			// TODO: Unification
			Type secattr = Type.GetType (cnam);
			SecurityAttribute sa = (Activator.CreateInstance (secattr, action) as SecurityAttribute);
			if (sa == null)
				return null;

			/*int optionalParametersLength =*/ ReadEncodedInt (data, ref position);
			int numberOfParameters = ReadEncodedInt (data, ref position);
			for (int j=0; j < numberOfParameters; j++) {
				bool property = false;
				switch (data [position++]) {
				case 0x53: // field (technically possible and working)
					property = false;
					break;
				case 0x54: // property (common case)
					property = true;
					break;
				default:
					return null;
				}

				bool array = false;
				byte type = data [position++];
				if (type == 0x1D) {
					array = true;
					type = data [position++];
				}

				int plen = ReadEncodedInt (data, ref position);
				string pnam = Encoding.UTF8.GetString (data, position, plen);
				position += plen;

				int arrayLength = 1;
				if (array) {
					arrayLength = BitConverter.ToInt32 (data, position);
					position += 4;
				}

				object obj = null;
				object[] arrayIndex = null;
				for (int i = 0; i < arrayLength; i++) {
					if (array) {
						// TODO - setup index
					}

					// sadly type values doesn't match ther TypeCode enum :(
					switch (type) {
					case 0x02: // MONO_TYPE_BOOLEAN
						obj = (object) Convert.ToBoolean (data [position++]);
						break;
					case 0x03: // MONO_TYPE_CHAR
						obj = (object) Convert.ToChar (data [position]);
						position += 2;
						break;
					case 0x04: // MONO_TYPE_I1
						obj = (object) Convert.ToSByte (data [position++]);
						break;
					case 0x05: // MONO_TYPE_U1
						obj = (object) Convert.ToByte (data [position++]);
						break;
					case 0x06: // MONO_TYPE_I2
						obj = (object) Convert.ToInt16 (data [position]);
						position += 2;
						break;
					case 0x07: // MONO_TYPE_U2
						obj = (object) Convert.ToUInt16 (data [position]);
						position += 2;
						break;
					case 0x08: // MONO_TYPE_I4
						obj = (object) Convert.ToInt32 (data [position]);
						position += 4;
						break;
					case 0x09: // MONO_TYPE_U4
						obj = (object) Convert.ToUInt32 (data [position]);
						position += 4;
						break;
					case 0x0A: // MONO_TYPE_I8
						obj = (object) Convert.ToInt64 (data [position]);
						position += 8;
						break;
					case 0x0B: // MONO_TYPE_U8
						obj = (object) Convert.ToUInt64 (data [position]);
						position += 8;
						break;
					case 0x0C: // MONO_TYPE_R4
						obj = (object) Convert.ToSingle (data [position]);
						position += 4;
						break;
					case 0x0D: // MONO_TYPE_R8
						obj = (object) Convert.ToDouble (data [position]);
						position += 8;
						break;
					case 0x0E: // MONO_TYPE_STRING
						string s = null;
						if (data [position] != 0xFF) {
							int slen = ReadEncodedInt (data, ref position);
							s = Encoding.UTF8.GetString (data, position, slen);
							position += slen;
						} else {
							position++;
						}
						obj = (object) s;
						break;
					case 0x50: // special for TYPE
						int tlen = ReadEncodedInt (data, ref position);
						obj = (object) Type.GetType (Encoding.UTF8.GetString (data, position, tlen));
						position += tlen;
						break;
					default:
						return null; // unsupported
					}

					if (property) {
						PropertyInfo pi = secattr.GetProperty (pnam);
						pi.SetValue (sa, obj, arrayIndex);
					} else {
						FieldInfo fi = secattr.GetField (pnam);
						fi.SetValue (sa, obj);
					}
				}
			}
			return sa.CreatePermission ();
		}
	}
}
