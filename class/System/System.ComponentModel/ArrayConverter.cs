//
// System.ComponentModel.ArrayConverter
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

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

using System.Globalization;

namespace System.ComponentModel
{
	public class ArrayConverter : CollectionConverter
	{
		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value,
						  Type destinationType)
		{
			if (destinationType == null)
				throw new ArgumentNullException ("destinationType");

			if (destinationType == typeof (string) && (value is Array))
				return "(Array)";

			return base.ConvertTo (context, culture, value, destinationType);
		}

		public override PropertyDescriptorCollection GetProperties (ITypeDescriptorContext context,
									    object value, Attribute[] attributes)
		{
			// LAMESPEC MS seems to always parse an PropertyDescriptorCollection without any PropertyDescriptors
			return PropertyDescriptorCollection.Empty;
		}

		public override bool GetPropertiesSupported (ITypeDescriptorContext context)
		{
			return true;
		}
	}
}

