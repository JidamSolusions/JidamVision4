using JidamVision4.Core;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JidamVision4.Algorithm
{
    //#12_MODEL SAVE#7 Xml Serialize를 위해서, 아래 코드 추가
    //XmlSerialize는 추상화된 상태를 알수 없어, 상속된 클래스를 명시적으로 포함해야 함.
    [XmlInclude(typeof(MatchAlgorithm))]
    [XmlInclude(typeof(BlobAlgorithm))]
    public abstract class InspAlgorithm
    { 
        //알고리즘 타입 정의
        public InspectType InspectType { get; set; } = InspectType.InspNone;

        //알고지즘을 사용할지 여부 결정
        public bool IsUse { get; set; } = true;
        //검사가 완료되었는지를 판단
        public bool IsInspected { get; set; } = false;

        //#8_INSPECT_BINARY#1 검사할 영역 정보를 저장하는 변수
        public Rect TeachRect { get; set; }
        public Rect InspRect { get; set; }

        public eImageChannel ImageChannel { get; set; } = eImageChannel.Gray;

        //검사할 원본 이미지
        protected Mat _srcImage = null;

        //검사 결과 정보
        public List<string> ResultString { get; set; } = new List<string>();

        //불량 여부
        public bool IsDefect { get; set; }

        //#10_INSPWINDOW#2 InspWindow 복사를 위한 InspAlgorithm 복사 함수
        public abstract InspAlgorithm Clone();
        public abstract bool CopyFrom(InspAlgorithm sourceAlgo);

        /// <summary>자식 클래스에서 공통 필드를 복사하려고 부르는 헬퍼</summary>
        protected void CopyBaseTo(InspAlgorithm target)
        {
            target.InspectType = this.InspectType;
            target.IsUse = this.IsUse;
            target.IsInspected = this.IsInspected;
            target.TeachRect = this.TeachRect;
            target.InspRect = this.InspRect;
            target.ImageChannel = this.ImageChannel;
            // NOTE: _srcImage 는 런타임 검사용이라 복사하지 않음
        }

        //#8_INSPECT_BINARY#2 검사할 이미지 정보 저장
        public virtual void SetInspData(Mat srcImage)
        {
            _srcImage = srcImage;
        }

        //검사 함수로, 상속 받는 클래스는 필수로 구현해야한다.
        public abstract bool DoInspect();

        //검사 결과 정보 초기화
        public virtual void ResetResult()
        {
            IsInspected = false;
            IsDefect = false;
            ResultString.Clear();
        }

        //#8_INSPECT_BINARY#3 검사 결과가 Rect정보로 출력이 가능하다면, 이 함수를 상속 받아서, 정보 반환
        public virtual int GetResultRect(out List<DrawInspectInfo> resultArea)
        {
            resultArea = null;
            return 0;
        }
    }
}
