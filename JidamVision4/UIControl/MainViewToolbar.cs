using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JidamVision4.Core;
using System.Drawing.Imaging;
using JidamVision4.Util;
using System.IO;
using JidamVision4.Setting;
using System.Runtime.Remoting.Channels;

namespace JidamVision4.UIControl
{
    /*
    #18_IMAGE_CHANNEL# - <<<이미지 채널 설정 기능>>> 
    검사에서 이미지 채널을 사용할 수 있도록 설정 기능 추가
    1) UIControl / MainViewToolbar 유저컨트롤 생성
    2) CameraFrom에 MainViewToolbar 컨트롤 추가
    3) #18_IMAGE_CHANNEL#1 ~ 14
   */

    public enum ToolbarButton
    {
        ShowROI,
        ChannelColor,
        ChannelGray,
        ChannelRed,
        ChannelBlue,
        ChannelGreen
    }

    public partial class MainViewToolbar : UserControl
    {
        private ToolStripDropDownButton _dropDownButton;
        private ToolStripButton _showROIButton;

        #region Events

        public event EventHandler<ToolbarEventArgs> ButtonChanged;

        #endregion
        public MainViewToolbar()
        {
            InitializeComponent();                 // 디자이너 지원 (Optional)
            BuildToolbar();
        }

        private void BuildToolbar()
        {
            // ───────────────── ToolStrip ─────────────────
            var bar = new ToolStrip
            {
                Dock = DockStyle.Fill,
                GripStyle = ToolStripGripStyle.Hidden,
                LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow,
                AutoSize = false,
                Width = 32,
                Padding = new Padding(2),
                ImageList = imageListToolbar   // 버튼용 ImageList 연결
            };

            // ───────────────── Helper ─────────────────
            ToolStripButton IconButton(string key, string tip, EventHandler onClick = null, bool toggle = false)
            {
                var b = new ToolStripButton
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Image,
                    ImageKey = key,
                    ImageScaling = ToolStripItemImageScaling.None,
                    AutoSize = true,
                    Width = 32,
                    Height = 32,
                    CheckOnClick = toggle,
                    ToolTipText = tip
                };
                if (onClick != null) b.Click += onClick;
                return b;
            }

            // ───────────────── Buttons ─────────────────
            _showROIButton = IconButton("ShowROI", "ROI보기", (s, e) => OnShowROI(), toggle: true);

            // ───────────────── Channel DropDown ─────────────────
            _dropDownButton = new ToolStripDropDownButton
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Image = imageListToolbar.Images["Color"],   // 기본 아이콘
                ImageScaling = ToolStripItemImageScaling.None,
                ToolTipText = "Channel"
            };

            void AddChannel(string name)
            {
                var item = new ToolStripMenuItem(name, imageListToolbar.Images[name], (s, e) =>
                {
                    ToolbarButton toolbarButton = ToolbarButton.ChannelGray;
                    if ("Color" == name)
                        toolbarButton = ToolbarButton.ChannelColor;
                    else if ("Red" == name)
                        toolbarButton = ToolbarButton.ChannelRed;
                    else if ("Green" == name)
                        toolbarButton = ToolbarButton.ChannelGreen;
                    else if ("Blue" == name)
                        toolbarButton = ToolbarButton.ChannelBlue;

                    OnSelectChannel(toolbarButton);
                    _dropDownButton.Image = imageListToolbar.Images[name]; // 선택 시 아이콘 변경
                })
                {
                    ImageScaling = ToolStripItemImageScaling.None
                };
                _dropDownButton.DropDownItems.Add(item);
            }

            AddChannel("Color");
            AddChannel("Gray");
            AddChannel("Red");
            AddChannel("Blue");
            AddChannel("Green");

            // ───────────────── Assemble ─────────────────
            bar.Items.AddRange(new ToolStripItem[]
            {
                _showROIButton,
                new ToolStripSeparator(),
                _dropDownButton
            });

            Controls.Add(bar);
        }

        #region Sample Handlers        
        private void OnShowROI()
        {
            ButtonChanged?.Invoke(this, new ToolbarEventArgs(ToolbarButton.ShowROI, _showROIButton.Checked));
        }
        private void OnSelectChannel(ToolbarButton buttonType)
        {
            ButtonChanged?.Invoke(this, new ToolbarEventArgs(buttonType, false));
        }
        #endregion

        public void SetSelectButton(eImageChannel channel)
        {
            string name = channel.ToString();
            SelectChannel(name);
        }

        private void SelectChannel(string name)
        {
            if (_dropDownButton is null)
                return;

            // 메뉴 항목에서 이름이 일치하는 항목 찾기
            var menuItem = _dropDownButton.DropDownItems
                .OfType<ToolStripMenuItem>()
                .FirstOrDefault(i => i.Text == name);

            if (menuItem == null)
                return;

            // 버튼 이미지도 선택된 것으로 변경
            _dropDownButton.Image = menuItem.Image;

            // 버튼 타입 매핑해서 이벤트 발생
            ToolbarButton mappedButton = ToolbarButton.ChannelGray; // 기본값
            if (Enum.TryParse("Channel" + name, out ToolbarButton result))
                mappedButton = result;

            OnSelectChannel(mappedButton);
        }

    }

    public class ToolbarEventArgs : EventArgs
    {
        public ToolbarButton Button { get; }
        public bool IsChecked { get; }

        public ToolbarEventArgs(ToolbarButton button, bool isChecked)
        {
            Button = button;
            IsChecked = isChecked;
        }
    }

}
