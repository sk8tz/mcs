// System.ComponentModel.Design.IResourceService.cs
//
// Author:
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
// 

using System.Globalization;
using System.Resources;

namespace System.ComponentModel.Design
{
	public interface IResourceService
	{
		IResourceReader GetResourceReader (CultureInfo info);

		IResourceWriter GetResourceWriter (CultureInfo info);
	}
}
