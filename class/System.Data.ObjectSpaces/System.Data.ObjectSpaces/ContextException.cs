//
// System.Data.ObjectSpaces.ContextException.cs : The exception thrown when an ObjectContext encounters an error
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
//

#if NET_1_2

using System.Runtime.Serialization;

namespace System.Data.ObjectSpaces
{
        public class ContextException : ObjectException
        {
                [MonoTODO]        
                protected ContextException () {}
                
                [MonoTODO]
                protected ContextException (SerializationInfo info, StreamingContext context) {}
                                
                [MonoTODO]
                protected ContextException (string message) {}

                [MonoTODO]
                protected ContextException (string message, Exception innerException) {}
        }
}

#endif