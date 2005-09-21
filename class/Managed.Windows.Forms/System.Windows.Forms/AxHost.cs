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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	<pbartok@novell.com>
//
//

// NOT COMPLETE

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Windows.Forms {
	[MonoTODO("Possibly implement this for Win32; find a way for Linux and Mac")]
	[DefaultEvent("Enter")]
	[Designer("System.Windows.Forms.Design.AxHostDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public abstract class AxHost : Control, ISupportInitialize, ICustomTypeDescriptor {
		#region AxHost Subclasses
			#region AxHost.ActiveXInvokeKind Enum
			public enum ActiveXInvokeKind {
				MethodInvoke	= 0,
				PropertyGet	= 1,
				PropertySet	= 2
			}
			#endregion	// AxHost.ActiveXInvokeKind Enum

			#region AxHost.AxComponentEditor Class
			public class AxComponentEditor : System.Windows.Forms.Design.WindowsFormsComponentEditor {
				public AxComponentEditor() {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public override bool EditComponent(ITypeDescriptorContext context, object obj, IWin32Window parent) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}
			}
			#endregion	// AxHost.AxComponentEditor Class

			#region AxHost.ClsidAttribute
			[AttributeUsage(AttributeTargets.Class,Inherited=false)]
			public sealed class ClsidAttribute : Attribute {
				string clsid;

				public ClsidAttribute (string clsid) {
					this.clsid = clsid;
				}

				public string Value {
					get {
						return clsid;
					}
				}
			}
			#endregion AxHost.ClsidAttribute

			#region AxHost.ConnectionPointCookie
			[ComVisible(false)]
			public class ConnectionPointCookie {
				public ConnectionPointCookie(object source, object sink, Type eventInterface) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public void Disconnect() {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				~ConnectionPointCookie() {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}
			}
			#endregion	// AxHost.ConnectionPointCookie
		
			#region AxHost.InvalidActiveXStateException  Class
			public class InvalidActiveXStateException : Exception {
				public InvalidActiveXStateException(string name, ActiveXInvokeKind kind) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public override string ToString() {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}
			}
			#endregion	// AxHost.InvalidActiveXStateException  Class
			
			#region AxHost.State Class
			[Serializable]
			[TypeConverter("System.ComponentModel.TypeConverter, " + Consts.AssemblySystem)]
			public class State : ISerializable {
				public State(Stream ms, int storageType, bool manualUpdate, string licKey) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				void ISerializable.GetObjectData(SerializationInfo si,StreamingContext context) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}
			}
			#endregion	// AxHost.State Class

			#region AxHost.TypeLibraryTimeStampAttribute Class
			[AttributeUsage(AttributeTargets.Assembly, Inherited=false)]
			public sealed class TypeLibraryTimeStampAttribute : Attribute {
				public TypeLibraryTimeStampAttribute(string timestamp) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public DateTime Value {
					get {
						throw new NotImplementedException("COM/ActiveX support is not implemented");
					}
				}
			}
			#endregion	// AxHost.TypeLibraryTimeStampAttribute Class

			#region AxHost.StateConverter Class
			public class StateConverter : System.ComponentModel.TypeConverter {
				public StateConverter() {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}

				public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) {
					throw new NotImplementedException("COM/ActiveX support is not implemented");
				}
			}
			#endregion	// AxHost.StateConverter Class
		#endregion	// AxHost Subclasses

		string text;

		#region Protected Constructors
		protected AxHost(string clsid) {
			text = "";
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected AxHost(string clsid, int flags) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		#endregion	// Public Instance Properties

		#region Public Instance Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color BackColor {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public ContainerControl ContainingControl {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ContextMenu ContextMenu {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool EditMode {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new virtual bool Enabled {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Font Font {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { 
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool HasAboutBox {
			get { 
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[DefaultValue(null)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[RefreshProperties(RefreshProperties.All)]
		public AxHost.State OcxState {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Localizable(true)]
		public new virtual bool RightToLeft {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}

			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		public override ISite Site {
			set {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get {
				return text;
			}

			set {
				text = value;
			}
		}
		#endregion	// Protected Constructors
		
		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				throw new NotImplementedException("COM/ActiveX support is not implemented");
			}
		}
		
		protected override Size DefaultSize {
			get {
				return new Size (75, 23);
			}
		}
		#endregion	// Protected Instance Properties

		#region Protected Static Methods
		[CLSCompliant(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static Color GetColorFromOleColor(uint color){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static Font GetFontFromIFont(object font){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static Font GetFontFromIFontDisp(object font){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static object GetIFontDispFromFont(Font font){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static object GetIFontFromFont(Font font){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static object GetIPictureDispFromPicture(Image image){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static object GetIPictureFromCursor(Cursor cursor){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static object GetIPictureFromPicture(Image image){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static double GetOADateFromTime(DateTime time){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[CLSCompliant(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static uint GetOleColorFromColor(Color color){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static Image GetPictureFromIPicture(object picture){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static Image GetPictureFromIPictureDisp(object picture){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected static DateTime GetTimeFromOADate(double date){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}
		#endregion	// Protected Static Methods

		#region Public Instance Methods
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void BeginInit() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		public void DoVerb(int verb){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual void EndInit() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public object GetOcx() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		public bool HasPropertyPages() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void InvokeEditMode(){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void MakeDirty(){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		public override bool PreProcessMessage(ref Message msg) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		public void ShowAboutBox() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		public void ShowPropertyPages() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		public void ShowPropertyPages(Control control) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void AttachInterfaces() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void CreateHandle() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void CreateSink(){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void DestroyHandle() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void DetachSink(){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected override void Dispose(bool disposing) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override bool IsInputChar(char charCode) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void OnBackColorChanged(EventArgs e) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void OnFontChanged(EventArgs e) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void OnForeColorChanged(EventArgs e) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void OnHandleCreated(EventArgs e) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected virtual void OnInPlaceActive() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnLostFocus(EventArgs e) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override bool ProcessDialogKey(Keys keyData) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected override bool ProcessMnemonic(char charCode) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected bool PropsValid(){
			throw new NotImplementedException("COM/ActiveX support is not implemented"); 
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseDown(short button, short shift, int x, int y){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseDown(short button, short shift, float x, float y){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseDown(object o1, object o2, object o3, object o4){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseMove(short button, short shift, int x, int y){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseMove(short button, short shift, float x, float y){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseMove(object o1, object o2, object o3, object o4){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseUp(short button, short shift, int x, int y){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseUp(short button, short shift, float x, float y){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void RaiseOnMouseUp(object o1, object o2, object o3, object o4){
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}

		protected void SetAboutBoxDelegate(AxHost.AboutBoxDelegate d) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void SetVisibleCore(bool value) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		protected override void WndProc(ref Message m) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		#endregion	// Protected Instance Methods

		#region	Private Instance Methods
		[EditorBrowsable(EditorBrowsableState.Never)]
		private bool ShouldSerializeContainingControl() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		#endregion	// Protected Instance Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		BackColorChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		BackgroundImageChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		BindingContextChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event UICuesEventHandler		ChangeUICues;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		Click;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		ContextMenuChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		CursorChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		DoubleClick;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event DragEventHandler		DragDrop;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event DragEventHandler		DragEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		DragLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event DragEventHandler		DragOver;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		EnabledChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		FontChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		ForeColorChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event GiveFeedbackEventHandler	GiveFeedback;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event HelpEventHandler		HelpRequested;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		ImeModeChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event KeyEventHandler		KeyDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event KeyPressEventHandler	KeyPress;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event KeyEventHandler		KeyUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event LayoutEventHandler		Layout;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event MouseEventHandler		MouseDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		MouseEnter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		MouseHover;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		MouseLeave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event MouseEventHandler		MouseMove;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event MouseEventHandler		MouseUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event MouseEventHandler		MouseWheel;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event PaintEventHandler		Paint;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event QueryAccessibilityHelpEventHandler	QueryAccessibilityHelp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event QueryContinueDragEventHandler	QueryContinueDrag;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		RightToLeftChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		StyleChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		TabIndexChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		TabStopChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler		TextChanged;
		#endregion	// Events

		#region Delegates
		[Serializable]
		protected delegate void AboutBoxDelegate();
		#endregion	// Delegates

		#region	Interfaces
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		AttributeCollection ICustomTypeDescriptor.GetAttributes() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		string ICustomTypeDescriptor.GetClassName() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		string ICustomTypeDescriptor.GetComponentName() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		TypeConverter ICustomTypeDescriptor.GetConverter() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
			throw new NotImplementedException("COM/ActiveX support is not implemented");
		}
		#endregion	// Interfaces
	}
}
