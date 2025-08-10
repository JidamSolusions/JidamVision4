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
        //#18_IMAGE_CHANNEL#10 이미지 채널 변경시 발생하는 이벤트
        public event EventHandler<ImageChannelEventArgs> ImageChannelChanged;
        //양방향 슬라이더 값 변경시 발생하는 이벤트
        public event EventHandler<RangeChangedEventArgs> RangeChanged;

        BlobAlgorithm _blobAlgo = null;

        // 속성값을 이용하여 이진화 임계값 설정
        public int LeftValue => binRangeTrackbar.ValueLeft;
        public int RightValue => binRangeTrackbar.ValueRight;

        //#8_INSPECT_BINARY#8 GridView의 업데이트 여부를 제어하는 변수와 Blob 특징 인덱스 정의
        private bool _updateDataGridView = true;
        private readonly int COL_USE = 1;
        private readonly int COL_MIN = 2;
        private readonly int COL_MAX = 3;


        public BinaryProp()
        {
            InitializeComponent();

            //#8_INSPECT_BINARY#9 이진화 속성창 초기화
            cbBinMethod.DataSource = Enum.GetValues(typeof(BinaryMethod)).Cast<BinaryMethod>().ToList();
            cbBinMethod.SelectedIndex = (int)BinaryMethod.Feature;

            InitializeFilterDataGridView();

            // TrackBar 초기 설정
            binRangeTrackbar.RangeChanged += Range_RangeChanged;

            binRangeTrackbar.ValueLeft = 0;
            binRangeTrackbar.ValueRight = 128;

            //#18_IMAGE_CHANNEL#9 이미지 채널 설정 콤보박스
            cbChannel.Items.Add("Gray");
            cbChannel.Items.Add("Red");
            cbChannel.Items.Add("Green");
            cbChannel.Items.Add("Blue");
            cbChannel.SelectedIndex = 0; // 기본값으로 "사용안함" 선택

            //이진화 프리뷰 콤보박스 초기화 설정
            cbHighlight.Items.Add("사용안함");
            cbHighlight.Items.Add("빨간색");
            cbHighlight.Items.Add("녹색");
            cbHighlight.Items.Add("파란색");
            cbHighlight.Items.Add("흑백");
            cbHighlight.SelectedIndex = 0; // 기본값으로 "사용안함" 선택
        }

        private void InitializeFilterDataGridView()
        {
            // 컬럼 설정
            dataGridViewFilter.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "필터명",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = 70
            });

            dataGridViewFilter.Columns.Add(new DataGridViewCheckBoxColumn()
            {
                HeaderText = "사용",
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = 40
            });

            dataGridViewFilter.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "최소값",
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = 65
            });

            dataGridViewFilter.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "최대값",
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = 65
            });

            // 항목 추가
            AddFilterRow("Area");
            AddFilterRow("Length");
            AddFilterRow("Width");
            AddFilterRow("Count");

            dataGridViewFilter.AllowUserToAddRows = false;
            dataGridViewFilter.RowHeadersVisible = false;
            dataGridViewFilter.AllowUserToResizeColumns = false;
            dataGridViewFilter.AllowUserToResizeRows = false;
            dataGridViewFilter.AllowUserToOrderColumns = false;
            dataGridViewFilter.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void AddFilterRow(string itemName)
        {
            dataGridViewFilter.Rows.Add(itemName, false, "", "");
        }

        public void SetAlgorithm(BlobAlgorithm blobAlgo)
        {
            _blobAlgo = blobAlgo;

            //#8_INSPECT_BINARY#10 이진화 알고리즘 필터값이 없을 경우, 기본값 설정
            if (_blobAlgo.BlobFilters.Count <= 0)
                blobAlgo.SetDefault();

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

            //#8_INSPECT_BINARY#11 이진화 검사 관련 속성값 적용
            cbBinMethod.SelectedIndex = (int)_blobAlgo.BinMethod;

            cbChannel.SelectedIndex  = (int)_blobAlgo.ImageChannel - 1;

            UpdateDataGridView(true);
            chkRotatedRect.Checked = _blobAlgo.UseRotatedRect;
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

            //#8_INSPECT_BINARY#12 GridView 값을 _blobAlgo에 반영
            UpdateDataGridView(false);
        }

        private void UpdateDataGridView(bool update)
        {
            if (_blobAlgo is null)
                return;

            if (update)
            {
                _updateDataGridView = false;
                List<BlobFilter> blobFilters = _blobAlgo.BlobFilters;

                for (int i = 0; i < blobFilters.Count; i++)
                {
                    if (i >= dataGridViewFilter.Rows.Count)
                        break;

                    dataGridViewFilter.Rows[i].Cells[COL_USE].Value = blobFilters[i].isUse;
                    dataGridViewFilter.Rows[i].Cells[COL_MIN].Value = blobFilters[i].min;
                    dataGridViewFilter.Rows[i].Cells[COL_MAX].Value = blobFilters[i].max;
                }
                _updateDataGridView = true;
            }
            else
            {
                if (_updateDataGridView == false)
                    return;

                List<BlobFilter> blobFilters = _blobAlgo.BlobFilters;

                for (int i = 0; i < blobFilters.Count; i++)
                {
                    BlobFilter blobFilter = blobFilters[i];
                    blobFilter.isUse = (bool)dataGridViewFilter.Rows[i].Cells[COL_USE].Value;

                    object value = dataGridViewFilter.Rows[i].Cells[COL_MIN].Value;

                    int min = 0;
                    if (value != null && int.TryParse(value.ToString(), out min))
                        blobFilter.min = min;

                    value = dataGridViewFilter.Rows[i].Cells[COL_MAX].Value;

                    int max = 0;
                    if (value != null && int.TryParse(value.ToString(), out max))
                        blobFilter.max = max;
                }
            }
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

            //#8_INSPECT_BINARY#13 검사 여부에 따른, 활성화 여부
            dataGridViewFilter.Enabled = useBinary;

            GetProperty();
        }

        //콤보박스 변경시 이진화 프리뷰 갱신
        private void cbHighlight_SelectedIndexChanged(object sender, EventArgs e)
        {
            //#18_IMAGE_CHANNEL#12 하이라이트 선택시, 이미지 채널 정보를 전달하여,
            //프리뷰에 나타나도록 이벤트 발생
            if (_blobAlgo is null)
                return;

            _blobAlgo.ImageChannel = (eImageChannel)cbChannel.SelectedIndex + 1;
            ImageChannelChanged?.Invoke(this, new ImageChannelEventArgs(_blobAlgo.ImageChannel));
            UpdateBinary();
        }

        //#8_INSPECT_BINARY#14 이벤트 발생시 값에 반영
        private void dataGridViewFilter_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_updateDataGridView == true)
                UpdateDataGridView(false);
        }

        // DataGridView안에 있는 체크박스의 경우, CellValueChanged가 발생하지 않아,
        // CellDirtyStateChanged 이벤트를 사용하여 체크박스의 상태가 변경될 때 CommitEdit을 호출합니다.
        private void dataGridViewFilter_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewFilter.CurrentCell is DataGridViewCheckBoxCell)
            {
                dataGridViewFilter.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void chkRotatedRect_CheckedChanged(object sender, EventArgs e)
        {
            if (_blobAlgo is null)
                return;

            _blobAlgo.UseRotatedRect = chkRotatedRect.Checked;
        }

        private void cbBinMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blobAlgo is null)
                return;

            _blobAlgo.BinMethod = (BinaryMethod)cbBinMethod.SelectedIndex;
            chkRotatedRect.Enabled = _blobAlgo.BinMethod == BinaryMethod.Feature;

            if (_blobAlgo.BinMethod == BinaryMethod.PixelCount)
            {
                for (int i = 0; i < dataGridViewFilter.Rows.Count; i++)
                {
                    bool useFeature = i == 0 ? true : false; // Area 필터만 사용 가능
                    dataGridViewFilter.Rows[i].Cells[COL_USE].Value = useFeature;
                }
                dataGridViewFilter.Columns[COL_USE].ReadOnly = true;
            }
            else
            {
                dataGridViewFilter.Columns[COL_USE].ReadOnly = false;
            }

            _updateDataGridView = true;
        }

        //#18_IMAGE_CHANNEL#11 이미지 채널 변경시, 화면에 해당 채널을 표시
        private void cbChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(_blobAlgo is null)
                return;

            _blobAlgo.ImageChannel = (eImageChannel)cbChannel.SelectedIndex + 1;
            ImageChannelChanged?.Invoke(this, new ImageChannelEventArgs(_blobAlgo.ImageChannel));
        }
    }

    public class ImageChannelEventArgs : EventArgs
    {
        public eImageChannel Channel{ get; }
        public int UpperValue { get; }
        public bool Invert { get; }
        public ShowBinaryMode ShowBinMode { get; }

        public ImageChannelEventArgs(eImageChannel channel)
        {
            Channel = channel;
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
