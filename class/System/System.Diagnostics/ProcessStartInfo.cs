//
// System.Diagnostics.ProcessStartInfo.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
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

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Diagnostics 
{
	[TypeConverter (typeof (ExpandableObjectConverter))]
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class ProcessStartInfo 
	{
		/* keep these fields in this order and in sync with metadata/process.h */
		private string arguments = "";
		private IntPtr error_dialog_parent_handle = (IntPtr)0;
		private string filename = "";
		private string verb = "";
		private string working_directory = "";
		private ProcessStringDictionary envVars;
		private bool create_no_window = false;
		private bool error_dialog = false;
		private bool redirect_standard_error = false;
		private bool redirect_standard_input = false;
		private bool redirect_standard_output = false;
		private bool use_shell_execute = true;
		private ProcessWindowStyle window_style = ProcessWindowStyle.Normal;

		public ProcessStartInfo() 
		{
		}

		public ProcessStartInfo(string filename) 
		{
			this.filename = filename;
		}

		public ProcessStartInfo(string filename, string arguments) 
		{
			this.filename = filename;
			this.arguments = arguments;
		}

		[RecommendedAsConfigurable (true), DefaultValue ("")]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]

		[MonitoringDescription ("Command line agruments for this process.")]
		public string Arguments {
			get {
				return(arguments);
			}
			set {
				arguments = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Start this process with a new window.")]
		public bool CreateNoWindow {
			get {
				return(create_no_window);
			}
			set {
				create_no_window = value;
			}
		}

		[MonoTODO("Need to read the env block somehow")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content), DefaultValue (null)]
		[Editor ("System.Diagnostics.Design.StringDictionaryEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MonitoringDescription ("Environment variables used for this process.")]
		public StringDictionary EnvironmentVariables {
			get {
				if (envVars == null) {
					envVars = new ProcessStringDictionary ();
					foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables ())
						envVars.Add ((string) entry.Key, (string) entry.Value);
				}

				return envVars;
			}
		}
		
		internal bool HaveEnvVars {
			get { return (envVars != null && envVars.Count > 0); }
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Thread shows dialogboxes for errors.")]
		public bool ErrorDialog {
			get {
				return(error_dialog);
			}
			set {
				error_dialog = value;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		public IntPtr ErrorDialogParentHandle {
			get {
				return(error_dialog_parent_handle);
			}
			set {
				error_dialog_parent_handle = value;
			}
		}
		
		[RecommendedAsConfigurable (true), DefaultValue ("")]
		[Editor ("System.Diagnostics.Design.StartFileNameEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The name of the resource to start this process.")]
		public string FileName {
			get {
				return(filename);
			}
			set {
				filename = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Errors of this process are redirected.")]
		public bool RedirectStandardError {
			get {
				return(redirect_standard_error);
			}
			set {
				redirect_standard_error = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Standard input of this process is redirected.")]
		public bool RedirectStandardInput {
			get {
				return(redirect_standard_input);
			}
			set {
				redirect_standard_input = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Standart output of this process is redirected.")]
		public bool RedirectStandardOutput {
			get {
				return(redirect_standard_output);
			}
			set {
				redirect_standard_output = value;
			}
		}
		
		[DefaultValue (true)]
		[MonitoringDescription ("Use the shell to start this process.")]
		public bool UseShellExecute {
			get {
				return(use_shell_execute);
			}
			set {
				use_shell_execute = value;
			}
		}
		
		[DefaultValue ("")]
		[TypeConverter ("System.Diagnostics.Design.VerbConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The verb to apply to a used document.")]
		public string Verb {
			get {
				return(verb);
			}
			set {
				verb = value;
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		public string[] Verbs {
			get {
				return(null);
			}
		}
		
		[DefaultValue (typeof (ProcessWindowStyle), "Normal")]
		[MonitoringDescription ("The window style used to start this process.")]
		public ProcessWindowStyle WindowStyle {
			get {
				return(window_style);
			}
			set {
				window_style = value;
			}
		}
		
		[RecommendedAsConfigurable (true), DefaultValue ("")]
		[Editor ("System.Diagnostics.Design.WorkingDirectoryEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The initial directory for this process.")]
		public string WorkingDirectory {
			get {
				return(working_directory);
			}
			set {
				working_directory = value == null ? String.Empty : value;
			}
		}
	}
}
