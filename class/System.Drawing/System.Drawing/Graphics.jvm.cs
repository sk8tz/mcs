using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using awt = java.awt;
using geom = java.awt.geom;

namespace System.Drawing {
	[ComVisible(false)]
	public sealed class Graphics : MarshalByRefObject, IDisposable {
		sealed class DummyStroke : awt.Stroke {
			#region Stroke Members

			awt.Shape awt.Stroke.createStrokedShape(awt.Shape arg_0) {
				throw new NotImplementedException("DummyStroke");
			}

			#endregion
		}

		sealed class NormalizingPathIterator : geom.PathIterator {

			#region fields

			readonly geom.PathIterator _iter;

			const float norm = 0.5f;
			const float rnd = (1.0f - norm);
			float ax = 0.0f;
			float ay = 0.0f;

			#endregion

			#region ctor

			public NormalizingPathIterator(geom.PathIterator iter) {
				_iter = iter;
			}

			#endregion

			#region methods

			static int GetIndex(int type) {
				int index;
				switch ((GraphicsPath.JPI)type) {
					case GraphicsPath.JPI.SEG_CUBICTO:
						index = 4;
						break;
					case GraphicsPath.JPI.SEG_QUADTO:
						index = 2;
						break;
					case GraphicsPath.JPI.SEG_MOVETO:
					case GraphicsPath.JPI.SEG_LINETO:
						index = 0;
						break;
					case GraphicsPath.JPI.SEG_CLOSE:
					default:
						index = -1;
						break;
				}

				return index;
			}

			#endregion

			#region PathIterator Members

			void geom.PathIterator.next() {
				_iter.next();
			}

			bool geom.PathIterator.isDone() {
				return _iter.isDone();
			}

			int geom.PathIterator.currentSegment(float[] point) {
				int type = _iter.currentSegment(point);

				int index = GetIndex(type);
				
				if (index >= 0) {
					float ox = point[index];
					float oy = point[index+1];
					float newax = (float) java.lang.Math.floor(ox + rnd) + norm;
					float neway = (float) java.lang.Math.floor(oy + rnd) + norm;
					point[index] = newax;
					point[index+1] = neway;
					newax -= ox;
					neway -= oy;
					switch ((GraphicsPath.JPI)type) {
						case GraphicsPath.JPI.SEG_CUBICTO:
							point[0] += ax;
							point[1] += ay;
							point[2] += newax;
							point[3] += neway;
							break;
						case GraphicsPath.JPI.SEG_QUADTO:
							point[0] += (newax + ax) / 2;
							point[1] += (neway + ay) / 2;
							break;
							//							case GraphicsPath.JPI.SEG_MOVETO:
							//							case GraphicsPath.JPI.SEG_LINETO:
							//							case GraphicsPath.JPI.SEG_CLOSE:
							//								break;
					}
					ax = newax;
					ay = neway;
				}

				return type;
			}

			int geom.PathIterator.currentSegment(double[] point) {
				int type = _iter.currentSegment(point);

				int index = GetIndex(type);

				if (index >= 0) {
					float ox = (float)point[index];
					float oy = (float)point[index+1];
					float newax = (float)java.lang.Math.floor(ox + rnd) + norm;
					float neway = (float)java.lang.Math.floor(oy + rnd) + norm;
					point[index] = newax;
					point[index+1] = neway;
					newax -= ox;
					neway -= oy;
					switch ((GraphicsPath.JPI)type) {
						case GraphicsPath.JPI.SEG_CUBICTO:
							point[0] += ax;
							point[1] += ay;
							point[2] += newax;
							point[3] += neway;
							break;
						case GraphicsPath.JPI.SEG_QUADTO:
							point[0] += (newax + ax) / 2;
							point[1] += (neway + ay) / 2;
							break;
							//							case GraphicsPath.JPI.SEG_MOVETO:
							//							case GraphicsPath.JPI.SEG_LINETO:
							//							case GraphicsPath.JPI.SEG_CLOSE:
							//								break;
					}
					ax = newax;
					ay = neway;
				}

				return type;
			}

			int geom.PathIterator.getWindingRule() {
				return _iter.getWindingRule();
			}

			#endregion

		}


		#region Variables

		readonly awt.Graphics2D _nativeObject;
		PixelOffsetMode _pixelOffsetMode = PixelOffsetMode.Default;
		int _textContrast = 4;
		readonly Image _image;
		
		readonly Matrix _transform;
		GraphicsUnit _pageUnit = GraphicsUnit.Display;
		float _pageScale = 1.0f;

		readonly Region _clip;
		readonly awt.Rectangle _windowRect;

		GraphicsState _nextGraphicsState = null;

		static readonly float [] _unitConversion = {
													   1,								// World
													   1,								// Display
													   1,								// Pixel
													   DefaultScreenResolution / 72.0f,	// Point
													   DefaultScreenResolution,			// Inch
													   DefaultScreenResolution / 300.0f,// Document
													   DefaultScreenResolution / 25.4f	// Millimeter
												   };

		static int _isHeadless;
		static internal bool IsHeadless {
			get {
				if (_isHeadless == 0) {
					bool isHeadless = awt.GraphicsEnvironment.isHeadless();
					if (!isHeadless) {
						try {
							awt.Toolkit.getDefaultToolkit();
						}
						catch{
							isHeadless = true;
						}
					}

					_isHeadless = isHeadless ? 2 : 1;
				}

				return _isHeadless > 1;
			}
		}
	
		#endregion

#if INTPTR_SUPPORT
		[ComVisible(false)]
		public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
							    int flags,
							    int dataSize,
							    IntPtr data,
							    PlayRecordCallback callbackData);
		[ComVisible (false)]
			public delegate bool DrawImageAbort (IntPtr callbackData);		
#endif			

		#region Constr. and Destr.
		private Graphics (Image image) {
			_nativeObject = (awt.Graphics2D)image.NativeObject.CurrentImage.NativeImage.getGraphics();
			_image = image;
			_transform = new Matrix ();

			NativeObject.setStroke(new DummyStroke());
			NativeObject.setRenderingHint(awt.RenderingHints.KEY_COLOR_RENDERING, awt.RenderingHints.VALUE_COLOR_RENDER_QUALITY);

			InterpolationMode = InterpolationMode.Bilinear;

			_windowRect = new awt.Rectangle(_image.Width, _image.Height);
			_clip = new Region();
		}

		#endregion
		
		#region Internal Accessors

		static internal int DefaultScreenResolution {
			get {
				return IsHeadless ? 96 :
					awt.Toolkit.getDefaultToolkit().getScreenResolution();
			}
		}
		
		internal java.awt.Graphics2D NativeObject {
			get {
				return _nativeObject;
			}
		}
		#endregion

		#region FromImage (static accessor)
		public static Graphics FromImage (Image image) {		
			return new Graphics(image);
		}
		#endregion

		#region Workers [INTERNAL]
		void InternalSetBrush(Brush b) {
			java.awt.Graphics2D g = NativeObject;
			g.setPaint (b);
			SolidBrush sb = b as SolidBrush;
			if(sb != null) {
				g.setColor(sb.Color.NativeObject);
			}
			else if(b is LinearGradientBrush) {
#if DEBUG_GRADIENT_BRUSH
				if(((LinearGradientBrush)b).dPoints != null)
				{
					PointF []pts = ((LinearGradientBrush)b).dPoints;
					java.awt.Shape s = g.getClip();
					g.setClip(0,0,99999,99999);
					g.setPaint(new java.awt.Color(255,0,0));
					g.drawLine((int)pts[0].X,(int)pts[0].Y,(int)pts[1].X,(int)pts[1].Y);
					g.setPaint(new java.awt.Color(0,255,0));
					g.drawLine((int)pts[1].X,(int)pts[1].Y,(int)pts[2].X,(int)pts[2].Y);
					g.setPaint(new java.awt.Color(255,0,0));
					g.drawLine((int)pts[2].X,(int)pts[2].Y,(int)pts[3].X,(int)pts[3].Y);
					g.setPaint(new java.awt.Color(0,255,0));
					g.drawLine((int)pts[3].X,(int)pts[3].Y,(int)pts[0].X,(int)pts[0].Y);
					
					g.setClip(s);
				}
#endif
			}
		}

