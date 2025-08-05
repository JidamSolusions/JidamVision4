using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using JidamVision4.Util;

namespace JidamVision4.Core
{
    public enum eImageChannel : int
    {
        None = -1,
        Color,
        Gray,
        Red,
        Green,
        Blue,
        ChannelCount,
    }

    //검사와 관련된 이미지 버퍼를 관리하는 클래스
    public class ImageSpace : IDisposable
    {
        private class ImageInfo : IDisposable
        {
            private Bitmap _bitmap;
            private Bitmap _tempBitmap;

            private PixelFormat Format { get; set; }

            public byte[] ImageData { get; set; }

            public int PixelBpp { get; set; }

            public int Width { get; set; }

            public int Height { get; set; }

            public SizeF PixelResolution { get; set; }

            public IntPtr Buffer { get; set; }

            public GCHandle Handle { get; set; }

            public int Stride { get; set; }

            public Bitmap ToBitmap()
            {
                if (_bitmap == null)
                {
                    _bitmap = new Bitmap(Width, Height,
                        (PixelBpp == 8 ?
                        System.Drawing.Imaging.PixelFormat.Format8bppIndexed :
                        System.Drawing.Imaging.PixelFormat.Format24bppRgb));

                    Format = _bitmap.PixelFormat;
                    Width = _bitmap.Width;
                    Height = _bitmap.Height;

                    Handle = GCHandle.Alloc(ImageData, GCHandleType.Pinned);
                    IntPtr pointer = Handle.AddrOfPinnedObject();

                    var bufferAndStride = _bitmap.ToBufferAndStride();
                    Buffer = pointer;
                    Stride = bufferAndStride.Item2;
                }

                if (_tempBitmap != null)
                    _tempBitmap = null;

                _tempBitmap = new Bitmap(Width, Height, Stride, Format, Buffer);

                if (_tempBitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                {
                    ColorPalette pal = _tempBitmap.Palette;
                    // Generate grayscale colours:
                    for (Int32 i = 0; i < 256; i++)
                        pal.Entries[i] = Color.FromArgb(i, i, i);
                    // Assign the edited palette to the bitmap.
                    _tempBitmap.Palette = pal;
                }
                return _tempBitmap;
            }

            public Mat ToMat()
            {
                MatType matType = PixelBpp == 8 ? MatType.CV_8UC1 : MatType.CV_8UC3;
                Mat mat = Mat.FromPixelData(Height, Width, matType, ImageData);

                return mat;
            }


            #region Disposable

            private bool disposed = false; // to detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        if (ImageData != null)
                            ImageData = null;

                        // Dispose managed resources.
                    }

                    // Dispose unmanaged managed resources.

                    disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            #endregion //Disposable
        }

        public class ImagePtr
        {
            public IntPtr Ptr { get; set; }
            public long Length { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int Step { get; set; }
            public int Bpp { get; set; }

            public ImagePtr(IntPtr ptr, long length, int width, int height, int step, int bpp)
            {
                Ptr = ptr;
                Length = length;
                Width = width;
                Height = height;
                Step = step;
                Bpp = bpp;
            }

            public Mat ToMat()
            {
                var type = Bpp == 1 ? MatType.CV_8UC1 : MatType.CV_8UC3;
                //return new Mat(Height, Width, type, Ptr);
                return Mat.FromPixelData(Height, Width, type, Ptr);
            }

            private static readonly ImagePtr _zero = new ImagePtr(IntPtr.Zero, 0, 0, 0, 0, 0);
            public static ImagePtr Zero => _zero;
        }

        private ImageInfo _inspectionImage = new ImageInfo();
        private Dictionary<int, ImageInfo> _imageInfo = new Dictionary<int, ImageInfo>();
        private Dictionary<int, Dictionary<eImageChannel, ImageInfo>> _imageByChannel = new Dictionary<int, Dictionary<eImageChannel, ImageInfo>>();

        protected byte[] _ImageData;

        public virtual byte[] ImageData
        {
            get
            {
                return _ImageData;
            }
            set
            {
                _ImageData = value;
            }
        }
        public bool UseImageSplit { get; set; } = true;
        public int BufferCount { get; set; } = 0;

        public ImageSpace()
        {
        }

