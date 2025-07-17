using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision4
{
    /*
    #1_BASE FRAME
    #2_DOCKPANEL
    #3_CAMERAVIEW_PROPERTY
    #4_IMAGE_VIEWER
    #5 CAMERA INTERFACE & MAIN TOOLBAR
    #6 BINAARY PREVIEW & FILTER
    #7 TEACHING ROI
    #8 SAVE MODEL & SETUP DIALOG
    #9 PATTERN MATCHING
    #10 AUTO RUN
    #11 WCF & FSM
    */

    /*
    #1_BASE FRAME# - <<<최초 프로젝트 생성 후, 기본 프레임 생성>>> 
    3개의 Form을 생성하고, DockPanel을 통해 기본 프레임을 구성
    1) MainForm WindowForm 생성
    2) CameraForm WindowForm 생성
    3) PropertiesForm WindowForm 생성
    4) 각 Form에 DockPanel을 추가하여 도킹
    5) #BASE FRAME#를 추적하여, 코드 수정
    6) Form1 삭제
    */

    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //#1_BASE FRAME#1 시작할 Form을 MainForm으로 변경
            //Application.Run(new Form1());
            Application.Run(new MainForm());
        }
    }
}
