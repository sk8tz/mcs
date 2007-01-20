//
// MailMessageTest.cs - NUnit Test Cases for System.Net.MailAddress.MailMessage
//
// Authors:
//   John Luke (john.luke@gmail.com)
//
// (C) 2005, 2006 John Luke
//
#if NET_2_0
using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Net.Mail;

namespace MonoTests.System.Net.Mail
{
	[TestFixture]
	public class MailMessageTest
	{
		MailMessage msg;
		MailMessage msg2;
		
		[SetUp]
		public void GetReady ()
		{
			msg = new MailMessage ("from@example.com", "to@example.com");
			msg.Subject = "the subject";
			msg.Body = "hello";
			//msg.AlternateViews.Add (AlternateView.CreateAlternateViewFromString ("<html><body>hello</body></html>", "text/html"));
			//Attachment a = Attachment.CreateAttachmentFromString ("blah blah", "text/plain");
			//msg.Attachments.Add (a);

			msg2 = new MailMessage ("from@example.com", "r1@t1.com, r2@t1.com");
		}

		[Test]
		public void TestRecipients ()
		{
			Assert.AreEqual (msg2.To.Count,  2);
			Assert.AreEqual (msg2.To[0].Address, "r1@t1.com");
			Assert.AreEqual (msg2.To[1].Address, "r2@t1.com");
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullCtor1 ()
		{
			new MailMessage (null, "to@example.com");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullCtor2 ()
		{
			new MailMessage (null, new MailAddress ("to@example.com"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullCtor3 ()
		{
			new MailMessage ("from@example.com", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgumentNullCtor4 ()
		{
			new MailMessage (new MailAddress ("from@example.com"), null);
		}

		/*[Test]
		public void AlternateView ()
		{
			Assert.AreEqual (msg.AlternateViews.Count, 1);
			AlternateView av = msg.AlternateViews[0];
			// test that the type is ok, etc.
		}*/

		/*[Test]
		public void Attachment ()
		{
			Assert.AreEqual (msg.Attachments.Count, 1);
			Attachment at = msg.Attachments[0];
			Assert.AreEqual (at.ContentType.MediaType, "text/plain");
		}*/

		[Test]
		public void Body ()
		{
			Assert.AreEqual (msg.Body, "hello");
		}
		
		[Test]
		public void BodyEncoding ()
		{
			Assert.AreEqual (msg.BodyEncoding, Encoding.ASCII);
		}

		[Test]
		public void From ()
		{
			Assert.AreEqual (msg.From.Address, "from@example.com");
		}

		[Test]
		public void IsBodyHtml ()
		{
			Assert.IsFalse (msg.IsBodyHtml);
		}

		[Test]
		public void Priority ()
		{
			Assert.AreEqual (msg.Priority, MailPriority.Normal);
		}

		[Test]
		public void Subject ()
		{
			Assert.AreEqual (msg.Subject, "the subject");
		}

		[Test]
		public void To ()
		{
			Assert.AreEqual (msg.To.Count, 1);
			Assert.AreEqual (msg.To[0].Address, "to@example.com");
		}
	}
}
#endif