		void DrawShape(Pen pen, awt.Shape shape) {
			if (pen == null)
				throw new ArgumentNullException("pen");

			if (StrokeFactory.CanCreateAdvancedStroke) {
				geom.AffineTransform oldT = NativeObject.getTransform();
				NativeObject.setTransform(Matrix.IdentityTransform.NativeObject);

				try {
					geom.Area clip = _clip.NativeObject;
					geom.AffineTransform t = GetFinalTransform();
					if (!oldT.isIdentity()) {
						clip = (geom.Area)clip.clone();
						clip.transform(oldT);

						t = (geom.AffineTransform)t.clone();
						t.preConcatenate(oldT);
					}
					
					double widthsquared = pen.GetSquaredTransformedWidth(t);

					bool antiAlias = (SmoothingMode == SmoothingMode.AntiAlias);

					bool thin = (widthsquared <= (antiAlias ? 
						AdvancedStroke.MinPenSizeAASquared :
						AdvancedStroke.MinPenSizeSquared));

					if (NeedsNormalization) {

						bool normThin = 
							widthsquared <= AdvancedStroke.MinPenSizeSquaredNorm;

						if (normThin) {
							shape = GetNormalizedShape(shape, t);
							shape = pen.GetNativeObject(
								t, null, thin).createStrokedShape(shape);
						}
						else {
							shape = pen.GetNativeObject(t, thin).createStrokedShape(shape);
							shape = GetNormalizedShape(shape, null);
						}
					}
					else {
						shape = pen.GetNativeObject(t, thin).createStrokedShape(shape);
					}

					FillScaledShape(pen.Brush, shape, clip);
				}
				finally {
					NativeObject.setTransform(oldT);
				}
			}
			else {
				awt.Stroke oldStroke = NativeObject.getStroke();
				NativeObject.setStroke(pen.GetNativeObject(null, false));
				try {
					awt.Paint oldPaint = NativeObject.getPaint();
					NativeObject.setPaint(pen.Brush);
					try {
						geom.AffineTransform oldT = NativeObject.getTransform();
						NativeObject.transform(GetFinalTransform());
						try {
							shape = IntersectUserClip(shape);
							NativeObject.draw(shape);
						}
						finally {
							NativeObject.setTransform(oldT);
						}
					}
					finally {
						NativeObject.setPaint(oldPaint);
					}
				}
				finally {
					NativeObject.setStroke(oldStroke);
				}
			}
		}
		void FillShape(Brush paint, awt.Shape shape) {
			if (paint == null)
				throw new ArgumentNullException("brush");

			geom.AffineTransform oldT = null;
			geom.Area clip = _clip.NativeObject;
			if (NeedsNormalization) {
				oldT = NativeObject.getTransform();
				geom.AffineTransform t = GetFinalTransform();
				if (!oldT.isIdentity()) {
					t = (geom.AffineTransform)t.clone();
					t.preConcatenate(oldT);
					clip = (geom.Area)clip.clone();
					clip.transform(oldT);
				}
				shape = GetNormalizedShape(shape, t);
			}
			else {
				geom.AffineTransform t = GetFinalTransform();
				if (!t.isIdentity())
					shape = t.createTransformedShape(shape);
			}

			if (oldT != null)
				NativeObject.setTransform(Matrix.IdentityTransform.NativeObject);

			try {
				FillScaledShape(paint, shape, clip);
			}
			finally {
				if (oldT != null)
					NativeObject.setTransform(oldT);
			}
		}

		bool NeedsNormalization {
			get {
				return PixelOffsetMode != PixelOffsetMode.Half &&
					PixelOffsetMode != PixelOffsetMode.HighQuality;
			}
		}

		static awt.Shape GetNormalizedShape(awt.Shape shape, geom.AffineTransform t) {
			geom.PathIterator iter = new NormalizingPathIterator(shape.getPathIterator(t));
	
			geom.GeneralPath path = new geom.GeneralPath(iter.getWindingRule());
			path.append(iter, false);
			return path;
		}

		void FillScaledShape(Brush paint, awt.Shape shape, geom.Area clip) {
			geom.Rectangle2D r = shape.getBounds2D();
			awt.Paint oldP = NativeObject.getPaint();


			Matrix m = null;
			if (!_transform.IsIdentity) {
				m = paint.BrushTransform;
				paint.BrushMultiplyTransform( _transform );
			}

			try {
				NativeObject.setPaint(paint);
				try {
					shape = IntersectUserClip(shape, clip);
					NativeObject.fill(shape);
				}
				finally {
					NativeObject.setPaint(oldP);
				}
			}
			finally {
				if (m != null)
					paint.BrushTransform = m;
			}
		}

