using JidamVision4.Core;
using JidamVision4.Util;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace JidamVision4.Algorithm
{ /*
    #11_MATCHING# - <<<템플릿 매칭 구현>>> 
    템플릿 매칭 클래스를 구현하고, 매칭에 필요한 이미지 연동
    1) MatchAlgorithm 클래스 생성 및 구현, InspAlgorithm 상속 받기
    2) UIControl / PatternImageEditor 클래스 구현 - 템플릿 이미지 편집을 위한 컨트롤
    3) Propety / MatchInspProp UserControl 생성 - 템플릿 매칭 속성 편집을 위한 컨트롤
    4) #11_MATCHING#1 ~ 9 구현
    */

    public class MatchAlgorithm : InspAlgorithm
    {
        //#12_MODEL SAVE#8 Xml 이미지는 Serialize 하지 않도록 설정
        [XmlIgnore]
        private List<Mat> _templateImages = new List<Mat>();

        //찾을 이미지의 매칭율
        public int MatchScore { get; set; } = 60;
        //입력된 이미지에서 실제로 검색할 영역 설정, 속도 향상을 위해,
        //입력된 ROI 기준으로 ExtSize만큼 확장하여, 그 영역에서 찾음
        public Size ExtSize { get; set; } = new Size(100, 100);

        public bool InvertResult { get; set; } = false; // 결과 반전 여부

        //매칭이 설공했을때, 결과 매칭율
        public int OutScore { get; set; } = 0;
        //찾은 위치
        public Point OutPoint { get; set; } = new Point(0, 0);

        public List<Point> OutPoints { get; set; } = new List<Point>();

        //템플릿 매칭으로 찾고 싶은 갯수
        public int MatchCount { get; set; } = 1;

        private int _scanStep = 8; // 검색 간격 (SCAN 값)

        public MatchAlgorithm()
        {
            //#ABSTRACT ALGORITHM#2 각 함수마다 자신의 알고리즘 타입 설정
            InspectType = InspectType.InspMatch;
        }

        public override InspAlgorithm Clone()
        {
            var cloneAlgo = new MatchAlgorithm();
            CopyBaseTo(cloneAlgo);
            cloneAlgo.MatchScore = this.MatchScore;
            cloneAlgo.ExtSize = this.ExtSize;
            cloneAlgo.InvertResult = this.InvertResult;
            cloneAlgo.MatchCount = this.MatchCount;
            return cloneAlgo;
        }

        public override bool CopyFrom(InspAlgorithm sourceAlgo)
        {
            MatchAlgorithm matchAlgo = (MatchAlgorithm)sourceAlgo;

            this.MatchScore = matchAlgo.MatchScore;
            this.ExtSize = matchAlgo.ExtSize;
            this.InvertResult = matchAlgo.InvertResult;
            this.MatchCount = matchAlgo.MatchCount;

            return true;
        }

        public void AddTemplateImage(Mat templateImage)
        {
            _templateImages.Add(templateImage.Clone());
        }

        public void ResetTemplateImages()
        {
            _templateImages.Clear();
        }

        public List<Mat> GetTemplateImages()
        {
            return _templateImages;
        }

        /// <summary>
        /// 하나의 최적 매칭 위치만 찾기
        /// </summary>
        public bool MatchTemplateSingle(Mat image, Point leftTopPos)
        {
            if (_templateImages.Count <= 0)
                return false;

            Mat result = new Mat();

            double maxScore = 0;
            Point maxLoc = new Point();

            for (int i = 0; i < _templateImages.Count; i++)
            {
                // 템플릿 매칭 수행
                Cv2.MatchTemplate(image, _templateImages[i], result, TemplateMatchModes.CCoeffNormed);

                // 가장 높은 점수 위치 찾기
                Cv2.MinMaxLoc(result, out _, out double value, out _, out Point loc);

                if (value > maxScore)
                {
                    maxScore = value;
                    maxLoc = loc;
                }
            }

            OutScore = (int)(maxScore * 100);
            OutPoint = maxLoc + leftTopPos;

            SLogger.Write($"최적 매칭 위치: {maxLoc}, 신뢰도: {maxScore:F2}");

            return true;
        }

        /// <summary>
        /// 여러 개의 매칭 위치 찾기 (임계값 이상인 경우)
        /// </summary>
        public int MatchTemplateMultiple(Mat image, Point leftTopPos, out List<Point> matchedPositions)
        {
            if (_templateImages.Count <= 0)
            {
                matchedPositions = new List<Point>();
                return 0;
            }

            matchedPositions = new List<Point>();
            float matchThreshold = MatchScore / 100.0f;
            Mat result = new Mat();

            // 템플릿 매칭 수행 (정규화된 상관 계수 방식)
            Cv2.MatchTemplate(image, _templateImages[0], result, TemplateMatchModes.CCoeffNormed);

            List<Rect> detectedRegions = new List<Rect>();
            int templateWidth = _templateImages[0].Width;
            int templateHeight = _templateImages[0].Height;

            int halfWidth = templateWidth / 2;
            int halfHeight = templateHeight / 2;

            // 결과 행렬을 스캔 (SCAN 간격 적용)
            for (int y = 0; y < result.Rows; y += _scanStep)
            {
                for (int x = 0; x < result.Cols; x += _scanStep)
                {
                    float score = result.At<float>(y, x);

                    if (score < matchThreshold)
                        continue;

                    Point matchLoc = new Point(x, y);

                    // 기존 매칭된 위치들과 겹치는지 확인
                    bool overlaps = false;
                    foreach (var rect in detectedRegions)
                    {
                        if (rect.Contains(matchLoc))
                        {
                            overlaps = true;
                            break;
                        }
                    }
                    if (overlaps)
                        continue;

                    Point bestPoint = matchLoc;

                    // 수직 & 수평 검색 수행하여 가장 좋은 위치 찾기
                    // 수직 검색 (위->아래)
                    int indexR = bestPoint.Y;
                    bool isFindVert = false;
                    while (true)
                    {
                        indexR++;
                        if (indexR >= result.Rows)
                            break;

                        float candidateScore = result.At<float>(indexR, bestPoint.X);
                        if (score > candidateScore)
                        {
                            isFindVert = true;
                            break;
                        }
                        else
                        {
                            score = candidateScore;
                            bestPoint.Y++;
                        }
                    }

                    if (!isFindVert)
                        continue;

                    // 수평 검색 (좌->우)
                    int indexC = bestPoint.X;
                    bool isFindHorz = false;
                    while (true)
                    {
                        indexC++;
                        if (indexC >= result.Cols)
                            break;

                        float candidateScore = result.At<float>(bestPoint.Y, indexC);
                        if (score > candidateScore)
                        {
                            isFindHorz = true;
                            break;
                        }
                        else
                        {
                            score = candidateScore;
                            bestPoint.X++;
                        }
                    }

                    if (!isFindHorz)
                        continue;

                    // 매칭된 위치 리스트에 추가
                    //Point matchPos = new Point(bestPoint.X + templateWidth, bestPoint.Y + templateHeight);
                    Point matchPos = bestPoint + leftTopPos;
                    matchedPositions.Add(matchPos);
                    detectedRegions.Add(new Rect(bestPoint.X - halfWidth, bestPoint.Y - halfHeight, templateWidth, templateHeight));
                }
            }

            return matchedPositions.Count;
        }

        //매칭 알고리즘 검사 구현
        public override bool DoInspect()
        {
            ResetResult();

            OutPoint = new Point(0, 0);
            OutPoints.Clear();
            OutScore = 0;

            if (_templateImages.Count <= 0)
            {
                MessageBox.Show("티칭 이미지는 유효하지 않습니다!");
                return false;
            }

            if (_templateImages[0].Type() == MatType.CV_8UC3)
            {
                MessageBox.Show("티칭 이미지는 칼라를 허용하지 않습니다!");
                return false;
            }

            Mat srcImage = Global.Inst.InspStage.GetMat(0, ImageChannel);

            Rect ExtArea = InspRect;
            ExtArea.Inflate(ExtSize);

            if (ExtArea.X < 0) { ExtArea.X = 0; }
            if (ExtArea.Y < 0) { ExtArea.Y = 0; }
            if (ExtArea.Right > srcImage.Width) { ExtArea.Width = srcImage.Width - ExtArea.X; }
            if (ExtArea.Bottom > srcImage.Height) { ExtArea.Height = srcImage.Height - ExtArea.Y; }

            Mat targetImage = srcImage[ExtArea];

            int halfWidth = (int)(_templateImages[0].Width * 0.5f + 0.5f);
            int halfHeight = (int)(_templateImages[0].Height * 0.5f + 0.5f);

            if (MatchCount == 1)
            {
                if (MatchTemplateSingle(targetImage, ExtArea.TopLeft) == false)
                    return false;

                OutPoints.Add(OutPoint);

                Point matchPos = new Point(OutPoint.X + halfWidth, OutPoint.Y + halfHeight);
                IsDefect = (OutScore >= MatchScore) ? false : true;

                if (InvertResult)
                    IsDefect = !IsDefect;

                string defectInfo = IsDefect ? "NG" : "OK";
                string resultInfo = $"[{defectInfo}] 매칭 결과 : X {matchPos.X}, Y {matchPos.Y}, Score {OutScore}";
                ResultString.Add(resultInfo);
            }
            else
            {
                List<Point> outPoints = new List<Point>();
                int matchCount = MatchTemplateMultiple(targetImage, ExtArea.TopLeft, out outPoints);
                if (matchCount <= 0)
                    return false;

                OutPoints = outPoints;

                string resultInfo;
                resultInfo = $"[Match Result] match count : {matchCount}";
                ResultString.Add(resultInfo);

                for (int i = 0; i < matchCount; i++)
                {
                    Point pos = outPoints[i];
                    Point matchPos = new Point(pos.X + halfWidth, pos.Y + halfHeight);
                    resultInfo = $"[매칭 결과 : X {matchPos.X}, Y {matchPos.Y}";
                    ResultString.Add(resultInfo);
                }
            }

            IsInspected = true;
            return true;
        }

        public Point GetOffset()
        {
            Point offset = new Point(0, 0);

            if (IsInspected)
            {
                offset.X = OutPoint.X - InspRect.X;
                offset.Y = OutPoint.Y - InspRect.Y;
            }

            return offset;
        }

        //매칭 검사로 찾을 Rect 리스트 반환
        public override int GetResultRect(out List<DrawInspectInfo> resultArea)
        {
            resultArea = null;

            if (!IsInspected)
                return -1;

            if (_templateImages.Count <= 0)
                return -1;

            resultArea = new List<DrawInspectInfo>();

            int halfWidth = _templateImages[0].Width;
            int halfHeight = _templateImages[0].Height;

            string info = $"{OutScore}%";

            DecisionType decisionType = DecisionType.Good;
            if (IsDefect)
                decisionType = DecisionType.Defect;

            foreach (var point in OutPoints)
            {
                resultArea.Add(new DrawInspectInfo(new Rect(point.X, point.Y, _templateImages[0].Width, _templateImages[0].Height),
                    info, InspectType.InspMatch, decisionType));
            }

            return resultArea.Count;
        }
    }
}
