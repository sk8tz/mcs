//
// System.Windows.Forms.Label.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {
	using System.Drawing;

	// <summary>
	//
	// </summary>
	
	public class Label : Control {
	
		public Label () : base (){
			this.Text = " ";
			// AutoSize = false;
			// BorderStyle = BorderStyle.None;
		}
		protected override void  OnTextChanged (EventArgs e){
			((Gtk.Label) Widget).Text = Text;
			
		}
		protected override void OnMouseUp (MouseEventArgs e){
			Console.WriteLine ("Mouse up label");
			if (e.Clicks == 2) {
				Console.WriteLine ("Doble-click label");
				OnDoubleClick (EventArgs.Empty);
			}
			if (e.Clicks == 1){
				Console.WriteLine ("Single-click label");
				OnClick (EventArgs.Empty);
			}
			base.OnMouseUp (e);
			//if (MouseUp != null){
			//	MouseUp (this, e);
			//}
		}
		
		internal override Gtk.Widget CreateWidget () {
			return new Gtk.Label (Text);
			//base.ConnectEvents();
		}
		
		[MonoTODO]
		public virtual bool AutoSize{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public virtual BorderStyle BorderStyle{
			get{ return BorderStyle.None; }
			set{
				//InvalidEnumArgumentException		
				//throw new NotImplementedException (); 
			}
		}		
		[MonoTODO]
		public FlatStyle FlatStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//InvalidEnumArgumentException
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Image Image {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ContentAlignment ImageAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				//InvalidEnumArgumentException
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				//ArgumentException
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
		//[MonoTODO]
		//public ImeMode ImeMode {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		[MonoTODO]
		public int PreferredHeight {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int PreferredWidth {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual bool RenderTransparent {
			get { return false; }
			set { return; }
			//get{ throw new NotImplementedException(); }
			//set{ throw new NotImplementedException (); }
		}
		//[MonoTODO]
		//public override bool TabStop {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
		
		[MonoTODO]
		public virtual ContentAlignment TextAlign {
			get {
				//throw new NotImplementedException ();
				return ContentAlignment.TopLeft;
			}
			set {
				//throw new NotImplementedException ();
				
			}
		}
		
		[MonoTODO]
		public bool UseMnemonic {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public override string ToString()
		//{
		//	throw new NotImplementedException ();
		//}

		//
		//  --- Public Events
		// 
		public event EventHandler AutoSizeChanged;
		public event EventHandler TextAlignChanged;
		//[MonoTODO]
		//public event EventHandler AutoSizeChanged {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}
		//[MonoTODO]
		//public event EventHandler TextAlignChanged {
		//	add {
		//		throw new NotImplementedException ();
		//	}
		//	remove {
		//		throw new NotImplementedException ();
		//	}
		//}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected  Rectangle CalcImageRenderBounds( Image image, Rectangle rect,  ContentAlignment align)
		{
			throw new NotImplementedException ();
		}
		
		//[MonoTODO]
		//protected  override AccessibleObject CreateAccessibilityInstance()
		//{
		//	throw new NotImplementedException ();
		//}
		
		//[MonoTODO]
		//protected void Dispose()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected  override void Dispose(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected  void DrawImage(  Graphics g,  Image img,  Rectangle r,  ContentAlignment align)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO] 
		protected virtual void OnAutoSizeChanged (EventArgs e){
			throw new NotImplementedException ();
		}
		
		//[MonoTODO]
		//protected override void  OnEnabledChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void  OnFontChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void  OnPaint (PaintEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void  OnParentChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		

		//[MonoTODO]
		//protected override void  OnVisibleChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override bool ProcessMnemonic(char charCode)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected ContentAlignment RtlTranslateAlignment( ContentAlignment alignment)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected HorizontalAlignment RtlTranslateAlignment( HorizontalAlignment alignment)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected virtual void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void SetBoundsCore(  int x, int y,  int width, int height  BoundsSpecified specified)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected void UpdateBounds(int b1, int b2, int b3, int b4)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void WndProc(ref Message m)
		//{
		//	throw new NotImplementedException ();
		//}

		
	}
}
