//
// System.EnterpriseServices.CompensatingResourceManager.ApplicationCrmEnabledAttribute.cs
//
// Author:
//   Alejandro S�nchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro S�nchez Acosta
//

using System.Runtime.InteropServices;

namespace System.EnterpriseServices.CompensatingResourceManager {

	/// <summary>
	///   ApplicationCrmEnable Attribute for classes. 
	/// </summary>
	
	[AttributeUsage(AttributeTargets.Assembly)]
	[ComVisible(false)]
	[ProgId("System.EnterpriseServices.Crm.ApplicationCrmEnabledAttribute")]
	public sealed class ApplicationCrmEnabledAttribute : Attribute
	{
		bool val;

		public ApplicationCrmEnabledAttribute()
		{
			val = true;
		}

		public ApplicationCrmEnabledAttribute (bool val)
		{
			this.val = val;
		}

		public bool Value 
		{
			get
			{
				return val;
			}
		}
	}
}
