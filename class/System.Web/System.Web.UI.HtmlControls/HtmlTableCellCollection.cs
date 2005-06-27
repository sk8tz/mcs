/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Collections;

namespace System.Web.UI.HtmlControls{
	public sealed class HtmlTableCellCollection : ICollection {
		private HtmlTableRow _owner;
		
		internal HtmlTableCellCollection(HtmlTableRow owner){
			_owner = owner;
		}
		
		public void Add(HtmlTableCell cell){
			Insert(-1, cell);
		}
		
		public void Clear(){
			if (_owner.HasControls()) _owner.Controls.Clear();
		}
		
		public void CopyTo(Array array, int index){
			IEnumerator tablecell = this.GetEnumerator();
			while(tablecell.MoveNext()){
				index = index + 1;
				array.SetValue(tablecell.Current, index);
			}
		}
		
		public IEnumerator GetEnumerator(){
			return _owner.Controls.GetEnumerator();
		}
		
		public void Insert(int index, HtmlTableCell cell){
			_owner.Controls.AddAt(index,cell);
		}
		
		public void Remove(HtmlTableCell cell){
			_owner.Controls.Remove(cell);
		}
		
		public void RemoveAt(int index){
			_owner.Controls.RemoveAt(index);
		}
		
		public int Count {
			get{
				if (_owner.HasControls()) return _owner.Controls.Count;
				return 0;
			}
		}
		
		public bool IsReadOnly {
			get{
				return false;
			}
		}
		
		public bool IsSynchronized {
			get{
				return false;
			}
		}
		
		public HtmlTableCell this[int index] {
			get{
				return _owner.Controls[index] as HtmlTableCell;
			}
		}
		
		public object SyncRoot {
			get{
				return null;
			}
		}
		
	} // end of System.Web.UI.HtmlControls.HtmlTableCellCollection
	
}
