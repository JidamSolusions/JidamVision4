using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using JidamVision4.Algorithm;
using JidamVision4.Core;

namespace JidamVision4.UIControl
{
    /*
    #4_IMAGE_VIEWER# - <<<줌인아웃 이미지 뷰어>>> 
    - 마우스 휠을 이용한 확대/축소 이미지 뷰어
    */

    public partial class ImageViewCtrl: UserControl
    {
        //#4_IMAGE_VIEWER#1 변수 선언
        private bool _isInitialized = false;

        // 현재 로드된 이미지
        private Bitmap _bitmapImage = null;

        // 더블 버퍼링을 위한 캔버스
        // 더블버퍼링 : 화면 깜빡임을 방지하고 부드러운 펜더링위해 사용
        private Bitmap Canvas = null;

        // 화면에 표시될 이미지의 크기 및 위치
        // 부동 소수점(float) 좌표를 사용하는 사각형 구조체
        private RectangleF ImageRect = new RectangleF(0, 0, 0, 0);

        // 현재 줌 배율
        private float _curZoom = 1.0f;
        // 줌 배율 변경 시, 확대/축소 단위
        private float _zoomFactor = 1.1f;

        // 최소 및 최대 줌 제한 값
        private float MinZoom = 1.0f;
        private const float MaxZoom = 100.0f;

        //#8_INSPECT_BINARY#15 템플릿 매칭 결과 출력을 위해 Rectangle 리스트 변수 설정
        private List<DrawInspectInfo> _rectInfos = new List<DrawInspectInfo>();


        public ImageViewCtrl()
        {
            InitializeComponent();
            InitializeCanvas();

            MouseWheel += new MouseEventHandler(ImageViewCCtrl_MouseWheel);
        }

        //#4_IMAGE_VIEWER#2 캔버스 초기화 및 설정
        private void InitializeCanvas()
        {
            // 캔버스를 UserControl 크기만큼 생성
            ResizeCanvas();

            // 화면 깜빡임을 방지하기 위한 더블 버퍼링 설정
            DoubleBuffered = true;
        }

        public Bitmap GetCurBitmap()
        {
            return _bitmapImage;
        }

        //줌에 따른 좌표 계산 기능 수정 
        private void ResizeCanvas()
        {
            if (Width <= 0 || Height <= 0 || _bitmapImage == null)
                return;

            // 캔버스를 UserControl 크기만큼 생성
            Canvas = new Bitmap(Width, Height);
            if (Canvas == null)
                return;

            // 이미지 원본 크기 기준으로 확대/축소 (ZoomFactor 유지)
            float virtualWidth = _bitmapImage.Width * _curZoom;
            float virtualHeight = _bitmapImage.Height * _curZoom;

            float offsetX = virtualWidth < Width ? (Width - virtualWidth) / 2f : 0f;
            float offsetY = virtualHeight < Height ? (Height - virtualHeight) / 2f : 0f;

            ImageRect = new RectangleF(offsetX, offsetY, virtualWidth, virtualHeight);
        }

        //#4_IMAGE_VIEWER#5 이미지 로딩 함수
        public void LoadBitmap(Bitmap bitmap)
        {
            // 기존에 로드된 이미지가 있다면 해제 후 초기화, 메모리누수 방지
            if (_bitmapImage != null)
            {
                //이미지 크기가 같다면, 이미지 변경 후, 화면 갱신
                if (_bitmapImage.Width == bitmap.Width && _bitmapImage.Height == bitmap.Height)
                {
                    _bitmapImage = bitmap;
                    Invalidate();
                    return;
                }

                _bitmapImage.Dispose(); // Bitmap 객체가 사용하던 메모리 리소스를 해제
                _bitmapImage = null;  //객체를 해제하여 가비지 컬렉션(GC)이 수집할 수 있도록 설정
            }

            // 새로운 이미지 로드
            _bitmapImage = bitmap;

            ////bitmap==null 예외처리도 초기화되지않은 변수들 초기화
            if (_isInitialized == false)
            {
                _isInitialized = true;
                ResizeCanvas();
            }

            FitImageToScreen();
        }

