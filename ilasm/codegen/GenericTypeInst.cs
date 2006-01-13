//
// Mono.ILASM.GenericTypeInst
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//  Ankit Jain (JAnkit@novell.com)
//
// (C) 2003 Latitude Geographics Group, All rights reserved
// (C) 2005 Novell, Inc (http://www.novell.com)
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class GenericTypeInst : BaseGenericTypeRef {

                private BaseClassRef class_ref;
                private PEAPI.GenericTypeInst p_gen_inst;
                private bool is_valuetypeinst;
                private GenericArguments gen_args;
                private bool is_added; /* Added to PEFile (to TypeSpec table) ? */
                private static Hashtable method_table = new Hashtable ();

                public GenericTypeInst (BaseClassRef class_ref, GenericArguments gen_args, bool is_valuetypeinst)
                        : this (class_ref, gen_args, is_valuetypeinst, null, null)
                {
                }

                public GenericTypeInst (BaseClassRef class_ref, GenericArguments gen_args, bool is_valuetypeinst,
                                string sig_mod, ArrayList conv_list)
                        : base ("", is_valuetypeinst, conv_list, sig_mod)
                {
                        if (class_ref is GenericTypeInst)
                                throw new ArgumentException (String.Format ("Cannot create nested GenericInst, '{0}' '{1}'", class_ref.FullName, gen_args.ToString ()));

                        this.class_ref = class_ref;
                        this.gen_args = gen_args;
                        is_added = false;
                }

                public override string FullName {
                        get { return class_ref.FullName + gen_args.ToString () + SigMod; }
                }

                public override BaseClassRef Clone ()
                {
                        //Clone'd instance shares the class_ref and gen_args,
                        //as its basically used to create modified types (arrays etc)
                        return new GenericTypeInst (class_ref, gen_args, is_valuetypeinst, sig_mod, 
                                        (ArrayList) ConversionList.Clone () );
                }

                public override void MakeValueClass ()
                {
                        class_ref.MakeValueClass ();
                }

                public override void ResolveNoTypeSpec (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        class_ref.Resolve (code_gen);
                        p_gen_inst = (PEAPI.GenericTypeInst) class_ref.ResolveInstance (code_gen, gen_args);

                        type = Modify (code_gen, p_gen_inst);

                        is_resolved = true;
                }

                public override void Resolve (CodeGen code_gen)
                {
                        ResolveNoTypeSpec (code_gen);
                        if (is_added)
                                return;

                        code_gen.PEFile.AddGenericClass ((PEAPI.GenericTypeInst) p_gen_inst);
                        is_added = true;
                }

                public override void Resolve (GenericParameters type_gen_params, GenericParameters method_gen_params)
                {
                        gen_args.Resolve (type_gen_params, method_gen_params);
                }

                public override IMethodRef GetMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string meth_name, BaseTypeRef[] param, int gen_param_count)
                {
                        string key = FullName + MethodDef.CreateSignature (ret_type, meth_name, param, gen_param_count);
                        TypeSpecMethodRef mr = method_table [key] as TypeSpecMethodRef;
                        if (mr == null) {         
                                mr = new TypeSpecMethodRef (this, call_conv, ret_type, meth_name, param, gen_param_count);
                                method_table [key] = mr;
                        }

                        return mr;
                }

                public override IFieldRef GetFieldRef (BaseTypeRef ret_type, string field_name)
                {
                        return new TypeSpecFieldRef (this, ret_type, field_name);
                }
        }
}

