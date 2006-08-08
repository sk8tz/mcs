#if TARGET_JVM_FOR_WEBTEST
#define TARGET_JVM
#endif

using System;
using System.Reflection;
using System.IO;
using System.Web.Hosting;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// The most important class from user perspective. See <see cref="Request"/>,
	/// <see cref="Response"/>, <see cref="Invoker"/>, <see cref="Run"/> for
	/// more information.
	/// </summary>
	/// <seealso cref="Request"/>
	/// <seealso cref="Response"/>
	/// <seealso cref="Invoker"/>
	/// <seealso cref="Run"/>
	[Serializable]
	public class WebTest
	{
		object _userData;
		/// <summary>
		/// Any user-defined data. Must be serializable to pass between appdomains.
		/// </summary>
		/// <example>
		/// [Test]
		/// public void SampleTest ()
		/// {
		///	WebTest t = new WebTest (new HandlerInvoker (MyCallback));
		///	t.Run ();
		///	Assert.AreEqual ("Was here", t.UserData.ToString());
		/// }
		/// 
		/// static public void MyCallback ()
		/// {
		///	WebTest.CurrentTest.UserData = "Was here";
		/// }
		/// </example>

		public object UserData
		{
			get { return _userData; }
			set { _userData = value; }
		}

		Response _response;
		/// <summary>
		/// The result of the last <see cref="Run"/>. See <see cref="MonoTests.SystemWeb.Framework.Response"/>,
		/// <see cref="FormRequest"/>.
		/// </summary>
		/// <seealso cref="Run"/>
		/// <seealso cref="MonoTests.SystemWeb.Framework.Response"/>
		/// <seealso cref="FormRequest"/>
		public Response Response
		{
			get { return _response; }
			set { _response = value; }
		}

		BaseInvoker _invoker;
		/// <summary>
		/// Set the invoker, which is executed in the web context by <see cref="Invoke"/>
		/// method. Most commonly used <see cref="PageInvoker"/>. See also: <see cref="BaseInvoker"/>,
		/// <see cref="HandlerInvoker"/>
		/// </summary>
		/// <seealso cref="Invoke"/>
		/// <seealso cref="PageInvoker"/>
		/// <seealso cref="BaseInvoker"/>
		/// <seealso cref="HandlerInvoker"/>
		public BaseInvoker Invoker
		{
			get { return _invoker; }
			set { _invoker = value; }
		}

		BaseRequest _request;
		/// <summary>
		/// Contains all the data necessary to create an <see cref="System.Web.HttpWorkerRequest"/> in
		/// the application appdomain. See also <see cref="BaseRequest"/>,
		/// <see cref="PostableRequest"/>, <see cref="FormRequest"/>.
		/// </summary>
		/// <seealso cref="System.Web.HttpWorkerRequest"/>
		/// <seealso cref="BaseRequest"/>
		/// <seealso cref="PostableRequest"/>
		/// <seealso cref="FormRequest"/>
		public BaseRequest Request
		{
			get { return _request; }
			set { _request = value; }
		}

		static MyHost host;
		private static MyHost Host
		{
			get {
				if (host != null)
					return host;
#if !TARGET_JVM
				host = AppDomain.CurrentDomain.GetData (HOST_INSTANCE_NAME) as MyHost;
				if (host != null)
					return host;
				try {
					host = new MyHost (); //Fake instance to make EnsureHosting happy
					host = InitHosting ();

				}
				catch {
					host = null; //Remove the fake instance if CreateHosting failed
					throw;
				}
#else
				host = new MyHost ();
#endif
				return host;
			}
		}

		/// <summary>
		/// Run the request using <see cref="Request"/> and <see cref="Invoker"/>
		/// values. Keep the result of the request in <see cref="Response"/> property.
		/// </summary>
		/// <returns>The body of the HTTP response (<see cref="MonoTests.SystemWeb.Framework.Response.Body"/>).</returns>
		/// <seealso cref="Request"/>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="Response"/>
		/// <seealso cref="MonoTests.SystemWeb.Framework.Response.Body"/>
		public string Run ()
		{
			if (Request.Url == null)
				Request.Url = Invoker.GetDefaultUrl ();
			WebTest newTestInstance = Host.Run (this);
			CopyFrom (newTestInstance);
			return _response.Body;
		}

		private void CopyFrom (WebTest newTestInstance)
		{
			this._invoker = newTestInstance._invoker;
			this._request = newTestInstance._request;
			this._response = newTestInstance._response;
			this._userData = newTestInstance._userData;
		}

		/// <summary>
		/// The instance of the currently running test. Defined only in the web appdomain.
		/// In different threads this property may have different values.
		/// </summary>
		public static WebTest CurrentTest
		{
			get { return MyHost.GetCurrentTest (); }
		}

		/// <summary>
		/// This method must be called when custom <see cref="System.Web.IHttpHandler.ProcessRequest"/> or aspx code behind is used,
		/// to allow the framework to invoke all user supplied delegates.
		/// </summary>
		/// <param name="param">Parameter defined by the <see cref="BaseInvoker"/> subclass. For example,
		/// <see cref="PageInvoker"/> expects to receive a <see cref="System.Web.UI.Page"/> instance here.</param>
		/// <seealso cref="System.Web.IHttpHandler.ProcessRequest"/>
		/// <seealso cref="BaseInvoker"/>
		/// <seealso cref="PageInvoker"/>
		public void Invoke (object param)
		{
			try {
				Invoker.DoInvoke (param);
			}
			catch (Exception ex) {
				RegisterException (ex);
				throw;
			}
		}

		public void SendHeaders ()
		{
			Host.SendHeaders (this);
		}

		/// <summary>
		/// This method is intended for use from <see cref="MonoTests.SystemWeb.Framework.BaseInvoker.DoInvoke"/> when
		/// the invocation causes an exception. In such cases, the exception must be registered
		/// with this method, and then swallowed. Before returning, <see cref="WebTest.Run"/>
		/// will rethrow this exception. This is done to hide the exception from <see cref="System.Web.HttpRuntime"/>,
		/// which normally swallows the exception and returns 500 ERROR http result.
		/// </summary>
		/// <param name="ex">The exception to be registered and rethrown.</param>
		/// <seealso cref="MonoTests.SystemWeb.Framework.BaseInvoker.DoInvoke"/>
		/// <seealso cref="WebTest.Run"/>
		/// <seealso cref="System.Web.HttpRuntime"/>
		public static void RegisterException (Exception ex)
		{
			Host.RegisterException (ex);
		}

		/// <summary>
		/// Unload the web appdomain and delete the temporary application root
		/// directory.
		/// </summary>
		public static void Unload ()
		{
#if !TARGET_JVM
			if (host == null)
				return;

			AppDomain oldDomain = host.AppDomain;
			host = null;
			AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, null);
			AppDomain.Unload (oldDomain);
			Directory.Delete (baseDir, true);
#endif
		}

		/// <summary>
		/// Default constructor. Initializes <see cref="Invoker"/> with a new
		/// <see cref="BaseInvoker"/> and <see cref="Request"/> with an empty
		/// <see cref="BaseRequest"/>.
		/// </summary>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="BaseInvoker"/>
		/// <seealso cref="Request"/>
		/// <seealso cref="BaseRequest"/>
		public WebTest ()
		{
			Invoker = new BaseInvoker ();
			Request = new BaseRequest ();
		}

		/// <summary>
		/// Same as <see cref="WebTest()"/>, and set <see cref="MonoTests.SystemWeb.Framework.BaseRequest.Url"/> to
		/// the specified Url.
		/// </summary>
		/// <param name="url">The URL used for the next <see cref="Run"/></param>
		/// <seealso cref="MonoTests.SystemWeb.Framework.BaseRequest.Url"/>
		/// <seealso cref="Run"/>
		public WebTest (string url)
			: this ()
		{
			Request.Url = url;
		}

		/// <summary>
		/// Create a new instance, initializing <see cref="Invoker"/> with the given
		/// value, and the <see cref="Request"/> with <see cref="BaseRequest"/>.
		/// </summary>
		/// <param name="invoker">The invoker used for this test.</param>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="Request"/>
		/// <seealso cref="BaseRequest"/>
		public WebTest (BaseInvoker invoker)
			: this ()
		{
			Invoker = invoker;
		}

		/// <summary>
		/// Create a new instance, initializing <see cref="Request"/> with the given
		/// value, and the <see cref="Invoker"/> with <see cref="BaseInvoker"/>.
		/// </summary>
		/// <param name="request">The request used for this test.</param>
		/// <seealso cref="Request"/>
		/// <seealso cref="Invoker"/>
		/// <seealso cref="BaseInvoker"/>
		public WebTest (BaseRequest request)
			: this ()
		{
			Request = request;
		}


