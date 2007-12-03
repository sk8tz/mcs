//
// WebClientTest.cs - NUnit Test Cases for System.Net.WebClient
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Collections;
using System.Runtime.Serialization;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class WebClientTest
	{
		private string _tempFolder;

		[SetUp]
		public void SetUp ()
		{
			_tempFolder = Path.Combine (Path.GetTempPath (),
				GetType ().FullName);
			if (Directory.Exists (_tempFolder))
				Directory.Delete (_tempFolder, true);
			Directory.CreateDirectory (_tempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (_tempFolder))
				Directory.Delete (_tempFolder, true);
		}

		[Test]
		[Category ("InetAccess")]
		public void DownloadTwice ()
		{
			WebClient wc = new WebClient();
			string filename = Path.GetTempFileName();
			
			// A new, but empty file has been created. This is a test case
			// for bug 81005
			wc.DownloadFile("http://google.com/", filename);
			
			// Now, remove the file and attempt to download again.
			File.Delete(filename);
			wc.DownloadFile("http://google.com/", filename);
		}

		[Test]
		public void DownloadData1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData ((string) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // DownloadData (string)
		public void DownloadData1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

#if NET_2_0
		[Test] // DownloadData (Uri)
		public void DownloadData2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadData (Uri)
		public void DownloadData2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadData (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}
#endif

		[Test]
		public void DownloadFile1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile ((string) null, "tmp.out");
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // DownloadFile (string, string)
		public void DownloadFile1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile ("tp://scheme.notsupported",
					"tmp.out");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // DownloadFile (string, string)
		public void DownloadFile1_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile ("tp://scheme.notsupported",
					(string) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("path", inner.ParamName, "#11");
			}
#endif
		}

#if NET_2_0
		[Test] // DownloadFile (Uri, string)
		public void DownloadFile2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile ((Uri) null, "tmp.out");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadFile (Uri, string)
		public void DownloadFile2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile (new Uri ("tp://scheme.notsupported"),
					"tmp.out");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // DownloadFile (Uri, string)
		public void DownloadFile2_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadFile (new Uri ("tp://scheme.notsupported"),
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadString (string)
		public void DownloadString1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadString (string)
		public void DownloadString1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // DownloadString (Uri)
		public void DownloadString2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // DownloadString (Uri)
		public void DownloadString2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.DownloadString (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}
#endif

		[Test] // OpenRead (string)
		public void OpenRead1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead ((string) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // OpenRead (string)
		public void OpenRead1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

#if NET_2_0
		[Test] // OpenRead (Uri)
		public void OpenRead2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenRead (Uri)
		public void OpenRead2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenRead (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}
#endif

		[Test] // OpenWrite (string)
		public void OpenWrite1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((string) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // OpenWrite (string)
		public void OpenWrite1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ("tp://scheme.notsupported");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // OpenWrite (string, string)
		public void OpenWrite2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((string) null, "PUT");
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // OpenWrite (string, string)
		public void OpenWrite2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ("tp://scheme.notsupported", "PUT");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

#if NET_2_0
		[Test] // OpenWrite (Uri)
		public void OpenWrite3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((Uri) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenWrite (Uri)
		public void OpenWrite3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite (new Uri ("tp://scheme.notsupported"));
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // OpenWrite (Uri, string)
		public void OpenWrite4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite ((Uri) null, "POST");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // OpenWrite (Uri, string)
		public void OpenWrite4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.OpenWrite (new Uri ("tp://scheme.notsupported"),
					"POST");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}
#endif

		[Test] // UploadData (string, byte [])
		public void UploadData1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((string) null, new byte [] { 0x1a });
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // UploadData (string, byte [])
		public void UploadData1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("tp://scheme.notsupported", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (string, byte [])
#if ONLY_1_1
		[Category ("NotDotNet")] // On MS, there's a nested NotImplementedException
#endif
		public void UploadData1_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("http://www.mono-project.com",
					(byte []) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("data", inner.ParamName, "#11");
			}
#endif
		}

#if NET_2_0
		[Test] // UploadData (Uri, byte [])
		public void UploadData2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((Uri) null, new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (Uri, byte [])
		public void UploadData2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("tp://scheme.notsupported"),
					new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (Uri, byte [])
		public void UploadData2_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("http://www.mono-project.com"),
					(byte []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}
#endif

		[Test] // UploadData (string, string, byte [])
		public void UploadData3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((string) null, "POST",
					new byte [] { 0x1a });
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // UploadData (string, string, byte [])
		public void UploadData3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("tp://scheme.notsupported",
					"POST", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (string, string, byte [])
#if ONLY_1_1
		[Category ("NotDotNet")] // On MS, there's a nested NotImplementedException
#endif
		public void UploadData3_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ("http://www.mono-project.com",
					"POST", (byte []) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("data", inner.ParamName, "#11");
			}
#endif
		}

#if NET_2_0
		[Test] // UploadData (Uri, string, byte [])
		public void UploadData4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData ((Uri) null, "POST", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadData (Uri, string, byte [])
		public void UploadData4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("tp://scheme.notsupported"),
					"POST", new byte [] { 0x1a });
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadData (Uri, string, byte [])
		public void UploadData4_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadData (new Uri ("http://www.mono-project.com"),
					"POST", (byte []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}
#endif

		[Test] // UploadFile (string, string)
		public void UploadFile1_Address_Null ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");
			File.Create (tempFile).Close ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((string) null, tempFile);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // UploadFile (string, string)
		public void UploadFile1_Address_SchemeNotSupported ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");
			File.Create (tempFile).Close ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadFile (string, string)
		public void UploadFile1_FileName_NotFound ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				// Could not find file "..."
				FileNotFoundException inner = ex.InnerException
					as FileNotFoundException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
				Assert.IsNotNull (inner.FileName, "#8");
				Assert.AreEqual (tempFile, inner.FileName, "#9");
				Assert.IsNull (inner.InnerException, "#10");
				Assert.IsNotNull (inner.Message, "#11");
			}
		}

		[Test] // UploadFile (string, string)
		public void UploadFile1_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					(string) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("path", inner.ParamName, "#11");
			}
#endif
		}

#if NET_2_0
		[Test] // UploadFile (Uri, string)
		public void UploadFile2_Address_Null ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((Uri) null, tempFile);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (Uri, string)
		public void UploadFile2_Address_SchemeNotSupported ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");
			File.Create (tempFile).Close ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadFile (Uri, string)
		public void UploadFile2_FileName_NotFound ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				// Could not find file "..."
				FileNotFoundException inner = ex.InnerException
					as FileNotFoundException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
				Assert.IsNotNull (inner.FileName, "#8");
				Assert.AreEqual (tempFile, inner.FileName, "#9");
				Assert.IsNull (inner.InnerException, "#10");
				Assert.IsNotNull (inner.Message, "#11");
			}
		}

		[Test] // UploadFile (Uri, string)
		public void UploadFile2_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}
