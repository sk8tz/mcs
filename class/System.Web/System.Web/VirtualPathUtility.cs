//
// System.Web.VirtualPathUtility.cs
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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


using System.Web.Util;
using System.Text;

namespace System.Web {

#if NET_2_0
	public
#endif
	static class VirtualPathUtility
	{
		public static string AppendTrailingSlash (string virtualPath)
		{
			if (virtualPath == null)
				return virtualPath;

			int length = virtualPath.Length;
			if (length == 0 || virtualPath [length - 1] == '/')
				return virtualPath;

			return virtualPath + "/";
		}

		public static string Combine (string basePath, string relativePath)
		{
			basePath = Normalize (basePath);

			if (IsRooted (relativePath))
				return Normalize (relativePath);

			if (basePath [basePath.Length - 1] != '/') {
				if (basePath.Length > 1) {
					int lastSlash = basePath.LastIndexOf ('/');
					if (lastSlash >= 0)
						basePath = basePath.Substring (0, lastSlash + 1);
				}
				else { // "~" only
					basePath += "/";
				}
			}

			return Normalize (basePath + relativePath);
		}

		public static string GetDirectory (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "") // Yes, "" throws an ArgumentNullException
				throw new ArgumentNullException ("virtualPath");

			if (virtualPath [0] != '/')
				throw new ArgumentException ("The virtual path is not rooted", "virtualPath");

            		if (virtualPath == "/")
                		return null; //.net behavior

            		//In .Net - will look for one '/' before the last one, and will return it as a directory
            		//therefor we always should remove the last slash.
            		if (virtualPath.EndsWith("/")) 
                		virtualPath = virtualPath.Substring(0, virtualPath.Length - 1);

            		string result = UrlUtils.GetDirectory (virtualPath);
			return AppendTrailingSlash (result);
		}

		public static string GetExtension (string virtualPath)
		{
            		if (virtualPath != null && virtualPath != "" && 
				virtualPath.IndexOf('/') == -1)
                	{
				virtualPath = "./" + virtualPath;
			}

            		string filename = GetFileName (virtualPath);
			int dot = filename.LastIndexOf ('.');
			if (dot == -1 || dot == filename.Length + 1)
				return "";

			return filename.Substring (dot);
		}

		public static string GetFileName (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "") // Yes, "" throws an ArgumentNullException
				throw new ArgumentNullException ("virtualPath");
			
