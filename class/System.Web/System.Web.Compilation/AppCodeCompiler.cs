//
// System.Web.Compilation.AppCodeCompiler: A compiler for the App_Code folder
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Profile;
using System.Web.Util;

namespace System.Web.Compilation
{
	internal class AppCodeAssembly
	{
		private List<string> files;
		private List<CodeCompileUnit> units;
		
		private string name;
		private string path;
		private bool validAssembly;
		private string outputAssemblyName;

		public string OutputAssemblyName
		{
			get {
				return outputAssemblyName;
			}
		}
		
		public bool IsValid
		{
			get { return validAssembly; }
		}

		public string SourcePath
		{
			get { return path; }
		}

// temporary
		public string Name
		{
			get { return name; }
		}
		
		public List<string> Files
		{
			get { return files; }
		}
// temporary
		
		public AppCodeAssembly (string name, string path)
		{
			this.files = new List<string> ();
			this.units = new List<CodeCompileUnit> ();
			this.validAssembly = true;
			this.name = name;
			this.path = path;
		}

		public void AddFile (string path)
		{
			files.Add (path);
		}

		public void AddUnit (CodeCompileUnit unit)
		{
			units.Add (unit);
		}
		
		object OnCreateTemporaryAssemblyFile (string path)
		{
			FileStream f = new FileStream (path, FileMode.CreateNew);
			f.Close ();
			return path;
		}
		
		// Build and add the assembly to the BuildManager's
		// CodeAssemblies collection
		public void Build (string[] binAssemblies)
		{
			Type compilerProvider = null;
			CompilerInfo compilerInfo = null, cit;
			string extension, language, cpfile = null;
			List<string> knownfiles = new List<string>();
			List<string> unknownfiles = new List<string>();
			
			// First make sure all the files are in the same
			// language
			bool known = false, unknown = false;
			foreach (string f in files) {
				known = true;
				language = null;
				
				extension = Path.GetExtension (f);
				if (String.IsNullOrEmpty (extension) || !CodeDomProvider.IsDefinedExtension (extension))
					known = false;
				if (known) {
					language = CodeDomProvider.GetLanguageFromExtension(extension);
					if (!CodeDomProvider.IsDefinedLanguage (language))
						known = false;
				}
				if (!known || language == null) {
					unknownfiles.Add (f);
					continue;
				}
				
				cit = CodeDomProvider.GetCompilerInfo (language);
				if (cit == null || !cit.IsCodeDomProviderTypeValid)
					continue;
				if (compilerProvider == null) {
					cpfile = f;
					compilerProvider = cit.CodeDomProviderType;
					compilerInfo = cit;
				} else if (compilerProvider != cit.CodeDomProviderType)
					throw new HttpException (
						String.Format (
							"Files {0} and {1} are in different languages - they cannot be compiled into the same assembly",
							Path.GetFileName (cpfile),
							Path.GetFileName (f)));
				knownfiles.Add (f);
			}

			CodeDomProvider provider = null;
			CompilationSection compilationSection = WebConfigurationManager.GetSection ("system.web/compilation") as CompilationSection;
			if (compilerInfo == null) {
				if (!CodeDomProvider.IsDefinedLanguage (compilationSection.DefaultLanguage))
					throw new HttpException ("Failed to retrieve default source language");
				compilerInfo = CodeDomProvider.GetCompilerInfo (compilationSection.DefaultLanguage);
				if (compilerInfo == null || !compilerInfo.IsCodeDomProviderTypeValid)
					throw new HttpException ("Internal error while initializing application");
				provider = compilerInfo.CreateProvider ();
				if (provider == null)
					throw new HttpException ("A code provider error occurred while initializing application.");
			}

			provider = compilerInfo.CreateProvider ();
			if (provider == null)
				throw new HttpException ("A code provider error occurred while initializing application.");

			AssemblyBuilder abuilder = new AssemblyBuilder (provider);
			foreach (string file in knownfiles)
				abuilder.AddCodeFile (file);
			foreach (CodeCompileUnit unit in units)
				abuilder.AddCodeCompileUnit (unit);
			
			BuildProvider bprovider;
			CompilerParameters parameters = compilerInfo.CreateDefaultCompilerParameters ();
			parameters.IncludeDebugInformation = compilationSection.Debug;
			
			if (binAssemblies != null && binAssemblies.Length > 0)
				parameters.ReferencedAssemblies.AddRange (binAssemblies);
			
			if (compilationSection != null) {
				AssemblyName asmName;
				foreach (AssemblyInfo ai in compilationSection.Assemblies)
					if (ai.Assembly != "*") {
						try {
							asmName = new AssemblyName (ai.Assembly);
							parameters.ReferencedAssemblies.Add (asmName.Name);
						} catch (Exception ex) {
							throw new HttpException (
								String.Format ("Could not find assembly {0}.", ai.Assembly),
								ex);
						}
					}
				
				BuildProviderCollection buildProviders = compilationSection.BuildProviders;
				
				foreach (string file in unknownfiles) {
					bprovider = GetBuildProviderFor (file, buildProviders);
					if (bprovider == null)
						continue;
					bprovider.GenerateCode (abuilder);
					unknown = true;
				}
			}

			if (knownfiles.Count == 0 && unknownfiles.Count == 0)
				return;
			
			outputAssemblyName = (string)FileUtils.CreateTemporaryFile (
				AppDomain.CurrentDomain.SetupInformation.DynamicBase,
				name, "dll", OnCreateTemporaryAssemblyFile);
			parameters.OutputAssembly = outputAssemblyName;
			foreach (Assembly a in BuildManager.TopLevelAssemblies)
				parameters.ReferencedAssemblies.Add (a.Location);
			CompilerResults results = abuilder.BuildAssembly (parameters);
			if (results.Errors.Count == 0) {
				BuildManager.CodeAssemblies.Add (results.CompiledAssembly);
				BuildManager.TopLevelAssemblies.Add (results.CompiledAssembly);
				HttpRuntime.WritePreservationFile (results.CompiledAssembly, name);
			} else {
				if (HttpContext.Current.IsCustomErrorEnabled)
					throw new HttpException ("An error occurred while initializing application.");
				throw new CompilationException (null, results.Errors, null);
			}
		}

