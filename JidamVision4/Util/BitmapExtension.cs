using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace JidamVision4.Util
{
    public static class BitmapExtension
    {
        public static Tuple<IntPtr, int> ToBufferAndStride(this Bitmap bitmap)
        {
            BitmapData bitmapData = null;

            try
            {
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);

                return new Tuple<IntPtr, int>(bitmapData.Scan0, bitmapData.Stride);
            }
            finally
            {
                if (bitmapData != null)
                    bitmap.UnlockBits(bitmapData);
            }
        }
        public static void Split(this Bitmap bitmap, byte[] r, byte[] g, byte[] b, byte[] gray)
        {
            lock (bitmap)
            {
                Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
                if (mat == null)
                    return;

                var rgbs = mat.Split();
                Marshal.Copy(rgbs[0].Data, b, 0, (int)rgbs[0].Total());
                Marshal.Copy(rgbs[1].Data, g, 0, (int)rgbs[1].Total());
                Marshal.Copy(rgbs[2].Data, r, 0, (int)rgbs[2].Total());

                var mExGray = rgbs[0] / 3 + rgbs[1] / 3 + rgbs[2] / 3;
                var mGray = mExGray.ToMat();
                Marshal.Copy(mGray.Data, gray, 0, (int)mGray.Total());

                foreach (var splitted in rgbs)
                {
                    splitted.Dispose();
                }

                mat.Dispose();
            }
        }
    }
}
