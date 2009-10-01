//
// ServiceBehaviorElement.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MsmqIntegration;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Security;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace System.ServiceModel.Configuration
{
	public class ServiceBehaviorElement
		 : NamedServiceModelExtensionCollectionElement<BehaviorExtensionElement>
	{
		public ServiceBehaviorElement (string name)
		{
			Name = name;
		}

		public ServiceBehaviorElement () {
		}

		protected override void DeserializeElement (XmlReader reader, bool serializeCollectionKey)
		{
			base.DeserializeElement (reader, serializeCollectionKey);
		}

		[MonoTODO ("implement using EvaluationContext")]
		internal override BehaviorExtensionElement DeserializeExtensionElement (string elementName, XmlReader reader)
		{
			//ExtensionElementCollection extensions = ((ExtensionsSection) EvaluationContext.GetSection ("system.serviceModel/extensions")).BehaviorExtensions;
			ExtensionElementCollection extensions = ConfigUtil.ExtensionsSection.BehaviorExtensions;

			ExtensionElement extension = extensions [elementName];
			if (extension == null)
				throw new ConfigurationErrorsException ("Invalid element in configuration. The extension name '" + reader.LocalName + "' is not registered in the collection at system.serviceModel/extensions/behaviorExtensions");

			BehaviorExtensionElement element = (BehaviorExtensionElement) Activator.CreateInstance (Type.GetType (extension.Type));
			element.DeserializeElementInternal (reader, false);
			return element;
		}

		public override void Add (BehaviorExtensionElement element)
		{
			base.Add (element);
		}

		public override bool CanAdd (BehaviorExtensionElement element)
		{
			return base.CanAdd (element);
		}
	}

}