		internal SizeF MeasureDraw (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format, bool fDraw) {			
			SizeF retVal = new SizeF(0,0);
			awt.Graphics2D g = NativeObject;
				
			java.awt.Font fnt = font.NativeObject;
			if(s != null && s.Length != 0 && fnt != null) {
				float size = fnt.getSize();
				float wid = layoutRectangle.Width;
				float hei = layoutRectangle.Height;
				float x = layoutRectangle.X;
				float y = layoutRectangle.Y;
				java.text.AttributedString astr = new java.text.AttributedString(s);
				astr.addAttribute(java.awt.font.TextAttribute.FONT, fnt);
				java.text.AttributedCharacterIterator prg = astr.getIterator();
				java.awt.font.TextLayout textlayout = new java.awt.font.TextLayout(prg, g.getFontRenderContext());
				int prgStart = prg.getBeginIndex();
				int prgEnd = prg.getEndIndex();
				java.awt.font.LineBreakMeasurer lineMeasurer = new java.awt.font.LineBreakMeasurer(
					prg, new java.awt.font.FontRenderContext(null, false, false));
				lineMeasurer.setPosition(prgStart);
				float formatWidth = wid;
				
				//some vertical layout magic - should be reviewed
				//				if(format != null)
				//				{
				//					StringFormatFlags formatflag = format.FormatFlags;
				//					if(formatflag != StringFormatFlags.DirectionVertical)
				//					{
				//						if(size > (float)8 && wid > (float)13)
				//							formatWidth = wid - (float)12;
				//						if(formatflag ==  StringFormatFlags.DirectionRightToLeft && size == (float)6)
				//							formatWidth = wid - (float)10;
				//					}
				//				}

				//now calculate number of lines and full layout height
				//this is required for LineAlignment calculations....
				int lines=0;
				float layHeight=0;
				float layPrevHeight=0;
				float layWidth=0;
				float drawPosY = y;
				//bool fSkipLastLine = false;
				java.awt.font.TextLayout layout;
				java.awt.geom.Rectangle2D bnds = new java.awt.geom.Rectangle2D.Float();
				while(lineMeasurer.getPosition() < prgEnd) {
					layout = lineMeasurer.nextLayout(formatWidth);
					lines++;
					bnds = bnds.createUnion(layout.getBounds());
					layPrevHeight = layHeight;
					layHeight += layout.getDescent() + layout.getLeading() + layout.getAscent();
					float advance;
					if((format != null) && 
						((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0))
						advance = layout.getAdvance();
					else
						advance = layout.getVisibleAdvance();
					if(layWidth < advance)
						layWidth = advance;
					if((format != null) && ((format.FormatFlags & StringFormatFlags.NoWrap) != 0))
						break;
				}
				//Overhanging parts of glyphs, and unwrapped text reaching outside 
				//the formatting rectangle are allowed to show. By default all text 
				//and glyph parts reaching outside the formatting rectangle are clipped.
				if((lines == 1) && 
					(format != null) && 
					((format.FormatFlags & StringFormatFlags.NoClip) != 0)) {
					formatWidth = layWidth;					
				}

				//Only entire lines are laid out in the formatting rectangle. By default layout 
				//continues until the end of the text, or until no more lines are visible as a 
				//result of clipping, whichever comes first. Note that the default settings allow 
				//the last line to be partially obscured by a formatting rectangle that is not a 
				//whole multiple of the line height. To ensure that only whole lines are seen, specify
				//this value and be careful to provide a formatting rectangle at least as tall as the 
				//height of one line.
				if(format != null && ((format.FormatFlags & StringFormatFlags.LineLimit) != 0) && 
					layHeight > hei && layPrevHeight < hei) {
					layHeight = layPrevHeight;
					lines--;
				}

				retVal.Height = layHeight;
				retVal.Width = layWidth+size/2.5f;
				if(!fDraw)
					return retVal;

				InternalSetBrush(brush);
				g.setRenderingHint(awt.RenderingHints.KEY_TEXT_ANTIALIASING, awt.RenderingHints.VALUE_TEXT_ANTIALIAS_ON);

				//end measurment

				//for draw we should probably update origins

				//Vertical...
				if(format != null) {
					StringAlignment align = format.LineAlignment;
					if(align == StringAlignment.Center) {
						drawPosY = y + (float)hei/2 - layHeight/2;
					}
					else if(align == StringAlignment.Far) {
						drawPosY = (float)y + (float)hei - layHeight;
					}
					//in both cases if string is not fit - switch to near
					if(drawPosY < y)
						drawPosY = y;
				}

				//Horisontal... on the fly
				lineMeasurer.setPosition(prgStart);
				float drawPosX = x;				
				for(int line = 0;line < lines /*lineMeasurer.getPosition() < prgEnd*/;line++, drawPosY += layout.getDescent() + layout.getLeading()) {
					layout = lineMeasurer.nextLayout(formatWidth);
					drawPosX = x + size / (float)5;//???
					drawPosY += layout.getAscent();
					if(format != null) {
						float advance;
						if((format.FormatFlags & StringFormatFlags.MeasureTrailingSpaces) != 0)
							advance = layout.getAdvance();
						else
							advance = layout.getVisibleAdvance();
						
						if(format.Alignment == StringAlignment.Center) {
							drawPosX = (float)((double)x + ((double)formatWidth)/2 - advance/2);
						}
						else if(format.Alignment == StringAlignment.Far) {
							drawPosX = (float)(drawPosX + formatWidth) - advance;
						}
						if((format.FormatFlags & StringFormatFlags.DirectionVertical ) != 0) {
							java.awt.geom.AffineTransform at1 = java.awt.geom.AffineTransform.getTranslateInstance(
								drawPosX +  size / (float)5, (drawPosY - layout.getAscent()) + size / (float)5);

							at1.rotate(Math.PI/2);
							awt.Shape sha = textlayout.getOutline(at1);
							geom.AffineTransform t = GetFinalTransform();
							if (!t.isIdentity())
								sha = t.createTransformedShape(sha);
							//g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON);
							sha = IntersectUserClip(sha);
							g.fill(sha);
							continue;
						}
						if((format.FormatFlags & StringFormatFlags.DirectionRightToLeft)  != 0) {
							drawPosX = ((drawPosX + formatWidth) - advance) + (float)9;
							IntersectScaledClipWithBase(_clip);
							try {
								layout.draw(g, drawPosX, drawPosY);
							}
							finally {
								RestoreBaseClip();
							}
						} 
					}					

					geom.AffineTransform oldT = null;
					geom.AffineTransform ft = GetFinalTransform();

					//Draw current line
					IntersectScaledClipWithBase(_clip);
					try {
						if (!ft.isIdentity()) {
							oldT = NativeObject.getTransform();
							NativeObject.transform(ft);
						}
						layout.draw(g, drawPosX, drawPosY);					
					}
					finally {
						if (oldT != null)
							NativeObject.setTransform(oldT);
						RestoreBaseClip();
					}
				}
			} //not nulls

			return retVal;
		}
		#endregion

		#region Dispose
		public void Dispose() {			
			NativeObject.dispose();
		}
		#endregion
		
		#region Clear
		public void Clear (Color color) {
			FillScaledShape(new SolidBrush( color ), _clip.NativeObject, null);
		}
		#endregion

		#region DrawArc
		public void DrawArc (Pen pen, Rectangle rect, float startAngle, float sweepAngle) {
			DrawArc (pen, 
				rect.X, 
				rect.Y, 
				rect.Width, 
				rect.Height, 
				startAngle, 
				sweepAngle);
		}

		
		public void DrawArc (Pen pen, RectangleF rect, float startAngle, float sweepAngle) {
			DrawArc (pen, 
				rect.X, 
				rect.Y, 
				rect.Width, 
				rect.Height, 
				startAngle, 
				sweepAngle);
		}

		public void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle) {
			DrawArc(pen,
				(float)x,
				(float)y,
				(float)width,
				(float)height,
				(float)startAngle,
				(float)sweepAngle);
		}

		public void DrawArc (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) {
			GraphicsPath path = new GraphicsPath();
			path.AddArc(x, y, width, height, startAngle, sweepAngle);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawBezier(s)
		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4) {
			DrawBezier(pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
		}

		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4) {
			DrawBezier(pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
		}

		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4) {
			geom.GeneralPath path = new geom.GeneralPath();
			path.moveTo(x1,y1);
			path.curveTo(x2,y2,x3,y3,x4,y4);
			DrawShape(pen, path);
		}

		public void DrawBeziers (Pen pen, Point [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddBeziers(points);
			DrawPath(pen, path);
		}

		public void DrawBeziers (Pen pen, PointF [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddBeziers(points);
			DrawPath(pen, path);
		}
		#endregion 

		#region DrawClosedCurve
		public void DrawClosedCurve (Pen pen, PointF [] points) {
			DrawClosedCurve(pen, points, 0.5f, FillMode.Alternate);
		}
		
		public void DrawClosedCurve (Pen pen, Point [] points) {
			DrawClosedCurve(pen, points, 0.5f, FillMode.Alternate);
		}
 			
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			DrawPath(pen, path);
		}
		
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawCurve
		public void DrawCurve (Pen pen, Point [] points) {
			DrawCurve(pen, points, 0.5f);
		}
		
		public void DrawCurve (Pen pen, PointF [] points) {
			DrawCurve(pen, points, 0.5f);
		}
		
		public void DrawCurve (Pen pen, PointF [] points, float tension) {
			DrawCurve(pen, points, 0, points.Length-1, tension);
		}
		
		public void DrawCurve (Pen pen, Point [] points, float tension) {
			DrawCurve(pen, points, 0, points.Length-1, tension);
		}
		
		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments) {
			DrawCurve(pen, points, offset, numberOfSegments, 0.5f);
		}

