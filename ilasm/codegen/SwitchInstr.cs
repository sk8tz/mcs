//
// Mono.ILASM.SwitchInstr
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;
using System.Collections;

namespace Mono.ILASM {

        public class SwitchInstr : IInstr {

                private ArrayList label_list;

                public SwitchInstr (ArrayList label_list, Location loc)
			: base (loc)
                {
                        this.label_list = label_list;
                }

                public override void Emit (CodeGen code_gen, MethodDef meth,
					   PEAPI.CILInstructions cil)
                {
                        int count = 0;
                        PEAPI.CILLabel[] label_array;

                        if (label_list != null) {
                                label_array = new PEAPI.CILLabel[label_list.Count];
                                foreach (object lab in label_list) {
                                        if (lab is string) {
                                                label_array[count++] = meth.GetLabelDef ((string) lab);
                                        } else {                                                
                                                throw new NotImplementedException ("offsets in switch statements.");
                                        }
                                }
                        } else {
                                label_array = new PEAPI.CILLabel [0];
                        }

                        cil.Switch (label_array);
                }
        }

}

