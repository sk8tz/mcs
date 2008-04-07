//
// System.Web.VirtualPath.cs
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
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
using System.Web.Util;

namespace System.Web
{
	internal class VirtualPath : IDisposable
	{
		string _absolute;
		string _appRelative;
		string _appRelativeNotRooted;
		string _extension;
		string _directory;
		
		public bool IsAbsolute {
			get;
			private set;
		}

		public bool IsRooted {
			get;
			private set;
		}

		public bool IsAppRelative {
			get;
			private set;
		}
		
		public string Original {
			get;
			private set;
		}

		public string Absolute {
			get {
				if (IsAbsolute)
					return Original;

				if (_absolute == null) {
					string original = Original;
					
					if (!VirtualPathUtility.IsRooted (original))
						_absolute = MakeRooted (original);
					else
						_absolute = original;

					if (VirtualPathUtility.IsAppRelative (_absolute))
						_absolute = VirtualPathUtility.ToAbsolute (_absolute);
				}

				return _absolute;
			}
		}

		public string AppRelative {
			get {
				if (IsAppRelative)
					return Original;

				if (_appRelative == null) {
					string original = Original;
					
					if (!VirtualPathUtility.IsRooted (original))
						_appRelative = MakeRooted (original);
					else
						_appRelative = original;
					
					if (VirtualPathUtility.IsAbsolute (_appRelative))
						_appRelative = VirtualPathUtility.ToAppRelative (_appRelative);
				}

				return _appRelative;
			}
		}
		
		public string AppRelativeNotRooted {
			get {
				if (_appRelativeNotRooted == null)
					_appRelativeNotRooted = AppRelative.Substring (2);

				return _appRelativeNotRooted;
			}
		}

		public string Extension {
			get {
				if (_extension == null)
					_extension = VirtualPathUtility.GetExtension (Original);

				return _extension;
			}
		}

		public string Directory {
			get {
				if (_directory == null)
					_directory = VirtualPathUtility.GetDirectory (Absolute);

				return _directory;
			}
		}
		
		public VirtualPath (string vpath)
		{
			Original = vpath;

			IsRooted = VirtualPathUtility.IsRooted (vpath);
			IsAbsolute = VirtualPathUtility.IsAbsolute (vpath);
			IsAppRelative = VirtualPathUtility.IsAppRelative (vpath);
		}

		public bool StartsWith (string s)
		{
			return StrUtils.StartsWith (Original, s);
		}
		
		string MakeRooted (string original)
		{
			return VirtualPathUtility.Combine (HttpRuntime.AppDomainAppVirtualPath, original);
		}

		public void Dispose ()
		{
			_absolute = null;
			_appRelative = null;
			_appRelativeNotRooted = null;
			_extension = null;
			_directory = null;
		}
		
		public override string ToString ()
		{
			return Original;
		}
	}
}
#endif
