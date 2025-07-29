using JidamVision4.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace JidamVision4
{
    /*
    #2_DOCKPANEL# - <<<MainForm에 연동할 Form 도킹>>> 
    도킹에 필요한 참조를 추가하고, MainForm에 Form을 도킹
    1) ..\ExternalLib\Dll\Docking\WeifenLuo.WinFormsUI.Docking.dll 참조 추가
    2) ..\ExternalLib\Dll\Docking\WeifenLuo.WinFormsUI.Docking.ThemeVS2015.dll 참조 추가
    */

    /*
    #3_CAMERAVIEW_PROPERTY# - <<<카메라뷰와 속성창 기본 구현>>> 
    카메라뷰에 Pane과 PictureBox를 추가하고, 이미지 로딩 기능 구현
    UserControl과 TabControl을 이용해 속성창 구현
    1) CameraForm에 PictureBox와 Pane 추가
    2) 풀다운 메뉴에 ImageOpen 메뉴 추가
    3) #3_CAMERAVIEW_PROPERTY#1 ~ 2 이미지 로딩 기능 구현
    4) Property 폴더를 솔루션탐색기에 추가
    5) PropertiesForm에 ImageFilterProp UserControl과 BinaryProp UserControl 추가
    6) PropertiesForm에 TabControl 추가
    3) #3_CAMERAVIEW_PROPERTY#3 ~ 탭 콘트롤 연동 기능 구현
    */

    public partial class MainForm: Form
    {
        //#2_DOCKPANEL#1 DockPanel을 전역으로 선언
        private static DockPanel _dockPanel;

        public MainForm()
        {
            InitializeComponent();

            //#2_DOCKPANEL#2 DockPanel 초기화
            _dockPanel = new DockPanel
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_dockPanel);

            // Visual Studio 2015 테마 적용
            _dockPanel.Theme = new VS2015BlueTheme();

            //#2_DOCKPANEL#6 도킹 윈도우 로드 메서드 호출
            LoadDockingWindows();
        }

        //#2_DOCKPANEL#5 도킹 윈도우를 로드하는 메서드
        private void LoadDockingWindows()
        {
            //도킹해제 금지 설정
            _dockPanel.AllowEndUserDocking = false;

            //메인폼 설정
            var cameraWindow = new CameraForm();
            cameraWindow.Show(_dockPanel, DockState.Document);

            //속성창 추가
            var propWindow = new PropertiesForm();
            propWindow.Show(_dockPanel, DockState.DockRight);
        }

        //#2_DOCKPANEL#6 쉽게 도킹패널에 접근하기 위한 정적 함수
        //제네릭 함수 사용를 이용해 입력된 타입의 폼 객체 얻기
        public static T GetDockForm<T>() where T : DockContent
        {
            var findForm = _dockPanel.Contents.OfType<T>().FirstOrDefault();
            return findForm;
        }

        //#3_CAMERAVIEW_PROPERTY#2 풀다운 메뉴에서 이미지 열기 기능 구현
        private void imageOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CameraForm cameraForm = GetDockForm<CameraForm>();
            if (cameraForm is null)
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "이미지 파일 선택";
                openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png;*.gif";
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    cameraForm.LoadImage(filePath);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.Inst.Dispose();
        }
    }
}
