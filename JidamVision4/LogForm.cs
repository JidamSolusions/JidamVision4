using JidamVision4.Core;
using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using JidamVision4.Util;

namespace JidamVision4
{
    /*
    #14_LOGFORM# - <<<로그 저장하는 기능 개발>>> 
    프로그램에서 일어나는 이벤트를 로그로 저장하는 기능
    리스트 박스에 로그로 출력하면서, 로그 파일로 저장함
    1) log4net 라이브러리 참조 추가하기 : ExternalLib\Dll\Log4Net\log4net.dll
    2) log4net.config 파일을 프로젝트 내에 추가하기 : ExternalLib\Dll\Log4Net\log4net.config
        파일 속성에서 [출력 디렉터리에 복사]를 "항상 복사"로 설정
    3) Util/SLogger.cs : 로그를 저장하는 클래스 생성
    4) LogForm 클래스에 listBox 컨트롤 추가
    5) #14_LOGFORM#1 ~ 2
    6) "SLogger.Write" 검색해서, 추가하기
    */

    public partial class LogForm : DockContent
    {
        public LogForm()
        {
            InitializeComponent();

            //폼이 닫힐 때 이벤트 제거를 위해 이벤트 추가
            this.FormClosed += LogForm_FormClosed;
            //로그가 추가될 때 이벤트 추가
            SLogger.LogUpdated += OnLogUpdated;
        }

        //로그 이벤트 발생시, 리스트박스에 로그 추가 함수 호출
        private void OnLogUpdated(string logMessage)
        {
            //UI 스레드가 아닐 경우, Invoke()를 호출하여 UI 스레드에서 실행되도록 강제함
            if (listBoxLogs.InvokeRequired)
            {
                listBoxLogs.Invoke(new Action(() => AddLog(logMessage)));
            }
            else
            {
                AddLog(logMessage);
            }
        }

        //리스트박스에 로그 추가
        private void AddLog(string logMessage)
        {
            //로그가 1000개 이상이면, 가장 오래된 로그를 삭제
            if (listBoxLogs.Items.Count > 1000)
            {
                listBoxLogs.Items.RemoveAt(0);
            }
            listBoxLogs.Items.Add(logMessage);

            //자동 스크롤
            listBoxLogs.TopIndex = listBoxLogs.Items.Count - 1;
        }

        //폼이 닫힐 때 이벤트 제거
        private void LogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SLogger.LogUpdated -= OnLogUpdated;
            this.FormClosed -= LogForm_FormClosed;
        }
    }
}
