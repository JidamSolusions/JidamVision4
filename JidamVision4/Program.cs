using JidamVision4.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JidamVision4
{
    /*
    #1_BASE FRAME       
    #2_DOCKPANEL         DockPanel을 이용한 도킹 프레임 구성
    #3_CAMERAVIEW_PROPERTY  이미지뷰어와 속성창 기본 구현
    #4_IMAGE_VIEWER     확대/축소 이미지 뷰어
    #5_SAIGE_SDK        Saige SDK를 이용한 카메라 인터페이스 구현
    #6_CORE_MODULE       핵심 모듈 및 인터페이스 정의
    #7_CAMERA_TOOLBAR   카메라 인터페이스와 메인 툴바
    #8_BINAARY PREVIEW & FILTER
    #9 TEACHING ROI
    #10 SAVE MODEL & SETUP DIALOG
    #11 PATTERN MATCHING
    #12 AUTO RUN
    #13 WCF & FSM
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
            //#14_LOGFORM#1 log4net 설정 파일을 읽어들임
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            SLogger.Write("Logger initialized!", SLogger.LogType.Info);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //#1_BASE FRAME#1 시작할 Form을 MainForm으로 변경
            Application.Run(new MainForm());
        }
    }
}
