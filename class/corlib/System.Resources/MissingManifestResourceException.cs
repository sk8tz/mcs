//
// System.Resources.MissingManifestResourceException.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.		http://www.ximian.com
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Resources
{

	   public class MissingManifestResourceException: Exception
	   {
			 private string param;

			 // Constructors
			 public MissingManifestResourceException ()
				    : base (Locale.GetText ("The assembly does not contain the resources for the required culture."))
				    {
				    }
									
			 public MissingManifestResourceException (string message)
				    :base (message)
				    {
				    }

			 public MissingManifestResourceException (SerializationInfo info, StreamingContext context)
				    :base (info, context)
				    {
				    }

			 public MissingManifestResourceException (String message, Exception e)
				    :base (message, e)
				    {
				    }
	   }
}
