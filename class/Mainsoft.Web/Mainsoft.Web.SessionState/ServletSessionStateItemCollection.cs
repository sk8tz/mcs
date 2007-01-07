//
// (C) 2006 Mainsoft Corporation (http://www.mainsoft.com)
// Author: Konstantin Triger <kostat@mainsoft.com>
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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.SessionState;
using System.Web;
using System.Threading;

using javax.servlet;
using javax.servlet.http;

namespace Mainsoft.Web.SessionState
{
	public sealed partial class ServletSessionStateStoreProvider
	{
		sealed class ServletSessionStateItemCollection : ISessionStateItemCollection, java.io.Externalizable
		{
			SessionStateItemCollection _items;
			HttpStaticObjectsCollection _staticObjects;
			bool _needSessionPersistence;

			public ServletSessionStateItemCollection () { } //for deserialization

			public ServletSessionStateItemCollection (HttpContext context)
				: this () {

				_items = new SessionStateItemCollection ();
				_staticObjects = new HttpStaticObjectsCollection ();

				if (context != null) {
					ServletConfig config = ServletSessionStateStoreProvider.GetWorkerRequest (context).Servlet.getServletConfig ();
					string sessionPersistance = config.getInitParameter (J2EEConsts.Enable_Session_Persistency);
					if (sessionPersistance == null)
						sessionPersistance = config.getServletContext().getInitParameter (J2EEConsts.Enable_Session_Persistency);
					if (sessionPersistance != null) {
						try {
							_needSessionPersistence = Boolean.Parse (sessionPersistance);
						}
						catch (Exception) {
							_needSessionPersistence = false;
#if DEBUG
							Console.WriteLine ("EnableSessionPersistency init param's value is invalid. the value is " + sessionPersistance);
#endif
						}
					}
				}
			}

			public HttpStaticObjectsCollection StaticObjects {
				get { return _staticObjects; }
			}
			#region ISessionStateItemCollection Members

			public void Clear () {
				_items.Clear ();
			}

			public bool Dirty {
				get {
					return _items.Dirty;
				}
				set {
					_items.Dirty = value;
				}
			}

			public System.Collections.Specialized.NameObjectCollectionBase.KeysCollection Keys {
				get { return _items.Keys; }
			}

			public void Remove (string name) {
				_items.Remove (name);
			}

			public void RemoveAt (int index) {
				_items.RemoveAt (index);
			}

			public object this [int index] {
				get {
					return _items [index];
				}
				set {
					_items [index] = value;
				}
			}

			public object this [string name] {
				get {
					return _items [name];
				}
				set {
					_items [name] = value;
				}
			}

			#endregion

			#region ICollection Members

			public void CopyTo (Array array, int index) {
				((ICollection) _items).CopyTo (array, index);
			}

			public int Count {
				get { return ((ICollection) _items).Count; }
			}

			public bool IsSynchronized {
				get { return ((ICollection) _items).IsSynchronized; }
			}

			public object SyncRoot {
				get { return ((ICollection) _items).SyncRoot; }
			}

			#endregion

			#region IEnumerable Members

			public System.Collections.IEnumerator GetEnumerator () {
				return ((IEnumerable) _items).GetEnumerator ();
			}

			#endregion

			#region Externalizable Members

			public void readExternal (java.io.ObjectInput input) {
				lock (this) {
					_needSessionPersistence = input.readBoolean ();
					if (!_needSessionPersistence) //noting has been written 
						return;

					ObjectInputStream ms = new ObjectInputStream (input);
					System.IO.BinaryReader br = new System.IO.BinaryReader (ms);
					_items = SessionStateItemCollection.Deserialize (br);
					_staticObjects = HttpStaticObjectsCollection.Deserialize (br);
				}
			}

			public void writeExternal (java.io.ObjectOutput output) {
				lock (this) {
					output.writeBoolean (_needSessionPersistence);
					if (!_needSessionPersistence)
						//indicates that there is nothing to serialize for this object
						return;

					ObjectOutputStream ms = new ObjectOutputStream (output);
					System.IO.BinaryWriter bw = new System.IO.BinaryWriter (ms);
					_items.Serialize (bw);
					_staticObjects.Serialize (bw);
				}
			}

			#endregion
		}
	}
}
