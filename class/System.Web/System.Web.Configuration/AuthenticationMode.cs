/* System.Web.Configuration
 * Authors:
 *   Leen Toelen (toelen@hotmail.com)
 *  Copyright (C) 2001 Leen Toelen
*/

namespace System.Web.Configuration {

	/// <summary>
	/// Defines the AuthenticationMode for a Web Application.
	/// </summary>
	public enum AuthenticationMode{
		Forms, 
		None,
		Passport, 
		Windows
	}

} //namespace System.Web.Configuration