        public void SetImageInfo(int inspectionPixelBpp, int inspectionWidth, int inspectionHeight, int inspectionStride)
        {
            _inspectionImage.PixelBpp = inspectionPixelBpp;
            _inspectionImage.Width = inspectionWidth;
            _inspectionImage.Height = inspectionHeight;
            _inspectionImage.Stride = inspectionStride;
        }

        public void InitImageSpace(int bufferCount)
        {
            if (_inspectionImage.Width == 0 || _inspectionImage.Height == 0)
                return;

            Dispose();

            _imageByChannel.Clear();
            _imageInfo.Clear();

            Func<int, ImageInfo> newImageInfo = (x) =>
            {
                var imageInfo = new ImageInfo();
                imageInfo.PixelBpp = x;
                imageInfo.Width = _inspectionImage.Width;
                imageInfo.Height = _inspectionImage.Height;

                int bpp = 1;
                if (imageInfo.PixelBpp == 24)
                    bpp = 3;

                imageInfo.Stride = imageInfo.Width * bpp;
                imageInfo.PixelResolution = _inspectionImage.PixelResolution;
                imageInfo.ImageData = new byte[imageInfo.Stride * imageInfo.Height];

                GCHandle Handle = GCHandle.Alloc(imageInfo.ImageData, GCHandleType.Pinned);
                imageInfo.Buffer = Handle.AddrOfPinnedObject();
                imageInfo.Handle = Handle;

                return imageInfo;
            };

            for (int i = 0; i < bufferCount; ++i)
            {
                #region Origin Image buffer Set

                if (_imageInfo.ContainsKey(i) == true)
                    continue;

                var imageInfo = newImageInfo(_inspectionImage.PixelBpp);
                _imageInfo.Add(i, imageInfo);

                if (_inspectionImage.PixelBpp == 24)
                {
                    Dictionary<eImageChannel, ImageInfo> imageByChannel = new Dictionary<eImageChannel, ImageInfo>
                    {
                        { eImageChannel.Color, imageInfo },
                        { eImageChannel.Red, newImageInfo(8) },
                        { eImageChannel.Green, newImageInfo(8) },
                        { eImageChannel.Blue, newImageInfo(8) },
                        { eImageChannel.Gray, newImageInfo(8) }
                    };

                    _imageByChannel.Add(i, imageByChannel);
                }

                #endregion Origin Image Buffer Set
            }

            BufferCount = bufferCount;
        }

        #region Property
        public byte[] this[int i]
        {
            get
            {
                return _imageInfo[i].ImageData;
            }
            set
            {
                value.CopyTo(_imageInfo[i].ImageData, 0);
            }
        }

        public System.Drawing.Size ImageSize
        {
            get => new System.Drawing.Size(_inspectionImage.Width, _inspectionImage.Height);
        }

        public int PixelBpp
        {
            get => _inspectionImage.PixelBpp;
        }

        public SizeF PixelResolution
        {
            get => _inspectionImage.PixelResolution;
        }

        public int Width
        {
            get => _inspectionImage.Width;
        }

        public int Height
        {
            get => _inspectionImage.Height;
        }

        public int Stride
        {
            get => _inspectionImage.Stride;
        }

        public long Length
        {
            get => _inspectionImage.Stride * _inspectionImage.Height;
        }

        #endregion //Properties

        public Dictionary<int, Dictionary<eImageChannel, ImagePtr>> GetImageByChannelToClone()
        {
            if (_inspectionImage.PixelBpp == 8)
            {
                return _imageInfo.ToDictionary(kvp => kvp.Key,
                    kvp => new Dictionary<eImageChannel, ImagePtr>{{ eImageChannel.Gray,
                            new ImagePtr(
                                new IntPtr(kvp.Value.Buffer.ToInt64()),
                                kvp.Value.ImageData.Length,
                                kvp.Value.Width,
                                kvp.Value.Height,
                                kvp.Value.Stride,
                                (int)kvp.Value.PixelBpp) }
                    });
            }
            else
            {
                return _imageByChannel.ToDictionary(x => x.Key, y => y.Value.ToDictionary(
                k => k.Key, z => new ImagePtr(new IntPtr(z.Value.Buffer.ToInt64()), z.Value.ImageData.Length, z.Value.Width, z.Value.Height, z.Value.Stride, (int)z.Value.PixelBpp)));
            }
        }

