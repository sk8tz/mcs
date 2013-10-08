// BuildManager.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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

using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;

namespace Microsoft.Build.Execution
{
	public class BuildManager
	{
		public BuildManager ()
		{
			throw new NotImplementedException ();
		}

		public BuildManager (string hostName)
		{
			throw new NotImplementedException ();
		}

		~BuildManager ()
		{

		}

		BuildParameters parameters;
		BuildRequestData request_data;
		List<BuildSubmission> submissions = new List<BuildSubmission> ();

		public void BeginBuild (BuildParameters parameters)
		{
			throw new NotImplementedException ();
		}

		public BuildResult Build (BuildParameters parameters, BuildRequestData requestData)
		{
			throw new NotImplementedException ();
		}

		public BuildResult BuildRequest (BuildRequestData requestData)
		{
			throw new NotImplementedException ();
		}

		public void CancelAllSubmissions ()
		{
			foreach (var sub in submissions)
				sub.Cancel ();
			submissions.Clear ();
		}

		public void EndBuild ()
		{
			throw new NotImplementedException ();
		}

		public ProjectInstance GetProjectInstanceForBuild (Project project)
		{
			throw new NotImplementedException ();
		}

		public BuildSubmission PendBuildRequest (BuildRequestData requestData)
		{
			var sub = new BuildSubmission (this, requestData);
			submissions.Add (sub);
			return sub;
		}

		public void ResetCaches ()
		{
			throw new NotImplementedException ();
		}

		static BuildManager default_manager = new BuildManager ();

		public static BuildManager DefaultBuildManager {
			get { return default_manager; }
		}
	}
}

