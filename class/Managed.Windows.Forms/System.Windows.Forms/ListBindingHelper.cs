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
// Copyright (c) 2007 Novell, Inc.
//
// Author:
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

#if NET_2_0

using System.Collections.Generic;

namespace System.Windows.Forms
{
	public static class ListBindingHelper 
	{
		public static object GetList (object list)
		{
			return GetList (list, String.Empty);
		}

		public static object GetList (object dataSource, string dataMember)
		{
			if (dataSource is IListSource)
				dataSource = ((IListSource) dataSource).GetList ();

			if (dataSource == null)
				return null;

			if (dataMember == null || dataMember.Length == 0)
				return dataSource;

			if (dataSource is IEnumerable) {
				IEnumerator e = ((IEnumerable) dataSource).GetEnumerator ();
				if (e == null || !e.MoveNext () || e.Current == null) {
					PropertyDescriptorCollection properties = GetListItemProperties (dataSource);
					if (properties [dataMember] == null)
						throw new ArgumentException ("dataMember");

					// Weird
					return null;
				}
					
				dataSource = e.Current;
			}

			PropertyDescriptor property = GetProperty (dataSource, dataMember);
			if (property == null)
				throw new ArgumentException ("dataMember");

			return property.GetValue (dataSource);
		}

		public static Type GetListItemType (object list)
		{
			return GetListItemType (list, String.Empty);
		}

		public static Type GetListItemType (object dataSource, string dataMember)
		{
			if (dataSource == null)
				return null;

			if (dataMember != null && dataMember.Length > 0) {
				PropertyDescriptor property = GetProperty (dataSource, dataMember);
				if (property == null)
					return typeof (object);

				return property.PropertyType;
			}

			if (dataSource is Array)
				return dataSource.GetType ().GetElementType ();

			// IEnumerable seems to have higher precedence over IList
			if (dataSource is IEnumerable) {
				IEnumerator enumerator = ((IEnumerable) dataSource).GetEnumerator ();
				if (enumerator.MoveNext () && enumerator.Current != null)
					return enumerator.Current.GetType ();

				if (dataSource is IList || dataSource.GetType () == typeof (IList<>)) {
					PropertyInfo property = GetPropertyByReflection (dataSource.GetType (), "Item");
					return property.PropertyType;
				}

				// fallback to object
				return typeof (object);
			}

			return dataSource.GetType ();
		}

		public static PropertyDescriptorCollection GetListItemProperties (object list)
		{
			return GetListItemProperties (list, null);
		}

		public static PropertyDescriptorCollection GetListItemProperties (object list, PropertyDescriptor [] listAccessors)
		{
			list = GetList (list);

			if (list == null)
				return new PropertyDescriptorCollection (null);

			if (list is ITypedList)
				return ((ITypedList)list).GetItemProperties (listAccessors);

			if (listAccessors == null || listAccessors.Length == 0) {
				Type item_type = GetListItemType (list);
				return TypeDescriptor.GetProperties (item_type, 
					new Attribute [] { new BrowsableAttribute (true) });
			}

			// Take into account only the first property
			Type property_type = listAccessors [0].PropertyType;
			if (typeof (IList).IsAssignableFrom (property_type) || 
				typeof (IList<>).IsAssignableFrom (property_type)) {

				PropertyInfo property = GetPropertyByReflection (property_type, "Item");
				return TypeDescriptor.GetProperties (property.PropertyType);
			}

			return new PropertyDescriptorCollection (new PropertyDescriptor [0]);
		}

		public static PropertyDescriptorCollection GetListItemProperties (object dataSource, string dataMember, 
			PropertyDescriptor [] listAccessors)
		{
			throw new NotImplementedException ();
		}

		public static string GetListName (object list, PropertyDescriptor [] listAccessors)
		{
			if (list == null)
				return String.Empty;

			Type item_type = GetListItemType (list);
			return item_type.Name;
		}

		static PropertyDescriptor GetProperty (object obj, string property_name)
		{
			Attribute [] attrs = new Attribute [] { new BrowsableAttribute (true) };

			PropertyDescriptorCollection properties;
			if (obj is ICustomTypeDescriptor)
				properties = ((ICustomTypeDescriptor)obj).GetProperties (attrs);
			else
				properties = TypeDescriptor.GetProperties (obj.GetType (), attrs);

			return properties [property_name];
		}

		// 
		// Need to use reflection as we need to bypass the TypeDescriptor.GetProperties () limitations
		//
		static PropertyInfo GetPropertyByReflection (Type type, string property_name)
		{
			foreach (PropertyInfo prop in type.GetProperties (BindingFlags.Public | BindingFlags.Instance))
				if (prop.Name == property_name)
					return prop;

			return null;
		}
	}
}

#endif
