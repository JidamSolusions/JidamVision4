using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision4.Util
{
    public class ImageLoader : IDisposable
    {
        private List<string> _sortedImages;
        private int _grabIndex = -1;

        public bool CyclicMode { get; set; } = true;

        public ImageLoader() { }

        public bool LoadImages(string imageDir)
        {
            if (!Directory.Exists(imageDir))
                return false;

            _sortedImages = ImageFileSorter.GetSortedImages(imageDir);
            if (_sortedImages.Count() <= 0)
                return false;

            _grabIndex = -1;

            return true;
        }

        public bool IsLoadedImages()
        {
            if (_sortedImages is null)
                return false;

            if (_sortedImages.Count() <= 0)
                return false;

            return true;
        }

        public bool Reset()
        {
            _grabIndex = -1;
            return true;
        }

        public string GetImagePath()
        {
            if (_sortedImages is null)
                return "";

            _grabIndex++;

            if (_grabIndex >= _sortedImages.Count)
            {
                if (CyclicMode == false)
                    return "";

                _grabIndex = 0;
            }

            return _sortedImages[_grabIndex];
        }

        public string GetNextImagePath(bool reset = false)
        {
            if (reset)
                Reset();

            return GetImagePath();
        }


        #region Dispose

        private bool _disposed = false; // to detect redundant calls

        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposed = true;
        }
        ~ImageLoader()
        {
            Dispose(disposing: false);
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
