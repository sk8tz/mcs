﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	internal class InputOrReplyRequestProcessor : BaseRequestProcessor
	{
		DispatchRuntime dispatch_runtime;
		IChannel reply_or_input;

		public InputOrReplyRequestProcessor (DispatchRuntime runtime, IChannel replyOrInput)
		{
			Init (runtime, reply_or_input);

			//initialization
			InitializeChain.AddHandler (new InitializingHandler ());

			//processing
			ProcessingChain.AddHandler (new PostReceiveRequestHandler ()).
							AddHandler(new SecurityHandler ()).
							AddHandler(new OperationInvokerHandler ()).
							AddHandler(new ReplyHandler (replyOrInput));

			//errors
			ErrorChain.AddHandler (new ErrorProcessingHandler (replyOrInput));

			//finalize
			FinalizationChain.AddHandler (new FinalizeProcessingHandler ());
		}

		void Init (DispatchRuntime runtime, IChannel replyOrInput)
		{
			dispatch_runtime = runtime;
			reply_or_input = replyOrInput;
		}

		public void ProcessInput (Message message)
		{
			OperationContext opcx = CreateOperationContext (message);
			ProcessRequest (new MessageProcessingContext (opcx));
		}

		public void ProcessReply (RequestContext rc)
		{
			OperationContext opcx = CreateOperationContext (rc.RequestMessage);
			opcx.RequestContext = rc;
			ProcessRequest (new MessageProcessingContext (opcx));
		}

		OperationContext CreateOperationContext (Message incoming)
		{
			ServiceRuntimeChannel contextChannel = new ServiceRuntimeChannel (reply_or_input,
													dispatch_runtime.ChannelDispatcher.DefaultOpenTimeout,
													dispatch_runtime.ChannelDispatcher.DefaultCloseTimeout);
			OperationContext opCtx = new OperationContext (contextChannel);
			opCtx.IncomingMessage = incoming;
			opCtx.EndpointDispatcher = dispatch_runtime.EndpointDispatcher;
			return opCtx;
		}
	}
}
