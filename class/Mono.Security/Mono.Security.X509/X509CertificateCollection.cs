//
// Based on System.Security.Cryptography.X509Certificates.X509CertificateCollection
//	in System assembly
//
// Authors:
//	Lawrence Pit (loz@cable.a2000.nl)
//	Sebastien Pouliot <sebastien@ximian.com>
//

using System;
using System.Collections;

namespace Mono.Security.X509 {

	[Serializable]
#if INSIDE_CORLIB
	internal
#else
	public 
#endif
	class X509CertificateCollection : CollectionBase, IEnumerable {
		
		public X509CertificateCollection () 
		{
		}
		
		public X509CertificateCollection (X509Certificate [] value) 
		{
			AddRange (value);
		}
		
		public X509CertificateCollection (X509CertificateCollection value)
		{
			AddRange (value);
		}
		
		// Properties
		
		public X509Certificate this [int index] {
			get { return (X509Certificate) InnerList [index]; }
			set { InnerList [index] = value; }
		}
		
		// Methods

		public int Add (X509Certificate value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			return InnerList.Add (value);
		}
		
		public void AddRange (X509Certificate [] value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			for (int i = 0; i < value.Length; i++) 
				InnerList.Add (value [i]);
		}
		
		public void AddRange (X509CertificateCollection value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			for (int i = 0; i < value.InnerList.Count; i++) 
				InnerList.Add (value [i]);
		}
		
		public bool Contains (X509Certificate value) 
		{
			return (IndexOf (value) != -1);
		}

		public void CopyTo (X509Certificate[] array, int index)
		{
			InnerList.CopyTo (array, index);
		}
		
		public new X509CertificateEnumerator GetEnumerator ()
		{
			return new X509CertificateEnumerator (this);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return InnerList.GetEnumerator ();
		}
		
		public override int GetHashCode () 
		{
			return InnerList.GetHashCode ();
		}
		
		public int IndexOf (X509Certificate value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			byte[] hash = value.Hash;
			for (int i=0; i < InnerList.Count; i++) {
				X509Certificate x509 = (X509Certificate) InnerList [i];
				if (Compare (x509.Hash, hash))
					return i;
			}
			return -1;
		}
		
		public void Insert (int index, X509Certificate value)
		{
			InnerList.Insert (index, value);
		}
		
		public void Remove (X509Certificate value)
		{
			InnerList.Remove (value);
		}

		// private stuff

		private bool Compare (byte[] array1, byte[] array2) 
		{
			if ((array1 == null) && (array2 == null))
				return true;
			if ((array1 == null) || (array2 == null))
				return false;
			if (array1.Length != array2.Length)
				return false;
			for (int i=0; i < array1.Length; i++) {
				if (array1 [i] != array2 [i])
					return false;
			}
			return true;
		}

		// Inner Class
		
		public class X509CertificateEnumerator : IEnumerator {

			private IEnumerator enumerator;

			// Constructors
			
			public X509CertificateEnumerator (X509CertificateCollection mappings)
			{
				enumerator = ((IEnumerable) mappings).GetEnumerator ();
			}

			// Properties
			
			public X509Certificate Current {
				get { return (X509Certificate) enumerator.Current; }
			}
			
			object IEnumerator.Current {
				get { return enumerator.Current; }
			}

			// Methods
			
			bool IEnumerator.MoveNext ()
			{
				return enumerator.MoveNext ();
			}
			
			void IEnumerator.Reset () 
			{
				enumerator.Reset ();
			}
			
			public bool MoveNext () 
			{
				return enumerator.MoveNext ();
			}
			
			public void Reset ()
			{
				enumerator.Reset ();
			}
		}		
	}
}
