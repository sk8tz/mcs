/*
 * graphics.c
 *
 * Copyright (c) 2003 Alexandre Pigolkine
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
 * NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Authors:
 *   Alexandre Pigolkine(pigolkine@gmx.de)
 *   Duncan Mak (duncan@ximian.com)
 *
 */

#include "gdip.h"
#include "gdip_win32.h"
#include <math.h>

void
gdip_graphics_init (GpGraphics *graphics)
{
	graphics->ct = cairo_create ();
	graphics->copy_of_ctm = cairo_matrix_create ();
	cairo_matrix_set_identity (graphics->copy_of_ctm);
	graphics->hdc = 0;
	graphics->hdc_busy_count = 0;
	graphics->image = 0;
	graphics->type = gtUndefined;
        /* cairo_select_font (graphics->ct, "serif:12"); */
	cairo_select_font (graphics->ct, "serif:12", CAIRO_FONT_SLANT_NORMAL, CAIRO_FONT_WEIGHT_NORMAL);
}

GpGraphics *
gdip_graphics_new ()
{
	GpGraphics *result = (GpGraphics *) GdipAlloc (sizeof (GpGraphics));
	gdip_graphics_init (result);
	return result;
}

void
gdip_graphics_attach_bitmap (GpGraphics *graphics, GpBitmap *image)
{
	cairo_set_target_image (graphics->ct, image->data.Scan0, image->cairo_format,
				image->data.Width, image->data.Height, image->data.Stride);
	graphics->image = image;
	graphics->type = gtMemoryBitmap;
}

void 
gdip_graphics_detach_bitmap (GpGraphics *graphics, GpBitmap *image)
{
	printf ("Implement graphics_detach_bitmap");
	/* FIXME: implement me */
}

#define C1 0.552

static void 
make_ellipse (GpGraphics *graphics, float x, float y, float width, float height)
{
        double rx = width / 2;
        double ry = height / 2;
        double cx = x + rx;
        double cy = y + ry;

        cairo_move_to (graphics->ct, cx + rx, cy);

        /* an approximate of the ellipse by drawing a curve in each
         * quartrant */
        cairo_curve_to (graphics->ct,
                        cx + rx, cy - C1 * ry,
                        cx + C1 * rx, cy - ry,
                        cx, cy - ry);
        cairo_curve_to (graphics->ct,
                        cx - C1 * rx, cy - ry,
                        cx - rx, cy - C1 * ry,
                        cx - rx, cy);
        cairo_curve_to (graphics->ct,
                        cx - rx, cy + C1 * ry,
                        cx - C1 * rx, cy + ry,
                        cx, cy + ry);
        cairo_curve_to (graphics->ct,
                        cx + C1 * rx, cy + ry,
                        cx + rx, cy + C1 * ry,
                        cx + rx, cy);

        cairo_close_path (graphics->ct);        
}

static void
make_polygon (GpGraphics *graphics, GpPointF *points, int count)
{
        int i;
        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count; i++)
                cairo_line_to (graphics->ct, points [i].X, points [i].Y);

        /*
         * Draw a line from the last point back to the first point if
         * they're not the same
         */
        if (points [0].X != points [count].X && points [0].Y != points [count].Y)
                cairo_line_to (graphics->ct, points [0].X, points [0].Y);

        cairo_close_path (graphics->ct);
}

static void
make_polygon_from_integers (
        GpGraphics *graphics, GpPoint *points, int count)
{
        int i;
        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count; i++)
                cairo_line_to (graphics->ct, points [i].X, points [i].Y);

        /*
         * Draw a line from the last point back to the first point if
         * they're not the same
         */
        if (points [0].X != points [count].X && points [0].Y != points [count].Y)
                cairo_line_to (graphics->ct, points [0].X, points [0].Y);

        cairo_close_path (graphics->ct);
}

static void
make_pie (GpGraphics *graphics, float x, float y, float width,
                float height, float startAngle, float sweepAngle)
{
        float ax, ay;           /* the first intersection */
        float bx, by;           /* the second intersection */
        float cx, cy;           /* center of the bounding rect */
        float f1, f2;           /* x coord. of foci */
        float c;                /* distance between center and focus */
        
        /*
         * we'll assume that we're working on a circle 
         * and transform back into an ellipse after the calculation
         */
        float radius = width / 2;
        float scale = height / width;

        ax = radius * cos (startAngle);
        ay = radius * sin (startAngle) * scale; 
        
        bx = radius * cos (startAngle + sweepAngle);        
        by = radius * sin (startAngle + sweepAngle) * scale;
        
        cx = x + (width / 2);
        cy = y + (height / 2);

        c = sqrt (-cy * cy + cx * cx);
        f1 = cx - c;
        f2 = cx + c;        

        cairo_move_to (graphics->ct, cx, cy);
        cairo_line_to (graphics->ct, ax, ay);

        cairo_curve_to (graphics->ct,
                        cx, f1, cx, f2, bx, by);

        cairo_line_to (graphics->ct, cx, cy);

        cairo_close_path (graphics->ct);
}

