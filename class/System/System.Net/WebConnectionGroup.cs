//
// System.Net.WebConnectionGroup
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Net.Configuration;
using System.Net.Sockets;

namespace System.Net
{
	class WebConnectionGroup
	{
		ServicePoint sPoint;
		string name;
		ArrayList connections;
		static ConnectionManagementData manager;
		Random rnd;

		public WebConnectionGroup (ServicePoint sPoint, string name)
		{
			this.sPoint = sPoint;
			this.name = name;
			connections = new ArrayList (1);
		}

		public WebConnection GetConnection ()
		{
			WebConnection cnc = null;
			lock (connections) {
				WeakReference cncRef = null;

				// Remove disposed connections
				int end = connections.Count;
				ArrayList removed = null;
				for (int i = 0; i < end; i++) {
					cncRef = (WeakReference) connections [i];
					cnc = cncRef.Target as WebConnection;
					if (cnc == null) {
						if (removed == null)
							removed = new ArrayList (1);

						removed.Add (i);
					}
				}

				if (removed != null) {
					for (int i = removed.Count - 1; i >= 0; i--)
						connections.RemoveAt ((int) removed [i]);
				}

				cnc = CreateOrReuseConnection ();
			}

			return cnc;
		}

		WebConnection CreateOrReuseConnection ()
		{
			// lock is up there.
			WebConnection cnc;
			WeakReference cncRef;

			int count = connections.Count;
			if (sPoint.ConnectionLimit > count) {
				cnc = new WebConnection (this, sPoint);
				connections.Add (new WeakReference (cnc));
				return cnc;
			}

			if (rnd == null)
				rnd = new Random ();

			foreach (WeakReference wr in connections) {
				cnc = wr.Target as WebConnection;
				if (cnc.Busy)
					continue;

				return cnc;
			}

			int idx = (count > 1) ? rnd.Next (0, count - 1) : 0;
			cncRef = (WeakReference) connections [idx];
			cnc = cncRef.Target as WebConnection;
			if (cnc == null) {
				cnc = new WebConnection (this, sPoint);
				connections.RemoveAt (idx);
				connections.Add (new WeakReference (cnc));
			}
			return cnc;
		}

		public string Name {
			get { return name; }
		}
		
	}
}