		private string PhysicalToVirtual (string file)
		{
			return file.Replace (HttpRuntime.AppDomainAppPath, "/").Replace (Path.DirectorySeparatorChar, '/');
		}
		
		private BuildProvider GetBuildProviderFor (string file, BuildProviderCollection buildProviders)
		{
			if (file == null || file.Length == 0 || buildProviders == null || buildProviders.Count == 0)
				return null;

			BuildProvider ret = buildProviders.GetProviderForExtension (Path.GetExtension (file));
			if (ret != null && IsCorrectBuilderType (ret)) {
				ret.SetVirtualPath (PhysicalToVirtual (file));
				return ret;
			}
				
			return null;
		}

		private bool IsCorrectBuilderType (BuildProvider bp)
		{
			if (bp == null)
				return false;
			Type type;
			object[] attrs;

			type = bp.GetType ();
			attrs = type.GetCustomAttributes (true);
			if (attrs == null)
				return false;
			
			BuildProviderAppliesToAttribute bpAppliesTo;
			bool attributeFound = false;
			foreach (object attr in attrs) {
				bpAppliesTo = attr as BuildProviderAppliesToAttribute;
				if (bpAppliesTo == null)
					continue;
				attributeFound = true;
				if ((bpAppliesTo.AppliesTo & BuildProviderAppliesTo.All) == BuildProviderAppliesTo.All ||
				    (bpAppliesTo.AppliesTo & BuildProviderAppliesTo.Code) == BuildProviderAppliesTo.Code)
					return true;
			}

			if (attributeFound)
				return false;
			return true;
		}
		
	}
	
	internal class AppCodeCompiler
	{
		static private bool _alreadyCompiled;
		internal static string DefaultAppCodeAssemblyName;
		
		// A dictionary that contains an entry per an assembly that will
		// be produced by compiling App_Code. There's one main assembly
		// and an optional number of assemblies as defined by the
		// codeSubDirectories sub-element of the compilation element in
		// the system.web section of the app's config file.
		// Each entry's value is an AppCodeAssembly instance.
		//
		// Assemblies are named as follows:
		//
		//  1. main assembly: App_Code.{HASH}
		//  2. subdir assemblies: App_SubCode_{DirName}.{HASH}
		//
		// If any of the assemblies contains files that would be
		// compiled with different compilers, a System.Web.HttpException
		// is thrown.
		//
		// Files for which there is no explicit builder are ignored
		// silently
		//
		// Files for which exist BuildProviders but which have no
		// unambiguous language assigned to them (e.g. .wsdl files), are
		// built using the default website compiler.
		private List<AppCodeAssembly> assemblies;
		string _bindir = null;
		string providerTypeName = null;
		
