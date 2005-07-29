//
// System.Web.UI.WebControls.HotSpot.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[TypeConverterAttribute (typeof(ExpandableObjectConverter))]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HotSpot: IStateManager
	{
		StateBag viewState = new StateBag ();
		
	    [LocalizableAttribute (true)]
	    [DefaultValueAttribute ("")]
		public virtual string AccessKey {
			get {
				object o = viewState ["AccessKey"];
				return o != null ? (string) o : "";
			}
			set {
				viewState ["AccessKey"] = value;
			}
		}
		
	    [NotifyParentPropertyAttribute (true)]
	    [WebCategoryAttribute ("Behavior")]
	    [DefaultValueAttribute ("")]
	    [BindableAttribute (true)]
		public virtual string AlternateText {
			get {
				object o = viewState ["AlternateText"];
				return o != null ? (string) o : "";
			}
			set {
				viewState ["AlternateText"] = value;
			}
		}
		
	    [WebCategoryAttribute ("Behavior")]
	    [DefaultValueAttribute (HotSpotMode.NotSet)]
	    [NotifyParentPropertyAttribute (true)]
		public virtual HotSpotMode HotSpotMode {
			get {
				object o = viewState ["HotSpotMode"];
				return o != null ? (HotSpotMode) o : HotSpotMode.NotSet;
			}
			set {
				viewState ["HotSpotMode"] = value;
			}
		}
		
	    [DefaultValueAttribute ("")]
	    [BindableAttribute (true)]
	    [EditorAttribute ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	    [NotifyParentPropertyAttribute (true)]
	    [UrlPropertyAttribute]
		public virtual string NavigateUrl {
			get {
				object o = viewState ["NavigateUrl"];
				return o != null ? (string) o : "";
			}
			set {
				viewState ["NavigateUrl"] = value;
			}
		}
		
	    [BindableAttribute (true)]
	    [WebCategoryAttribute ("Behavior")]
	    [DefaultValueAttribute ("")]
	    [NotifyParentPropertyAttribute (true)]
		public virtual string PostBackValue {
			get {
				object o = viewState ["PostBackValue"];
				return o != null ? (string) o : "";
			}
			set {
				viewState ["PostBackValue"] = value;
			}
		}
		
	    [DefaultValueAttribute (0)]
	    [WebCategoryAttribute ("Accessibility")]
		public virtual short TabIndex {
			get {
				object o = viewState ["TabIndex"];
				return o != null ? (short) o : (short) 0;
			}
			set {
				viewState ["TabIndex"] = value;
			}
		}
		
	    [WebCategoryAttribute ("Behavior")]
	    [NotifyParentPropertyAttribute (true)]
	    [DefaultValueAttribute ("")]
	    [TypeConverterAttribute (typeof(TargetConverter))]
		public virtual string Target {
			get {
				object o = viewState ["Target"];
				return o != null ? (string) o : "";
			}
			set {
				viewState ["Target"] = value;
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected StateBag ViewState {
			get { return viewState; }
		} 
		
		protected virtual void LoadViewState (object savedState)
		{
			viewState.LoadViewState (savedState);
		}
		
		protected virtual object SaveViewState ()
		{
			return viewState.SaveViewState ();
		}
		
		protected virtual void TrackViewState ()
		{
			viewState.TrackViewState ();
		}
		
		protected virtual bool IsTrackingViewState
		{
			get { return viewState.IsTrackingViewState; }
		}
	
		void IStateManager.LoadViewState (object savedState)
		{
			LoadViewState (savedState);
		}
		
		object IStateManager.SaveViewState ()
		{
			return SaveViewState ();
		}
		
		void IStateManager.TrackViewState ()
		{
			TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState
		{
			get { return IsTrackingViewState; }
		}
		
		public override string ToString ()
		{
			return GetType().Name;
		}
		
		internal void SetDirty ()
		{
			viewState.SetDirty ();
		}
	
		public abstract string GetCoordinates ();
		
		protected internal abstract string MarkupName { get; }
	}
}

#endif
