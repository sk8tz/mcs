//
// System.Configuration.ProtectedConfigurationSection.cs
//
// Authors:
// 	Duncan Mak (duncan@ximian.com)
//	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

namespace System.Configuration
{
	public sealed class ProtectedConfigurationSection: ConfigurationSection
	{
		[ConfigurationProperty ("defaultProvider", DefaultValue="RsaProtectedConfigurationProvider")]
		public string DefaultProvider {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get {
				throw new NotImplementedException ();
			}
		}

		[ConfigurationProperty ("providers")] 
		public ProviderSettingsCollection Providers {
			get {
				throw new NotImplementedException ();
			}
		}

	}
}
#endif
