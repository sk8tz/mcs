//
// System.Web.Mail.MailUtil.cs
//
// Author(s):
//   Per Arneng <pt99par@student.bth.se>
//
//
using System;
using System.Text;

namespace System.Web.Mail {
    
    // This class contains some utillity functions
    // that doesnt fit in other classes and to keep
    // high cohesion on the other classes.
    internal class MailUtil {
	
	// determines if a string needs to
	// be encoded for transfering over
	// the smtp protocol without risking
	// that it would be changed.
	public static bool NeedEncoding( string str ) {
	    bool needEnc = false;
	    
	    foreach( char chr in str ) {
		int ch = (int)chr;
		if( ! ( ( ch > 33 ) && ( ch < 61 ) ) ||
		    ( ( ch > 61 ) && ( ch < 127 ) ) ) needEnc = true;
	    }

	    return needEnc;
	}

	// Encodes a string to base4
	public static string Base64Encode( string str ) {
	    return Convert.ToBase64String( Encoding.Default.GetBytes( str ) );
	}
	
    }


}
