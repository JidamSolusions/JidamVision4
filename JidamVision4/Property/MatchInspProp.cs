using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JidamVision4.Algorithm;
using JidamVision4.Core;
using JidamVision4.UIControl;
using OpenCvSharp.Extensions;
using OpenCvSharp;

namespace JidamVision4.Property
{
    public partial class MatchInspProp: UserControl
    {
        public event EventHandler<EventArgs> PropertyChanged;

        MatchAlgorithm _matchAlgo = null;

        public MatchInspProp()
        {
            InitializeComponent();

            txtExtendX.Leave += OnUpdateValue;
            txtExtendY.Leave += OnUpdateValue;
            txtScore.Leave += OnUpdateValue;

            patternImageEditor.ButtonChanged += PatternImage_ButtonChanged;
        }

        public void SetAlgorithm(MatchAlgorithm matchAlgo)
        {
            _matchAlgo = matchAlgo;
            SetProperty();
        }

        public void SetProperty()
        {
            if (_matchAlgo is null)
                return;

            chkUse.Checked = _matchAlgo.IsUse;

            OpenCvSharp.Size extendSize = _matchAlgo.ExtSize;
            int matchScore = _matchAlgo.MatchScore;
            int matchCount = _matchAlgo.MatchCount;

            txtExtendX.Text = extendSize.Width.ToString();
            txtExtendY.Text = extendSize.Height.ToString();
            txtScore.Text = matchScore.ToString();

            chkInvertResult.Checked = _matchAlgo.InvertResult;

            List<Mat> templateImages = _matchAlgo.GetTemplateImages();
            if (templateImages.Count > 0)
            {
                List<Bitmap> teachImages = new List<Bitmap>();

                foreach (var teachImage in templateImages)
                {
                    Bitmap bmpImage = BitmapConverter.ToBitmap(teachImage);
                    teachImages.Add(bmpImage);
                }

                patternImageEditor.DrawThumbnails(teachImages);
            }
        }

        private void OnUpdateValue(object sender, EventArgs e)
        {
            if (_matchAlgo == null)
                return;

            OpenCvSharp.Size extendSize = _matchAlgo.ExtSize;

            if (!int.TryParse(txtExtendX.Text, out extendSize.Width))
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                return;
            }

            if (!int.TryParse(txtExtendY.Text, out extendSize.Height))
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                return;
            }

            int score = _matchAlgo.MatchScore;
            if (!int.TryParse(txtScore.Text, out score))
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                return;
            }
            ;

            _matchAlgo.ExtSize = extendSize;
            _matchAlgo.MatchScore = score;

            PropertyChanged?.Invoke(this, null);
        }

        private void chkUse_CheckedChanged(object sender, EventArgs e)
        {
            bool useMatch = chkUse.Checked;

            grpMatch.Enabled = useMatch;
            patternImageEditor.Enabled = useMatch;

            if (_matchAlgo != null)
                _matchAlgo.IsUse = useMatch;
        }

        private void chkInvertResult_CheckedChanged(object sender, EventArgs e)
        {
            if (_matchAlgo is null)
                return;

            _matchAlgo.InvertResult = chkInvertResult.Checked;
        }

        private void PatternImage_ButtonChanged(object sender, PatternImageEventArgs e)
        {
            int index = e.Index;

            switch (e.Button)
            {
                case PatternImageButton.UpdateImage:
                    Global.Inst.InspStage.UpdateTeachingImage(index);
                    break;
                case PatternImageButton.AddImage:
                    Global.Inst.InspStage.UpdateTeachingImage(-1);
                    break;
                case PatternImageButton.DelImage:
                    Global.Inst.InspStage.DelTeachingImage(index);
                    break;
            }
        }

    }
}
