//
// System.Drawing.Pen.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing.Drawing2D;

namespace System.Drawing {

	public sealed class Pen : MarshalByRefObject, ICloneable, IDisposable {
		Brush brush;
		Color color;
		float width;

		internal IPen implementation;
		internal static IPenFactory factory = Factories.GetPenFactory();

		public Pen (Brush brush)
		{
			implementation = factory.Pen(brush, 1);
			this.brush = brush;
			width = 1;
		}

		public Pen (Color color)
		{
			implementation = factory.Pen(color, 1);
			this.color = color;
			width = 1;
		}

		public Pen (Brush brush, float width)
		{
			implementation = factory.Pen(brush, width);
			this.width = width;
			this.brush = brush;
		}

		public Pen (Color color, float width)
		{
			implementation = factory.Pen(color, width);
			this.width = width;
			this.color = color;
		}

		//
		// Properties
		//
		public PenAlignment Alignment {
			get {
				return implementation.Alignment;
			}

			set {
				implementation.Alignment = value;
			}
		}

		public Brush Brush {
			get {
				return brush;
			}

			set {
				brush = value;
			}
		}

		public Color Color {
			get {
				return color;
			}

			set {
				color = value;
			}
		}

		public float Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}
		
		public object Clone ()
		{
			return implementation.Clone ();
		}

		public void Dispose ()
		{
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		void Dispose (bool disposing)
		{
			implementation.Dispose();
		}

		~Pen ()
		{
			Dispose (false);
		}
	}
}
