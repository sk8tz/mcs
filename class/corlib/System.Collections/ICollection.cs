// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.ICollection
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;

namespace System.Collections {

	public interface ICollection : IEnumerable {
		int Count { get; }

		bool IsSynchronized { get; }

		object SyncRoot { get; }

		void CopyTo (Array array, int index);
	}
}