#if !TARGET_JVM
		static void LoadAssemblyRecursive (Assembly ass)
		{
			if (ass.GlobalAssemblyCache)
				return;
			foreach (AssemblyName ran in ass.GetReferencedAssemblies ()) {
				bool found = false;
				foreach (Assembly domain_ass in AppDomain.CurrentDomain.GetAssemblies ()) {
					if (domain_ass.FullName == ran.FullName) {
						found = true;
						break;
					}
				}
				if (found)
					continue;
				Assembly ra = Assembly.Load (ran, null);
				LoadAssemblyRecursive (ra);
			}
		}

		private static void CopyAssembly (Assembly ass, string dir)
		{
			if (ass.GlobalAssemblyCache)
				return;
			string oldfn = ass.Location;
			if (oldfn.EndsWith (".exe"))
				return;
			string newfn = Path.Combine (dir, Path.GetFileName (oldfn));
			if (File.Exists (newfn))
				return;
			File.Copy (oldfn, newfn);
		}
#endif

		private static void EnsureDirectoryExists (string directory)
		{
			if (directory == string.Empty)
				return;
			if (Directory.Exists (directory))
				return;
			EnsureDirectoryExists (Path.GetDirectoryName (directory));
			Directory.CreateDirectory (directory);
		}

		/// <summary>
		/// Copy a resource embedded in the assembly into the web application
		/// </summary>
		/// <param name="type">A type in the assembly that contains the embedded resource.</param>
		/// <param name="resourceName">The name of the resource.</param>
		/// <param name="targetUrl">The URL where the resource will be available</param>
		/// <exception cref="System.ArgumentException">Thrown when resource with name resourceName is not found.</exception>
		/// <example><code>CopyResource (GetType (), "Default.skin", "App_Themes/Black/Default.skin");</code></example>
		public static void CopyResource (Type type, string resourceName, string targetUrl)
		{
#if !TARGET_JVM
			EnsureHosting ();
			EnsureDirectoryExists (Path.Combine (baseDir,
				Path.GetDirectoryName (targetUrl)));
			using (Stream source = type.Assembly.GetManifestResourceStream (resourceName)) {
				if (source == null)
					throw new ArgumentException ("resource not found: " + resourceName, "resourceName");
				using (FileStream target = new FileStream (Path.Combine (baseDir, targetUrl), FileMode.Create)) {
					byte[] array = new byte[source.Length];
					source.Read (array, 0, array.Length);
					target.Write (array, 0, array.Length);
				}
			}
#endif
		}

		private static void EnsureHosting ()
		{
			MyHost h = Host;
		}

