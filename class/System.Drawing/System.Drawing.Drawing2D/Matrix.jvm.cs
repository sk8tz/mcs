using System;
using System.Drawing;
using System.Runtime.InteropServices;
using geom = java.awt.geom;
using JMath = java.lang.Math;

namespace System.Drawing.Drawing2D
{
	public sealed class Matrix : MarshalByRefObject, IDisposable
	{
		#region fields

		geom.AffineTransform _nativeMatrix;				

		#endregion
                
		#region ctors

		Matrix (geom.AffineTransform ptr)
		{
			_nativeMatrix = ptr;
		}
                
		public Matrix () : this(new geom.AffineTransform())
		{		
		}
        
		public Matrix (Rectangle rect , Point[] plgpts)
		{
			double x1 = plgpts[1].X - plgpts[0].X;
			double y1 = plgpts[1].Y - plgpts[0].Y;
			
			double x2 = plgpts[2].X - plgpts[0].X;
			double y2 = plgpts[2].Y - plgpts[0].Y;

			_nativeMatrix = new geom.AffineTransform(x1/rect.Width, y1/rect.Width, x2/rect.Height, y2/rect.Height, plgpts[0].X, plgpts[0].Y);
			_nativeMatrix.translate(-rect.X,-rect.Y);
		}
        
		public Matrix (RectangleF rect , PointF[] plgpts)
		{
			double x1 = plgpts[1].X - plgpts[0].X;
			double y1 = plgpts[1].Y - plgpts[0].Y;
			
			double x2 = plgpts[2].X - plgpts[0].X;
			double y2 = plgpts[2].Y - plgpts[0].Y;

			_nativeMatrix = new geom.AffineTransform(x1/rect.Width, y1/rect.Width, x2/rect.Height, y2/rect.Height, plgpts[0].X, plgpts[0].Y);
			_nativeMatrix.translate(-rect.X,-rect.Y);
		}

		public Matrix (float m11, float m12, float m21, float m22, float dx, float dy)
			: this(new geom.AffineTransform(m11,m12,m21,m22,dx,dy))
		{
		}

		#endregion
        
		#region properties

		public float[] Elements 
		{
			get 
			{
				float [] elems = new float[] {
					(float)NativeObject.getScaleX(),
					(float)NativeObject.getShearY(),
					(float)NativeObject.getShearX(),
					(float)NativeObject.getScaleY(),
					(float)NativeObject.getTranslateX(),
					(float)NativeObject.getTranslateY()};
				return elems;
			}
		}
        
		public bool IsIdentity 
		{
			get 
			{
				return NativeObject.isIdentity();
			}
		}
        
		public bool IsInvertible 
		{
			get 
			{
				try
				{
					return NativeObject.getDeterminant() != 0.0;
				}
				catch(geom.NoninvertibleTransformException)
				{
					return false;
				}
			}
		}
        
		public float OffsetX 
		{
			get 
			{
				return (float)NativeObject.getTranslateX();
			}
		}
        
		public float OffsetY 
		{
			get 
			{
				return (float)NativeObject.getTranslateY();
			}
		}

		#endregion

		#region methods

		public Matrix Clone()
		{
			return new Matrix ((geom.AffineTransform)NativeObject.clone());
		}
                
        
		public void Dispose ()
		{
		}                       
        
		public override bool Equals (object obj)
		{
			Matrix m = obj as Matrix;
						

			if (m == null) 
				return false;

			return NativeObject.equals(m.NativeObject);
		}
                
		public override int GetHashCode ()
		{
			return NativeObject.hashCode();
		}
        
		public void Invert ()
		{
			try {
				_nativeMatrix = _nativeMatrix.createInverse();
			}
			catch(geom.NoninvertibleTransformException e) {
				throw new ArgumentException(e.Message, e);
			}
		}
        
		public void Multiply (Matrix matrix)
		{
			Multiply (matrix, MatrixOrder.Prepend);
		}
        
