// Mono.Util.CorCompare.MissingField
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Reflection;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a class event that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/24/2002 10:43:57 PM
	/// </remarks>
	class MissingField : MissingMember {
		// e.g. <method name="Equals" status="missing"/>
		public MissingField (MemberInfo info) : base (info) {}

		public override string Type {
			get {
				return "field";
			}
		}
	}
}
