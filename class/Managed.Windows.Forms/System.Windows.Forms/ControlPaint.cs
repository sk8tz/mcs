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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//
// $Revision: 1.1 $
// $Modtime: $
// $Log: ControlPaint.cs,v $
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Windows.Forms {
	public sealed class ControlPaint {
		#region Local Variables
		static int		RGBMax=255;
		static int		HLSMax=255;
		#endregion	// Local Variables

		#region Private Enumerations
		private enum DrawFrameControlTypes {
			Caption	= 1,
			Menu	= 2,
			Scroll	= 3,
			Button	= 4
		}

		private enum DrawFrameControlStates {
			ButtonCheck		= 0x0000,
			ButtonRadioImage	= 0x0001,
			ButtonRadioMask		= 0x0002,
			ButtonRadio		= 0x0004,
			Button3State		= 0x0008,
			ButtonPush		= 0x0010,

			CaptionClose		= 0x0000,
			CaptionMin		= 0x0001,
			CaptionMax		= 0x0002,
			CaptionRestore		= 0x0004,
			CaptionHelp		= 0x0008,

			MenuArrow		= 0x0000,
			MenuCheck		= 0x0001,
			MenuBullet		= 0x0002,
			MenuArrowRight		= 0x0004,

			ScrollUp		= 0x0000,
			ScrollDown		= 0x0001,
			ScrollLeft		= 0x0002,
			ScrollRight		= 0x0003,
			ScrollComboBox		= 0x0005,
			ScrollSizeGrip		= 0x0008,
			ScrollSizeGripRight	= 0x0010,

			Inactive		= 0x0100,
			Pushed			= 0x0200,
			Checked			= 0x0400,
			Transparent		= 0x0800,
			Hot			= 0x1000,
			AdjustRect		= 0x2000,
			Flat			= 0x4000,
			Mono			= 0x8000
			
		}
		#endregion	// Private Enumerations

		#region Helpers
		private static Color Win32ToColor(int Win32Color) {
			return(Color.FromArgb(
				(int)(Win32Color) & 0xff0000 >> 16,		// blue
				(int)(Win32Color) & 0xff00 >> 8,		// green
				(int)(Win32Color) & 0xff			// red
			));
		}

		internal static void Color2HBS(Color color, out int h, out int l, out int s) {
			int	r;
			int	g;
			int	b;
			int	cMax;
			int	cMin;
			int	rDelta;
			int	gDelta;
			int	bDelta;
	
			r=color.R;
			g=color.G;
			b=color.B;

			cMax = Math.Max(Math.Max(r, g), b);
			cMin = Math.Min(Math.Min(r, g), b);

			l = (((cMax+cMin)*HLSMax)+RGBMax)/(2*RGBMax);

			if (cMax==cMin) {		// Achromatic
				h=0;					// h undefined
				s=0;
				l=r;
				return;
			}

			/* saturation */
			if (l<=(HLSMax/2)) {
				s=(((cMax-cMin)*HLSMax)+((cMax+cMin)/2))/(cMax+cMin);
			} else {
				s=(((cMax-cMin)*HLSMax)+((2*RGBMax-cMax-cMin)/2))/(2*RGBMax-cMax-cMin);
			}

			/* hue */
			rDelta=(((cMax-r)*(HLSMax/6))+((cMax-cMin)/2))/(cMax-cMin);
			gDelta=(((cMax-g)*(HLSMax/6))+((cMax-cMin)/2))/(cMax-cMin);
			bDelta=(((cMax-b)*(HLSMax/6))+((cMax-cMin)/2))/(cMax-cMin);

			if (r == cMax) {
				h=bDelta - gDelta;
			} else if (g == cMax) {
				h=(HLSMax/3) + rDelta - bDelta;
			} else { /* B == cMax */
				h=((2*HLSMax)/3) + gDelta - rDelta;
			}

			if (h<0) {
				h+=HLSMax;
			}

			if (h>HLSMax) {
				h-=HLSMax;
			}
		}

		private static int HueToRGB(int n1, int n2, int hue) {
			if (hue<0) {
				hue+=HLSMax;
			}

			if (hue>HLSMax) {
				hue -= HLSMax;
			}

			/* return r,g, or b value from this tridrant */ 
			if (hue<(HLSMax/6)) {
				return(n1+(((n2-n1)*hue+(HLSMax/12))/(HLSMax/6)));
			}

			if (hue<(HLSMax/2)) {
				return(n2);
			}

			if (hue<((HLSMax*2)/3)) {
				return(n1+(((n2-n1)*(((HLSMax*2)/3)-hue)+(HLSMax/12))/(HLSMax/6)));
			} else {
				return(n1);
			}
		}

		private static Color HBS2Color(int hue, int lum, int sat) {
			int	R;
			int	G;
			int	B;
			int	Magic1;
			int	Magic2;

			if (sat == 0) {            /* Achromatic */ 
				R=G=B=(lum*RGBMax)/HLSMax;
				// FIXME : Should throw exception if hue!=0
			} else {
				if (lum<=(HLSMax/2)) {
					Magic2=(lum*(HLSMax+sat)+(HLSMax/2))/HLSMax;
				} else {
					Magic2=sat+lum-((sat*lum)+(HLSMax/2))/HLSMax;
				}
				Magic1=2*lum-Magic2;
				
				R = Math.Min(255, (HueToRGB(Magic1,Magic2,hue+(HLSMax/3))*RGBMax+(HLSMax/2))/HLSMax);
				G = Math.Min(255, (HueToRGB(Magic1,Magic2,hue)*RGBMax+(HLSMax/2))/HLSMax);
				B = Math.Min(255, (HueToRGB(Magic1,Magic2,hue-(HLSMax/3))*RGBMax+(HLSMax/2))/HLSMax);
			}
			return(Color.FromArgb(R, G, B));
		}
		#endregion	// Helpers

		#region Public Static Properties
		public static Color ContrastControlDark {
			get { return(SystemColors.ControlDark); }
		}
		#endregion	// Public Static Properties
		
		#region Public Static Methods
		public static IntPtr CreateHBitmap16Bit(Bitmap bitmap, Color background){
			throw new NotImplementedException ();
		}
		
		public static IntPtr CreateHBitmapColorMask(Bitmap bitmap, IntPtr monochromeMask){
			throw new NotImplementedException ();
		}
		
		public static IntPtr CreateHBitmapTransparencyMask(Bitmap bitmap){
			throw new NotImplementedException ();
		}

		public static Color Light(Color baseColor) {
			return Light( baseColor, 10.0f);
		}
		
		public static Color Light(Color baseColor,float percOfLightLight) {
			int H, I, S;

			ControlPaint.Color2HBS(baseColor, out H, out I, out S);
			int NewIntensity = Math.Min( 255, I + ((255*(int)percOfLightLight)/100));
			return ControlPaint.HBS2Color(H, NewIntensity, S);
		}

		public static Color LightLight(Color baseColor) {
			return Light( baseColor, 20.0f);
		}

		public static Color Dark(Color baseColor) {
			return Dark(baseColor, 10.0f);
		}
		
		public static Color Dark(Color baseColor,float percOfDarkDark) {
			int H, I, S;
			ControlPaint.Color2HBS(baseColor, out H, out I, out S);
			int NewIntensity = Math.Max(0, I - ((255*(int)percOfDarkDark) / 100));
			return ControlPaint.HBS2Color(H, NewIntensity, S);
		}
		
		public static Color DarkDark(Color baseColor) {
			return Dark(baseColor, 20.0f);
		}
		
		public static void DrawBorder(Graphics graphics, Rectangle bounds, Color color, ButtonBorderStyle style) {
			DrawBorder(graphics, bounds, color, 1, style, color, 1, style, color, 1, style, color, 1, style);
		}
		
		public static void DrawBorder( Graphics graphics, Rectangle bounds, Color leftColor, int leftWidth,
			ButtonBorderStyle leftStyle, Color topColor, int topWidth, ButtonBorderStyle topStyle,
			Color rightColor, int rightWidth, ButtonBorderStyle rightStyle, Color bottomColor, int bottomWidth,
			ButtonBorderStyle bottomStyle) {

			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Left, bounds.Bottom-1, leftWidth, leftColor, leftStyle, Border3DSide.Left);
			DrawBorderInternal(graphics, bounds.Left, bounds.Top, bounds.Right-1, bounds.Top, topWidth, topColor, topStyle, Border3DSide.Top);
			DrawBorderInternal(graphics, bounds.Right-1, bounds.Top, bounds.Right-1, bounds.Bottom-1, rightWidth, rightColor, rightStyle, Border3DSide.Right);
			DrawBorderInternal(graphics, bounds.Left, bounds.Bottom-1, bounds.Right-1, bounds.Bottom-1, bottomWidth, bottomColor, bottomStyle, Border3DSide.Bottom);
		}

		private static void DrawBorderInternal(Graphics graphics, int startX, int startY, int endX, int endY, 
			int width, Color color, ButtonBorderStyle style, Border3DSide side) {

			Pen	pen=new Pen(color, 1);

			switch(style) {
				case ButtonBorderStyle.Solid: {
					pen.DashStyle=DashStyle.Solid;
					break;
				}

				case ButtonBorderStyle.Dashed: {
					pen.DashStyle=DashStyle.Dash;
					break;
				}

				case ButtonBorderStyle.Dotted: {
					pen.DashStyle=DashStyle.Dot;
					break;
				}

				case ButtonBorderStyle.Inset: {
					pen.DashStyle=DashStyle.Solid;
					break;
				}

				case ButtonBorderStyle.Outset: {
					pen.DashStyle=DashStyle.Solid;
					break;
				}

				default:
				case ButtonBorderStyle.None: {
					pen.Dispose();
					return;
				}
			}


			switch(style) {
				case ButtonBorderStyle.Outset: {
					Color		colorGrade;
					int		hue, brightness, saturation;
					int		brightnessSteps;
					int		brightnessDownSteps;

					Color2HBS(color, out hue, out brightness, out saturation);

					brightnessDownSteps=brightness/width;
					if (brightness>127) {
						brightnessSteps=Math.Max(6, (160-brightness)/width);
					} else {
						brightnessSteps=(127-brightness)/width;
					}
					
					for (int i=0; i<width; i++) {
						switch(side) {
							case Border3DSide.Left:	{
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
								break;
							}
						
							case Border3DSide.Right: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
								break;
							}

							case Border3DSide.Top: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
								break;
							}
					
							case Border3DSide.Bottom: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
								break;
							}
						}
					}
					break;
				}

				case ButtonBorderStyle.Inset: {
					Color		colorGrade;
					int		hue, brightness, saturation;
					int		brightnessSteps;
					int		brightnessDownSteps;

					Color2HBS(color, out hue, out brightness, out saturation);

					brightnessDownSteps=brightness/width;
					if (brightness>127) {
						brightnessSteps=Math.Max(6, (160-brightness)/width);
					} else {
						brightnessSteps=(127-brightness)/width;
					}
					
					for (int i=0; i<width; i++) {
						switch(side) {
							case Border3DSide.Left:	{
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
								break;
							}
						
							case Border3DSide.Right: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
								break;
							}

							case Border3DSide.Top: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Max(0, brightness-brightnessDownSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
								break;
							}
					
							case Border3DSide.Bottom: {
								pen.Dispose();
								colorGrade=HBS2Color(hue, Math.Min(255, brightness+brightnessSteps*(width-i)), saturation);
								pen=new Pen(colorGrade, 1);
								graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
								break;
							}
						}
					}
					break;
				}

				/*
					I decided to have the for-loop duplicated for speed reasons; 
					that way we only have to switch once (as opposed to have the 
					for-loop around the switch)
				*/
				default: {
					switch(side) {
						case Border3DSide.Left:	{
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX+i, startY+i, endX+i, endY-i);
							}
							break;
						}
						
						case Border3DSide.Right: {
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX-i, startY+i, endX-i, endY-i);
							}
							break;
						}

						case Border3DSide.Top: {
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX+i, startY+i, endX-i, endY+i);
							}
							break;
						}
					
						case Border3DSide.Bottom: {
							for (int i=0; i<width; i++) {
								graphics.DrawLine(pen, startX+i, startY-i, endX-i, endY-i);
							}
							break;
						}
					}
					break;
				}
			}
			pen.Dispose();
		}
		
		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle) {
			DrawBorder3D(graphics, rectangle, Border3DStyle.Etched, Border3DSide.All);
		}
		
		public static void DrawBorder3D(Graphics graphics, Rectangle rectangle, Border3DStyle style) {
			DrawBorder3D(graphics, rectangle, style, Border3DSide.All);
		}
		
		public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height) {
			DrawBorder3D(graphics, new Rectangle(x, y, width, height), Border3DStyle.Etched, Border3DSide.All);
		}

		public static void DrawBorder3D(Graphics graphics, int x, int y, int width, int height, Border3DStyle style) {
			DrawBorder3D(graphics, new Rectangle(x, y, width, height), style, Border3DSide.All);
		}

		public static void DrawBorder3D( Graphics graphics, int x, int y, int width, int height, Border3DStyle style,Border3DSide sides) {
			DrawBorder3D( graphics, new Rectangle(x, y, width, height), style, sides);
		}

		public static void DrawBorder3D( Graphics graphics, Rectangle rectangle, Border3DStyle style, Border3DSide sides) {
			Pen			penTopLeft;
			Pen			penTopLeftInner;
			Pen			penBottomRight;
			Pen			penBottomRightInner;
			Rectangle	rect= new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			bool			doInner = false;

			if ((style & Border3DStyle.Adjust)!=0) {
				rect.Y-=2;
				rect.X-=2;
				rect.Width+=4;
				rect.Height+=4;
			}

			/* default to flat */
			penTopLeft=SystemPens.ControlDark;
			penTopLeftInner=SystemPens.ControlDark;
			penBottomRight=SystemPens.ControlDark;
			penBottomRightInner=SystemPens.ControlDark;

			if ((style & Border3DStyle.RaisedOuter)!=0) {
				penTopLeft=SystemPens.ControlLightLight;
				penBottomRight=SystemPens.ControlDarkDark;
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			} else if ((style & Border3DStyle.SunkenOuter)!=0) {
				penTopLeft=SystemPens.ControlDarkDark;
				penBottomRight=SystemPens.ControlLightLight;
				if ((style & (Border3DStyle.RaisedInner | Border3DStyle.SunkenInner))!=0) {
					doInner=true;
				}
			}

			if ((style & Border3DStyle.RaisedInner)!=0) {
				if (doInner) {
					penTopLeftInner=SystemPens.ControlLight;
					penBottomRightInner=SystemPens.ControlDark;
				} else {
					penTopLeft=SystemPens.ControlLightLight;
					penBottomRight=SystemPens.ControlDarkDark;
				}
			} else if ((style & Border3DStyle.SunkenInner)!=0) {
				if (doInner) {
					penTopLeftInner=SystemPens.ControlDark;
					penBottomRightInner=SystemPens.ControlLight;
				} else {
					penTopLeft=SystemPens.ControlDarkDark;
					penBottomRight=SystemPens.ControlLightLight;
				}
			}

			if ((sides & Border3DSide.Middle)!=0) {
				graphics.FillRectangle(SystemBrushes.Control, rect);
			}

			if ((sides & Border3DSide.Left)!=0) {
				graphics.DrawLine(penTopLeft, rect.Left, rect.Bottom-1, rect.Left, rect.Top);
				if (doInner) {
					graphics.DrawLine(penTopLeftInner, rect.Left+1, rect.Bottom-1, rect.Left+1, rect.Top);
				}
			}

			if ((sides & Border3DSide.Top)!=0) {
				graphics.DrawLine(penTopLeft, rect.Left, rect.Top, rect.Right-1, rect.Top);

				if (doInner) {
					if ((sides & Border3DSide.Left)!=0) {
						graphics.DrawLine(penTopLeftInner, rect.Left+1, rect.Top+1, rect.Right-1, rect.Top+1);
					} else {
						graphics.DrawLine(penTopLeftInner, rect.Left, rect.Top+1, rect.Right-1, rect.Top+1);
					}
				}
			}

			if ((sides & Border3DSide.Right)!=0) {
				graphics.DrawLine(penBottomRight, rect.Right-1, rect.Top, rect.Right-1, rect.Bottom-1);

				if (doInner) {
					if ((sides & Border3DSide.Top)!=0) {
						graphics.DrawLine(penBottomRightInner, rect.Right-2, rect.Top+1, rect.Right-2, rect.Bottom-1);
					} else {
						graphics.DrawLine(penBottomRightInner, rect.Right-2, rect.Top, rect.Right-2, rect.Bottom-1);
					}
				}
			}

			if ((sides & Border3DSide.Bottom)!=0) {
				int	left=rect.Left;

				if ((sides & Border3DSide.Left)!=0) {
					left+=1;
				}

				graphics.DrawLine(penBottomRight, rect.Left, rect.Bottom-1, rect.Right-1, rect.Bottom-1);

				if (doInner) {
					if ((sides & Border3DSide.Right)!=0) {
						graphics.DrawLine(penBottomRightInner, left, rect.Bottom-2, rect.Right-2, rect.Bottom-2);
					} else {
						graphics.DrawLine(penBottomRightInner, left, rect.Bottom-2, rect.Right-1, rect.Bottom-2);
					}
				}
			}
		}

		public static void DrawButton( Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawButton(graphics, new Rectangle(x, y, width, height), state);
		}

		public static void DrawButton( Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonPush;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}
			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);
		}

		/* 
			This function actually draws the various caption elements. 
			This way we can scale them nicely, no matter what size, and they
			still look like MS's scaled caption buttons. (as opposed to scaling a bitmap)
		*/

		private static void DrawCaptionHelper(Graphics graphics, Color color, Pen pen, int lineWidth, int shift, Rectangle captionRect, CaptionButton button) {
			switch(button) {
				case CaptionButton.Close: {
					pen.StartCap=LineCap.Triangle;
					pen.EndCap=LineCap.Triangle;
					if (lineWidth<2) {
						graphics.DrawLine(pen, captionRect.Left+2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
						graphics.DrawLine(pen, captionRect.Right-2*lineWidth+1+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+1+shift, captionRect.Bottom-2*lineWidth+shift);
					}

					graphics.DrawLine(pen, captionRect.Left+2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Right-2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
					graphics.DrawLine(pen, captionRect.Right-2*lineWidth+shift, captionRect.Top+2*lineWidth+shift, captionRect.Left+2*lineWidth+shift, captionRect.Bottom-2*lineWidth+shift);
					return;
				}

				case CaptionButton.Help: {
					StringFormat	sf = new StringFormat();
					SolidBrush		sb = new SolidBrush(color);
					Font				font = new Font("Microsoft Sans Serif", captionRect.Height, FontStyle.Bold, GraphicsUnit.Pixel);

					sf.Alignment=StringAlignment.Center;
					sf.LineAlignment=StringAlignment.Center;
					

					graphics.DrawString("?", font, sb, captionRect.X+captionRect.Width/2+shift, captionRect.Y+captionRect.Height/2+shift+lineWidth/2, sf);

					sf.Dispose();
					sb.Dispose();
					font.Dispose();

					return;
				}

				case CaptionButton.Maximize: {
					/* Top 'caption bar' line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+2*lineWidth+shift+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift+i);
					}

					/* Left side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
					}

					/* Right side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift+i, captionRect.Bottom-lineWidth+shift);
					}

					/* Bottom line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
					}
					return;
				}

				case CaptionButton.Minimize: {
					/* Bottom line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth+shift, captionRect.Bottom-lineWidth+shift-i);
					}
					return;
				}

				case CaptionButton.Restore: {
					/** First 'window' **/
					/* Top 'caption bar' line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift, captionRect.Top+2*lineWidth+shift-i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+2*lineWidth+shift-i);
					}

					/* Left side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+2*lineWidth+shift, captionRect.Left+3*lineWidth+shift+i, captionRect.Top+4*lineWidth+shift);
					}

					/* Right side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+2*lineWidth+shift, captionRect.Right-lineWidth-lineWidth/2+shift-i, captionRect.Top+5*lineWidth-lineWidth/2+shift);
					}

					/* Bottom line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i, captionRect.Right-lineWidth-lineWidth/2+shift, captionRect.Top+5*lineWidth-lineWidth/2+shift+1+i);
					}

					/** Second 'window' **/
					/* Top 'caption bar' line */
					for (int i=0; i<Math.Max(2, lineWidth); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Top+4*lineWidth+shift+1-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Top+4*lineWidth+shift+1-i);
					}

					/* Left side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift+i, captionRect.Top+4*lineWidth+shift+1, captionRect.Left+lineWidth+shift+i, captionRect.Bottom-lineWidth+shift);
					}

					/* Right side line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Top+4*lineWidth+shift+1, captionRect.Right-3*lineWidth-lineWidth/2+shift-i, captionRect.Bottom-lineWidth+shift);
					}

					/* Bottom line */
					for (int i=0; i<Math.Max(1, lineWidth/2); i++) {
						graphics.DrawLine(pen, captionRect.Left+lineWidth+shift, captionRect.Bottom-lineWidth+shift-i, captionRect.Right-3*lineWidth-lineWidth/2+shift, captionRect.Bottom-lineWidth+shift-i);
					}

					return;
				}

			}
		}

		public static void DrawCaptionButton(Graphics graphics, int x, int y, int width, int height, CaptionButton button, ButtonState state) {
			DrawCaptionButton(graphics, new Rectangle(x, y, width, height), button, state);
		}

		public static void DrawCaptionButton(Graphics graphics, Rectangle rectangle, CaptionButton button, ButtonState state) {
			Rectangle	captionRect;
			int			lineWidth;

			DrawButton(graphics, rectangle, state);

			if (rectangle.Width<rectangle.Height) {
				captionRect=new Rectangle(rectangle.X+1, rectangle.Y+rectangle.Height/2-rectangle.Width/2+1, rectangle.Width-4, rectangle.Width-4);
			} else {
				captionRect=new Rectangle(rectangle.X+rectangle.Width/2-rectangle.Height/2+1, rectangle.Y+1, rectangle.Height-4, rectangle.Height-4);
			}

			if ((state & ButtonState.Pushed)!=0) {
				captionRect=new Rectangle(rectangle.X+2, rectangle.Y+2, rectangle.Width-3, rectangle.Height-3);
			}

			/* Make sure we've got at least a line width of 1 */
			lineWidth=Math.Max(1, captionRect.Width/7);

			switch(button) {
				case CaptionButton.Close: {
					Pen	pen;

					if ((state & ButtonState.Inactive)!=0) {
						pen=new Pen(SystemColors.ControlLightLight, lineWidth);
						DrawCaptionHelper(graphics, SystemColors.ControlLightLight, pen, lineWidth, 1, captionRect, button);
						pen.Dispose();

						pen=new Pen(SystemColors.ControlDark, lineWidth);
						DrawCaptionHelper(graphics, SystemColors.ControlDark, pen, lineWidth, 0, captionRect, button);
						pen.Dispose();
						return;
					} else {
						pen=new Pen(SystemColors.ControlText, lineWidth);
						DrawCaptionHelper(graphics, SystemColors.ControlText, pen, lineWidth, 0, captionRect, button);
						pen.Dispose();
						return;
					}
				}

				case CaptionButton.Help:
				case CaptionButton.Maximize:
				case CaptionButton.Minimize:
				case CaptionButton.Restore: {
					if ((state & ButtonState.Inactive)!=0) {
						DrawCaptionHelper(graphics, SystemColors.ControlLightLight, SystemPens.ControlLightLight, lineWidth, 1, captionRect, button);

						DrawCaptionHelper(graphics, SystemColors.ControlDark, SystemPens.ControlDark, lineWidth, 0, captionRect, button);
						return;
					} else {
						DrawCaptionHelper(graphics, SystemColors.ControlText, SystemPens.ControlText, lineWidth, 0, captionRect, button);
						return;
					}
				}
			}
		}

		public static void DrawCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawCheckBox(graphics, new Rectangle(x, y, width, height), state);
		}
		
		public static void DrawCheckBox(Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonCheck;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}

			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);
		}
		
		public static void DrawComboButton(Graphics graphics, Rectangle rectangle, ButtonState state) {
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			if ((state & ButtonState.Checked)!=0) {
				HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.ControlLightLight);
				graphics.FillRectangle(hatchBrush,rectangle);
				hatchBrush.Dispose();
			}

			if ((state & ButtonState.Flat)!=0) {
				DrawBorder(graphics, rectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
			} else {
				if ((state & (ButtonState.Pushed | ButtonState.Checked))!=0) {
					DrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
				} else {
					DrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
				}
			}

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			rect.Y-=shiftY;
			centerY-=shiftY;
			P1=new Point(rect.Left, centerY);
			P2=new Point(rect.Right, centerY);
			P3=new Point(centerX, rect.Bottom);

			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;

			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				/* Move away from the shadow */
				P1.X-=1;		P1.Y-=1;
				P2.X-=1;		P2.Y-=1;
				P3.X-=1;		P3.Y-=1;

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;
				

				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}
		}
		
		public static void DrawComboButton(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawComboButton(graphics, new Rectangle(x, y, width, height), state);
		}
		
		public static void DrawContainerGrabHandle(Graphics graphics, Rectangle bounds) {
			SolidBrush	sb		= new SolidBrush(Color.White);
			Pen			pen	= new Pen(Color.Black, 1);
			Rectangle	rect	= new Rectangle(bounds.X, bounds.Y, bounds.Width-1, bounds.Height-1);	// Dunno why, but MS does it that way, too
			int			X;
			int			Y;

			graphics.FillRectangle(sb, rect);
			graphics.DrawRectangle(pen, rect);

			X=rect.X+rect.Width/2;
			Y=rect.Y+rect.Height/2;

			/* Draw the cross */
			graphics.DrawLine(pen, X, rect.Y+2, X, rect.Bottom-2);
			graphics.DrawLine(pen, rect.X+2, Y, rect.Right-2, Y);

			/* Draw 'arrows' for vertical lines */
			graphics.DrawLine(pen, X-1, rect.Y+3, X+1, rect.Y+3);
			graphics.DrawLine(pen, X-1, rect.Bottom-3, X+1, rect.Bottom-3);

			/* Draw 'arrows' for horizontal lines */
			graphics.DrawLine(pen, rect.X+3, Y-1, rect.X+3, Y+1);
			graphics.DrawLine(pen, rect.Right-3, Y-1, rect.Right-3, Y+1);
		}
		
		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle) {
			DrawFocusRectangle(graphics, rectangle, Color.White, Color.Black);
		}
		
		public static void DrawFocusRectangle( Graphics graphics, Rectangle rectangle, Color foreColor, Color backColor) {
			//Color			colorForeInverted;
			Color			colorBackInverted;
			Pen			pen;

			//colorForeInverted=Color.FromArgb(Math.Abs(foreColor.R-255), Math.Abs(foreColor.G-255), Math.Abs(foreColor.B-255));
			//pen=new Pen(colorForeInverted, 1);
			// MS seems to always use black
			pen=new Pen(Color.Black, 1);
			graphics.DrawRectangle(pen, rectangle);
			pen.Dispose();

			colorBackInverted=Color.FromArgb(Math.Abs(backColor.R-255), Math.Abs(backColor.G-255), Math.Abs(backColor.B-255));
			pen=new Pen(colorBackInverted, 1);
			pen.DashStyle=DashStyle.Dot;
			graphics.DrawRectangle(pen, rectangle);
			pen.Dispose();
		}
		
		public static void DrawGrabHandle(Graphics graphics, Rectangle rectangle, bool primary, bool enabled) {
			SolidBrush	sb;
			Pen			pen;

			if (primary==true) {
				pen=new Pen(Color.Black, 1);
				if (enabled==true) {
					sb=new SolidBrush(Color.White);
				} else {
					sb=new SolidBrush(SystemColors.Control);
				}
			} else {
				pen=new Pen(Color.White, 1);
				if (enabled==true) {
					sb=new SolidBrush(Color.Black);
				} else {
					sb=new SolidBrush(SystemColors.Control);
				}
			}
			graphics.FillRectangle(sb, rectangle);
			graphics.DrawRectangle(pen, rectangle);
			sb.Dispose();
			pen.Dispose();
		}
		
		public static void DrawGrid(Graphics graphics, Rectangle area, Size pixelsBetweenDots, Color backColor) {
			Color	foreColor;
			int	h;
			int	b;
			int	s;

			Color2HBS(backColor, out h, out b, out s);

			if (b>127) {
				foreColor=Color.Black;
			} else {
				foreColor=Color.White;
			}

#if false
			/* Commented out until I take the time and figure out 
				which HatchStyle will match requirements. The code below
				is only correct for Percent50.
			*/
			if (pixelsBetweenDots.Width==pixelsBetweenDots.Height) {
				HatchBrush	brush=null;

				switch(pixelsBetweenDots.Width) {
					case 2: brush=new HatchBrush(HatchStyle.Percent50, foreColor, backColor); break;
					case 4: brush=new HatchBrush(HatchStyle.Percent25, foreColor, backColor); break;
					case 5: brush=new HatchBrush(HatchStyle.Percent20, foreColor, backColor); break;
					default: {
						/* Have to do it the slow way */
						break;
					}
				}
				if (brush!=null) {
					graphics.FillRectangle(brush, area);
					pen.Dispose();
					brush.Dispose();
					return;
				}
			}
#endif
			/* Slow method */

			Bitmap bitmap = new Bitmap(area.Width, area.Height, graphics);

			for (int x=0; x<area.Width; x+=pixelsBetweenDots.Width) {
				for (int y=0; y<area.Height; y+=pixelsBetweenDots.Height) {
					bitmap.SetPixel(x, y, foreColor);
				}
			}
			graphics.DrawImage(bitmap, area.X, area.Y, area.Width, area.Height);
			bitmap.Dispose();
		}
		
		public static void DrawImageDisabled(Graphics graphics, Image image, int x, int y, Color background) {
			/*
				Microsoft seems to ignore the background and simply make 
				the image grayscale. At least when having > 256 colors on 
				the display.
			*/

			ImageAttributes	imageAttributes=new ImageAttributes();
			ColorMatrix			colorMatrix=new ColorMatrix(new float[][] {
// This table would create a perfect grayscale image, based on luminance
//				new float[]{0.3f,0.3f,0.3f,0,0},
//				new float[]{0.59f,0.59f,0.59f,0,0},
//				new float[]{0.11f,0.11f,0.11f,0,0},
//				new float[]{0,0,0,1,0,0},
//				new float[]{0,0,0,0,1,0},
//				new float[]{0,0,0,0,0,1}

// This table generates a image that is grayscaled and then 
// brightened up. Seems to match MS close enough.
				new float[]{0.2f,0.2f,0.2f,0,0},
				new float[]{0.41f,0.41f,0.41f,0,0},
				new float[]{0.11f,0.11f,0.11f,0,0},
				new float[]{0.15f,0.15f,0.15f,1,0,0},
				new float[]{0.15f,0.15f,0.15f,0,1,0},
				new float[]{0.15f,0.15f,0.15f,0,0,1}
			});
	
			imageAttributes.SetColorMatrix(colorMatrix);
			graphics.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
			imageAttributes.Dispose();
		}
		
		public static void DrawLockedFrame(Graphics graphics, Rectangle rectangle, bool primary) {
			Pen	penBorder;
			Pen	penInside;

			if (primary) {
				penBorder=new Pen(Color.White, 2);
				penInside=new Pen(Color.Black, 1);
			} else {
				penBorder=new Pen(Color.Black, 2);
				penInside=new Pen(Color.White, 1);
			}
			penBorder.Alignment=PenAlignment.Inset;
			penInside.Alignment=PenAlignment.Inset;

			graphics.DrawRectangle(penBorder, rectangle);
			graphics.DrawRectangle(penInside, rectangle.X+2, rectangle.Y+2, rectangle.Width-5, rectangle.Height-5);
			penBorder.Dispose();
			penInside.Dispose();
		}
		
		public static void DrawMenuGlyph(Graphics graphics, Rectangle rectangle, MenuGlyph glyph) {
			Rectangle	rect;
			int			lineWidth;

			// MS seems to draw the background white
			graphics.FillRectangle(new SolidBrush(Color.White), rectangle);

			switch(glyph) {
				case MenuGlyph.Arrow: {
					Point[]			arrow = new Point[3];
					Point				P1;
					Point				P2;
					Point				P3;
					int				centerX;
					int				centerY;
					int				shiftX;
					int				shiftY;

					rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
					centerX=rect.Left+rect.Width/2;
					centerY=rect.Top+rect.Height/2;
					shiftX=Math.Max(1, rect.Width/8);
					shiftY=Math.Max(1, rect.Height/8);

					rect.X-=shiftX;
					centerX-=shiftX;

					P1=new Point(centerX, rect.Top-1);
					P2=new Point(centerX, rect.Bottom);
					P3=new Point(rect.Right, centerY);

					arrow[0]=P1;
					arrow[1]=P2;
					arrow[2]=P3;

					graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);

					return;
				}

				case MenuGlyph.Bullet: {
					SolidBrush	sb;

					lineWidth=Math.Max(2, rectangle.Width/3);
					rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);

					sb=new SolidBrush(SystemColors.MenuText);
					graphics.FillEllipse(sb, rect);
					sb.Dispose();
					return;
				}

				case MenuGlyph.Checkmark: {
					int			Scale;

					lineWidth=Math.Max(2, rectangle.Width/6);
					Scale=Math.Max(1, rectangle.Width/12);

					rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);

					for (int i=0; i<lineWidth; i++) {
						graphics.DrawLine(SystemPens.MenuText, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
						graphics.DrawLine(SystemPens.MenuText, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
					}
					return;
				}
			}
		}
		
		public static void DrawMenuGlyph(Graphics graphics, int x, int y, int width, int height, MenuGlyph glyph) {
			DrawMenuGlyph(graphics, new Rectangle(x, y, width, height), glyph);
		}
		
		public static void DrawMixedCheckBox(Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawCheckBox(graphics, rectangle, state);
		}
		
		public static void DrawMixedCheckBox(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawMixedCheckBox(graphics, new Rectangle(x, y, width, height), state);
		}

		[MonoTODO("Finish drawing code for Caption, Menu and Scroll")]
		private static void DrawFrameControl(Graphics graphics, Rectangle rectangle, DrawFrameControlTypes Type, DrawFrameControlStates State) {
			switch(Type) {
				case DrawFrameControlTypes.Button: {
					if ((State & DrawFrameControlStates.ButtonPush)!=0) {
						/* Goes first, affects the background */
						if ((State & DrawFrameControlStates.Checked)!=0) {
							HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.ControlLightLight);
							graphics.FillRectangle(hatchBrush,rectangle);
							hatchBrush.Dispose();
						}

						if ((State & DrawFrameControlStates.Pushed)!=0) {
							DrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						} else if ((State & DrawFrameControlStates.Flat)!=0) {
							DrawBorder(graphics, rectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
						} else if ((State & DrawFrameControlStates.Inactive)!=0) {
							/* Same as normal, it would seem */
							DrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						} else {
							DrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						}
					} else if ((State & DrawFrameControlStates.ButtonRadio)!=0) {
						Pen			penFatDark	= new Pen(SystemColors.ControlDarkDark, 2);
						Pen			penFatLight	= new Pen(SystemColors.ControlLight, 2);
						int			lineWidth;

						graphics.DrawArc(penFatDark, rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 135, 180);
						graphics.DrawArc(penFatLight, rectangle.X+1, rectangle.Y+1, rectangle.Width-2, rectangle.Height-2, 315, 180);

						graphics.DrawArc(SystemPens.ControlDark, rectangle, 135, 180);
						graphics.DrawArc(SystemPens.ControlLightLight, rectangle, 315, 180);

						lineWidth=Math.Max(1, Math.Min(rectangle.Width, rectangle.Height)/3);

						if ((State & DrawFrameControlStates.Checked)!=0) {
							SolidBrush	buttonBrush;

							if ((State & DrawFrameControlStates.Inactive)!=0) {
								buttonBrush=(SolidBrush)SystemBrushes.ControlDark;
							} else {
								buttonBrush=(SolidBrush)SystemBrushes.ControlText;
							}
							graphics.FillPie(buttonBrush, rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2, 0, 359);
						}
						penFatDark.Dispose();
						penFatLight.Dispose();
					} else if ((State & DrawFrameControlStates.ButtonRadioImage)!=0) {
						throw new NotImplementedException () ;
					} else if ((State & DrawFrameControlStates.ButtonRadioMask)!=0) {
						throw new NotImplementedException ();
					} else {	/* Must be Checkbox */
						Pen			pen;
						int			lineWidth;
						Rectangle	rect;
						int			Scale;

						/* FIXME: I'm sure there's an easier way to calculate all this, but it should do for now */

						/* Goes first, affects the background */
						if ((State & DrawFrameControlStates.Pushed)!=0) {
							HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.ControlLightLight);
							graphics.FillRectangle(hatchBrush,rectangle);
							hatchBrush.Dispose();
						}

						/* Draw the sunken frame */
						if ((State & DrawFrameControlStates.Flat)!=0) {
							DrawBorder(graphics, rectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
						} else {
							DrawBorder3D(graphics, rectangle, Border3DStyle.Sunken, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
						}

						/* Make sure we've got at least a line width of 1 */
						lineWidth=Math.Max(3, rectangle.Width/6);
						Scale=Math.Max(1, rectangle.Width/12);

						rect=new Rectangle(rectangle.X+lineWidth, rectangle.Y+lineWidth, rectangle.Width-lineWidth*2, rectangle.Height-lineWidth*2);
						if ((State & DrawFrameControlStates.Inactive)!=0) {
							pen=SystemPens.ControlDark;
						} else {
							pen=SystemPens.ControlText;
						}

						if ((State & DrawFrameControlStates.Checked)!=0) {
							/* Need to draw a check-mark */
							for (int i=0; i<lineWidth; i++) {
								graphics.DrawLine(pen, rect.Left+lineWidth/2, rect.Top+lineWidth+i, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i);
								graphics.DrawLine(pen, rect.Left+lineWidth/2+2*Scale, rect.Top+lineWidth+2*Scale+i, rect.Left+lineWidth/2+6*Scale, rect.Top+lineWidth-2*Scale+i);
							}
							
						}
					}
					return;
				}

				case DrawFrameControlTypes.Caption: {
					// FIXME:
					break;
				}

				case DrawFrameControlTypes.Menu: {
					// FIXME:
					break;
				}

				case DrawFrameControlTypes.Scroll: {
					// FIXME:
					break;
				}
			}
		}
	
		public static void DrawRadioButton(Graphics graphics, int x, int y, int width, int height, ButtonState state) {
			DrawRadioButton(graphics, new Rectangle(x, y, width, height), state);
		}
		
		public static void DrawRadioButton(Graphics graphics, Rectangle rectangle, ButtonState state) {
			DrawFrameControlStates	dfcs=DrawFrameControlStates.ButtonRadio;

			if ((state & ButtonState.Pushed)!=0) {
				dfcs |= DrawFrameControlStates.Pushed;
			}

			if ((state & ButtonState.Checked)!=0) {
				dfcs |= DrawFrameControlStates.Checked;
			}

			if ((state & ButtonState.Flat)!=0) {
				dfcs |= DrawFrameControlStates.Flat;
			}

			if ((state & ButtonState.Inactive)!=0) {
				dfcs |= DrawFrameControlStates.Inactive;
			}
			DrawFrameControl(graphics, rectangle, DrawFrameControlTypes.Button, dfcs);
		}
		
		[MonoTODO]
		public static void DrawReversibleFrame(Rectangle rectangle, Color backColor, FrameStyle style) {
			//FIXME:
		}
		
		[MonoTODO]
		public static void DrawReversibleLine(Point start, Point end, Color backColor) {
			//FIXME:
		}

		[MonoTODO]
		public static void FillReversibleRectangle(Rectangle rectangle, Color backColor) {
			//FIXME:
		}
		
		
		public static void DrawScrollButton(Graphics graphics, Rectangle rectangle, ScrollButton button, ButtonState state) {
			Point[]			arrow = new Point[3];
			Point				P1;
			Point				P2;
			Point				P3;
			int				centerX;
			int				centerY;
			int				shiftX;
			int				shiftY;
			Rectangle		rect;

			if ((state & ButtonState.Checked)!=0) {
				HatchBrush	hatchBrush=new HatchBrush(HatchStyle.Percent50, SystemColors.ControlLight, SystemColors.ControlLightLight);
				graphics.FillRectangle(hatchBrush,rectangle);
				hatchBrush.Dispose();
			}

			if ((state & ButtonState.Flat)!=0) {
				DrawBorder(graphics, rectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
			} else {
				DrawBorder3D(graphics, rectangle, Border3DStyle.Raised, Border3DSide.Left | Border3DSide.Top | Border3DSide.Right | Border3DSide.Bottom);
			}

			rect=new Rectangle(rectangle.X+rectangle.Width/4, rectangle.Y+rectangle.Height/4, rectangle.Width/2, rectangle.Height/2);
			centerX=rect.Left+rect.Width/2;
			centerY=rect.Top+rect.Height/2;
			shiftX=Math.Max(1, rect.Width/8);
			shiftY=Math.Max(1, rect.Height/8);

			if ((state & ButtonState.Pushed)!=0) {
				shiftX++;
				shiftY++;
			}

			switch(button) {
				default:
				case ScrollButton.Down: {
					rect.Y-=shiftY;
					centerY-=shiftY;
					P1=new Point(rect.Left, centerY);
					P2=new Point(rect.Right, centerY);
					P3=new Point(centerX, rect.Bottom);
					break;
				}

				case ScrollButton.Up: {
					rect.Y+=shiftY;
					centerY+=shiftY;
					P1=new Point(rect.Left, centerY);
					P2=new Point(rect.Right, centerY);
					P3=new Point(centerX, rect.Top-1);
					break;
				}

				case ScrollButton.Left: {
					rect.X+=shiftX;
					centerX+=shiftX;
					P1=new Point(centerX, rect.Top-1);
					P2=new Point(centerX, rect.Bottom);
					P3=new Point(rect.Left, centerY);
					break;
				}

				case ScrollButton.Right: {
					rect.X-=shiftX;
					centerX-=shiftX;
					P1=new Point(centerX, rect.Top-1);
					P2=new Point(centerX, rect.Bottom);
					P3=new Point(rect.Right, centerY);
					break;
				}
			}
			arrow[0]=P1;
			arrow[1]=P2;
			arrow[2]=P3;

			/* Draw the arrow */
			if ((state & ButtonState.Inactive)!=0) {
				graphics.FillPolygon(SystemBrushes.ControlLightLight, arrow, FillMode.Winding);

				/* Move away from the shadow */
				P1.X-=1;		P1.Y-=1;
				P2.X-=1;		P2.Y-=1;
				P3.X-=1;		P3.Y-=1;

				arrow[0]=P1;
				arrow[1]=P2;
				arrow[2]=P3;
				
				graphics.FillPolygon(SystemBrushes.ControlDark, arrow, FillMode.Winding);
			} else {
				graphics.FillPolygon(SystemBrushes.ControlText, arrow, FillMode.Winding);
			}
		}
		
		public static void DrawScrollButton(Graphics graphics, int x, int y, int width, int height, ScrollButton button, ButtonState state) {
			DrawScrollButton(graphics, new Rectangle(x, y, width, height), button, state);
		}
		
		public static void DrawSelectionFrame(Graphics graphics, bool active, Rectangle outsideRect, Rectangle insideRect, Color backColor) {
			int					h;
			int					b;
			int					s;
			Color					foreColor;
			HatchBrush			brush;
			Color					transparent;
			GraphicsContainer	container;

			Color2HBS(backColor, out h, out b, out s);

			if (b>127) {
				foreColor=SystemColors.ControlDark;
			} else {
				foreColor=SystemColors.ControlLight;
			}
			transparent=Color.FromArgb(0, backColor);

			if (active==true) {
				brush=new HatchBrush(HatchStyle.LightUpwardDiagonal, foreColor, transparent);
			} else {
				brush=new HatchBrush(HatchStyle.Percent25, foreColor, transparent);
			}

			container=graphics.BeginContainer();
			graphics.ExcludeClip(insideRect);
			graphics.FillRectangle(brush, outsideRect);
			graphics.EndContainer(container);
			brush.Dispose();
		}
		
		public static void DrawSizeGrip(Graphics graphics, Color backColor, Rectangle bounds) {
			int	h;
			int	b;
			int	s;
			Pen	pen1;
			Pen	pen2;

			Color2HBS(backColor, out h, out b, out s);

			pen1=new Pen(HBS2Color(h, Math.Min(255, (b*166)/100), s), 1);
			pen2=new Pen(HBS2Color(h, (b*33)/100, s), 1);

			for (int i=0; i<bounds.Width; i+=4) {
				graphics.DrawLine(pen1, bounds.Left+i, bounds.Bottom, bounds.Right, bounds.Top+i);
				graphics.DrawLine(pen2, bounds.Left+i+1, bounds.Bottom, bounds.Right, bounds.Top+i+1);
				graphics.DrawLine(pen2, bounds.Left+i+2, bounds.Bottom, bounds.Right, bounds.Top+i+2);
			}
			pen1.Dispose();
			pen2.Dispose();
		}
		
		public static void DrawSizeGrip(Graphics graphics, Color backColor, int x, int y, int width, int height) {
			DrawSizeGrip(graphics, backColor, new Rectangle(x, y, width, height));
		}
		
		public static void DrawStringDisabled(Graphics graphics, string s, Font font, Color color, RectangleF layoutRectangle, StringFormat format) {
			SolidBrush	brush;

			brush=new SolidBrush(ControlPaint.Light(color, 25));

			layoutRectangle.Offset(1.0f, 1.0f);
			graphics.DrawString(s, font, brush, layoutRectangle, format);

			brush.Color=ControlPaint.Dark(color, 35);
			layoutRectangle.Offset(-1.0f, -1.0f);
			graphics.DrawString(s, font, brush, layoutRectangle, format);

			brush.Dispose();
		}
		#endregion	// Public Static Methods
	}
}
