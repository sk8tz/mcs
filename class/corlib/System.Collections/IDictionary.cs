// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.IDictionary
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;

namespace System.Collections {

	public interface IDictionary : ICollection {
		// properties

		bool IsFixedSize { get; }

		bool IsReadOnly { get; }

		object this[object key] { get; set; }

		ICollection Keys { get; }

		ICollection Values { get; }

		// methods

		void Add (object key, object value);

		void Clear ();

		bool Contains (object key);

		new IDictionaryEnumerator GetEnumerator ();

		void Remove (object key);
	}
}
