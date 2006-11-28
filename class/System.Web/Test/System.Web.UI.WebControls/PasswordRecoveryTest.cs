//
// PasswordRecoveryTest.cs - Unit tests for System.Web.UI.WebControls.PasswordRecovery
//
// Author:
//	Vladimir Krasnov  <vladimirk@mainsoft.com>
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

#if NET_2_0

using System;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Text.RegularExpressions;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls
{

	public class TestPasswordRecovery : PasswordRecovery
	{

		public string Tag
		{
			get { return base.TagName; }
		}

		public StateBag StateBag
		{
			get { return base.ViewState; }
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		public Style GetStyle ()
		{
			return base.CreateControlStyle ();
		}

		public void TrackState ()
		{
			TrackViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void SetDesignMode (IDictionary dic)
		{
			base.SetDesignModeState (dic);
		}

		private bool onBubble;

		public bool OnBubbleEventCalled
		{
			get { return onBubble; }
			set { onBubble = value; }
		}

		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			onBubble = true;
			return base.OnBubbleEvent (source, e);
		}

		public bool DoBubbleEvent (object source, EventArgs e)
		{
			return base.OnBubbleEvent (source, e);
		}

		public void DoEnsureChildControls ()
		{
			base.EnsureChildControls ();
		}

		public bool DoOnBubbleEvent (EventArgs e)
		{
			return base.OnBubbleEvent (this, e);
		}
	}

	[Serializable]
	[TestFixture]
	[Category("NotWorking")]
	//
	// This  uses CopyResource to Web.config, and until WebTest is not fixed to create
	// per-test domains, this will break.
	//
	public class PasswordRecoveryTest
	{
		[TestFixtureSetUp]
		public void CopyTestResources ()
		{
			WebTest.CopyResource (GetType (), "WebControl.config", "Web.config");
		}

		[SetUp]
		public void TestSetup ()
		{
			Thread.Sleep (150);
		}

		[Test]
		public void DefaultProperties ()
		{
			TestPasswordRecovery w = new TestPasswordRecovery ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");

			Assert.AreEqual ("Answer:", w.AnswerLabelText, "AnswerLabelText");
			Assert.AreEqual ("Answer is required.", w.AnswerRequiredErrorMessage, "AnswerRequiredErrorMessage");
			//Assert.IsFalse (w.AutoGeneratePassword, "AutoGeneratePassword");
			Assert.AreEqual ("Your attempt to retrieve your password was not successful. Please try again.", w.GeneralFailureText, "CompleteSuccessText");
			Assert.AreEqual (string.Empty, w.HelpPageIconUrl, "HelpPageIconUrl");
			Assert.AreEqual (string.Empty, w.HelpPageText, "HelpPageText");
			Assert.AreEqual (string.Empty, w.HelpPageUrl, "HelpPageUrl");
			Assert.AreEqual ("Your answer could not be verified. Please try again.", w.QuestionFailureText, "InstructionText");
			Assert.AreEqual ("Answer the following question to receive your password.", w.QuestionInstructionText, "InstructionText");
			Assert.AreEqual ("Question:", w.QuestionLabelText, "ConfirmPasswordCompareErrorMessage");
			Assert.AreEqual ("Identity Confirmation", w.QuestionTitleText, "ConfirmPasswordLabelText");
			Assert.AreEqual ("Submit", w.SubmitButtonText, "ConfirmPasswordRequiredErrorMessage");
			Assert.AreEqual (ButtonType.Button, w.SubmitButtonType, "ContinueButtonImageUrl");
			Assert.AreEqual (string.Empty, w.SuccessPageUrl, "ContinueButtonText");
			Assert.AreEqual ("Your password has been sent to you.", w.SuccessText, "ContinueButtonType");
			Assert.AreEqual ("We were unable to access your information. Please try again.", w.UserNameFailureText, "ContinueDestinationPageUrl");
			Assert.AreEqual ("Enter your User Name to receive your password.", w.UserNameInstructionText, "CreateUserButtonImageUrl");
			Assert.AreEqual ("User Name:", w.UserNameLabelText, "CreateUserButtonText");
			Assert.AreEqual ("User Name is required.", w.UserNameRequiredErrorMessage, "CreateUserButtonType");
			Assert.AreEqual ("Forgot Your Password?", w.UserNameTitleText, "DuplicateEmailErrorMessage");
		}

		[Test]
		public void AssignToDefaultProperties ()
		{
			TestPasswordRecovery w = new TestPasswordRecovery ();
			Assert.AreEqual (0, w.Attributes.Count, "Attributes.Count");
			Assert.AreEqual (0, w.StateBag.Count, "ViewState.Count");

			int count = 0;

			w.AnswerLabelText = "text";
			Assert.AreEqual ("text", w.AnswerLabelText, "Assign AnswerLabelText,");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate AnswerLabelText,");

			w.AnswerRequiredErrorMessage = "text";
			Assert.AreEqual ("text", w.AnswerRequiredErrorMessage, "Assign AnswerRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate AnswerRequiredErrorMessage");

			w.GeneralFailureText = "text";
			Assert.AreEqual ("text", w.GeneralFailureText, "Assign GeneralFailureText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate GeneralFailureText");

			w.HelpPageIconUrl = "text";
			Assert.AreEqual ("text", w.HelpPageIconUrl, "Assign HelpPageIconUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageIconUrl");

			w.HelpPageText = "text";
			Assert.AreEqual ("text", w.HelpPageText, "Assign HelpPageText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageText");

			w.HelpPageUrl = "text";
			Assert.AreEqual ("text", w.HelpPageUrl, "Assign HelpPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate HelpPageUrl");

			w.MembershipProvider = "text";
			Assert.AreEqual ("text", w.MembershipProvider, "Assign MembershipProvider");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate MembershipProvider");

			w.QuestionFailureText = "msg";
			Assert.AreEqual ("msg", w.QuestionFailureText, "Assign QuestionFailureText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate QuestionFailureText");

			w.QuestionInstructionText = "msg";
			Assert.AreEqual ("msg", w.QuestionInstructionText, "Assign QuestionInstructionText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate QuestionInstructionText");

			w.QuestionLabelText = "msg";
			Assert.AreEqual ("msg", w.QuestionLabelText, "Assign QuestionLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate QuestionLabelText");

			w.QuestionTitleText = "msg";
			Assert.AreEqual ("msg", w.QuestionTitleText, "Assign QuestionTitleText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate QuestionTitleText");

			w.SubmitButtonImageUrl = "msg";
			Assert.AreEqual ("msg", w.SubmitButtonImageUrl, "Assign SubmitButtonImageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate SubmitButtonImageUrl");

			w.SuccessPageUrl = "msg";
			Assert.AreEqual ("msg", w.SuccessPageUrl, "Assign SuccessPageUrl");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate SuccessPageUrl");

			w.SuccessText = "msg";
			Assert.AreEqual ("msg", w.SuccessText, "Assign SuccessText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate SuccessText");

			w.UserName = "msg";
			Assert.AreEqual ("msg", w.UserName, "Assign UserName");
			Assert.AreEqual (count, w.StateBag.Count, "Viewstate UserName");

			w.UserNameInstructionText = "msg";
			Assert.AreEqual ("msg", w.UserNameInstructionText, "Assign UserNameInstructionText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UserNameInstructionText");

			w.UserNameLabelText = "msg";
			Assert.AreEqual ("msg", w.UserNameLabelText, "Assign UserNameLabelText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UserNameLabelText");

			w.UserNameRequiredErrorMessage = "msg";
			Assert.AreEqual ("msg", w.UserNameRequiredErrorMessage, "Assign UserNameRequiredErrorMessage");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UserNameRequiredErrorMessage");

			w.UserNameTitleText = "msg";
			Assert.AreEqual ("msg", w.UserNameTitleText, "Assign UserNameTitleText");
			Assert.AreEqual (++count, w.StateBag.Count, "Viewstate UserNameTitleText");

		}

		public static void BasicRenderTestInit (Page p)
		{
			CreateTestControl (p);
		}

		public static PasswordRecovery CreateTestControl (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);

			PasswordRecovery w = new PasswordRecovery ();
			w.ID = "PasswordRecovery1";

			p.Form.Controls.Add (lcb);
			p.Form.Controls.Add (w);
			p.Form.Controls.Add (lce);

			return w;
		}

		[Test]
		[Category ("NunitWeb")]
		public void BasicRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (BasicRenderTestInit))).Run ();

			int st = 0;
			Assert.IsTrue ((st = html.IndexOf ("<table", st)) > 0, "base render test 1");
			Assert.IsTrue ((st = html.IndexOf ("PasswordRecovery1", st)) > 0, "base render test 2");
			Assert.IsTrue ((st = html.IndexOf ("border-collapse:collapse", st)) > 0, "base render test 3");
			Assert.IsTrue ((st = html.IndexOf ("<table", st)) > 0, "base render test 4");
			Assert.IsTrue ((st = html.IndexOf ("Forgot Your Password?", st)) > 0, "base render test 6");
			Assert.IsTrue ((st = html.IndexOf ("Enter your User Name to receive your password.", st)) > 0, "base render test 7");
			Assert.IsTrue ((st = html.IndexOf ("User Name:", st)) > 0, "base render test 8");
			Assert.IsTrue ((st = html.IndexOf ("Submit", st)) > 0, "base render test 9");
		}

		[Test]
		[Category ("NunitWeb")]
		public void TitlesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (TitlesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("userid") > 0, "UserNameLabelText");
			Assert.IsTrue (html.IndexOf ("forgot") > 0, "UserNameTitleText");
			Assert.IsTrue (html.IndexOf ("UserNameInstructionText") > 0, "UserNameInstructionText");
			Assert.IsTrue (html.IndexOf ("zzxcmnmncx") > 0, "SubmitButtonText");
		}

		public static void TitlesRenderTestInit (Page p)
		{
			PasswordRecovery w = CreateTestControl (p);
			w.UserNameLabelText = "userid";
			w.UserNameTitleText = "forgot";
			w.UserNameInstructionText = "UserNameInstructionText";
			w.SubmitButtonText = "zzxcmnmncx";
		}

		[Test]
		[Category ("NunitWeb")]
		public void ExtraTitlesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (ExtraTitlesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("http://www.HelpPageUrl.com") > 0, "HelpPageUrl");
			Assert.IsTrue (html.IndexOf ("HelpPageText") > 0, "HelpPageText");
			Assert.IsTrue (html.IndexOf ("http://www.HelpPageIconUrl.com") > 0, "HelpPageIconUrl");
		}

		public static void ExtraTitlesRenderTestInit (Page p)
		{
			PasswordRecovery w = CreateTestControl (p);
			w.HelpPageUrl = "http://www.HelpPageUrl.com";
			w.HelpPageText = "HelpPageText";
			w.HelpPageIconUrl = "http://www.HelpPageIconUrl.com";
		}

		[Test]
		[Category ("NunitWeb")]
		public void StylesRenderTest ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (StylesRenderTestInit))).Run ();

			Assert.IsTrue (html.IndexOf ("LightGoldenrodYellow;") > 0, "TextBoxStyle");
			Assert.IsTrue (html.IndexOf ("732px") > 0, "TitleTextStyle");
			Assert.IsTrue (html.IndexOf ("LightSkyBlue;") > 0, "HyperLinkStyle");
			Assert.IsTrue (html.IndexOf ("MediumSeaGreen;") > 0, "InstructionTextStyle");
			Assert.IsTrue (html.IndexOf ("MediumSpringGreen;") > 0, "LabelStyle");
		}

		private string GetDecoratedId (string html, string id)
		{
			Regex reg = new Regex ("name=\".*[\\$\\:]" + id + "\"");
			Match match = reg.Match (html);

			string fixedId = match.Value;
			if (fixedId.Length > 0)
				fixedId = fixedId.Substring (fixedId.IndexOf ("\""), fixedId.Length - fixedId.IndexOf ("\"")).Trim ('\"');

			return fixedId;
		}

		private static string GetEventTarget (string html, string id)
		{
			Regex reg = new Regex ("__doPostBack.*\\(.*'.*" + id + "'");
			Match match = reg.Match (html);

			string fixedId = match.Value;
			if (fixedId.Length > 0)
				fixedId = fixedId.Substring (fixedId.IndexOf ("'"), fixedId.Length - fixedId.IndexOf ("'")).Trim ('\'');

			return fixedId;
		}

		[Test]
		[Category ("NunitWeb")]
		public void BasicPostbackTest ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (StylesRenderTestInit));
			WebTest test = new WebTest (pi);

			string html = test.Run ();
			test.Invoker = pi;

			FormRequest fr = new FormRequest (test.Response, "form1");

			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "username"));

			PageDelegates pd = new PageDelegates ();
			pd.PreRender = new PageDelegate (BasicPostTestPreRender);
			pd.Load = new PageDelegate (StylesRenderTestInit);
			pi.Delegates = pd;

			test.Request = fr;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("username") > 0, "rendered user name");

			Assert.IsTrue (html.IndexOf ("LightGoldenrodYellow;") > 0, "TextBoxStyle");
			Assert.IsTrue (html.IndexOf ("732px") > 0, "TitleTextStyle");
			Assert.IsTrue (html.IndexOf ("LightSkyBlue;") > 0, "HyperLinkStyle");
			Assert.IsTrue (html.IndexOf ("MediumSeaGreen;") > 0, "InstructionTextStyle");
			Assert.IsTrue (html.IndexOf ("MediumSpringGreen;") > 0, "LabelStyle");
		}

		public static void BasicPostTestPreRender (Page p)
		{
			PasswordRecovery w = (PasswordRecovery) p.FindControl ("PasswordRecovery1");
			if (w == null)
				Assert.Fail ("postback1");

			Assert.AreEqual ("username", w.UserName, "posted user name");
		}

		public static void StylesRenderTestInit (Page p)
		{
			PasswordRecovery w = CreateTestControl (p);
			w.MembershipProvider = "FakeProvider";
			w.SendingMail += new MailMessageEventHandler(w_SendingMail);
			w.SendMailError += new SendMailErrorEventHandler(w_SendMailError);

			if (!p.IsPostBack) {
				w.TextBoxStyle.BackColor = Color.LightGoldenrodYellow;
				w.TitleTextStyle.Height = Unit.Pixel (732);
				w.LabelStyle.BackColor = Color.MediumSpringGreen;

				w.HelpPageUrl = "http://www.HelpPageUrl.com";
				w.HelpPageText = "hhh";
				w.HyperLinkStyle.BackColor = Color.LightSkyBlue;

				w.UserNameInstructionText = "text";
				w.InstructionTextStyle.BackColor = Color.MediumSeaGreen;
			}
		}

		public static void w_SendingMail (object sender, MailMessageEventArgs e)
		{
			if (e.Message.Body.IndexOf ("123") > 0)
				WebTest.CurrentTest.UserData = "w_SendingMail";
		}
		
		public static void w_SendMailError (object sender, SendMailErrorEventArgs e)
		{
			e.Handled = true;
		}

		// TODO:
		// ValidatorTextStyle
		// ErrorMessageStyle
		[Test]
		[Category ("NotDotNet")]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void GetPasswordTest ()
		{
			PageInvoker pi = PageInvoker.CreateOnLoad (new PageDelegate (StylesRenderTestInit));
			WebTest test = new WebTest (pi);

			string html = test.Run ();
			test.Invoker = pi;

			FormRequest fr = new FormRequest (test.Response, "form1");

			fr.Controls.Add (new BaseControl (GetDecoratedId (html, "UserName"), "heh"));
			string button = GetDecoratedId (html, "SubmitButton");
			if (button.Length > 0)
				fr.Controls.Add (new BaseControl (GetDecoratedId (html, "SubmitButton"), "SubmitButton"));
			else
				fr.Controls.Add (new BaseControl ("__EVENTTARGET", GetEventTarget (html, "SubmitButton")));

			test.Request = fr;
			html = test.Run ();

//6) MonoTests.System.Web.UI.WebControls.PasswordRecoveryTest.GetPasswordTest : System.Xml.XmlException : comments cannot contain '--'  Line 40, position 33.^M
//  at Mono.Xml2.XmlTextReader.ReadComment () [0x000b4] in /home/cvs/mcs/class/System.XML/System.Xml/XmlTextReader.cs:2279
//  at Mono.Xml2.XmlTextReader.ReadDeclaration () [0x00031] in /home/cvs/mcs/class/System.XML/System.Xml/XmlTextReader.cs:2223
//  at Mono.Xml2.XmlTextReader.ReadContent () [0x0012f] in /home/cvs/mcs/class/System.XML/System.Xml/XmlTextReader.cs:1273
//  at Mono.Xml2.XmlTextReader.ReadContent () [0x00155] in /home/cvs/mcs/class/System.XML/System.Xml/XmlTextReader.cs:1286
//  at Mono.Xml2.XmlTextReader.Read () [0x00125] in /home/cvs/mcs/class/System.XML/System.Xml/XmlTextReader.cs:605
//  at System.Xml.XmlTextReader.Read () [0x0006d] in /home/cvs/mcs/class/System.XML/System.Xml/XmlTextReader2.cs:551
//  at System.Xml.XmlDocument.ReadNodeCore (System.Xml.XmlReader reader) [0x00364] in /home/cvs/mcs/class/System.XML/System.Xml/XmlDocument.cs:976
//  at System.Xml.XmlDocument.ReadNodeCore (System.Xml.XmlReader reader) [0x00195] in /home/cvs/mcs/class/System.XML/System.Xml/XmlDocument.cs:904
//  at System.Xml.XmlDocument.ReadNode (System.Xml.XmlReader reader) [0x00033] in /home/cvs/mcs/class/System.XML/System.Xml/XmlDocument.cs:823
//  at System.Xml.XmlDocument.Load (System.Xml.XmlReader xmlReader) [0x00019] in /home/cvs/mcs/class/System.XML/System.Xml/XmlDocument.cs:693
//  at System.Xml.XmlDocument.LoadXml (System.String xml) [0x00023] in /home/cvs/mcs/class/System.XML/System.Xml/XmlDocument.cs:716
//  at MonoTests.SystemWeb.Framework.FormRequest.ExtractFormAndHiddenControls (MonoTests.SystemWeb.Framework.Response response, System.String formId) [0x00033] in /home/cvs/mcs/class/System.Web/Test/mainsoft/NunitWeb/NunitWeb/FormRequest.cs:55
//  at MonoTests.SystemWeb.Framework.FormRequest..ctor (MonoTests.SystemWeb.Framework.Response response, System.String formId) [0x00011] in /home/cvs/mcs/class/System.Web/Test/mainsoft/NunitWeb/NunitWeb/FormRequest.cs:31
//  at MonoTests.System.Web.UI.WebControls.PasswordRecoveryTest.GetPasswordTest () [0x000c8] in /home/cvs/mcs/class/System.Web/Test/System.Web.UI.WebControls/PasswordRecoveryTest.cs:474
//  at <0x00000> <unknown method>
//  at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (object,object[])
//  at System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00056] in /home/cvs/mcs/class/corlib/System.Reflection/MonoMethod.cs:143
//
			FormRequest fr2 = new FormRequest (test.Response, "form1");

			fr2.Controls.Add (new BaseControl (GetDecoratedId (html, "Answer"), "heh"));
			button = GetDecoratedId (html, "SubmitButton");
			if (button.Length > 0)
				fr2.Controls.Add (new BaseControl (GetDecoratedId (html, "SubmitButton"), "SubmitButton"));
			else
				fr2.Controls.Add (new BaseControl ("__EVENTTARGET", GetEventTarget (html, "SubmitButton")));

			test.Request = fr2;
			html = test.Run ();

			Assert.IsTrue (html.IndexOf ("sent to you") > 0, "GetPassword");
			Assert.AreEqual ("w_SendingMail", (string)test.UserData, "Mailsent");
		}

	}
}

#endif
