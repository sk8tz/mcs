//
// System.IO.Path Test Cases
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Add a more thorough workout for some
// of the "trickier" functions.

using NUnit.Framework;
using System.IO;
using System;
using System.Text;

namespace MonoTests.System.IO
{

public class PathTest : TestCase {

        string path1;
	string path2;
        string path3;
     
	public static ITest Suite {
		get {
			return new TestSuite(typeof(PathTest));
		}
	}

	public PathTest() : base ("MonoTests.System.IO.PathTest testcase") { }
	public PathTest( string name ): base(name) { }

        protected override void SetUp() {
		if ('/' == Path.VolumeSeparatorChar){
			path1 = "/foo/test.txt";
			path2 = "/etc";
			path3 = "init.d";
		} else {
			path1 = "c:\\foo\\test.txt";
			path2 = "c:\\winnt";
			path3 = "system32";
		}

		// For Mac.  Figure this out when we need it
		//path1 = "foo:test.txt";
		//path2 = "foo";
		//path3 = "bar";
        }


        public void TestChangeExtension() {
                string testPath = Path.ChangeExtension( path1, "doc" );

                #if WINDOWS
                AssertEquals( "c:\\foo\\test.doc", testPath );
                #elif UNIX
                AssertEquals( "/foo/test.doc", testPath );
                #elif MAC
                AssertEquals( "foo:test.doc", testPath );
                #endif
        }

        public void TestCombine() {
                string testPath = Path.Combine( path2, path3 );

                #if WINDOWS
                AssertEquals( "c:\\winnt\\system32", testPath );
                #elif UNIX
                AssertEquals( "/etc/init.d", testPath );
                #elif MAC
                AssertEquals( "foo:bar", testPath );
                #endif
        }

        public void TestDirectoryName() {
                string testDirName = Path.GetDirectoryName( path1 );

                #if WINDOWS
                AssertEquals( "c:\\foo", testDirName );
                #elif UNIX
                AssertEquals( "/etc", testDirName );
                #elif MAC
                AssertEquals( "foo", testDirName );
                #endif
        }

        public void TestGetExtension() {
                string testExtn = Path.GetExtension( path1 );

                AssertEquals( ".txt", testExtn );

                testExtn = Path.GetExtension( path2 );

                AssertEquals ( "", testExtn );
        }

        public void TestGetFileName() {
                string testFileName = Path.GetFileName( path1 );

                AssertEquals( "test.txt", testFileName );
        }

        public void TestGetFileNameWithoutExtension() {
                string testFileName = Path.GetFileNameWithoutExtension( path1 );

                AssertEquals( "test", testFileName );
        }

        public void TestGetFullPath() {
                string testFullPath = Path.GetFullPath( "foo.txt" );
         //       AssertEquals( "foo.txt", testFullPath );
        }
        
        public void TestGetTempPath() {
                string getTempPath = Path.GetTempPath();
                Assert ("Temp Path should not be empty",  getTempPath != String.Empty);
        }

        public void TestGetTempFileName() {
		string getTempFileName = "";
		try {
			getTempFileName = Path.GetTempFileName();
			Assert ("Temp file name should not be empty", getTempFileName != String.Empty);
			Assert ("File should exist", File.Exists(getTempFileName));
		} finally {
			if (getTempFileName != null && getTempFileName != String.Empty){
				File.Delete(getTempFileName);
			}
		}
        }

        public void TestHasExtension() {
                AssertEquals( true, Path.HasExtension( "foo.txt" ) );
                AssertEquals( false, Path.HasExtension( "foo" ) );
                AssertEquals( true, Path.HasExtension( path1 ) );
                AssertEquals( false, Path.HasExtension( path2 ) );
        }

        public void TestRooted() {
                Assert ("Path should be rooted", Path.IsPathRooted(path2));
                Assert ("Path should NOT be rooted", !Path.IsPathRooted(path3));
        }
}

}
