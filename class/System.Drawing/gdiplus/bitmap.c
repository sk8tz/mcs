//
// bitmap.c
// 
// Copyright (c) 2003 Alexandre Pigolkine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Authors:
//   Alexandre Pigolkine(pigolkine@gmx.de)
//
#include <glib.h>
#include "gdip_main.h"
#include "gdip_win32.h"
#include <string.h>
#include <unistd.h>

#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>

void gdip_bitmap_init (gdip_bitmap_ptr bitmap)
{
	gdip_image_init (&bitmap->image);
	bitmap->image.type = imageBitmap;
	bitmap->cairo_format = 0;
	bitmap->data.Width = 0;
	bitmap->data.Height = 0;
	bitmap->data.Stride = 0;
	bitmap->data.PixelFormat = 0;
	bitmap->data.Scan0 = 0;
	bitmap->data.Reserved = 0;
	
	bitmap->hBitmapDC = 0;
	bitmap->hInitialBitmap = 0;
	bitmap->hBitmap = 0;
}

gdip_bitmap_ptr gdip_bitmap_new ()
{
	gdip_bitmap_ptr result = (gdip_bitmap_ptr)GdipAlloc(sizeof(gdip_bitmap));
	gdip_bitmap_init (result);
	return result;
}

/*
 * This should only be called from GdipDisposeImage, and it should *not* free
 * the structure, that one is freed by GdipDisposeImage
 */
void gdip_bitmap_dispose (gdip_bitmap_ptr bitmap)
{
	
}

void gdip_bitmap_fill_info_header (gdip_bitmap_ptr bitmap, PBITMAPINFOHEADER bmi)
{
	int  bitmapLen = bitmap->data.Stride * bitmap->data.Height;
	memset (bmi, 0, 40);
	bmi->biSize = sizeof (BITMAPINFOHEADER);
	bmi->biWidth = bitmap->data.Width;
	bmi->biHeight = -bitmap->data.Height;
	bmi->biPlanes = 1;
	bmi->biBitCount = bitmap->cairo_format == CAIRO_FORMAT_RGB24 ? 24 : 32;
	bmi->biCompression = BI_RGB;
	bmi->biSizeImage = bitmapLen; 
}

void gdip_bitmap_save_bmp (const char *name, gdip_bitmap_ptr bitmap)
{
	BITMAPFILEHEADER bmfh;
	BITMAPINFOHEADER bmi;
	int  bitmapLen = bitmap->data.Stride * bitmap->data.Height;
	FILE *fp;
	
	bmfh.bfReserved1 = bmfh.bfReserved2 = 0;
	bmfh.bfType = BFT_BITMAP;
	bmfh.bfOffBits = (14 + 40 + 0 * 4);
	bmfh.bfSize = (bmfh.bfOffBits + bitmapLen);
	fp = fopen (name, "w+b");
	fwrite (&bmfh, 1, sizeof (bmfh), fp);
	gdip_bitmap_fill_info_header (bitmap, &bmi);
	fwrite (&bmi, 1, sizeof (bmi), fp);
	fwrite (bitmap->data.Scan0, 1, bitmapLen, fp);
	fclose (fp);
}

void *gdip_bitmap_create_Win32_HDC (gdip_bitmap_ptr bitmap)
{
	void * result = 0;
	void * hdc = CreateCompatibleDC_pfn (0);
	void * hbitmap = 0, * holdbitmap = 0;
	void * hdcDesc = GetDC_pfn (0);
			
	hbitmap = CreateCompatibleBitmap_pfn (hdcDesc, bitmap->data.Width, bitmap->data.Height);
	if (hbitmap != 0) {
		BITMAPINFO	bmi;
		gdip_bitmap_fill_info_header (bitmap, &bmi.bmiHeader);
		//_saveBmp ("file1.bmp", bitmap);
		SetDIBits_pfn (hdc, hbitmap, 0, bitmap->data.Height, bitmap->data.Scan0, &bmi, 0);
		holdbitmap = SelectObject_pfn (hdc, hbitmap);
		bitmap->hBitmapDC = hdc;
		bitmap->hInitialBitmap = holdbitmap;
		bitmap->hBitmap = hbitmap;
		result = hdc;
	}
	else {
		DeleteDC_pfn (hdc);
	}
	ReleaseDC_pfn (0, hdcDesc);
	return result;
}

