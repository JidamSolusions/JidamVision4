using JidamVision4.Setting;
using JidamVision4.Util;
using MessagingLibrary;
using MessagingLibrary.MessageInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static JidamVision4.Util.SLogger;
using static MessagingLibrary.Message;
using System.Timers;

namespace JidamVision4.Sequence
{
    /*
    #19_VISION_SEQUENCE# - <<<비젼 - 제어 통신을 이용한 검사 시퀸스 개발>>> 
    제어(서버) - 비젼(클라이언트) 간의 통신
    1) 참조에 System.ServiceModel 추가
    2) 참조에 ExternalLib\Dll\MessagingLibrary 추가
    3) SettingXml에 통신 타입, IP 주소 추가
    4) 환경설정 / Communicator 설정창 추가
        - CommunicatorSetting UserControl 추가
    4) Sequence / Communicator 클래스 구현 -> 코드 복사
    5) Sequence / VisionSequence 클래스 구현
        - VisionSequence 클래스는 Communicator를 상속받아 구현
    6) #19_VISION_SEQUENCE# 1 ~ 7 구현
    7) 시퀀스를 위한 설정
        - 비젼 시스템과 같은 이름의 레시피 있어야함
        - 비젼 시스템의 MachineName = "VISION01" 설정(조별 번호 설정, ex : 2조 -> "VISION02")
        - 비젼 시스템에 시뮬레이터의 IP 설정 필요
    8) 시퀀스 
        a) 비젼 -> 제어 : 모델 열기 요청
        b) 비젼 -> 제어 : 전체 사이클 검사 요청
        c) 제어 -> 비젼 : 단위 검사 요청
        d) 비젼 -> 제어 : 검사 완료 및 결과 반환
    */

    //시퀀스에서 비젼 내부에, 특정 기능을 요청
    public enum SeqCmd
    {
        None = 0,
        //OpenRecipe,         //제어 -> 비젼으로 모델 열기 명령
        //InspReady,  
        InspStart,
        InspEnd,
    }

    public enum Vision2Mmi
    {
        None = 0,
        InspDone,               //비젼 -> 제어 : 검사 완료 및 결과 반환
        InspEnd,
        Error
    }


    public class VisionSequence : IDisposable
    {
        private enum VisionSeq
        {
            None = 0,
            OpenRecipe,             //비젼 -> 제어 : 모델 열기 요청
            MmiStart,                  //비젼 -> 제어 : 전체 사이클 검사 요청
            MmiStop,                  //비젼 -> 제어 : 전체 사이클 검사 정지 요청
            Error
        }

        private static VisionSequence _sequence = null;

        public static VisionSequence Inst
        {
            get
            {
                if (_sequence == null)
                {
                    _sequence = new VisionSequence();
                }
                return _sequence;
            }
        }

        #region Event
        public delegate void XEventHandler<T1, T2>(object sender, T1 e1, T2 e2);
        public event XEventHandler<SeqCmd, object> SeqCommand = delegate { };
        protected void RaiseSeqCommand(SeqCmd cmd, object param)
        {
            SeqCommand(this, cmd, param);
        }
        #endregion

        private Message _message = null;
        private Communicator _communicator = null;

        protected Thread _sequenceThread = null;
        protected bool _isRun = true;
        private VisionSeq _visionState = VisionSeq.None;

        protected Stopwatch _stopwatch = new Stopwatch();

        protected string _modelName = "";

        protected string _lastErrMsg;

        protected bool _mmiOpenRecipe = false;

        protected bool IsMmiConnected { get; set; } = false;

        protected bool _rtyconnect = false;

        protected System.Timers.Timer _timerReConnect = new System.Timers.Timer();

        public VisionSequence()
        {
            _rtyconnect = false;
        }
        ~VisionSequence()
        {
            _rtyconnect = false;
        }

        private bool InitCommunicator()
        {
            if (SettingXml.Inst.CommType == CommunicatorType.WCF)
            {
                SLogger.Write("WCF 통신 초기화!");

                string ipAddr = SettingXml.Inst.CommIP;

                _timerReConnect.Interval = 5000;
                _timerReConnect.Elapsed += _timerReConnect_Elapsed;
                _timerReConnect.Stop();
                _rtyconnect = true;

                #region WCF

                _communicator = new Communicator();
                _communicator.ReceiveMessage += Communicator_ReceiveMessage;
                _communicator.Closed += Communicator_Closed;
                _communicator.Opened += Communicator_Opened;

                _communicator.Create(CommunicatorType.WCF, ipAddr);

                if (_communicator.State == System.ServiceModel.CommunicationState.Opened)
                    _communicator.SendMachineInfo();

                if (_communicator.State != System.ServiceModel.CommunicationState.Opened)
                {
                    SLogger.Write("MMI 연결 실패!", SLogger.LogType.Error);
                    return false;
                }

                #endregion WCF
            }
            else
            {
                return false;
            }

            return true;
        }

