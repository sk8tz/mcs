
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
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : Calendar
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.WebControls;

namespace System.Web.UI.MobileControls
{
	public class Calendar : MobileControl, IPostBackEventHandler
	{
		private System.Web.UI.WebControls.Calendar webCal;
		private static readonly object SelectionChangedEvent = new object();

		public Calendar()
		{
			webCal = CreateWebCalendar();
			webCal.Visible = false;
			webCal.Controls.Add(webCal);
			webCal.SelectionChanged += new EventHandler(WebSelectionChanged);
		}

		protected virtual System.Web.UI.WebControls.Calendar CreateWebCalendar()
		{
			return new System.Web.UI.WebControls.Calendar();
		}

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			if(MobilePage.ActiveForm != Form)
				MobilePage.ActiveForm = Form;
			Adapter.HandlePostBackEvent(eventArgument);
		}

		public event EventHandler SelectionChanged
		{
			add
			{
				Events.AddHandler(SelectionChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(SelectionChangedEvent, value);
			}
		}

		public string CalendarEntryText
		{
			get
			{
				object o = ViewState["CalendarEntryText"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["CalendarEntryText"] = value;
			}
		}

		public FirstDayOfWeek FirstDayOfWeek
		{
			get
			{
				return webCal.FirstDayOfWeek;
			}
			set
			{
				webCal.FirstDayOfWeek = value;
			}
		}

		public DateTime SelectedDate
		{
			get
			{
				return webCal.SelectedDate;
			}
			set
			{
				webCal.SelectedDate = value;
			}
		}

		public SelectedDatesCollection SelectedDates
		{
			get
			{
				return webCal.SelectedDates;
			}
		}

		public CalendarSelectionMode SelectionMode
		{
			get
			{
				return webCal.SelectionMode;
			}
			set
			{
				webCal.SelectionMode = value;
			}
		}

		public bool ShowDayHeader
		{
			get
			{
				return webCal.ShowDayHeader;
			}
			set
			{
				webCal.ShowDayHeader = value;
			}
		}

		public DateTime VisibleDate
		{
			get
			{
				return webCal.VisibleDate;
			}
			set
			{
				webCal.VisibleDate = value;
			}
		}

		public System.Web.UI.WebControls.Calendar WebCalendar
		{
			get
			{
				return webCal;
			}
		}

		private void WebSelectionChanged(object sender, EventArgs e)
		{
			OnSelectionChanged();
		}

		protected virtual void OnSelectionChanged()
		{
			EventHandler eh = (EventHandler)(Events[SelectionChangedEvent]);
			if(eh != null)
			{
				eh(this, new EventArgs());
			}
		}

		public void RaiseSelectionChangedEvent()
		{
			WebSelectionChanged(this, new EventArgs());
		}
	}
}
