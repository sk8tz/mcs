/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzm�n �lvarez
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;

namespace Mono.Security.Protocol.Tls.Alerts
{
	public delegate void TlsWarningAlertEventHandler(object sender, TlsWarningAlertEventArgs e);

	public sealed class TlsWarningAlertEventArgs
	{
		#region FIELDS

		private TlsAlertLevel		level;
		private TlsAlertDescription description;
		private string				message;

		#endregion

		#region PROPERTIES

		public TlsAlertLevel Level
		{
			get { return level; }
		}

		public TlsAlertDescription Description
		{
			get { return description; }
		}

		public string Message
		{
			get { return message; }
		}

		#endregion

		#region CONSTRUCTORS
		
		internal TlsWarningAlertEventArgs(TlsAlertLevel	level, TlsAlertDescription description)
		{
			this.level			= level;
			this.description	= description;
			this.message		= TlsAlert.GetAlertMessage(description);
		}

		#endregion
	}
}
