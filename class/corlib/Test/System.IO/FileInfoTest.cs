// FileInfoTest.cs - NUnit Test Cases for System.IO.FileInfo class
//
// Ville Palo (vi64pa@koti.soon.fi)
// 
// (C) 2003 Ville Palo
// 


using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System.IO
{
	[TestFixture]
        public class FileInfoTest
        {
                public FileInfoTest() {}
                
                [SetUp]
                protected void SetUp() {
                }
                
                [TearDown]
                protected void TearDown() {}
                
                [Test]
                public void Ctr ()
                {
                	string path = "resources/FIT.Ctr.Test";
                	DeleteFile (path);
                	
                	FileInfo info = new FileInfo (path);
                	Assertion.AssertEquals ("test#01", true, info.DirectoryName.EndsWith ("resources"));
                	Assertion.AssertEquals ("test#02", false, info.Exists);
                	Assertion.AssertEquals ("test#03", ".Test", info.Extension);
                	Assertion.AssertEquals ("test#05", "FIT.Ctr.Test", info.Name);                	
                }
                
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CtorArgumentNullException ()
		{
			FileInfo info = new FileInfo (null);			
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CtorArgumentException1 ()
		{
			FileInfo info = new FileInfo ("");			
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CtorArgumentException2 ()
		{
			FileInfo info = new FileInfo ("      ");			
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CtorArgumentException3 ()
		{
			string path = "";
			foreach (char c in Path.InvalidPathChars) {
				path += c;
			}
			FileInfo info = new FileInfo (path);			
		}
			       
		[Test]
		public void Directory ()
		{
			string path = "resources/FIT.Directory.Test";
			DeleteFile (path);
			
			FileInfo info = new FileInfo (path);
			DirectoryInfo dir = info.Directory;
			Assertion.AssertEquals ("test#01", "resources", dir.Name);
		}
		
		[Test]
		public void Exists ()
		{
			string path = "resources/FIT.Exists.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assertion.AssertEquals ("test#01", false, info.Exists);
			
				File.Create (path).Close ();
				Assertion.AssertEquals ("test#02", false, info.Exists);
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#03", true, info.Exists);			
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void Length ()
		{
			string path = "resources/FIT.Length.Test";
			DeleteFile (path);
			
			try {
				FileStream stream = File.Create (path);
				FileInfo info = new FileInfo (path);
				Assertion.AssertEquals ("test#01", 0, info.Length);
				stream.WriteByte (12);
				stream.Flush ();
				Assertion.AssertEquals ("test#02", 0, info.Length);
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#03", 1, info.Length);
				stream.Close ();
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void LengthException ()
		{
			string path = "resources/FIT.LengthException.Test";
			DeleteFile (path);
			FileInfo info = new FileInfo (path);
			long l = info.Length;
		}
		
		[Test]
		public void AppendText ()
		{
			string path = "resources/FIT.AppendText.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
			
				Assertion.AssertEquals ("test#01", false, info.Exists);
			
				StreamWriter writer = info.AppendText ();
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#02", true, info.Exists );
				
				writer.Write ("aaa");
				writer.Flush ();
				writer.Close ();
			
				Assertion.AssertEquals ("test#03", 0, info.Length);
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#04", 3, info.Length);
			} finally {
				DeleteFile (path);
			}
			
		}
		
		[Test]
		public void CopyTo ()
		{
			string path1 = "resources/FIT.CopyTo.Source.Test";
			string path2 = "resources/FIT.CopyTo.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);
			try {
				File.Create (path1).Close ();
			
				FileInfo info = new FileInfo (path1);
				Assertion.AssertEquals ("test#01", true, info.Exists);
						
				FileInfo info2 = info.CopyTo (path2);
				info = new FileInfo (path1);
				Assertion.AssertEquals ("test#02", true, info2.Exists);
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);			
			}
		}
		
		[Test]
		public void CopyTo2 ()
		{
			string path1 = "resources/FIT.CopyTo2.Source.Test";
			string path2 = "resources/FIT.CopyTo2.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);
			try {
				File.Create (path1).Close ();
				File.Create (path2).Close ();		
				FileInfo info = new FileInfo (path1);
			
				FileInfo info2 = info.CopyTo (path2, true);
				info = new FileInfo (path1);
				Assertion.AssertEquals ("test#01", true, info.Exists);
				Assertion.AssertEquals ("test#02", true, info2.Exists);
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}
		
		[Test]
		[ExpectedException (typeof (IOException))]
		public void CopyToIOException ()
		{
			string path1 = "resources/FIT.CopyToException.Source.Test";
			string path2 = "resources/FIT.CopyToException.Dest.Test";

			try {
				DeleteFile (path1);
				DeleteFile (path2);
				File.Create (path1).Close ();
				File.Create (path2).Close ();		
				FileInfo info = new FileInfo (path1);			
				FileInfo info2 = info.CopyTo (path2);			
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void CopyToIOException2 ()
		{
			string path1 = "resources/FIT.CopyToException.Source.Test";
			string path2 = "resources/FIT.CopyToException.Dest.Test";

			try {
				DeleteFile (path1);
				DeleteFile (path2);
				File.Create (path1).Close ();
				File.Create (path2).Close ();		
				FileInfo info = new FileInfo (path1);			
				FileInfo info2 = info.CopyTo (path2, false);
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CopyToArgumentNullException ()
		{
			string path = "resources/FIT.CopyToArgumentNullException.Test";
			DeleteFile (path);
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				info.CopyTo (null, false);
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyToArgumentException1 ()
		{
			string path = "resources/FIT.CopyToArgument1Exception.Test";
			DeleteFile (path);
			
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				info.CopyTo ("", false);
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyToArgumentException2 ()
		{
			string path = "resources/FIT.CopyToArgument2Exception.Test";
			DeleteFile (path);
			
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				info.CopyTo ("    ", false);
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyToArgumentException3 ()
		{
			string path = "resources/FIT.CopyToArgument3Exception.Test";
			DeleteFile (path);
			
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				info.CopyTo ("    ", false);
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CopyToArgumentException4 ()
		{
			string path = "resources/FIT.CopyToArgument4Exception.Test";
			string path2 = "";
			DeleteFile (path);
			
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
			
				foreach (char c in Path.InvalidPathChars) {
					path2 += c;
				}
				info.CopyTo (path2, false);
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void Create ()
		{
			string path = "resources/FIT.Create.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assertion.AssertEquals ("test#01", false, info.Exists);
				FileStream stream = info.Create ();				
				Assertion.AssertEquals ("test#02", false, info.Exists);
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#03", true, info.Exists);
				Assertion.AssertEquals ("test#04", true, stream.CanRead);
				Assertion.AssertEquals ("test#05", true, stream.CanWrite);
				Assertion.AssertEquals ("test#06", true, stream.CanSeek);
				stream.Close ();
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		public void CreateText ()
		{
			string path = "resources/FIT.CreateText.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assertion.AssertEquals ("test#01", false, info.Exists);
				StreamWriter writer = info.CreateText ();
				writer.WriteLine ("test");
				writer.Close ();
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#02", true, info.Exists);
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void CreateTextUnauthorizedAccessException ()
		{
			string path = "resources";
			
			FileInfo info = new FileInfo (path);
			Assertion.AssertEquals ("test#01", false, info.Exists);
			StreamWriter writer = info.CreateText ();
			
		}
		
		[Test]
		public void Delete ()
		{
			string path = "resources/FIT.Delete.Test";
			DeleteFile (path);
			
			try {
				FileInfo info = new FileInfo (path);
				Assertion.AssertEquals ("test#01", false, info.Exists);
				info.Create ().Close ();
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#02", true, info.Exists);
				info.Delete ();
				Assertion.AssertEquals ("test#03", true, info.Exists);
				info = new FileInfo (path);
				Assertion.AssertEquals ("test#04", false, info.Exists);								
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (UnauthorizedAccessException))]
		public void DeleteUnauthorizedAccessException ()
		{
			string path = "resources";
			FileInfo info = new FileInfo (path);
			info.Delete ();			
		}
		
		[Test]
		public void MoveTo ()
		{
			string path1 = "resources/FIT.MoveTo.Source.Test";
			string path2 = "resources/FIT.MoveTo.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);
			
			try {
				File.Create (path1).Close ();
				FileInfo info = new FileInfo (path1);
				FileInfo info2 = new FileInfo (path2);
				Assertion.AssertEquals ("test#01", false, info2.Exists);
				
				info.MoveTo (path2);
				info2 = new FileInfo (path2);
				Assertion.AssertEquals ("test#02", true, info2.Exists);	
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);			
			}			
		}
		
		[Test]
		[ExpectedException (typeof (IOException))]
		public void MoveToIOException ()
		{
			string path1 = "resources/FIT.MoveToIOException.Source.Test";
			string path2 = "resources/FIT.MoveToIOException.Dest.Test";
			DeleteFile (path1);
			DeleteFile (path2);

			try {
				File.Create (path1).Close ();
				File.Create (path2).Close ();
				
				FileInfo info = new FileInfo (path1);
				info.MoveTo (path2);
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MoveToArgumentNullException ()
		{
			string path = "resources/FIT.MoveToArgumentNullException.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();				
				FileInfo info = new FileInfo (path);
				info.MoveTo (null);
			} finally {
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MoveToArgumentException ()
		{
			string path = "resources/FIT.MoveToArgumentException.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();				
				FileInfo info = new FileInfo (path);
				info.MoveTo ("   ");
			} finally {
				DeleteFile (path);
			}
		}

		/// <summary>
		/// msdn says this should throw UnauthorizedAccessException, byt
		/// it throws IOException.
		/// </summary>
		[Test]
		[ExpectedException (typeof (IOException))]
		public void MoveToUnauthorizedAccessException ()
		{
			string path = "resources/FIT.UnauthorizedAccessException.Test";
			DeleteFile (path);

			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				info.MoveTo ("resources");
			} finally {
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void MoveToFileNotFoundException ()
		{
			string path1 = "resources/FIT.MoveToFileNotFoundException.Src";
			string path2 = "resources/FIT.MoveToFileNotFoundException.Dst";
			DeleteFile (path1);
			DeleteFile (path2);
			
			try {
				FileInfo info = new FileInfo (path1);
				info.MoveTo (path2);
			} finally {
				DeleteFile (path1);
				DeleteFile (path2);
			}
		}
				
		[Test]
		public void Open ()
		{
			string path = "resources/FIT.Open.Test";
			DeleteFile (path);
			FileStream stream = null;
			try {
				FileInfo info = new FileInfo (path);
				stream = info.Open (FileMode.CreateNew);
				Assertion.AssertEquals ("test#01", true, stream.CanRead);
				Assertion.AssertEquals ("test#02", true, stream.CanSeek);
				Assertion.AssertEquals ("test#03", true, stream.CanWrite);				
				stream.Close ();
				
				stream = info.Open (FileMode.Open);				
				Assertion.AssertEquals ("test#04", true, stream.CanRead);
				Assertion.AssertEquals ("test#05", true, stream.CanSeek);
				Assertion.AssertEquals ("test#06", true, stream.CanWrite);				
				stream.Close ();
				
				stream = info.Open (FileMode.Append, FileAccess.Write);
				Assertion.AssertEquals ("test#07", false, stream.CanRead);
				Assertion.AssertEquals ("test#08", true, stream.CanSeek);
				Assertion.AssertEquals ("test#09", true, stream.CanWrite);				
				stream.Close ();				

				stream = info.Open (FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				Assertion.AssertEquals ("test#10", true, stream.CanRead);
				Assertion.AssertEquals ("test#11", true, stream.CanSeek);
				Assertion.AssertEquals ("test#12", true, stream.CanWrite);				
				stream.Close ();												
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException(typeof(FileNotFoundException))]
		public void OpenFileNotFoundException ()
		{
			string path = "resources/FIT.OpenFileNotFoundException.Test";
			DeleteFile (path);
			
			FileInfo info = new FileInfo (path);
			info.Open (FileMode.Open);
		}
		
		[Test]
		public void OpenRead ()
		{
			string path = "resources/FIT.OpenRead.Test";
			DeleteFile (path);
			FileStream stream = null;
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				stream = info.OpenRead ();
				Assertion.AssertEquals ("test#01", true, stream.CanRead);
				Assertion.AssertEquals ("test#02", true, stream.CanSeek);
				Assertion.AssertEquals ("test#03", false, stream.CanWrite);
				stream.Close ();
				
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException(typeof (IOException))]
		public void OpenReadIOException ()
		{
			string path = "resources/FIT.OpenReadIOException.Test";
			DeleteFile (path);
			FileStream stream = null;
			
			try {
				stream = File.Create (path);
				FileInfo info = new FileInfo (path);
				info.OpenRead ();
			} finally {
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}

		[Test]
		[ExpectedException(typeof (UnauthorizedAccessException))]
		public void OpenReadUnauthorizedAccessException ()
		{
			string path = "resources";
			
			FileInfo info = new FileInfo (path);
			info.OpenRead ();
		}
		
		[Test]
		public void OpenText ()
		{
			string path = "resources/FIT.OpenText.Test";
			DeleteFile (path);
			StreamReader reader = null;
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				reader = info.OpenText ();
				Assertion.AssertEquals ("test#01", -1, reader.Peek ());		
			} finally {
				
				if (reader != null)
					reader.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException(typeof (FileNotFoundException))]
		public void OpenTextFileNotFoundException ()
		{
			string path = "resources/FIT.OpenTextFileNotFoundException.Test";
			DeleteFile (path);
			FileInfo info = new FileInfo (path);
			info.OpenText ();
		}

		[Test]
		[ExpectedException(typeof (UnauthorizedAccessException))]
		public void OpenTextUnauthorizedAccessException ()
		{
			string path = "resources";
			
			FileInfo info = new FileInfo (path);
			info.OpenText ();
		}

		[Test]
		public void OpenWrite ()
		{
			string path = "resources/FIT.OpenWrite.Test";
			DeleteFile (path);
			FileStream stream = null;
			try {
				File.Create (path).Close ();
				FileInfo info = new FileInfo (path);
				stream = info.OpenWrite ();
				Assertion.AssertEquals ("test#01", false, stream.CanRead);
				Assertion.AssertEquals ("test#02", true, stream.CanSeek);
				Assertion.AssertEquals ("test#03", true, stream.CanWrite);
			} finally {
				
				if (stream != null)
					stream.Close ();
				DeleteFile (path);
			}
		}
		
		[Test]
		[ExpectedException(typeof (UnauthorizedAccessException))]
		public void OpenWriteUnauthorizedAccessException ()
		{
			string path = "resources";
			
			FileInfo info = new FileInfo (path);
			info.OpenWrite ();
		}

		private void DeleteFile (string path)
		{
			if (File.Exists (path))
				File.Delete (path);
		}
        }
}

