using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing.Configuration
{
	[ConfigurationCollection (typeof (BackupEndpointCollection), AddItemName = "backupList")]
	public class BackupListCollection : ConfigurationElementCollection
	{
		public void Add (BackupEndpointCollection element)
		{
			BaseAdd (element);
		}

		protected override ConfigurationElement CreateNewElement ()
		{
			return new BackupEndpointElement ();
		}

		protected override Object GetElementKey (ConfigurationElement element)
		{
			return ((BackupEndpointElement) element).EndpointName;
		}

		public void Remove (BackupEndpointCollection element)
		{
			BaseRemove (element);
		}
	}
}
