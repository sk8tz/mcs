
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
/**
 * Namespace:   System.Web.UI.Design.WebControls
 * Class:       RepeaterDesigner
 *
 * Author:      Gaurav Vaish
 * Maintainer:  gvaish_mono@lycos.com
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections;
using System.Data;
using System.Web.UI.WebControls;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls
{
	public class RepeaterDesigner : ControlDesigner, IDataSourceProvider
	{
		private DataTable desTimeDataTable;
		private DataTable dummyDataTable;
		private Repeater  repeater;

		public RepeaterDesigner()
		{
		}

		public string DataMember
		{
			get
			{
				return repeater.DataMember;
			}
			set
			{
				repeater.DataMember = value;
			}
		}

		public string DataSource
		{
			get
			{
				DataBinding db = DataBindings["DataSource"];
				if(db != null)
					return db.Expression;
				return String.Empty;
			}
			set
			{
				if(value == null || value.Length == 0)
				{
					DataBindings.Remove("DataSource");
				} else
				{
					DataBinding toSet = new DataBinding("DataSource",
					                                    typeof(IEnumerable), value);
					toSet.Expression = value;
					DataBindings.Add(toSet);
				}
				OnDataSourceChanged();
				OnBindingsCollectionChanged("DataSource");
			}
		}

		public virtual void OnDataSourceChanged()
		{
			desTimeDataTable = null;
		}

		protected bool TemplateExists
		{
			get
			{
				return (repeater.ItemTemplate != null ||
				        repeater.HeaderTemplate != null ||
				        repeater.FooterTemplate != null ||
				        repeater.AlternatingItemTemplate != null);
			}
		}

		protected IEnumerable GetDesignTimeDataSource(int minimumRows)
		{
			return GetDesignTimeDataSource(GetResolvedSelectedDataSource(),
			                               minimumRows);
		}

		protected IEnumerable GetDesignTimeDataSource(IEnumerable selectedDataSource,
		                                              int minimumRows)
		{
			DataTable toDeploy = desTimeDataTable;
			if(toDeploy == null)
			{
				if(selectedDataSource != null)
				{
					desTimeDataTable = DesignTimeData.CreateSampleDataTable(
					                                  selectedDataSource);
					toDeploy = desTimeDataTable;
				} else
				{
					if(dummyDataTable == null)
						dummyDataTable = DesignTimeData.CreateDummyDataTable();
					toDeploy = dummyDataTable;
				}
			}
			return DesignTimeData.GetDesignTimeDataSource(toDeploy,
			                                              minimumRows);
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
				repeater = null;
			base.Dispose(disposing);
		}

		public object GetSelectedDataSource()
		{
			object retVal = null;
			DataBinding db = DataBindings["DataSource"];
			if(db != null)
			{
				retVal = DesignTimeData.GetSelectedDataSource(repeater, db.Expression);
			}
			return retVal;
		}

		public virtual IEnumerable GetResolvedSelectedDataSource()
		{
			IEnumerable retVal = null;
			DataBinding db = DataBindings["DataSource"];
			if(db != null)
			{
				retVal = DesignTimeData.GetSelectedDataSource(repeater, db.Expression, DataMember);
			}
			return retVal;
		}
	}
}