		public void Multiply (Matrix matrix, MatrixOrder order)
		{
			Multiply(matrix.NativeObject, order);
		}
        
		public void Reset()
		{
			NativeObject.setToIdentity();
		}
        
		public void Rotate (float angle)
		{
			Rotate (angle, MatrixOrder.Prepend);
		}
        
		public void Rotate (float angle, MatrixOrder order)
		{
			Multiply(geom.AffineTransform.getRotateInstance(JMath.toRadians(angle)), order);					
		}
        
		public void RotateAt (float angle, PointF point)
		{
			RotateAt (angle, point, MatrixOrder.Prepend);
		}
        
		public void RotateAt (float angle, PointF point, MatrixOrder order)
		{
			Multiply(geom.AffineTransform.getRotateInstance(JMath.toRadians(angle),point.X, point.Y), order);
		}
        
		public void Scale (float scaleX, float scaleY)
		{
			Scale (scaleX, scaleY, MatrixOrder.Append);
		}
        
		public void Scale (float scaleX, float scaleY, MatrixOrder order)
		{
			Multiply(geom.AffineTransform.getScaleInstance(scaleX, scaleY), order);
		}
        
		public void Shear (float shearX, float shearY)
		{
			Shear (shearX, shearY, MatrixOrder.Prepend);
		}
        
		public void Shear (float shearX, float shearY, MatrixOrder order)
		{
			Multiply(geom.AffineTransform.getShearInstance (shearX, shearY), order);
		}
        
		public void TransformPoints (Point[] pts)
		{
			geom.Point2D.Float pt = new geom.Point2D.Float();
			for(int i =0;i < pts.Length;i++) {
				pt.setLocation(pts[i].X,pts[i].Y);
				NativeObject.transform(pt,pt);
				pts[i].X=(int)pt.getX();
				pts[i].Y=(int)pt.getY();
			}
		}
        
		public void TransformPoints (PointF[] pts)
		{
			geom.Point2D.Float pt = new geom.Point2D.Float();
			for(int i =0;i < pts.Length;i++) {
				pt.setLocation(pts[i].X,pts[i].Y);
				NativeObject.transform(pt,pt);
				pts[i].X=(float)pt.getX();
				pts[i].Y=(float)pt.getY();
			}
		}
        
		public void TransformVectors (Point[] pts)
		{
			geom.Point2D.Float pt = new geom.Point2D.Float();
			for(int i =0;i < pts.Length;i++) {
				pt.setLocation(pts[i].X,pts[i].Y);
				NativeObject.deltaTransform(pt,pt);
				pts[i].X=(int)pt.getX();
				pts[i].Y=(int)pt.getY();
			}
		}
        
		public void TransformVectors (PointF[] pts)
		{
			geom.Point2D.Float pt = new geom.Point2D.Float();
			for(int i =0;i < pts.Length;i++) {
				pt.setLocation(pts[i].X,pts[i].Y);
				NativeObject.deltaTransform(pt,pt);
				pts[i].X=(float)pt.getX();
				pts[i].Y=(float)pt.getY();
			}
		}
        
		public void Translate (float offsetX, float offsetY)
		{
			Translate (offsetX, offsetY, MatrixOrder.Prepend);
		}
        
		public void Translate (float offsetX, float offsetY, MatrixOrder order)
		{
			Multiply(geom.AffineTransform.getTranslateInstance(offsetX, offsetY), order);
		}
        
		public void VectorTransformPoints (Point[] pts)
		{
			TransformVectors (pts);
		}
                
		internal geom.AffineTransform NativeObject
		{
			get
			{
				return _nativeMatrix;
			}
		}

		void Multiply(geom.AffineTransform at, MatrixOrder order) {
			Multiply(NativeObject, at, order);
		}

		internal static void Multiply(geom.AffineTransform to, geom.AffineTransform add, MatrixOrder order) {
			if(order == MatrixOrder.Prepend)
				to.concatenate(add);
			else
				to.preConcatenate(add);
		}

		#endregion
	}
}