		public void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension) {
			GraphicsPath path = new GraphicsPath();
			path.AddCurve(points, offset, numberOfSegments, tension);
			DrawPath(pen, path);
		}

		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension) {
			GraphicsPath path = new GraphicsPath();
			path.AddCurve(points, offset, numberOfSegments, tension);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawEllipse
		public void DrawEllipse (Pen pen, Rectangle rect) {
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, RectangleF rect) {
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, int x, int y, int width, int height) {
			DrawEllipse(pen,(float)x,(float)y,(float)width,(float)height);
		}

		public void DrawEllipse (Pen pen, float x, float y, float width, float height) {
			DrawShape(pen, new geom.Ellipse2D.Float(x,y,width,height));
		}
		#endregion

		#region DrawIcon
		public void DrawIcon (Icon icon, Rectangle targetRect) {
			Bitmap b = icon.ToBitmap ();
			this.DrawImage (b, targetRect);
		}

		public void DrawIcon (Icon icon, int x, int y) {
			Bitmap b = icon.ToBitmap ();
			this.DrawImage (b, x, y);
		}

		public void DrawIconUnstretched (Icon icon, Rectangle targetRect) {
			Bitmap b = icon.ToBitmap ();
			this.DrawImageUnscaled (b, targetRect);
		}
		#endregion

		#region DrawImage

		public void DrawImage (Image image, Point point) {
			DrawImage(image, point.X, point.Y);
		}

		public void DrawImage (Image image, PointF point) {
			DrawImage(image, point.X, point.Y);
		}

		
		public void DrawImage (Image image, Point [] destPoints) {
			Matrix m = new Matrix(new Rectangle(0, 0, image.Width, image.Height), destPoints);
			DrawImage(image, m);
		}

		public void DrawImage (Image image, PointF [] destPoints) {
			Matrix m = new Matrix(new RectangleF(0, 0, image.Width, image.Height), destPoints);
			DrawImage(image, m);
		}

		
		public void DrawImage (Image image, Rectangle rect) {
			DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawImage (Image image, RectangleF rect) {
			DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
		}

		
		public void DrawImage (Image image, int x, int y) {
			DrawImage(image, (float)x, (float)y);
		}

		public void DrawImage (Image image, float x, float y) {
			if ((image.HorizontalResolution != DpiX) || (image.VerticalResolution != DpiY))
				DrawImage( image, x, y, 
					(float)image.Width * (DpiX / image.HorizontalResolution), 
					(float)image.Height * (DpiY / image.VerticalResolution));
			else
				DrawImage( image, x, y, (float)image.Width, (float)image.Height );
		}

		
		public void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit) {
			DrawImage(
				image,
				new Point [] {
								 new Point( destRect.X, destRect.Y),
								 new Point( destRect.X + destRect.Width, destRect.Y),
								 new Point( destRect.X, destRect.Y + destRect.Height)},
				srcRect, 
				srcUnit);
		}
	
		public void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit) {
			DrawImage(
				image,
				new PointF [] {
								 new PointF( destRect.X, destRect.Y),
								 new PointF( destRect.X + destRect.Width, destRect.Y),
								 new PointF( destRect.X, destRect.Y + destRect.Height)},
				srcRect, 
				srcUnit);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit) {
			DrawImage(image, destPoints, srcRect, srcUnit, null);
		}

		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit) {
			DrawImage(image, destPoints, srcRect, srcUnit, null);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr) {
			//TBD: ImageAttributes
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			Matrix mx = new Matrix(srcRect, destPoints);

			Region region = new Region(srcRect);
			region.Transform (mx);
			
			geom.AffineTransform t = _transform.NativeObject;
			if (!t.isIdentity())
				region.NativeObject.transform(t);
			region.Intersect(_clip);

			DrawImage(image, mx, region);
		}
		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr) {
			//TBD: ImageAttributes
			if (srcUnit != GraphicsUnit.Pixel)
				throw new NotImplementedException();
			// Like in .NET http://dotnet247.com/247reference/msgs/45/227979.aspx

			Matrix mx = new Matrix(srcRect, destPoints);

			Region region = new Region(srcRect);
			region.Transform (mx);
			
			geom.AffineTransform t = GetFinalTransform();
			if (!t.isIdentity())
				region.NativeObject.transform(t);
			region.Intersect(_clip);

			DrawImage(image, mx, region);
		}


		public void DrawImage (Image image, int x, int y, int width, int height) {
			DrawImage(image, (float)x, (float)y, (float)width, (float)height);
		}

		public void DrawImage (Image image, float x, float y, float width, float height) {
			geom.Point2D.Float pt = new geom.Point2D.Float(x, y);
			GetFinalTransform().transform(pt, pt);

			Matrix mx = new Matrix();
			mx.Translate((float)pt.getX(), (float)pt.getY());
			mx.Scale(width / (float)image.Width, height / (float)image.Height);

			DrawImage( image, mx );
		}

		
		public void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit) {			
			DrawImage(image, new Rectangle(x, y, srcRect.Width, srcRect.Height), srcRect, srcUnit);
		}
		
		public void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit) {	
			DrawImage(image, new RectangleF(x, y, srcRect.Width, srcRect.Height), srcRect, srcUnit);
		}


		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit) {
			DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, null);
		}

		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit) {
			DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, null);
		}
		

		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr) {			
			//TBD: attributes
			DrawImage(
				image, 
				destRect,
				new Rectangle(srcX, srcY, srcWidth, srcHeight),
				srcUnit);
		}
		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs) {
			//TBD: attributes
			DrawImage(
				image, 
				destRect,
				new RectangleF(srcX, srcY, srcWidth, srcHeight),
				srcUnit);
		}
		

		internal void DrawImage (Image image, Matrix m) {
			DrawImage(image, m, null);
		}

		internal void DrawImage (Image image, Matrix m, Region clip) {
			if (clip == null) {
				clip = new Region( new RectangleF( 0, 0, image.Width, image.Height ) );
				clip.Transform( m );
			}
			clip.Intersect(_clip);
			
			geom.AffineTransform oldT = NativeObject.getTransform();
			// must set clip before the base transform is altered
			if (NeedsNormalization) {
				Matrix normMatrix = ComputeClipNormalization(clip.GetBounds(this));
				clip.Transform(normMatrix);
			}

			IntersectScaledClipWithBase(clip);
			
			try {
				NativeObject.transform(_transform.NativeObject);
				
				Matrix mm = ComputeImageNormalization(image, m);
				NativeObject.drawImage(image.NativeObject.CurrentImage.NativeImage, mm.NativeObject, null);
			}
			finally {
				NativeObject.setTransform(oldT);
				RestoreBaseClip();
			}
		}

		private static Matrix ComputeImageNormalization(Image img, Matrix m) {
			if ( m.IsIdentity )
				return m;

			//m.Translate( -(m.Elements[0] + m.Elements[2]) / 2.0f,  -(m.Elements[3] + m.Elements[1]) / 2.0f, MatrixOrder.Append);
			m.Translate( 
				-(float)(m.NativeObject.getScaleX() + m.NativeObject.getShearX()) / 2.0f,  
				-(float)(m.NativeObject.getScaleY() + m.NativeObject.getShearY()) / 2.0f, MatrixOrder.Append);
			
			PointF [] p = new PointF[] { 
										   new PointF( 0, 0 ),
										   new PointF( img.Width, 0 ),
										   new PointF( 0, img.Height )};

			m.TransformPoints(p);
			for (int i=0; i < p.Length; i++) {
				p[i].X = (float)( p[i].X + 0.5f );
				p[i].Y = (float)( p[i].Y + 0.5f );
			}

			return new Matrix( new Rectangle(0, 0, img.Width, img.Height), p );
		}
		private static Matrix ComputeClipNormalization(RectangleF rect) {
			PointF [] p = new PointF[] { 
										   new PointF( rect.X, rect.Y ),
										   new PointF( rect.X + rect.Width, rect.Y ),
										   new PointF( rect.X, rect.Y + rect.Height )};

			for (int i=0; i < p.Length; i++) {
				p[i].X = (float)Math.Round( p[i].X + 0.5f ) + 0.5f;
				p[i].Y = (float)Math.Round( p[i].Y + 0.5f ) + 0.5f;
			}

			return new Matrix( rect, p );
		}
		

#if INTPTR_SUPPORT
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException();
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			
			throw new NotImplementedException();
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException();
		}
#endif

#if INTPTR_SUPPORT		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException();
		}
#endif

#if INTPTR_SUPPORT		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,destRect.X,destRect.Y,destRect.Width,destRect.Height,srcX,srcY,srcWidth,srcHeight,null);
		}
		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,
				(int)destRect.X,
				(int)destRect.Y,
				(int)destRect.Width,
				(int)destRect.Height,
				(int)srcX,
				(int)srcY,
				(int)srcWidth,
				(int)srcHeight,null);
		}

		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,
				(int)destRect.X,
				(int)destRect.Y,
				(int)destRect.Width,
				(int)destRect.Height,
				(int)srcX,
				(int)srcY,
				(int)srcWidth,
				(int)srcHeight,null);
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			//TBD:units,attributes, callback
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;
			g.drawImage(image.NativeObject,
				destRect.X,
				destRect.Y,
				destRect.Width,
				destRect.Height,
				srcX,
				srcY,
				srcWidth,
				srcHeight,null);
		}		