		public AppCodeCompiler ()
		{
			assemblies = new List<AppCodeAssembly>();
		}

		bool ProcessAppCodeDir (string appCode, AppCodeAssembly defasm)
		{
			// First process the codeSubDirectories
			CompilationSection cs = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation");
			
			if (cs != null) {
				string aname;
				for (int i = 0; i < cs.CodeSubDirectories.Count; i++) {
					aname = String.Format ("App_SubCode_{0}", cs.CodeSubDirectories[i].DirectoryName);
					assemblies.Add (new AppCodeAssembly (
								aname,
								Path.Combine (appCode, cs.CodeSubDirectories[i].DirectoryName)));
				}
			}
			
			return CollectFiles (appCode, defasm);
		}

		CodeTypeReference GetProfilePropertyType (string type)
		{
			if (String.IsNullOrEmpty (type))
				throw new ArgumentException ("String size cannot be 0", "type");
			return new CodeTypeReference (type);
		}

		Type GetTypeFromBin (string typeName)
		{
			string bindir = BinDir;
			if (!Directory.Exists (bindir))
				return null;
			
			string [] binDlls = Directory.GetFiles (bindir, "*.dll");
			Type ret = null;
			foreach (string dll in binDlls) {
				try {
					Assembly asm = Assembly.LoadFrom (dll);
					ret = asm.GetType (typeName, false);
					if (ret != null)
						break;
				} catch (Exception) {
					continue;
				}
			}

			return ret;
		}

		string FindProviderTypeName (ProfileSection ps, string providerName)
		{
			if (ps.Providers == null || ps.Providers.Count == 0)
				return null;
			
			ProviderSettings pset = ps.Providers [providerName];
			if (pset == null)
				return null;
			return pset.Type;
		}
		
		void GetProfileProviderAttribute (ProfileSection ps, CodeAttributeDeclarationCollection collection,
						  string providerName)
		{
			if (String.IsNullOrEmpty (providerName))
				providerTypeName = FindProviderTypeName (ps, ps.DefaultProvider);
			else
				providerTypeName = FindProviderTypeName (ps, providerName);
			if (providerTypeName == null)
				throw new HttpException (String.Format ("Profile provider type not defined: {0}",
									providerName));
			
			collection.Add (
				new CodeAttributeDeclaration (
					"ProfileProvider",
					new CodeAttributeArgument (
						new CodePrimitiveExpression (providerTypeName)
					)
				)
			);
		}

		void GetProfileSettingsSerializeAsAttribute (ProfileSection ps, CodeAttributeDeclarationCollection collection,
							     SerializationMode mode)
		{
			string parameter = String.Format ("SettingsSerializeAs.{0}", mode.ToString ());
			collection.Add (
				new CodeAttributeDeclaration (
					"SettingsSerializeAs",
					new CodeAttributeArgument (
						new CodeSnippetExpression (parameter)
					)
				)
			);
					
		}

		void AddProfileClassGetProfileMethod (CodeTypeDeclaration profileClass)
		{
			CodeMethodReferenceExpression mref = new CodeMethodReferenceExpression (
				new CodeTypeReferenceExpression (typeof (System.Web.Profile.ProfileBase)),
				"Create");
			CodeMethodInvokeExpression minvoke = new CodeMethodInvokeExpression (
				mref,
				new CodeExpression[] { new CodeVariableReferenceExpression ("username") }
			);
			CodeCastExpression cast = new CodeCastExpression ();
			cast.TargetType = new CodeTypeReference ("ProfileCommon");
			cast.Expression = minvoke;
			
			CodeMethodReturnStatement ret = new CodeMethodReturnStatement ();
			ret.Expression = cast;
			
			CodeMemberMethod method = new CodeMemberMethod ();
			method.Name = "GetProfile";
			method.ReturnType = new CodeTypeReference ("ProfileCommon");
			method.Parameters.Add (new CodeParameterDeclarationExpression("System.String", "username"));
			method.Statements.Add (ret);
			method.Attributes = MemberAttributes.Public;
			
			profileClass.Members.Add (method);
		}
		
