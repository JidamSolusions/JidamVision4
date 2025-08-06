using System;
using log4net;
using static JidamVision4.Util.SLogger;

namespace JidamVision4.Util
{
    //log4net을 이용한 로그 저장 클래스
    public static class SLogger
    {
        //로그의 종류, 필요시 추가 가능
        public enum LogType
        {
            Info,
            Error,
            Debug
        }

        //로그 기록을 위한 인터페이스
        private static readonly ILog _log = LogManager.GetLogger(nameof(_log));

        // 로그가 추가될 때 발생하는 이벤트
        public static event Action<string> LogUpdated;

        static SLogger()
        {
        }

        // 로그를 기록하고 이벤트 발생
        public static void Write(string message, LogType type = LogType.Info)
        {
            //로그 파일에는 자동으로 시간이 기록되므로, 타입과 메세지만 기록
            string logMessage = $"[{type}] {message}";

            // 파일 로그 기록
            switch (type)
            {
                case LogType.Error:
                    _log.Error(logMessage);
                    break;
                case LogType.Debug:
                    _log.Debug(logMessage);
                    break;
                default:
                    _log.Info(logMessage);
                    break;
            }

            // UI에 시간이 표시되도록함
            logMessage = $"[{DateTime.Now:MM-dd HH:mm:ss}] {logMessage}";

            // 로그가 추가되었음을 알림
            LogUpdated?.Invoke(logMessage);
        }
    }
}