#endif
		
		public void DrawImageUnscaled (Image image, Point point) 
		{
			DrawImageUnscaled (image, point.X, point.Y);
		}
		
		public void DrawImageUnscaled (Image image, Rectangle rect) {
			DrawImageUnscaled (image, rect.X, rect.Y, rect.Width, rect.Height);
		}
		
		public void DrawImageUnscaled (Image image, int x, int y) {
			DrawImage (image, x, y, image.Width, image.Height);
		}

		public void DrawImageUnscaled (Image image, int x, int y, int width, int height) {
			Image tmpImg = new Bitmap (width, height);
			Graphics g = FromImage (tmpImg);
			g.DrawImage (image, 0, 0, image.Width, image.Height);
			this.DrawImage (tmpImg, x, y, width, height);
			tmpImg.Dispose ();
			g.Dispose ();		
		}
		#endregion

		#region DrawLine
		public void DrawLine (Pen pen, PointF pt1, PointF pt2) {
			DrawLine(pen,pt1.X,pt1.Y,pt2.X,pt2.Y);
		}

		public void DrawLine (Pen pen, Point pt1, Point pt2) {
			DrawLine(pen,(float)pt1.X,(float)pt1.Y,(float)pt2.X,(float)pt2.Y);
		}

		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2) {
			DrawLine(pen,(float)x1,(float)y1,(float)x2,(float)y2);
		}

		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2) {
			DrawShape(pen, new geom.Line2D.Float(x1,y1,x2,y2));
		}

		public void DrawLines (Pen pen, PointF [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddLines(points);
			DrawShape(pen, path);
		}

		public void DrawLines (Pen pen, Point [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddLines(points);
			DrawShape(pen, path);
		}
		#endregion

		#region DrawPath
		public void DrawPath (Pen pen, GraphicsPath path) {
			DrawShape(pen, path);
		}
		#endregion
		
		#region DrawPie
		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle) {
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle) {
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) {
			GraphicsPath path = new GraphicsPath();
			path.AddPie(x, y, width, height, startAngle, sweepAngle);
			DrawPath(pen, path);
		}
		
		public void DrawPie (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle) {
			DrawPie(pen,(float)x,(float)y,(float)width,(float)height,(float)startAngle,(float)sweepAngle);
		}
		#endregion

		#region DrawPolygon
		public void DrawPolygon (Pen pen, Point [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddPolygon(points);
			DrawPath(pen, path);
		}

		public void DrawPolygon (Pen pen, PointF [] points) {
			GraphicsPath path = new GraphicsPath();
			path.AddPolygon(points);
			DrawPath(pen, path);
		}
		#endregion

		#region DrawRectangle(s)
		internal void DrawRectangle (Pen pen, RectangleF rect) {
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, Rectangle rect) {
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, float x, float y, float width, float height) {
			DrawShape(pen, new geom.Rectangle2D.Float(x,y,width,height));
		}

		public void DrawRectangle (Pen pen, int x, int y, int width, int height) {
			DrawRectangle (pen,(float) x,(float) y,(float) width,(float) height);
		}

		public void DrawRectangles (Pen pen, RectangleF [] rects) {
			foreach(RectangleF r in rects)
				DrawRectangle (pen, r.Left, r.Top, r.Width, r.Height);
		}

		public void DrawRectangles (Pen pen, Rectangle [] rects) {
			foreach(Rectangle r in rects)
				DrawRectangle (pen, r.Left, r.Top, r.Width, r.Height);
		}
		#endregion

		#region DrawString
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle) {			
			MeasureDraw(s,font,brush,layoutRectangle,null,true);
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point) {
			MeasureDraw(s,font,brush,new RectangleF(point.X,point.Y,99999f,99999f),null,true);
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format) {
			MeasureDraw(s,font,brush,new RectangleF(point.X,point.Y,99999f,99999f),format,true);
		}

		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format) {
			MeasureDraw(s,font,brush,layoutRectangle,format,true);
		}
	
#if false		
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			java.awt.Graphics2D g = (java.awt.Graphics2D)nativeObject;			
			//set
			java.awt.Paint oldpaint = g.getPaint();
			g.setPaint(brush.NativeObject);
			java.awt.Font oldfont = g.getFont();
			g.setFont(font.NativeObject);
			java.awt.Shape oldb = g.getClip();
			g.setClip(new java.awt.geom.Rectangle2D.Float(layoutRectangle.X,layoutRectangle.Y,layoutRectangle.Width,layoutRectangle.Height));
			//draw
			g.drawString(s,layoutRectangle.X,layoutRectangle.Y+layoutRectangle.Height);
			//restore
			g.setPaint(oldpaint);
			g.setClip(oldb);
			g.setFont(oldfont);
		}
#endif
		public void DrawString (string s, Font font, Brush brush, float x, float y) {
			MeasureDraw(s,font,brush,new RectangleF(x,y,99999f,99999f),null,true);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format) {
			MeasureDraw(s,font,brush,new RectangleF(x,y,99999f,99999f),format,true);
		}
		#endregion

		#region Container

		void PushGraphicsState(GraphicsState state) {
			state.Next = _nextGraphicsState;
			_nextGraphicsState = state;
		}

		GraphicsState PopGraphicsState() {
			GraphicsState state = _nextGraphicsState;
			_nextGraphicsState = _nextGraphicsState.Next;
			return state;
		}

		bool ContainsGraphicsState(GraphicsState state) {
			GraphicsState gs = _nextGraphicsState;

			while(gs != null) {
				if (gs == state)
					return true;

				gs = gs.Next;
			}

			return false;
		}

		public void EndContainer (GraphicsContainer container) {
			Restore(container.StateObject);
		}

		public GraphicsContainer BeginContainer () {
			return new GraphicsContainer(Save(Matrix.IdentityTransform, true));
		}
		
		public GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit) {
			Matrix containerTransfrom =
				new Matrix(	srcrect,
				new Point [] {	 new Point (dstrect.X, dstrect.Y), 
								 new Point (dstrect.X + dstrect.Width, dstrect.Y), 
								 new Point (dstrect.X, dstrect.Y + dstrect.Height) });

			float scale = 1/_unitConversion[ (int)unit ];
			containerTransfrom.Scale(scale, scale);

			return new GraphicsContainer(Save(containerTransfrom, true));
		}

		
		public GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit) {
			Matrix containerTransfrom =
				new Matrix(	srcrect,
				new PointF [] {	 new PointF (dstrect.X, dstrect.Y), 
								 new PointF (dstrect.X + dstrect.Width, dstrect.Y), 
								 new PointF (dstrect.X, dstrect.Y + dstrect.Height) });

			float scale = 1/_unitConversion[ (int)unit ];
			containerTransfrom.Scale(scale, scale);

			return new GraphicsContainer(Save(containerTransfrom, true));
		}

		GraphicsState Save(Matrix matrix, bool resetState) {
			GraphicsState graphicsState = new GraphicsState(this, matrix, resetState);

			PushGraphicsState( graphicsState );
			return graphicsState;
		}

		public GraphicsState Save () {
			return Save(Matrix.IdentityTransform, false);
		}

		public void Restore (GraphicsState graphicsState) {
			if (ContainsGraphicsState(graphicsState)) {
				GraphicsState gs = PopGraphicsState();
				while ( gs != graphicsState )
					gs = PopGraphicsState();

				graphicsState.RestoreState(this);
			}
		}

		#endregion

		#region Metafiles Staff [TODO NotSupp]
		public void AddMetafileComment (byte [] data) {
			throw new NotImplementedException ();
		}

