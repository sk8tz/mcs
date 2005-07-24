//
// System.Web.Configuration.MachineKeyConfigHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

using System;
using System.Collections;
using System.Configuration;
using System.Xml;

namespace System.Web.Configuration
{
	class MachineKeyConfigHandler : IConfigurationSectionHandler
	{
		static byte ToHexValue (char c, bool high)
		{
			byte v;
			if (c >= '0' && c <= '9')
				v = (byte) (c - '0');
			else if (c >= 'a' && c <= 'f')
				v = (byte) (c - 'a' + 10);
			else if (c >= 'A' && c <= 'F')
				v = (byte) (c - 'A' + 10);
			else
				throw new ArgumentException ("Invalid hex character");

			if (high)
				v <<= 4;

			return v;
		}
		
		internal static byte [] GetBytes (string key, int len)
		{
			byte [] result = new byte [len / 2];
			for (int i = 0; i < len; i += 2)
				result [i / 2] = (byte) (ToHexValue (key [i], true) + ToHexValue (key [i + 1], false));

			return result;
		}

		public object Create (object parent, object context, XmlNode section)
		{
			if (section.HasChildNodes)
				ThrowException ("Child nodes not allowed here", section.FirstChild);

			MachineKeyConfig config = new MachineKeyConfig (parent);

			try {
				config.SetValidationKey (AttValue ("validationKey", section));
			} catch (ArgumentException e) {
				ThrowException (e.Message, section);
			}

			try {
				config.SetDecryptionKey (AttValue ("decryptionKey", section));
			} catch (ArgumentException e) {
				ThrowException (e.Message, section);
			}

			string validation = AttValue ("validation", section);
			if (validation != "SHA1" && validation != "MD5" && validation != "3DES")
				ThrowException ("Invalid 'validation' value", section);

			config.ValidationType = validation;

			if (section.Attributes != null && section.Attributes.Count != 0)
				ThrowException ("Unrecognized attribute", section);

			MachineKeyConfig.MachineKey = config;
			return config;
		}

		// A few methods to save some typing
		static string AttValue (string name, XmlNode node)
		{
			return HandlersUtil.ExtractAttributeValue (name, node, true);
		}

		static void ThrowException (string message, XmlNode node)
		{
			HandlersUtil.ThrowException (message, node);
		}
		//

	}
}

