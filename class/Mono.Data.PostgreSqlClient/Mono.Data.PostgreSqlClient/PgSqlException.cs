//
// Mono.Data.PostgreSqlClient.PgSqlException.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc
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
using System.Runtime.Serialization;

namespace Mono.Data.PostgreSqlClient
{
	/// <summary>
	/// Exceptions, as returned by SQL databases.
	/// </summary>
	public sealed class PgSqlException : SystemException
	{
		private PgSqlErrorCollection errors; 

		internal PgSqlException() 
			: base("a SQL Exception has occurred") {
			errors = new PgSqlErrorCollection();
		}

		internal PgSqlException(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) 
				: base(message) {	
			
			errors = new PgSqlErrorCollection (theClass, 
				lineNumber, message,
				number, procedure,
				server, source, state);
		}

		#region Properties

		[MonoTODO]
		public byte Class {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception here?
				else
					return errors[0].Class;
			}

			set { 
				errors[0].SetClass(value);
			}
		}

		[MonoTODO]
		public PgSqlErrorCollection Errors {
			get { 
				return errors;
			}

			set { 
				errors = value;
			}
		}

		[MonoTODO]
		public int LineNumber {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception here?
				return errors[0].LineNumber;
			}

			set { 
				errors[0].SetLineNumber(value);
			}
		}
		
		[MonoTODO]
		public override string Message 	{
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else {
					String msg = "";
					int i = 0;
					
					for(i = 0; i < errors.Count - 1; i++) {
						msg = msg + errors[i].Message + "\n";
                                        }
					msg = msg + errors[i].Message;

					return msg;
				}
			}
		}
		
		[MonoTODO]
		public int Number {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception?
				else
					return errors[0].Number;
			}

			set { 
				errors[0].SetNumber(value);
			}
		}
		
		[MonoTODO]
		public string Procedure {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Procedure;
			}

			set { 
				errors[0].SetProcedure(value);
			}
		}

		[MonoTODO]
		public string Server {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Server;
			}

			set { 
				errors[0].SetServer(value);
			}
		}
		
		[MonoTODO]
		public override string Source {
			get { 
				if(errors.Count == 0)
					return ""; // FIXME: throw exception?
				else
					return errors[0].Source;
			}

			set { 
				errors[0].SetSource(value);
			}
		}

		[MonoTODO]
		public byte State {
			get { 
				if(errors.Count == 0)
					return 0; // FIXME: throw exception?
				else
					return errors[0].State;
			}

			set { 
				errors[0].SetState(value);
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void GetObjectData(SerializationInfo si,
			StreamingContext context) {
			// FIXME: to do
		}

		// [Serializable]
		// [ClassInterface(ClassInterfaceType.AutoDual)]
		public override string ToString() {
			String toStr = "";
			for (int i = 0; i < errors.Count; i++) {
				toStr = toStr + errors[i].ToString() + "\n";
			}
			return toStr;
		}

		internal void Add(byte theClass, int lineNumber,
			string message,	int number, string procedure,
			string server, string source, byte state) {
			
			errors.Add (theClass, lineNumber, message,
				number, procedure,
				server, source, state);
		}

		[MonoTODO]
		~PgSqlException() {
			// FIXME: destructor to release resources
		}

		#endregion // Methods
	}
}