			return UrlUtils.GetFile (RemoveTrailingSlash (virtualPath));
		}

		static bool IsRooted (string virtualPath)
		{
			return IsAbsolute (virtualPath) || IsAppRelative (virtualPath);
		}

		public static bool IsAbsolute (string virtualPath)
		{
			if (StrUtils.IsNullOrEmpty (virtualPath))
				throw new ArgumentNullException ("virtualPath");

			return (virtualPath [0] == '/');
		}

		public static bool IsAppRelative (string virtualPath)
		{
			if (StrUtils.IsNullOrEmpty (virtualPath))
				throw new ArgumentNullException ("virtualPath");

			if (virtualPath.Length == 1 && virtualPath [0] == '~')
				return true;

			if (virtualPath [0] == '~' && virtualPath [1] == '/')
				return true;

			return false;
		}

		// MSDN: If the fromPath and toPath parameters are not rooted; that is, 
		// they do not equal the root operator (the tilde [~]), do not start with a tilde (~), 
		// such as a tilde and a slash mark (~/) or a tilde and a double backslash (~//), 
		// or do not start with a slash mark (/), an ArgumentException exception is thrown.
		public static string MakeRelative (string fromPath, string toPath)
		{
			if (fromPath == null || toPath == null)
				throw new NullReferenceException (); // yeah!

			if (toPath == "")
				return toPath;

			toPath = ToAbsoluteInternal (toPath);
			fromPath = ToAbsoluteInternal (fromPath);

			if (String.CompareOrdinal (fromPath, toPath) == 0 && fromPath [fromPath.Length - 1] == '/')
				return "./";

			string [] toPath_parts = toPath.Split ('/');
			string [] fromPath_parts = fromPath.Split ('/');
			int dest = 1;
			while (toPath_parts [dest] == fromPath_parts [dest]) {
				if (toPath_parts.Length == (dest + 1) || fromPath_parts.Length == (dest + 1)) {
					break;
				}
				dest++;
			}
			string res = "";
			for (int i = 1; i < fromPath_parts.Length - dest; i++) {
				res += "../";
			}
			res += String.Join ("/", toPath_parts, dest, toPath_parts.Length - dest);
			return res;
		}

		private static string ToAbsoluteInternal (string virtualPath)
		{
			if (IsAppRelative (virtualPath))
				return ToAbsolute (virtualPath);
			else if (IsAbsolute (virtualPath))
				return Normalize (virtualPath);

			throw new ArgumentOutOfRangeException ("Specified argument was out of the range of valid values.");
		}

		public static string RemoveTrailingSlash (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "")
				return null;

			int last = virtualPath.Length - 1;
			if (last == 0 || virtualPath [last] != '/')
				return virtualPath;

			return virtualPath.Substring (0, last);
		}

		public static string ToAbsolute (string virtualPath)
		{
			string apppath = HttpRuntime.AppDomainAppVirtualPath;
			if (apppath == null)
				throw new HttpException ("The path to the application is not known");

			return ToAbsolute (virtualPath,apppath);
		}

		// If virtualPath is: 
		// Absolute, the ToAbsolute method returns the virtual path with no changes.
		// Application relative, the ToAbsolute method adds applicationPath to the beginning of the virtual path.
		// Not rooted, the ToAbsolute method raises an ArgumentOutOfRangeException exception.
		public static string ToAbsolute (string virtualPath, string applicationPath)
		{
			if (StrUtils.IsNullOrEmpty (applicationPath))
				throw new ArgumentNullException ("applicationPath");

			if (StrUtils.IsNullOrEmpty (virtualPath))
				throw new ArgumentNullException ("virtualPath");

			if (IsAppRelative(virtualPath)) {
				if (applicationPath [0] != '/')
					throw new ArgumentException ("appPath is not rooted", "applicationPath");
				return Normalize ((applicationPath + (virtualPath.Length == 1 ? "/" : virtualPath.Substring (1))));
			}

			if (virtualPath [0] != '/')
				throw new ArgumentException (String.Format ("Relative path not allowed: '{0}'", virtualPath));

			return Normalize (virtualPath);

		}

		public static string ToAppRelative (string virtualPath)
		{
			string apppath = HttpRuntime.AppDomainAppVirtualPath;
			if (apppath == null)
				throw new HttpException ("The path to the application is not known");

			return ToAppRelative (virtualPath, apppath);
		}

		public static string ToAppRelative (string virtualPath, string applicationPath)
		{
			virtualPath = Normalize (virtualPath);
			
			if (IsAppRelative (virtualPath))
				return virtualPath;

			if (!IsAbsolute (applicationPath))
				throw new ArgumentException ("appPath is not absolute", "applicationPath");
			
			applicationPath = Normalize (applicationPath);

			if (applicationPath.Length == 1)
				return "~" + virtualPath;

			int appPath_lenght = applicationPath.Length;
			if (String.CompareOrdinal (virtualPath, applicationPath) == 0)
				return "~/";
			if (String.CompareOrdinal (virtualPath, 0, applicationPath, 0, appPath_lenght) == 0)
				return "~" + virtualPath.Substring (appPath_lenght);

			return virtualPath;
		}

		static char [] path_sep = { '/' };

		static string Normalize (string path)
		{
			if (!IsRooted (path))
				throw new ArgumentException (String.Format ("The relative virtual path '{0}' is not allowed here.", path));

			if (path.Length == 1) // '/' or '~'
				return path;

			path = Canonize (path);

			if (path.IndexOf ('.') < 0)
				return path;

			bool starts_with_tilda = false;
			bool ends_with_slash = false;
			string [] apppath_parts= null;

			if (path [0] == '~') {
				if (path.Length == 2) // "~/"
					return "~/";
				starts_with_tilda = true;
				path = path.Substring (1);
			}
			else if (path.Length == 1) { // "/"
				return "/";
			}

			if (path [path.Length - 1] == '/')
				ends_with_slash = true;

			string [] parts = StrUtils.SplitRemoveEmptyEntries (path, path_sep);
			int end = parts.Length;

			int dest = 0;

			for (int i = 0; i < end; i++) {
				string current = parts [i];
				if (current == ".")
					continue;

				if (current == "..") {
					dest--;

					if(dest >= 0)
						continue;

					if (starts_with_tilda) {
						if (apppath_parts == null) {
							string apppath = HttpRuntime.AppDomainAppVirtualPath;
							apppath_parts = StrUtils.SplitRemoveEmptyEntries (apppath, path_sep);
						}

						if ((apppath_parts.Length + dest) >= 0)
							continue;
					}
					
					throw new HttpException ("Cannot use a leading .. to exit above the top directory.");
				}

				if (dest >= 0)
					parts [dest] = current;
				else
					apppath_parts [apppath_parts.Length + dest] = current;
				
				dest++;
			}

			StringBuilder str = new StringBuilder();
			if (apppath_parts != null) {
				starts_with_tilda = false;
				int count = apppath_parts.Length;
				if (dest < 0)
					count += dest;
				for (int i = 0; i < count; i++) {
					str.Append ('/');
					str.Append (apppath_parts [i]);
				}
			}
			else if (starts_with_tilda) {
				str.Append ('~');
			}

			for (int i = 0; i < dest; i++) {
				str.Append ('/');
				str.Append (parts [i]);
			}

			if (str.Length > 0) {
				if (ends_with_slash)
					str.Append ('/');
			}
			else {
				return "/";
			}

			return str.ToString ();
		}

		static string Canonize (string path)
		{
			int index = -1;
			for (int i=0; i < path.Length; i++) {
				if ((path [i] == '\\') || (path [i] == '/' && (i + 1) < path.Length && (path [i + 1] == '/' || path [i + 1] == '\\'))) {
					index = i;
					break;
				}
			}
			if (index < 0)
				return path;

			StringBuilder sb = new StringBuilder (path.Length);
			sb.Append (path, 0, index);

			for (int i = index; i < path.Length; i++) {
				if (path [i] == '\\' || path [i] == '/') {
					int next = i + 1;
					if (next < path.Length && (path [next] == '\\' || path [next] == '/'))
						continue;
					sb.Append ('/');
				}
				else {
					sb.Append (path [i]);
				}
			}

			return sb.ToString ();
		}

	}
}
