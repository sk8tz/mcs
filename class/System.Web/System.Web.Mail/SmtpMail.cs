//
// System.Web.Mail.SmtpMail.cs
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Per Arneng (pt99par@student.bth.se) (SmtpMail.Send)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Reflection;

namespace System.Web.Mail
{
	/// <remarks>
	/// </remarks>
	public class SmtpMail
	{
		private static string smtpServer = "localhost";
		
		// Constructor		
		private SmtpMail ()
		{
			/* empty */
		}		

		// Properties
		public static string SmtpServer {
			get { return smtpServer; } 
			set { smtpServer = value; }
		}
		
		
		public static void Send (MailMessage message) 
		{
		   		   		    
		    try {
			
			// wrap the MailMessage in a MailMessage wrapper for easier
			// access to properties and to add some functionality
			MailMessageWrapper messageWrapper = new MailMessageWrapper( message );
			
#if TARGET_JVM
			string currentSmtpServer = smtpServer;
			if (currentSmtpServer == "localhost")
			{
				java.net.InetAddress address = java.net.InetAddress.getLocalHost();
				currentSmtpServer = address.getHostAddress();
			}
			SmtpClient smtp = new SmtpClient (currentSmtpServer);
#else
			SmtpClient smtp = new SmtpClient (smtpServer);
#endif
			
			smtp.Send (messageWrapper);
		       
			smtp.Close ();
		    
		    } catch (SmtpException ex) {
			// LAMESPEC:
			// .NET sdk throws HttpException
			// for some reason so to be compatible
			// we have to do it to :(
			throw new HttpException (ex.Message, ex);
		    
		    } catch (IOException ex) {
			
			throw new HttpException (ex.Message, ex);
			
		    } catch (FormatException ex) {
			
			throw new HttpException (ex.Message, ex);
		    
		    } catch (SocketException ex) {
			
			throw new HttpException (ex.Message, ex);
			
		    }
		    
		}
		
		public static void Send (string from, string to, string subject, string messageText) 
		{
			MailMessage message = new MailMessage ();
			message.From = from;
			message.To = to;
			message.Subject = subject;
			message.Body = messageText;
			Send (message);
		}
	
	}
	
} //namespace System.Web.Mail
