using JidamVision4.Core;
using JidamVision4.Setting;
using JidamVision4.Teach;
using JidamVision4.Util;
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

            //#6_INSP_STAGE#1 전역 인스턴스 초기화
            Global.Inst.Initialize();

            //#15_INSP_WORKER#2 연속 검사 모드 설정값 로딩
            LoadSetting();
        }

        //#2_DOCKPANEL#5 도킹 윈도우를 로드하는 메서드
        private void LoadDockingWindows()
        {
            //도킹해제 금지 설정
            _dockPanel.AllowEndUserDocking = false;

            //메인폼 설정
            var cameraWindow = new CameraForm();
            cameraWindow.Show(_dockPanel, DockState.Document);

            //#13_INSP_RESULT#7 검사 결과창 30% 비율로 추가
            var resultWindow = new ResultForm();
            resultWindow.Show(cameraWindow.Pane, DockAlignment.Bottom, 0.3);

            //# MODEL TREE#3 검사 결과창 우측에 40% 비율로 모델트리 추가
            var modelTreeWindow = new ModelTreeForm();
            modelTreeWindow.Show(resultWindow.Pane, DockAlignment.Right, 0.4);

            //실행창 추가
            var runWindow = new RunForm();
            runWindow.Show(modelTreeWindow.Pane, null);

            //속성창 추가
            var propWindow = new PropertiesForm();
            propWindow.Show(_dockPanel, DockState.DockRight);

            //#14_LOGFORM#2 로그창 추가
            var logWindow = new LogForm();
            logWindow.Show(propWindow.Pane, DockAlignment.Bottom, 0.3);
        }
        private void LoadSetting()
        {
            cycleModeMenuItem.Checked = SettingXml.Inst.CycleMode;
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
                    //#13_SET_IMAGE_BUFFER#2 이미지에 맞게 버퍼를 먼저 설정하도록 변경
                    string filePath = openFileDialog.FileName;
                    Global.Inst.InspStage.SetImageBuffer(filePath);
                    Global.Inst.InspStage.CurModel.InspectImagePath = filePath;
                }
            }
        }

        //#9_SETUP#1 환경설정창 실행
        private void SetupMenuItem_Click(object sender, EventArgs e)
        {
            SLogger.Write($"환경설정창 열기");
            SetupForm setupForm = new SetupForm();
            setupForm.ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Global.Inst.Dispose();
        }

        //#12_MODEL SAVE#3 모델 파일 열기,저장, 다른 이름으로 저장 기능 구현
        private string GetMdoelTitle(Model curModel)
        {
            if (curModel is null)
                return "";

            string modelName = curModel.ModelName;
            return $"{Define.PROGRAM_NAME} - MODEL : {modelName}";
        }

        private void modelNewMenuItem_Click(object sender, EventArgs e)
        {
            //신규 모델 추가를 위한 모델 정보를 받기 위한 창 띄우기
            NewModel newModel = new NewModel();
            newModel.ShowDialog();

            Model curModel = Global.Inst.InspStage.CurModel;
            if (curModel != null)
            {
                this.Text = GetMdoelTitle(curModel);
            }
        }

        private void modelOpenMenuItem_Click(object sender, EventArgs e)
        {
            //모델 파일 열기
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "모델 파일 선택";
                openFileDialog.Filter = "Model Files|*.xml;";
                openFileDialog.Multiselect = false;
                openFileDialog.InitialDirectory = SettingXml.Inst.ModelDir;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    if (Global.Inst.InspStage.LoadModel(filePath))
                    {
                        Model curModel = Global.Inst.InspStage.CurModel;
                        if (curModel != null)
                        {
                            this.Text = GetMdoelTitle(curModel);
                        }
                    }
                }
            }
        }

        private void modelSaveMenuItem_Click(object sender, EventArgs e)
        {
            //모델 파일 저장
            Global.Inst.InspStage.SaveModel("");
        }

        private void modelSaveAsMenuItem_Click(object sender, EventArgs e)
        {
            //다른이름으로 모델 파일 저장
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = SettingXml.Inst.ModelDir;
                saveFileDialog.Title = "모델 파일 선택";
                saveFileDialog.Filter = "Model Files|*.xml;";
                saveFileDialog.DefaultExt = "xml";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    Global.Inst.InspStage.SaveModel(filePath);
                }
            }
        }

        //#15_INSP_WORKER#3 Cycle 모드 설정
        private void cycleModeMenuItem_Click(object sender, EventArgs e)
        {
            // 현재 체크 상태 확인
            bool isChecked = cycleModeMenuItem.Checked;
            SettingXml.Inst.CycleMode = isChecked;
        }
    }
}
