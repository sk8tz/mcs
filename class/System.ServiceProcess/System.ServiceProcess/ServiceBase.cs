//
// System.ServiceProcess.ServiceBase.cs
//
// Authors:
//      Cesar Octavio Lopez Nataren (cesar@ciencias.unam.mx)
//      Duncan Mak (duncan@ximian.com)
//
// (C) 2003, Ximian Inc and Cesar Octavio Lopez Nataren.
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
using System.Globalization;
using System.Diagnostics;

namespace System.ServiceProcess
{
	public class ServiceBase : System.ComponentModel.Component
	{
		public ServiceBase() { }

                public const int MaxNameLength = 80;

                bool hasStarted;
                
                bool auto_log;
                bool can_handle_power_event;
                bool can_pause_and_continue;
                bool can_shutdown;
                bool can_stop;
                EventLog event_log;
                string service_name;

                public bool AutoLog {

                        get { return auto_log; }

                        set { auto_log = value; }
                }

                public bool CanHandlePowerEvent {

                        get { return can_handle_power_event; }

                        set {
                                if (hasStarted)
                                        throw new InvalidOperationException (
                                                Locale.GetText ("Cannot modify this property " +
                                                                "after the service has started."));

                                can_handle_power_event = value;
                        }
                }

                public bool CanPauseAndContinue {

                        get { return can_pause_and_continue; }

                        set {
                                if (hasStarted)
                                        throw new InvalidOperationException (
                                                Locale.GetText ("Cannot modify this property " +
                                                                "after the service has started."));

                                can_pause_and_continue = value;
                        }
                }

                public bool CanShutdown {

                        get { return can_shutdown; }

                        set {
                                if (hasStarted)
                                        throw new InvalidOperationException (
                                                Locale.GetText ("Cannot modify this property " +
                                                                "after the service has started."));

                                can_shutdown = value;
                        }
                }

                public bool CanStop {

                        get { return can_stop; }

                        set {
                                if (hasStarted)
                                        throw new InvalidOperationException (
                                                Locale.GetText ("Cannot modify this property " +
                                                                "after the service has started."));

                                can_stop = value;
                        }
                }
                
                public virtual EventLog EventLog {
                        get { return event_log; }
                }

                public string ServiceName {

                        get { return service_name; }

                        set {
                                if (hasStarted)
                                        throw new InvalidOperationException (
                                                Locale.GetText ("Cannot modify this property " +
                                                                "after the service has started."));

                                service_name = value;
                        }
                }
                                
		protected override void Dispose (bool disposing) { }

		protected virtual void OnStart (string [] args) { }

		protected virtual void OnStop () { }
		
		protected virtual void OnContinue () { }

		protected virtual void OnCustomCommand () { }

		protected virtual void OnPause () { }

		protected virtual void OnPowerEvent () { }

		protected virtual void OnShutdown () { }

                public static void Run (ServiceBase service) { }

		public static void Run (ServiceBase [] ServicesToRun) { }

	}
}
