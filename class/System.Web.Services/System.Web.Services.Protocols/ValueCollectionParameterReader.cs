// 
// System.Web.Services.Protocols.ValueCollectionParameterReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections.Specialized;
using System.Reflection;
using System.Web;

namespace System.Web.Services.Protocols {
	public abstract class ValueCollectionParameterReader : MimeParameterReader {

		ParameterInfo[] parameters;

		#region Constructors

		protected ValueCollectionParameterReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			if (IsSupported (methodInfo)) return methodInfo.Parameters;
			else return null;
		}

		public override void Initialize (object o)
		{
			parameters = (ParameterInfo[]) o;
		}

		public static bool IsSupported (LogicalMethodInfo methodInfo)
		{
			foreach (ParameterInfo param in methodInfo.Parameters)
				if (!IsSupported (param)) return false;
			return true;
		}

		public static bool IsSupported (ParameterInfo paramInfo)
		{
			return !paramInfo.ParameterType.IsByRef && !paramInfo.IsOut;
		}

		protected object[] Read (NameValueCollection collection)
		{
			object[] res = new object [parameters.Length];
			for (int n=0; n<res.Length; n++)
			{
				string val = collection [parameters[n].Name];
				if (val == null) throw new InvalidOperationException ("Missing parameter: " + parameters[n].Name);
				try
				{
					res [n] = Convert.ChangeType (val, parameters[n].ParameterType);
				}
				catch (Exception ex)
				{
					string error = "Cannot convert " + val + " to " + parameters[n].ParameterType.FullName + "\n";
					error += "Parameter name: " + parameters[n].Name + " --> " + ex.Message;
					throw new InvalidOperationException (error);
				}
			}
			return res;
		}

		#endregion // Methods
	}
}
