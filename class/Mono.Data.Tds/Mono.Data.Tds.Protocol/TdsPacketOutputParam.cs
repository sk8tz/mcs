//
// Mono.Data.Tds.Protocol.TdsPacketOutputParam.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace Mono.Data.Tds.Protocol {
	public class TdsPacketOutputParam : TdsPacketResult
	{
		#region Fields
		
		object value;

		#endregion // Fields

		#region Constructors

		public TdsPacketOutputParam (object value)
			: base (TdsPacketSubType.Param)
		{
			this.value = value;
		}

		#endregion // Constructors

		#region Properties

		public object Value {
			get { return value; }
		}

		#endregion // Properties
	}
}