#if !TARGET_JVM
		const string VIRTUAL_BASE_DIR = "/NunitWeb";
		private static string baseDir;
		private static string binDir;
		const string HOST_INSTANCE_NAME = "MonoTests/SysWeb/Framework/Host";

		private static MyHost InitHosting ()
		{
			string tmpFile = Path.GetTempFileName ();
			File.Delete (tmpFile);
			Directory.CreateDirectory (tmpFile);
			baseDir = tmpFile;
			binDir = Directory.CreateDirectory (Path.Combine (baseDir, "bin")).FullName;

			CopyResources ();
			File.Create (Path.Combine (baseDir, "page.fake"));
			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				LoadAssemblyRecursive (ass);

			foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies ())
				CopyAssembly (ass, binDir);

			MyHost host = (MyHost) ApplicationHost.CreateApplicationHost (typeof (MyHost), VIRTUAL_BASE_DIR, baseDir);
			AppDomain.CurrentDomain.SetData (HOST_INSTANCE_NAME, host);
			host.AppDomain.SetData (HOST_INSTANCE_NAME, host);
			return host;
		}

		private static void CopyResources ()
		{
#if VISUAL_STUDIO
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.Web.config",
				"Web.config");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.MyPage.aspx",
				"MyPage.aspx");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.MyPage.aspx.cs",
				"MyPage.aspx.cs");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.MyPageWithMaster.aspx",
				"MyPageWithMaster.aspx");
			CopyResource (typeof (WebTest),
				"MonoTests.SystemWeb.Framework.Resources.My.master",
				"My.master");
//#if TARGET_JVM
//                        CopyResource (typeof (WebTest),
//                                "MonoTests.SystemWeb.Framework.Resources.assemblies.global.asax.xml",
//                                "assemblies/global.asax.xml");
//                        CopyResource (typeof (WebTest),
//                                "MonoTests.SystemWeb.Framework.Resources.assemblies.mypage.aspx.xml",
//                                "assemblies/mypage.aspx.xml");
//                        CopyResource (typeof (WebTest),
//                                "MonoTests.SystemWeb.Framework.Resources.assemblies.hnnefdht.dll.ghres",
//                                "assemblies/hnnefdht/dll.ghres");
//                        CopyResource (typeof (WebTest),
//                                "MonoTests.SystemWeb.Framework.Resources.assemblies.hnnefdht.hnnefdhtAttrib.class",
//                                "assemblies/hnnefdht/hnnefdhtAttrib.class");
////			CopyResource (typeof (WebTest),
////				"MonoTests.SystemWeb.Framework.Resources.assemblies.hnnefdht.ASP.MyPage_aspx_MyPage_aspxAttrib.class",
////				"assemblies/hnnefdht/ASP/MyPage_aspx$MyPage_aspxAttrib.class");
////			CopyResource (typeof (WebTest),
////				"MonoTests.SystemWeb.Framework.Resources.assemblies.hnnefdht.ASP.MyPage_aspx.class",
////				"assemblies/hnnefdht/ASP/MyPage_aspx.class");
//#endif
#else
			CopyResource (typeof (WebTest), "Web.config", "Web.config");
			CopyResource (typeof (WebTest), "MyPage.aspx", "MyPage.aspx");
			CopyResource (typeof (WebTest), "MyPage.aspx.cs", "MyPage.aspx.cs");
			CopyResource (typeof (WebTest), "MyPageWithMaster.aspx", "MyPageWithMaster.aspx");
			CopyResource (typeof (WebTest), "My.master", "My.master");
#endif
		}
#endif
	}
}
