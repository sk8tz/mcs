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
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2004 Novell, Inc.
//

#if NET_2_0
using System;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms {

	[Serializable]
	public sealed class TableLayoutSettings : LayoutSettings {
		//TableLayoutPanel panel;
		ColumnStyleCollection column_style;
		TableLayoutPanelGrowStyle grow_style;
		LayoutEngine layout_engine;
		int column_count;
		int row_count;

		internal TableLayoutSettings (TableLayoutPanel panel)
		{
			//this.panel = panel;
			column_count = 0;
			row_count = 0;
			grow_style = TableLayoutPanelGrowStyle.AddRows;
			column_style = new ColumnStyleCollection (panel);
			layout_engine = new TableLayout ();
		}

		public int ColumnCount {
			get {
				return column_count;
			}

			set {
				column_count = value;
			}
		}

		public int RowCount {
			get {
				return row_count;
			}

			set {
				row_count = value;
			}
		}

		public TableLayoutPanelGrowStyle GrowStyle {
			get {
				return grow_style;
			}
		}

		public override LayoutEngine LayoutEngine {
			get {
				return layout_engine;
			}
		}
				
		public TableLayoutSettings.ColumnStyleCollection ColumnStyles {
			get {
				return column_style;
			}
		}

		public abstract class StyleCollection {
			ArrayList al = new ArrayList ();
			TableLayoutPanel table;
			
			internal StyleCollection (TableLayoutPanel table)
			{
				this.table = table;
			}
			
			public int Add (TableLayoutSettings.Style style)
			{
				return al.Add (style);
			}

			// FIXME; later this should be an override.
			public void Clear ()
			{
				al.Clear ();

				// FIXME: Need to investigate what happens when the style is gone.
				table.Relayout ();
			}

#region IList methods
			//
			// The IList methods will later be implemeneted, this is to get us started
			//
			internal bool Contains (Style style)
			{
				return al.Contains (style);
			}

			internal int IndexOf (Style style)
			{
				return al.IndexOf (style);
			}

			internal void Insert (int index, Style style)
			{
				al.Insert (index, style);
			}

			internal void Remove (Style style)
			{
				al.Remove (style);
			}

#endregion
			public Style this [int idx] {
				get {
					return (Style) al [idx];
				}

				set {
					al [idx] = value;
				}
			}
		}
		
		public class ColumnStyleCollection : StyleCollection {

			internal ColumnStyleCollection (TableLayoutPanel panel) : base (panel)
			{
			}
			
			public void Add (ColumnStyle style)
			{
				base.Add (style);
			}

			public bool Contains (ColumnStyle style)
			{
				return base.Contains (style);
			}

			public int IndexOf (ColumnStyle style)
			{
				return base.IndexOf (style);
			}

			public void Insert (int index, ColumnStyle style)
			{
				base.Insert (index, style);
			}

			public void Remove (ColumnStyle style)
			{
				base.Remove (style);
			}

			public new ColumnStyle this [int index] {
				get {
					return (ColumnStyle) base [index];
				}

				set {
					base [index] = value;
				}
			}
		}

		public class Style {
			internal SizeType size_type;
			
			public SizeType SizeType {
				get {
					return size_type;
				}

				set {
					size_type = value;
				}
			}
		}
	}

}
#endif
