using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace JidamVision4.UIControl
{
    public enum PatternImageButton
    {
        UpdateImage,
        AddImage,
        DelImage
    }

    public partial class PatternImageEditor : UserControl
    {
        private const int THUMBNAIL_WIDTH = 100;
        private const int THUMBNAIL_HEIGHT = 100;

        private readonly ImageList _imageListThumb;      // holds the generated thumbnails

        // ––– placeholders for your business objects –––
        //private object _inspWindow;
        private object _isImg;
        private int _curAlgoIndex = -1;
        private string _editBmpPath = string.Empty;
        //private int _editIndex = -1;

        #region Events
        public event EventHandler<PatternImageEventArgs> ButtonChanged;
        #endregion

        public PatternImageEditor()
        {
            InitializeComponent();

            // designer‑free init – winforms designer is optional
            _imageListThumb = new ImageList { ImageSize = new Size(THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT), ColorDepth = ColorDepth.Depth24Bit };
            listThumbnail.LargeImageList = _imageListThumb;

            new ToolTip().SetToolTip(btnUpdate, "Update Image");
            new ToolTip().SetToolTip(btnAdd, "Add Image");
            new ToolTip().SetToolTip(btnDel, "Del Image");

            //Resize += (s, e) => AdjustLayout();
            //AdjustLayout();
        }

        //–––––––––––– public API ––––––––––––
        public void Reset()
        {
            _imageListThumb.Images.Clear();
            listThumbnail.Items.Clear();
            _isImg = null;
            //_editIndex = -1;
            //_inspWindow = null;
            _curAlgoIndex = -1;
        }

        public int ImageCount => listThumbnail.Items.Count;

        public void SetCurAlgorithmIndex(int algoIndex)
        {
            _curAlgoIndex = algoIndex;
            //DrawThumbnails(_isImg);
        }

        /// <summary>
        ///   Populate the control with a collection of Bitmaps (your CISImg replacement).
        /// </summary>
        public void DrawThumbnails(object isImg)
        {
            _imageListThumb.Images.Clear();
            listThumbnail.Items.Clear();

            // minimal stub – expect IEnumerable<Bitmap>
            if (!(isImg is IEnumerable<Bitmap> images)) return;

            int idx = 0;
            foreach (Bitmap bmp in images)
            {
                Bitmap thumb = CreateThumbnail(bmp);
                _imageListThumb.Images.Add(thumb);
                listThumbnail.Items.Add(new ListViewItem("chip", idx));
                idx++;
            }

            _isImg = isImg;  // remember last source
        }

        //–––––––––––– internal helpers ––––––––––––
        private void AdjustLayout()
        {
            if (listThumbnail is null)
                return;

            // set tile size so that caption fits below thumbnail
            listThumbnail.TileSize = new Size(THUMBNAIL_WIDTH + 5, THUMBNAIL_HEIGHT + 20);
        }

        private static Bitmap CreateThumbnail(Bitmap source)
        {
            double imgTan = (double)source.Height / source.Width;
            double rectTan = (double)THUMBNAIL_HEIGHT / THUMBNAIL_WIDTH;

            int destW = THUMBNAIL_WIDTH;
            int destH = THUMBNAIL_HEIGHT;

            if (imgTan < rectTan)
                destH = (int)(source.Height * (double)THUMBNAIL_WIDTH / source.Width);
            else
                destW = (int)(source.Width * (double)THUMBNAIL_HEIGHT / source.Height);

            var thumb = new Bitmap(THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
            using (Graphics g = Graphics.FromImage(thumb))
            {
                g.Clear(Color.White);
                g.DrawImage(source, (THUMBNAIL_WIDTH - destW) / 2, (THUMBNAIL_HEIGHT - destH) / 2, destW, destH);
            }
            return thumb;
        }

        private int GetSelectedIndex()
        {
            if (listThumbnail.SelectedIndices.Count > 0)
            {
                return listThumbnail.SelectedIndices[0];
            }
            return -1; // no selection
        }

        private void Update_Click(object sender, EventArgs e)
        {
            int nSelItem = GetSelectedIndex();
            if (nSelItem < 0)
            {
                //1개 이하 일때 기본값 사용
                if (listThumbnail.Items.Count == 1)
                    nSelItem = 0;
                else
                    return;
            }

            OnSelectChannel(PatternImageButton.UpdateImage, nSelItem);
        }

        private void Add_Click(object sender, EventArgs e)
        {
            OnSelectChannel(PatternImageButton.AddImage);
        }

        private void Del_Click(object sender, EventArgs e)
        {
            //1개 이하 일때 리턴
            if (listThumbnail.Items.Count <= 1)
                return;

            int nSelItem = GetSelectedIndex();
            if (nSelItem < 0)
                return;

            OnSelectChannel(PatternImageButton.DelImage, nSelItem);
        }

        private void OnSelectChannel(PatternImageButton buttonType, int selItem = -1)
        {
            ButtonChanged?.Invoke(this, new PatternImageEventArgs(buttonType, selItem));
        }
    }

    public class PatternImageEventArgs : EventArgs
    {
        public PatternImageButton Button { get; }
        public int Index { get; set; } = -1; // index of the selected image, if applicable

        public PatternImageEventArgs(PatternImageButton button, int index)
        {
            Button = button;
            Index = index;
        }
    }
}
