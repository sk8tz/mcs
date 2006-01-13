//
// Mono.ILASM.GlobalMethodRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class GlobalMethodRef : IMethodRef {

                private BaseTypeRef ret_type;
                private string name;
                private BaseTypeRef[] param;
                private PEAPI.CallConv call_conv;

                private PEAPI.Method peapi_method;
		private bool is_resolved;
		private int gen_param_count;

                public GlobalMethodRef (BaseTypeRef ret_type, PEAPI.CallConv call_conv,
                                string name, BaseTypeRef[] param, int gen_param_count)
                {
                        this.ret_type = ret_type;
                        this.call_conv = call_conv;
                        this.name = name;
                        this.param = param;
			this.gen_param_count = gen_param_count;
			if (gen_param_count > 0)
				CallConv |= PEAPI.CallConv.Generic;

			is_resolved = false;
                }

                public PEAPI.Method PeapiMethod {
                        get { return peapi_method; }
                }

		public PEAPI.CallConv CallConv {
			get { return call_conv; }
			set { call_conv = value; }
		}

		public BaseTypeRef Owner {
			get { return null; }
		}

                public void Resolve (CodeGen code_gen)
                {
			if (is_resolved)
				return;

                        string sig;

                        if ((call_conv & PEAPI.CallConv.Vararg) == 0) {
                                sig = MethodDef.CreateSignature (ret_type, name, param, gen_param_count);
                                peapi_method = code_gen.ResolveMethod (sig);
                        } else {
                                ArrayList opt_list = new ArrayList ();
                                bool in_opt = false;
                                foreach (BaseTypeRef type in param) {
                                        if (type is SentinelTypeRef) {
                                                in_opt = true;
                                        } else if (in_opt) {
                                                type.Resolve (code_gen);
                                                opt_list.Add (type.PeapiType);
                                        }
                                }
                                sig = MethodDef.CreateVarargSignature (ret_type, name, param);
                                peapi_method = code_gen.ResolveVarargMethod (sig, code_gen,
                                                (PEAPI.Type[]) opt_list.ToArray (typeof (PEAPI.Type)));
                        }

                        peapi_method.AddCallConv (call_conv);
			
			is_resolved = true;
                }

        }

}

