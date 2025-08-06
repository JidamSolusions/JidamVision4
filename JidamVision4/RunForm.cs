using JidamVision4.Core;
using JidamVision4.Setting;
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
    public partial class RunForm: DockContent
    {
        public RunForm()
        {
            InitializeComponent();
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            //#13_SET_IMAGE_BUFFER#3 그랩시 이미지 버퍼를 먼저 설정하도록 변경
            Global.Inst.InspStage.CheckImageBuffer();
            Global.Inst.InspStage.Grab(0);
        }

        //#8_INSPECT_BINARY#20 검사 시작 버튼을 디자인창에서 만들고, 검사 함수 호출
        private void btnStart_Click(object sender, EventArgs e)
        {
            //#15_INSP_WORKER#10 카메라 타입에 따라 자동 검사 모드 설정

            string serialID = $"{DateTime.Now:MM-dd HH:mm:ss}";
            Global.Inst.InspStage.InspectReady("LOT_NUMBER", serialID);

            if (SettingXml.Inst.CamType == Grab.CameraType.None)
            {
                bool cycleMode = SettingXml.Inst.CycleMode;
                Global.Inst.InspStage.CycleInspect(cycleMode);
            }
            else
            {
                Global.Inst.InspStage.StartAutoRun();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.StopCycle();
        }

        //#8_LIVE#3 라이브 모드 버튼 추가
        private void btnLive_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.LiveMode = !Global.Inst.InspStage.LiveMode;

            //#17_WORKING_STATE#6 LIVE 상태 화면 표시
            if (Global.Inst.InspStage.LiveMode)
            {
                Global.Inst.InspStage.SetWorkingState(WorkingState.LIVE);

                //#13_SET_IMAGE_BUFFER#4 그랩시 이미지 버퍼를 먼저 설정하도록 변경
                Global.Inst.InspStage.CheckImageBuffer();
                Global.Inst.InspStage.Grab(0);
            }
            else
            {
                Global.Inst.InspStage.SetWorkingState(WorkingState.NONE);
            }
        }

    }
}
