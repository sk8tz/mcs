// 
// System.Web.HttpCachePolicy
//
// Authors:
//	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//	Tim Coleman (tim@timcoleman.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
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
using System.Text;
using System.Web.UI;
using System.Web.Util;

namespace System.Web {

	class CacheabilityUpdatedEventArgs : EventArgs {

		public readonly HttpCacheability Cacheability;

		public CacheabilityUpdatedEventArgs (HttpCacheability cacheability)
		{
			Cacheability = cacheability;
		}
	}
	
	internal delegate void CacheabilityUpdatedCallback (object sender, CacheabilityUpdatedEventArgs args);
	
	public sealed class HttpCachePolicy {

		internal HttpCachePolicy ()
		{
		}

		#region Fields

		HttpCacheVaryByHeaders varyByHeaders = new HttpCacheVaryByHeaders ();
		HttpCacheVaryByParams varyByParams = new HttpCacheVaryByParams ();
		ArrayList validationCallbacks;
		StringBuilder cacheExtension;
		HttpCacheability cacheability;
		string etag;
		bool etagFromFileDependencies;
		bool haveExpireDate;
		DateTime expireDate;
		bool haveLastModified;
		DateTime lastModified;
		//bool lastModifiedFromFileDependencies;
		//bool noServerCaching;
		//bool noStore;
		//bool noTransforms;
		HttpCacheRevalidation revalidation;
		//bool validUntilExpires;
		string varyByCustom;
		bool haveMaxAge;
		TimeSpan maxAge;
		bool haveProxyMaxAge;
		TimeSpan proxyMaxAge;
		ArrayList fields;
		bool slidingExpiration;
		int duration;
#if NET_1_1
		bool allowResponseInBrowserHistory;
#endif
                
		#endregion

                internal event CacheabilityUpdatedCallback CacheabilityUpdated;
                
		#region Properties
                
		public HttpCacheVaryByHeaders VaryByHeaders {
			get { return varyByHeaders; }
		}

		public HttpCacheVaryByParams VaryByParams {
			get { return varyByParams; }
		}

                internal DateTime Expires {
                        get { return expireDate; }
                }

		internal int Duration {
			get { return duration; }
			set { duration = value; }
		}

		internal bool Sliding {
			get { return slidingExpiration; }
		}

		#endregion // Properties

		#region Methods

		public void AddValidationCallback (HttpCacheValidateHandler handler, object data)
		{
			if (handler == null)
				throw new ArgumentNullException ("handler");

			if (validationCallbacks == null)
				validationCallbacks = new ArrayList ();

			validationCallbacks.Add (new Pair (handler, data));
		}

		public void AppendCacheExtension (string extension)
		{
			if (extension == null)
				throw new ArgumentNullException ("extension");

			if (cacheExtension == null)
				cacheExtension = new StringBuilder (extension);
			else
				cacheExtension.Append (", " + extension);
		}

		public void SetCacheability (HttpCacheability cacheability)
		{
			if (cacheability < HttpCacheability.NoCache || cacheability > HttpCacheability.Public)
				throw new ArgumentOutOfRangeException ("cacheability");

			if (this.cacheability > 0 && cacheability > this.cacheability)
				return;
			
			this.cacheability = cacheability;

			if (CacheabilityUpdated != null)
				CacheabilityUpdated (this, new CacheabilityUpdatedEventArgs (cacheability));
		}

		public void SetCacheability (HttpCacheability cacheability, string field)
		{
			if (field == null)
				throw new ArgumentNullException ("field");

			if (cacheability != HttpCacheability.Private &&
			    cacheability != HttpCacheability.NoCache)
				throw new ArgumentException ("Must be NoCache or Private", "cacheability");

			if (fields == null)
				fields = new ArrayList ();

			fields.Add (new Pair (cacheability, field));
		}

		public void SetETag (string etag)
		{
			if (etag == null)
				throw new ArgumentNullException ("etag");

			if (this.etag != null)
				throw new InvalidOperationException ("The ETag header has already been set");

			if (etagFromFileDependencies)
				throw new InvalidOperationException ("SetEtagFromFileDependencies has already been called");

			this.etag = etag;
		}