		void AddProfileClassProperty (ProfileSection ps, CodeTypeDeclaration profileClass, ProfilePropertySettings pset)
		{
			string name = pset.Name;
			if (String.IsNullOrEmpty (name))
				throw new HttpException ("Profile property 'Name' attribute cannot be null.");
			CodeMemberProperty property = new CodeMemberProperty ();
			string typeName = pset.Type;
			if (typeName == "string")
				typeName = "System.String";
			property.Name = name;
			property.Type = GetProfilePropertyType (typeName);
			property.Attributes = MemberAttributes.Public;
			
			CodeAttributeDeclarationCollection collection = new CodeAttributeDeclarationCollection();
			GetProfileProviderAttribute (ps, collection, pset.Provider);
			GetProfileSettingsSerializeAsAttribute (ps, collection, pset.SerializeAs);

			property.CustomAttributes = collection;
			CodeMethodReturnStatement ret = new CodeMethodReturnStatement ();
			CodeCastExpression cast = new CodeCastExpression ();
			ret.Expression = cast;

			CodeMethodReferenceExpression mref = new CodeMethodReferenceExpression (
				new CodeThisReferenceExpression (),
				"GetPropertyValue");
			CodeMethodInvokeExpression minvoke = new CodeMethodInvokeExpression (
				mref,
				new CodeExpression[] { new CodePrimitiveExpression (name) }
			);
			cast.TargetType = new CodeTypeReference (typeName);
			cast.Expression = minvoke;
			property.GetStatements.Add (ret);

			if (!pset.ReadOnly) {
				mref = new CodeMethodReferenceExpression (
					new CodeThisReferenceExpression (),
					"SetPropertyValue");
				minvoke = new CodeMethodInvokeExpression (
					mref,
					new CodeExpression[] { new CodePrimitiveExpression (name), new CodeSnippetExpression ("value") }
				);
				property.SetStatements.Add (minvoke);
			}
			
			
			profileClass.Members.Add (property);
		}

		void AddProfileClassGroupProperty (string groupName, string memberName, CodeTypeDeclaration profileClass)
		{			
			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = memberName;
			property.Type = new CodeTypeReference (groupName);
			property.Attributes = MemberAttributes.Public;

			CodeMethodReturnStatement ret = new CodeMethodReturnStatement ();
			CodeCastExpression cast = new CodeCastExpression ();
			ret.Expression = cast;

			CodeMethodReferenceExpression mref = new CodeMethodReferenceExpression (
				new CodeThisReferenceExpression (),
				"GetProfileGroup");
			CodeMethodInvokeExpression minvoke = new CodeMethodInvokeExpression (
				mref,
				new CodeExpression[] { new CodePrimitiveExpression (memberName) }
			);
			cast.TargetType = new CodeTypeReference (groupName);
			cast.Expression = minvoke;
			property.GetStatements.Add (ret);
			
			profileClass.Members.Add (property);
		}
		
		void BuildProfileClass (ProfileSection ps, string className, ProfilePropertySettingsCollection psc,
					CodeNamespace ns, string baseClass, bool baseIsGlobal,
					SortedList <string, string> groupProperties)
		{
			CodeTypeDeclaration profileClass = new CodeTypeDeclaration (className);
			CodeTypeReference cref = new CodeTypeReference (baseClass);
			if (baseIsGlobal)
				cref.Options |= CodeTypeReferenceOptions.GlobalReference;
			profileClass.BaseTypes.Add (cref);
			profileClass.TypeAttributes = TypeAttributes.Public;
			ns.Types.Add (profileClass);
			
			foreach (ProfilePropertySettings pset in psc)
				AddProfileClassProperty (ps, profileClass, pset);
			if (groupProperties != null && groupProperties.Count > 0)
				foreach (KeyValuePair <string, string> group in groupProperties)
					AddProfileClassGroupProperty (group.Key, group.Value, profileClass);
			AddProfileClassGetProfileMethod (profileClass);
		}

		string MakeGroupName (string name)
		{
			return String.Format ("ProfileGroup{0}", name);
		}
		
