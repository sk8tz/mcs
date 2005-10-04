//
// System.CodeDom.Compiler TempFileCollection Class implementation
//
// Author:
//	Dick Porter (dick@ximian.com)
//
// (C) Copyright 2003 Ximian, Inc.
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

using System.IO;
using System.Collections;

namespace System.CodeDom.Compiler
{
	public class TempFileCollection:ICollection, IEnumerable, IDisposable
	{
		Hashtable filehash;
		string tempdir;
		bool keepfiles;
		string basepath;
		Random rnd;
		
		public TempFileCollection(): this(null, false)
		{
		}

		public TempFileCollection(string tempDir): this(tempDir, false)
		{
		}

		public TempFileCollection(string tempDir, bool keepFiles)
		{
			filehash=new Hashtable();
			tempdir=tempDir;
			keepfiles=keepFiles;
		}

		public string BasePath
		{
			get {
				if(basepath==null) {
					if (tempdir==null) {
						/* Get the system temp dir */
						MonoIO.GetTempPath(out tempdir);
					}

					if (rnd == null)
						rnd = new Random ();

					string random = rnd.Next (10000,99999).ToString ();
					basepath = Path.Combine (tempdir, random);
				}

				return(basepath);
			}
		}

		public int Count
		{
			get {
				return(filehash.Count);
			}
		}

		public bool KeepFiles
		{
			get {
				return(keepfiles);
			}
			set {
				keepfiles=value;
			}
		}

		public string TempDir
		{
			get {
				if(tempdir==null) {
					return(String.Empty);
				} else {
					return(tempdir);
				}
			}
		}

		public string AddExtension(string fileExtension)
		{
			return(AddExtension(fileExtension, keepfiles));
		}

		public string AddExtension(string fileExtension, bool keepFile)
		{
			string filename=BasePath+"."+fileExtension;
			AddFile(filename, keepFile);
			return(filename);
		}

		public void AddFile(string fileName, bool keepFile)
		{
			filehash.Add(fileName, keepFile);
		}

		public void CopyTo(string[] fileNames, int start)
		{
			filehash.Keys.CopyTo(fileNames, start);
		}

		void ICollection.CopyTo(Array array, int start)
		{
			filehash.Keys.CopyTo(array, start);
		}

		object ICollection.SyncRoot {
			get {
				return filehash.SyncRoot;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return(false);
			}
		}
		
		void IDisposable.Dispose() 
		{
			Dispose(true);
		}
		
		public void Delete()
		{
			string[] filenames=new string[filehash.Count];
			filehash.Keys.CopyTo(filenames, 0);

			foreach(string file in filenames) {
				if((bool)filehash[file]==false) {
					File.Delete(file);
					filehash.Remove(file);
				}
			}
		}

		public IEnumerator GetEnumerator()
		{
			return(filehash.Keys.GetEnumerator());
		}

		protected virtual void Dispose(bool disposing)
		{
			Delete();
			if (disposing) {
				GC.SuppressFinalize (true);
			}
		}

		~TempFileCollection()
		{
			Dispose(false);
		}
		
	}
}
