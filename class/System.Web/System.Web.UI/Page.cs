//
// System.Web.UI.Page
//
// Authors:
//	Duncan Mak  (duncan@ximian.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

namespace System.Web.UI
{

public class Page : TemplateControl, IHttpHandler
{
	private string _culture;
	private bool _viewState = true;
	private bool _viewStateMac = false;
	private string _errorPage;
	private ArrayList _fileDependencies;
	private string _ID;
	private bool _isPostBack;
	private bool _isValid;
	private HttpServerUtility _server;
	private bool _smartNavigation = false;
	private TraceContext _trace;
	private bool _traceEnabled;
	private TraceMode _traceModeValue;
	private int _transactionMode;
	private string _UICulture;
	private HttpContext _context;
	private ValidatorCollection _validators;
	private bool _visible;
	private string _responseEncoding;
	private HttpSessionState _session;

	#region Fields
	 	protected const string postEventArgumentID = ""; //FIXME
		protected const string postEventSourceID = "";
	#endregion

	#region Constructor
	public Page ()
	{
	}

	#endregion		

	#region Properties

	[MonoTODO]
	public HttpApplicationState Application
	{
		get { throw new NotImplementedException (); }
	}

	bool AspCompatMode
	{
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	bool Buffer
	{
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public Cache Cache
	{
		get { throw new NotImplementedException (); }
	}

	[MonoTODO]
	public string ClientTarget
	{
		get { throw new NotImplementedException (); }
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	int CodePage
	{
		set { throw new NotImplementedException (); }
	}

	[MonoTODO]
	string ContentType
	{
		set { throw new NotImplementedException (); }
	}

	protected override HttpContext Context
	{
		get { return _context; }
	}

	string Culture
	{
		set { _culture = value; }
	}

	public override bool EnableViewState
	{
		get { return _viewState; }
		set { _viewState = value; }
	}

	protected bool EnableViewStateMac
	{
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}

	public string ErrorPage
	{
		get { return _errorPage; }
		set { _errorPage = value; }
	}

	public ArrayList FileDependencies
	{
		set { _fileDependencies = value; }
	}

	public override string ID
	{
		get { return _ID; }
		set { _ID = value; }
	}

	public bool IsPostBack
	{
		get { return _isPostBack; }
	}

	[MonoTODO]
	public bool IsReusable
	{
		get { throw new NotImplementedException (); }
	}

	public bool IsValid
	{
		get { return _isValid; }
	}

	[MonoTODO]
	int LCID {
		set { throw new NotImplementedException (); }
	}

	public HttpRequest Request
	{
		get { return _context.Request; }
	}

	public HttpResponse Response
	{
		get { return _context.Response; }
	}

	string ResponseEncoding
	{
		set { _responseEncoding = value; }
	}

	public HttpServerUtility Server
	{
		get { return _server; }
	}

	public virtual HttpSessionState Session
	{
		get { return _session; }
	}

	public bool SmartNavigation
	{
		get { return _smartNavigation; }
		set { _smartNavigation = value; }
	}

	public TraceContext Trace
	{
		get { return _trace; }
	}

	bool TraceEnabled
	{
		set { _traceEnabled = value; }
	}

	TraceMode TraceModeValue
	{
		set { _traceModeValue = value; }
	}

	int TransactionMode
	{
		set { _transactionMode = value; }
	}

	string UICulture
	{
		set { _UICulture = value; }
	}

	public IPrincipal User
	{
		get { return _context.User; }
	}

	public ValidatorCollection Validators
	{
		get { return _validators; }
	}

	public override bool Visible
	{
		get { return _visible; }
		set { _visible = value; }
	}

	#endregion

	#region Methods

	[MonoTODO]
	protected IAsyncResult AspCompatBeginProcessRequest (HttpContext context,
							     AsyncCallback cb, 
							     object extraData)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	protected void AspcompatEndProcessRequest (IAsyncResult result)
	{
		throw new NotImplementedException ();
	}
	
	protected virtual HtmlTextWriter CreateHtmlTextWriter (TextWriter tw)
	{
		return new HtmlTextWriter (tw);
	}

	[MonoTODO]
	public void DesignerInitialize ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	protected virtual NameValueCollection DeterminePostBackMode ()
	{
		/* Why does this not compile? HttpSessionState has IsNewSession...
		if (_session.IsNewSession)
			return null;
		*/
		
		if (IsPostBack)
			return _context.Request.Form; //FIXME: is this enough?

		throw new NotImplementedException ("GET method got to parse Request.QueryString");
	}
	
	[MonoTODO]
	public string GetPostBackClientEvent (Control control, string argument)
	{
		// Don't throw the exception. keep going
		//throw new NotImplementedException ();
		return "";
	}

	[MonoTODO]
	public string GetPostBackClientHyperlink (Control control, string argument)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string GetPostBackEventReference (Control control)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public string GetPostBackEventReference (Control control, string argument)
	{
		throw new NotImplementedException ();
	}

	public virtual int GetTypeHashCode ()
	{
		return this.GetHashCode ();
	}

	[MonoTODO]
	protected virtual void InitOutputCache (int duration,
						string varyByHeader,
						string varyByCustom,
						OutputCacheLocation location,
						string varyByParam)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public bool IsClientScriptBlockRegistered (string key)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public bool IsStartupScriptRegistered (string key)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	protected virtual object LoadPageStateFromPersistenceMedium ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public string MapPath (string virtualPath)
	{
		throw new NotImplementedException ();
	}
	
	private void InvokeEventMethod (string m_name, object sender, EventArgs e)
	{
		MethodInfo evt_method = GetType ().GetMethod(m_name,
						BindingFlags.IgnoreCase | BindingFlags.Public |
						BindingFlags.NonPublic | BindingFlags.DeclaredOnly |
						BindingFlags.Instance);

		if (evt_method != null){
			object [] parms = new object [2];
			parms [0] = sender;
			parms [1] = e;
			evt_method.Invoke (this, parms);
		}
	}

	private void Page_Init (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Init", sender, e);
	}

	private void Page_Load (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Load", sender, e);
	}

	private int defaultNumberID;
	private void SetDefaults (ControlCollection col)
	{
		if (col.Count == 0)
			return;
		foreach (Control ctrl in col){
			/*
			 * Assing a default ID for controls that don't have one.
			 * This can happen for programatically added controls (see web_placeholder.aspx).
			 */
			if (ctrl.ID == null)
				ctrl.ID = "_ctrl_" + defaultNumberID++;
			//
			ctrl.Page = this;
			SetDefaults (ctrl.Controls);
		}
	}

	public void ProcessRequest (HttpContext context)
	{
		FrameworkInitialize ();
		// This 2 should depend on AutoEventWireUp in Page directive. Defaults to true.
		Init += new EventHandler (Page_Init);
		Load += new EventHandler (Page_Load);

		//-- Control execution lifecycle in the docs
		OnInit (EventArgs.Empty);
		//LoadViewState ();
		//if (this is IPostBackDataHandler)
		//	LoadPostData ();
		OnLoad (EventArgs.Empty);
		//if (this is IPostBackDataHandler)
		//	RaisePostBackEvent ();
		OnPreRender (EventArgs.Empty);

		//--
		_context = context;
		HtmlTextWriter output = new HtmlTextWriter (context.Response.Output);
		
		foreach (Control ctrl in Controls){
			// Assing Control.Page here. Controls should do the same before 
			// rendering their children
			ctrl.Page = this;
			SetDefaults (ctrl.Controls);
			//
			ctrl.RenderControl (output);
		}
	}

	protected virtual void RaisePostBackEvent (IPostBackEventHandler sourceControl, string eventArgument)
	{
		sourceControl.RaisePostBackEvent (eventArgument);
	}
	
	[MonoTODO]
	public void RegisterArrayDeclaration (string arrayName, string arrayValue)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public virtual void RegisterClientScriptBlock (string key, string script)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public virtual void RegisterHiddenField (string hiddenFieldName, string hiddenFieldInitialValue)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public void RegisterClientScriptFile (string a, string b, string c)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public void RegisterOnSubmitStatement (string key, string script)
	{
		throw new NotImplementedException ();
	}
	
	[MonoTODO]
	public void RegisterRequiresPostBack (Control control)
	{
		// Don't throw the exception. keep going
		//throw new NotImplementedException ();
	}

	[MonoTODO]
	public virtual void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	public void RegisterViewStateHandler ()
	{
		throw new NotImplementedException ();
	}

	[MonoTODO]
	protected virtual void SavePageStatetoPersistenceMedium (object viewState)
	{
		throw new NotImplementedException ();
	}
	
	public virtual void Validate ()
	{
		bool all_valid = true;
		foreach (IValidator v in _validators){
			v.Validate ();
			if (v.IsValid == false)
				all_valid = false;
		}

		if (all_valid)
			_isValid = true;
	}

	[MonoTODO]
	public virtual void VerifyRenderingInServerForm (Control control)
	{
		return;
	}

	#endregion
}
}