		// FIXME: there should be some validation of syntactic correctness of the member/class name
		// for the groups/properties. For now it's left to the compiler to report errors.
		//
		// CodeGenerator.IsValidLanguageIndependentIdentifier (id) - use that
		//
		bool ProcessCustomProfile (ProfileSection ps, AppCodeAssembly defasm)
		{
			CodeCompileUnit unit = new CodeCompileUnit ();
			CodeNamespace ns = new CodeNamespace (null);
			unit.Namespaces.Add (ns);
			defasm.AddUnit (unit);
			
			ns.Imports.Add (new CodeNamespaceImport ("System"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Configuration"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Web"));
			ns.Imports.Add (new CodeNamespaceImport ("System.Web.Profile"));
			
			RootProfilePropertySettingsCollection props = ps.PropertySettings;
			if (props == null)
				return true;

			SortedList<string, string> groupProperties = new SortedList<string, string> ();
			string groupName;
			foreach (ProfileGroupSettings pgs in props.GroupSettings) {
				groupName = MakeGroupName (pgs.Name);
				groupProperties.Add (groupName, pgs.Name);
				BuildProfileClass (ps, groupName, pgs.PropertySettings, ns,
						   "System.Web.Profile.ProfileGroupBase", true, null);
			}
			
			string baseType = ps.Inherits;
			bool baseIsGlobal = false;
			if (String.IsNullOrEmpty (baseType)) {
				baseType = "System.Web.Profile.ProfileBase";
				baseIsGlobal = true;
			}
			
			BuildProfileClass (ps, "ProfileCommon", props, ns, baseType, baseIsGlobal, groupProperties);
			return true;
		}

//		void PutCustomProfileInContext (HttpContext context, string assemblyName)
//		{
//			Type type = Type.GetType (String.Format ("ProfileCommon, {0}",
//								 Path.GetFileNameWithoutExtension (assemblyName)));
//			ProfileBase pb = Activator.CreateInstance (type) as ProfileBase;
//			if (pb != null)
//				context.Profile = pb;
//		}

		public static bool HaveCustomProfile (ProfileSection ps)
		{
			if (ps == null || !ps.Enabled)
				return false;
			if (!String.IsNullOrEmpty (ps.Inherits) || (ps.PropertySettings != null && ps.PropertySettings.Count > 0))
				return true;

			return false;
		}
		
		public void Compile ()
		{
			if (_alreadyCompiled)
				return;
			
			string appCode = Path.Combine (HttpRuntime.AppDomainAppPath, "App_Code");
			ProfileSection ps = WebConfigurationManager.GetSection ("system.web/profile") as ProfileSection;
			bool haveAppCodeDir = Directory.Exists (appCode);
			bool haveCustomProfile = HaveCustomProfile (ps);
			
			if (!haveAppCodeDir && !haveCustomProfile)
				return;

			AppCodeAssembly defasm = new AppCodeAssembly ("App_Code", appCode);
			assemblies.Add (defasm);

			bool haveCode = false;
			if (haveAppCodeDir)
				haveCode = ProcessAppCodeDir (appCode, defasm);
			if (haveCustomProfile)
				if (ProcessCustomProfile (ps, defasm))
					haveCode = true;

			if (!haveCode)
				return;

			HttpRuntime.EnableAssemblyMapping (true);
			string bindir = BinDir;
			string[] binAssemblies = null;
			if (Directory.Exists (bindir))
				binAssemblies = Directory.GetFiles (bindir, "*.dll");
			foreach (AppCodeAssembly aca in assemblies)
				aca.Build (binAssemblies);
			_alreadyCompiled = true;
			DefaultAppCodeAssemblyName = Path.GetFileNameWithoutExtension (defasm.OutputAssemblyName);
			
			if (haveCustomProfile && providerTypeName != null) {
				if (Type.GetType (providerTypeName, false) == null) {
					foreach (Assembly asm in BuildManager.TopLevelAssemblies) {
						if (asm == null)
							continue;
						
						if (asm.GetType (providerTypeName, false) != null)
							return;
					}
				} else
					return;

				if (GetTypeFromBin (providerTypeName) == null)
					throw new HttpException (String.Format ("Profile provider type not found: {0}",
										providerTypeName));
			}
		}

		private bool CollectFiles (string dir, AppCodeAssembly aca)
		{
			bool haveFiles = false;
			
			AppCodeAssembly curaca = aca;
			foreach (string f in Directory.GetFiles (dir)) {
				aca.AddFile (f);
				haveFiles = true;
			}
			
			foreach (string d in Directory.GetDirectories (dir)) {
				foreach (AppCodeAssembly a in assemblies)
					if (a.SourcePath == d) {
						curaca = a;
						break;
					}
				if (CollectFiles (d, curaca))
					haveFiles = true;
				curaca = aca;
			}
			return haveFiles;
		}

		private string BinDir {
			get {
				if (_bindir != null)
					return _bindir;
				AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
				_bindir = Path.Combine (setup.ApplicationBase, setup.PrivateBinPath);
				return _bindir;
			}
		}
	}
}
#endif
