//
// System.Data.ObjectSpaces.Schema.SchemaClass.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

using System.Data.Mapping;
using System.Xml;

namespace System.Data.ObjectSpaces.Schema {
	public sealed class SchemaClass : IDomainStructure
	{
		#region Fields

		bool canInherit;
		Type classType;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SchemaClass ()
		{
		}

		[MonoTODO]
		public SchemaClass (Type classType)
		{
			ClassType = classType;
		}

		#endregion // Constructors

		#region Properties

		public bool CanInherit {
			get { return canInherit; }
			set { canInherit = value; }
		}

		public Type ClassType {
			get { return classType; }
			set { classType = value; }
		}

		[MonoTODO]
		public ObjectSchema DeclaringObjectSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ExtendedPropertyCollection ExtendedProperties {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		IDomainSchema IDomainStructure.DomainSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		string IDomainStructure.Select {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public SchemaMemberCollection SchemaMembers {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		IDomainField IDomainStructure.GetDomainField (string select, IXmlNamespaceResolver namespaces)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
