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
// Copyright (c) 2004-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Text;
using System.Text;

namespace System.Windows.Forms
{
	internal class LineTag
	{
		#region	Local Variables;
		// Payload; formatting
		internal Font		font;		// System.Drawing.Font object for this tag
		internal SolidBrush	color;		// The font color for this tag

		// In 2.0 tags can have background colours.  I'm not going to #ifdef
		// at this level though since I want to reduce code paths
		internal SolidBrush	back_color;  

		// Payload; text
		internal int		start;		// start, in chars; index into Line.text
		internal bool		r_to_l;		// Which way is the font

		// Drawing support
		internal int		height;		// Height in pixels of the text this tag describes

		internal int		ascent;		// Ascent of the font for this tag
		internal int		shift;		// Shift down for this tag, to stay on baseline

		// Administrative
		internal Line		line;		// The line we're on
		internal LineTag	next;		// Next tag on the same line
		internal LineTag	previous;	// Previous tag on the same line
		#endregion;

		#region Constructors
		public LineTag (Line line, int start)
		{
			this.line = line;
			this.start = start;
		}
		#endregion	// Constructors

		#region Public Properties
		public int End {
			get { return start + Length; }
		}

		public virtual bool IsTextTag {
			get { return true; }
		}

		public int Length {
			get {
				int res = 0;
				if (next != null)
					res = next.start - start;
				else
					res = line.text.Length - (start - 1);

				return res > 0 ? res : 0;
			}
		}

		public int TextEnd {
			get { return start + TextLength; }
		}

		public int TextLength {
			get {
				int res = 0;
				if (next != null)
					res = next.start - start;
				else
					res = line.TextLengthWithoutEnding () - (start - 1);

				return res > 0 ? res : 0;
			}
		}

		public float Width {
			get {
				if (Length == 0)
					return 0;
				return line.widths [start + Length - 1] - (start != 0 ? line.widths [start - 1] : 0);
			}
		}

		public float X {
			get {
				if (start == 0)
					return line.X;
				return line.X + line.widths [start - 1];
			}
		}

		#endregion
		
		#region Internal Methods
		///<summary>Break a tag into two with identical attributes; pos is 1-based; returns tag starting at &gt;pos&lt; or null if end-of-line</summary>
		internal LineTag Break(int pos) {

			LineTag	new_tag;

			// Sanity
			if (pos == this.start) {
				return this;
			} else if (pos >= (start + Length)) {
				return null;
			}

			new_tag = new LineTag(line, pos);
			new_tag.CopyFormattingFrom (this);

			new_tag.next = this.next;
			this.next = new_tag;
			new_tag.previous = this;

			if (new_tag.next != null) {
				new_tag.next.previous = new_tag;
			}

			return new_tag;
		}

		/// <summary>Combines 'this' tag with 'other' tag</summary>
		internal bool Combine (LineTag other)
		{
			if (!this.Equals (other)) {
				return false;
			}

			this.next = other.next;
			if (this.next != null) {
				this.next.previous = this;
			}

			return true;
		}

		public void CopyFormattingFrom (LineTag other)
		{
			height = other.height;
			font = other.font;
			color = other.color;
			back_color = other.back_color;
		}

		internal virtual void Draw (Graphics dc, Brush brush, float x, float y, int start, int end)
		{
			TextBoxTextRenderer.DrawText (dc, line.text.ToString (start, end), font, brush, x, y, false);
		}

		internal virtual void Draw (Graphics dc, Brush brush, float xoff, float y, int start, int end, string text)
		{
			while (start < end) {
				int tab_index = text.IndexOf ("\t", start);
				if (tab_index == -1)
					tab_index = end;

				TextBoxTextRenderer.DrawText (dc, text.Substring (start, tab_index - start), font, brush, xoff + line.widths[start], y, false);

				// non multilines get the unknown char 
				if (!line.document.multiline && tab_index != end)
					TextBoxTextRenderer.DrawText (dc, "\u0013", font, brush, xoff + line.widths[tab_index], y, true);

				start = tab_index + 1;
			}
		}

		/// <summary>Checks if 'this' tag describes the same formatting options as 'obj'</summary>
		public override bool Equals (object obj)
		{
			LineTag other;

			if (obj == null)
				return false;

			if (!(obj is LineTag))
				return false;

			if (obj == this)
				return true;

			other = (LineTag)obj;

			if (other.IsTextTag != IsTextTag)
				return false;

			if (this.font.Equals (other.font) && this.color.Equals (other.color))	// FIXME add checking for things like link or type later
				return true;

			return false;
		}

