// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 1.1.
//
// The APL v2.0:
//
//---------------------------------------------------------------------------
//   Copyright (C) 2007, 2008 LShift Ltd., Cohesive Financial
//   Technologies LLC., and Rabbit Technologies Ltd.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//---------------------------------------------------------------------------
//
// The MPL v1.1:
//
//---------------------------------------------------------------------------
//   The contents of this file are subject to the Mozilla Public License
//   Version 1.1 (the "License"); you may not use this file except in
//   compliance with the License. You may obtain a copy of the License at
//   http://www.rabbitmq.com/mpl.html
//
//   Software distributed under the License is distributed on an "AS IS"
//   basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//   License for the specific language governing rights and limitations
//   under the License.
//
//   The Original Code is The RabbitMQ .NET Client.
//
//   The Initial Developers of the Original Code are LShift Ltd.,
//   Cohesive Financial Technologies LLC., and Rabbit Technologies Ltd.
//
//   Portions created by LShift Ltd., Cohesive Financial Technologies
//   LLC., and Rabbit Technologies Ltd. are Copyright (C) 2007, 2008
//   LShift Ltd., Cohesive Financial Technologies LLC., and Rabbit
//   Technologies Ltd.;
//
//   All Rights Reserved.
//
//   Contributor(s): ______________________________________.
//
//---------------------------------------------------------------------------
using System;
using System.Collections;

namespace RabbitMQ.Client.Events
{
    ///<summary>Describes an exception that was thrown during the
    ///library's invocation of an application-supplied callback
    ///handler.</summary>
    ///<remarks>
    ///<para>
    /// When an exception is thrown from a callback registered with
    /// part of the RabbitMQ .NET client library, it is caught,
    /// packaged into a CallbackExceptionEventArgs, and passed through
    /// the appropriate IModel's or IConnection's CallbackException
    /// event handlers. If an exception is thrown in a
    /// CallbackException handler, it is silently swallowed, as
    /// CallbackException is the last chance to handle these kinds of
    /// exception.
    ///</para>
    ///<para>
    /// Code constructing CallbackExceptionEventArgs instances will
    /// usually place helpful information about the context of the
    /// call in the IDictionary available through the Detail property.
    ///</para>
    ///</remarks>
    public class CallbackExceptionEventArgs: EventArgs
    {
        private IDictionary m_detail;
        private Exception m_exception;

        ///<summary>Wrap an exception thrown by a callback.</summary>
        public CallbackExceptionEventArgs(Exception exception)
        {
            m_detail = new Hashtable();
            m_exception = exception;
        }

        ///<summary>Access helpful information about the context in
        ///which the wrapped exception was thrown.</summary>
        public IDictionary Detail { get { return m_detail; } }

        ///<summary>Access the wrapped exception.</summary>
        public Exception Exception { get { return m_exception; } }
    }
}
