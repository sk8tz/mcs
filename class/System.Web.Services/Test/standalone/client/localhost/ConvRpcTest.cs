/*
WARNING: This code was generated by a tool.
Changes to this file will be lost if the code is regenerated
*/

using System;
using System.Threading;
using NUnit.Framework;
using System.Web.Services.Protocols;
using System.Xml;
using ConvRpcTests.Soap;

namespace Localhost.ConvRpcTests
{
	[TestFixture]
	public class ConverterTest: WebServiceTest
	{
		[Test]
		public void TestService ()
		{
			ConverterService cs = new ConverterService ();
			cs.Login ("lluis");
			cs.SetCurrencyRate ("EUR", 0.5);
			Assert.AreEqual (0.5, cs.GetCurrencyRate ("EUR"), "#1");
			
			double res = cs.Convert ("EUR","USD",6);
			Assert.AreEqual ((int)res, (int)12, "#2");
			
			CurrencyInfo[] infos = cs.GetCurrencyInfo ();
			Assert.IsNotNull (infos, "infos");
			
			foreach (CurrencyInfo info in infos)
			{
				double val = 0;
				Assert.IsNotNull (info.Name, "info.Name");
				
				switch (info.Name)
				{
					case "USD": val = 1; break;
					case "EUR": val = 0.5; break;
					case "GBP": val = 0.611817; break;
					case "JPY": val = 118.271; break;
					case "CAD": val = 1.36338; break;
					case "AUD": val = 1.51485; break;
					case "CHF": val = 1.36915; break;
					case "RUR": val = 30.4300; break;
					case "CNY": val = 8.27740; break;
					case "ZAR": val = 7.62645; break;
					case "MXN": val = 10.5025; break;
				}
				Assert.AreEqual (val, info.Rate, "#3 " + info.Name);
			}
			cs.SetCurrencyRate ("EUR", 0.9);
		}
		
		// Async tests
		
		ConverterService acs;
		bool a1;
		bool a2;
		bool a3;
		AutoResetEvent eve = new AutoResetEvent (false);
		
		[Test]
		public void AsyncTestService ()
		{
			IAsyncResult ar;
			acs = new ConverterService ();
			
			ar = acs.BeginLogin ("lluis", null, null);
			acs.EndLogin (ar);
			
			acs.BeginSetCurrencyRate ("EUR", 0.5, new AsyncCallback(Callback1), null);
			
			Assert.IsTrue (eve.WaitOne (5000, false), "#0");
			Assert.IsTrue (a1, "#1");
			
			Assert.IsTrue (eve.WaitOne (5000, false), "#2");
			Assert.IsTrue (a2, "#3");
			
			Assert.IsTrue (eve.WaitOne (5000, false), "#4");
			Assert.IsTrue (a3, "#5");
		}
		
		void Callback1 (IAsyncResult ar)
		{
			acs.EndSetCurrencyRate (ar);
			acs.BeginGetCurrencyRate ("EUR", new AsyncCallback(Callback2), null);
		}
		
		void Callback2 (IAsyncResult ar)
		{
			double res = acs.EndGetCurrencyRate (ar);
			a1 = (res == 0.5);
			eve.Set ();
			
			acs.BeginConvert ("EUR","USD",6, new AsyncCallback(Callback3), null);
		}
		
		void Callback3 (IAsyncResult ar)
		{
			double res = acs.EndConvert (ar);
			a2 = (res == 12);
			eve.Set ();
			
			acs.BeginGetCurrencyInfo (new AsyncCallback(Callback4),null);
		}
		
		void Callback4 (IAsyncResult ar)
		{
			CurrencyInfo[] infos = acs.EndGetCurrencyInfo (ar);
			
			foreach (CurrencyInfo info in infos)
			{
				double val = 0;
				switch (info.Name)
				{
					case "USD": val = 1; break;
					case "EUR": val = 0.5; break;
					case "GBP": val = 0.611817; break;
					case "JPY": val = 118.271; break;
					case "CAD": val = 1.36338; break;
					case "AUD": val = 1.51485; break;
					case "CHF": val = 1.36915; break;
					case "RUR": val = 30.4300; break;
					case "CNY": val = 8.27740; break;
					case "ZAR": val = 7.62645; break;
					case "MXN": val = 10.5025; break;
				}
				a3 = (val == info.Rate);
				if (!a3) break;
			}
			eve.Set ();
		}
		
		[Test]
		public void TestException ()
		{
			ConverterService cs = new ConverterService ();
			try
			{
				cs.SetCurrencyRate ("EUR", 0.5);
				Assert.Fail ("#0");
			}
			catch (SoapException ex)
			{
				Assert.IsTrue (ex.Message.IndexOf ("User not logged") != -1, "#1");
				Assert.AreEqual (SoapException.ServerFaultCode, ex.Code, "#2");
			}
		}
		
		[Test]
		public void AsyncTestException ()
		{
			ConverterService cs = new ConverterService ();
			IAsyncResult ar = cs.BeginSetCurrencyRate ("EUR", 0.5, null, null);
			try
			{
				cs.EndSetCurrencyRate (ar);
				Assert.Fail ("#0");
			}
			catch (SoapException ex)
			{
				Assert.IsTrue (ex.Message.IndexOf ("User not logged") != -1, "#1");
				Assert.AreEqual (SoapException.ServerFaultCode, ex.Code, "#2");
			}
		}
		
		[Test]
		public void TestObjectReturn ()
		{
			ConverterServiceExtraTest et = new ConverterServiceExtraTest ();
			
			// Test the Discover method.
			et.Url = "http://localhost:8080/ConvRpc.asmx?disco";
			et.Discover ();
			
			string d;
			object res = et.GetTestInfo ("hi", out d);
			
			Assert.AreEqual ("iii", d, "t1");
			Assert.IsNotNull (res, "t2");
			Assert.IsTrue (res is XmlNode[], "t3");
			XmlNode[] nods = res as XmlNode[];
			Assert.AreEqual (5, nods.Length, "t4");
			
			Assert.IsTrue (nods[0] is XmlAttribute, "t5");
			XmlAttribute at = nods[0] as XmlAttribute;
			Assert.AreEqual ("id", at.LocalName, "t6");
			
			Assert.IsTrue (nods[1] is XmlAttribute, "t7");
			at = nods[1] as XmlAttribute;
			Assert.AreEqual ("type", at.LocalName, "t8");
			
			Assert.IsTrue (nods[2] is XmlAttribute, "t9");
			at = nods[2] as XmlAttribute;
			
			Assert.IsTrue (nods[3] is XmlElement, "t10");
			XmlElement el = nods[3] as XmlElement;
			Assert.AreEqual ("a", el.Name, "t11");
			
			Assert.IsTrue (nods[4] is XmlElement, "t12");
			el = nods[4] as XmlElement;
			Assert.AreEqual ("b", el.Name, "t13");
		}		
	}
	
	[System.Web.Services.WebServiceBindingAttribute(Name="ConverterServiceSoap", Namespace="urn:mono-ws-tests")]
	public class ConverterServiceExtraTest : System.Web.Services.Protocols.SoapHttpClientProtocol
	{
		[System.Web.Services.Protocols.SoapRpcMethodAttribute("urn:mono-ws-tests/GetTestInfo", RequestNamespace="urn:mono-ws-tests", ResponseNamespace="urn:mono-ws-tests" )]
		public object GetTestInfo(string s, out string d) {
			object[] results = this.Invoke("GetTestInfo", new object[] {s});
			d = (string) results[1];
	        return ((object)(results[0]));
		}
	}
}
