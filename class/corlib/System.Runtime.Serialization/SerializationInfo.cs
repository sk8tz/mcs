//
// System.Runtime.Serialization.SerializationInfo.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.Collections;

namespace System.Runtime.Serialization
{
	public sealed class SerializationInfo
	{
		
		Hashtable serialized = new Hashtable ();
		string assemblyName; // name of the assembly where the type/class being serialized resides.
		string fullTypeName; // name of the type being serialized.

		[CLSCompliant (false)] IFormatterConverter converter;

		// Constructor
		[CLSCompliant (false)]
		public SerializationInfo (Type type, IFormatterConverter converter)
		{
			if (type == null && converter == null)
				throw new ArgumentNullException ("Null arguments.");
						
			this.converter = converter;
			assemblyName = type.Assembly.FullName;
			fullTypeName = type.FullName;
		}

		// Properties
		public string AssemblyName
		{
			get { return assemblyName; }
			
			set {
				if (value == null)
					throw new ArgumentNullException ("Argument is null.");
				assemblyName = value;
			}
		}
		
		public string FullTypeName
		{
			get { return fullTypeName; }
			
			set {
				if ( value == null)
					throw new ArgumentNullException ("Argument is null.");
				fullTypeName = value;
			}
		}
		
		public int MemberCount
		{
			get { return serialized.Count; }
		}

		// Methods
		public void AddValue (string name, object value, Type type)
		{
			if (serialized.ContainsKey (name))
				throw new SerializationException ("Value has been serialized already.");
			
			SerializationEntry values = new SerializationEntry (name, type, value);
			serialized.Add (name, values);
		}

		public object GetValue (string name, Type type)
		{
			if (name == null)
				throw new ArgumentNullException ("name is null.");
			if (!serialized.ContainsKey (name))
				throw new SerializationException ("No element named " + name + " could be found.");
                        			
			SerializationEntry values = (SerializationEntry) serialized [name];

			if (values.ObjectType != type)
				throw new InvalidCastException ("Invalid Type casting.");
			
			return values.Value;
		}

		public void SetType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type is null.");

			fullTypeName = type.FullName;
			assemblyName = type.Assembly.FullName;
		}

		public SerializationInfoEnumerator GetEnumerator ()
		{
			return null;
		}
		
		[CLSCompliant(false)]
		public void AddValue (string name, short value)
		{
			AddValue (name, value, Type.GetType ("System.Int16"));
		}

		[CLSCompliant(false)]
		public void AddValue (string name, UInt16 value)
		{
			AddValue (name, value, Type.GetType ("System.UInt16"));
		}
		
		public void AddValue (string name, int value)
		{
			AddValue (name, value, Type.GetType ("System.Int32"));
		}
		
		public void AddValue (string name, byte value)
		{
			AddValue (name, value, Type.GetType ("System.Byte"));
		}
		
		public void AddValue (string name, bool value)
		{
			AddValue (name, value, Type.GetType ("System.Boolean"));
		}
	       
	        public void AddValue (string name, char value)
		{
			AddValue (name, value, Type.GetType ("System.Char"));
		}

		[CLSCompliant(false)]
	        public void AddValue (string name, SByte value)
		{
			AddValue (name, value, Type.GetType ("System.SByte"));
		}
		
	        public void AddValue (string name, double value)
		{
			AddValue (name, value, Type.GetType ("System.Double"));
		}
		
	        public void AddValue (string name, Decimal value)
		{
			AddValue (name, value, Type.GetType ("System.Decimal"));
		}
		
	        public void AddValue (string name, DateTime value)
		{
			AddValue (name, value, Type.GetType ("System.DateTime"));
		}
		
	        public void AddValue (string name, float value)
		{
			AddValue (name, value, Type.GetType ("System.Single"));
		}

		[CLSCompliant(false)]
	        public void AddValue (string name, UInt32 value)
		{
			AddValue (name, value, Type.GetType ("System.UInt32"));
		}
	       
	        public void AddValue (string name, long value)
		{
			AddValue (name, value, Type.GetType ("System.Int64"));
		}
		[CLSCompliant(false)]
	        public void AddValue (string name, UInt64 value)
		{
			AddValue (name, value, Type.GetType ("System.UInt64"));
		}
		
	        public void AddValue (string name, object value)
		{
			AddValue (name, value, value.GetType ());
		}		
		
		public bool GetBoolean (string name)
		{
			return (bool) GetValue (name, Type.GetType ("System.Boolean")); 
		}
		
	        public byte GetByte (string name)
		{
			return (byte) GetValue (name, Type.GetType ("System.Byte"));
		}
		
	        public char GetChar (string name)
		{
			return (char) GetValue (name, Type.GetType ("System.Char"));
		}

	        public DateTime GetDateTime (string name)
		{
			return (DateTime) GetValue (name, Type.GetType ("System.DateTime"));
		}
		
		public Decimal GetDecimal (string name)
		{
			return (Decimal) GetValue (name, Type.GetType ("System.Decimal"));
		}
		
		public double GetDouble (string name)
		{
			return (double) GetValue (name, Type.GetType ("System.Double"));
		}
						
		public short GetInt16 (string name)
		{
			return (short) GetValue (name, Type.GetType ("System.Int16"));
		}
		
		public int GetInt32 (string name)
		{
			return (int) GetValue (name, Type.GetType ("System.Int32"));
		}
	       
		public long GetInt64 (string name)
		{
			return (long) GetValue (name, Type.GetType ("System.Int64"));
		}

		[CLSCompliant(false)]
		public SByte GetSByte (string name)
		{
			return (sbyte) GetValue (name, Type.GetType ("System.SByte"));
		}
		
		public float GetSingle (string name)
		{
			return (float) GetValue (name, Type.GetType ("System.SIngle"));
		}
		
		public string GetString (string name)
		{
			return (string) GetValue (name, Type.GetType ("System.String"));
		}

		[CLSCompliant(false)]
		public UInt16 GetUInt16 (string name)
		{
			return (UInt16) GetValue (name, Type.GetType ("System.UInt16"));
		}
		
		[CLSCompliant(false)]
		public UInt32 GetUInt32 (string name)
		{
			return (UInt32) GetValue (name, Type.GetType ("System.UInt32"));
		}
		[CLSCompliant(false)]
		public UInt64 GetUInt64 (string name)
		{
			return (UInt64) GetValue (name, Type.GetType ("System.UInt64"));
		}
	}
}
