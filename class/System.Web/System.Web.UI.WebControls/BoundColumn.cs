//
// System.Web.UI.WebControls.BoundColumn.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
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
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class BoundColumn : DataGridColumn
	{
		public static readonly string thisExpr = "!";

		private bool boundFieldDescriptionValid;
		private string boundField;
		private string formatting;
		
		private PropertyDescriptor desc;

		public BoundColumn(): base()
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			desc       = null;
			boundField = DataField;
			formatting = DataFormatString;
			boundFieldDescriptionValid = false;
		}

		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);

			Control bindCtrl = null;
			Control toAdd    = null;
			switch(itemType)
			{
				case ListItemType.Item : goto case ListItemType.SelectedItem;
				case ListItemType.AlternatingItem
				                       : goto case ListItemType.SelectedItem;
				case ListItemType.SelectedItem
				                       : if(DataField.Length != 0)
				                         	bindCtrl = cell;
				                         break;
				case ListItemType.EditItem
					                   : if(!ReadOnly)
					                     {
					                     	TextBox box = new TextBox();
								toAdd = box;
								if(DataField.Length != 0)
									bindCtrl = box;
							     } else 
								     goto case ListItemType.SelectedItem;

					                     break;
			}
			if(toAdd != null)
				cell.Controls.Add(toAdd);
			if(bindCtrl != null)
				bindCtrl.DataBinding += new EventHandler(OnDataBindColumn);
			//throw new NotImplementedException();
		}

		private void OnDataBindColumn(object sender, EventArgs e)
		{
			Control senderCtrl = (Control)sender;
			DataGridItem item  = (DataGridItem)senderCtrl.NamingContainer;
			object       data  = item.DataItem;

			if(!boundFieldDescriptionValid)
			{
				if(boundField != BoundColumn.thisExpr)
				{
					desc = TypeDescriptor.GetProperties(data).Find(boundField, true);
					if(desc == null && !DesignMode)
					{
						throw new HttpException(
						          HttpRuntime.FormatResourceString("File_Not_Found",
						                                           boundField));
					}
					boundFieldDescriptionValid = true;
				}
			}
			object value = data;
			string val = String.Empty;
			if(desc == null && DesignMode)
			{
				throw new NotImplementedException();
			} else
			{
				if(desc != null)
					value = desc.GetValue(data);
				val = FormatDataValue(value);
			}
			if(senderCtrl is TableCell)
			{
				if(val.Length == 0)
					val = "&nbsp;";
				((TableCell)senderCtrl).Text = val;
			} else
			{
				((TextBox)senderCtrl).Text = val;
			}
		}

		[DefaultValue (""), WebCategory ("Misc")]
		[WebSysDescription ("The field that this column is bound to.")]
		public virtual string DataField
		{
			get
			{
				object o = ViewState["DataField"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataField"] = value;
				OnColumnChanged();
			}
		}

		[DefaultValue (""), WebCategory ("Misc")]
		[WebSysDescription ("A format string that is applied to the data value.")]
		public virtual string DataFormatString
		{
			get
			{
				object o = ViewState["DataFormatString"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataFormatString"] = value;
				OnColumnChanged();
			}
		}

		[DefaultValue (false), WebCategory ("Misc")]
		[WebSysDescription ("Determines if the databound field can only be displayed or also edited.")]
		public virtual bool ReadOnly
		{
			get
			{
				object o = ViewState["ReadOnly"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ReadOnly"] = value;
			}
		}

		protected virtual string FormatDataValue(Object dataValue)
		{
			string retVal = String.Empty;
			if(dataValue != null)
			{
				if(formatting.Length == 0)
					retVal = dataValue.ToString();
				else
					retVal = String.Format(formatting, dataValue);
			}
			return retVal;
		}
	}
}
