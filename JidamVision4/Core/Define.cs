using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision4.Core
{
    //#10_INSPWINDOW#1 검사 알고리즘 타입 추가
    public enum InspectType
    {
        InspNone = -1,
        InspBinary,
        InspMatch,
        InspFilter,
        InspAIModule,
        InspCount
    }

    //#10_INSPWINDOW#4 InspWindow 정의
    public enum InspWindowType
    {
        None = 0,
        Base,
        Body,
        Sub,
        ID
    }

    public enum DecisionType
    {
        None = 0,
        Good,           //양품
        Defect,         //불량
        Info,
        Error,          //오류
        Timeout         //타임아웃
    }

    //#17_WORKING_STATE#1 작업 상태 정의

    public enum WorkingState
    {
        NONE = 0,
        INSPECT,
        LIVE,
        ALARM
    }

    public static class Define
    {
        //# SAVE ROI#4 전역적으로, ROI 저장 파일명을 설정
        //Define.cs 클래스 생성 먼저 할것
        public static readonly string ROI_IMAGE_NAME = "RoiImage.png";

        public static readonly string PROGRAM_NAME = "JidamVision";
    }
}