        virtual public void InitSequence()
        {
            if (!InitCommunicator())
                return;

            //통신 초기화
            //통신 이벤트 등록
            if (_message is null)
                _message = new Message();

            _message.MachineName = SettingXml.Inst.MachineName;

            _sequenceThread = new Thread(SequenceThread);
            _sequenceThread.IsBackground = true;
            _sequenceThread.Start();
        }

        public void ResetCommunicator(Communicator communicator)
        {
            if (_communicator is null)
                return;

            _communicator.ReceiveMessage -= Communicator_ReceiveMessage;
            _communicator = communicator;
            _communicator.ReceiveMessage += Communicator_ReceiveMessage;
        }

        private bool SendMessage(MmiMessageInfo message)
        {
            if (_communicator is null)
                return false;

            _message.Time = string.Format($"{DateTime.Now:HH:mm:ss:fff}");
            return _communicator.SendMessage(message);
        }

        protected void SequenceThread()
        {
            while (_isRun)
            {
                UpdateSeqState();
                Thread.Sleep(1);
            }
        }

        //#WCF_FSM#2 비젼 -> 제어에 자동 검사 요청
        virtual public void StartAutoRun(string modelName)
        {
            _visionState = VisionSeq.OpenRecipe;
            _modelName = modelName;
        }

        //#WCF_FSM#8 비젼 -> 제어에 자동 검사 정지 요청
        public void StopAutoRun()
        {
            _visionState = VisionSeq.MmiStop;
        }
        public virtual void ResetAlarm()
        {
        }

        virtual protected void UpdateSeqState()
        {
            switch (_visionState)
            {
                case VisionSeq.None:
                    {
                    }
                    break;
                case VisionSeq.OpenRecipe:
                    {
                        //#WCF_FSM#3 비젼 -> 제어에 모델 열기 요청

                        if (_modelName == "")
                        {
                            _visionState = VisionSeq.None;
                            break;
                        }

                        SLogger.Write("Vision Seq : " + _visionState.ToString());
                        _mmiOpenRecipe = false;
                        //OpenRecipe 명령 전달
                        _message.Command = Message.MessageCommand.OpenRecipe;
                        //_modelName 모델명 전달
                        _message.Tool = _modelName;
                        _message.Status = CommandStatus.None;
                        _message.ErrorMessage = "";
                        SendMessage(_message);

                        _visionState = VisionSeq.MmiStart;
                    }
                    break;
                case VisionSeq.MmiStart:
                    {
                        if (!_mmiOpenRecipe)
                            break;

                        SLogger.Write("Vision Seq : " + _visionState.ToString());

                        //#WCF_FSM#3 비젼 -> 제어에 전체 검사 요청
                        _message.Command = Message.MessageCommand.MmiStart;
                        _message.Status = CommandStatus.None;
                        _message.ErrorMessage = "";
                        SendMessage(_message);

                        _visionState = VisionSeq.None;
                    }
                    break;
                case VisionSeq.MmiStop:
                    {
                        //#WCF_FSM#9 제어에 자동 검사 정지 요청
                        SLogger.Write("Vision Seq : " + _visionState.ToString());

                        _message.Command = Message.MessageCommand.MmiStop;
                        _message.Status = CommandStatus.None;
                        _message.ErrorMessage = "";
                        SendMessage(_message);

                        _visionState = VisionSeq.None;
                    }
                    break;
                case VisionSeq.Error:
                    {
                        _message.Command = Message.MessageCommand.Error;
                        _message.Status = CommandStatus.Fail;
                        _message.ErrorMessage = _lastErrMsg;
                        SendMessage(_message);

                        _visionState = VisionSeq.None;
                    }
                    break;
            }
        }

