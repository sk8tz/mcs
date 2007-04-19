//
// System.Web.UI.TemplateParser
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
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

using System.CodeDom.Compiler;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Util;

#if NET_2_0
using System.Collections.Generic;
#endif

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class TemplateParser : BaseParser
	{
		string inputFile;
		string text;
		string privateBinPath;
		Hashtable mainAttributes;
		ArrayList dependencies;
		ArrayList assemblies;
		Hashtable anames;
		ArrayList imports;
		ArrayList interfaces;
		ArrayList scripts;
		Type baseType;
		bool baseTypeIsGlobal;
		string className;
		RootBuilder rootBuilder;
		bool debug;
		string compilerOptions;
		string language;
		bool strictOn = false;
		bool explicitOn = false;
		bool linePragmasOn = false;
		bool output_cache;
		int oc_duration;
		string oc_header, oc_custom, oc_param, oc_controls;
		bool oc_shared;
		OutputCacheLocation oc_location;
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
#if NET_2_0
		string src;
		string partialClassName;
		string codeFileBaseClass;
		string metaResourceKey;
		Type codeFileBaseClassType;
		List <UnknownAttributeDescriptor> unknownMainAttributes;
#endif
		Assembly srcAssembly;
		int appAssemblyIndex = -1;

		internal TemplateParser ()
		{
			imports = new ArrayList ();
#if NET_2_0
			AddNamespaces (imports);
#else
			imports.Add ("System");
			imports.Add ("System.Collections");
			imports.Add ("System.Collections.Specialized");
			imports.Add ("System.Configuration");
			imports.Add ("System.Text");
			imports.Add ("System.Text.RegularExpressions");
			imports.Add ("System.Web");
			imports.Add ("System.Web.Caching");
			imports.Add ("System.Web.Security");
			imports.Add ("System.Web.SessionState");
			imports.Add ("System.Web.UI");
			imports.Add ("System.Web.UI.WebControls");
			imports.Add ("System.Web.UI.HtmlControls");
#endif

			assemblies = new ArrayList ();
#if NET_2_0
			bool addAssembliesInBin = false;
			foreach (AssemblyInfo info in CompilationConfig.Assemblies) {
				if (info.Assembly == "*")
					addAssembliesInBin = true;
				else
					AddAssemblyByName (info.Assembly);
			}
			if (addAssembliesInBin)
				AddAssembliesInBin ();

			foreach (NamespaceInfo info in PagesConfig.Namespaces) {
				imports.Add (info.Namespace);
			}
#else
			foreach (string a in CompilationConfig.Assemblies)
				AddAssemblyByName (a);
			if (CompilationConfig.AssembliesInBin)
				AddAssembliesInBin ();
#endif

			language = CompilationConfig.DefaultLanguage;
		}
		
		internal void AddApplicationAssembly ()
		{
			if (Context.ApplicationInstance == null)
                                return; // this may happen if we have Global.asax and have
                                        // controls registered from Web.Config
			string location = Context.ApplicationInstance.AssemblyLocation;
			if (location != typeof (TemplateParser).Assembly.Location) {
				appAssemblyIndex = assemblies.Add (location);
			}
		}

		protected abstract Type CompileIntoType ();

#if NET_2_0
		void AddNamespaces (ArrayList imports)
		{
			if (BuildManager.HaveResources)
				imports.Add ("System.Resources");
			
			PagesSection pages = WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
			if (pages == null)
				return;

			NamespaceCollection namespaces = pages.Namespaces;
			if (namespaces == null || namespaces.Count == 0)
				return;

			foreach (NamespaceInfo nsi in namespaces)
				imports.Add (nsi.Namespace);
		}
#endif
		
		internal void RegisterCustomControl (string tagPrefix, string tagName, string src)
                {
                        string realpath = MapPath (src);
                        if (String.Compare (realpath, inputFile, false, invariantCulture) == 0)
                                return;
                        
                        if (!File.Exists (realpath))
                                throw new ParseException (Location, "Could not find file \"" + realpath + "\".");
                        string vpath = UrlUtils.Combine (BaseVirtualDir, src);
                        Type type = null;
                        AddDependency (realpath);
                        try {
                                ArrayList other_deps = new ArrayList ();
                                type = UserControlParser.GetCompiledType (vpath, realpath, other_deps, Context);
                                foreach (string s in other_deps) {
                                        AddDependency (s);
                                }
                        } catch (ParseException pe) {
                                if (this is UserControlParser)
                                        throw new ParseException (Location, pe.Message, pe);
                                throw;
                        }

                        AddAssembly (type.Assembly, true);
                        RootBuilder.Foundry.RegisterFoundry (tagPrefix, tagName, type);
                }

                internal void RegisterNamespace (string tagPrefix, string ns, string assembly)
                {
                        AddImport (ns);
                        Assembly ass = AddAssemblyByName (assembly);
                        AddDependency (ass.Location);
                        RootBuilder.Foundry.RegisterFoundry (tagPrefix, ass, ns);
                }

		internal virtual void HandleOptions (object obj)
		{
		}

		internal static string GetOneKey (Hashtable tbl)
		{
			foreach (object key in tbl.Keys)
				return key.ToString ();

			return null;
		}
		
		internal virtual void AddDirective (string directive, Hashtable atts)
		{
			if (String.Compare (directive, DefaultDirectiveName, true) == 0) {
				if (mainAttributes != null)
					ThrowParseException ("Only 1 " + DefaultDirectiveName + " is allowed");

				mainAttributes = atts;
				ProcessMainAttributes (mainAttributes);
				return;
			}

			int cmp = String.Compare ("Assembly", directive, true);
			if (cmp == 0) {
				string name = GetString (atts, "Name", null);
				string src = GetString (atts, "Src", null);

				if (atts.Count > 0)
					ThrowParseException ("Attribute " + GetOneKey (atts) + " unknown.");

				if (name == null && src == null)
					ThrowParseException ("You gotta specify Src or Name");
					
				if (name != null && src != null)
					ThrowParseException ("Src and Name cannot be used together");

				if (name != null) {
					AddAssemblyByName (name);
				} else {
					GetAssemblyFromSource (src);
				}

				return;
			}

			cmp = String.Compare ("Import", directive, true);
			if (cmp == 0) {
				string namesp = GetString (atts, "Namespace", null);
				if (atts.Count > 0)
					ThrowParseException ("Attribute " + GetOneKey (atts) + " unknown.");
				
				if (namesp != null && namesp != "")
					AddImport (namesp);
				return;
			}

			cmp = String.Compare ("Implements", directive, true);
			if (cmp == 0) {
				string ifacename = GetString (atts, "Interface", "");

				if (atts.Count > 0)
					ThrowParseException ("Attribute " + GetOneKey (atts) + " unknown.");
				
				Type iface = LoadType (ifacename);
				if (iface == null)
					ThrowParseException ("Cannot find type " + ifacename);

				if (!iface.IsInterface)
					ThrowParseException (iface + " is not an interface");

				AddInterface (iface.FullName);
				return;
			}

			cmp = String.Compare ("OutputCache", directive, true);
			if (cmp == 0) {
				HttpResponse response = HttpContext.Current.Response;
				if (response != null)
					response.Cache.SetValidUntilExpires (true);
				
				output_cache = true;
				
				if (atts ["Duration"] == null)
					ThrowParseException ("The directive is missing a 'duration' attribute.");
				if (atts ["VaryByParam"] == null)
					ThrowParseException ("This directive is missing a 'VaryByParam' " +
							"attribute, which should be set to \"none\", \"*\", " +
							"or a list of name/value pairs.");

				foreach (DictionaryEntry entry in atts) {
					string key = (string) entry.Key;
					switch (key.ToLower ()) {
					case "duration":
						oc_duration = Int32.Parse ((string) entry.Value);
						if (oc_duration < 1)
							ThrowParseException ("The 'duration' attribute must be set " +
									"to a positive integer value");
						break;
					case "varybyparam":
						oc_param = (string) entry.Value;
						if (String.Compare (oc_param, "none") == 0)
							oc_param = null;
						break;
					case "varybyheader":
						oc_header = (string) entry.Value;
						break;
					case "varybycustom":
						oc_custom = (string) entry.Value;
						break;
					case "location":
						if (!(this is PageParser))
							goto default;

						try {
							oc_location = (OutputCacheLocation) Enum.Parse (
								typeof (OutputCacheLocation), (string) entry.Value, true);
						} catch {
							ThrowParseException ("The 'location' attribute is case sensitive and " +
									"must be one of the following values: Any, Client, " +
									"Downstream, Server, None, ServerAndClient.");
						}
						break;
					case "varybycontrol":
						if (this is PageParser)
							goto default;

                                                oc_controls = (string) entry.Value;
						break;
					case "shared":
						if (this is PageParser)
							goto default;

						try {
							oc_shared = Boolean.Parse ((string) entry.Value);
						} catch {
							ThrowParseException ("The 'shared' attribute is case sensitive" +
									" and must be set to 'true' or 'false'.");
						}
						break;
					default:
						ThrowParseException ("The '" + key + "' attribute is not " +
								"supported by the 'Outputcache' directive.");
						break;
					}
					
				}
				
				return;
			}

			ThrowParseException ("Unknown directive: " + directive);
		}

		internal Type LoadType (string typeName)
		{
			// First try loaded assemblies, then try assemblies in Bin directory.
			Type type = null;
			bool seenBin = false;
			Assembly [] assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (Assembly ass in assemblies) {
				type = ass.GetType (typeName);
				if (type == null)
					continue;

				if (Path.GetDirectoryName (ass.Location) != PrivateBinPath) {
					AddAssembly (ass, true);
				} else {
					seenBin = true;
				}

				AddDependency (ass.Location);
				return type;
			}

			if (seenBin)
				return null;

			// Load from bin
			if (!Directory.Exists (PrivateBinPath))
				return null;

			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			foreach (string s in binDlls) {
				Assembly binA = Assembly.LoadFrom (s);
				type = binA.GetType (typeName);
				if (type == null)
					continue;

				AddDependency (binA.Location);
				return type;
			}

			return null;
		}

		void AddAssembliesInBin ()
		{
			if (!Directory.Exists (PrivateBinPath))
				return;

			string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
			foreach (string s in binDlls)
				assemblies.Add (s);
		}

		internal virtual void AddInterface (string iface)
		{
			if (interfaces == null)
				interfaces = new ArrayList ();

			if (!interfaces.Contains (iface))
				interfaces.Add (iface);
		}
		
		internal virtual void AddImport (string namesp)
		{
			if (imports == null)
				imports = new ArrayList ();

			if (!imports.Contains (namesp))
				imports.Add (namesp);
		}

		internal virtual void AddSourceDependency (string filename)
		{
			if (dependencies != null && dependencies.Contains (filename)) {
				ThrowParseException ("Circular file references are not allowed. File: " + filename);
			}

			AddDependency (filename);
		}

		internal virtual void AddDependency (string filename)
		{
			if (filename == "")
				return;

			if (dependencies == null)
				dependencies = new ArrayList ();

			if (!dependencies.Contains (filename))
				dependencies.Add (filename);
		}
		
		internal virtual void AddAssembly (Assembly assembly, bool fullPath)
		{
			if (assembly.Location == "")
				return;

			if (anames == null)
				anames = new Hashtable ();

			string name = assembly.GetName ().Name;
			string loc = assembly.Location;
			if (fullPath) {
				if (!assemblies.Contains (loc)) {
					assemblies.Add (loc);
				}

				anames [name] = loc;
				anames [loc] = assembly;
			} else {
				if (!assemblies.Contains (name)) {
					assemblies.Add (name);
				}

				anames [name] = assembly;
			}
		}

		internal virtual Assembly AddAssemblyByFileName (string filename)
		{
			Assembly assembly = null;
			Exception error = null;

			try {
				assembly = Assembly.LoadFrom (filename);
			} catch (Exception e) { error = e; }

			if (assembly == null)
				ThrowParseException ("Assembly " + filename + " not found", error);

			AddAssembly (assembly, true);
			return assembly;
		}

		internal virtual Assembly AddAssemblyByName (string name)
		{
			if (anames == null)
				anames = new Hashtable ();

			if (anames.Contains (name)) {
				object o = anames [name];
				if (o is string)
					o = anames [o];

				return (Assembly) o;
			}

			Assembly assembly = null;
			Exception error = null;
			if (name.IndexOf (',') != -1) {
				try {
					assembly = Assembly.Load (name);
				} catch (Exception e) { error = e; }
			}

			if (assembly == null) {
				try {
					assembly = Assembly.LoadWithPartialName (name);
				} catch (Exception e) { error = e; }
			}
			
			if (assembly == null)
				ThrowParseException ("Assembly " + name + " not found", error);

			AddAssembly (assembly, true);
			return assembly;
		}
		
		internal virtual void ProcessMainAttributes (Hashtable atts)
		{
			atts.Remove ("Description"); // ignored
#if NET_1_1
			atts.Remove ("CodeBehind");  // ignored
#endif
			atts.Remove ("AspCompat"); // ignored
#if NET_2_0
			// these two are ignored for the moment
			atts.Remove ("Async");
			atts.Remove ("AsyncTimeOut");
#endif
			
			debug = GetBool (atts, "Debug", true);
			compilerOptions = GetString (atts, "CompilerOptions", "");
			language = GetString (atts, "Language", CompilationConfig.DefaultLanguage);
			strictOn = GetBool (atts, "Strict", CompilationConfig.Strict);
			explicitOn = GetBool (atts, "Explicit", CompilationConfig.Explicit);
			linePragmasOn = GetBool (atts, "LinePragmas", false);
			
			string inherits = GetString (atts, "Inherits", null);
#if NET_2_0
			// In ASP 2, the source file is actually integrated with
			// the generated file via the use of partial classes. This
			// means that the code file has to be confirmed, but not
			// used at this point.
			src = GetString (atts, "CodeFile", null);
			codeFileBaseClass = GetString (atts, "CodeFileBaseClass", null);

			if (src == null && codeFileBaseClass != null)
				ThrowParseException ("The 'CodeFileBaseClass' attribute cannot be used without a 'CodeFile' attribute");
			
			if (src != null && inherits != null) {
				// Make sure the source exists
				src = UrlUtils.Combine (BaseVirtualDir, src);
				string realPath = MapPath (src, false);
				if (!File.Exists (realPath))
					ThrowParseException ("File " + src + " not found");

				// We are going to create a partial class that shares
				// the same name as the inherits tag, so reset the
				// name. The base type is changed because it is the
				// code file's responsibilty to extend the classes
				// needed.
				partialClassName = inherits;

				// Add the code file as an option to the
				// compiler. This lets both files be compiled at once.
				compilerOptions += " \"" + realPath + "\"";

				if (codeFileBaseClass != null) {
					try {
						codeFileBaseClassType = LoadType (codeFileBaseClass);
					} catch (Exception) {
					}

					if (codeFileBaseClassType == null)
						ThrowParseException ("Could not load type '{0}'", codeFileBaseClass);
				}
			} else if (inherits != null) {
				// We just set the inherits directly because this is a
				// Single-Page model.
				SetBaseType (inherits);
			}
#else
			string src = GetString (atts, "Src", null);

			if (src != null)
				srcAssembly = GetAssemblyFromSource (src);

			if (inherits != null)
				SetBaseType (inherits);
#endif
			className = GetString (atts, "ClassName", null);
			if (className != null) {
#if NET_2_0
				string [] identifiers = className.Split ('.');
				for (int i = 0; i < identifiers.Length; i++)
					if (!CodeGenerator.IsValidLanguageIndependentIdentifier (identifiers [i]))
						ThrowParseException (String.Format ("'{0}' is not a valid "
							+ "value for attribute 'classname'.", className));
#else
				if (!CodeGenerator.IsValidLanguageIndependentIdentifier (className))
					ThrowParseException (String.Format ("'{0}' is not a valid "
						+ "value for attribute 'classname'.", className));
#endif
			}

#if NET_2_0
			if (this is TemplateControlParser)
				metaResourceKey = GetString (atts, "meta:resourcekey", null);
			
			if (inherits != null && (this is PageParser || this is UserControlParser) && atts.Count > 0) {
				if (unknownMainAttributes == null)
					unknownMainAttributes = new List <UnknownAttributeDescriptor> ();
				string key, val;
				
				foreach (DictionaryEntry de in atts) {
					key = de.Key as string;
					val = de.Value as string;
					
					if (String.IsNullOrEmpty (key) || String.IsNullOrEmpty (val))
						continue;
					CheckUnknownAttribute (key, val, inherits);
				}
				return;
			}
#endif
			if (atts.Count > 0)
				ThrowParseException ("Unknown attribute: " + GetOneKey (atts));
		}

#if NET_2_0
		void CheckUnknownAttribute (string name, string val, string inherits)
		{
			MemberInfo mi = null;
			bool missing = false;
			string memberName = name.Trim ().ToLower (CultureInfo.InvariantCulture);
			Type parent = codeFileBaseClassType;

			if (parent == null)
				parent = baseType;
			
			try {
				MemberInfo[] infos = parent.GetMember (memberName,
								       MemberTypes.Field | MemberTypes.Property,
								       BindingFlags.Public | BindingFlags.Instance |
								       BindingFlags.IgnoreCase | BindingFlags.Static);
				if (infos.Length != 0) {
					// prefer public properties to public methods (it's what MS.NET does)
					foreach (MemberInfo tmp in infos) {
						if (tmp is PropertyInfo) {
							mi = tmp;
							break;
						}
					}
					if (mi == null)
						mi = infos [0];
				} else
					missing = true;
			} catch (Exception) {
				missing = true;
			}
			if (missing)
				ThrowParseException (
					"Error parsing attribute '{0}': Type '{1}' does not have a public property named '{0}'",
					memberName, inherits);
			
			Type memberType = null;
			if (mi is PropertyInfo) {
				PropertyInfo pi = mi as PropertyInfo;
				
				if (!pi.CanWrite)
					ThrowParseException (
						"Error parsing attribute '{0}': The '{0}' property is read-only and cannot be set.",
						memberName);
				memberType = pi.PropertyType;
			} else if (mi is FieldInfo) {
				memberType = ((FieldInfo)mi).FieldType;
			} else
				ThrowParseException ("Could not determine member the kind of '{0}' in base type '{1}",
						     memberName, inherits);
			TypeConverter converter = TypeDescriptor.GetConverter (memberType);
			bool convertible = true;
			object value = null;
			
			if (converter == null || !converter.CanConvertFrom (typeof (string)))
				convertible = false;

			if (convertible) {
				try {
					value = converter.ConvertFromInvariantString (val);
				} catch (Exception) {
					convertible = false;
				}
			}

			if (!convertible)
				ThrowParseException ("Error parsing attribute '{0}': Cannot create an object of type '{1}' from its string representation '{2}' for the '{3}' property.",
						     memberName, memberType, val, mi.Name);
			
			UnknownAttributeDescriptor desc = new UnknownAttributeDescriptor (mi, value);
			unknownMainAttributes.Add (desc);
		}
#endif
		
		internal void SetBaseType (string type)
		{
			if (type == DefaultBaseTypeName)
				return;

			Type parent = null;
			if (srcAssembly != null)
				parent = srcAssembly.GetType (type);

			if (parent == null)
				parent = LoadType (type);

			if (parent == null)
				ThrowParseException ("Cannot find type " + type);

			if (!DefaultBaseType.IsAssignableFrom (parent))
				ThrowParseException ("The parent type does not derive from " + DefaultBaseType);

			baseType = parent;
			if (parent.FullName.IndexOf ('.') == -1)
				baseTypeIsGlobal = true;
		}

		Assembly GetAssemblyFromSource (string vpath)
		{
			vpath = UrlUtils.Combine (BaseVirtualDir, vpath);
			string realPath = MapPath (vpath, false);
			if (!File.Exists (realPath))
				ThrowParseException ("File " + vpath + " not found");

			AddSourceDependency (realPath);

			CompilerResults result = CachingCompiler.Compile (language, realPath, realPath, assemblies);
			if (result.NativeCompilerReturnValue != 0) {
				StreamReader reader = new StreamReader (realPath);
				throw new CompilationException (realPath, result.Errors, reader.ReadToEnd ());
			}

			AddAssembly (result.CompiledAssembly, true);
			return result.CompiledAssembly;
		}
		
		internal abstract Type DefaultBaseType { get; }
		internal abstract string DefaultBaseTypeName { get; }
		internal abstract string DefaultDirectiveName { get; }

		internal string InputFile
		{
			get { return inputFile; }
			set { inputFile = value; }
		}

#if NET_2_0
		internal bool IsPartial {
			get { return src != null; }
		}

		internal string PartialClassName {
			get { return partialClassName; }
		}

		internal string CodeFileBaseClass {
			get { return codeFileBaseClass; }
		}

		internal string MetaResourceKey {
			get { return metaResourceKey; }
		}
		
		internal Type CodeFileBaseClassType
		{
			get { return codeFileBaseClassType; }
		}
		
		internal List <UnknownAttributeDescriptor> UnknownMainAttributes
		{
			get { return unknownMainAttributes; }
		}
#endif

		internal string Text
		{
			get { return text; }
			set { text = value; }
		}

		internal Type BaseType
		{
			get {
				if (baseType == null)
					baseType = DefaultBaseType;

				return baseType;
			}
		}
		
		internal bool BaseTypeIsGlobal {
			get { return baseTypeIsGlobal; }
		}
		
		internal string ClassName {
			get {
				if (className != null)
					return className;

#if NET_2_0
				string physPath = HttpContext.Current.Request.PhysicalApplicationPath;
				
				if (StrUtils.StartsWith (inputFile, physPath)) {
					className = inputFile.Substring (physPath.Length).ToLower (CultureInfo.InvariantCulture);
					className = className.Replace ('.', '_');
					className = className.Replace ('/', '_').Replace ('\\', '_');
				} else
#endif
				className = Path.GetFileName (inputFile).Replace ('.', '_');
				className = className.Replace ('-', '_'); 
				className = className.Replace (' ', '_');

				if (Char.IsDigit(className[0])) {
					className = "_" + className;
				}

				return className;
			}
		}

		internal string PrivateBinPath {
			get {
				if (privateBinPath != null)
					return privateBinPath;

				AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
				privateBinPath = Path.Combine (setup.ApplicationBase, setup.PrivateBinPath);

				return privateBinPath;
			}
		}

		internal ArrayList Scripts {
			get {
				if (scripts == null)
					scripts = new ArrayList ();

				return scripts;
			}
		}

		internal ArrayList Imports {
			get { return imports; }
		}

		internal ArrayList Assemblies {
			get {
				if (appAssemblyIndex != -1) {
					object o = assemblies [appAssemblyIndex];
					assemblies.RemoveAt (appAssemblyIndex);
					assemblies.Add (o);
					appAssemblyIndex = -1;
				}

				return assemblies;
			}
		}

		internal ArrayList Interfaces {
			get { return interfaces; }
		}

		internal RootBuilder RootBuilder {
			get { return rootBuilder; }
			set { rootBuilder = value; }
		}

		internal ArrayList Dependencies {
			get { return dependencies; }
			set { dependencies = value; }
		}

		internal string CompilerOptions {
			get { return compilerOptions; }
		}

		internal string Language {
			get { return language; }
		}

		internal bool StrictOn {
			get { return strictOn; }
		}

		internal bool ExplicitOn {
			get { return explicitOn; }
		}
		
		internal bool Debug {
			get { return debug; }
		}

		internal bool OutputCache {
			get { return output_cache; }
		}

		internal int OutputCacheDuration {
			get { return oc_duration; }
		}

		internal string OutputCacheVaryByHeader {
			get { return oc_header; }
		}

		internal string OutputCacheVaryByCustom {
			get { return oc_custom; }
		}

		internal string OutputCacheVaryByControls {
			get { return oc_controls; }
		}
		
		internal bool OutputCacheShared {
			get { return oc_shared; }
		}
		
		internal OutputCacheLocation OutputCacheLocation {
			get { return oc_location; }
		}

		internal string OutputCacheVaryByParam {
			get { return oc_param; }
		}

#if NET_2_0
		internal PagesSection PagesConfig {
			get {
				return WebConfigurationManager.GetSection ("system.web/pages") as PagesSection;
			}
		}
#else
		internal PagesConfiguration PagesConfig {
			get { return PagesConfiguration.GetInstance (Context); }
		}
#endif
	}
}

