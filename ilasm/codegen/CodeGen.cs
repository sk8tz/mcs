//
// Mono.ILASM.CodeGen.cs
//
// Author(s):
//  Sergey Chaban (serge@wildwestsoftware.com)
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) Sergey Chaban
// (C) 2003 Jackson Harper, All rights reserved
//

using PEAPI;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.ILASM {

        public class CodeGen {

                private PEFile pefile;
                private string assembly_name;
                private Report report;
                private ExternAssembly current_assemblyref;
                private string current_namespace;
                private TypeDef current_typedef;
                private MethodDef current_methoddef;
                private Stack typedef_stack;

                private byte [] assembly_public_key;
                private int assembly_major_version;
                private int assembly_minor_version;
                private int assembly_build_version;
                private int assembly_revision_version;
                private string assembly_locale;
                private int assembly_hash_algorithm;
                private ArrayList assembly_custom_attributes;
                        
                private TypeManager type_manager;
                private ExternTable extern_table;
                private Hashtable global_field_table;
                private Hashtable global_method_table;
                private ArrayList data_list;

                private ArrayList defcont_list;

                private int sub_system;
                private int cor_flags;
                private long image_base;

                private string output_file;
                private bool is_dll;
                private bool is_assembly;

                public CodeGen (string output_file, bool is_dll, bool is_assembly, Report report)
                {
                        this.output_file = output_file;
                        this.is_dll = is_dll;
                        this.is_assembly = is_assembly;
                        this.report = report;

                        type_manager = new TypeManager (this);
                        extern_table = new ExternTable ();
                        typedef_stack = new Stack ();
                        global_field_table = new Hashtable ();
                        global_method_table = new Hashtable ();

                        data_list = new ArrayList ();

                        defcont_list = new ArrayList ();

                        sub_system = -1;
                        cor_flags = -1;
                        image_base = -1;
                }

                public PEFile PEFile {
                        get { return pefile; }
                }

                public Report Report {
                        get { return report; }
                }

                public string CurrentNameSpace {
                        get { return current_namespace; }
                        set { current_namespace = value; }
                }

                public TypeDef CurrentTypeDef {
                        get { return current_typedef; }
                }

                public MethodDef CurrentMethodDef {
                        get { return current_methoddef; }
                }

                public ExternAssembly CurrentAssemblyRef {
                        get { return current_assemblyref; }
                }

                public ExternTable ExternTable {
                        get { return extern_table; }
                }

                public TypeManager TypeManager {
                        get { return type_manager; }
                }

                public void SetSubSystem (int sub_system)
                {
                        this.sub_system = sub_system;
                }

                public void SetCorFlags (int cor_flags)
                {
                        this.cor_flags = cor_flags;
                }

                public void SetImageBase (long image_base)
                {
                        this.image_base = image_base;
                }

                public void SetAssemblyName (string name)
                {
                        assembly_name = name;
                }

                public bool IsThisAssembly (string name)
                {
                        return (name == assembly_name);
                }

                public void BeginTypeDef (TypeAttr attr, string name, IClassRef parent,
                                ArrayList impl_list, Location location)
                {
                        TypeDef outer = null;
                        string cache_name = CacheName (name);

                        if (typedef_stack.Count > 0) {
                                outer = (TypeDef) typedef_stack.Peek ();
                                cache_name = CacheName (outer.Name + '/' + name);
                        }

                        TypeDef typedef = type_manager[cache_name];

                        if (typedef != null) {
                                // Class head is allready defined, we are just reopening the class
                                current_typedef = typedef;
                                typedef_stack.Push (current_typedef);
                                return;
                        }

                        typedef = new TypeDef (attr, current_namespace,
                                        name, parent, impl_list, location);

                        if (outer != null)
                                typedef.OuterType = outer;

                        type_manager[cache_name] = typedef;
                        current_typedef = typedef;
                        typedef_stack.Push (typedef);
                }

                public void AddFieldDef (FieldDef fielddef)
                {
                        if (current_typedef != null) {
                                current_typedef.AddFieldDef (fielddef);
                        } else {
                                global_field_table.Add (fielddef.Name,
                                                fielddef);
                        }
                }

                public void AddDataDef (DataDef datadef)
                {
                        data_list.Add (datadef);
                }

                public PEAPI.DataConstant GetDataConst (string name)
                {
                        foreach (DataDef def in data_list) {
                                if (def.Name == name)
                                        return (DataConstant) def.PeapiConstant;
                        }
                        return null;

                }

                public void BeginMethodDef (MethodDef methoddef)
                {
                        if (current_typedef != null) {
                                current_typedef.AddMethodDef (methoddef);
                        } else {
                                global_method_table.Add (methoddef.Signature,
                                                methoddef);
                        }

                        current_methoddef = methoddef;
                }

                public void EndMethodDef ()
                {
                        current_methoddef = null;
                }

                public void EndTypeDef ()
                {
                        typedef_stack.Pop ();

                        if (typedef_stack.Count > 0)
                                current_typedef = (TypeDef) typedef_stack.Peek ();
                        else
                                current_typedef = null;

                }

                public void BeginAssemblyRef (string name, AssemblyName asmb_name)
                {
                        current_assemblyref = ExternTable.AddAssembly (name, asmb_name);
                }

                public void EndAssemblyRef ()
                {
                        current_assemblyref = null;
                }

                public void AddToDefineContentsList (TypeDef typedef)
                {
                        defcont_list.Add (typedef);
                }

                public void SetAssemblyPublicKey (byte [] public_key)
                {
                        assembly_public_key = public_key;
                }

                public void SetAssemblyVersion (int major, int minor, int build, int revision)
                {
                        assembly_major_version = major;
                        assembly_minor_version = minor;
                        assembly_build_version = build;
                        assembly_revision_version = revision;
                }

                public void SetAssemblyLocale (string locale)
                {
                        assembly_locale = locale;
                }

                public void SetAssemblyHashAlgorithm (int algorithm)
                {
                        assembly_hash_algorithm = algorithm;
                }

                public void AddAssemblyCustomAttribute (CustomAttr attribute)
                {
                        if (assembly_custom_attributes == null)
                                assembly_custom_attributes = new ArrayList ();
                        assembly_custom_attributes.Add (attribute);
                }

                public void Write ()
                {
                        FileStream out_stream = null;

                        try {
                                out_stream = new FileStream (output_file, FileMode.Create, FileAccess.Write);
                                pefile = new PEFile (assembly_name, is_dll, is_assembly, out_stream);
                                extern_table.Resolve (this);
                                type_manager.DefineAll ();

                                foreach (FieldDef fielddef in global_field_table.Values) {
                                        fielddef.Define (this);
                                }

                                foreach (MethodDef methoddef in global_method_table.Values) {
                                        methoddef.Define (this);
                                }

                                foreach (TypeDef typedef in defcont_list) {
                                        typedef.DefineContents (this);
                                }

                                if (sub_system != -1)
                                        pefile.SetSubSystem ((PEAPI.SubSystem) sub_system);
                                if (cor_flags != -1)
                                        pefile.SetCorFlags (cor_flags);

                                PEAPI.Assembly asmb = pefile.GetThisAssembly ();
                                asmb.AddAssemblyInfo(assembly_major_version,
                                                assembly_minor_version, assembly_build_version,
                                                assembly_revision_version, assembly_public_key,
                                                (uint) assembly_hash_algorithm, assembly_locale);

                                pefile.WritePEFile ();
                        } catch {
                                throw;
                        } finally {
                                if (out_stream != null)
                                        out_stream.Close ();
                        }
                }

                public PEAPI.Method ResolveMethod (string signature)
                {
                        MethodDef methoddef = (MethodDef) global_method_table[signature];

                        return methoddef.Resolve (this);
                }

                public PEAPI.Method ResolveVarargMethod (string signature,
                                CodeGen code_gen, PEAPI.Type[] opt)
                {
                        MethodDef methoddef = (MethodDef) global_method_table[signature];
                        methoddef.Resolve (code_gen);

                        return methoddef.GetVarargSig (opt);
                }

                public PEAPI.Field ResolveField (string name)
                {
                        FieldDef fielddef = (FieldDef) global_field_table[name];

                        return fielddef.Resolve (this);
                }

                private string CacheName (string name)
                {
                        if (current_namespace == null ||
                                        current_namespace == String.Empty)
                                return name;

                        return current_namespace + "." + name;
                }
        }

}

