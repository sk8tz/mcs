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
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Util;

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
	private bool renderingForm;
	private object _savedViewState;
	private ArrayList _requiresPostBack;
	private ArrayList requiresPostDataChanged;
	private ArrayList requiresRaiseEvent;
	private NameValueCollection secondPostData;
	private bool requiresPostBackScript = false;
	private bool postBackScriptRendered = false;

	#region Fields
	 	protected const string postEventArgumentID = ""; //FIXME
		protected const string postEventSourceID = "";
	#endregion

	#region Constructor
	public Page ()
	{
		Page = this;
	}

	#endregion		

	#region Properties

	public HttpApplicationState Application
	{
		get { return _context.Application; }
	}

	bool AspCompatMode
	{
		set { throw new NotImplementedException (); }
	}

	bool Buffer
	{
		set { Response.BufferOutput = value; }
	}

	public Cache Cache
	{
		get { return _context.Cache; }
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

	ArrayList FileDependencies
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
		get {
			return (0 == String.Compare (Request.HttpMethod, "POST", true));
		}
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
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}

	public HttpServerUtility Server
	{
		get { return _server; }
	}

	public virtual HttpSessionState Session
	{
		get { return _context.Session; }
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
		get { 
			if (_validators == null)
				_validators = new ValidatorCollection ();
			return _validators;
		}
	}

	public override bool Visible
	{
		get { return base.Visible; }
		set { base.Visible = value; }
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

	protected virtual NameValueCollection DeterminePostBackMode ()
	{
		if (_context == null)
			return null;

		HttpRequest req = _context.Request;
		if (req == null)
			return null;

		NameValueCollection coll = null;
		if (IsPostBack)
			//coll =  _context.Request.Form; FIXME: the correct is this one. commented out to let xsp work
			coll = _context.Request.QueryString;
		else 
			coll = _context.Request.QueryString;

		if (coll == null || coll ["__VIEWSTATE"] == null)
			return null;
		return coll;
	}
	
	[MonoTODO]
	public string GetPostBackClientEvent (Control control, string argument)
	{
		// Don't throw the exception. keep going
		//throw new NotImplementedException ();
		StringBuilder result = new StringBuilder ();
		result.AppendFormat ("GetPostBackClientEvent ('{0}', '{1}')", control.ID, argument);
		return result.ToString ();
	}

	public string GetPostBackClientHyperlink (Control control, string argument)
	{
		return "javascript:" + GetPostBackEventReference (control, argument);
	}

	public string GetPostBackEventReference (Control control)
	{
		return GetPostBackEventReference (control, "");
	}
	
	public string GetPostBackEventReference (Control control, string argument)
	{
		RequiresPostBackScript ();
		return String.Format ("__doPostBack ('{0}', '{1}')", control.ID, argument);
	}

	internal void RequiresPostBackScript ()
	{
		requiresPostBackScript = true;
	}

	public virtual int GetTypeHashCode ()
	{
		return GetHashCode ();
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
	public string MapPath (string virtualPath)
	{
		throw new NotImplementedException ();
	}
	
	MethodInfo [] autoEventsMethods = null;

	private void InvokeEventMethod (string m_name, object sender, EventArgs e)
	{
		if (autoEventsMethods == null) {
			BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic |
					  BindingFlags.DeclaredOnly | BindingFlags.Instance;
			
			MethodInfo [] m1 = GetType ().GetMethods (bf);
			bf = BindingFlags.Public | BindingFlags.NonPublic |
			     BindingFlags.DeclaredOnly | BindingFlags.Static;

			MethodInfo [] m2 = GetType ().GetMethods (bf);
			autoEventsMethods = new MethodInfo [m1.Length + m2.Length];
			m1.CopyTo (autoEventsMethods, 0);
			m2.CopyTo (autoEventsMethods, m1.Length);
		}

		foreach (MethodInfo m in autoEventsMethods) {
			if (m.ReturnType != typeof (void))
				continue;

			if (m.Name != m_name)
				continue;

			ParameterInfo [] pi = m.GetParameters ();
			if (pi.Length != 2)
				continue;

			if (pi [0].ParameterType != typeof (object) ||
			    pi [1].ParameterType != typeof (EventArgs))
				continue;

			object [] parms = new object [2];
			parms [0] = sender;
			parms [1] = e;
			m.Invoke (this, parms);
			break;
		}
	}

	private void RenderPostBackScript (HtmlTextWriter writer, string formUniqueID)
	{
		writer.WriteLine ("<input type=\"hidden\" name=\"__EVENTTARGET\" value=\"\" />");
		writer.WriteLine ("<input type=\"hidden\" name=\"__EVENTARGUMENT\" value=\"\" />");
		writer.WriteLine ();
		writer.WriteLine ("<script language=\"javascript\">");
		writer.WriteLine ("<!--");
		writer.WriteLine ("\tfunction __doPostBack(eventTarget, eventArgument) {");
		writer.WriteLine ("\t\tvar theform = document.{0};", formUniqueID);
		writer.WriteLine ("\t\ttheform.__EVENTTARGET.value = eventTarget;");
		writer.WriteLine ("\t\ttheform.__EVENTARGUMENT.value = eventArgument;");
		writer.WriteLine ("\t\ttheform.submit();");
		writer.WriteLine ("\t}");
		writer.WriteLine ("// -->");
		writer.WriteLine ("</script>");
	}

	private bool got_state = false;
	private int _random;
	private int queryStringHash;
	internal void OnFormRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (renderingForm)
			throw new HttpException ("Only 1 HtmlForm is allowed per page.");

		renderingForm = true;
		writer.WriteLine ();
		writer.Write ("<input type=\"hidden\" name=\"__VIEWSTATE\" ");
		writer.WriteLine ("value=\"{0}\" />", GetViewStateString ());
		if (requiresPostBackScript) {
			RenderPostBackScript (writer, formUniqueID);
			postBackScriptRendered = true;
		}
	}

	public string GetViewStateString ()
	{
		StringBuilder state_string = new StringBuilder ();
		state_string.AppendFormat ("{0:X}", GetTypeHashCode ());
		state_string.AppendFormat ("{0:X}", queryStringHash);

		if (!got_state) {
			Random rnd = new Random ();
			_random = rnd.Next ();
			if (_random < 0)
				_random = -_random;
			_random++;
			got_state = true;
		}

		state_string.AppendFormat ("{0:X}", _random);
		return state_string.ToString ();
	}

	internal void OnFormPostRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (!postBackScriptRendered && requiresPostBackScript)
			RenderPostBackScript (writer, formUniqueID);

		renderingForm = false;
		postBackScriptRendered = false;
	}


	private void _Page_Init (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Init", sender, e);
	}

	private void _Page_Load (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Load", sender, e);
	}

	private void _Page_DataBind (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_DataBind", sender, e);
	}

	private void _Page_PreRender (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_PreRender", sender, e);
	}

	private void _Page_Dispose (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Dispose", sender, e);
	}

	private void _Page_Error (object sender, EventArgs e)
	{
		InvokeEventMethod ("Page_Error", sender, e);
	}

	private void ProcessPostData (NameValueCollection data, bool second)
	{
		if (data == null)
			return;

		foreach (string id in data.AllKeys){
			if (id == "__VIEWSTATE")
				continue;

			Control ctrl = FindControl (id);
			if (ctrl != null){
				IPostBackDataHandler pbdh = ctrl as IPostBackDataHandler;
				IPostBackEventHandler pbeh = ctrl as IPostBackEventHandler;

				if (pbdh == null) {
					if (pbeh != null)
						RegisterRequiresRaiseEvent (pbeh);
					continue;
				}
		
				if (pbdh.LoadPostData (id, data) == true) {
					if (requiresPostDataChanged == null)
						requiresPostDataChanged = new ArrayList ();
					requiresPostDataChanged.Add (pbdh);
				}
			} else if (!second) {
				if (secondPostData == null)
					secondPostData = new NameValueCollection ();
				secondPostData.Add (id, null);
			}
		}
	}

	private bool init_done;
	public void ProcessRequest (HttpContext context)
	{
		_context = context;
		WebTrace.PushContext ("Page.ProcessRequest ()");
		WebTrace.WriteLine ("Entering");
		if (!init_done){
			init_done = true;
			// These should depend on AutoEventWireUp in Page directive. Defaults to true.
			Init += new EventHandler (_Page_Init);
			Load += new EventHandler (_Page_Load);
			DataBinding += new EventHandler (_Page_DataBind);
			PreRender += new EventHandler (_Page_PreRender);
			Disposed += new EventHandler (_Page_Dispose);
			Error += new EventHandler (_Page_Error);
			WebTrace.WriteLine ("Finished init");
		}
		//-- Control execution lifecycle in the docs
		WebTrace.WriteLine ("Controls.Clear");
		Controls.Clear ();
		WebTrace.WriteLine ("FrameworkInitialize");
		FrameworkInitialize ();
		WebTrace.WriteLine ("InitRecursive");
		InitRecursive (null);
		got_state = false;
		renderingForm = false;	
		_context = context;
		queryStringHash = _context.Request.QueryString.GetHashCode ();
		if (IsPostBack) {
			LoadPageViewState ();
			ProcessPostData (DeterminePostBackMode (), false);
		}

		WebTrace.WriteLine ("LoadRecursive");
		LoadRecursive ();
		if (IsPostBack) {
			ProcessPostData (secondPostData, true);
			RaiseChangedEvents ();
			RaisePostBackEvents ();
		}
		WebTrace.WriteLine ("PreRenderRecursiveInternal");
		PreRenderRecursiveInternal ();

		WebTrace.WriteLine ("SavePageViewState");
		SavePageViewState ();
		//--
		StringBuilder sb = new StringBuilder ();
		StringWriter sr = new StringWriter (sb);
		HtmlTextWriter output = new HtmlTextWriter (context.Response.Output);
		WebTrace.WriteLine ("RenderControl");
		RenderControl (output);
		_context = null;
		WebTrace.WriteLine ("End");
		WebTrace.PopContext ();
	}

	internal void RaisePostBackEvents ()
	{
		NameValueCollection postdata = DeterminePostBackMode ();
		string eventTarget = postdata ["__EVENTTARGET"];
		if (eventTarget != null && eventTarget.Length > 0) {
			Control target = FindControl (eventTarget);
			if (!(target is IPostBackEventHandler))
				return;
			string eventArgument = postdata ["__EVENTARGUMENT"];
			RaisePostBackEvent ((IPostBackEventHandler) target, eventArgument);
			return;
		}

		if (requiresRaiseEvent == null)
			return;

		foreach (Control c in requiresRaiseEvent)
			RaisePostBackEvent ((IPostBackEventHandler) c, postdata [c.ID]);
		requiresRaiseEvent.Clear ();
	}

	internal void RaiseChangedEvents ()
	{
		if (requiresPostDataChanged == null)
			return;

		foreach (IPostBackDataHandler ipdh in requiresPostDataChanged)
			ipdh.RaisePostDataChangedEvent ();
		requiresPostDataChanged.Clear ();
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
	
	public void RegisterRequiresPostBack (Control control)
	{
		if (_requiresPostBack == null)
			_requiresPostBack = new ArrayList ();

		_requiresPostBack.Add (control);
	}

	public virtual void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
	{
		if (requiresRaiseEvent == null)
			requiresRaiseEvent = new ArrayList ();
		requiresRaiseEvent.Add (control);
	}

	[MonoTODO]
	public void RegisterViewStateHandler ()
	{
		// Do nothing
	}

	protected virtual void SavePageStateToPersistenceMedium (object viewState)
	{
		_savedViewState = viewState;
	}
	
	protected virtual object LoadPageStateFromPersistenceMedium ()
	{
		return _savedViewState;
	}

	internal void LoadPageViewState()
	{
		object sState = LoadPageStateFromPersistenceMedium ();
		if (sState != null) {
			Pair pair = (Pair) sState;
			LoadViewStateRecursive (pair.First);
			_requiresPostBack = pair.Second as ArrayList;
		}
	}

	internal void SavePageViewState ()
	{
		Pair pair = new Pair ();
		pair.First = SaveViewStateRecursive ();
		pair.Second = _requiresPostBack;
		SavePageStateToPersistenceMedium (pair);
	}

	public virtual void Validate ()
	{
		if (_validators == null || _validators.Count == 0){
			_isValid = true;
			return;
		}

		bool all_valid = true;
		foreach (IValidator v in _validators){
			v.Validate ();
			if (v.IsValid == false)
				all_valid = false;
		}

		if (all_valid)
			_isValid = true;
	}

	public virtual void VerifyRenderingInServerForm (Control control)
	{
		if (!renderingForm)
			throw new HttpException ("Control '" + control.ClientID + " " + control.GetType () + 
						 "' must be rendered within a HtmlForm");
	}

	#endregion
}
}
