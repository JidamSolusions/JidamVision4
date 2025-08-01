using OpenCvSharp;
using SaigeVision.Net.V2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JidamVision4.Core;

namespace JidamVision4.Algorithm
{
    /*
   #7_BINARY_PREVIEW# - <<<이진화 프리뷰 구현>>> 
   이진화 검사를 위한 프리뷰를 R,G,B,Mono 별로 보여주는 기능 구현
    비젼검사를 위한 알고리즘 클래스를 구성하고, 그 안에서 이진화 임계값을 사용해 프리뷰 구현
   1) Algorithm / InspAlgorithm 클래스 생성 - 검사 알고리즘을 위한 추상화 클래스
   2) Algorithm / BlobAlgorithm 클래스 생성 - InspAlgorithm를 상속 받아, 이진화 검사를 위한 클래스
   3) Core / PreviewImage 클래스 생성 - 이진화 프리뷰를 구현하는 클래스
   4) #7_BINARY_PREVIEW#1~10까지 구현
   */

    //이진화 임계값 설정을 구조체로 만들기
    public struct BinaryThreshold
    {
        public int lower { get; set; }
        public int upper { get; set; }
        public bool invert { get; set; }
    }
    
    public class BlobAlgorithm : InspAlgorithm
    {
        public BinaryThreshold BinThreshold { get; set; } = new BinaryThreshold();

        public BlobAlgorithm()
        {
            InspectType = InspectType.InspBinary;
        }

        //InspAlgorithm을 상속받아, 구현하고, 인자로 입력받던 것을 부모의 _srcImage 이미지 사용
        //검사 시작전 IsInspected = false로 초기화하고, 검사가 정상적으로 완료되면,IsInspected = true로 설정
        public override bool DoInspect()
        {
            ResetResult();
            
            IsInspected = true;

            return true;
        }

        //검사 결과 초기화
        public override void ResetResult()
        {
            base.ResetResult();
        }
    }
}
