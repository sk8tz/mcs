//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using System.IO;
using System.Web.J2EE;
using System.Xml;
using vmw.common;
using System.Web.Util;

namespace System.Web.UI {

	public abstract class TemplateControl : Control, INamingContainer
	{
		static object abortTransaction = new object ();
		static object commitTransaction = new object ();
		static object error = new object ();
		static string [] methodNames = { "Page_Init",
						 "Page_Load",
						 "Page_DataBind",
						 "Page_PreRender",
						 "Page_Disposed",
						 "Page_Error",
						 "Page_Unload",
						 "Page_AbortTransaction",
						 "Page_CommitTransaction" };

		const BindingFlags bflags = BindingFlags.Public |
						BindingFlags.NonPublic |
						BindingFlags.Instance;

		private string _templateSourceDir;
		private static string hashTableMutex = "lock"; //used to sync access ResourceHash property
		private byte [] GetResourceBytes (Type type)
		{
			Hashtable table = (Hashtable) AppDomain.CurrentDomain.GetData ("TemplateControl.RES_BYTES");
			if (table == null) {
				return null;
			}
			return (byte []) table [type];
		}
		private void SetResourceBytes (Type type, byte [] bytes)
		{
			Hashtable table = (Hashtable) AppDomain.CurrentDomain.GetData ("TemplateControl.RES_BYTES");
			if (table == null) {
				table = new Hashtable ();
				AppDomain.CurrentDomain.SetData ("TemplateControl.RES_BYTES", table);
			}
			table [type] = bytes;
			return;
		}

		private Hashtable ResourceHash
		{
			get
			{
				Hashtable table = (Hashtable) AppDomain.CurrentDomain.GetData ("TemplateControl.RES_STRING");
				if (table == null) {
					table = new Hashtable ();
					AppDomain.CurrentDomain.SetData ("TemplateControl.RES_STRING", table);
				}
				return table;
			}
		}

		private string CachedString (string filename, int offset, int size)
		{
			string key = filename + offset + size;
			lock (hashTableMutex) {
				string strObj = (string) ResourceHash [key];
				if (strObj == null) {

					char [] tmp = System.Text.Encoding.UTF8.GetChars (GetResourceBytes (this.GetType ()), offset, size);
					strObj = new string (tmp);
					ResourceHash.Add (key, strObj);
				}

				return strObj;
			}

		}
		public virtual string TemplateSourceDirectory_Private
		{
			get { return null; }
		}

		[MonoTODO]
		// This shouldnt be there, Page.TemplateSourceDirectory must know to get 
		// the right directory of the control.
		public override string TemplateSourceDirectory
		{
			get
			{
#if NET_2_0
				if (this is MasterPage)
					// because MasterPage also has implementation of this property,
					// but not always gets the right directory, in case where master page
					// is in the root of webapp and the page that uses it is in sub folder.
					return base.TemplateSourceDirectory;
#endif
				int location = 0;
				if (_templateSourceDir == null) {
					string tempSrcDir = AppRelativeTemplateSourceDirectory;
					if (tempSrcDir == null && Parent != null)
						tempSrcDir = Parent.TemplateSourceDirectory;
					if (tempSrcDir != null && tempSrcDir.Length > 1) {
						location = tempSrcDir.IndexOf ('/', 1);
						if (location != -1)
							tempSrcDir = tempSrcDir.Substring (location + 1);
						else
							tempSrcDir = string.Empty;
					}
					string answer = HttpRuntime.AppDomainAppVirtualPath;
					if (tempSrcDir == null)
						tempSrcDir = "";

					if (tempSrcDir.Length > 0 && tempSrcDir [tempSrcDir.Length - 1] == '/')
						tempSrcDir = tempSrcDir.Substring (0, tempSrcDir.Length - 1);

					if (tempSrcDir.StartsWith ("/") || tempSrcDir.Length == 0)
						_templateSourceDir = answer + tempSrcDir;
					else
						_templateSourceDir = answer + "/" + tempSrcDir;
				}
				return _templateSourceDir;
			}
		}


		#region Constructor
		protected TemplateControl ()
		{
			Construct ();
		}

		#endregion