void gdip_bitmap_destroy_Win32_HDC (gdip_bitmap_ptr bitmap, void *hdc)
{
	if (bitmap->hBitmapDC == hdc) {
		
		BITMAPINFO	bmi;
		int res = 0;
		unsigned long *array, *end;
			
		SelectObject_pfn (bitmap->hBitmapDC, bitmap->hInitialBitmap);
			
		gdip_bitmap_fill_info_header (bitmap, &bmi.bmiHeader);
		res = GetDIBits_pfn (bitmap->hBitmapDC, bitmap->hBitmap, 0, bitmap->data.Height, bitmap->data.Scan0, &bmi, 0);
		if (bitmap->cairo_format == CAIRO_FORMAT_ARGB32) {
			array = bitmap->data.Scan0;
			end = array + (bmi.bmiHeader.biSizeImage >> 2);
			while (array < end) {
				*array |= 0xff000000;
				++array;
			}
		}
		//_saveBmp ("file2.bmp", bitmap);

		DeleteObject_pfn (bitmap->hBitmap);
		DeleteDC_pfn (bitmap->hBitmapDC);
		bitmap->hBitmapDC = 0;
		bitmap->hInitialBitmap = 0;
		bitmap->hBitmap = 0;
	}
}

Status GdipCreateBitmapFromScan0 (int width, int height, int stride, int format, void * scan0, gdip_bitmap_ptr *bitmap)
{
	gdip_bitmap_ptr result = 0;
	int cairo_format = 0;

	if (stride == 0)
		return InvalidParameter;
	if (scan0 == NULL)
		return InvalidParameter;
			
	switch (format) {
	case Format24bppRgb:
		cairo_format = CAIRO_FORMAT_RGB24;	
	break;
	case Format32bppArgb:
		cairo_format = CAIRO_FORMAT_ARGB32;	
	break;
	default:
		*bitmap = 0;
		return NotImplemented;
	}
	result = gdip_bitmap_new ();
	result->cairo_format = cairo_format;
	result->data.Width = width;
	result->data.Height = height;
	result->data.Stride = stride;
	result->data.PixelFormat = format;
	result->data.Scan0 = scan0;
	
	*bitmap = result;
	return Ok;
}

Status GdipCreateBitmapFromGraphics (int width, int height, gdip_graphics_ptr graphics, gdip_bitmap_ptr * bitmap)
{
	gdip_bitmap_ptr result = 0;
	int bmpSize = 0;
	int cairo_format = 0;
	int stride = width;

	/*
	 * FIXME: should get the stride based on the format of the graphics object.
	 */
	fprintf (stderr, "GdipCreateBitmapFromGraphics: This routine has not been checked for stride size\n");
	while (stride % 4)
		stride++;
	
	stride *= 4;
	cairo_format = CAIRO_FORMAT_ARGB32;	
	bmpSize = stride * height;
	result = gdip_bitmap_new ();
	result->cairo_format = cairo_format;
	result->data.Width = width;
	result->data.Height = height;
	result->data.Stride = stride;
	result->data.PixelFormat = Format32bppArgb;
	result->data.Scan0 = GdipAlloc (bmpSize);
	result->data.Reserved = 1;
	*bitmap = result;
	return Ok;
}

Status GdipBitmapLockBits (gdip_bitmap_ptr bitmap, Rect *rc, int flags, int format, GdipBitmapData *result)
{
	if (bitmap == 0){
		printf ("Bitmap is null\n");
		return InvalidParameter;
	}

	/* Special case: the entire image is requested */
	if (rc->left == 0 && rc->right == bitmap->data.Width &&
	    rc->top == 0 && rc->bottom == bitmap->data.Height &&
	    format == bitmap->data.PixelFormat){
		*result = bitmap->data;
		result->Reserved = result->Reserved & ~1;
		return Ok;
	}
	
	if (bitmap->data.PixelFormat != format){
		printf ("Requesting format change, not supported yet %d %d\n", bitmap->data.PixelFormat, format);
		return InvalidParameter;
	}
	
	result->Width = bitmap->data.Width; 
	result->Height = bitmap->data.Height; 
	result->Stride = bitmap->data.Stride; 
	result->PixelFormat = bitmap->data.PixelFormat; 
	result->Reserved = bitmap->data.Reserved; 
	result->Scan0 = bitmap->data.Scan0;

	return Ok;
}

Status ____BitmapLockBits (gdip_bitmap_ptr bitmap, Rect *rc, int flags, int format, int *width, int *height, int *stride, int *fptr, int *res, int *scan0)
{
	GdipBitmapData d;
	int s;
	
	s = GdipBitmapLockBits (bitmap, rc, flags, format, &d);
	*width = d.Width;
	*height = d.Height;
	*stride = d.Stride;
	*fptr = d.PixelFormat;
	*res = d.Reserved;
	*scan0 = d.Scan0;

	return s;
}

Status GdipBitmapUnlockBits (gdip_bitmap_ptr bitmap, GdipBitmapData *bitmap_data)
{
	if (bitmap_data->Reserved & 1)
		GdipFree (bitmap_data->Scan0);
	return Ok;
}

