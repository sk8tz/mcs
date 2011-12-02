//
// MediaTypeHeaderValue.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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

using System.Collections.Generic;

namespace System.Net.Http.Headers
{
	public class MediaTypeHeaderValue : ICloneable
	{
		internal List<NameValueHeaderValue> parameters;
		string media_type;

		public MediaTypeHeaderValue (string mediaType)
		{
			if (mediaType == null)
				throw new ArgumentNullException ("mediaType");

			MediaType = mediaType;
		}

		protected MediaTypeHeaderValue (MediaTypeHeaderValue source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			media_type = source.media_type;
			if (source.parameters != null) {
				foreach (var item in source.parameters)
					Parameters.Add (new NameValueHeaderValue (item));
			}
		}

		public string CharSet {
			get {
				if (parameters == null)
					return null;

				var found = parameters.Find (l => StringComparer.OrdinalIgnoreCase.Equals (l.Name, "charset"));
				if (found == null)
					return null;

				return found.Value;
			}

			set {
				if (parameters == null)
					parameters = new List<NameValueHeaderValue> ();

				var found = parameters.Find (l => StringComparer.OrdinalIgnoreCase.Equals (l.Name, "charset"));
				if (string.IsNullOrEmpty (value)) {
					if (found != null)
						parameters.Remove (found);
				} else {
					if (found != null) {
						found.Value = value;
					} else {
						parameters.Add (new NameValueHeaderValue ("charset", value));
					}
				}
			}
		}

		public string MediaType {
			get {
				return media_type;
			}
			set {
				media_type = value;
			}
		}

		public ICollection<NameValueHeaderValue> Parameters {
			get {
				return parameters ?? (parameters = new List<NameValueHeaderValue> ());
			}
		}

		object ICloneable.Clone ()
		{
			return new MediaTypeHeaderValue (this);
		}

		public override bool Equals (object obj)
		{
			var source = obj as MediaTypeHeaderValue;
			if (source == null)
				return false;

			return string.Equals (source.media_type, media_type, StringComparison.OrdinalIgnoreCase) &&
				source.parameters.SequenceEqual (parameters);
		}

		public override int GetHashCode ()
		{
			return media_type.ToLowerInvariant ().GetHashCode () ^ HashCodeCalculator.Calculate (parameters);
		}

		public static MediaTypeHeaderValue Parse (string input)
		{
			MediaTypeHeaderValue value;
			if (TryParse (input, out value))
				return value;

			throw new FormatException (input);
		}

		public override string ToString ()
		{
			// TODO:
			return media_type;
		}
		
		public static bool TryParse (string input, out MediaTypeHeaderValue parsedValue)
		{
			throw new NotImplementedException ();
		}
	}
}
