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

namespace JidamVision4.Property
{
    //#7_BINARY_PREVIEW#6 이진화 검사를 위한 속성창 구현
    /*
    1) UIControl / RangeTracbar 유저콘트롤 생성 후, 내부 코드만 복사&붙여넣기
    2) 이진화 속성창 디자인
    3) BinaryProp 클래스 구현
     */

    public enum ShowBinaryMode : int
    {
        ShowBinaryNone = 0,             //이진화 하이라이트 끄기
        ShowBinaryHighlightRed,         //Red 하이라이트 보기
        ShowBinaryHighlightGreen,         //Green 하이라이트 보기
        ShowBinaryHighlightBlue,         //Blue 하이라이트 보기
        ShowBinaryOnly                  //배경 없이 이진화 이미지만 보기
    }

    public partial class BinaryProp: UserControl
    {
        //속성창의 값이 변경시 발생하는 이벤트
        public event EventHandler<EventArgs> PropertyChanged;
        //양방향 슬라이더 값 변경시 발생하는 이벤트
        public event EventHandler<RangeChangedEventArgs> RangeChanged;

        BlobAlgorithm _blobAlgo = null;

        // 속성값을 이용하여 이진화 임계값 설정
        public int LeftValue => binRangeTrackbar.ValueLeft;
        public int RightValue => binRangeTrackbar.ValueRight;

        public BinaryProp()
        {
            InitializeComponent();
            
            // TrackBar 초기 설정
            binRangeTrackbar.RangeChanged += Range_RangeChanged;

            binRangeTrackbar.ValueLeft = 0;
            binRangeTrackbar.ValueRight = 128;

            //이진화 프리뷰 콤보박스 초기화 설정
            cbHighlight.Items.Add("사용안함");
            cbHighlight.Items.Add("빨간색");
            cbHighlight.Items.Add("녹색");
            cbHighlight.Items.Add("파란색");
            cbHighlight.Items.Add("흑백");
            cbHighlight.SelectedIndex = 0; // 기본값으로 "사용안함" 선택
        }
        
        public void SetAlgorithm(BlobAlgorithm blobAlgo)
        {
            _blobAlgo = blobAlgo;

            SetProperty();
        }

        //이진화 알고리즘 클래스의 정보를 UI컨트롤러에 적용
        public void SetProperty()
        {
            if (_blobAlgo is null)
                return;

            chkUse.Checked = _blobAlgo.IsUse;

            BinaryThreshold threshold = _blobAlgo.BinThreshold;

            if (threshold.invert)
            {
                binRangeTrackbar.SetThreshold(threshold.upper, threshold.lower);
            }
            else
            {
                binRangeTrackbar.SetThreshold(threshold.lower, threshold.upper);
            }
        }

        //UI컨트롤러 값을 이진화 알고리즘 클래스에 적용
        public void GetProperty()
        {
            if (_blobAlgo is null)
                return;

            _blobAlgo.IsUse = chkUse.Checked;

            BinaryThreshold threshold = new BinaryThreshold();

            int leftValue = LeftValue;
            int rightValue = RightValue;

            if (leftValue < rightValue)
            {
                threshold.lower = leftValue;
                threshold.upper = rightValue;
                threshold.invert = false;
            }
            else
            {
                threshold.lower = rightValue;
                threshold.upper = leftValue;
                threshold.invert = true;
            }

            _blobAlgo.BinThreshold = threshold;
        }

        //이진화 옵션을 선택할때마다, 이진화 이미지가 갱신되도록 하는 함수
        private void UpdateBinary()
        {
            GetProperty();

            int leftValue = LeftValue;
            int rightValue = RightValue;
            bool invert = false;

            if (leftValue > rightValue)
            {
                leftValue = RightValue;
                rightValue = LeftValue;
                invert = true;
            }

            ShowBinaryMode showBinaryMode = (ShowBinaryMode)cbHighlight.SelectedIndex;
            RangeChanged?.Invoke(this, new RangeChangedEventArgs(leftValue, rightValue, invert, showBinaryMode));
        }

        //GUI 이벤트와 UpdateBinary함수 연동
        private void Range_RangeChanged(object sender, EventArgs e)
        {
            UpdateBinary();
        }

        private void chkUse_CheckedChanged(object sender, EventArgs e)
        {
            bool useBinary = chkUse.Checked;
            grpBinary.Enabled = useBinary;

            GetProperty();
        }

        //콤보박스 변경시 이진화 프리뷰 갱신
        private void cbHighlight_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBinary();
        }
    }

    //이진화 관련 이벤트 발생시, 전달할 값 추가
    public class RangeChangedEventArgs : EventArgs
    {
        public int LowerValue { get; }
        public int UpperValue { get; }
        public bool Invert { get; }
        public ShowBinaryMode ShowBinMode { get; }

        public RangeChangedEventArgs(int lowerValue, int upperValue, bool invert, ShowBinaryMode showBinaryMode)
        {
            LowerValue = lowerValue;
            UpperValue = upperValue;
            Invert = invert;
            ShowBinMode = showBinaryMode;
        }
    }
}