        private void FitImageToScreen()
        {
            if (_bitmapImage is null)
                return;

            RecalcZoomRatio();

            float NewWidth = _bitmapImage.Width * _curZoom;
            float NewHeight = _bitmapImage.Height * _curZoom;

            // 이미지가 UserControl 중앙에 배치되도록 정렬
            ImageRect = new RectangleF(
                (Width - NewWidth) / 2, // UserControl 너비에서 이미지 너비를 뺀 후, 절반을 왼쪽 여백으로 설정하여 중앙 정렬
                (Height - NewHeight) / 2,
                NewWidth,
                NewHeight
            );

            Invalidate();
        }

        //#GROUP ROI#7 현재 이미지를 기준으로 줌 비율 재계산
        private void RecalcZoomRatio()
        {
            if (_bitmapImage == null || Width <= 0 || Height <= 0)
                return;

            Size imageSize = new Size(_bitmapImage.Width, _bitmapImage.Height);

            float aspectRatio = (float)imageSize.Height / (float)imageSize.Width;
            float clientAspect = (float)Height / (float)Width;

            //UserControl과 이미지의 비율의 관계를 통해, 이미지가 UserControl안에 들어가도록 Zoom비율 설정
            float ratio;
            if (aspectRatio <= clientAspect)
                ratio = (float)Width / (float)imageSize.Width;
            else
                ratio = (float)Height / (float)imageSize.Height;

            //최소 줌 비율은 이미지가 UserControl에 꽉차게 들어가는 것으로 설정
            float minZoom = ratio;

            // MinZoom 및 줌 적용
            MinZoom = minZoom;

            _curZoom = Math.Max(MinZoom, Math.Min(MaxZoom, ratio));

            Invalidate();
        }

