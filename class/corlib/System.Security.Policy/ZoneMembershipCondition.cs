//
// System.Security.Policy.ZoneMembershipCondition.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc.
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

namespace System.Security.Policy {

        public sealed class ZoneMembershipCondition
                : IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable, IConstantMembershipCondition
        {
                SecurityZone zone;
                
                public ZoneMembershipCondition (SecurityZone zone)
                {
                        this.zone = zone;
                }

                public SecurityZone SecurityZone {
                        set { zone = value; }
                        get { return zone; }
                }

                public bool Check (Evidence evidence)
                {
			if (evidence == null)
				return false;

			foreach (object o in evidence) {
				if (o is Zone) {
					Zone z = (o as Zone);
					if (z.SecurityZone == zone)
						return true;
				}
			}
                        return false;
                }

                public IMembershipCondition Copy ()
                {
                        return new ZoneMembershipCondition (zone);
                }

                public override bool Equals (Object o)
                {
                        if (o is ZoneMembershipCondition == false)
                                return false;
                        else
                                return ((ZoneMembershipCondition) o).SecurityZone == zone;
                }

                public void FromXml (SecurityElement element)
                {
                        FromXml (element, null);
                }

                public void FromXml (SecurityElement element, PolicyLevel level)
                {
			if (element == null)
				throw new ArgumentNullException (
                                        Locale.GetText ("The argument is null."));

                        if (element.Attribute ("class") != GetType ().AssemblyQualifiedName)
                                throw new ArgumentException (
                                        Locale.GetText ("The argument is invalid."));

                        if (element.Attribute ("version") != "1")
                                throw new ArgumentException (
                                        Locale.GetText ("The argument is invalid."));

                        zone = (SecurityZone) Enum.Parse (
                                typeof (SecurityZone), element.Attribute ("Zone"));
                }

                public override int GetHashCode ()
                {
                        return zone.GetHashCode ();
                }

                public override string ToString ()
                {
                        return "Zone - " + zone;
                }

                public SecurityElement ToXml ()
                {
                        return ToXml (null);
                }

                public SecurityElement ToXml (PolicyLevel level)
                {
                        SecurityElement element = new SecurityElement ("IMembershipCondition");
                        element.AddAttribute ("version", "1");

                        element.AddAttribute ("Zone", zone.ToString ());

                        return element;
                }
        }
}
