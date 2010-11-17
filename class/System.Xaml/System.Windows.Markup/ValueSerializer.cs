//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xaml;

namespace System.Windows.Markup
{
	[System.Runtime.CompilerServices.TypeForwardedFrom (Consts.AssemblyWindowsBase)]
	public abstract class ValueSerializer
	{
		public static ValueSerializer GetSerializerFor (PropertyDescriptor descriptor)
		{
			return GetSerializerFor (descriptor, null);
		}

		public static ValueSerializer GetSerializerFor (Type type)
		{
			return GetSerializerFor (type, null);
		}

		// untested
		public static ValueSerializer GetSerializerFor (PropertyDescriptor descriptor, IValueSerializerContext context)
		{
			if (descriptor == null)
				throw new ArgumentNullException ("descriptor");
			if (context != null)
				return context.GetValueSerializerFor (descriptor);

			var tc = descriptor.Converter;
			if (tc != null && tc.GetType () != typeof (TypeConverter))
				return new TypeConverterValueSerializer (tc);
			return null;
		}

		public static ValueSerializer GetSerializerFor (Type type, IValueSerializerContext context)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (context != null)
				return context.GetValueSerializerFor (type);

			// Standard MarkupExtensions are serialized without ValueSerializer.
			if (typeof (MarkupExtension).IsAssignableFrom (type) && XamlLanguage.AllTypes.Any (x => x.UnderlyingType == type))
				return null;

			// DateTime is documented as special.
			if (type == typeof (DateTime))
				return new DateTimeValueSerializer ();
			// String too.
			if (type == typeof (string))
				return new StringValueSerializer ();

			// FIXME: this is hack. The complete condition is fully documented at http://msdn.microsoft.com/en-us/library/ms590363.aspx
			if (type.GetCustomAttribute<TypeConverterAttribute> (true) != null) {
				var tc = TypeDescriptor.GetConverter (type);
				if (tc != null && tc.GetType () != typeof (TypeConverter))
					return new TypeConverterValueSerializer (tc);
			}

			// Undocumented, but System.Type seems also special. While other MarkupExtension returned types are not handled specially, this method returns a valid instance for System.Type. Note that it doesn't for TypeExtension.
			if (type == typeof (Type))
				// Since System.Type does not have a valid TypeConverter, I use TypeExtensionConverter (may sound funny considering the above notes!) for this serializer.
				return new TypeConverterValueSerializer (new TypeExtensionConverter ());

			// Undocumented, but several primitive types get a valid serializer while it does not have TypeConverter.
			switch (Type.GetTypeCode (type)) {
			case TypeCode.Object:
			case TypeCode.DBNull:
				break;
			default:
				return new TypeConverterValueSerializer (TypeDescriptor.GetConverter (type));
			}

			// There is still exceptional type! TimeSpan. Why aren't they documented?
			if (type == typeof (TimeSpan))
				return new TypeConverterValueSerializer (TypeDescriptor.GetConverter (type));

			return null;
		}

		// instance members

		public virtual bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return false;
		}

		public virtual bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return false;
		}

		public virtual object ConvertFromString (string value, IValueSerializerContext context)
		{
			throw GetConvertFromException (value);
		}

		public virtual string ConvertToString (object value,     IValueSerializerContext context)
		{
			throw GetConvertToException (value, typeof (string));
		}

		protected Exception GetConvertFromException (object value)
		{
			return new NotSupportedException (String.Format ("Conversion from string '{0}' is not supported", value));
		}

		protected Exception GetConvertToException (object value, Type destinationType)
		{
			return new NotSupportedException (String.Format ("Conversion from '{0}' to {1} is not supported", value != null ? value.GetType ().Name : "(null)", destinationType));
		}

		public virtual IEnumerable<Type> TypeReferences (object value, IValueSerializerContext context)
		{
			yield break;
		}
	}

	internal class StringValueSerializer : ValueSerializer
	{
		public override bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return true;
		}

		public override bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return true;
		}

		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}

		public override string ConvertToString (object value,     IValueSerializerContext context)
		{
			return (string) value;
		}

		public override IEnumerable<Type> TypeReferences (object value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
	}

	#region Internal implementations.

	internal class TypeConverterValueSerializer : ValueSerializer
	{
		public TypeConverterValueSerializer (TypeConverter typeConverter)
		{
			c = typeConverter;
		}

		TypeConverter c;

		public override bool CanConvertFromString (string value, IValueSerializerContext context)
		{
			return c.CanConvertFrom (context, typeof (string));
		}

		public override bool CanConvertToString (object value, IValueSerializerContext context)
		{
			return c.CanConvertTo (context, typeof (string));
		}

		public override object ConvertFromString (string value, IValueSerializerContext context)
		{
			return c.ConvertFromInvariantString (context, value);
		}

		public override string ConvertToString (object value,     IValueSerializerContext context)
		{
			return value == null ? String.Empty : c.ConvertToInvariantString (context, value);
		}

		public override IEnumerable<Type> TypeReferences (object value, IValueSerializerContext context)
		{
			throw new NotImplementedException ();
		}
	}
	
	#endregion
}
