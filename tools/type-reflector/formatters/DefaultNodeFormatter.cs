//
// DefaultNodeFormatter.cs: Formats NodeInfo instances for display
//
// Author: Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002 Jonathan Pryor
//

using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.TypeReflector
{
	public class DefaultNodeFormatter : NodeFormatter {

		protected override string GetFieldDescription (FieldInfo field, object instance)
		{
			try {
				return string.Format ("{0}={1}", field.Name, 
						GetValue (field.GetValue(instance)));
			} catch {
				return field.Name;
			}
		}

		protected override string GetMethodDescription (MethodInfo mb, object instance)
		{
			if (mb.GetParameters().Length == 0) {
				try {
					object r = mb.Invoke (instance, null);
					string s = GetValue (r);
					return string.Format ("{0}()={1}", mb.Name, s);
				}
				catch {
				}
			}
			return mb.Name;
		}

		protected override string GetPropertyDescription (PropertyInfo property, object instance)
		{
			string v = "";
			try {
				// object o = property.GetGetMethod(true).Invoke(instance, null);
				object o = property.GetValue (instance, null);
				v = string.Format ("={0}", GetValue (o));
			} catch {
			}
			return string.Format ("{0}{1}", property.Name, v);
		}
	}
}

