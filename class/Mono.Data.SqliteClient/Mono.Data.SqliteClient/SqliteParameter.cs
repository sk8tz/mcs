// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteParameter.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;

namespace Mono.Data.SqliteClient
{
        public class SqliteParameter : IDbDataParameter
        {
                string name;
                DbType type;
                string source_column;
                ParameterDirection direction;
                DataRowVersion row_version;
                object param_value;
                byte precision;
                byte scale;
                int size;

                public SqliteParameter ()
                {
                        type = DbType.String;
                        direction = ParameterDirection.Input;
                }

                public SqliteParameter (string name_in, DbType type_in)
                {
                        name = name_in;
                        type = type_in;
                }

                public SqliteParameter (string name_in, object param_value_in)
                {
                        name = name_in;
                        type = DbType.String;
                        param_value = param_value_in;
                        direction = ParameterDirection.Input;
                }

                public SqliteParameter (string name_in, DbType type_in, int size_in)
                        : this (name_in, type_in)
                {
                        size = size_in;
                }

                public SqliteParameter (string name_in, DbType type_in, int size, string src_column)
                        : this (name_in ,type_in)
                {
                        source_column = src_column;
                }

                public DbType DbType {
                        get {
                                return type;
                        }
                        set {
                                type = value;
                        }
                }

                public ParameterDirection Direction {
                        get {
                                return direction;
                        }
                        set {
                                direction = value;
                        }
                }

                public bool IsNullable {
                        get {
                                // uhh..
                                return true;
                        }
                }

                public string ParameterName {
                        get {
                                return name;
                        }
                        set {
                                name = value;
                        }
                }

                public string SourceColumn {
                        get {
                                return source_column;
                        }
                        set {
                                source_column = value;
                        }
                }

                public DataRowVersion SourceVersion {
                        get {
                                return row_version;
                        }
                        set {
                                row_version = value;
                        }
                }

                public object Value {
                        get {
                                return param_value;
                        }
                        set {
                                param_value = value;
                        }
                }

                public byte Precision {
                        get {
                                return precision;
                        }
                        set {
                                precision = value;
                        }
                }

                public byte Scale {
                        get {
                                return scale;
                        }
                        set {
                                scale = value;
                        }
                }

                public int Size {
                        get {
                                return size;
                        }
                        set {
                                size = value;
                        }
                }
        }
}