static cairo_fill_rule_t
convert_fill_mode (GpFillMode fill_mode)
{
        if (fill_mode == FillModeAlternate) 
                return CAIRO_FILL_RULE_EVEN_ODD;
        else
                return CAIRO_FILL_RULE_WINDING;
}


GpStatus 
GdipCreateFromHDC (int hDC, GpGraphics **graphics)
{
	DC* dc = _get_DC_by_HDC (hDC);
	
	/* printf ("GdipCreateFromHDC. in %d, DC %p\n", hDC, dc); */
	if (dc == 0) return NotImplemented;
	
	*graphics = gdip_graphics_new ();
	cairo_set_target_drawable ((*graphics)->ct, GDIP_display, dc->physDev->drawable);
	_release_hdc (hDC);
	(*graphics)->hdc = (void*)hDC;
	(*graphics)->type = gtX11Drawable;
	/* printf ("GdipCreateFromHDC. graphics %p, ct %p\n", (*graphics), (*graphics)->ct); */
	return Ok;
}

GpStatus 
GdipDeleteGraphics (GpGraphics *graphics)
{
	/* FIXME: attention to surface (image, etc.) */
	/* printf ("GdipDeleteGraphics. graphics %p\n", graphics); */
	cairo_matrix_destroy (graphics->copy_of_ctm);
	cairo_destroy (graphics->ct);
	GdipFree (graphics);
	return Ok;
}

GpStatus 
GdipGetDC (GpGraphics *graphics, int *hDC)
{
	if (graphics->hdc == 0) {
		if (graphics->image != 0) {
			/* Create DC */
			graphics->hdc = gdip_image_create_Win32_HDC (graphics->image);
			if (graphics->hdc != 0) {
				++graphics->hdc_busy_count;
			}
		}
	}
	*hDC = (int)graphics->hdc;
	return Ok;
}

GpStatus 
GdipReleaseDC (GpGraphics *graphics, int hDC)
{
	if (graphics->hdc != (void *)hDC) return InvalidParameter;
	if (graphics->hdc_busy_count > 0) {
		--graphics->hdc_busy_count;
		if (graphics->hdc_busy_count == 0) {
			/* Destroy DC */
			gdip_image_destroy_Win32_HDC (graphics->image, (void*)hDC);
			graphics->hdc = 0;
		}
	}
	return Ok;
}

#define MAX_GRAPHICS_STATE_STACK 100

GpState saved_stack [MAX_GRAPHICS_STATE_STACK];
int current_stack_pos = 0;

GpStatus 
GdipRestoreGraphics (GpGraphics *graphics, unsigned int graphicsState)
{
	if (graphicsState < MAX_GRAPHICS_STATE_STACK) {
		cairo_matrix_copy (graphics->copy_of_ctm, saved_stack[graphicsState].matrix);
		cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
	}
	else {
		return InvalidParameter;
	}
	return Ok;
}

GpStatus 
GdipSaveGraphics(GpGraphics *graphics, unsigned int *state)
{
	if (current_stack_pos < MAX_GRAPHICS_STATE_STACK) {
		saved_stack[current_stack_pos].matrix = cairo_matrix_create ();
		cairo_matrix_copy (saved_stack[current_stack_pos].matrix, graphics->copy_of_ctm);
		*state = current_stack_pos;
		++current_stack_pos;
	}
	else {
		return OutOfMemory;
	}
	return Ok;
}

#define PI 3.14159265358979323846
#define GRADTORAD PI / 180.0

GpStatus 
GdipRotateWorldTransform (GpGraphics *graphics, float angle, int order)
{
	cairo_matrix_t *mtx = cairo_matrix_create ();
	cairo_matrix_rotate (mtx, angle * GRADTORAD);
	cairo_matrix_multiply (graphics->copy_of_ctm, mtx, graphics->copy_of_ctm );
	cairo_matrix_destroy ( mtx);
	cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
	return Ok;
}

