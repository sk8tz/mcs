/**
 * Namespace: System.Web.Utils
 * Class:     FilePathParser
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.Utils
{
	internal class FilePathParser
	{
		private static char[] pathSeparators = {
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar
		};
		
		private string dirName;
		private string fileName;
		private string shortDirName;
		private string shortFileName;
		
		private bool   exists;
		
		public FilePathParser(string path, bool isFile, bool getShortNames)
		{
			path = path.Trim();
			if(Path.GetPathRoot(path).Length < path.Length)
			{
				path = path.TrimEnd(pathSeparators);
			}
			if(!isFile)
			{
				dirName = GetBaseDirOrRoot(path);
			} else
			{
				dirName = path;
			}
			if(getShortNames)
			{
				if(!Directory.Exists(dirName))
				{
					dirName = null;
					return;
				}
				shortDirName = GetShortPathName(dirName);
				if(shortDirName==null)
				{
					dirName = null;
					return;
				}
				if(shortDirName == dirName)
				{
					shortDirName = null;
				} else
				{
					throw new NotImplementedException();
				}
			}
		}
		
		public static string GetBaseDirOrRoot(string file)
		{
			string bDir = Path.GetDirectoryName(file);
			return ( bDir!=null ? bDir : GetPathRoot(file));
		}
		
		public static string GetShortPathName(string path)
		{
			//TODO: Native calls required, it's in kernel32.dll for windows
			throw new NotImplementedException();
		}
	}
}