		public void SetETagFromFileDependencies ()
		{
			if (this.etag != null)
				throw new InvalidOperationException ("The ETag header has already been set");

			etagFromFileDependencies = true;
		}

		public void SetExpires (DateTime date)
		{
			if (haveExpireDate && date > expireDate)
				return;

			haveExpireDate = true;
			expireDate = date;
		}

		public void SetLastModified (DateTime date)
		{
			if (date > DateTime.Now)
				throw new ArgumentOutOfRangeException ("date");

			if (haveLastModified && date < lastModified)
				return;

			haveLastModified = true;
			lastModified = date;
		}

		[MonoTODO]
		public void SetLastModifiedFromFileDependencies ()
		{
			//lastModifiedFromFileDependencies = true;
		}

		public void SetMaxAge (TimeSpan date)
		{
			if (date < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException ("date");
			
			if (haveMaxAge && maxAge < date)
				return;

			maxAge = date;
			haveMaxAge = true;
		}

		[MonoTODO]
		public void SetNoServerCaching ()
		{
			//noServerCaching = true;
		}

		[MonoTODO]
		public void SetNoStore ()
		{
			//noStore = true;
		}

		[MonoTODO]
		public void SetNoTransforms ()
		{
			//noTransforms = true;
		}

		public void SetProxyMaxAge (TimeSpan delta)
		{
			if (delta < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException ("delta");

			if (haveProxyMaxAge && proxyMaxAge < delta)
				return;

			proxyMaxAge = delta;
		}

		public void SetRevalidation (HttpCacheRevalidation revalidation)
		{
			if (revalidation < HttpCacheRevalidation.AllCaches ||
			    revalidation > HttpCacheRevalidation.None)
				throw new ArgumentOutOfRangeException ("revalidation");

			if (this.revalidation > revalidation)
				this.revalidation = revalidation;
		}

		public void SetSlidingExpiration (bool slide)
		{
			slidingExpiration = slide;
		}

		[MonoTODO]
		public void SetValidUntilExpires (bool validUntilExpires)
		{
			//this.validUntilExpires = validUntilExpires;
		}

		public void SetVaryByCustom (string custom)
		{
			if (custom == null)
				throw new ArgumentNullException ("custom");

			if (varyByCustom != null)
				throw new InvalidOperationException ("VaryByCustom has already been set.");

			varyByCustom = custom;
		}

		internal string GetVaryByCustom ()
		{
			return varyByCustom;
		}

#if NET_1_1
		public void SetAllowResponseInBrowserHistory (bool allow)
		{
			if (cacheability == HttpCacheability.NoCache ||
			    cacheability == HttpCacheability.ServerAndNoCache) {
				allowResponseInBrowserHistory = allow;
			}
		}
#endif

		internal void SetHeaders (HttpResponse response, ArrayList headers)
		{
			string cc, expires;
			if (cacheability > HttpCacheability.NoCache) {
				cc = String.Format ("{0}, max-age={1}", cacheability, (long) maxAge.TotalSeconds);
				expires = TimeUtil.ToUtcTimeString (expireDate);
				headers.Add (new HttpResponseHeader ("Expires", expires));
			} else {
				cc = "no-cache";
				response.CacheControl = cc;
#if NET_1_1
				if (!allowResponseInBrowserHistory)
#endif
				{
					expires = "-1";
					headers.Add (new HttpResponseHeader ("Expires", expires));
				}
			}
			
			headers.Add (new HttpResponseHeader ("Cache-Control", cc));
						
			if (etag != null)
				headers.Add (new HttpResponseHeader ("ETag", etag));

			if (haveLastModified)
				headers.Add (new HttpResponseHeader ("Last-Modified",
							     TimeUtil.ToUtcTimeString (lastModified)));

			if (!varyByParams.IgnoreParams) {
				HttpResponseHeader vb = varyByParams.GetResponseHeader ();
				if (vb != null)
					headers.Add (vb);
			}

		}
		
		#endregion // Methods
	}
}

