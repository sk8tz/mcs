//
// System.Windows.Forms.ComboBox.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Collections;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows combo box control.
   	/// </summary>

	[MonoTODO]
	public class ComboBox : ListControl {

		// private fields
		DrawMode drawMode;
		ComboBoxStyle dropDownStyle;
		bool droppedDown;
		bool integralHeight;
		bool sorted;
		Image backgroundImage;
		ControlStyles controlStyles;
		string text;
		int selectedLength;
		string selectedText;
		int selectedIndex;
		object selectedItem;
		int selecedStart;

		bool updateing; // true when begin update has been called. do not paint when true;
		// --- Constructor ---
		public ComboBox() : base() 
		{
			selectedLength = 0;
			selectedText = "";
			selectedIndex = 0;
			selectedItem = null;
			selecedStart = 0;
			updateing = false;
			//controlStyles = null;
			drawMode = DrawMode.Normal;
			dropDownStyle = ComboBoxStyle.DropDown;
			droppedDown = false;
			integralHeight = true;
			sorted = false;
			backgroundImage = null;
			text = "";
			
		}
		
		// --- Properties ---
		[MonoTODO]
		public override Color BackColor {
			get { 
				return base.BackColor;
			}
			set { 
				if(BackColor.A != 255){
					if(
						(controlStyles & ControlStyles.SupportsTransparentBackColor) != 
						ControlStyles.SupportsTransparentBackColor 
						){
						throw new 
							ArgumentOutOfRangeException("BackColor", BackColor, "Transparant background color not allowed.");
					}
				}
				base.BackColor = value;
			}
		}
		
		public override Image BackgroundImage {
			get {
				return backgroundImage; 
			}
			set { 
				backgroundImage = value;
			}
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "COMBOBOX";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//			createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
				return createParams;
			}		
		}
		
		protected override Size DefaultSize {
			get {
				return new Size(121,21);//correct size
			}
		}
		
		public DrawMode DrawMode {
			get {
				return drawMode;
			}
			set {
				drawMode = value;
			}
		}
		
		public ComboBoxStyle DropDownStyle {
			get {
				return dropDownStyle;
			}
			set {
				dropDownStyle = value;
			}
		}
		
		[MonoTODO]
		public int DropDownWidth {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		public bool DroppedDown {
			get { 
				return droppedDown;
			}
			set {
				droppedDown = value; 
			}
		}
		
		[MonoTODO]
		public override bool Focused {
			get {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public override Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		public bool IntegralHeight {
			get {
				return integralHeight;
			}
			set {
				integralHeight=value;
			}
		}
		
		[MonoTODO]
		public int ItemHeight {
			get {
				throw new NotImplementedException (); 
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public ComboBox.ObjectCollection Items {
			get { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public int MaxDropDownItems {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}
		
		[MonoTODO]
		public int MaxLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}
		
		[MonoTODO]
		public int PreferredHeight {
			get {
				return 20; //FIXME: this is the default, good as any?
			}
		}
	
		[MonoTODO]
		public override int SelectedIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public object SelectedItem {
			get {
				throw new NotImplementedException ();
			}
			set { 
				//FIXME:
			}
		}
		
		[MonoTODO]
		public string SelectedText {
			get { 
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public int SelectionLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public int SelectionStart {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		public bool Sorted {
			get {
				return sorted;
			}
			set {
				sorted = value;
			}
		}
		
		[MonoTODO]
		public override string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}
		
		
		
		
		/// --- Methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		[MonoTODO]
		protected override void OnSelectedValueChanged(EventArgs e){ // .NET V1.1 Beta
			//FIXME:
			base.OnSelectedValueChanged(e);
		}

		/// - protected override void SetItemCore(int index,object value);
		[MonoTODO]
		protected virtual void AddItemsCore(object[] value) {
			//FIXME:		
		}
		
		[MonoTODO]
		public void BeginUpdate() 
		{
			updateing = true;
		}
		
		[MonoTODO]
		public void EndUpdate() 
		{
			updateing = false;
		}
		
		[MonoTODO]
		public int FindString(string s) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindString(string s,int startIndex) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindStringExact(string s) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindStringExact(string s,int startIndex) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int GetItemHeight(int index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) 
		{
			//FIXME:
			return base.IsInputKey(keyData);
		}
		
		/// [methods for events]
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnBackColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnDataSourceChanged(EventArgs e) 
		{
			//FIXME:
			base.OnDataSourceChanged(e);
		}
		
		[MonoTODO]
		protected override void OnDisplayMemberChanged(EventArgs e) 
		{
			//FIXME:
			base.OnDisplayMemberChanged(e);
		}
		
		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e) 
		{
			//FIXME:		
		}
		
		[MonoTODO]
		protected virtual void OnDropDown(EventArgs e) 
		{
			//FIXME:		
		}
		
		[MonoTODO]
		protected virtual void OnDropDownStyleChanged(EventArgs e) 
		{
			//FIXME:		
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) 
		{
			//FIXME:
			base.OnFontChanged(e);
		}
		
		[MonoTODO]
		protected override void OnForeColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnForeColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			//FIXME:
			base.OnHandleCreated(e);
		}
		
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) 
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}
		
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e) 
		{
			//FIXME:
			base.OnKeyPress(e);
		}
		
		[MonoTODO]
		protected virtual void OnMeasureItem(MeasureItemEventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnParentBackColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnParentBackColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			//FIXME:
			base.OnResize(e);
		}
		
		[MonoTODO]
		protected override void OnSelectedIndexChanged(EventArgs e) 
		{
			//FIXME:
			base.OnSelectedIndexChanged(e);
		}
		
		[MonoTODO]
		protected virtual void OnSelectionChangeCommitted(EventArgs e) 
		{
			//FIXME:		
		}
		/// end of [methods for events]
		
		
		[MonoTODO]
		protected override void RefreshItem(int index) 
		{
			//FIXME:
			base.Refresh();
		}
		
		[MonoTODO]
		public void Select(int start,int length) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void SelectAll() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) 
		{
			//FIXME:
			base.SetBoundsCore(x,y,width,height,specified);
		}
		
		// for IList interface
		// FIXME not sure how to handle this
		//[MonoTODO]
		//protected override void SetItemsCore(IList value) 
		//{
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		public override string ToString() 
		{
			//FIXME:
			return base.ToString();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			//FIXME:
			base.WndProc(ref m);
		}
		
	
		/// --- Button events ---
		/// commented out, cause it only supports the .NET Framework infrastructure
		[MonoTODO]
		public event DrawItemEventHandler DrawItem;
		
		[MonoTODO]
		public event EventHandler DropDown;
		
		[MonoTODO]
		public event EventHandler DropDownStyleChanged;
		
		[MonoTODO]
		public event MeasureItemEventHandler MeasureItem;
		
		/* only supports .NET framework
			[MonoTODO]
			public new event PaintEventHandler Paint;
		*/
		
		[MonoTODO]
		public event EventHandler SelectedIndexChanged;
		
		[MonoTODO]
		public event EventHandler SelectionChangeCommitted;
		
		/// --- public class ComboBox.ChildAccessibleObject : AccessibleObject ---
		/// the class is not stubbed, cause it's only used for .NET framework
		
		
		/// sub-class: ComboBox.ObjectCollection
		/// <summary>
		/// Represents the collection of items in a ComboBox.
		/// </summary>
		[MonoTODO]
		public class ObjectCollection : IList, ICollection, IEnumerable {
			
			/// --- ObjectCollection.constructor ---
			[MonoTODO]
			public ObjectCollection (ComboBox owner) 
			{
				
			}
			
			/// --- ObjectCollection Properties ---
			[MonoTODO]
			public int Count {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public int this[int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}

			/// --- ICollection properties ---
			bool IList.IsFixedSize {
				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			object IList.this[int index] {

				[MonoTODO] get { throw new NotImplementedException (); }
				[MonoTODO] set { throw new NotImplementedException (); }
			}
	
			object ICollection.SyncRoot {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
	
			bool ICollection.IsSynchronized {

				[MonoTODO] get { throw new NotImplementedException (); }
			}
			
			/// --- methods ---
			/// --- ObjectCollection Methods ---
			/// Note: IList methods are stubbed out, otherwise IList interface cannot be implemented
			[MonoTODO]
			public int Add(object item) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddRange(object[] items) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Clear() 
			{
				//FIXME:		
			}
			
			[MonoTODO]
			public bool Contains(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void CopyTo(object[] dest,int arrayIndex) 
			{
				throw new NotImplementedException ();
			}
			
			/// for ICollection:
			[MonoTODO]
			void ICollection.CopyTo(Array dest,int index) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int IndexOf(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void Insert(int index,object item) 
			{
				//FIXME:		
			}
			
			[MonoTODO]
			public void Remove(object value) 
			{
				//FIXME:		
			}
			
			[MonoTODO]
			public void RemoveAt(int index) 
			{
				//FIXME:		
			}
		}  // --- end of ComboBox.ObjectCollection ---
	}
}
