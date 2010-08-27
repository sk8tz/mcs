//
// ecmadoc.cs
//
// Author:
//   Jonathan Pryor  <jpryor@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Monodoc;
using Mono.Documentation;

using Mono.Options;
using Mono.Rocks;

namespace Mono.Documentation
{
	public class MDocUpdateEcmaXml : MDocCommand
	{
		string file = "CLILibraryTypes.xml";
		List<string> directories;
		Dictionary<string, List<string>> libraries = new Dictionary<string, List<string>>();

		public override void Run (IEnumerable<string> args)
		{
			string current_library = "";

			var options = new OptionSet () {
				{ "o|out=", 
					"{FILE} to generate/update documentation within.\n" + 
					"If not specified, will process " + file + ".\n" +
					"Set to '-' to skip updates and write to standard output.",
					v => file = v },
				{ "library=",
					"The {LIBRARY} that the following --type=TYPE types should be a part of.",
					v => current_library = v },
				{ "type=",
					"The full {TYPE} name of a type to copy into the output file.",
					v => Add (libraries, current_library, v) },
			};
			directories = Parse (options, args, "export-ecma-xml", 
					"[OPTIONS]+ DIRECTORIES",
					"Export mdoc documentation within DIRECTORIES into ECMA-format XML.\n\n" +
					"DIRECTORIES are mdoc(5) directories as produced by 'mdoc update'.");
			if (directories == null || directories.Count == 0)
				return;

			Update ();
		}

		static void Add (Dictionary<string, List<string>> libraries, string library, string type)
		{
			List<string> types;
			if (!libraries.TryGetValue (library, out types))
				libraries.Add (library, types = new List<string> ());
			types.Add (type.Replace ('/', '.').Replace ('+', '.'));
		}

		void Update ()
		{
			XDocument input = LoadFile (file);

			var seenLibraries = new HashSet<string> ();
			using (var output = CreateWriter (file)) {
				// spit out header comments, DTD, etc.
				foreach (var node in input.Nodes ()) {
					if (node.NodeType == XmlNodeType.Element)
						continue;
					node.WriteTo (output);
				}

				using (var librariesElement = new Element (output, o => o.WriteStartElement ("Libraries"))) {
					UpdateExistingLibraries (input, output, seenLibraries);
					GenerateMissingLibraries (input, output, seenLibraries);
				}
				output.WriteWhitespace ("\r\n");
			}
		}

		static XDocument LoadFile (string file)
		{
			if (file == "-" || !File.Exists (file))
				return CreateDefaultDocument ();

			var settings = new XmlReaderSettings {
				ProhibitDtd = false,
			};
			using (var reader = XmlReader.Create (file, settings))
				return XDocument.Load (reader);
		}

		static XDocument CreateDefaultDocument ()
		{
			return new XDocument (
					new XComment (" ====================================================================== "),
					new XComment (" This XML is a description of the Common Language Infrastructure (CLI) library. "),
					new XComment (" This file is a normative part of Partition IV of the following standards: ISO/IEC 23271 and ECMA 335 "),
					new XComment (" ====================================================================== "),
					new XDocumentType ("Libraries", null, "CLILibraryTypes.dtd", null),
					new XElement ("Libraries"));
		}

		static XmlWriter CreateWriter (string file)
		{
			var settings = new XmlWriterSettings {
				Encoding            = Encoding.UTF8,
				Indent              = true,
				IndentChars         = "\t",
				NewLineChars        = "\r\n",
				OmitXmlDeclaration  = true,
			};
			return file == "-"
				? XmlWriter.Create (Console.Out, settings)
				: XmlWriter.Create (Path.GetTempFileName (), settings);
		}

		struct Element : IDisposable {
			XmlWriter output;

			public Element (XmlWriter output, Action<XmlWriter> action)
			{
				this.output = output;
				action (output);
			}

			public void Dispose ()
			{
				output.WriteEndElement ();
			}
		}

		void UpdateExistingLibraries (XDocument input, XmlWriter output, HashSet<string> seenLibraries)
		{
		}

		void GenerateMissingLibraries (XDocument input, XmlWriter output, HashSet<string> seenLibraries)
		{
			foreach (KeyValuePair<string, List<string>> lib in libraries) {
				if (seenLibraries.Contains (lib.Key))
					continue;
				seenLibraries.Add (lib.Key);
				using (var typesElement = new Element (output, o => {
							o.WriteStartElement ("Types"); 
							o.WriteAttributeString ("Library", lib.Key);})) {
					foreach (string type in lib.Value) {
						LoadType (type).WriteTo (output);
					}
				}
			}
		}

		XElement LoadType (string type)
		{
			foreach (KeyValuePair<string, string> permutation in GetTypeDirectoryFilePermutations (type)) {
				foreach (string root in directories) {
					string path = Path.Combine (root, Path.Combine (permutation.Key, permutation.Value + ".xml"));
					if (File.Exists (path))
						return XElement.Load (path);
				}
			}
			throw new FileNotFoundException ("Unable to find documentation file for type: " + type + ".");
		}

		// type has been "normalized", which (alas) means we have ~no clue which
		// part is the namespace and which is the type name, particularly
		// problematic as types may be nested to any level.
		// Try ~all permutations. :-)
		static IEnumerable<KeyValuePair<string, string>> GetTypeDirectoryFilePermutations (string type)
		{
			int end = type.Length-1;
			int dot;
			while ((dot = type.LastIndexOf ('.', end)) >= 0) {
				yield return new KeyValuePair<string, string> (type.Substring (0, dot), type.Substring (dot+1));
			}
		}
	}
}
