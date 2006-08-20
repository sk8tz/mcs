//
// System.Diagnostics.EventLogImpl.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Atsushi Enomoto  <atsushi@ximian.com>
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2003 Andreas Nahr
// (C) 2006 Novell, Inc.
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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Win32;

namespace System.Diagnostics
{
	internal abstract class EventLogImpl
	{
		readonly EventLog _coreEventLog;

		protected EventLogImpl (EventLog coreEventLog)
		{
			_coreEventLog = coreEventLog;
		}

		public event EntryWrittenEventHandler EntryWritten;

		protected EventLog CoreEventLog {
			get { return _coreEventLog; }
		}

		public int EntryCount {
			get {
				if (_coreEventLog.Log == null || _coreEventLog.Log.Length == 0) {
					throw new ArgumentException ("Log property is not set.");
				}

				if (!EventLog.Exists (_coreEventLog.Log, _coreEventLog.MachineName)) {
					throw new InvalidOperationException (string.Format (
						CultureInfo.InvariantCulture, "The event log '{0}' on "
						+ " computer '{1}' does not exist.", _coreEventLog.Log,
						_coreEventLog.MachineName));
				}

				return GetEntryCount ();
			}
		}

		public EventLogEntry this[int index] {
			get {
				if (_coreEventLog.Log == null || _coreEventLog.Log.Length == 0) {
					throw new ArgumentException ("Log property is not set.");
				}

				if (!EventLog.Exists (_coreEventLog.Log, _coreEventLog.MachineName)) {
					throw new InvalidOperationException (string.Format (
						CultureInfo.InvariantCulture, "The event log '{0}' on "
						+ " computer '{1}' does not exist.", _coreEventLog.Log,
						_coreEventLog.MachineName));
				}

				if (index < 0 || index >= EntryCount)
					throw new ArgumentException ("Index out of range");

				return GetEntry (index);
			}
		}

		public string LogDisplayName {
			get {
#if NET_2_0
				// to-do perform valid character checks
				if (_coreEventLog.Log != null && _coreEventLog.Log.Length == 0) {
					throw new InvalidOperationException ("Event log names must"
						+ " consist of printable characters and cannot contain"
						+ " \\, *, ?, or spaces.");
				}
#endif
				if (_coreEventLog.Log != null) {
					if (!EventLog.Exists (_coreEventLog.Log, _coreEventLog.MachineName)) {
						throw new InvalidOperationException (string.Format (
							CultureInfo.InvariantCulture, "Cannot find Log {0}"
							+ " on computer {1}.", _coreEventLog.Log,
							_coreEventLog.MachineName));
					}
				}

				return GetLogDisplayName ();
			}
		}

		public EventLogEntry [] GetEntries ()
		{
			string logName = CoreEventLog.Log;
			if (logName == null || logName.Length == 0)
				throw new ArgumentException ("Log property value has not been specified.");

			if (!EventLog.Exists (logName))
				throw new InvalidOperationException (string.Format (
					CultureInfo.InvariantCulture, "The event log '{0}' on "
					+ " computer '{1}' does not exist.", logName,
					_coreEventLog.MachineName));

			int entryCount = GetEntryCount ();
			EventLogEntry [] entries = new EventLogEntry [entryCount];
			for (int i = 0; i < entryCount; i++) {
				entries [i] = GetEntry (i);
			}
			return entries;
		}

		public abstract void BeginInit ();

		public abstract void Clear ();

		public abstract void Close ();

		public abstract void CreateEventSource (EventSourceCreationData sourceData);

		public abstract void Delete (string logName, string machineName);

		public abstract void DeleteEventSource (string source, string machineName);

		public abstract void Dispose (bool disposing);

		public abstract void EndInit ();

		public abstract bool Exists (string logName, string machineName);

		protected abstract int GetEntryCount ();

		protected abstract EventLogEntry GetEntry (int index);

		public abstract EventLog [] GetEventLogs (string machineName);

		protected abstract string GetLogDisplayName ();

		public abstract string LogNameFromSourceName (string source, string machineName);

		public abstract bool SourceExists (string source, string machineName);

		public abstract void WriteEntry (string [] replacementStrings, EventLogEntryType type, uint instanceID, short category, byte[] rawData);

		protected abstract string FormatMessage (string source, uint messageID, string [] replacementStrings);
	}
}
