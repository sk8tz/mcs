//
// System.Configuration.Provider.ProviderBase
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;

namespace System.Configuration.Provider
{
	public abstract class ProviderBase
	{
		bool alreadyInitialized;
		
		protected ProviderBase ()
		{
		}
		
		public virtual void Initialize (string name, NameValueCollection config)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (name.Length == 0)
				throw new ArgumentException ("Provider name cannot be null or empty.", "name");
			if (alreadyInitialized)
				throw new InvalidOperationException ("This provider instance has already been initialized.");
			alreadyInitialized = true;
			
			_name = name;

			if (config != null) {
				_description = config ["description"];
				config.Remove ("description");
			}
			if (_description == null || _description.Length == 0)
				_description = _name;
		}
		
		public virtual string Name { 
			get { return _name; }
		}

		public virtual string Description {
			get { return _description; }
		}

		string _description;
		string _name;
	}
}

#endif
