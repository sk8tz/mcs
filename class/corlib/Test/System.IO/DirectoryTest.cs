//
// System.IO.Directory
//
// Authors: 
//	Ville Palo (vi64pa@kolumbus.fi)
//
// (C) 2003 Ville Palo
//
// TODO: Find out why ArgumentOutOfRange tests does not release directories properly
//

using NUnit.Framework;
using System.IO;
using System.Text;
using System;
using System.Globalization;
using System.Threading;

namespace MonoTests.System.IO {

[TestFixture]
public class DirectoryTest : Assertion {
	
	string TempFolder = Path.Combine (Path.GetTempPath (), "MonoTests.System.IO.Tests");
	static readonly char DSC = Path.DirectorySeparatorChar;

	[SetUp]
	public void SetUp ()
	{
		if (!Directory.Exists (TempFolder))
			Directory.CreateDirectory (TempFolder);

		Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US");
	}
	
	[TearDown]
	public void TearDown () {
		if (Directory.Exists (TempFolder))
			Directory.Delete (TempFolder, true);
	}

	[Test]
	public void CreateDirectory ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.1";
		DeleteDirectory (path);
		try {
			DirectoryInfo info = Directory.CreateDirectory (path);
			AssertEquals ("test#01", true, info.Exists);
			AssertEquals ("test#02", ".1", info.Extension);
			AssertEquals ("test#03", true, info.FullName.EndsWith ("DirectoryTest.Test.1"));
			AssertEquals ("test#04", "DirectoryTest.Test.1", info.Name);
		} finally {
			DeleteDirectory (path);		
		}
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void CreateDirectoryNotSupportedException ()
	{
		DeleteDirectory (":");
		DirectoryInfo info = Directory.CreateDirectory (":");
		DeleteDirectory (":");
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void CreateDirectoryArgumentNullException ()
	{
		DirectoryInfo info = Directory.CreateDirectory (null as string);		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CreateDirectoryArgumentException1 ()
	{
		DirectoryInfo info = Directory.CreateDirectory ("");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CreateDirectoryArgumentException2 ()
	{
		DirectoryInfo info = Directory.CreateDirectory ("            ");		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void CreateDirectoryArgumentException3 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test";
		DeleteDirectory (path);
		try {
			path += Path.InvalidPathChars [0];
			path += ".2";
			DirectoryInfo info = Directory.CreateDirectory (path);		
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	public void Delete ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.Delete.1";
		DeleteDirectory (path);
		try {
			Directory.CreateDirectory (path);
			AssertEquals ("test#01", true, Directory.Exists (path));
		
			Directory.CreateDirectory (path + DSC + "DirectoryTest.Test.Delete.1.2");
			AssertEquals ("test#02", true, Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"));
		
			Directory.Delete (path + DSC + "DirectoryTest.Test.Delete.1.2");
			AssertEquals ("test#03", false, Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"));
			AssertEquals ("test#04", true, Directory.Exists (path));
		
			Directory.Delete (path);
			AssertEquals ("test#05", false, Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"));
			AssertEquals ("test#06", false, Directory.Exists (path));		
	
			Directory.CreateDirectory (path);
			Directory.CreateDirectory (path + DSC + "DirectoryTest.Test.Delete.1.2");
			AssertEquals ("test#07", true, Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"));
			AssertEquals ("test#08", true, Directory.Exists (path));
		
			Directory.Delete (path, true);
			AssertEquals ("test#09", false, Directory.Exists (path + DSC + "DirectoryTest.Test.Delete.1.2"));
			AssertEquals ("test#10", false, Directory.Exists (path));
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]	
	[ExpectedException(typeof(ArgumentException))]
	public void DeleteArgumentException ()
	{
		Directory.Delete ("");		
	}

	[Test]	
	[ExpectedException(typeof(ArgumentException))]
	public void DeleteArgumentException2 ()
	{
		Directory.Delete ("     ");		
	}

	[Test]	
	[ExpectedException(typeof(ArgumentException))]
	public void DeleteArgumentException3 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.4";
		DeleteDirectory (path);
		
		path += Path.InvalidPathChars [0];
		Directory.Delete (path);
	}

	[Test]	
	[ExpectedException(typeof(ArgumentNullException))]
	public void DeleteArgumentNullException ()
	{
		Directory.Delete (null as string);		
	}

	[Test]	
	[ExpectedException(typeof(DirectoryNotFoundException))]
	public void DeleteDirectoryNotFoundException ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.5";
		DeleteDirectory (path);
		
		Directory.Delete (path);		
	}

	[Test]	
	[ExpectedException(typeof(IOException))]
	public void DeleteArgumentException4 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.6";
		DeleteDirectory (path);
		FileStream s = null;
		Directory.CreateDirectory (path);
		try {
			s = File.Create (path + DSC + "DirectoryTest.Test.6");
			Directory.Delete (path);
		} finally {
			if (s != null)
				s.Close ();
			DeleteDirectory (path);	
		};
	}

	[Test]
	public void Exists ()
	{
		AssertEquals ("test#01", false, Directory.Exists (null as string));
	}
	
	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void GetCreationTimeException1 ()
	{
		Directory.GetCreationTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetCreationTimeException2 ()
	{
		Directory.GetCreationTime ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException(typeof(IOException))]
#endif
	public void GetCreationTimeException_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetCreationTime.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetCreationTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assertion.AssertEquals ("#1", expectedTime.Year, time.Year);
			Assertion.AssertEquals ("#2", expectedTime.Month, time.Month);
			Assertion.AssertEquals ("#3", expectedTime.Day, time.Day);
			Assertion.AssertEquals ("#4", expectedTime.Hour, time.Hour);
			Assertion.AssertEquals ("#5", expectedTime.Second, time.Second);
			Assertion.AssertEquals ("#6", expectedTime.Millisecond, time.Millisecond);
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetCreationTimeException4 ()
	{
		Directory.GetCreationTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetCreationTimeException5 ()
	{
		Directory.GetCreationTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void GetCreationTimeUtcException1 ()
	{
		Directory.GetCreationTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetCreationTimeUtcException2 ()
	{
		Directory.GetCreationTimeUtc ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	public void GetCreationTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetCreationTimeUtc.1";
		DeleteDirectory (path);
		
		try {
			DateTime time = Directory.GetCreationTimeUtc (path);

#if NET_2_0
			Assertion.AssertEquals ("#1", 1601, time.Year);
			Assertion.AssertEquals ("#2", 1, time.Month);
			Assertion.AssertEquals ("#3", 1, time.Day);
			Assertion.AssertEquals ("#4", 0, time.Hour);
			Assertion.AssertEquals ("#5", 0, time.Second);
			Assertion.AssertEquals ("#6", 0, time.Millisecond);
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetCreationTimeUtcException4 ()
	{
		Directory.GetCreationTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetCreationTimeUtcException5 ()
	{
		Directory.GetCreationTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void GetLastAccessTimeException1 ()
	{
		Directory.GetLastAccessTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeException2 ()
	{
		Directory.GetLastAccessTime ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	public void GetLastAccessTime_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastAccessTime.1";
		DeleteDirectory (path);
		
		try {
			DateTime time = Directory.GetLastAccessTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assertion.AssertEquals ("#1", expectedTime.Year, time.Year);
			Assertion.AssertEquals ("#2", expectedTime.Month, time.Month);
			Assertion.AssertEquals ("#3", expectedTime.Day, time.Day);
			Assertion.AssertEquals ("#4", expectedTime.Hour, time.Hour);
			Assertion.AssertEquals ("#5", expectedTime.Second, time.Second);
			Assertion.AssertEquals ("#6", expectedTime.Millisecond, time.Millisecond);
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeException4 ()
	{
		Directory.GetLastAccessTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeException5 ()
	{
		Directory.GetLastAccessTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void GetLastAccessTimeUtcException1 ()
	{
		Directory.GetLastAccessTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeUtcException2 ()
	{
		Directory.GetLastAccessTimeUtc ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	public void GetLastAccessTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastAccessTimeUtc.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastAccessTimeUtc (path);

#if NET_2_0
			Assertion.AssertEquals ("#1", 1601, time.Year);
			Assertion.AssertEquals ("#2", 1, time.Month);
			Assertion.AssertEquals ("#3", 1, time.Day);
			Assertion.AssertEquals ("#4", 0, time.Hour);
			Assertion.AssertEquals ("#5", 0, time.Second);
			Assertion.AssertEquals ("#6", 0, time.Millisecond);
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeUtcException4 ()
	{
		Directory.GetLastAccessTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastAccessTimeUtcException5 ()
	{
		Directory.GetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void GetLastWriteTimeException1 ()
	{
		Directory.GetLastWriteTime (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeException2 ()
	{
		Directory.GetLastWriteTime ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	public void GetLastWriteTime_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastWriteTime.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastWriteTime (path);

#if NET_2_0
			DateTime expectedTime = (new DateTime (1601, 1, 1)).ToLocalTime ();
			Assertion.AssertEquals ("#1", expectedTime.Year, time.Year);
			Assertion.AssertEquals ("#2", expectedTime.Month, time.Month);
			Assertion.AssertEquals ("#3", expectedTime.Day, time.Day);
			Assertion.AssertEquals ("#4", expectedTime.Hour, time.Hour);
			Assertion.AssertEquals ("#5", expectedTime.Second, time.Second);
			Assertion.AssertEquals ("#6", expectedTime.Millisecond, time.Millisecond);
#endif
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeException4 ()
	{
		Directory.GetLastWriteTime ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeException5 ()
	{
		Directory.GetLastWriteTime (Path.InvalidPathChars [0].ToString ());
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void GetLastWriteTimeUtcException1 ()
	{
		Directory.GetLastWriteTimeUtc (null as string);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeUtcException2 ()
	{
		Directory.GetLastWriteTimeUtc ("");
	}
	
	[Test]
#if !NET_2_0
	[ExpectedException (typeof (IOException))]
#endif
	public void GetLastWriteTimeUtc_NonExistingPath ()
	{
		string path = TempFolder + DSC + "DirectoryTest.GetLastWriteTimeUtc.1";
		DeleteDirectory (path);
		try {
			DateTime time = Directory.GetLastWriteTimeUtc (path);

#if NET_2_0
			Assertion.AssertEquals ("#1", 1601, time.Year);
			Assertion.AssertEquals ("#2", 1, time.Month);
			Assertion.AssertEquals ("#3", 1, time.Day);
			Assertion.AssertEquals ("#4", 0, time.Hour);
			Assertion.AssertEquals ("#5", 0, time.Second);
			Assertion.AssertEquals ("#6", 0, time.Millisecond);
#endif
		} finally {
			DeleteDirectory (path);
		}
		
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeUtcException4 ()
	{
		Directory.GetLastWriteTimeUtc ("    ");
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void GetLastWriteTimeUtcException5 ()
	{
		Directory.GetLastWriteTimeUtc (Path.InvalidPathChars[0].ToString ());
	}

	[Test]
	public void Move ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.9";
		string path2 = TempFolder + DSC + "DirectoryTest.Test.10";
		DeleteDirectory (path);
		DeleteDirectory (path2);
		try {
			Directory.CreateDirectory (path);
			Directory.CreateDirectory (path + DSC + "dir");
			AssertEquals ("test#01", true, Directory.Exists (path + DSC + "dir"));
		
			Directory.Move (path, path2);
			AssertEquals ("test#02", false, Directory.Exists (path + DSC + "dir"));		
			AssertEquals ("test#03", true, Directory.Exists (path2 + DSC + "dir"));
		
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path2);
			if (Directory.Exists (path2 + DSC + "dir"))
				Directory.Delete (path2 + DSC + "dir", true);			
		}
	}
	
	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException1 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.8";
		DeleteDirectory (path);		
		try {
			Directory.Move (path, path);		
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException2 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.11";
		DeleteDirectory (path);		
		try {
			Directory.Move ("", path);		
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException3 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.12";
		DeleteDirectory (path);		
		try {
			Directory.Move ("             ", path);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]
	public void MoveException4 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.13";
		path += Path.InvalidPathChars [0];
		string path2 = TempFolder + DSC + "DirectoryTest.Test.13";		
		DeleteDirectory (path);
		DeleteDirectory (path2);
		try {
			Directory.CreateDirectory (path2);		
			Directory.Move (path2, path);
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path2);
		}
	}

	[Test]
	[ExpectedException(typeof(DirectoryNotFoundException))]
	public void MoveException5 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.14";
		DeleteDirectory (path);
		try {
			Directory.Move (path, path + "Test.Test");		
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path + "Test.Test");
		}
	}

	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException6 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.15";
		DeleteDirectory (path);
		try {
			Directory.CreateDirectory (path);		
			Directory.Move (path, path + DSC + "dir");
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path + DSC + "dir");			
		}
	}

	[Test]
	[ExpectedException(typeof(IOException))]
	public void MoveException7 ()
	{
		string path = TempFolder + DSC + "DirectoryTest.Test.16";
		string path2 = TempFolder + DSC + "DirectoryTest.Test.17";
		
		DeleteDirectory (path);
		DeleteDirectory (path2);		
		try {
			Directory.CreateDirectory (path);
			Directory.CreateDirectory (path2);
			Directory.Move (path, path2);
		} finally {
			DeleteDirectory (path);
			DeleteDirectory (path2);		
		}
	}
	
	[Test]
        [Ignore("Unix doesnt support CreationTime")]
	public void CreationTime ()
	{
		string path = TempFolder + DSC + "DirectoryTest.CreationTime.1";
		DeleteDirectory (path);
		
		try {
			Directory.CreateDirectory (path);
			Directory.SetCreationTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

			DateTime time = Directory.GetCreationTime (path);		
			AssertEquals ("test#01", 2003, time.Year);
			AssertEquals ("test#02", 6, time.Month);
			AssertEquals ("test#03", 4, time.Day);
			AssertEquals ("test#04", 6, time.Hour);
			AssertEquals ("test#05", 4, time.Minute);
			AssertEquals ("test#06", 0, time.Second);
		
			time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetCreationTimeUtc (path));
			AssertEquals ("test#07", 2003, time.Year);
			AssertEquals ("test#08", 6, time.Month);
			AssertEquals ("test#09", 4, time.Day);
			AssertEquals ("test#10", 6, time.Hour);
			AssertEquals ("test#11", 4, time.Minute);
			AssertEquals ("test#12", 0, time.Second);

			Directory.SetCreationTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
			time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetCreationTime (path));
			AssertEquals ("test#13", 2003, time.Year);
			AssertEquals ("test#14", 6, time.Month);
			AssertEquals ("test#15", 4, time.Day);
			AssertEquals ("test#16", 6, time.Hour);
			AssertEquals ("test#17", 4, time.Minute);
			AssertEquals ("test#18", 0, time.Second);

			time = Directory.GetCreationTimeUtc (path);		
			AssertEquals ("test#19", 2003, time.Year);
			AssertEquals ("test#20", 6, time.Month);
			AssertEquals ("test#21", 4, time.Day);
			AssertEquals ("test#22", 6, time.Hour);
			AssertEquals ("test#23", 4, time.Minute);
			AssertEquals ("test#24", 0, time.Second);
		} finally {
			DeleteDirectory (path);	
		}
	}

	[Test]
	public void LastAccessTime ()
	{
		string path = TempFolder + DSC + "DirectoryTest.AccessTime.1";
		DeleteDirectory (path);
		
		try {
			Directory.CreateDirectory (path);
			Directory.SetLastAccessTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

			DateTime time = Directory.GetLastAccessTime (path);		
			AssertEquals ("test#01", 2003, time.Year);
			AssertEquals ("test#02", 6, time.Month);
			AssertEquals ("test#03", 4, time.Day);
			AssertEquals ("test#04", 6, time.Hour);
			AssertEquals ("test#05", 4, time.Minute);
			AssertEquals ("test#06", 0, time.Second);
		
			time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetLastAccessTimeUtc (path));
			AssertEquals ("test#07", 2003, time.Year);
			AssertEquals ("test#08", 6, time.Month);
			AssertEquals ("test#09", 4, time.Day);
			AssertEquals ("test#10", 6, time.Hour);
			AssertEquals ("test#11", 4, time.Minute);
			AssertEquals ("test#12", 0, time.Second);

			Directory.SetLastAccessTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
			time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetLastAccessTime (path));
			AssertEquals ("test#13", 2003, time.Year);
			AssertEquals ("test#14", 6, time.Month);
			AssertEquals ("test#15", 4, time.Day);
			AssertEquals ("test#16", 6, time.Hour);
			AssertEquals ("test#17", 4, time.Minute);
			AssertEquals ("test#18", 0, time.Second);

			time = Directory.GetLastAccessTimeUtc (path);		
			AssertEquals ("test#19", 2003, time.Year);
			AssertEquals ("test#20", 6, time.Month);
			AssertEquals ("test#21", 4, time.Day);
			AssertEquals ("test#22", 6, time.Hour);
			AssertEquals ("test#23", 4, time.Minute);
			AssertEquals ("test#24", 0, time.Second);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	public void LastWriteTime ()
	{
		string path = TempFolder + DSC + "DirectoryTest.WriteTime.1";
		DeleteDirectory (path);		
		
		try {
			Directory.CreateDirectory (path);
			Directory.SetLastWriteTime (path, new DateTime (2003, 6, 4, 6, 4, 0));

			DateTime time = Directory.GetLastWriteTime (path);		
			AssertEquals ("test#01", 2003, time.Year);
			AssertEquals ("test#02", 6, time.Month);
			AssertEquals ("test#03", 4, time.Day);
			AssertEquals ("test#04", 6, time.Hour);
			AssertEquals ("test#05", 4, time.Minute);
			AssertEquals ("test#06", 0, time.Second);
		
			time = TimeZone.CurrentTimeZone.ToLocalTime (Directory.GetLastWriteTimeUtc (path));
			AssertEquals ("test#07", 2003, time.Year);
			AssertEquals ("test#08", 6, time.Month);
			AssertEquals ("test#09", 4, time.Day);
			AssertEquals ("test#10", 6, time.Hour);
			AssertEquals ("test#11", 4, time.Minute);
			AssertEquals ("test#12", 0, time.Second);

			Directory.SetLastWriteTimeUtc (path, new DateTime (2003, 6, 4, 6, 4, 0));
			time = TimeZone.CurrentTimeZone.ToUniversalTime (Directory.GetLastWriteTime (path));
			AssertEquals ("test#13", 2003, time.Year);
			AssertEquals ("test#14", 6, time.Month);
			AssertEquals ("test#15", 4, time.Day);
			AssertEquals ("test#16", 6, time.Hour);
			AssertEquals ("test#17", 4, time.Minute);
			AssertEquals ("test#18", 0, time.Second);

			time = Directory.GetLastWriteTimeUtc (path);		
			AssertEquals ("test#19", 2003, time.Year);
			AssertEquals ("test#20", 6, time.Month);
			AssertEquals ("test#21", 4, time.Day);
			AssertEquals ("test#22", 6, time.Hour);
			AssertEquals ("test#23", 4, time.Minute);
			AssertEquals ("test#24", 0, time.Second);
		} finally {
			DeleteDirectory (path);		
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void SetLastWriteTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastWriteTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastWriteTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastWriteTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetLastWriteTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + DSC + "DirectoryTest.SetLastWriteTime.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastWriteTime (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastWriteTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastWriteTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastWriteTimeException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastWriteTime (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
//	public void SetLastWriteTimeException6 ()
//	{
//		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
//		string path = TempFolder + Path.DirectorySeparatorChar + "DirectoryTest.SetLastWriteTime.1";
//
//		try {
//			if (!Directory.Exists (path))			
//				Directory.CreateDirectory (path);
//		
//			Directory.SetLastWriteTime (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void SetLastWriteTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastWriteTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastWriteTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastWriteTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetLastWriteTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + DSC + "DirectoryTest.SetLastWriteTimeUtc.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastWriteTimeUtc (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastWriteTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastWriteTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastWriteTimeUtcException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastWriteTimeUtc (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
//	public void SetLastWriteTimeUtcException6 ()
//	{
//		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastWriteTimeUtc.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetLastWriteTimeUtc (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void SetLastAccessTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetLastAccessTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastAccessTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastAccessTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetLastAccessTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTime.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastAccessTime (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastAccessTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastAccessTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastAccessTimeException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastAccessTime (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
//	public void SetLastAccessTimeException6 ()
//	{
//		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTime.1";
//
//		if (!Directory.Exists (path))			
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetLastAccessTime (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void SetLastAccessTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastAccessTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastAccessTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastAccessTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetLastAccessTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.2";
		DeleteDirectory (path);
		try {
			Directory.SetLastAccessTimeUtc (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastAccessTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastAccessTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetLastAccessTimeUtcException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetLastAccessTimeUtc (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
//	public void SetLastAccessTimeUtcException6 ()
//	{
//		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetLastAccessTimeUtc (path, time);
//		} finally {
//			DeleteDirectory (path);
//		}
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]
	public void SetCreationTimeException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);
		Directory.SetCreationTime (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetCreationTimeException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetCreationTime ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetCreationTimeException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + DSC + "DirectoryTest.SetCreationTime.2";
		DeleteDirectory (path);
		
		try {
			Directory.SetCreationTime (path, time);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetCreationTimeException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetCreationTime ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetCreationTimeException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetCreationTime (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
//	public void SetCreationTimeException6 ()
//	{
//		DateTime time = new DateTime (1003, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetCreationTime.1";
//
//		if (!Directory.Exists (path))			
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetCreationTime (path, time);
//			DeleteDirectory (path);			
//		} finally {
//			DeleteDirectory (path);
//		}
//
//	}

	[Test]
	[ExpectedException(typeof(ArgumentNullException))]	
	public void SetCreationTimeUtcException1 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetCreationTimeUtc (null as string, time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetCreationTimeUtcException2 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetCreationTimeUtc ("", time);
	}
	
	[Test]
	[ExpectedException(typeof(FileNotFoundException))]
	public void SetCreationTimeUtcException3 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.2";
		DeleteDirectory (path);
		
		try {
			Directory.SetCreationTimeUtc (path, time);
			DeleteDirectory (path);
		} finally {
			DeleteDirectory (path);
		}
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetCreationTimeUtcException4 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetCreationTimeUtc ("    ", time);
	}

	[Test]
	[ExpectedException(typeof(ArgumentException))]	
	public void SetCreationTimeUtcException5 ()
	{
		DateTime time = new DateTime (2003, 4, 6, 6, 4, 2);		
		Directory.SetCreationTimeUtc (Path.InvalidPathChars [0].ToString (), time);
	}

//	[Test]
//	[ExpectedException(typeof(ArgumentOutOfRangeException))]	
//	public void SetCreationTimeUtcException6 ()
//	{
//		DateTime time = new DateTime (1000, 4, 6, 6, 4, 2);
//		string path = TempFolder + DSC + "DirectoryTest.SetLastAccessTimeUtc.1";
//
//		if (!Directory.Exists (path))
//			Directory.CreateDirectory (path);
//		try {
//			Directory.SetCreationTimeUtc (path, time);
//			DeleteDirectory (path);
//		} finally {
//			DeleteDirectory (path);
//		}
//	}

	[Test]
	public void GetDirectories ()
	{
		string path = TempFolder;
		string DirPath = TempFolder + Path.DirectorySeparatorChar + ".GetDirectories";
		DeleteDirectory (DirPath);
		
		try {
			Directory.CreateDirectory (DirPath);
		
			string [] dirs = Directory.GetDirectories (path);
		
			foreach (string directory in dirs) {
			
				if (directory == DirPath)
					return;
			}
		
			Assert ("Directory Not Found", false);
		} finally {
			DeleteDirectory (DirPath);
		}
	}

	[Test]
	public void GetParentOfRootDirectory ()
	{
		DirectoryInfo info;

		info = Directory.GetParent (Path.GetPathRoot (Path.GetTempPath ()));
		AssertEquals (null, info);
	}
	
	[Test]
	public void GetFiles ()
	{
		string path = TempFolder;
		string DirPath = TempFolder + Path.DirectorySeparatorChar + ".GetFiles";
		if (File.Exists (DirPath))
			File.Delete (DirPath);
		
		try {
			File.Create (DirPath).Close ();
			string [] files = Directory.GetFiles (TempFolder);
			foreach (string directory in files) {
			
				if (directory == DirPath)
					return;
			}
		
			Assert ("File Not Found", false);
		} finally {
			if (File.Exists (DirPath))
				File.Delete (DirPath);
			
		}								
	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void SetCurrentDirectoryNull ()
	{
		Directory.SetCurrentDirectory (null);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void SetCurrentDirectoryEmpty ()
	{
		Directory.SetCurrentDirectory (String.Empty);
	}

	[Test]
	[ExpectedException (typeof (ArgumentException))]
	public void SetCurrentDirectoryWhitespace ()
	{
		Directory.SetCurrentDirectory (" ");
	}


	[Test]
	public void GetNoFiles () // Bug 58875. This throwed an exception on windows.
	{
		DirectoryInfo dir = new DirectoryInfo (".");
		dir.GetFiles ("*.nonext");
	}


	[Test]
	public void FilenameOnly () // bug 78209
	{
		Directory.GetParent ("somefile");
	}

	private void DeleteDirectory (string path)
	{
		if (Directory.Exists (path))
			Directory.Delete (path, true);		
	}
}
}
