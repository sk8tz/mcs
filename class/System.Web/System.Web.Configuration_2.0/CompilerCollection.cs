//
// System.Web.Configuration.CompilerCollection
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Configuration;

namespace System.Web.Configuration
{
	[ConfigurationCollection (typeof (Compiler), AddItemName = "compiler", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public sealed class CompilerCollection : ConfigurationElementCollection
	{
		static ConfigurationPropertyCollection properties;

		static CompilerCollection ()
		{
			properties = new ConfigurationPropertyCollection ();
		}

		public CompilerCollection ()
		{
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new Compiler ();
		}

		public Compiler Get (int index)
		{
			return (Compiler) BaseGet (index);
		}

		public Compiler Get (string language)
		{
			return (Compiler) BaseGet (language);
		}

		protected override object GetElementKey (ConfigurationElement element)
		{
			return ((Compiler)element).Language;
		}

		public string GetKey (int index)
		{
			return (string)BaseGetKey (index);
		}

		public string[ ] AllKeys {
			get {
				string[] keys = new string[Count];
				for (int i = 0; i < Count; i ++)
					keys[i] = this[i].Language;
				return keys;
			}
		}

		public override ConfigurationElementCollectionType CollectionType {
			get { return ConfigurationElementCollectionType.BasicMap; }
		}

		protected override string ElementName {
			get { return "compiler"; }
		}

		protected override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		public Compiler this[int index] {
			get { return (Compiler) BaseGet (index); }
		}

		public new Compiler this[string language] {
			get {
				foreach (Compiler c in this) {
					if (c.Language.ToLower().Contains (language.ToLower()))
						return c;
				}
				return null;
			}
		}

#region CompatabilityCode
		[MonoTODO ("we shouldn't need this..")]
		internal bool CompareLanguages (string lang1, string lang2)
		{
			return (this [lang1] == this [lang2]);
		}

		internal void Add (Compiler compiler)
		{
			BaseAdd (compiler);
		}
#endregion
	}
}
#endif
