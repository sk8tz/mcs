//
// System.Security.Policy.StrongNameMembershipCondition.cs
//
// Author:
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003 Duncan Mak, Ximian Inc.
//

using System;
using System.Globalization;
using System.Security.Permissions;

namespace System.Security.Policy {

        public sealed class StrongNameMembershipCondition
                : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IConstantMembershipCondition
        {
                StrongNamePublicKeyBlob blob;
                string name;
                Version version;
                
                public StrongNameMembershipCondition (
                        StrongNamePublicKeyBlob blob, string name, Version version)
                {
                        if (blob == null)
                                throw new ArgumentNullException ("blob");

                        this.blob = blob;
                        this.name = name;
                        this.version = version;
                }

                public string Name {

                        get { return name; }

                        set { name = value; }
                }

                public Version Version {

                        get { return version; }

                        set { version = value; }
                }

                public StrongNamePublicKeyBlob PublicKey {

                        get { return blob; }

                        set {
                                if (value == null)
                                        throw new ArgumentNullException (
                                                Locale.GetText ("The argument is null."));

				blob = value;
			}
		}

		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;

			foreach (object o in evidence) {
				if (o is StrongName) {
					StrongName sn = (o as StrongName);
					if (sn.PublicKey.Equals (blob) && (sn.Name == name) && (sn.Version.Equals (version)))
						return true;
				}
			}
			return false;
		}

		public IMembershipCondition Copy ()
		{
			return new StrongNameMembershipCondition (blob, name, version);
		}

		public override bool Equals (object o)
		{	 
			if (o is StrongName == false)
				return false;
			else {
				StrongName sn = (StrongName) o;
				return (sn.Name == Name && sn.Version == Version && sn.PublicKey == PublicKey);
			}
		}

		public override int GetHashCode ()
		{
			return blob.GetHashCode ();
		}

		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			if (e == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));

			if (e.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is invalid."));

			if (e.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is invalid."));

			blob = StrongNamePublicKeyBlob.FromString (e.Attribute ("PublicKeyBlob"));
			name = e.Attribute ("Name");
			version = new Version (e.Attribute ("AssemblyVersion"));
		}

                public override string ToString ()
                {
                        return String.Format ( "Strong Name - {0} name = {1} version {2}",
                                        blob, name, version);
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
                        SecurityElement element = new SecurityElement ("IMembershipCondition");
                        element.AddAttribute ("class", this.GetType ().AssemblyQualifiedName);
                        element.AddAttribute ("version", "1");

                        element.AddAttribute ("PublicKeyBlob", blob.ToString ());
                        element.AddAttribute ("Name", name);
                        element.AddAttribute ("AssemblyVersion", version.ToString ());

                        return element;
                }
        }
}
