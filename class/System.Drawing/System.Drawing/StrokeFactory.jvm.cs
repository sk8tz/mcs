using System;
using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing {
	internal sealed class StrokeFactory {

		StrokeFactory() {}

		interface StrokeCreator {
			awt.Stroke Create(float width, int cap, int join, float miterlimit,
				float[] dash, float dash_phase, geom.AffineTransform penTransform,
				geom.AffineTransform outputTransform, bool fitPen);
		}

		sealed class AdvancedCreator : StrokeCreator {
			#region StrokeCreator Members

			public awt.Stroke Create(float width, int cap, int join, float miterlimit, float[] dash, float dash_phase, geom.AffineTransform penTransform,
				geom.AffineTransform outputTransform, bool fitPen) {
				return new System.Drawing.AdvancedStroke(width, cap, join, miterlimit, dash, dash_phase, penTransform, outputTransform, fitPen);
			}

			#endregion
		}

		sealed class DefaultCreator : StrokeCreator {
			#region StrokeCreator Members

			public awt.Stroke Create(float width, int cap, int join, float miterlimit, float[] dash, float dash_phase, geom.AffineTransform penTransform,
				geom.AffineTransform outputTransform, bool fitPen) {
				return new awt.BasicStroke(width, cap, join, miterlimit, dash, dash_phase);
			}

			#endregion
		}

		static readonly StrokeCreator Creator;
		static StrokeFactory() {
			try {
				Type type = typeof(System.Drawing.AdvancedStroke);
				Activator.CreateInstance(type);
				Creator = new AdvancedCreator();
			}
			catch{
				Creator = new DefaultCreator();
			}
		}

		static public bool CanCreateAdvancedStroke {
			get {
				return !(Creator is DefaultCreator);
			}
		}

		static public awt.Stroke CreateStroke(float width, int cap, int join, float miterlimit,
			float[] dash, float dash_phase, geom.AffineTransform penTransform,
			geom.AffineTransform outputTransform, bool fitPen) {

			return Creator.Create(width, cap, join, miterlimit, dash, dash_phase, penTransform, outputTransform, fitPen);
		}
	}
}