        public Dictionary<int, Dictionary<eImageChannel, ImagePtr>> GetImageByChannel()
        {
            if (_inspectionImage.PixelBpp == 8)
            {
                return _imageInfo.ToDictionary(kvp => kvp.Key,
                    kvp => new Dictionary<eImageChannel, ImagePtr>{{ eImageChannel.Gray,
                            new ImagePtr(
                                kvp.Value.Buffer,
                                kvp.Value.ImageData.Length,
                                kvp.Value.Width,
                                kvp.Value.Height,
                                kvp.Value.Stride,
                                (int)kvp.Value.PixelBpp) }
                    });
            }
            else
            {
                return _imageByChannel.ToDictionary(x => x.Key, y => y.Value.ToDictionary(
                k => k.Key, z => new ImagePtr(z.Value.Buffer, z.Value.ImageData.Length, z.Value.Width, z.Value.Height, z.Value.Stride, (int)z.Value.PixelBpp)));
            }
        }

        public byte[] GetInspectionBuffer(int index = 0)
        {
            if (_imageInfo.Count <= index)
                return null;

            return _imageInfo[index].ImageData;
        }

        public IntPtr GetnspectionBufferPtr(int index = 0)
        {
            if (_imageInfo.Count <= index)
                return IntPtr.Zero;

            return _imageInfo[index].Buffer;
        }

        public GCHandle GetInspectionBufferHandle(int index = 0)
        {
            if (_imageInfo.Count <= index)
                return new GCHandle();

            return _imageInfo[index].Handle;
        }

        public Bitmap GetBitmap(int index = 0, eImageChannel channel = eImageChannel.Color)
        {
            if (index < 0 || _imageInfo.Count <= index)
                return null;

            if (PixelBpp == 8 || channel == eImageChannel.Color)
            {
                return _imageInfo[index].ToBitmap();
            }
            else
            {
                return _imageByChannel[index][channel].ToBitmap();
            }
        }

        public Mat GetMat(int index = 0, eImageChannel channel = eImageChannel.Gray)
        {
            if (_imageInfo.Count <= index)
                return null;

            if (channel == eImageChannel.Color)
            {
                return _imageInfo[index].ToMat();
            }
            else
            {
                if (_imageByChannel[index][channel] != null)
                    return _imageByChannel[index][channel].ToMat();
            }

            return null;
        }

        public Dictionary<int, Bitmap> GetBitmaps()
        {
            Dictionary<int, Bitmap> listBitmap = new Dictionary<int, Bitmap>();
            if (PixelBpp == 8)
            {
                foreach (var light in _imageInfo)
                {
                    listBitmap.Add(light.Key,
                        (Bitmap)(light.Value.ToBitmap().Clone()));
                }
            }
            else
            {
                foreach (var light in _imageByChannel)
                {
                    listBitmap.Add(light.Key,
                        (Bitmap)(light.Value[eImageChannel.Color].ToBitmap().Clone()));
                }
            }

            return listBitmap;
        }

        public void Split(int index)
        {
            if (!UseImageSplit)
                return;

            if (_imageInfo.Count <= index)
                return;

            if (PixelBpp == 8)
                return;

            var original = _imageInfo[index].ToBitmap();

            original.Split(_imageByChannel[index][eImageChannel.Red].ImageData,
                _imageByChannel[index][eImageChannel.Green].ImageData,
                _imageByChannel[index][eImageChannel.Blue].ImageData,
                _imageByChannel[index][eImageChannel.Gray].ImageData);
        }

        #region Disposable

        private bool disposed = false; // to detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_inspectionImage != null)
                    {
                        _inspectionImage.Dispose();
                    }

                    if (_imageInfo != null)
                    {
                        foreach (var image in _imageInfo)
                        {
                            image.Value.Dispose();
                        }
                    }

                    if (_imageByChannel != null)
                    {
                        foreach (var image in _imageByChannel)
                        {
                            foreach (var innerImage in image.Value)
                            {
                                innerImage.Value.Dispose();
                            }
                        }
                    }

                    // Dispose managed resources.
                }

                // Dispose unmanaged managed resources.

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion //Disposable
    }
}
