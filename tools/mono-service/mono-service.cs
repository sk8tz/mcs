/*
 * monod.cs: Mono daemon for running services based on System.ServiceProcess
 *
 * Author:
 *   Joerg Rosenkranz (joergr@voelcker.com)
 *   Miguel de Icaza (miguel@novell.com)
 *
 * (C) 2005 Voelcker Informatik AG
 * (C) 2005 Novell Inc
 */
using System;
using System.IO;
using System.Reflection;
using Mono.Unix;
using System.ServiceProcess;
using System.Threading;
using System.Runtime.InteropServices;

class MonoServiceRunner {
	static string assembly, directory, lockfile, name, logname;

	static void info (string format, params object [] args)
	{
		Syscall.syslog (SyslogLevel.LOG_INFO, String.Format ("{0}: {1}", assembly, String.Format (format, args)));
	}
	
	static void error (string format, params object [] args)
	{
		Syscall.syslog (SyslogLevel.LOG_ERR, String.Format ("{0}: {1}", assembly, String.Format (format, args)));
	}
	
	static void Usage ()
	{
		Console.Error.WriteLine (
					 "Usage is:\n" +
					 "monod [-d:DIRECTORY] [-l:LOCKFILE] [-n:NAME] [-m:LOGNAME] service.exe\n");
		Environment.Exit (1);
	}

	delegate void sighandler_t (int arg);
	
	static AutoResetEvent signal_event;

	[DllImport ("libc")]
	extern static int signal (int signum, sighandler_t handler);

	static int signum;
	
	static void my_handler (int sig)
	{
		signum = sig;
		signal_event.Set ();
	}

	static void call (object o, string method, object [] arg)
	{
		MethodInfo m = o.GetType ().GetMethod (method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		if (arg != null)
			m.Invoke (o, new object [1] { arg });
		else
			m.Invoke (o, null);
	}
	
	static int Main (string [] args)
	{
		foreach (string s in args){
			if (s.Length > 3 && s [0] == '-' && s [2] == ':'){
				string arg = s.Substring (3);

				switch (Char.ToLower (s [1])){
				case 'd': directory = arg; break;
				case 'l': lockfile = arg; break;
				case 'n': name = arg; break;
				case 'm': logname = arg; break;
				default: Usage (); break;
				}
			} else {
				if (assembly != null)
					Usage ();
				
				assembly = s;
			}
		}

		if (assembly == null){
			error ("Assembly name is missing");
			Usage ();
		}

		if (logname == null)
			logname = assembly;
		
		if (directory != null){
			if (Syscall.chdir (directory) != 0){
				error ("Could not change to directory {0}", directory);
				return 1;
			}
		}

		Assembly a = null;
		
		try {
			a = Assembly.LoadFrom (assembly);
		} catch (FileNotFoundException fnf) {
			error ("Could not find assembly {0}", assembly);
			return 1;
		} catch (BadImageFormatException){
			error ("File {0} is not a valid assembly", assembly);
			return 1;
		} catch { }
		
		if (a == null){
			error ("Could not load assembly {0}", assembly);
			return 1;
		}

		if (lockfile == null)
			lockfile = String.Format ("/tmp/{0}.lock", Path.GetFileName (assembly));

		MethodInfo entry = a.EntryPoint;
		if (entry == null){
			error ("Entry point not defined in service");
			return 1;
		}

		string [] service_args = new string [0];
		entry.Invoke (null, service_args);

		FieldInfo fi = typeof (ServiceBase).GetField ("RegisteredServices", BindingFlags.Static | BindingFlags.NonPublic);
		if (fi == null){
			error ("Internal Mono Error: Could not find RegisteredServices in ServiceBase");
			return 1;
		}

		ServiceBase [] services = (ServiceBase []) fi.GetValue (null);
		if (services == null || services.Length == 0){
			error ("No services were registered by this service");
			return 1;
		}

		//
		// Setup signals
		//
		signal_event = new AutoResetEvent (false);

		// Invoke all the code used in the signal handler, so the JIT does
		// not kick-in inside the signal handler
		signal_event.Set ();
		signal_event.Reset ();

		// Hook up 
		signal (UnixConvert.FromSignum (Signum.SIGTERM), my_handler);
		signal (UnixConvert.FromSignum (Signum.SIGUSR1), my_handler);
		signal (UnixConvert.FromSignum (Signum.SIGUSR2), my_handler);

		// Start up the service.

		ServiceBase service;
		
		if (name != null){
			foreach (ServiceBase svc in services){
				if (svc.ServiceName == name){
					service = svc;
					break;
				}
			}
		} else {
			service = services [0];
		}

		call (service, "OnStart", new string [0]);
		info ("Service {0} started", service.ServiceName);

		for (bool running = true; running; ){
			signal_event.WaitOne ();
			Signum v;
			
			if (UnixConvert.TryToSignum (signum, out v)){
				signum = 0;
				
				switch (v){
				case Signum.SIGTERM:
					info ("Stopping service {0}", service.ServiceName);
					call (service, "OnStop", null);
					running = false;
					break;
				case Signum.SIGUSR1:
					info ("Pausing service {0}", service.ServiceName);
					call (service, "OnPause", null);
					break;
				case Signum.SIGUSR2:
					info ("Continuing service {0}", service.ServiceName);
					call (service, "OnContinue", null);
					break;
				}
			}
		}

		
		return 0;
	}
}
