using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JidamVision4.Core;

namespace JidamVision4.Algorithm
{
    public class DrawInspectInfo
    {
        public Rect rect;
        public Point2f[] rotatedPoints;
        public string info;
        public InspectType inspectType;
        public DecisionType decision;
        public bool UseRotatedRect = false; // 회전된 Rect를 사용할지 여부

        public DrawInspectInfo()
        {
            rect = new Rect();
            rotatedPoints = null;
            info = string.Empty;
            inspectType = InspectType.InspNone;
            decision = DecisionType.None;
        }

        public DrawInspectInfo(Rect _rect, string _info, InspectType _inspectType, DecisionType _decision)
        {
            rect = _rect;
            info = _info;
            inspectType = _inspectType;
            decision = _decision;
        }

        public void SetRotatedRectPoints(Point2f[] _rotatedPoints)
        {
            if (_rotatedPoints is null)
                return;

            rotatedPoints = new Point2f[_rotatedPoints.Length];
            for (int i = 0; i < _rotatedPoints.Length; i++)
            {
                rotatedPoints[i] = _rotatedPoints[i]; // Point2f는 구조체이므로 값 복사됨
            }
            UseRotatedRect = true;
        }
    }
}
