/**

 * Namespace: System.Web.Utils
 * Class:     WebHashCodeProvider
 *

 * Author:  Gaurav Vaish

 * Maintainer: gvaish@iitk.ac.in

 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>

 * Implementation: yes

 * Status:  ??%

 *

 * (C) Gaurav Vaish (2001)

 */


using System.Collections;

namespace System.Web.Utils
{
	public class WebHashCodeProvider : IHashCodeProvider
	{
		private static readonly IHashCodeProvider defHcp;

		public WebHashCodeProvider()
		{
		}
		
		int IHashCodeProvider.GetHashCode(object key)
		{
			return Default.GetHashCode(key);
		}

		public static IHashCodeProvider Default
		{
			get
			{
				if(defHcp==null)
				{
					 defHcp = = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture);
				}
				return defHcp;
			}
		}
	}
}
