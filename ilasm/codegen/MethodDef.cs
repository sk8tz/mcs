//
// Mono.ILASM.MethodDef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Text;
using System.Collections;


namespace Mono.ILASM {

        public class MethodDef {

                protected class LabelInfo : IComparable {

                        public readonly string Name;
                        public readonly int Pos;
                        public PEAPI.CILLabel Label;

                        public LabelInfo (string name, int pos)
                        {
                                Name = name;
                                Pos = pos;
                                Label = null;
                        }

                        public void Define (PEAPI.CILLabel label)
                        {
                                Label = label;
                        }

                        public int CompareTo (object obj)
                        {
                                LabelInfo other = obj as LabelInfo;

                                if(other != null)
                                        return Pos.CompareTo(other.Pos);

                                throw new ArgumentException ("object is not a LabelInfo");
                        }
                }

                private PEAPI.MethAttr meth_attr;
                private PEAPI.ImplAttr impl_attr;
                private string name;
                private string signature;
                private ITypeRef ret_type;
                private ArrayList param_list;
                private ArrayList inst_list;
                private Hashtable label_table;
                private PEAPI.MethodDef methoddef;
                private bool entry_point;
                private bool is_resolved;
                private bool is_defined;
                private ArrayList local_list;
                private Hashtable named_local_table;
                private bool init_locals;
                private int max_stack;

                public MethodDef (PEAPI.MethAttr meth_attr, PEAPI.ImplAttr impl_attr,
                                string name, ITypeRef ret_type, ArrayList param_list)
                {
                        this.meth_attr = meth_attr;
                        this.impl_attr = impl_attr;
                        this.name = name;
                        this.ret_type = ret_type;
                        this.param_list = param_list;

                        inst_list = new ArrayList ();
                        label_table = new Hashtable ();
                        local_list = new ArrayList ();
                        named_local_table = new Hashtable ();
                        entry_point = false;
                        init_locals = false;
                        max_stack = -1;

                        is_defined = false;
                        is_resolved = false;
                        CreateSignature ();
                }

                public string Name {
                        get { return name; }
                }

                public string Signature {
                        get { return signature; }
                }

                public PEAPI.MethodDef PeapiMethodDef {
                        get { return methoddef; }
                }

                public void AddLocals (ArrayList local_list)
                {
                        int slot_pos = this.local_list.Count;

                        foreach (Local local in local_list) {
                                if (local.Slot == -1) {
                                        local.Slot = slot_pos;
                                }
                                slot_pos++;
                                if (local.Name == null)
                                        continue;
                                named_local_table.Add (local.Name, local);
                        }

                        this.local_list.AddRange (local_list);
                }

                public Local GetNamedLocal (string name)
                {
                        return (Local) named_local_table[name];
                }

                public void InitLocals ()
                {
                        init_locals = true;
                }

                public void EntryPoint ()
                {
                        entry_point = true;
                }

                public void SetMaxStack (int max_stack)
                {
                        this.max_stack = max_stack;
                }

                public PEAPI.MethodDef Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return methoddef;

                        PEAPI.Param[] param_array = new PEAPI.Param[param_list.Count];
                        int count = 0;
                        ret_type.Resolve (code_gen);

                        foreach (ParamDef paramdef in param_list) {
                                paramdef.Define (code_gen);
                                param_array[count++] = paramdef.PeapiParam;
                        }

                        methoddef = code_gen.PEFile.AddMethod (meth_attr, impl_attr,
                                        name, ret_type.PeapiType, param_array);

                        is_resolved = true;

                        return methoddef;
                }

                public PEAPI.MethodDef Resolve (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_resolved)
                                return methoddef;

                        PEAPI.Param[] param_array = new PEAPI.Param[param_list.Count];
                        int count = 0;
                        ret_type.Resolve (code_gen);

                        foreach (ParamDef paramdef in param_list) {
                                paramdef.Define (code_gen);
                                param_array[count++] = paramdef.PeapiParam;
                        }

                        methoddef = classdef.AddMethod (meth_attr, impl_attr,
                                        name, ret_type.PeapiType, param_array);

                        is_resolved = true;

                        return methoddef;
                }

                /// <summary>
                ///  Define a global method
                /// </summary>
                public void Define (CodeGen code_gen)
                {
                        if (is_defined)
                                return;

                        Resolve (code_gen);

                        WriteCode (code_gen, methoddef);

                        is_defined = true;
                }

                /// <summary>
                ///  Define a member method
                /// </summary>
                public void Define (CodeGen code_gen, PEAPI.ClassDef classdef)
                {
                        if (is_defined)
                                return;

                        Resolve (code_gen, classdef);

                        WriteCode (code_gen, methoddef);

                        is_defined = true;
                }

                public void AddInstr (IInstr instr)
                {
                        inst_list.Add (instr);
                }

                protected void WriteCode (CodeGen code_gen, PEAPI.MethodDef methoddef)
                {
                        if (entry_point)
                                methoddef.DeclareEntryPoint ();

                        if (local_list != null) {
                                PEAPI.Local[] local_array = new PEAPI.Local[local_list.Count];
                                int i = 0;

                                foreach (Local local in local_list)
                                        local_array[local.Slot]  = local.GetPeapiLocal (code_gen);

                                methoddef.AddLocals (local_array, init_locals);
                        }

                        if (max_stack >= 0)
                                methoddef.SetMaxStack (max_stack);

                        if (inst_list.Count < 1)
                                return;

                        PEAPI.CILInstructions cil = methoddef.CreateCodeBuffer ();
                        /// Create all the labels
                        /// TODO: Most labels don't actually need to be created so we could
                        /// probably only create the ones that need to be
                        LabelInfo[] label_info = new LabelInfo[label_table.Count];
                        label_table.Values.CopyTo (label_info, 0);
                        Array.Sort (label_info);

                        foreach (LabelInfo label in label_info)
                                label.Define (cil.NewLabel ());

                        int label_pos = 0;
                        int next_label_pos = (label_info.Length > 0 ? label_info[0].Pos : -1);

                        for (int i=0; i<inst_list.Count; i++) {
                                IInstr instr = (IInstr) inst_list[i];
                                if (next_label_pos == i) {
                                        cil.CodeLabel (label_info[label_pos].Label);
                                        if (++label_pos < label_info.Length)
                                                next_label_pos = label_info[label_pos].Pos;
                                        else
                                                next_label_pos = -1;
                                }
                                instr.Emit (code_gen, cil);
                        }

                }

                public void AddLabel (string name)
                {
                        LabelInfo label_info = new LabelInfo (name, inst_list.Count);

                        label_table.Add (name, label_info);
                }

                public PEAPI.CILLabel GetLabelDef (string name)
                {
                        LabelInfo label_info = (LabelInfo) label_table[name];

                        return label_info.Label;
                }

                private void CreateSignature ()
                {
                        signature = CreateSignature (name, param_list);
                }

                public static string CreateSignature (string name, ICollection param_list)
                {
                        StringBuilder builder = new StringBuilder ();

                        builder.Append (name);
                        builder.Append ('(');

                        bool first = true;
                        foreach (ParamDef paramdef in param_list) {
                                if (!first)
                                        builder.Append (',');
                                builder.Append (paramdef.TypeName);
                        }
                        builder.Append (')');


                        return builder.ToString ();
                }
        }

}

