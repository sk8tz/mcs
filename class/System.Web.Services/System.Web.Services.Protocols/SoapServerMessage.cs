// 
// System.Web.Services.Protocols.SoapServerMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Web.Services;
using System.IO;

namespace System.Web.Services.Protocols {
	public sealed class SoapServerMessage : SoapMessage {

		#region Fields

		string action;
		SoapMethodStubInfo stubInfo;
		object server;
		string url;
		object[] parameters;

		#endregion

		#region Constructors

		internal SoapServerMessage (HttpRequest request, object server, Stream stream)
			: base (stream, null)
		{
			this.action = request.Headers ["SOAPAction"];
			this.server = server;
			this.url = request.Url.ToString();
		}

		internal SoapServerMessage (HttpRequest request, SoapException exception, SoapMethodStubInfo stubInfo, object server, Stream stream)
			: base (stream, exception)
		{
			this.action = request.Headers ["SOAPAction"];
			this.stubInfo = stubInfo;
			this.server = server;
			this.url = request.Url.ToString();
		}

		#endregion

		#region Properties

		public override LogicalMethodInfo MethodInfo {
			get { return stubInfo.MethodInfo; }
		}

		public override string Action {
			get { return action; }
		}

		internal SoapMethodStubInfo MethodStubInfo {
			get { return stubInfo; }
			set { stubInfo = value; }
		}

		public override bool OneWay {
			get { return stubInfo.OneWay; }
		}

		public object Server {
			get { return server; }
		}	

		public override string Url {
			get { return url; }
		}

#if NET_2_0
		[MonoTODO]
		public override SoapProtocolVersion SoapVersion {
			get { throw new NotImplementedException (); }
		}
#endif

		#endregion // Properties

		#region Methods

		protected override void EnsureInStage ()
		{
			EnsureStage (SoapMessageStage.AfterDeserialize);
		}

		protected override void EnsureOutStage ()
		{
			EnsureStage (SoapMessageStage.BeforeSerialize);
		}

		#endregion // Methods
	}
}
