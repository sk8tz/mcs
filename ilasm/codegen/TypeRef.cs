//
// Mono.ILASM.TypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//

using System;
using System.Collections;

namespace Mono.ILASM {

        /// <summary>
        /// Reference to a type in the module being compiled.
        /// </summary>
        public class TypeRef : ModifiableType, IClassRef {

                private enum ConversionMethod {
                        MakeArray,
                        MakeBoundArray,
                        MakeManagedPointer,
                        MakeUnmanagedPointer,
                        MakeCustomModified
                }

                private Location location;
                private string full_name;
                private PEAPI.Type type;

                private bool is_resolved;

                public static readonly TypeRef Ellipsis = new TypeRef ("ELLIPSIS", null);
                public static readonly TypeRef Any = new TypeRef ("any", null);

                public TypeRef (string full_name, Location location)
                {
                        this.full_name = full_name;
                        this.location = location;
                        is_resolved = false;
                }

                public string FullName {
                        get { return full_name; }
                }

                public PEAPI.Type PeapiType {
                        get { return type; }
                }

                public PEAPI.Class PeapiClass {
                        get { return type as PEAPI.Class; }
                }

                public bool IsResolved {
                        get { return is_resolved; }
                }

                public  IMethodRef GetMethodRef (ITypeRef ret_type,
                        PEAPI.CallConv call_conv, string name, ITypeRef[] param)
                {
                        return new MethodRef (this, call_conv, ret_type, name, param);
                }

                public IFieldRef GetFieldRef (ITypeRef ret_type, string name)
                {
                        return new FieldRef (this, ret_type, name);
                }

                public void Resolve (CodeGen code_gen)
                {
                        if (is_resolved)
                                return;

                        PEAPI.Type base_type;

                        base_type = code_gen.TypeManager.GetPeapiType (full_name);
                        type = Modify (code_gen, base_type, ref full_name);

                        is_resolved = true;
                }

                public IClassRef AsClassRef (CodeGen code_gen)
                {
                        return this;
                }

                public override string ToString ()
                {
                        return FullName;
                }

        }

}