GpStatus 
GdipTranslateWorldTransform (GpGraphics *graphics, float dx, float dy, int order)
{
	/* FIXME: consider order here */
	cairo_matrix_translate (graphics->copy_of_ctm, dx, dy);
	cairo_set_matrix (graphics->ct, graphics->copy_of_ctm);
	return Ok;
}

/* XXX: TODO */
GpStatus
GdipDrawArc (GpGraphics *graphics, GpPen *pen, 
                float x, float y, float width, float height, 
                float startAngle, float sweepAngle)
{
        gdip_pen_setup (graphics, pen);

        return NotImplemented;
}

GpStatus
GdipDrawArcI (GpGraphics *graphics, GpPen *pen, 
                int x, int y, int width, int height, 
                int startAngle, int sweepAngle)
{
        gdip_pen_setup (graphics, pen);

        return NotImplemented;
}

GpStatus 
GdipDrawBezier (GpGraphics *graphics, GpPen *pen, 
                float x1, float y1, float x2, float y2,
                float x3, float y3, float x4, float y4)
{
        gdip_pen_setup (graphics, pen);        
        cairo_move_to (graphics->ct, x1, y1);
        cairo_curve_to (graphics->ct, x2, y2, x3, y3, x4, y4);
        cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus GdipDrawBezierI (GpGraphics *graphics, GpPen *pen, 
                int x1, int y1, int x2, int y2,
                int x3, int y3, int x4, int y4)
{
        return GdipDrawBezier (graphics, pen,
                        x1, y1, x2, y2, x3, y3, x4, y4);
}

GpStatus 
GdipDrawBeziers (GpGraphics *graphics, GpPen *pen,
                GpPointF *points, int count)
{
        int i, j, k;
        
        if (count == 0)
                return Ok;

        gdip_pen_setup (graphics, pen);
        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count; i += 3) {
                j = i + 1;
                k = i + 2;
                cairo_curve_to (graphics->ct,
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y,
                                points [k].X, points [k].Y);
        }

        cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipDrawBeziersI (GpGraphics *graphics, GpPen *pen,
                GpPoint *points, int count)
{
        int i, j, k;
        
        if (count == 0)
                return Ok;

        gdip_pen_setup (graphics, pen);
        cairo_move_to (graphics->ct, points [0].X, points [0].Y);

        for (i = 0; i < count; i += 3) {
                j = i + 1;
                k = i + 2;
                cairo_curve_to (graphics->ct,
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y,
                                points [k].X, points [k].Y);
        }

        cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus 
GdipDrawEllipse (GpGraphics *graphics, GpPen *pen, 
                float x, float y, float width, float height)
{
        gdip_pen_setup (graphics, pen);
        make_ellipse (graphics, x, y, width, height);
        cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipDrawEllipseI (GpGraphics *graphics, GpPen *pen,
                int x, int y, int width, int height)
{
        return GdipDrawEllipse (graphics, pen, x, y, width, height);
}

GpStatus
GdipDrawLine (GpGraphics *graphics, GpPen *pen,
                float x1, float y1, float x2, float y2)
{
	gdip_pen_setup (graphics, pen);

	cairo_move_to (graphics->ct, x1, y1);
	cairo_line_to (graphics->ct, x2, y2);

	cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus 
GdipDrawLineI (GpGraphics *graphics, GpPen *pen, 
                int x1, int y1, int x2, int y2)
{
        return GdipDrawLine (graphics, pen, x1, y1, x2, y2);
}

GpStatus 
GdipDrawLines (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count)
{
        GpStatus s;
        int i, j;

        for (i = 0; i < count - 1; i++) {
                j = i + 1;
                s = GdipDrawLine (graphics, pen, 
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y);

                if (s != Ok) return s;
        }

        return Ok;
}

GpStatus 
GdipDrawLinesI (GpGraphics *graphics, GpPen *pen,
                GpPoint *points, int count)
{
        GpStatus s;
        int i, j;

        for (i = 0; i < count - 1; i++) {
                j = i + 1;
                s = GdipDrawLineI (graphics, pen, 
                                points [i].X, points [i].Y,
                                points [j].X, points [j].Y);

                if (s != Ok) return s;
        }

        return Ok;
}

GpStatus
GdipDrawPie (GpGraphics *graphics, GpPen *pen, float x, float y, 
                float width, float height, float startAngle, float sweepAngle)
{
        gdip_pen_setup (graphics, pen);
        make_pie (graphics, x, y, width, height, startAngle, sweepAngle);
        cairo_stroke (graphics->ct);
        cairo_close_path (graphics->ct);
        
        return gdip_get_status (graphics->ct);
}

GpStatus
GdipDrawPieI (GpGraphics *graphics, GpPen *pen, int x, int y, 
                int width, int height, float startAngle, float sweepAngle)
{
        gdip_pen_setup (graphics, pen);
        make_pie (graphics, x, y, width, height, startAngle, sweepAngle);
        cairo_stroke (graphics->ct);
        cairo_close_path (graphics->ct);
        
        return gdip_get_status (graphics->ct);
}

GpStatus
GdipDrawPolygon (GpGraphics *graphics, GpPen *pen, GpPointF *points, int count)
{
        gdip_pen_setup (graphics, pen);
        make_polygon (graphics, points, count);
        cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipDrawPolygonI (GpGraphics *graphics, GpPen *pen, GpPoint *points, int count)
{
        gdip_pen_setup (graphics, pen);
        make_polygon_from_integers (graphics, points, count);
        cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipDrawRectangle (GpGraphics *graphics, GpPen *pen,
                float x, float y, float width, float height)
{
        gdip_pen_setup (graphics, pen);
        cairo_rectangle (graphics->ct, x, y, width, height);
        cairo_stroke (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipDrawRectangleI (GpGraphics *graphics, GpPen *pen,
                int x, int y, int width, int height)
{
        return GdipDrawRectangle (graphics, pen, x, y, width, height);
}

GpStatus
GdipFillEllipse (GpGraphics *graphics, GpBrush *brush,
                float x, float y, float width, float height)
{
        gdip_brush_setup (graphics, brush);
        make_ellipse (graphics, x, y, width, height);
        cairo_fill (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipFillEllipseI (GpGraphics *graphics, GpBrush *brush,
                int x, int y, int width, int height)
{
        return GdipFillEllipse (graphics, brush, x, y, width, height);
}

GpStatus 
GdipFillRectangle (GpGraphics *graphics, GpBrush *brush, 
                float x, float y, float width, float height)
{
	gdip_brush_setup (graphics, brush);
	cairo_rectangle (graphics->ct, x, y, width, height);
	cairo_fill (graphics->ct);
	return gdip_get_status (graphics->ct);
}

GpStatus
GdipFillPolygon (GpGraphics *graphics, GpBrush *brush, 
                GpPointF *points, int count, GpFillMode fillMode)
{
        gdip_brush_setup (graphics, brush);
        make_polygon (graphics, points, count);

        cairo_set_fill_rule (
                graphics->ct,
                convert_fill_mode (fillMode));

        cairo_fill (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipFillPolygonI (GpGraphics *graphics, GpBrush *brush, 
                GpPoint *points, int count, GpFillMode fillMode)
{
        gdip_brush_setup (graphics, brush);
        make_polygon_from_integers (graphics, points, count);

        cairo_set_fill_rule (
                graphics->ct,
                convert_fill_mode (fillMode));
        
        cairo_fill (graphics->ct);

        return gdip_get_status (graphics->ct);
}

GpStatus
GdipFillPolygon2 (GpGraphics *graphics, GpBrush *brush, GpPointF *points, int count)
{
        return GdipFillPolygon (graphics, brush, points, count, FillModeAlternate);
}

GpStatus
GdipFillPolygon2I (GpGraphics *graphics, GpBrush *brush, GpPoint *points, int count)
{
        return GdipFillPolygonI (graphics, brush, points, count, FillModeAlternate);
}

GpStatus 
GdipDrawString (GpGraphics *graphics, const char *string,
                int len, void *font, RectF *rc, void *format, GpBrush *brush)
{
	cairo_save (graphics->ct);
	if (brush) {
		gdip_brush_setup (graphics, brush);
	} else {
		cairo_set_rgb_color (graphics->ct, 0., 0., 0.);
	}
	cairo_move_to (graphics->ct, rc->left, rc->top + 12);
	cairo_scale_font (graphics->ct, 12);
	cairo_show_text (graphics->ct, string);
	cairo_restore(graphics->ct);

	return gdip_get_status (graphics->ct);
}

GpStatus 
GdipSetRenderingOrigin (GpGraphics *graphics, int x, int y)
{
        cairo_move_to (graphics->ct, x, y);
        cairo_close_path (graphics->ct);

        return gdip_get_status (graphics->ct);
}

/*
 * FIXME: cairo_current_point does not reflect changes made from
 * cairo_move_to.
 */
GpStatus 
GdipGetRenderingOrigin (GpGraphics *graphics, int *x, int *y)
{
        double cx, cy;
        cairo_current_point (graphics->ct, &cx, &cy);

        *x = (int) cx;
        *y = (int) cy;

        return gdip_get_status (graphics->ct);
}

