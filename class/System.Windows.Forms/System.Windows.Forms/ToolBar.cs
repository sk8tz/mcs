//
// System.Windows.Forms.ToolBar
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.Collections;
namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

    public class ToolBar : Control {

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public ToolBar() 
		{
			throw new NotImplementedException ();
		}
		//
		// --- Public Properties
		//
		[MonoTODO]
		public ToolBarAppearance Appearance {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool AutoSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public BorderStyle BorderStyle{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ToolBar.ToolBarButtonCollection Buttons {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Size ButtonSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Divider {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override DockStyle Dock{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool DropDownArrows {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ImageList ImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Size ImageSize {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public new ImeMode ImeMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowToolTips {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string Text {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ToolBarTextAlign TextAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Wrappable{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event ToolBarButtonClickEventHandler ButtonClick;
		[MonoTODO]
		public event ToolBarButtonClickEventHandler ButtonDropDown;
		//
		// --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				throw new NotImplementedException ();
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void Dispose(bool disposing) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnButtonClick(ToolBarButtonClickEventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnButtonDropDown(ToolBarButtonClickEventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message m) {
			throw new NotImplementedException ();
		}
		public class ToolBarButtonCollection : IList, ICollection, IEnumerable {
			//
			// --- Public Constructor
			//
			[MonoTODO]
			public ToolBarButtonCollection(ToolBar owner)
			{
				throw new NotImplementedException ();
			}
			//
			// --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public virtual ToolBarButton this[int index] {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}
			//
			// --- Public Methods
			//
			[MonoTODO]
			public int Add(string text) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int Add(ToolBarButton button) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(ToolBarButton[] buttons) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Clear() {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public bool Contains(ToolBarButton button) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator() {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(ToolBarButton button) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Insert(int index, ToolBarButton button) {
				throw new NotImplementedException ();
			}
			//[MonoTODO]
			//public void Insert(int index, ToolBarButton button) {
			//	throw new NotImplementedException ();
			//}
			[MonoTODO]
			public void Remove(ToolBarButton button) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void RemoveAt(int index) {
				throw new NotImplementedException ();
			}
		}
	}
}

