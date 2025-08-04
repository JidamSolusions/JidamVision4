using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision4.UIControl
{
    /// <summary>
    /// 수평 양방향 Range 트랙바 (WinForms)
    /// 최소/최대 Thumb 두 개를 가지고 있으며,
    /// 서로 겹칠 때는 반전 Highlight 로 영역을 표시합니다.
    /// </summary>
    public partial class RangeTrackbar : UserControl
    {
        #region Fields & Constants
        private const int ThumbWidth = 10;
        private const int ThumbHeight = 14;
        private const int TrackHeight = 4;
        private const int Pad = 2;

        private int _minimum = 0;
        private int _maximum = 255;
        private int _valueLeft = 80;
        private int _valueRight = 200;

        private bool _dragLeft;
        private bool _dragRight;
        #endregion

        #region Events
        public event EventHandler RangeChanged;
        protected virtual void OnRangeChanged() => RangeChanged?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Properties
        [DefaultValue(0)]
        public int Minimum
        {
            get { return _minimum; }
            set { _minimum = value; Invalidate(); }
        }

        [DefaultValue(255)]
        public int Maximum
        {
            get { return _maximum; }
            set { _maximum = value; Invalidate(); }
        }

        public int ValueLeft
        {
            get { return _valueLeft; }
            set
            {
                _valueLeft = Clamp(value);
                OnRangeChanged();
                Invalidate();
            }
        }

        public int ValueRight
        {
            get { return _valueRight; }
            set
            {
                _valueRight = Clamp(value);
                OnRangeChanged();
                Invalidate();
            }
        }
        #endregion

        public RangeTrackbar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Height = ThumbHeight + Pad * 4;

            // InitializeComponent(); // 디자이너 파일이 있을 때만 필요
        }

        public void SetThreshold(int left, int right)
        {
            _valueLeft = left;
            _valueRight = right;
            Invalidate();
        }

        #region Painting
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle trackRect = GetTrackRectangle();

            // 트랙 바닥
            using (SolidBrush trackBrush = new SolidBrush(SystemColors.ControlDark))
            {
                g.FillRectangle(trackBrush, trackRect);
            }

            // Thumb 위치
            int leftPx = ValueToPixel(_valueLeft, trackRect);
            int rightPx = ValueToPixel(_valueRight, trackRect);

            // Highlight
            if (_valueLeft <= _valueRight)
            {
                Rectangle hl = new Rectangle(leftPx, trackRect.Top, rightPx - leftPx, TrackHeight);
                using (SolidBrush hlBrush = new SolidBrush(SystemColors.Highlight))
                {
                    g.FillRectangle(hlBrush, hl);
                }
            }
            else
            {
                Rectangle leftHl = new Rectangle(trackRect.Left, trackRect.Top, rightPx - trackRect.Left, TrackHeight);
                Rectangle rightHl = new Rectangle(leftPx, trackRect.Top, trackRect.Right - leftPx, TrackHeight);
                using (SolidBrush hlBrush = new SolidBrush(SystemColors.Highlight))
                {
                    g.FillRectangle(hlBrush, leftHl);
                    g.FillRectangle(hlBrush, rightHl);
                }
            }

            // Thumb & value 텍스트
            DrawThumb(g, leftPx, trackRect.Top + TrackHeight / 2, _valueLeft);
            DrawThumb(g, rightPx, trackRect.Top + TrackHeight / 2, _valueRight);
        }

        private void DrawThumb(Graphics g, int centerX, int centerY, int value)
        {
            Rectangle thumbRect = new Rectangle(centerX - ThumbWidth / 2, centerY - ThumbHeight / 2, ThumbWidth, ThumbHeight);
            using (SolidBrush b = new SolidBrush(SystemColors.ControlLightLight))
            {
                g.FillRectangle(b, thumbRect);
            }
            g.DrawRectangle(Pens.Gray, thumbRect);

            // 값 텍스트 (thumb 위에)
            string txt = value.ToString();
            SizeF sz = g.MeasureString(txt, Font);
            PointF txtPos = new PointF(centerX - sz.Width / 2, thumbRect.Top - sz.Height - 2);
            using (SolidBrush txtBrush = new SolidBrush(ForeColor))
            {
                g.DrawString(txt, Font, txtBrush, txtPos);
            }
        }
        #endregion

        #region Mouse Handling
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Rectangle trackRect = GetTrackRectangle();
            int leftPx = ValueToPixel(_valueLeft, trackRect);
            int rightPx = ValueToPixel(_valueRight, trackRect);
            if (IsPointOnThumb(e.Location, rightPx, trackRect)) _dragRight = true;
            else if (IsPointOnThumb(e.Location, leftPx, trackRect)) _dragLeft = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragLeft = _dragRight = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_dragLeft && !_dragRight) return;
            Rectangle trackRect = GetTrackRectangle();
            int v = PixelToValue(e.X, trackRect);
            if (_dragLeft) ValueLeft = v;
            else if (_dragRight) ValueRight = v;
        }

        private bool IsPointOnThumb(Point p, int thumbCenterX, Rectangle trackRect)
        {
            Rectangle thumbRect = new Rectangle(thumbCenterX - ThumbWidth / 2, trackRect.Top - ThumbHeight / 2,
                                                 ThumbWidth, ThumbHeight + TrackHeight);
            return thumbRect.Contains(p);
        }
        #endregion

        #region Helpers
        private Rectangle GetTrackRectangle()
        {
            int trackY = Height / 2 - TrackHeight / 2 + Pad * 2;
            int trackLeft = Pad + ThumbWidth / 2;
            int trackRight = Width - Pad - ThumbWidth / 2;
            return new Rectangle(trackLeft, trackY, trackRight - trackLeft, TrackHeight);
        }

        private int Clamp(int v)
        {
            if (v < _minimum) return _minimum;
            if (v > _maximum) return _maximum;
            return v;
        }

        private int ValueToPixel(int v, Rectangle track)
        {
            double ratio = (double)(v - _minimum) / (_maximum - _minimum);
            return track.Left + (int)(ratio * track.Width);
        }

        private int PixelToValue(int px, Rectangle track)
        {
            double ratio = (double)(px - track.Left) / track.Width;
            int v = _minimum + (int)(ratio * (_maximum - _minimum));
            return Clamp(v);
        }
        #endregion
    }
}