#if INTPTR_SUPPORT
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}
#endif 	
		#endregion	

		#region ExcludeClip
		void ExcludeClip(geom.Area area) {

			geom.AffineTransform t = GetFinalTransform();
			if (!t.isIdentity()) {
				area = (geom.Area)area.clone();
				area.transform(t);
			}

			_clip.NativeObject.subtract(area);
		}

		public void ExcludeClip (Rectangle rect) {
			ExcludeClip(new geom.Area(rect.NativeObject));
		}

		public void ExcludeClip (Region region) {
			if (region == null)
				throw new ArgumentNullException("region");
			ExcludeClip(region.NativeObject);
		}
		#endregion 

		#region FillClosedCurve
		public void FillClosedCurve (Brush brush, PointF [] points) {
			FillClosedCurve (brush, points, FillMode.Alternate);
		}

		
		public void FillClosedCurve (Brush brush, Point [] points) {
			FillClosedCurve (brush, points, FillMode.Alternate);
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode) {
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}
		
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode) {
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}

		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			FillPath(brush, path);
		}

		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension) {
			GraphicsPath path = new GraphicsPath(fillmode);
			path.AddClosedCurve(points, tension);
			FillPath(brush, path);
		}
		#endregion

		#region FillEllipse
		public void FillEllipse (Brush brush, Rectangle rect) {
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, RectangleF rect) {
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, float x, float y, float width, float height) {
			FillShape(brush,new java.awt.geom.Ellipse2D.Float(x,y,width,height));
		}

		public void FillEllipse (Brush brush, int x, int y, int width, int height) {
			FillEllipse (brush,(float)x,(float)y,(float)width,(float)height);
		}
		#endregion

		#region FillPath
		public void FillPath (Brush brush, GraphicsPath path) {
			if (path == null)
				throw new ArgumentNullException("path");

			FillShape(brush,path);
		}
		#endregion

		#region FillPie
		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle) {
			FillPie(brush,(float)rect.X,(float)rect.Y,(float)rect.Width,(float)rect.Height,(float)startAngle,(float)sweepAngle);
		}

		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle) {
			FillPie(brush,(float)x,(float)y,(float)width,(float)height,(float)startAngle,(float)sweepAngle);
		}

		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle) {
			GraphicsPath path = new GraphicsPath();
			path.AddPie(x, y, width, height, startAngle, sweepAngle);
			FillPath(brush, path);
		}
		#endregion

		#region FillPolygon
		public void FillPolygon (Brush brush, PointF [] points) {
			FillPolygon(brush, points, FillMode.Alternate);
		}

		public void FillPolygon (Brush brush, Point [] points) {
			FillPolygon(brush, points, FillMode.Alternate);
		}

		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode) {
			GraphicsPath path = new GraphicsPath(fillMode);
			path.AddPolygon(points);
			FillPath(brush,path);
		}

		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode) {
			GraphicsPath path = new GraphicsPath(fillMode);
			path.AddPolygon(points);
			FillPath(brush,path);
		}
		#endregion

		#region FillRectangle
		public void FillRectangle (Brush brush, RectangleF rect) {
			FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, Rectangle rect) {
			FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, int x, int y, int width, int height) {
			FillRectangle(brush,(float)x,(float)y,(float)width,(float)height);
		}

		public void FillRectangle (Brush brush, float x, float y, float width, float height) {
			FillShape(brush,new java.awt.geom.Rectangle2D.Float(x,y,width,height));
		}

		public void FillRectangles (Brush brush, Rectangle [] rects) {
			GraphicsPath path = new GraphicsPath();
			path.AddRectangles(rects);
			FillPath(brush,path);
		}

		public void FillRectangles (Brush brush, RectangleF [] rects) {
			GraphicsPath path = new GraphicsPath();
			path.AddRectangles(rects);
			FillPath(brush,path);
		}
		#endregion

		#region FillRegion
		public void FillRegion (Brush brush, Region region) {
			FillShape(brush,region);
		}

		#endregion

		public void Flush () {
			Flush (FlushIntention.Flush);
		}

		
		public void Flush (FlushIntention intention) {
			if (_image != null)
				_image.NativeObject.CurrentImage.NativeImage.flush();
		}

