//
// Mono.ILASM.PrimitiveTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        /// <summary>
        /// Reference to a primitive type, ie string, object, char
        /// </summary>
        public class PrimitiveTypeRef : PeapiTypeRef {

                public PrimitiveTypeRef (PEAPI.PrimitiveType prim_type,
                                string full_name) : base (prim_type, full_name)
                {

                }
        }

}