        //#4_IMAGE_VIEWER#3
        // Windows Forms에서 컨트롤이 다시 그려질 때 자동으로 호출되는 메서드
        // 화면새로고침(Invalidate()), 창 크기변경, 컨트롤이 숨겨졌다가 나타날때 실행
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_bitmapImage != null && Canvas != null)
            {
                // 캔버스를 초기화하고 이미지 그리기
                using (Graphics g = Graphics.FromImage(Canvas))  // 메모리누수방지
                {
                    g.Clear(Color.Transparent); // 배경을 투명하게 설정

                    //이미지 확대or축소때 화질 최적화 방식(Interpolation Mode) 설정                    
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(_bitmapImage, ImageRect);

                    //#8_INSPECT_BINARY#16 _rectInfos 그리기
                    DrawDiagram(g);

                    // 캔버스를 UserControl 화면에 표시
                    e.Graphics.DrawImage(Canvas, 0, 0);
                }
            }
        }

        private void DrawDiagram(Graphics g)
        {
            // 이미지 좌표 → 화면 좌표 변환 후 사각형 그리기
            if (_rectInfos != null)
            {
                foreach (DrawInspectInfo rectInfo in _rectInfos)
                {
                    Color lineColor = Color.LightCoral;
                    if (rectInfo.decision == DecisionType.Defect)
                        lineColor = Color.Red;
                    else if (rectInfo.decision == DecisionType.Good)
                        lineColor = Color.LightGreen;

                    Rectangle rect = new Rectangle(rectInfo.rect.X, rectInfo.rect.Y, rectInfo.rect.Width, rectInfo.rect.Height);
                    Rectangle screenRect = VirtualToScreen(rect);

                    using (Pen pen = new Pen(lineColor, 2))
                    {
                        if (rectInfo.UseRotatedRect)
                        {
                            PointF[] screenPoints = rectInfo.rotatedPoints
                                                    .Select(p => VirtualToScreen(new PointF(p.X, p.Y))) // 화면 좌표계로 변환
                                                    .ToArray();

                            if (screenPoints.Length == 4)
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    g.DrawLine(pen, screenPoints[i], screenPoints[(i + 1) % 4]); // 시계방향으로 선 연결
                                }
                            }
                        }
                        else
                        {
                            g.DrawRectangle(pen, screenRect);
                        }
                    }

                    if (rectInfo.info != "")
                    {
                        float baseFontSize = 20.0f;

                        if (rectInfo.decision == DecisionType.Info)
                        {
                            baseFontSize = 3.0f;
                            lineColor = Color.LightBlue;
                        }

                        float fontSize = baseFontSize * _curZoom;

                        // 스코어 문자열 그리기 (우상단)
                        string infoText = rectInfo.info;
                        PointF textPos = new PointF(screenRect.Left, screenRect.Top); // 위로 약간 띄우기

                        if (rectInfo.inspectType == InspectType.InspBinary
                            && rectInfo.decision != DecisionType.Info)
                        {
                            textPos.Y = screenRect.Bottom - fontSize;
                        }

                        DrawText(g, infoText, textPos, fontSize, lineColor);
                    }
                }
            }
        }

        private void DrawText(Graphics g, string text, PointF position, float fontSize, Color color)
        {
            using (Font font = new Font("Arial", fontSize, FontStyle.Bold))
            // 테두리용 검정색 브러시
            using (Brush outlineBrush = new SolidBrush(Color.Black))
            // 본문용 노란색 브러시
            using (Brush textBrush = new SolidBrush(color))
            {
                // 테두리 효과를 위해 주변 8방향으로 그리기
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue; // 가운데는 제외
                        PointF borderPos = new PointF(position.X + dx, position.Y + dy);
                        g.DrawString(text, font, outlineBrush, borderPos);
                    }
                }

                // 본문 텍스트
                g.DrawString(text, font, textBrush, position);
            }
        }

        //#4_IMAGE_VIEWER#4 마우스휠을 이용한 확대/축소
        private void ImageViewCCtrl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                ZoomMove(_curZoom / _zoomFactor, e.Location);
            else
                ZoomMove(_curZoom * _zoomFactor, e.Location);

            // 새로운 이미지 위치 반영 (점진적으로 초기 상태로 회귀)
            if (_bitmapImage != null)
            {
                ImageRect.Width = _bitmapImage.Width * _curZoom;
                ImageRect.Height = _bitmapImage.Height * _curZoom;
            }

            // 다시 그리기 요청
            Invalidate();
        }

        //휠에 의해, Zoom 확대/축소 값 계산
        private void ZoomMove(float zoom, Point zoomOrigin)
        {
            PointF virtualOrigin = ScreenToVirtual(new PointF(zoomOrigin.X, zoomOrigin.Y));

            _curZoom = Math.Max(MinZoom, Math.Min(MaxZoom, zoom));
            if (_curZoom <= MinZoom)
                return;

            PointF zoomedOrigin = VirtualToScreen(virtualOrigin);

            float dx = zoomedOrigin.X - zoomOrigin.X;
            float dy = zoomedOrigin.Y - zoomOrigin.Y;

            ImageRect.X -= dx;
            ImageRect.Y -= dy;
        }

        // Virtual <-> Screen 좌표계 변환
        #region 좌표계 변환
        private PointF GetScreenOffset()
        {
            return new PointF(ImageRect.X, ImageRect.Y);
        }

        private Rectangle ScreenToVirtual(Rectangle screenRect)
        {
            PointF offset = GetScreenOffset();
            return new Rectangle(
                (int)((screenRect.X - offset.X) / _curZoom + 0.5f),
                (int)((screenRect.Y - offset.Y) / _curZoom + 0.5f),
                (int)(screenRect.Width / _curZoom + 0.5f),
                (int)(screenRect.Height / _curZoom + 0.5f));
        }

        private Rectangle VirtualToScreen(Rectangle virtualRect)
        {
            PointF offset = GetScreenOffset();
            return new Rectangle(
                (int)(virtualRect.X * _curZoom + offset.X + 0.5f),
                (int)(virtualRect.Y * _curZoom + offset.Y + 0.5f),
                (int)(virtualRect.Width * _curZoom + 0.5f),
                (int)(virtualRect.Height * _curZoom + 0.5f));
        }

        private PointF ScreenToVirtual(PointF screenPos)
        {
            PointF offset = GetScreenOffset();
            return new PointF(
                (screenPos.X - offset.X) / _curZoom,
                (screenPos.Y - offset.Y) / _curZoom);
        }

        private PointF VirtualToScreen(PointF virtualPos)
        {
            PointF offset = GetScreenOffset();
            return new PointF(
                virtualPos.X * _curZoom + offset.X,
                virtualPos.Y * _curZoom + offset.Y);
        }
        #endregion

        private void ImageViewCtrl_Resize(object sender, EventArgs e)
        {
            ResizeCanvas();
            Invalidate();
        }

        private void ImageViewCtrl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            FitImageToScreen();
        }

        //#8_INSPECT_BINARY#17 화면에 보여줄 영역 정보를 표시하기 위해, 위치 입력 받는 함수
        public void AddRect(List<DrawInspectInfo> rectInfos)
        {
            _rectInfos.AddRange(rectInfos);
            Invalidate();
        }

        public void ResetEntity()
        {
            _rectInfos.Clear();
            Invalidate();
        }
    }
}
