//
// Mono.ILASM.ExternTypeRef
//
// Author(s):
//  Jackson Harper (Jackson@LatitudeGeo.com)
//
// (C) 2003 Jackson Harper, All rights reserved
//


using System;

namespace Mono.ILASM {

        /// <summary>
        /// A reference to a type in another assembly
        /// </summary>
        public class ExternTypeRef : PeapiTypeRef {

                public ExternTypeRef (PEAPI.ClassRef extern_type,
                                string full_name) : base (extern_type, full_name)
                {

                }
        }

}

