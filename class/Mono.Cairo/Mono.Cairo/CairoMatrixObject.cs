//
// Mono.Cairo.CairoMatrixObject.cs
//
// Author: Duncan Mak
//
// (C) Ximian Inc, 2003.
//
// This is an OO wrapper API for the Cairo API
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Mono.Cairo;

namespace Mono.Cairo {

	public class CairoMatrixObject
        {
                IntPtr matrix;

                public CairoMatrixObject ()
                        : this (Create ())
                {                        
                }

                internal CairoMatrixObject (IntPtr ptr)
                {
                        matrix = ptr;
                }

                public static IntPtr Create ()
                {
                        return Cairo.cairo_matrix_create ();
                }

                public void Destroy ()
                {
                        Cairo.cairo_matrix_destroy (matrix);
                }

                public Cairo.Status Copy (out CairoMatrixObject other)
                {
                        IntPtr p = IntPtr.Zero;
                        
                        Cairo.Status status = Cairo.cairo_matrix_copy (matrix, out p);

                        other = new CairoMatrixObject (p);

                        return status;
                }

                public IntPtr Pointer {
                        get { return matrix; }
                }

                public Cairo.Status SetIdentity ()
                {
                        return Cairo.cairo_matrix_set_identity (matrix);
                }

                public Cairo.Status SetAffine (
                        double a, double b, double c, double d, double tx, double ty)
                {
                        return Cairo.cairo_matrix_set_affine (
                                matrix, a, b, c, d, tx, ty);
                }
                
                public Cairo.Status GetAffine (
                        out double a, out double b, out double c, out double d, out double tx, out double ty)
                {
                        return Cairo.cairo_matrix_get_affine (
                                matrix, out a, out b, out c, out d, out tx, out ty);
                }

                public Cairo.Status Scale (double sx, double sy)
                {
                        return Cairo.cairo_matrix_scale (matrix, sx, sy);
                }

                public Cairo.Status Rotate (double radians)
                {
                        return Cairo.cairo_matrix_rotate (matrix, radians);
                }

                public Cairo.Status Invert ()
                {
                        return Cairo.cairo_matrix_invert (matrix);
                }

                public static Cairo.Status Multiply (
                        out CairoMatrixObject result,
                        CairoMatrixObject a, CairoMatrixObject b)
                {
                        IntPtr p = IntPtr.Zero;
                        
                        Cairo.Status status = Cairo.cairo_matrix_multiply (
                                out p, a.Pointer, b.Pointer);

                        result = new CairoMatrixObject (p);

                        return status;
                }

                public Cairo.Status TransformDistance (ref double dx, ref double dy)
                {
                        return Cairo.cairo_matrix_transform_distance (
                                matrix, ref dx, ref dy);
                }

                public Cairo.Status TransformPoint (ref double x, ref double y)
                {
                        return Cairo.cairo_matrix_transform_distance (
                                matrix, ref x, ref y);
                }
        }
}
