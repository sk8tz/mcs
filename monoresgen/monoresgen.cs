/*
 * monoresgen: convert between the resource formats (.txt, .resources, .resx).
 *
 * Copyright (c) 2002 Ximian, Inc
 *
 * Author: Paolo Molaro (lupus@ximian.com)
 */

/*
 * TODO:
 * * escape/unescape in the .txt reader/writer to be able to roundtrip values with newlines
 *   (unlike the MS ResGen utility)
 * * add .po format to help traslators on unixy systems
 */

using System;
using System.IO;
using System.Collections;
using System.Resources;
using System.Reflection;

class ResGen {

	static Assembly swf;
	static Type resxr;
	static Type resxw;

	/*
	 * We load the ResX format stuff on demand, since the classes are in 
	 * System.Windows.Forms (!!!) and we can't depend on that assembly in mono, yet.
	 */
	static void LoadResX () {
		if (swf != null)
			return;
		try {
			swf = Assembly.LoadWithPartialName ("System.Windows.Forms");
			resxr = swf.GetType ("System.Resources.ResXResourceReader");
			resxw = swf.GetType ("System.Resources.ResXResourceWriter");
		} catch (Exception e) {
			throw new Exception ("Cannot load support for ResX format: " + e.Message);
		}
	}

	static void Usage () {
		string Usage = @"Mono Resource Generator version 0.1
Usage:
		monoresgen source.ext [dest.ext]
		monoresgen /compile source.ext[,dest.resources] [...]

Convert a resource file from one format to another.
The currently supported formats are: '.txt' '.resources' '.resx'.
If the destination file is not specified, source.resources will be used.
The /compile option takes a list of .resX or .txt files to convert to
.resources files in one bulk operation, replacing .ext with .resources for
the output file name.
";
		Console.WriteLine( Usage );
	}
	
	static IResourceReader GetReader (Stream stream, string name) {
		string format = Path.GetExtension (name);
		switch (format.ToLower ()) {
		case ".txt":
		case ".text":
			return new TxtResourceReader (stream);
		case ".resources":
			return new ResourceReader (stream);
		case ".resx":
			LoadResX ();
			return (IResourceReader)Activator.CreateInstance (resxr, new object[] {stream});
		default:
			throw new Exception ("Unknown format in file " + name);
		}
	}
	
	static IResourceWriter GetWriter (Stream stream, string name) {
		string format = Path.GetExtension (name);
		switch (format.ToLower ()) {
		case ".txt":
		case ".text":
			return new TxtResourceWriter (stream);
		case ".resources":
			return new ResourceWriter (stream);
		case ".resx":
			LoadResX ();
			return (IResourceWriter)Activator.CreateInstance (resxw, new object[] {stream});
		default:
			throw new Exception ("Unknown format in file " + name);
		}
	}
	
	static int CompileResourceFile(string sname, string dname ) {
		try {
		FileStream source, dest;
		IResourceReader reader;
		IResourceWriter writer;

			source = new FileStream (sname, FileMode.Open, FileAccess.Read);
			dest = new FileStream (dname, FileMode.OpenOrCreate, FileAccess.Write);

			reader = GetReader (source, sname);
			writer = GetWriter (dest, dname);

			int rescount = 0;
			foreach (DictionaryEntry e in reader) {
				rescount++;
				object val = e.Value;
				if (val is string)
					writer.AddResource ((string)e.Key, (string)e.Value);
				else
					writer.AddResource ((string)e.Key, e.Value);
			}
			Console.WriteLine( "Read in {0} resources from '{1}'", rescount, sname );

			reader.Close ();
			writer.Close ();
			Console.WriteLine("Writing resource file...  Done.");
		} catch (Exception e) {
			Console.WriteLine ("Error: {0}", e.Message);
			return 1;
		}
		return 0;
	}
	
	static int Main (string[] args) {
		string sname = "", dname = ""; 
		if ((int) args.Length < 1 || args[0] == "-h" || args[0] == "-?" || args[0] == "/h" || args[0] == "/?") {
			  Usage();
			  return 1;
		}		
		if (args[0] == "/compile" || args[0] == "-compile") {
			for ( int i=1; i< args.Length; i++ ) {				
				if ( args[i].IndexOf(",") != -1 ){
					string[] pair =  args[i].Split(',');
					sname = pair[0]; 
					dname = pair[1];
					if (dname == ""){
						Console.WriteLine(@"error: You must specify an input & outfile file name like this:");
						Console.WriteLine("inFile.txt,outFile.resources." );
						Console.WriteLine("You passed in '{0}'.", args[i] );
						return 1;
					}
				} else {
					sname = args[i]; 
					dname = Path.ChangeExtension (sname, "resources");
				}
				int ret = CompileResourceFile( sname, dname );
				if (ret != 0 ) {
					return ret;
				}
			}
			return 0;
		
		}
		else if (args.Length == 1) {
			sname = args [0];
			dname = Path.ChangeExtension (sname, "resources");
		} else if (args.Length != 2) {
			Usage ();
			return 1;
		} else {
			sname = args [0];
			dname = args [1];			
		}		
		return CompileResourceFile( sname, dname );
	}
}

class TxtResourceWriter : IResourceWriter {
	StreamWriter s;
	
	public TxtResourceWriter (Stream stream) {
		s = new StreamWriter (stream);
	}
	
	public void AddResource (string name, byte[] value) {
		throw new Exception ("Binary data not valid in a text resource file");
	}
	
	public void AddResource (string name, object value) {
		if (value is string) {
			AddResource (name, (string)value);
			return;
		}
		throw new Exception ("Objects not valid in a text resource file");
	}
	
	/* FIXME: handle newlines */
	public void AddResource (string name, string value) {
		s.WriteLine ("{0}={1}", name, value);
	}
	
	public void Close () {
		s.Close ();
	}
	
	public void Dispose () {}
	
	public void Generate () {}
}

class TxtResourceReader : IResourceReader {
	Hashtable data;
	Stream s;
	
	public TxtResourceReader (Stream stream) {
		data = new Hashtable ();
		s = stream;
		Load ();
	}
	
	public virtual void Close () {
	}
	
	public IDictionaryEnumerator GetEnumerator() {
		return data.GetEnumerator ();
	}
	
	void Load () {
		StreamReader reader = new StreamReader (s);
		string line, key, val;
		int epos, line_num = 0;
		while ((line = reader.ReadLine ()) != null) {
			line_num++;
			line = line.Trim ();
			if (line.Length == 0 || line [0] == '#' ||
			    line [0] == ';')
				continue;
			epos = line.IndexOf ('=');
			if (epos < 0) 
				throw new Exception ("Invalid format at line " + line_num);
			key = line.Substring (0, epos);
			val = line.Substring (epos + 1);
			key = key.Trim ();
			val = val.Trim ();
			if (key.Length == 0) 
				throw new Exception ("Key is empty at line " + line_num);
			data.Add (key, val);
		}
	}
	
	IEnumerator IEnumerable.GetEnumerator () {
		return ((IResourceReader) this).GetEnumerator();
	}

	void IDisposable.Dispose () {}
}