        private void Communicator_ReceiveMessage(object sender, Message e)
        {
            switch (e.Command)
            {
                case Message.MessageCommand.Reset:
                    {
                        ResetSequence();
                    }
                    break;
                case Message.MessageCommand.OpenRecipe:
                    {
                        if (e.Status == Message.CommandStatus.Success)
                        {
                            //비젼의 요청에 의해, OpenRecipe가 성공한 경우
                            _mmiOpenRecipe = true;
                            break;
                        }
                        else
                        {
                            //Mmi에서 비젼에, OpenRecipe를 요청한 경우
                            //SeqCommand(this, SeqCmd.OpenRecipe, (object)e.Tool);
                        }
                    }
                    break;
                case Message.MessageCommand.MmiStart:
                    {
                        if (e.Status == Message.CommandStatus.Success)
                        {
                            //비젼의 요청에 의해, MmiStart가 성공한 경우
                            break;
                        }
                    }
                    break;
                case Message.MessageCommand.MmiStop:
                    {
                        if (e.Status == Message.CommandStatus.Success)
                        {
                            //비젼의 요청에 의해, MmiStop가 성공한 경우
                            break;
                        }
                    }
                    break;
                case Message.MessageCommand.InspStart:
                    {
                        //#WCF_FSM#4 제어 -> 비젼으로 검사 시작 요청
                        RaiseSeqCommand(SeqCmd.InspStart, e);
                    }
                    break;
                case Message.MessageCommand.InspEnd:
                    {
                        RaiseSeqCommand(SeqCmd.InspEnd, e);
                    }
                    break;
            }
        }

        //비젼 -> MMI에게 Command 전송
        virtual public void VisionCommand(Vision2Mmi visionCmd, Object e)
        {
            switch (visionCmd)
            {
                // case Vision2Mmi.ModeLoaded:
                //    {
                //        string errMsg = (string)e;
                //        if (errMsg != "")
                //        {
                //            _lastErrMsg = errMsg;
                //            SendError();
                //            break;
                //        }

                //        _message.Command = Message.MessageCommand.OpenRecipe;
                //        _message.Status = CommandStatus.Success;
                //        SendMessage(_message);
                //    }
                //    break;
                //case Vision2Mmi.InspReady:
                //    {
                //        string errMsg = (string)e;
                //        if (errMsg != "")
                //        {
                //            _lastErrMsg = errMsg; 
                //            SendError();
                //            break;
                //        }

                //        _message.Command = Message.MessageCommand.InspReady;
                //        _message.Status = CommandStatus.Success;
                //        SendMessage(_message);
                //    }
                //    break;
                case Vision2Mmi.InspDone:
                    {
                        //#WCF_FSM#7 제어에 Ng/Good 결과를 담아 검사 완료 명령 전송
                        bool isDefect = (bool)e;

                        _message.Command = Message.MessageCommand.InspDone;
                        _message.Status = isDefect ? CommandStatus.Ng : CommandStatus.Good;
                        SendMessage(_message);
                    }
                    break;
            }
        }

        private void SendError()
        {
            _message.Command = Message.MessageCommand.Error;
            _message.Status = CommandStatus.Fail;
            _message.ErrorMessage = _lastErrMsg;
            SendMessage(_message);
        }

        protected void ResetSequence()
        {
            _visionState = VisionSeq.None;
        }

        private void _timerReConnect_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timerReConnect.Stop();
            if (_rtyconnect)
            {
                string machineName = SettingXml.Inst.MachineName;
                _communicator.Create(CommunicatorType.WCF, SettingXml.Inst.CommIP);
                if (_communicator.State == System.ServiceModel.CommunicationState.Opened)
                {
                    _communicator.SendMachineInfo();
                    SLogger.Write("WCF " + machineName + " : Opened", LogType.Info);
                    IsMmiConnected = true;
                }
                else
                {
                    _timerReConnect.Start();
                }
            }
        }

        private void Communicator_Opened(object sender, EventArgs e)
        {
            SLogger.Write($"MMI에 연결되었습니다.");
            IsMmiConnected = true;
        }

        private void Communicator_Closed(object sender, EventArgs e)
        {
            _timerReConnect.Start();
            SLogger.Write("서버와의 연결이 끊어졌습니다.", SLogger.LogType.Error);
            IsMmiConnected = false;
        }

        #region Disposable
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _isRun = false;
                    if (_communicator != null)
                    {
                        _communicator.ReceiveMessage -= Communicator_ReceiveMessage;
                        _communicator.Opened -= Communicator_Opened;
                        _communicator.Closed -= Communicator_Closed;
                        _communicator = null;
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion //Disposable
    }
}
