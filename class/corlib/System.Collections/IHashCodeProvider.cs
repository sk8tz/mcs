// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Collections.IHashCodeProvider
//
// Author:
//    Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2001 Vladimir Vukicevic
//

using System;

namespace System.Collections {

	public interface IHashCodeProvider {
		int GetHashCode (object obj);
	}
}