#endif

		[Test] // UploadFile (string, string, string)
		public void UploadFile3_Address_Null ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");
			File.Create (tempFile).Close ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((string) null, "POST", tempFile);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("path", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // UploadFile (string, string, string)
		public void UploadFile3_Address_SchemeNotSupported ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");
			File.Create (tempFile).Close ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					"POST", tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadFile (string, string, string)
		public void UploadFile3_FileName_NotFound ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					"POST", tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				// Could not find file "..."
				FileNotFoundException inner = ex.InnerException
					as FileNotFoundException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
				Assert.IsNotNull (inner.FileName, "#8");
				Assert.AreEqual (tempFile, inner.FileName, "#9");
				Assert.IsNull (inner.InnerException, "#10");
				Assert.IsNotNull (inner.Message, "#11");
			}
		}

		[Test] // UploadFile (string, string, string)
		public void UploadFile3_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ("tp://scheme.notsupported",
					"POST", (string) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("path", inner.ParamName, "#11");
			}
#endif
		}

#if NET_2_0
		[Test] // UploadFile (Uri, string, string)
		public void UploadFile4_Address_Null ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile ((Uri) null, "POST", tempFile);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadFile (Uri, string, string)
		public void UploadFile4_Address_SchemeNotSupported ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");
			File.Create (tempFile).Close ();

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					"POST", tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadFile (Uri, string, string)
		public void UploadFile4_FileName_NotFound ()
		{
			string tempFile = Path.Combine (_tempFolder, "upload.tmp");

			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					"POST", tempFile);
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				// Could not find file "..."
				FileNotFoundException inner = ex.InnerException
					as FileNotFoundException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (FileNotFoundException), inner.GetType (), "#7");
				Assert.IsNotNull (inner.FileName, "#8");
				Assert.AreEqual (tempFile, inner.FileName, "#9");
				Assert.IsNull (inner.InnerException, "#10");
				Assert.IsNotNull (inner.Message, "#11");
			}
		}

		[Test] // UploadFile (Uri, string, string)
		public void UploadFile4_FileName_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadFile (new Uri ("tp://scheme.notsupported"),
					"POST", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string)
		public void UploadString1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((string) null, (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string)
		public void UploadString1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported", "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (string, string)
		public void UploadString1_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string)
		public void UploadString2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((Uri) null, (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string)
		public void UploadString2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"), "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (Uri, string)
		public void UploadString2_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"),
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string, string)
		public void UploadString3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((string) null, (string) null,
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (string, string, string)
		public void UploadString3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported",
					"POST", "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (string, string, string)
		public void UploadString3_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ("tp://scheme.notsupported",
					"POST", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string, string)
		public void UploadString4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString ((Uri) null, (string) null,
					(string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadString (Uri, string, string)
		public void UploadString4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"),
					"POST", "abc");
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadString (Uri, string, string)
		public void UploadString4_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadString (new Uri ("tp://scheme.notsupported"),
					"POST", (string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}
#endif

		[Test] // UploadValues (string, NameValueCollection)
		public void UploadValues1_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((string) null, new NameValueCollection ());
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // UploadValues (string, NameValueCollection)
		public void UploadValues1_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("tp://scheme.notsupported",
					new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (string, NameValueCollection)
#if ONLY_1_1
		[Category ("NotDotNet")] // On MS, there's a nested NotImplementedException
#endif
		public void UploadValues1_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("http://www.mono-project.com",
					(NameValueCollection) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("data", inner.ParamName, "#11");
			}
#endif
		}

#if NET_2_0
		[Test] // UploadValues (Uri, NameValueCollection)
		public void UploadValues2_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((Uri) null, new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (Uri, NameValueCollection)
		public void UploadValues2_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("tp://scheme.notsupported"),
					new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (Uri, NameValueCollection)
		public void UploadValues2_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("http://www.mono-project.com"),
					(NameValueCollection) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}
#endif

		[Test] // UploadValues (string, string, NameValueCollection)
		public void UploadValues3_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((string) null, "POST",
					new NameValueCollection ());
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("uriString", inner.ParamName, "#11");
			}
#endif
		}

		[Test] // UploadValues (string, string, NameValueCollection)
		public void UploadValues3_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("tp://scheme.notsupported",
					"POST", new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (string, string, NameValueCollection)
#if ONLY_1_1
		[Category ("NotDotNet")] // On MS, there's a nested NotImplementedException
#endif
		public void UploadValues3_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ("http://www.mono-project.com",
					"POST", (NameValueCollection) null);
				Assert.Fail ("#1");
#if NET_2_0
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
#else
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.Message, "#3");
				Assert.IsNull (ex.Response, "#4");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#5");

				ArgumentNullException inner = ex.InnerException
					as ArgumentNullException;
				Assert.IsNotNull (inner, "#6");
				Assert.AreEqual (typeof (ArgumentNullException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
				Assert.IsNotNull (inner.ParamName, "#10");
				Assert.AreEqual ("data", inner.ParamName, "#11");
			}
#endif
		}

#if NET_2_0
		[Test] // UploadValues (Uri, string, NameValueCollection)
		public void UploadValues4_Address_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues ((Uri) null, "POST",
					new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("address", ex.ParamName, "#6");
			}
		}

		[Test] // UploadValues (Uri, string, NameValueCollection)
		public void UploadValues4_Address_SchemeNotSupported ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("tp://scheme.notsupported"),
					"POST", new NameValueCollection ());
				Assert.Fail ("#1");
			} catch (WebException ex) {
				// An error occurred performing a WebClient request
				Assert.AreEqual (typeof (WebException), ex.GetType (), "#2");
				Assert.IsNotNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.Response, "#5");
				Assert.AreEqual (WebExceptionStatus.UnknownError, ex.Status, "#6");

				// The URI prefix is not recognized
				Exception inner = ex.InnerException;
				Assert.AreEqual (typeof (NotSupportedException), inner.GetType (), "#7");
				Assert.IsNull (inner.InnerException, "#8");
				Assert.IsNotNull (inner.Message, "#9");
			}
		}

		[Test] // UploadValues (Uri, string, NameValueCollection)
		public void UploadValues4_Data_Null ()
		{
			WebClient wc = new WebClient ();
			try {
				wc.UploadValues (new Uri ("http://www.mono-project.com"),
					"POST", (NameValueCollection) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("data", ex.ParamName, "#6");
			}
		}
#endif
	}
}