		/// <summary>Finds the tag that describes the character at position 'pos' on 'line'</summary>
		internal static LineTag FindTag (Line line, int pos)
		{
			LineTag tag = line.tags;

			// Beginning of line is a bit special
			if (pos == 0) {
				// Not sure if we should get the final tag here
				return tag;
			}

			while (tag != null) {
				if ((tag.start <= pos) && (pos <= tag.End))
					return GetFinalTag (tag);

				tag = tag.next;
			}

			return null;
		}

		/// <summary>Applies 'font' and 'brush' to characters starting at 'start' for 'length' chars; 
		/// Removes any previous tags overlapping the same area; 
		/// returns true if lineheight has changed</summary>
		/// <param name="start">1-based character position on line</param>
		internal static bool FormatText (Line line, int start, int length, Font font, SolidBrush color, SolidBrush back_color, FormatSpecified specified)
		{
			LineTag tag;
			LineTag start_tag;
			LineTag end_tag;
			int end;
			bool retval = false;		// Assume line-height doesn't change

			// Too simple?
			if (((FormatSpecified.Font & specified) == FormatSpecified.Font) && font.Height != line.height)
				retval = true;

			line.recalc = true;		// This forces recalculation of the line in RecalculateDocument

			// A little sanity, not sure if it's needed, might be able to remove for speed
			if (length > line.text.Length)
				length = line.text.Length;

			tag = line.tags;
			end = start + length;

			// Common special case
			if ((start == 1) && (length == tag.Length)) {
				tag.ascent = 0;
				SetFormat (tag, font, color, back_color, specified);
				return retval;
			}

			start_tag = FindTag (line, start);
			tag = start_tag.Break (start);

			while (tag != null && tag.End <= end) {
				SetFormat (tag, font, color, back_color, specified);
				tag = tag.next;
			}

			if (tag != null && tag.End == end)
				return retval;

			/// Now do the last tag
			end_tag = FindTag (line, end);

			if (end_tag != null) {
				end_tag.Break (end);
				SetFormat (end_tag, font, color, back_color, specified);
			}

			return retval;
		}

		// There can be multiple tags at the same position, we want to make
		// sure we are using the very last tag at the given position
		internal static LineTag GetFinalTag (LineTag tag)
		{
			LineTag res = tag;

			while (res.Length == 0 && res.next != null && res.next.Length == 0)
				res = res.next;

			return res;
		}

		internal virtual int MaxHeight ()
		{
			return font.Height;
		}

		/// <summary>Remove 'this' tag ; to be called when formatting is to be removed</summary>
		internal bool Remove ()
		{
			if ((this.start == 1) && (this.next == null)) {
				// We cannot remove the only tag
				return false;
			}
			if (this.start != 1) {
				this.previous.next = this.next;
				this.next.previous = this.previous;
			} else {
				this.next.start = 1;
				this.line.tags = this.next;
				this.next.previous = null;
			}
			return true;
		}

		private static void SetFormat (LineTag tag, Font font, SolidBrush color, SolidBrush back_color, FormatSpecified specified)
		{
			if ((FormatSpecified.Font & specified) == FormatSpecified.Font)
				tag.font = font;
			if ((FormatSpecified.Color & specified) == FormatSpecified.Color)
				tag.color = color;
			if ((FormatSpecified.BackColor & specified) == FormatSpecified.BackColor) {
				tag.back_color = back_color;
			}
			// Console.WriteLine ("setting format:   {0}  {1}   new color {2}", color.Color, specified, tag.color.Color);
		}

		internal virtual SizeF SizeOfPosition (Graphics dc, int pos)
		{
			if (pos >= line.TextLengthWithoutEnding () && line.document.multiline)
				return SizeF.Empty;

			string text = line.text.ToString (pos, 1);
			switch ((int) text [0]) {
			case '\t':
				if (!line.document.multiline)
					goto case 10;
				SizeF res = TextBoxTextRenderer.MeasureText (dc, " ", font); 
				res.Width *= 8.0F;
				return res;
			case 10:
			case 13:
				return TextBoxTextRenderer.MeasureText (dc, "\u0013", font);
			}
			
			return TextBoxTextRenderer.MeasureText (dc, text, font);
		}

		public virtual string Text ()
		{
			return line.text.ToString (start - 1, Length);
		}

		public override string ToString ()
		{
			if (Length > 0)
				return string.Format ("{0} Tag starts at index: {1}, length: {2}, text: {3}, font: {4}", GetType (), start, Length, Text (), font.ToString ());
				
			return string.Format ("Zero Length tag at index: {0}", start);
		}
		#endregion	// Internal Methods
	}
}
