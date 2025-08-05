using JidamVision4.Algorithm;
using JidamVision4.Core;
using JidamVision4.Teach;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JidamVision4.Inspect
{
    /*
   #13_INSP_RESULT# - <<<검사결과를 보기 위해, 트리 리스트 뷰에 출력하는 기능 개발>>> 
   검사 결과를 저장하고, ResultForm에 표시하기 위한 클래스
   1) Inspect / InspResult 클래스 생성 - 검사 결과를 저장하는 클래스
   2) ResultForm WinForm 클래스 생성 - 검사 결과를 표시하는 폼
   3) 검사 결과를 출력하기 위한 TreeListView 컨트롤을 사용하기 위해, 
         ExternalLib\Dll\ObjectListView\ObjectListView.dll을 참조에 추가할것
         using BrightIdeasSoftware; 코드 추가
   4) ResultForm에 TreeListView와 자세한 결과를 보여주는 TextBox, 
         2개 컨트롤을 상하로 분리하는 SplitContainer 추가    
   5) #13_INSP_RESULT#1 ~
   */

    //검사 결과를 저장하기 위한 클래스
    public class InspResult
    {
        //검사한 ROI 정보
        public InspWindow InspObject { get; set; }
        //ROI가 여러개 있을 때, 기준이 되는 ROI
        public string GroupID { get; set; }
        //실제 검사한 ROI
        public string ObjectID { get; set; }

        //검사한 ROI의 타입
        public InspWindowType ObjectType { get; set; }

        //검사한 알고리즘 타입
        public InspectType InspType { get; set; }
        //검사 결과 코드
        public int ErrorCode { get; set; }
        //결과가 불량인지 여부
        public bool IsDefect { get; set; }
        //결과 값(점수가 아닌 실제 값)
        public string ResultValue { get; set; }
        //세부적인 검사 결과
        public string ResultInfos { get; set; }

        //검사 결과로 찾은 불량 위치
        [XmlIgnore]
        public List<DrawInspectInfo> ResultRectList { get; set; } = null;

        public InspResult()
        {
            InspObject = new InspWindow();
            GroupID = string.Empty;
            ObjectID = string.Empty;
            ObjectType = InspWindowType.None;
            ErrorCode = 0;
            IsDefect = false;
            ResultValue = "";
            ResultInfos = string.Empty;
        }

        public InspResult(InspWindow window, string baseID, string objectID, InspWindowType objectType)
        {
            InspObject = window;
            GroupID = baseID;
            ObjectID = objectID;
            ObjectType = objectType;
            ErrorCode = 0;
            IsDefect = false;
            ResultValue = "";
            ResultInfos = string.Empty;
        }
    }
}
