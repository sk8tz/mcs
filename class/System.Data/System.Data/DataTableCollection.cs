//
// System.Data.DataTableCollection.cs
//
// Authors:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Chris Podurgiel
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Represents the collection of tables for the DataSet.
	/// </summary>
	public class DataTableCollection : InternalDataCollectionBase
	{
		DataSet dataSet;
		const string defaultTableName = "Table1";
		Hashtable tables;

		#region Constructors 

		// LAMESPEC: This constructor is undocumented
		protected internal DataTableCollection (DataSet dataSet)
			: base ()
		{
			this.dataSet = dataSet;
			this.tables = new Hashtable ();
		}
		
		#endregion
		
		#region Properties

		public override int Count {
			get { return list.Count; }
		}

		public DataTable this[int index] {
			get { return (DataTable)(list[index]); }
		}

		public DataTable this[string name] {
			get { return (DataTable)(tables[name]); }
		}

		protected override ArrayList List {
			get { return list; }
		}

		#endregion
	
		#region Methods	

		public virtual DataTable Add () 
		{
			return this.Add (defaultTableName);
		}

		public virtual void Add (DataTable table) 
		{
			list.Add (table);
			table.dataSet = dataSet;
			tables[table.TableName] = table;
		}

		public virtual DataTable Add (string name) 
		{
			DataTable table = new DataTable (name);
			this.Add (table);
			return table;
		}

		public void AddRange (DataTable[] tables) 
		{
			foreach (DataTable table in tables)
				this.Add (table);
		}

		[MonoTODO]
		public bool CanRemove (DataTable table) 
		{
			throw new NotImplementedException ();
		}

		public void Clear () 
		{
			list.Clear ();
			tables.Clear ();
		}

		public bool Contains (string name) 
		{
			return tables.Contains (name);
		}

		public virtual int IndexOf (DataTable table) 
		{
			return list.IndexOf (table);
		}

		public virtual int IndexOf (string name) 
		{
			return list.IndexOf (tables[name]);
		}

		public void Remove (DataTable table) 
		{
			this.Remove (table.TableName);
		}

		public void Remove (string name) 
		{
			list.Remove (tables[name]);
			tables.Remove (name);
		}

		public void RemoveAt (int index) 
		{
			tables.Remove (((DataTable)(list[index])).TableName);
			list.RemoveAt (index);
		}

		#endregion

		#region Events
		
		public event CollectionChangeEventHandler CollectionChanged;
		public event CollectionChangeEventHandler CollectionChanging;

		#endregion
	}
}
