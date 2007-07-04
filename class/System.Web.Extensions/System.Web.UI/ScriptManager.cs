//
// ScriptManager.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Security.Permissions;
using System.Collections.Specialized;
using System.Collections;
using System.Web.Handlers;
using System.Reflection;
using System.Web.Configuration;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Web.Script.Serialization;
using System.Web.Script.Services;

namespace System.Web.UI
{
	[ParseChildrenAttribute (true)]
	[DefaultPropertyAttribute ("Scripts")]
	[DesignerAttribute ("System.Web.UI.Design.ScriptManagerDesigner, System.Web.Extensions.Design, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[NonVisualControlAttribute]
	[PersistChildrenAttribute (false)]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ScriptManager : Control, IPostBackDataHandler
	{
		// the keywords are used in fomatting async response
		const string updatePanel = "updatePanel";
		const string hiddenField = "hiddenField";
		const string arrayDeclaration = "arrayDeclaration";
		const string scriptBlock = "scriptBlock";
		const string expando = "expando";
		const string onSubmit = "onSubmit";
		const string asyncPostBackControlIDs = "asyncPostBackControlIDs";
		const string postBackControlIDs = "postBackControlIDs";
		const string updatePanelIDs = "updatePanelIDs";
		const string asyncPostBackTimeout = "asyncPostBackTimeout";
		const string childUpdatePanelIDs = "childUpdatePanelIDs";
		const string panelsToRefreshIDs = "panelsToRefreshIDs";
		const string formAction = "formAction";
		const string dataItem = "dataItem";
		const string dataItemJson = "dataItemJson";
		const string scriptDispose = "scriptDispose";
		const string pageRedirect = "pageRedirect";
		const string error = "error";
		const string pageTitle = "pageTitle";
		const string focus = "focus";
		const string scriptContentNoTags = "ScriptContentNoTags";
		const string scriptContentWithTags = "ScriptContentWithTags";
		const string scriptPath = "ScriptPath";

		static JavaScriptSerializer _cultureInfoSerializer;

		int _asyncPostBackTimeout = 90;
		List<Control> _asyncPostBackControls;
		List<Control> _postBackControls;
		List<UpdatePanel> _childUpdatePanels;
		List<UpdatePanel> _panelsToRefresh;
		List<UpdatePanel> _updatePanels;
		ScriptReferenceCollection _scripts;
		bool _isInAsyncPostBack;
		bool _isInPartialRendering;
		string _asyncPostBackSourceElementID;
		ScriptMode _scriptMode = ScriptMode.Auto;
		bool _enableScriptGlobalization;
		bool _enableScriptLocalization;
		string _scriptPath;
		ScriptEntry _clientScriptBlocks;
		ScriptEntry _startupScriptBlocks;
		ScriptEntry _scriptIncludes;
		ScriptEntry _onSubmitStatements;
		List<ArrayDeclaration> _arrayDeclarations;
		Hashtable _hiddenFields;
		List<IScriptControl> _registeredScriptControls;
		Dictionary<IExtenderControl, Control> _registeredExtenderControls;
		bool? _supportsPartialRendering;
		bool _enablePartialRendering = true;
		bool _init;
		string _panelToRefreshID;
		Dictionary<Control, DataItemEntry> _dataItems;
		bool _enablePageMethods;

		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool AllowCustomErrorsRedirect {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue ("")]
		public string AsyncPostBackErrorMessage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public string AsyncPostBackSourceElementID {
			get {
				if (_asyncPostBackSourceElementID == null)
					return String.Empty;
				return _asyncPostBackSourceElementID;
			}
		}

		[DefaultValue (90)]
		[Category ("Behavior")]
		public int AsyncPostBackTimeout {
			get {
				return _asyncPostBackTimeout;
			}
			set {
				_asyncPostBackTimeout = value;
			}
		}

		[Category ("Behavior")]
		[MergableProperty (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public AuthenticationServiceManager AuthenticationService {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnablePageMethods {
			get {
				return _enablePageMethods;
			}
			set {
				_enablePageMethods = value;
			}
		}

		[DefaultValue (true)]
		[Category ("Behavior")]
		public bool EnablePartialRendering {
			get {
				return _enablePartialRendering;
			}
			set {
				if (_init)
					throw new InvalidOperationException ();
				_enablePartialRendering = value;
			}
		}

		[DefaultValue (false)]
		[Category ("Behavior")]
		public bool EnableScriptGlobalization {
			get {
				return _enableScriptGlobalization;
			}
			set {
				_enableScriptGlobalization = value;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (false)]
		public bool EnableScriptLocalization {
			get {
				return _enableScriptLocalization;
			}
			set {
				_enableScriptLocalization = value;
			}
		}

		[Browsable (false)]
		public bool IsDebuggingEnabled {
			get {
				if (IsDeploymentRetail)
					return false;
#if !TARGET_J2EE
				CompilationSection compilation = (CompilationSection) WebConfigurationManager.GetSection ("system.web/compilation");
				if (!compilation.Debug && (ScriptMode == ScriptMode.Auto || ScriptMode == ScriptMode.Inherit))
					return false;
#endif
				if (ScriptMode == ScriptMode.Release)
					return false;

				return true;
			}
		}

		bool IsDeploymentRetail {
			get {
				DeploymentSection deployment = (DeploymentSection) WebConfigurationManager.GetSection ("system.web/deployment");
				return deployment.Retail;
			}
		}

		[Browsable (false)]
		public bool IsInAsyncPostBack {
			get {
				return _isInAsyncPostBack;
			}
		}

		internal bool IsInPartialRendering {
			get {
				return _isInPartialRendering;
			}
			set {
				_isInPartialRendering = value;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool LoadScriptsBeforeUI {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Category ("Behavior")]
		[MergableProperty (false)]
		public ProfileServiceManager ProfileService {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
#if TARGET_J2EE
		[MonoLimitation ("The 'Auto' value is the same as 'Debug'.")]
#endif
		public ScriptMode ScriptMode {
			get {
				return _scriptMode;
			}
			set {
				if (value == ScriptMode.Inherit)
					value = ScriptMode.Auto;
				_scriptMode = value;
			}
		}

		[DefaultValue ("")]
		[Category ("Behavior")]
		public string ScriptPath {
			get {
				if (_scriptPath == null)
					return String.Empty;
				return _scriptPath;
			}
			set {
				_scriptPath = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[Category ("Behavior")]
		[MergableProperty (false)]
		public ScriptReferenceCollection Scripts {
			get {
				if (_scripts == null)
					_scripts = new ScriptReferenceCollection ();

				return _scripts;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DefaultValue ("")]
		[MergableProperty (false)]
		[Category ("Behavior")]
		public ServiceReferenceCollection Services {
			get {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (true)]
		[Browsable (false)]
		public bool SupportsPartialRendering {
			get {
				if (!_supportsPartialRendering.HasValue)
					_supportsPartialRendering = CheckSupportsPartialRendering ();
				return _supportsPartialRendering.Value;
			}
			set {
				if (_init)
					throw new InvalidOperationException ();
				if (!EnablePartialRendering && value)
					throw new InvalidOperationException ("The SupportsPartialRendering property cannot be set when EnablePartialRendering is false.");

				_supportsPartialRendering = value;
			}
		}

		bool CheckSupportsPartialRendering () {
			if (!EnablePartialRendering)
				return false;
			// TODO: consider browser capabilities
			return true;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Visible {
			get {
				return true;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Action")]
		public event EventHandler<AsyncPostBackErrorEventArgs> AsyncPostBackError;

		[Category ("Action")]
		public event EventHandler<ScriptReferenceEventArgs> ResolveScriptReference;

		static ScriptManager () {
			_cultureInfoSerializer = new JavaScriptSerializer ();
			_cultureInfoSerializer.RegisterConverters (CultureInfoConverter.GetConverters ());
		}

		public static ScriptManager GetCurrent (Page page) {
			HttpContext ctx = HttpContext.Current;
			if (ctx == null)
				return null;

			return (ScriptManager) ctx.Items [page];
		}

		static void SetCurrent (Page page, ScriptManager instance) {
			HttpContext ctx = HttpContext.Current;
			ctx.Items [page] = instance;
		}

		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection) {
			_isInAsyncPostBack = true;
			string arg = postCollection [postDataKey];
			if (!String.IsNullOrEmpty (arg)) {
				string [] args = arg.Split ('|');
				_panelToRefreshID = args [0];
				_asyncPostBackSourceElementID = args [1];
				return true;
			}
			return false;
		}

		protected internal virtual void OnAsyncPostBackError (AsyncPostBackErrorEventArgs e) {
			if (AsyncPostBackError != null)
				AsyncPostBackError (this, e);
		}

		protected override void OnInit (EventArgs e) {
			base.OnInit (e);

			if (GetCurrent (Page) != null)
				throw new InvalidOperationException ("Only one instance of a ScriptManager can be added to the page.");

			SetCurrent (Page, this);

			_init = true;
		}

		protected override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);

			Page.PreRenderComplete += new EventHandler (OnPreRenderComplete);

			if (IsInAsyncPostBack) {
				Page.SetRenderMethodDelegate (RenderPageCallback);
			}
			else {
				if (EnableScriptGlobalization) {
					CultureInfo culture = Thread.CurrentThread.CurrentCulture;
					string script = String.Format ("var __cultureInfo = '{0}';", _cultureInfoSerializer.Serialize (culture));
					RegisterClientScriptBlock (this, typeof (ScriptManager), "ScriptGlobalization", script, true);
				}
				if (EnablePageMethods) {
					LogicalTypeInfo logicalTypeInfo = LogicalTypeInfo.GetLogicalTypeInfo (Page.GetType (), Page.Request.FilePath);
					RegisterStartupScript (this, typeof (ScriptManager), "PageMethods", logicalTypeInfo.Proxy, true);
				}

				// Register startup script
				StringBuilder sb = new StringBuilder ();
				sb.AppendLine ();
				sb.AppendLine ("Sys.Application.initialize();");
				RegisterStartupScript (this, typeof (ExtenderControl), "Sys.Application.initialize();", sb.ToString (), true);
			}
		}

		void OnPreRenderComplete (object sender, EventArgs e) {
			// Register Scripts
			List<ScriptReferenceEntry> scripts = new List<ScriptReferenceEntry> ();
			foreach (ScriptReferenceEntry script in GetScriptReferences ()) {
				OnResolveScriptReference (new ScriptReferenceEventArgs (script.ScriptReference));
				scripts.Add (script);
			}
			for (int i = 0; i < scripts.Count; i++)
				if (!IsInAsyncPostBack || HasBeenRendered (scripts [i].Control))
					RegisterScriptReference (scripts [i].ScriptReference);
		}

		static bool HasBeenRendered (Control control) {
			if(control==null)
				return false;

			UpdatePanel parent = control.Parent as UpdatePanel;
			if (parent != null && parent.RequiresUpdate)
				return true;

			return HasBeenRendered (control.Parent);
		}

		IEnumerable<ScriptReferenceEntry> GetScriptReferences () {
			ScriptReference script;

			script = new ScriptReference ("MicrosoftAjax.js", String.Empty);
			script.NotifyScriptLoaded = false;
			yield return new ScriptReferenceEntry (this, script);

			script = new ScriptReference ("MicrosoftAjaxWebForms.js", String.Empty);
			script.NotifyScriptLoaded = false;
			yield return new ScriptReferenceEntry (this, script);

			if (_scripts != null && _scripts.Count > 0) {
				for (int i = 0; i < _scripts.Count; i++) {
					yield return new ScriptReferenceEntry (this, _scripts [i]);
				}
			}

			if (_registeredScriptControls != null && _registeredScriptControls.Count > 0) {
				for (int i = 0; i < _registeredScriptControls.Count; i++) {
					IEnumerable<ScriptReference> scripts = _registeredScriptControls [i].GetScriptReferences ();
					if (scripts != null)
						foreach (ScriptReference s in scripts)
							yield return new ScriptReferenceEntry ((Control) _registeredScriptControls [i], s);
				}
			}

			if (_registeredExtenderControls != null && _registeredExtenderControls.Count > 0) {
				foreach (IExtenderControl ex in _registeredExtenderControls.Keys) {
					IEnumerable<ScriptReference> scripts = ex.GetScriptReferences ();
					if (scripts != null)
						foreach (ScriptReference s in scripts)
							yield return new ScriptReferenceEntry ((Control) ex, s);
				}
			}
		}

		protected virtual void OnResolveScriptReference (ScriptReferenceEventArgs e) {
			if (ResolveScriptReference != null)
				ResolveScriptReference (this, e);
		}

		protected virtual void RaisePostDataChangedEvent () {
			UpdatePanel up = Page.FindControl (_panelToRefreshID) as UpdatePanel;
			if (up != null && up.ChildrenAsTriggers)
				up.Update ();
		}

		public static void RegisterArrayDeclaration (Control control, string arrayName, string arrayValue) {
			RegisterArrayDeclaration (control.Page, arrayName, arrayValue);
		}

		public static void RegisterArrayDeclaration (Page page, string arrayName, string arrayValue) {
			ScriptManager sm = GetCurrent (page);
			if (sm.IsInAsyncPostBack)
				sm.RegisterArrayDeclaration (arrayName, arrayValue);
			else
				page.ClientScript.RegisterArrayDeclaration (arrayName, arrayValue);
		}

		void RegisterArrayDeclaration (string arrayName, string arrayValue) {
			if (_arrayDeclarations == null)
				_arrayDeclarations = new List<ArrayDeclaration> ();

			_arrayDeclarations.Add (new ArrayDeclaration (arrayName, arrayValue));
		}

		public void RegisterAsyncPostBackControl (Control control) {
			if (control == null)
				return;

			if (_asyncPostBackControls == null)
				_asyncPostBackControls = new List<Control> ();

			if (_asyncPostBackControls.Contains (control))
				return;

			_asyncPostBackControls.Add (control);
		}

		public static void RegisterClientScriptBlock (Control control, Type type, string key, string script, bool addScriptTags) {
			RegisterClientScriptBlock (control.Page, type, key, script, addScriptTags);
		}

		public static void RegisterClientScriptBlock (Page page, Type type, string key, string script, bool addScriptTags) {
			ScriptManager sm = GetCurrent (page);
			if (sm.IsInAsyncPostBack)
				RegisterScript (ref sm._clientScriptBlocks, type, key, script, addScriptTags ? ScriptEntryType.ScriptContentNoTags : ScriptEntryType.ScriptContentWithTags);
			else
				page.ClientScript.RegisterClientScriptBlock (type, key, script, addScriptTags);
		}

		public static void RegisterClientScriptInclude (Control control, Type type, string key, string url) {
			RegisterClientScriptInclude (control.Page, type, key, url);
		}

		public static void RegisterClientScriptInclude (Page page, Type type, string key, string url) {
			ScriptManager sm = GetCurrent (page);
			if (sm.IsInAsyncPostBack)
				RegisterScript (ref sm._scriptIncludes, type, key, url, ScriptEntryType.ScriptPath);
			else
				page.ClientScript.RegisterClientScriptInclude (type, key, url);
		}

		public static void RegisterClientScriptResource (Control control, Type type, string resourceName) {
			RegisterClientScriptResource (control.Page, type, resourceName);
		}

		public static void RegisterClientScriptResource (Page page, Type type, string resourceName) {
			RegisterClientScriptInclude (page, type, "resource-" + resourceName, ScriptResourceHandler.GetResourceUrl (type, resourceName));
		}

		void RegisterScriptReference (ScriptReference script) {

			bool isDebugMode = IsDeploymentRetail ? false : (script.ScriptModeInternal == ScriptMode.Inherit ? IsDebuggingEnabled : (script.ScriptModeInternal == ScriptMode.Debug));
			string url;
			if (!String.IsNullOrEmpty (script.Path)) {
				url = GetScriptName (ResolveClientUrl (script.Path), isDebugMode, EnableScriptLocalization ? script.ResourceUICultures : null);
			}
			else if (!String.IsNullOrEmpty (script.Name)) {
				Assembly assembly;
				if (String.IsNullOrEmpty (script.Assembly))
					assembly = typeof (ScriptManager).Assembly;
				else
					assembly = Assembly.Load (script.Assembly);
				string name = GetScriptName (script.Name, isDebugMode, null);
				if (script.IgnoreScriptPath || String.IsNullOrEmpty (ScriptPath))
					url = ScriptResourceHandler.GetResourceUrl (assembly, name, script.NotifyScriptLoaded);
				else {
					AssemblyName an = assembly.GetName ();
					url = ResolveClientUrl (String.Concat (VirtualPathUtility.AppendTrailingSlash (ScriptPath), an.Name, '/', an.Version, '/', name));
				}
			}
			else {
				throw new InvalidOperationException ("Name and Path cannot both be empty.");
			}

			RegisterClientScriptInclude (this, typeof (ScriptManager), url, url);
		}

		static string GetScriptName (string releaseName, bool isDebugMode, string [] supportedUICultures) {
			if (!isDebugMode && (supportedUICultures == null || supportedUICultures.Length == 0))
				return releaseName;

			if (releaseName.Length < 3 || !releaseName.EndsWith (".js", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException (String.Format ("'{0}' is not a valid script path.  The path must end in '.js'.", releaseName));

			StringBuilder sb = new StringBuilder (releaseName);
			sb.Length -= 3;
			if (isDebugMode)
				sb.Append (".debug");
			string culture = Thread.CurrentThread.CurrentUICulture.Name;
			if (supportedUICultures != null && Array.IndexOf<string> (supportedUICultures, culture) >= 0)
				sb.AppendFormat (".{0}", culture);
			sb.Append (".js");

			return sb.ToString ();
		}

		public void RegisterDataItem (Control control, string dataItem) {
			RegisterDataItem (control, dataItem, false);
		}

		public void RegisterDataItem (Control control, string dataItem, bool isJsonSerialized) {
			if (!IsInAsyncPostBack)
				throw new InvalidOperationException ("RegisterDataItem can only be called during an async postback.");
			if (control == null)
				throw new ArgumentNullException ("control");

			if (_dataItems == null)
				_dataItems = new Dictionary<Control, DataItemEntry> ();

			if (_dataItems.ContainsKey (control))
				throw new ArgumentException (String.Format ("'{0}' already has a data item registered.", control.ID), "control");

			_dataItems.Add (control, new DataItemEntry (dataItem, isJsonSerialized));
		}

		public void RegisterDispose (Control control, string disposeScript) {
			throw new NotImplementedException ();
		}

		public static void RegisterExpandoAttribute (Control control, string controlId, string attributeName, string attributeValue, bool encode) {
			Page page = control.Page;
			ScriptManager sm = GetCurrent (page);
			if (sm.IsInAsyncPostBack)
				sm.RegisterExpandoAttribute (controlId, attributeName, attributeValue, encode);
			else
				page.ClientScript.RegisterExpandoAttribute (controlId, attributeName, attributeValue, encode);
		}

		private void RegisterExpandoAttribute (string controlId, string attributeName, string attributeValue, bool encode) {
			// seems MS do nothing.
		}

		public static void RegisterHiddenField (Control control, string hiddenFieldName, string hiddenFieldInitialValue) {
			RegisterHiddenField (control.Page, hiddenFieldName, hiddenFieldInitialValue);
		}

		public static void RegisterHiddenField (Page page, string hiddenFieldName, string hiddenFieldInitialValue) {
			ScriptManager sm = GetCurrent (page);
			if (sm.IsInAsyncPostBack)
				sm.RegisterHiddenField (hiddenFieldName, hiddenFieldInitialValue);
			else
				page.ClientScript.RegisterHiddenField (hiddenFieldName, hiddenFieldInitialValue);
		}

		void RegisterHiddenField (string hiddenFieldName, string hiddenFieldInitialValue) {
			if (_hiddenFields == null)
				_hiddenFields = new Hashtable ();

			if (!_hiddenFields.ContainsKey (hiddenFieldName))
				_hiddenFields.Add (hiddenFieldName, hiddenFieldInitialValue);
		}

		public static void RegisterOnSubmitStatement (Control control, Type type, string key, string script) {
			RegisterOnSubmitStatement (control.Page, type, key, script);
		}

		public static void RegisterOnSubmitStatement (Page page, Type type, string key, string script) {
			ScriptManager sm = GetCurrent (page);
			if (sm.IsInAsyncPostBack)
				RegisterScript (ref sm._onSubmitStatements, type, key, script, ScriptEntryType.OnSubmit);
			else
				page.ClientScript.RegisterOnSubmitStatement (type, key, script);
		}

		public void RegisterPostBackControl (Control control) {
			if (control == null)
				return;

			if (_postBackControls == null)
				_postBackControls = new List<Control> ();

			if (_postBackControls.Contains (control))
				return;

			_postBackControls.Add (control);
		}

		internal void RegisterUpdatePanel (UpdatePanel updatePanel) {
			if (_updatePanels == null)
				_updatePanels = new List<UpdatePanel> ();

			if (_updatePanels.Contains (updatePanel))
				return;

			_updatePanels.Add (updatePanel);
		}

		public void RegisterScriptDescriptors (IExtenderControl extenderControl) {
			if (extenderControl == null)
				return;

			if (_registeredExtenderControls == null || !_registeredExtenderControls.ContainsKey (extenderControl))
				return;

			Control targetControl = _registeredExtenderControls [extenderControl];
			RegisterScriptDescriptors (extenderControl.GetScriptDescriptors (targetControl));
		}

		public void RegisterScriptDescriptors (IScriptControl scriptControl) {
			if (scriptControl == null)
				return;

			if (_registeredScriptControls == null || !_registeredScriptControls.Contains (scriptControl))
				return;

			RegisterScriptDescriptors (scriptControl.GetScriptDescriptors ());
		}

		void RegisterScriptDescriptors (IEnumerable<ScriptDescriptor> scriptDescriptors) {
			if (scriptDescriptors == null)
				return;
			if (IsInAsyncPostBack && !IsInPartialRendering)
				return;

			StringBuilder sb = new StringBuilder ();
			foreach (ScriptDescriptor scriptDescriptor in scriptDescriptors) {
				sb.AppendLine ("Sys.Application.add_init(function() {");
				sb.AppendLine (scriptDescriptor.GetScript ());
				sb.AppendLine ("});");
			}
			string script = sb.ToString ();
			RegisterStartupScript (this, typeof (ExtenderControl), script, script, true);
		}

		public static void RegisterStartupScript (Control control, Type type, string key, string script, bool addScriptTags) {
			RegisterStartupScript (control.Page, type, key, script, addScriptTags);
		}

		public static void RegisterStartupScript (Page page, Type type, string key, string script, bool addScriptTags) {
			ScriptManager sm = GetCurrent (page);
			if (sm.IsInAsyncPostBack)
				RegisterScript (ref sm._startupScriptBlocks, type, key, script, addScriptTags ? ScriptEntryType.ScriptContentNoTags : ScriptEntryType.ScriptContentWithTags);
			else
				page.ClientScript.RegisterStartupScript (type, key, script, addScriptTags);
		}

		public void RegisterScriptControl<TScriptControl> (TScriptControl scriptControl) where TScriptControl : Control, IScriptControl {
			if (scriptControl == null)
				throw new ArgumentNullException ("scriptControl");

			if (_registeredScriptControls == null)
				_registeredScriptControls = new List<IScriptControl> ();

			if (!_registeredScriptControls.Contains (scriptControl))
				_registeredScriptControls.Add (scriptControl);
		}

		public void RegisterExtenderControl<TExtenderControl> (TExtenderControl extenderControl, Control targetControl) where TExtenderControl : Control, IExtenderControl {
			if (extenderControl == null)
				throw new ArgumentNullException ("extenderControl");
			if (targetControl == null)
				throw new ArgumentNullException ("targetControl");

			if (_registeredExtenderControls == null)
				_registeredExtenderControls = new Dictionary<IExtenderControl, Control> ();

			if (!_registeredExtenderControls.ContainsKey (extenderControl))
				_registeredExtenderControls.Add (extenderControl, targetControl);
		}

		static void RegisterScript (ref ScriptEntry scriptList, Type type, string key, string script, ScriptEntryType scriptEntryType) {
			ScriptEntry last = null;
			ScriptEntry entry = scriptList;

			while (entry != null) {
				if (entry.Type == type && entry.Key == key)
					return;
				last = entry;
				entry = entry.Next;
			}

			entry = new ScriptEntry (type, key, script, scriptEntryType);

			if (last != null)
				last.Next = entry;
			else
				scriptList = entry;
		}

		protected override void Render (HtmlTextWriter writer) {
			// MSDN: This method is used by control developers to extend the ScriptManager control. 
			// Notes to Inheritors: 
			// When overriding this method, call the base Render(HtmlTextWriter) method 
			// so that PageRequestManager is rendered on the page.
			if (SupportsPartialRendering) {
				writer.WriteLine ("<script type=\"text/javascript\">");
				writer.WriteLine ("//<![CDATA[");
				writer.WriteLine ("Sys.WebForms.PageRequestManager._initialize('{0}', document.getElementById('{1}'));", UniqueID, Page.Form.ClientID);
				writer.WriteLine ("Sys.WebForms.PageRequestManager.getInstance()._updateControls([{0}], [{1}], [{2}], {3});", FormatUpdatePanelIDs (_updatePanels, true), FormatListIDs (_asyncPostBackControls, true), FormatListIDs (_postBackControls, true), AsyncPostBackTimeout);
				writer.WriteLine ("//]]");
				writer.WriteLine ("</script>");
			}
			base.Render (writer);
		}

		static string FormatUpdatePanelIDs (List<UpdatePanel> list, bool useSingleQuote) {
			if (list == null || list.Count == 0)
				return null;

			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < list.Count; i++) {
				sb.AppendFormat ("{0}{1}{2}{0},", useSingleQuote ? "'" : String.Empty, list [i].ChildrenAsTriggers ? "t" : "f", list [i].UniqueID);
			}
			if (sb.Length > 0)
				sb.Length--;

			return sb.ToString ();
		}

		static string FormatListIDs<T> (List<T> list, bool useSingleQuote) where T : Control {
			if (list == null || list.Count == 0)
				return null;

			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < list.Count; i++) {
				sb.AppendFormat ("{0}{1}{0},", useSingleQuote ? "'" : String.Empty, list [i].UniqueID);
			}
			if (sb.Length > 0)
				sb.Length--;

			return sb.ToString ();
		}

		public void SetFocus (Control control) {
			throw new NotImplementedException ();
		}

		public void SetFocus (string clientID) {
			throw new NotImplementedException ();
		}

		#region IPostBackDataHandler Members

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection) {
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent () {
			RaisePostDataChangedEvent ();
		}

		#endregion

		static internal void WriteCallbackException (TextWriter output, Exception ex) {
			HttpException httpEx = ex as HttpException;
			WriteCallbackOutput (output, error, httpEx == null ? "500" : httpEx.GetHttpCode ().ToString (), ex.GetBaseException ().Message);
		}

		static internal void WriteCallbackRedirect (TextWriter output, string redirectUrl) {
			WriteCallbackOutput (output, pageRedirect, null, redirectUrl);
		}

		internal void WriteCallbackPanel (TextWriter output, UpdatePanel panel, StringBuilder panelOutput) {
			if (_panelsToRefresh == null)
				_panelsToRefresh = new List<UpdatePanel> ();
			_panelsToRefresh.Add (panel);

			WriteCallbackOutput (output, updatePanel, panel.ClientID, panelOutput);
		}

		internal void RegisterChildUpdatePanel (UpdatePanel updatePanel) {
			if (_childUpdatePanels == null)
				_childUpdatePanels = new List<UpdatePanel> ();
			_childUpdatePanels.Add (updatePanel);
		}

		static void WriteCallbackOutput (TextWriter output, string type, string name, object value) {
			string str = value as string;
			StringBuilder sb = value as StringBuilder;
			int length = 0;
			if (str != null)
				length = str.Length;
			else if (sb != null)
				length = sb.Length;

			//output.Write ("{0}|{1}|{2}|{3}|", value == null ? 0 : value.Length, type, name, value);
			output.Write (length);
			output.Write ('|');
			output.Write (type);
			output.Write ('|');
			output.Write (name);
			output.Write ('|');
			for (int i = 0; i < length; i++)
				if (str != null)
					output.Write (str [i]);
				else
					output.Write (sb [i]);
			output.Write ('|');
		}

		void RenderPageCallback (HtmlTextWriter output, Control container) {
			Page page = (Page) container;

			page.Form.SetRenderMethodDelegate (RenderFormCallback);
			HtmlTextParser parser = new HtmlTextParser (output);
			page.Form.RenderControl (parser);

			WriteCallbackOutput (output, asyncPostBackControlIDs, null, FormatListIDs (_asyncPostBackControls, false));
			WriteCallbackOutput (output, postBackControlIDs, null, FormatListIDs (_postBackControls, false));
			WriteCallbackOutput (output, updatePanelIDs, null, FormatUpdatePanelIDs (_updatePanels, false));
			WriteCallbackOutput (output, childUpdatePanelIDs, null, FormatListIDs (_childUpdatePanels, false));
			WriteCallbackOutput (output, panelsToRefreshIDs, null, FormatListIDs (_panelsToRefresh, false));
			WriteCallbackOutput (output, asyncPostBackTimeout, null, AsyncPostBackTimeout.ToString ());
			WriteCallbackOutput (output, pageTitle, null, Page.Title);

			if (_dataItems != null)
				foreach (Control control in _dataItems.Keys) {
					DataItemEntry entry = _dataItems [control];
					WriteCallbackOutput (output, entry.IsJsonSerialized ? dataItemJson : dataItem, control.ClientID, entry.DataItem);
				}
			
			WriteArrayDeclarations (output);
			WriteScriptBlocks (output, _clientScriptBlocks);
			WriteScriptBlocks (output, _scriptIncludes);
			WriteScriptBlocks (output, _startupScriptBlocks);
			WriteScriptBlocks (output, _onSubmitStatements);
			WriteHiddenFields (output);
		}

		void WriteArrayDeclarations (HtmlTextWriter writer) {
			if (_arrayDeclarations != null) {
				for (int i = 0; i < _arrayDeclarations.Count; i++) {
					ArrayDeclaration array = _arrayDeclarations [i];
					WriteCallbackOutput (writer, arrayDeclaration, array.ArrayName, array.ArrayValue);
				}
			}
		}

		void WriteScriptBlocks (HtmlTextWriter output, ScriptEntry scriptList) {
			while (scriptList != null) {
				switch (scriptList.ScriptEntryType) {
				case ScriptEntryType.ScriptContentNoTags:
					WriteCallbackOutput (output, scriptBlock, scriptContentNoTags, scriptList.Script);
					break;
				case ScriptEntryType.ScriptContentWithTags:
					string script = SerializeScriptBlock (scriptList);
					WriteCallbackOutput (output, scriptBlock, scriptContentWithTags, script);
					break;
				case ScriptEntryType.ScriptPath:
					WriteCallbackOutput (output, scriptBlock, scriptPath, scriptList.Script);
					break;
				case ScriptEntryType.OnSubmit:
					WriteCallbackOutput (output, onSubmit, null, scriptList.Script);
					break;
				}
				scriptList = scriptList.Next;
			}
		}

		void WriteHiddenFields (HtmlTextWriter output) {
			if (_hiddenFields == null)
				return;
			foreach (string key in _hiddenFields.Keys) {
				string value = _hiddenFields [key] as string;
				WriteCallbackOutput (output, hiddenField, key, value);
			}
		}

		[MonoTODO ()]
		static string SerializeScriptBlock (ScriptEntry scriptList) {
			throw new InvalidOperationException (String.Format ("The script tag registered for type '{0}' and key '{1}' has invalid characters outside of the script tags: {2}. Only properly formatted script tags can be registered.", scriptList.Type, scriptList.Key, scriptList.Script));
		}

		void RenderFormCallback (HtmlTextWriter output, Control container) {
			output = ((HtmlTextParser) output).ResponseOutput;
			HtmlForm form = (HtmlForm) container;
			HtmlTextWriter writer = new HtmlDropWriter (output);
			if (form.HasControls ()) {
				for (int i = 0; i < form.Controls.Count; i++) {
					form.Controls [i].RenderControl (writer);
				}
			}
		}

		internal class AlternativeHtmlTextWriter : HtmlTextWriter
		{
			readonly HtmlTextWriter _responseOutput;

			public HtmlTextWriter ResponseOutput {
				get { return _responseOutput; }
			}

			public AlternativeHtmlTextWriter (TextWriter writer, HtmlTextWriter responseOutput)
				: base (writer) {
				_responseOutput = responseOutput;
			}
		}

		sealed class HtmlTextParser : AlternativeHtmlTextWriter
		{
			public HtmlTextParser (HtmlTextWriter responseOutput)
				: base (new TextParser (responseOutput), responseOutput) {
			}
		}

		sealed class TextParser : TextWriter
		{
			int _state;
			char _charState = (char) 255;
			const char nullCharState = (char) 255;
			StringBuilder _sb = new StringBuilder ();
			Dictionary<string, string> _currentField;
			string _currentAttribute;
			readonly HtmlTextWriter _responseOutput;

			public override Encoding Encoding {
				get { return Encoding.UTF8; }
			}

			public TextParser (HtmlTextWriter responseOutput) {
				_responseOutput = responseOutput;
			}

			public override void Write (char value) {
				switch (_state) {
				case 0:
					ParseBeginTag (value);
					break;
				case 1:
					ParseAttributeName (value);
					break;
				case 2:
					ParseAttributeValue (value);
					break;
				}
			}

			private void ParseAttributeValue (char value) {
				switch (value) {
				case '>':
					ResetState ();
					break;
				case '"':
					_currentField.Add (_currentAttribute, _sb.ToString ());
					_state = 1;
					_sb.Length = 0;
					ProbeWriteOutput ();
					break;
				default:
					_sb.Append (value);
					break;
				}
			}

			private void ParseAttributeName (char value) {
				switch (value) {
				case '>':
					ResetState ();
					break;
				case ' ':
				case '=':
					break;
				case '"':
					_currentAttribute = _sb.ToString ();
					_state = 2;
					_sb.Length = 0;
					break;
				default:
					_sb.Append (value);
					break;
				}
			}

			void ParseBeginTag (char value) {
				switch (_charState) {
				case nullCharState:
					if (value == '<')
						_charState = value;
					break;
				case '<':
					if (value == 'i')
						_charState = value;
					else
						ResetState ();
					break;
				case 'i':
					if (value == 'n')
						_charState = value;
					else
						ResetState ();
					break;
				case 'n':
					if (value == 'p')
						_charState = value;
					else
						ResetState ();
					break;
				case 'p':
					if (value == 'u')
						_charState = value;
					else
						ResetState ();
					break;
				case 'u':
					if (value == 't')
						_charState = value;
					else
						ResetState ();
					break;
				case 't':
					if (value == ' ') {
						_state = 1;
						_currentField = new Dictionary<string, string> ();
					}
					else
						ResetState ();
					break;
				}
			}

			private void ResetState () {
				_charState = nullCharState;
				_state = 0;
				_sb.Length = 0;
			}

			private void ProbeWriteOutput () {
				if (!_currentField.ContainsKey ("name"))
					return;
				if (!_currentField.ContainsKey ("value"))
					return;

				string value = _currentField ["value"];
				if (String.IsNullOrEmpty (value))
					return;

				ScriptManager.WriteCallbackOutput (_responseOutput, ScriptManager.hiddenField, _currentField ["name"], value);
			}
		}

		sealed class HtmlDropWriter : AlternativeHtmlTextWriter
		{
			public HtmlDropWriter (HtmlTextWriter responseOutput)
				: base (new DropWriter (), responseOutput) {
			}
		}

		sealed class DropWriter : TextWriter
		{
			public override Encoding Encoding {
				get { return Encoding.UTF8; }
			}
		}

		sealed class ScriptEntry
		{
			readonly public Type Type;
			readonly public string Key;
			readonly public string Script;
			readonly public ScriptEntryType ScriptEntryType;
			public ScriptEntry Next;

			public ScriptEntry (Type type, string key, string script, ScriptEntryType scriptEntryType) {
				Key = key;
				Type = type;
				Script = script;
				ScriptEntryType = scriptEntryType;
			}
		}

		enum ScriptEntryType
		{
			ScriptContentNoTags,
			ScriptContentWithTags,
			ScriptPath,
			OnSubmit
		}

		sealed class ArrayDeclaration
		{
			readonly public string ArrayName;
			readonly public string ArrayValue;

			public ArrayDeclaration (string arrayName, string arrayValue) {
				ArrayName = arrayName;
				ArrayValue = arrayValue;
			}
		}

		sealed class CultureInfoConverter : JavaScriptConverter
		{
			private CultureInfoConverter () { }
			static readonly Type typeofCultureInfo = typeof (CultureInfo);
			static CultureInfoConverter _instance = new CultureInfoConverter ();

			public static IEnumerable<JavaScriptConverter> GetConverters () { yield return _instance; }

			public override IEnumerable<Type> SupportedTypes {
				get { yield return typeofCultureInfo; }
			}

			public override object Deserialize (IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer) {
				throw new NotSupportedException ();
			}

			public override IDictionary<string, object> Serialize (object obj, JavaScriptSerializer serializer) {
				CultureInfo ci = (CultureInfo) obj;
				if (ci == null)
					return null;
				Dictionary<string, object> d = new Dictionary<string, object> (StringComparer.Ordinal);
				d.Add ("name", ci.Name);
				d.Add ("numberFormat", ci.NumberFormat);
				d.Add ("dateTimeFormat", ci.DateTimeFormat);
				return d;
			}
		}

		sealed class ScriptReferenceEntry
		{
			readonly Control _control;
			readonly ScriptReference _scriptReference;

			public Control Control { get { return _control; } }
			public ScriptReference ScriptReference { get { return _scriptReference; } }

			public ScriptReferenceEntry (Control control, ScriptReference scriptReference) {
				_control = control;
				_scriptReference = scriptReference;
			}
		}

		sealed class DataItemEntry
		{
			readonly string _dataItem;
			readonly bool _isJsonSerialized;

			public string DataItem { get { return _dataItem; } }
			public bool IsJsonSerialized { get { return _isJsonSerialized; } }

			public DataItemEntry (string dataItem, bool isJsonSerialized) {
				_dataItem = dataItem;
				_isJsonSerialized = isJsonSerialized;
			}
		}
	}
}
