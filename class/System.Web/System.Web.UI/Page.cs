//
// System.Web.UI.Page.cs
//
// Authors:
//   Duncan Mak  (duncan@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2003 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Util;

namespace System.Web.UI
{

[MonoTODO ("FIXME missing the IRootDesigner Attribute")]
[DefaultEvent ("Load"), DesignerCategory ("ASPXCodeBehind")]
[ToolboxItem (false)]
[Designer ("System.Web.UI.Design.ControlDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
[RootDesignerSerializer ("Microsoft.VSDesigner.WebForms.RootCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design, true)]
public class Page : TemplateControl, IHttpHandler
{
	private bool _viewState = true;
	private bool _viewStateMac;
	private string _errorPage;
	private bool _isValid;
	private bool _smartNavigation;
	private int _transactionMode;
	private HttpContext _context;
	private ValidatorCollection _validators;
	private bool renderingForm;
	private object _savedViewState;
	private ArrayList _requiresPostBack;
	private ArrayList _requiresPostBackCopy;
	private ArrayList requiresPostDataChanged;
	private IPostBackEventHandler requiresRaiseEvent;
	private NameValueCollection secondPostData;
	private bool requiresPostBackScript;
	private bool postBackScriptRendered;
	private Hashtable registeredArrayDeclares;
	Hashtable clientScriptBlocks;
	Hashtable startupScriptBlocks;
	Hashtable hiddenFields;
	internal Hashtable submitStatements;
	bool handleViewState;
	string viewStateUserKey;
	NameValueCollection _requestValueCollection;
	string clientTarget;

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected const string postEventArgumentID = "__EVENTARGUMENT";
	[EditorBrowsable (EditorBrowsableState.Never)]
	protected const string postEventSourceID = "__EVENTTARGET";

	#region Constructor
	public Page ()
	{
		Page = this;
	}

	#endregion		

	#region Properties

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpApplicationState Application
	{
		get { return _context.Application; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool AspCompatMode
	{
		set { throw new NotImplementedException (); }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool Buffer
	{
		set { Response.BufferOutput = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public Cache Cache
	{
		get { return _context.Cache; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false), DefaultValue ("")]
	[WebSysDescription ("Value do override the automatic browser detection and force the page to use the specified browser.")]
	public string ClientTarget
	{
		get { return (clientTarget == null) ? "" : clientTarget; }
		set {
			clientTarget = value;
			if (value == "")
				clientTarget = null;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int CodePage
	{
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string ContentType
	{
		set { Response.ContentType = value; }
	}

	protected override HttpContext Context
	{
		get {
			if (_context == null)
				return HttpContext.Current;

			return _context;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string Culture
	{
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
	}

	[Browsable (false)]
	public override bool EnableViewState
	{
		get { return _viewState; }
		set { _viewState = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool EnableViewStateMac
	{
		get { return _viewStateMac; }
		set { _viewStateMac = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false), DefaultValue ("")]
	[WebSysDescription ("The URL of a page used for error redirection.")]
	public string ErrorPage
	{
		get { return _errorPage; }
		set {
			_errorPage = value;
			if (_context != null)
				_context.ErrorPage = value;
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected ArrayList FileDependencies
	{
		set {
			if (Response != null)
				Response.AddFileDependencies (value);
		}
	}

	[Browsable (false)]
	public override string ID
	{
		get { return base.ID; }
		set { base.ID = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public bool IsPostBack
	{
		get {
			return (0 == String.Compare (Request.HttpMethod, "POST", true));
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never), Browsable (false)]
	public bool IsReusable {
		get { return false; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public bool IsValid
	{
		get { return _isValid; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int LCID {
		set { Thread.CurrentThread.CurrentCulture = new CultureInfo (value); }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpRequest Request
	{
		get { return _context.Request; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpResponse Response
	{
		get { return _context.Response; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string ResponseEncoding
	{
		set { Response.ContentEncoding = Encoding.GetEncoding (value); }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public HttpServerUtility Server
	{
		get {
			return Context.Server;
		}
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public virtual HttpSessionState Session
	{
		get {
			if (_context.Session == null)
				throw new HttpException ("Session state can only be used " +
						"when enableSessionState is set to true, either " +
						"in a configuration file or in the Page directive.");

			return _context.Session;
		}
	}

	[Browsable (false)]
	public bool SmartNavigation
	{
		get { return _smartNavigation; }
		set { _smartNavigation = value; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public TraceContext Trace
	{
		get { return Context.Trace; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected bool TraceEnabled
	{
		set { Trace.IsEnabled = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected TraceMode TraceModeValue
	{
		set { Trace.TraceMode = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected int TransactionMode
	{
		set { _transactionMode = value; }
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected string UICulture
	{
		set { Thread.CurrentThread.CurrentUICulture = new CultureInfo (value); }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public IPrincipal User
	{
		get { return _context.User; }
	}

	[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
	[Browsable (false)]
	public ValidatorCollection Validators
	{
		get { 
			if (_validators == null)
				_validators = new ValidatorCollection ();
			return _validators;
		}
	}

	[MonoTODO ("Use this when encrypting/decrypting ViewState")]
	[Browsable (false)]
	public string ViewStateUserKey {
		get { return viewStateUserKey; }
		set { viewStateUserKey = value; }
	}

	[Browsable (false)]
	public override bool Visible
	{
		get { return base.Visible; }
		set { base.Visible = value; }
	}

	#endregion

	#region Methods

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected IAsyncResult AspCompatBeginProcessRequest (HttpContext context,
							     AsyncCallback cb, 
							     object extraData)
	{
		throw new NotImplementedException ();
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected void AspCompatEndProcessRequest (IAsyncResult result)
	{
		throw new NotImplementedException ();
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual HtmlTextWriter CreateHtmlTextWriter (TextWriter tw)
	{
		return new HtmlTextWriter (tw);
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public void DesignerInitialize ()
	{
		InitRecursive (null);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual NameValueCollection DeterminePostBackMode ()
	{
		if (_context == null)
			return null;

		HttpRequest req = _context.Request;
		if (req == null)
			return null;

		NameValueCollection coll = null;
		if (IsPostBack)
			coll =	req.Form;
		else 
			coll = req.QueryString;

		
		if (coll == null || coll ["__VIEWSTATE"] == null)
			return null;

		return coll;
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientEvent (Control control, string argument)
	{
		return GetPostBackEventReference (control, argument);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackClientHyperlink (Control control, string argument)
	{
		return "javascript:" + GetPostBackEventReference (control, argument);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control)
	{
		return GetPostBackEventReference (control, "");
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public string GetPostBackEventReference (Control control, string argument)
	{
		RequiresPostBackScript ();
		return String.Format ("__doPostBack('{0}','{1}')", control.UniqueID, argument);
	}

	internal void RequiresPostBackScript ()
	{
		requiresPostBackScript = true;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public virtual int GetTypeHashCode ()
	{
		return 0;
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	protected virtual void InitOutputCache (int duration,
						string varyByHeader,
						string varyByCustom,
						OutputCacheLocation location,
						string varyByParam)
	{
		HttpCachePolicy cache = _context.Response.Cache;
		bool set_vary = false;

		switch (location) {
		case OutputCacheLocation.Any:
			cache.SetCacheability (HttpCacheability.Public);
			cache.SetMaxAge (new TimeSpan (0, 0, duration));		
			cache.SetLastModified (_context.Timestamp);
			set_vary = true;
			break;
		case OutputCacheLocation.Client:
			cache.SetCacheability (HttpCacheability.Private);
			cache.SetMaxAge (new TimeSpan (0, 0, duration));		
			cache.SetLastModified (_context.Timestamp);
			break;
		case OutputCacheLocation.Downstream:
			cache.SetCacheability (HttpCacheability.Public);
			cache.SetMaxAge (new TimeSpan (0, 0, duration));		
			cache.SetLastModified (_context.Timestamp);
			break;
		case OutputCacheLocation.Server:			
			cache.SetCacheability (HttpCacheability.Server);
			set_vary = true;
			break;
		case OutputCacheLocation.None:
			break;
		}

		if (set_vary) {
			if (varyByCustom != null)
				cache.SetVaryByCustom (varyByCustom);

			if (varyByParam != null && varyByParam.Length > 0) {
				string[] prms = varyByParam.Split (';');
				foreach (string p in prms)
					cache.VaryByParams [p.Trim ()] = true;
				cache.VaryByParams.IgnoreParams = false;
			} else {
				cache.VaryByParams.IgnoreParams = true;
			}
			
			if (varyByHeader != null && varyByHeader.Length > 0) {
				string[] hdrs = varyByHeader.Split (';');
				foreach (string h in hdrs)
					cache.VaryByHeaders [h.Trim ()] = true;
			}
		}
			
		cache.Duration = duration;
		cache.SetExpires (_context.Timestamp.AddSeconds (duration));
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public bool IsClientScriptBlockRegistered (string key)
	{
		if (clientScriptBlocks == null)
			return false;

		return clientScriptBlocks.ContainsKey (key);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public bool IsStartupScriptRegistered (string key)
	{
		if (startupScriptBlocks == null)
			return false;

		return startupScriptBlocks.ContainsKey (key);
	}

	public string MapPath (string virtualPath)
	{
		return Request.MapPath (virtualPath);
	}
	
	private void RenderPostBackScript (HtmlTextWriter writer, string formUniqueID)
	{
		writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" value=\"\" />", postEventSourceID);
		writer.WriteLine ("<input type=\"hidden\" name=\"{0}\" value=\"\" />", postEventArgumentID);
		writer.WriteLine ();
		writer.WriteLine ("<script language=\"javascript\">");
		writer.WriteLine ("<!--");
		writer.WriteLine ("\tfunction __doPostBack(eventTarget, eventArgument) {");
		writer.WriteLine ("\t\tvar theform = document.getElementById ('{0}');", formUniqueID);
		writer.WriteLine ("\t\ttheform.{0}.value = eventTarget;", postEventSourceID);
		writer.WriteLine ("\t\ttheform.{0}.value = eventArgument;", postEventArgumentID);
		writer.WriteLine ("\t\ttheform.submit();");
		writer.WriteLine ("\t}");
		writer.WriteLine ("// -->");
		writer.WriteLine ("</script>");
	}

	static void WriteScripts (HtmlTextWriter writer, Hashtable scripts)
	{
		if (scripts == null)
			return;

		foreach (string key in scripts.Values)
			writer.WriteLine (key);
	}
	
	void WriteHiddenFields (HtmlTextWriter writer)
	{
		if (hiddenFields == null)
			return;

		foreach (string key in hiddenFields.Keys) {
			string value = hiddenFields [key] as string;
			writer.WriteLine ("\n<input type=\"hidden\" name=\"{0}\" value=\"{1}\" />", key, value);
		}

		hiddenFields = null;
	}

	internal void OnFormRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (renderingForm)
			throw new HttpException ("Only 1 HtmlForm is allowed per page.");

		renderingForm = true;
		writer.WriteLine ();
		WriteHiddenFields (writer);
		if (requiresPostBackScript) {
			RenderPostBackScript (writer, formUniqueID);
			postBackScriptRendered = true;
		}

		if (handleViewState) {
			string vs = GetViewStateString ();
			writer.Write ("<input type=\"hidden\" name=\"__VIEWSTATE\" ");
			writer.WriteLine ("value=\"{0}\" />", vs);
		}

		WriteScripts (writer, clientScriptBlocks);
	}

	internal string GetViewStateString ()
	{
		StringWriter sr = new StringWriter ();
		LosFormatter fmt = new LosFormatter ();
		fmt.Serialize (sr, _savedViewState);
		return sr.GetStringBuilder ().ToString ();
	}

	internal void OnFormPostRender (HtmlTextWriter writer, string formUniqueID)
	{
		if (registeredArrayDeclares != null) {
			writer.WriteLine();
			writer.WriteLine("<script language=\"javascript\">");
			writer.WriteLine("<!--");
			IDictionaryEnumerator arrayEnum = registeredArrayDeclares.GetEnumerator();
			while (arrayEnum.MoveNext()) {
				writer.Write("\tvar ");
				writer.Write(arrayEnum.Key);
				writer.Write(" =  new Array(");
				IEnumerator arrayListEnum = ((ArrayList) arrayEnum.Value).GetEnumerator();
				bool isFirst = true;
				while (arrayListEnum.MoveNext()) {
					if (isFirst)
						isFirst = false;
					else
						writer.Write(", ");
					writer.Write(arrayListEnum.Current);
				}
				writer.WriteLine(");");
			}
			writer.WriteLine("// -->");
			writer.WriteLine("</script>");
			writer.WriteLine();
		}

		if (!postBackScriptRendered && requiresPostBackScript)
			RenderPostBackScript (writer, formUniqueID);

		WriteHiddenFields (writer);
		WriteScripts (writer, startupScriptBlocks);
		renderingForm = false;
		postBackScriptRendered = false;
	}

	private void ProcessPostData (NameValueCollection data, bool second)
	{
		if (data == null)
			return;

		if (_requiresPostBackCopy == null && _requiresPostBack != null)
			_requiresPostBackCopy = (ArrayList) _requiresPostBack.Clone ();

		Hashtable used = new Hashtable ();
		foreach (string id in data.AllKeys){
			if (id == "__VIEWSTATE" || id == postEventSourceID || id == postEventArgumentID)
				continue;

			string real_id = id;
			int dot = real_id.IndexOf ('.');
			if (dot >= 1)
				real_id = real_id.Substring (0, dot);
			
			if (real_id == null || used.ContainsKey (real_id))
				continue;

			used.Add (real_id, real_id);

			Control ctrl = FindControl (real_id);
			if (ctrl != null){
				IPostBackDataHandler pbdh = ctrl as IPostBackDataHandler;
				IPostBackEventHandler pbeh = ctrl as IPostBackEventHandler;

				if (pbdh == null) {
					if (pbeh != null)
						RegisterRequiresRaiseEvent (pbeh);
					continue;
				}
		
				if (pbdh.LoadPostData (real_id, data) == true) {
					if (requiresPostDataChanged == null)
						requiresPostDataChanged = new ArrayList ();
					requiresPostDataChanged.Add (pbdh);
					if (_requiresPostBackCopy != null)
						_requiresPostBackCopy.Remove (ctrl.UniqueID);
				}
			} else if (!second) {
				if (secondPostData == null)
					secondPostData = new NameValueCollection ();
				secondPostData.Add (real_id, data [id]);
			}
		}

		if (_requiresPostBackCopy != null && _requiresPostBackCopy.Count > 0) {
			string [] handlers = (string []) _requiresPostBackCopy.ToArray (typeof (string));
			foreach (string id in handlers) {
				IPostBackDataHandler pbdh = FindControl (id) as IPostBackDataHandler;
				if (pbdh == null)
					continue;
			
				_requiresPostBackCopy.Remove (id);
				if (pbdh.LoadPostData (id, data)) {
					if (requiresPostDataChanged == null)
						requiresPostDataChanged = new ArrayList ();

					requiresPostDataChanged.Add (pbdh);
				}
			}
		}
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	public void ProcessRequest (HttpContext context)
	{
		_context = context;
		if (clientTarget != null)
			Request.ClientTarget = clientTarget;

		WireupAutomaticEvents ();
		//-- Control execution lifecycle in the docs

		// Save culture information because it can be modified in FrameworkInitialize()
		CultureInfo culture = Thread.CurrentThread.CurrentCulture;
		CultureInfo uiculture = Thread.CurrentThread.CurrentUICulture;
		FrameworkInitialize ();
		context.ErrorPage = _errorPage;

		try {
			InternalProcessRequest ();
		} finally {
			try {
				UnloadRecursive (true);
			} catch {}
			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = uiculture;
		}
	}

	void InternalProcessRequest ()
	{
		_requestValueCollection = this.DeterminePostBackMode();
		Trace.Write ("aspx.page", "Begin Init");
		InitRecursive (null);
		Trace.Write ("aspx.page", "End Init");
	      
		renderingForm = false;	
		if (IsPostBack) {
			Trace.Write ("aspx.page", "Begin LoadViewState");
			LoadPageViewState ();
			Trace.Write ("aspx.page", "End LoadViewState");
			Trace.Write ("aspx.page", "Begin ProcessPostData");
			ProcessPostData (_requestValueCollection, false);
			Trace.Write ("aspx.page", "End ProcessPostData");
		}

		LoadRecursive ();
		if (IsPostBack) {
			Trace.Write ("aspx.page", "Begin ProcessPostData Second Try");
			ProcessPostData (secondPostData, true);
			Trace.Write ("aspx.page", "End ProcessPostData Second Try");
			Trace.Write ("aspx.page", "Begin Raise ChangedEvents");
			RaiseChangedEvents ();
			Trace.Write ("aspx.page", "End Raise ChangedEvents");
			Trace.Write ("aspx.page", "Begin Raise PostBackEvent");
			RaisePostBackEvents ();
			Trace.Write ("aspx.page", "End Raise PostBackEvent");
		}
		Trace.Write ("aspx.page", "Begin PreRender");
		PreRenderRecursiveInternal ();
		Trace.Write ("aspx.page", "End PreRender");

		Trace.Write ("aspx.page", "Begin SaveViewState");
		SavePageViewState ();
		Trace.Write ("aspx.page", "End SaveViewState");
		
		//--
		Trace.Write ("aspx.page", "Begin Render");
		HtmlTextWriter output = new HtmlTextWriter (_context.Response.Output);
		RenderControl (output);
		Trace.Write ("aspx.page", "End Render");
		
		RenderTrace (output);
		_context = null;
	}

	private void RenderTrace (HtmlTextWriter output)
	{
		TraceManager manager = HttpRuntime.TraceManager;
		
		if (!Trace.IsEnabled && !manager.Enabled)
			return;
		
		Trace.SaveData ();
		
		if (Trace.IsEnabled || manager.PageOutput)
			Trace.Render (output);
	}
	
	internal void RaisePostBackEvents ()
	{
		if (requiresRaiseEvent != null) {
			RaisePostBackEvent (requiresRaiseEvent, null);
			return;
		}

		NameValueCollection postdata = _requestValueCollection;
		if (postdata == null)
			return;

		string eventTarget = postdata [postEventSourceID];
		if (eventTarget == null || eventTarget.Length == 0) {
			Validate ();
			return;
                }

		IPostBackEventHandler target = FindControl (eventTarget) as IPostBackEventHandler;
		if (target == null)
			return;

		string eventArgument = postdata [postEventArgumentID];
		RaisePostBackEvent (target, eventArgument);
	}

	internal void RaiseChangedEvents ()
	{
		if (requiresPostDataChanged == null)
			return;

		foreach (IPostBackDataHandler ipdh in requiresPostDataChanged)
			ipdh.RaisePostDataChangedEvent ();

		requiresPostDataChanged = null;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual void RaisePostBackEvent (IPostBackEventHandler sourceControl, string eventArgument)
	{
		sourceControl.RaisePostBackEvent (eventArgument);
	}
	
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterArrayDeclaration (string arrayName, string arrayValue)
	{
		if (registeredArrayDeclares == null)
			registeredArrayDeclares = new Hashtable();

		if (!registeredArrayDeclares.ContainsKey (arrayName))
			registeredArrayDeclares.Add (arrayName, new ArrayList());

		((ArrayList) registeredArrayDeclares[arrayName]).Add(arrayValue);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterClientScriptBlock (string key, string script)
	{
		if (IsClientScriptBlockRegistered (key))
			return;

		if (clientScriptBlocks == null)
			clientScriptBlocks = new Hashtable ();

		clientScriptBlocks.Add (key, script);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterHiddenField (string hiddenFieldName, string hiddenFieldInitialValue)
	{
		if (hiddenFields == null)
			hiddenFields = new Hashtable ();

		if (!hiddenFields.ContainsKey (hiddenFieldName))
			hiddenFields.Add (hiddenFieldName, hiddenFieldInitialValue);
	}

	[MonoTODO("Used in HtmlForm")]
	internal void RegisterClientScriptFile (string a, string b, string c)
	{
		throw new NotImplementedException ();
	}


	[MonoTODO]
	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterOnSubmitStatement (string key, string script)
	{
		if (submitStatements == null)
			submitStatements = new Hashtable ();

		if (submitStatements.ContainsKey (key))
			return;

		submitStatements.Add (key, script);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterRequiresPostBack (Control control)
	{
		if (_requiresPostBack == null)
			_requiresPostBack = new ArrayList ();

		_requiresPostBack.Add (control.UniqueID);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterRequiresRaiseEvent (IPostBackEventHandler control)
	{
		requiresRaiseEvent = control;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void RegisterStartupScript (string key, string script)
	{
		if (IsStartupScriptRegistered (key))
			return;

		if (startupScriptBlocks == null)
			startupScriptBlocks = new Hashtable ();

		startupScriptBlocks.Add (key, script);
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public void RegisterViewStateHandler ()
	{
		handleViewState = true;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual void SavePageStateToPersistenceMedium (object viewState)
	{
		_savedViewState = viewState;
	}

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	protected virtual object LoadPageStateFromPersistenceMedium ()
	{
		NameValueCollection postdata = _requestValueCollection;
		string view_state;
		if (postdata == null || (view_state = postdata ["__VIEWSTATE"]) == null)
			return null;

		_savedViewState = null;
		LosFormatter fmt = new LosFormatter ();

		try { 
			_savedViewState = fmt.Deserialize (view_state);
		} catch (Exception e) {
			throw new HttpException ("Error restoring page viewstate.\n", e);
		}

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
		if (!handleViewState)
			return;

		Pair pair = new Pair ();
		pair.First = SaveViewStateRecursive ();
		if (_requiresPostBack != null && _requiresPostBack.Count > 0)
			pair.Second = _requiresPostBack;

		if (pair.First == null && pair.Second == null)
			pair = null;

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

	[EditorBrowsable (EditorBrowsableState.Advanced)]
	public virtual void VerifyRenderingInServerForm (Control control)
	{
		if (!renderingForm)
			throw new HttpException ("Control '" + control.ClientID + " " + control.GetType () + 
						 "' must be rendered within a HtmlForm");
	}

	#endregion
	
	#if NET_2_0
	public string GetWebResourceUrl(Type type, string resourceName)
	{
		if (type == null)
			throw new ArgumentNullException ("type");
	
		if (resourceName == null || resourceName.Length == 0)
			throw new ArgumentNullException ("type");
	
		return System.Web.Handlers.AssemblyResourceLoader.GetResourceUrl (type, resourceName); 
	}
	
	Stack dataItemCtx;
	
	internal void PushDataItemContext (object o)
	{
		if (dataItemCtx == null)
			dataItemCtx = new Stack ();
		
		dataItemCtx.Push (o);
	}
	
	internal void PopDataItemContext ()
	{
		if (dataItemCtx == null)
			throw new InvalidOperationException ();
		
		dataItemCtx.Pop ();
	}
	
	internal object CurrentDataItem {
		get {
			if (dataItemCtx == null)
				throw new InvalidOperationException ("No data item");
			
			return dataItemCtx.Peek ();
		}
	}
	
	protected object Eval (string expression)
	{
		return DataBinder.Eval (CurrentDataItem, expression);
	}
	
	protected object Eval (string expression, string format)
	{
		return DataBinder.Eval (CurrentDataItem, expression, format);
	}
	
	protected object XPath (string xpathexpression)
	{
		return XPathBinder.Eval (CurrentDataItem, xpathexpression);
	}
	
	protected object XPath (string xpathexpression, string format)
	{
		return XPathBinder.Eval (CurrentDataItem, xpathexpression, format);
	}
	
	protected IEnumerable XPathSelect (string xpathexpression)
	{
		return XPathBinder.Select (CurrentDataItem, xpathexpression);
	}
	
	#endif
}
}