		#region Properties
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected virtual int AutoHandlers
		{
			get { return 0; }
			set { }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected virtual bool SupportAutoEvents
		{
			get { return true; }
		}

		#endregion

		#region Methods

		protected virtual void Construct ()
		{
		}

		[MonoTODO]
		protected LiteralControl CreateResourceBasedLiteralControl (int offset,
											int size,
											bool fAsciiOnly)
		{
			string str = CachedString (this.GetType ().FullName, offset, size);
			return new LiteralControl (str);
		}

		internal void WireupAutomaticEvents ()
		{
			if (!SupportAutoEvents || !AutoEventWireup)
				return;

			Type type = GetType ();
			foreach (string methodName in methodNames) {
				MethodInfo method = type.GetMethod (methodName, bflags);
				if (method == null)
					continue;

#if ONLY_1_1
				if (method.DeclaringType != type) {
					if (!method.IsPublic && !method.IsFamilyOrAssembly &&
					    !method.IsFamilyAndAssembly && !method.IsFamily)
						continue;
				}
#endif

				if (method.ReturnType != typeof (void))
					continue;

				ParameterInfo [] parms = method.GetParameters ();
				int length = parms.Length;
				bool noParams = (length == 0);
				if (!noParams && (length != 2 ||
					parms [0].ParameterType != typeof (object) ||
					parms [1].ParameterType != typeof (EventArgs)))
					continue;

				int pos = methodName.IndexOf ("_");
				string eventName = methodName.Substring (pos + 1);
				EventInfo evt = type.GetEvent (eventName);
				if (evt == null) {
					/* This should never happen */
					continue;
				}

				if (noParams) {
					NoParamsInvoker npi = new NoParamsInvoker (this, methodName);
					evt.AddEventHandler (this, npi.FakeDelegate);
				}
				else {
					evt.AddEventHandler (this, Delegate.CreateDelegate (
							typeof (EventHandler), this, methodName));
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected virtual void FrameworkInitialize ()
		{
		}

		Type GetTypeFromControlPath (string virtualPath)
		{
			if (virtualPath == null)
				throw new ArgumentNullException ("virtualPath");

			string vpath = UrlUtils.Combine (TemplateSourceDirectory, virtualPath);
			return PageMapper.GetObjectType (vpath);
		}

		public Control LoadControl (string virtualPath)
		{
			object control = Activator.CreateInstance (GetTypeFromControlPath (virtualPath));
			if (control is UserControl)
				((UserControl) control).InitializeAsUserControl (Page);

			return (Control) control;
		}

		public ITemplate LoadTemplate (string virtualPath)
		{
			Type t = GetTypeFromControlPath (virtualPath);
			return new SimpleTemplate (t);
		}

		protected virtual void OnAbortTransaction (EventArgs e)
		{
			EventHandler eh = Events [abortTransaction] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCommitTransaction (EventArgs e)
		{
			EventHandler eh = Events [commitTransaction] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnError (EventArgs e)
		{
			EventHandler eh = Events [error] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		[MonoTODO]
		public Control ParseControl (string content)
		{
			return null;
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public static object ReadStringResource (Type t)
		{
			return t;
		}
#if NET_2_0
		[MonoTODO ("is this correct?")]
		public Object ReadStringResource ()
		{
			return this.GetType ();
		}
#endif
		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected void SetStringResourcePointer (object stringResourcePointer,
							 int maxResourceOffset)
		{
			if (GetResourceBytes (this.GetType ()) != null)
				return;

			java.lang.Class c = vmw.common.TypeUtils.ToClass (stringResourcePointer);
			java.lang.ClassLoader contextClassLoader = c.getClassLoader ();

			//TODO:move this code to page mapper
			string assemblyName = PageMapper.GetAssemblyResource (this.AppRelativeVirtualPath);

			java.io.InputStream inputStream = contextClassLoader.getResourceAsStream (assemblyName);

			System.IO.Stream strim = null;
			if (inputStream == null) {
				string descPath = String.Join ("/", new string [] { "assemblies", this.GetType ().Assembly.GetName ().Name, assemblyName });
				try {
					strim = new StreamReader (HttpContext.Current.Request.MapPath ("/" + descPath)).BaseStream;
				}
				catch (Exception ex) {
					throw new System.IO.IOException ("couldn't open resource file:" + assemblyName, ex);
				}
				if (strim == null)
					throw new System.IO.IOException ("couldn't open resource file:" + assemblyName);
			}

			try {
				if (strim == null)
					strim = (System.IO.Stream) vmw.common.IOUtils.getStream (inputStream);
				int capacity = (int) strim.Length;
				byte [] resourceBytes = new byte [capacity];
				strim.Read (resourceBytes, 0, capacity);
				SetResourceBytes (this.GetType (), resourceBytes);
			}
			catch (Exception e) {
				throw new HttpException ("problem with dll.ghres file", e);
			}
			finally {
				if (strim != null)
					strim.Close ();
				if (inputStream != null)
					inputStream.close ();
			}
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected void WriteUTF8ResourceString (HtmlTextWriter output, int offset,
							int size, bool fAsciiOnly)
		{
			string str = CachedString (this.GetType ().FullName, offset, size);
			output.Write (str);
		}

		#endregion

		#region Events

		[WebSysDescription ("Raised when the user aborts a transaction.")]
		public event EventHandler AbortTransaction
		{
			add { Events.AddHandler (abortTransaction, value); }
			remove { Events.RemoveHandler (abortTransaction, value); }
		}

		[WebSysDescription ("Raised when the user initiates a transaction.")]
		public event EventHandler CommitTransaction
		{
			add { Events.AddHandler (commitTransaction, value); }
			remove { Events.RemoveHandler (commitTransaction, value); }
		}

		[WebSysDescription ("Raised when an exception occurs that cannot be handled.")]
		public event EventHandler Error
		{
			add { Events.AddHandler (error, value); }
			remove { Events.RemoveHandler (error, value); }
		}

		#endregion

		class SimpleTemplate : ITemplate
		{
			Type type;

			public SimpleTemplate (Type type)
			{
				this.type = type;
			}

			public void InstantiateIn (Control control)
			{
				Control template = Activator.CreateInstance (type) as Control;
				template.SetBindingContainer (false);
				control.Controls.Add (template);
			}
		}

#if NET_2_0

		string _appRelativeVirtualPath = null;

		public string AppRelativeVirtualPath
		{
			get { return _appRelativeVirtualPath; }
			set
			{
				if (value == null)
					throw new ArgumentNullException ("value");
				if (!UrlUtils.IsRooted (value) && !(value.Length > 0 && value [0] == '~'))
					throw new ArgumentException ("The path that is set is not rooted");
				_appRelativeVirtualPath = value;

				int lastSlash = _appRelativeVirtualPath.LastIndexOf ('/');
				AppRelativeTemplateSourceDirectory = (lastSlash > 0) ? _appRelativeVirtualPath.Substring (0, lastSlash + 1) : "~/";
			}
		}

		protected internal object Eval (string expression)
		{
			return DataBinder.Eval (Page.GetDataItem (), expression);
		}

		protected internal string Eval (string expression, string format)
		{
			return DataBinder.Eval (Page.GetDataItem (), expression, format);
		}

		protected internal object XPath (string xpathexpression)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression);
		}

		protected internal object XPath (string xpathexpression, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, null, resolver);
		}

		protected internal string XPath (string xpathexpression, string format)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, format);
		}

		protected internal string XPath (string xpathexpression, string format, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Eval (Page.GetDataItem (), xpathexpression, format, resolver);
		}

		protected internal IEnumerable XPathSelect (string xpathexpression)
		{
			return XPathBinder.Select (Page.GetDataItem (), xpathexpression);
		}

		protected internal IEnumerable XPathSelect (string xpathexpression, IXmlNamespaceResolver resolver)
		{
			return XPathBinder.Select (Page.GetDataItem (), xpathexpression, resolver);
		}

		protected object GetGlobalResourceObject (string className, string resourceKey)
		{
			return HttpContext.GetGlobalResourceObject (className, resourceKey);
		}

		[MonoTODO ("Not implemented")]
		protected object GetGlobalResourceObject (string className, string resourceKey, Type objType, string propName)
		{
			// FIXME: not sure how to implement that one yet
			throw new NotSupportedException ();
		}

		protected Object GetLocalResourceObject (string resourceKey)
		{
			return HttpContext.GetLocalResourceObject (Context.Request.Path, resourceKey);
		}

		[MonoTODO ("Not implemented")]
		protected Object GetLocalResourceObject (string resourceKey, Type objType, string propName)
		{
			// FIXME: not sure how to implement that one yet
			throw new NotSupportedException ();
		}

#endif

	}
}