#if INTPTR_SUPPORTED
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ReleaseHdc (IntPtr hdc)
		{
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ReleaseHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHdc (IntPtr hdc)
		{
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHdc (IntPtr hdc, IntPtr hdevice)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHwndInternal (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		internal static Graphics FromXDrawable (IntPtr drawable, IntPtr display)
		{
			throw new NotImplementedException();
		}

		public static IntPtr GetHalftonePalette ()
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IntPtr GetHdc ()
		{
			throw new NotImplementedException();
		}
#endif
		
		#region GetNearestColor [TODO]
		public Color GetNearestColor (Color color) {
			throw new NotImplementedException();
		}
		#endregion

		#region IntersectClip
		void IntersectClip (geom.Area area) {
			
			geom.AffineTransform t = GetFinalTransform();
			if (!t.isIdentity()) {
				area = (geom.Area)area.clone();
				area.transform(t);
			}

			_clip.NativeObject.intersect(area);
		}

		public void IntersectClip (Region region) {
			if (region == null)
				throw new ArgumentNullException("region");

			IntersectClip(region.NativeObject);
		}
		
		public void IntersectClip (RectangleF rect) {
			IntersectClip(new geom.Area(rect.NativeObject));
		}

		public void IntersectClip (Rectangle rect) {			
			IntersectClip(new geom.Area(rect.NativeObject));
		}
		#endregion

		#region IsVisible
		public bool IsVisible (Point point) {
			return IsVisible(point.X,point.Y);
		}

		
		public bool IsVisible (RectangleF rect) {
			return IsVisible ((float)rect.X,(float)rect.Y,(float)rect.Width,(float)rect.Height);
		}

		public bool IsVisible (PointF point) {
			return IsVisible(point.X,point.Y);
		}
		
		public bool IsVisible (Rectangle rect) {
			return IsVisible ((float)rect.X,(float)rect.Y,(float)rect.Width,(float)rect.Height);
		}
		
		public bool IsVisible (float x, float y) {
			double dx = x;
			double dy = y;
			geom.AffineTransform t = GetFinalTransform();
			if (!t.isIdentity()) {
				double[] p = new double[] {dx, dy};
				t.transform(p, 0, p, 0, 1);

				dx = p[0];
				dy = p[1];
			}
			if (!_clip.NativeObject.contains(dx, dy))
				return false;

			awt.Shape clip = NativeObject.getClip();
			if (clip == null)
				return true;

			return clip.contains(dx, dy);
		}
		
		public bool IsVisible (int x, int y) {
			return IsVisible ((float)x,(float)y);
		}
		
		public bool IsVisible (float x, float y, float width, float height) {

			geom.AffineTransform t = GetFinalTransform();
			geom.Rectangle2D r = new geom.Rectangle2D.Float(x, y, width, height);
			
			if (!t.isIdentity())
				r = t.createTransformedShape(r).getBounds2D();
		
			return NativeObject.hitClip(
					(int)(r.getX()+0.5), (int)(r.getY()+0.5),
					(int)(r.getWidth()+0.5), (int)(r.getHeight()+0.5))
				&& _clip.NativeObject.intersects(r);
		}

		
		public bool IsVisible (int x, int y, int width, int height) {
			return IsVisible ((float)x,(float)y,(float)width,(float)height);
		}
		#endregion

		#region MeasureCharacterRanges [TODO]
		public Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat) {
			throw new NotImplementedException();
		}
		#endregion
		
		#region MeasureString [1 method still TODO]
		public SizeF MeasureString (string text, Font font) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,99999f,99999f),null,false);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,layoutArea.Width,layoutArea.Height),null,false);
		}

		
		public SizeF MeasureString (string text, Font font, int width) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,(float)width,99999f),null,false);
		}


		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat format) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,layoutArea.Width,layoutArea.Height),format,false);
		}

		
		public SizeF MeasureString (string text, Font font, int width, StringFormat format) {
			return MeasureDraw(text,font,null,new RectangleF(0,0,(float)width,99999f),format,false);
		}

		
		public SizeF MeasureString (string text, Font font, PointF origin, StringFormat format) {
			//TBD: MeasureDraw still not dealing with clipping region of dc
			return MeasureDraw(text,font,null,new RectangleF(origin.X,origin.Y,99999f,99999f),format,false);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, out int charactersFitted, out int linesFilled) {	
			//TBD: charcount
			throw new NotImplementedException();
		}
		#endregion

		#region MultiplyTransform
		public void MultiplyTransform (Matrix matrix) {
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order) {
			ConcatenateTransform(matrix.NativeObject, order);
		}
		#endregion

		#region Reset (Clip and Transform)
		public void ResetClip () {
			_clip.MakeInfinite();
		}

		public void ResetTransform () {
			_transform.Reset();
		}
		#endregion

		#region RotateTransform
		public void RotateTransform (float angle) {
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order) {
			ConcatenateTransform(
				geom.AffineTransform.getRotateInstance(java.lang.Math.toRadians(angle)),
				order);
		}
		#endregion

		#region ScaleTransform
		public void ScaleTransform (float sx, float sy) {
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order) {
			ConcatenateTransform(
				geom.AffineTransform.getScaleInstance(sx, sy),
				order);
		}
		#endregion

		#region SetClip [Must be reviewed - more abstraction needed]
		public void SetClip (RectangleF rect) {
			SetClip (rect, CombineMode.Replace);
		}

		public void SetClip (GraphicsPath path) {
			SetClip (path, CombineMode.Replace);
		}

		public void SetClip (Rectangle rect) {
			SetClip (rect, CombineMode.Replace);
		}

		public void SetClip (Graphics g) {
			SetClip (g, CombineMode.Replace);
		}

		public void SetClip (Graphics g, CombineMode combineMode) {
			if(g == null)
				throw new NullReferenceException();
			
			CombineClipArea(g._clip.NativeObject, combineMode);
		}

		public void SetClip (Rectangle rect, CombineMode combineMode) {
			SetClip(rect.X,rect.Y,rect.Width,rect.Height,combineMode);			
		}		
		public void SetClip (RectangleF rect, CombineMode combineMode) {			
			SetClip(rect.X,rect.Y,rect.Width,rect.Height,combineMode);			
		}
		
		public void SetClip (Region region, CombineMode combineMode) {
			if(region == null)
				throw new ArgumentNullException("region");

			CombineClipArea((geom.Area)region.NativeObject.clone(),combineMode);
		}
		
		public void SetClip (GraphicsPath path, CombineMode combineMode) {
			if(path == null)
				throw new ArgumentNullException("path");

			CombineClipArea(new geom.Area(path.NativeObject), combineMode);
		}
		#endregion

		#region Clipping Staff [INTERNAL]
		internal Region ScaledClip {
			get {
				return _clip.Clone();
			}
			set {
				_clip.NativeObject.reset();
				_clip.NativeObject.add(value.NativeObject);
			}
		}
		internal void SetClip(float x,float y,float width,float height,CombineMode combineMode) {					
			CombineClipArea(new geom.Area(
				new geom.Rectangle2D.Float(x,y,width,height)),combineMode);
		}

		void CombineClipArea(geom.Area area, CombineMode combineMode) {
			geom.AffineTransform t = GetFinalTransform();
			if (!t.isIdentity())
				area.transform(t);
			if (combineMode == CombineMode.Replace) {
				_clip.NativeObject.reset();
				_clip.NativeObject.add(area);
				return;
			}

			geom.Area curClip = _clip.NativeObject;
			switch(combineMode) {
				case CombineMode.Complement:
					curClip.add(area);
					break;
				case CombineMode.Exclude:
					curClip.subtract(area);
					break;
				case CombineMode.Intersect:
					curClip.intersect(area);
					break;
				case CombineMode.Union:
					curClip.add(area);
					break;
				case CombineMode.Xor:
					curClip.exclusiveOr(area);
					break;
				default:
					throw new ArgumentOutOfRangeException();					
			}
		}

		internal void IntersectScaledClipWithBase(awt.Shape clip) {
			NativeObject.clip(clip);
		}

		awt.Shape IntersectUserClip(awt.Shape shape, geom.Area clip) {
			if (clip == null)
				return shape;

			geom.Area area = new geom.Area(shape);
			area.intersect(clip);
			return area;
		}

		awt.Shape IntersectUserClip(awt.Shape shape) {
			return IntersectUserClip(shape, _clip.NativeObject);
		}

		void RestoreBaseClip() {
			if (_nextGraphicsState == null) {
				NativeObject.setClip(null);
				return;
			}

			_nextGraphicsState.RestoreBaseClip(this);
		}
		
		#endregion
		
		#region TransformPoints
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts) {
			//TBD:CoordinateSpace
			java.awt.geom.AffineTransform tr = this.Transform.NativeObject;
			float[] fpts = new float[2];
			for(int i = 0; i< pts.Length; i++) {
				fpts[0] = pts[i].X;
				fpts[1] = pts[i].Y;
				tr.transform(fpts, 0, fpts, 0, 1);
				pts[i].X = fpts[0];
				pts[i].Y = fpts[1];
			}
		}

		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts) {						
			//TBD:CoordinateSpace
			java.awt.geom.AffineTransform tr = this.Transform.NativeObject;
			float[] fpts = new float[2];
			for(int i = 0; i< pts.Length; i++) {
				fpts[0] = pts[i].X;
				fpts[1] = pts[i].Y;
				tr.transform(fpts, 0, fpts, 0, 1);
				pts[i].X = (int)fpts[0];
				pts[i].Y = (int)fpts[1];
			}
		}
		#endregion

		#region TranslateClip
		public void TranslateClip (int dx, int dy) {
			TranslateClip((float)dx, (float)dy);
		}

		
		public void TranslateClip (float dx, float dy) {
			double x = dx;
			double y = dy;
			geom.AffineTransform f = GetFinalTransform();

			if (!f.isIdentity()) {
				double[] p = new double[] {x, y};
				f.deltaTransform(p, 0, p, 0, 1);

				x = p[0];
				y = p[1];
			}

			// It seems .Net does exactly this...
			x = Math.Floor(x+0.96875);
			y = Math.Floor(y+0.96875);

			geom.AffineTransform t = geom.AffineTransform.getTranslateInstance(x, y);

			_clip.NativeObject.transform(t);
		}
		#endregion

		#region TranslateTransform
		public void TranslateTransform (float dx, float dy) {
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		
		public void TranslateTransform (float dx, float dy, MatrixOrder order) {
			ConcatenateTransform(
				geom.AffineTransform.getTranslateInstance(dx, dy), 
				order);
		}
		#endregion

		#region Properties [Partial TODO]
		public Region Clip {
			get {
				Region r = _clip.Clone();
				geom.AffineTransform t = GetFinalTransform();
				if (!t.isIdentity())
					r.NativeObject.transform(t.createInverse());

				return r;
			}
			set {
				SetClip (value, CombineMode.Replace);
			}
		}

		public RectangleF ClipBounds {
			get {
				awt.Shape shape = _clip.NativeObject;
				if (shape == null)
					shape = Region.InfiniteRegion.NativeObject;

				geom.RectangularShape r = shape.getBounds2D();
				geom.AffineTransform t = GetFinalTransform();
				if (!t.isIdentity()) {
					geom.AffineTransform it = t.createInverse();
					r = it.createTransformedShape(r).getBounds2D();
				}

				return new RectangleF (r);
			}
		}

		public CompositingMode CompositingMode {
			//TBD:check this carefully
			get {
				return (NativeObject.getComposite() == awt.AlphaComposite.SrcOver) ?
					CompositingMode.SourceOver : CompositingMode.SourceCopy;
			}
			set {
				NativeObject.setComposite(
					(value == CompositingMode.SourceOver) ?
					awt.AlphaComposite.SrcOver : awt.AlphaComposite.Src);
			}

		}

		public CompositingQuality CompositingQuality {
			get {
				awt.RenderingHints hints = NativeObject.getRenderingHints();
				if(hints.containsKey(awt.RenderingHints.KEY_ALPHA_INTERPOLATION)) {
					object value_ai = hints.get(awt.RenderingHints.KEY_ALPHA_INTERPOLATION);

					if (value_ai == awt.RenderingHints.VALUE_ALPHA_INTERPOLATION_SPEED)
						return CompositingQuality.HighSpeed;
					if (value_ai == awt.RenderingHints.VALUE_ALPHA_INTERPOLATION_QUALITY)
						return CompositingQuality.HighQuality;
					if (value_ai == awt.RenderingHints.VALUE_ALPHA_INTERPOLATION_DEFAULT)
						return CompositingQuality.Default;
				}

				return CompositingQuality.Default;
					
			}
			set {
				awt.RenderingHints hints = NativeObject.getRenderingHints();
				switch (value) {
					case CompositingQuality.AssumeLinear:
					case CompositingQuality.Default:
					case CompositingQuality.GammaCorrected:
						hints.put(awt.RenderingHints.KEY_ALPHA_INTERPOLATION,
							awt.RenderingHints.VALUE_ALPHA_INTERPOLATION_DEFAULT);
						break;
					case CompositingQuality.HighQuality:
						hints.put(awt.RenderingHints.KEY_ALPHA_INTERPOLATION,
							awt.RenderingHints.VALUE_ALPHA_INTERPOLATION_QUALITY);
						break;
					case CompositingQuality.HighSpeed:
						hints.put(awt.RenderingHints.KEY_ALPHA_INTERPOLATION,
							awt.RenderingHints.VALUE_ALPHA_INTERPOLATION_SPEED);
						break;
//					case CompositingQuality.Invalid:
//						if(hints.containsKey(awt.RenderingHints.KEY_ALPHA_INTERPOLATION))
//							hints.remove(awt.RenderingHints.KEY_ALPHA_INTERPOLATION);
				}

				NativeObject.setRenderingHints(hints);
			}
		}

		public float DpiX {
			get {				
				return DefaultScreenResolution;
			}
		}

		public float DpiY {
			get {
				//TBD: assume 72 (screen) for now
				return DpiX;
			}
		}

		public InterpolationMode InterpolationMode {
			get {				
				awt.RenderingHints hints = NativeObject.getRenderingHints();
				if(hints.containsKey(awt.RenderingHints.KEY_INTERPOLATION)) {
					object value_i = hints.get(awt.RenderingHints.KEY_INTERPOLATION);

					if (value_i == awt.RenderingHints.VALUE_INTERPOLATION_BILINEAR)
						return InterpolationMode.Bilinear;
					if (value_i == awt.RenderingHints.VALUE_INTERPOLATION_BICUBIC)
						return InterpolationMode.Bicubic;
					if (value_i == awt.RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR)
						return InterpolationMode.NearestNeighbor;
				}

				return InterpolationMode.Default;
			}
			set {
				awt.RenderingHints hints = NativeObject.getRenderingHints();

				switch (value) {
					case InterpolationMode.Bicubic:
					case InterpolationMode.HighQualityBicubic:
					case InterpolationMode.Low:
						hints.put(awt.RenderingHints.KEY_INTERPOLATION, awt.RenderingHints.VALUE_INTERPOLATION_BICUBIC);
						break;
					case InterpolationMode.High:
					case InterpolationMode.Bilinear:
					case InterpolationMode.HighQualityBilinear:
						hints.put(awt.RenderingHints.KEY_INTERPOLATION, awt.RenderingHints.VALUE_INTERPOLATION_BILINEAR);
						break;
					case InterpolationMode.Default:
						if (hints.containsKey(awt.RenderingHints.KEY_INTERPOLATION))
							hints.remove(awt.RenderingHints.KEY_INTERPOLATION);
						break;
					case InterpolationMode.NearestNeighbor:
						hints.put(awt.RenderingHints.KEY_INTERPOLATION, awt.RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR);
						break;
					case InterpolationMode.Invalid:
						throw new ArgumentException();
					default:
						throw new ArgumentOutOfRangeException();
				}

				NativeObject.setRenderingHints(hints);
			}
		}

		public bool IsClipEmpty {
			get {
				return _clip.IsEmpty(this);
			}
		}

		public bool IsVisibleClipEmpty {
			get {
				if (_clip.IsEmpty(this))
					return true;

				return VisibleClipBounds.IsEmpty;
			}
		}

		public float PageScale {
			get {
				return _pageScale;
			}
			set {
				_pageScale = value;
			}
		}

		public GraphicsUnit PageUnit {
			get {
				return _pageUnit;
			}
			set {
				_pageUnit = value;
			}
		}

		static internal geom.AffineTransform GetFinalTransform(
			geom.AffineTransform transform, GraphicsUnit pageUnit, float pageScale) {
			geom.AffineTransform t = null;
			if (pageUnit != GraphicsUnit.Display) {
				float scale = pageScale * _unitConversion[ (int)pageUnit ];
				if (Math.Abs(scale-1f) > float.Epsilon)
					t = geom.AffineTransform.getScaleInstance(scale, scale);
			}

			if (t != null)
				t.concatenate(transform);
			else
				t = transform;
			
			return t;
		}

		geom.AffineTransform GetFinalTransform() {
			return GetFinalTransform(_transform.NativeObject, PageUnit, PageScale);
		}

		public PixelOffsetMode PixelOffsetMode {
			get {
				return _pixelOffsetMode;
			}
			set {
				_pixelOffsetMode = value;
			}
		}

		public Point RenderingOrigin {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public SmoothingMode SmoothingMode {
			get {
				awt.RenderingHints hints = NativeObject.getRenderingHints();
				if(hints.containsKey(awt.RenderingHints.KEY_ANTIALIASING)) {
					object value_aa = hints.get(awt.RenderingHints.KEY_ANTIALIASING);
					if (value_aa == awt.RenderingHints.VALUE_ANTIALIAS_ON) {
						if(hints.containsKey(awt.RenderingHints.KEY_RENDERING)) {
							object value_render = hints.get(awt.RenderingHints.KEY_RENDERING);
							if (value_render == awt.RenderingHints.VALUE_RENDER_QUALITY)
								return SmoothingMode.HighQuality;
							if (value_render == awt.RenderingHints.VALUE_RENDER_SPEED)
								return SmoothingMode.HighSpeed;
						}

						return SmoothingMode.AntiAlias;
					}

					if (value_aa == awt.RenderingHints.VALUE_ANTIALIAS_DEFAULT)
						return SmoothingMode.Default;
				}
				return SmoothingMode.None;

			}

			set {
				awt.RenderingHints hints = NativeObject.getRenderingHints();

				switch (value) {
					case SmoothingMode.None:
						if(hints.containsKey(awt.RenderingHints.KEY_ANTIALIASING))
							hints.remove(awt.RenderingHints.KEY_ANTIALIASING);
						if(hints.containsKey(awt.RenderingHints.KEY_RENDERING))
							hints.remove(awt.RenderingHints.KEY_RENDERING);
						break;
					case SmoothingMode.AntiAlias:
						hints.put(awt.RenderingHints.KEY_ANTIALIASING, awt.RenderingHints.VALUE_ANTIALIAS_ON);
						break;
					case SmoothingMode.HighQuality:
						hints.put(awt.RenderingHints.KEY_RENDERING, awt.RenderingHints.VALUE_RENDER_QUALITY);
						goto case SmoothingMode.AntiAlias;
					case SmoothingMode.HighSpeed:
						hints.put(awt.RenderingHints.KEY_RENDERING, awt.RenderingHints.VALUE_RENDER_SPEED);
						goto case SmoothingMode.None;
					case SmoothingMode.Default:
						hints.put(awt.RenderingHints.KEY_RENDERING, awt.RenderingHints.VALUE_RENDER_DEFAULT);
						goto case SmoothingMode.AntiAlias;
					case SmoothingMode.Invalid:
						throw new ArgumentException("Invalid parameter used.");
				}

				NativeObject.setRenderingHints(hints);
			}
		}

		/// <summary>
		/// Java does not have similar functionality
		/// </summary>
		public int TextContrast {
			get {
				return _textContrast;
			}

			set {
				_textContrast = value;
			}
		}

		public TextRenderingHint TextRenderingHint {
			get {
				awt.RenderingHints hints = NativeObject.getRenderingHints();
				if(hints.containsKey(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING)) {
					if(hints.get(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING) == 
						java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_ON)
						return TextRenderingHint.AntiAlias;
					if(hints.get(java.awt.RenderingHints.KEY_TEXT_ANTIALIASING) == 
						java.awt.RenderingHints.VALUE_TEXT_ANTIALIAS_OFF)
						return TextRenderingHint.SingleBitPerPixel;
				}
				return TextRenderingHint.SystemDefault;
			}

			set {
				// TODO implement
				awt.RenderingHints hints = NativeObject.getRenderingHints();
				switch (value) {
					case TextRenderingHint.AntiAlias:
					case TextRenderingHint.AntiAliasGridFit:
					case TextRenderingHint.ClearTypeGridFit:
					case TextRenderingHint.SingleBitPerPixel:
					case TextRenderingHint.SingleBitPerPixelGridFit:
					case TextRenderingHint.SystemDefault:
						break;
				}
			}
		}

		public Matrix Transform {
			get {
				return _transform.Clone();
			}
			set {
				if (value == null)
					throw new ArgumentNullException("matrix");

				if (!value.IsInvertible)
					throw new ArgumentException("Invalid parameter used.");

				value.CopyTo(_transform);
			}
		}

		internal Matrix BaseTransform {
			get {
				return new Matrix(NativeObject.getTransform());
			}
			set {
				NativeObject.setTransform(value.NativeObject);
			}
		}

		internal void PrependBaseTransform(geom.AffineTransform t) {
			NativeObject.transform(t);
		}

		internal awt.Shape VisibleShape {
			get {
				return _windowRect;
			}
		}

		public RectangleF VisibleClipBounds {
			get {
				if (_clip.IsEmpty(this))
					return RectangleF.Empty;

				geom.Rectangle2D r = _clip.NativeObject.getBounds2D();
				awt.Shape clip = NativeObject.getClip();
				geom.Rectangle2D clipBounds = (clip != null) ? clip.getBounds2D() : _windowRect;
				geom.Rectangle2D.intersect(r, clipBounds, r);
				geom.AffineTransform t = GetFinalTransform();
				if (!t.isIdentity()) {
					geom.AffineTransform it = t.createInverse();
					r = it.createTransformedShape(r).getBounds2D();
				}

				return new RectangleF (r);
			}
		}

		void ConcatenateTransform(geom.AffineTransform transform, MatrixOrder order) {
			geom.AffineTransform at = _transform.NativeObject;
			Matrix.Multiply(at, transform, order);
		}
		#endregion
	}
}

