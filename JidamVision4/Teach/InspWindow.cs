using JidamVision4.Algorithm;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using JidamVision4.Core;
using JidamVision4.Inspect;

namespace JidamVision4.Teach
{
    /*
    #10_INSPWINDOW# - <<<검사 ROI>>> 
    검사할 영역을 정의하는 클래스로, 검사 알고리즘을 포함하고 있다.
    1) Teach / InspWindow 클래스 생성 - 검사 영역을 정의하는 클래스
    2) Teach / InspWindowFactory 클래스 생성 - InspWindow 객체를 생성하는 팩토리 클래스
    3) Teach / Model 클래스 생성 - InspWindowList를 관리하는 클래스로, 검사를 위한 모델 정보를 저장한다.
    4) Teach / DiagramEntity 클래스 생성 - InspWindow를 ImageViewCtrl에 표시하기 위한 클래스
    5) #10_INSPWINDOW#1~24 구현
    */

    public class InspWindow
    {
        public InspWindowType InspWindowType { get; set; }

        public string Name { get; set; }
        public string UID { get; set; }

        public Rect WindowArea { get; set; }
        public Rect InspArea { get; set; }
        public bool IsTeach { get; set; } = false;

        //#12_MODEL SAVE#5 Xml Serialize를 위해서, Element을 명확하게 알려줘야 함        
        [XmlElement("InspAlgorithm")]
        public List<InspAlgorithm> AlgorithmList { get; set; } = new List<InspAlgorithm>();

        //#13_INSP_RESULT#1 검사 결과를 저장하기 위한 리스트
        public List<InspResult> InspResultList { get; set; } = new List<InspResult>();

        //#12_MODEL SAVE#6 Xml Serialize를 하지 않도록 설정
        [XmlIgnore]
        public List<Mat> _windowImages = new List<Mat>();
        public void AddWindowImage(Mat image)
        {
            if (image is null)
                return;

            _windowImages.Add(image.Clone());
        }

        public void ResetWindowImages()
        {
            _windowImages.Clear();
        }

        public void SetWindowImage(Mat image, int index)
        {
            if (image is null)
                return;

            if (index < 0 || index >= _windowImages.Count)
                return;

            _windowImages[index] = image.Clone();
        }

        public void DelWindowImage(int index)
        {
            if (index < 0 || index >= _windowImages.Count)
                return;

            _windowImages.RemoveAt(index);

            IsPatternLearn = false;
            PatternLearn();
        }

        public bool IsPatternLearn { get; set; } = false;


        public InspWindow()
        {
        }

        public InspWindow(InspWindowType windowType, string name)
        {
            InspWindowType = windowType;
            Name = name;
        }

        public InspWindow Clone(OpenCvSharp.Point offset, bool includeChildren = true)
        {
            InspWindow cloneWindow = InspWindowFactory.Inst.Create(this.InspWindowType, false);
            cloneWindow.WindowArea = this.WindowArea + offset;
            cloneWindow.IsTeach = false;

            cloneWindow.IsPatternLearn = false;

            foreach (var img in _windowImages.ToList())
                cloneWindow._windowImages.Add(img?.Clone());

            foreach (InspAlgorithm algo in AlgorithmList)
            {
                var cloneAlgo = algo.Clone();
                cloneWindow.AlgorithmList.Add(cloneAlgo);
            }

            return cloneWindow;
        }

        //#11_MATCHING#2 InspWindow에 있는 템플릿 이미지를 MatchAlgorithm에 등록하는 함수
        public bool PatternLearn()
        {
            if (IsPatternLearn == true)
                return true;

            foreach (var algorithm in AlgorithmList)
            {
                if (algorithm.InspectType != InspectType.InspMatch)
                    continue;

                MatchAlgorithm matchAlgo = (MatchAlgorithm)algorithm;
                matchAlgo.ResetTemplateImages();

                for (int i = 0; i < _windowImages.Count; i++)
                {
                    Mat tempImage = _windowImages[i];
                    if (tempImage is null)
                        continue;

                    if (tempImage.Type() == MatType.CV_8UC3)
                    {
                        Mat grayImage = new Mat();
                        Cv2.CvtColor(tempImage, grayImage, ColorConversionCodes.BGR2GRAY);
                        matchAlgo.AddTemplateImage(grayImage);
                    }
                    else
                    {
                        matchAlgo.AddTemplateImage(tempImage);
                    }
                }
            }

            IsPatternLearn = true;

            return true;
        }

        //#ABSTRACT ALGORITHM#10 타입에 따라 알고리즘을 추가하는 함수
        public bool AddInspAlgorithm(InspectType inspType)
        {
            InspAlgorithm inspAlgo = null;

            switch (inspType)
            {
                case InspectType.InspBinary:
                    inspAlgo = new BlobAlgorithm();
                    break;

                //#11_MATCHING#3 패턴매칭 알고리즘 추가
                case InspectType.InspMatch:
                    inspAlgo = new MatchAlgorithm();
                    break;
            }

            if (inspAlgo is null)
                return false;

            AlgorithmList.Add(inspAlgo);

            return true;
        }

        //알고리즘을 리스트로 관리하므로, 필요한 타입의 알고리즘을 찾는 함수
        public InspAlgorithm FindInspAlgorithm(InspectType inspType)
        {
            return AlgorithmList.Find(algo => algo.InspectType == inspType);
        }

        //클래스 내에서, 인자로 입력된 타입의 알고리즘을 검사하거나,
        ///모든 알고리즘을 검사하는 옵션을 가지는 검사 함수
        public virtual bool DoInpsect(InspectType inspType)
        {
            foreach (var inspAlgo in AlgorithmList)
            {
                if (inspAlgo.InspectType == inspType || inspType == InspectType.InspNone)
                    inspAlgo.DoInspect();
            }

            return true;
        }

        public bool IsDefect()
        {
            foreach (InspAlgorithm algo in AlgorithmList)
            {
                if (!algo.IsInspected)
                    continue;

                if (algo.IsDefect)
                    return true;
            }
            return false;
        }

        public virtual bool OffsetMove(OpenCvSharp.Point offset)
        {
            Rect windowRect = WindowArea;
            windowRect.X += offset.X;
            windowRect.Y += offset.Y;
            WindowArea = windowRect;
            return true;
        }

        public bool SetInspOffset(OpenCvSharp.Point offset)
        {
            InspArea = WindowArea + offset;
            AlgorithmList.ForEach(algo => algo.InspRect = algo.TeachRect + offset);
            return true;
        }

        //#12_MODEL SAVE#1 InspWindow가 가지고 있는 이미지를 모델 폴더에 저장과 로딩
        public virtual bool SaveInspWindow(Model curModel)
        {
            if (curModel is null)
                return false;

            string imgDir = Path.Combine(Path.GetDirectoryName(curModel.ModelPath), "Images");
            if (!Directory.Exists(imgDir))
            {
                Directory.CreateDirectory(imgDir);
            }

            for (int i = 0; i < _windowImages.Count; i++)
            {
                Mat img = _windowImages[i];
                if (img is null)
                    continue;

                string targetPath = Path.Combine(imgDir, $"{UID}_{i}.png");
                Cv2.ImWrite(targetPath, img);
            }

            return true;
        }

        public virtual bool LoadInspWindow(Model curModel)
        {
            if (curModel is null)
                return false;

            string imgDir = Path.Combine(Path.GetDirectoryName(curModel.ModelPath), "Images");

            foreach (InspAlgorithm algo in AlgorithmList)
            {
                if (algo is null)
                    continue;

                if (algo.InspectType == InspectType.InspMatch)
                {
                    MatchAlgorithm matchAlgo = algo as MatchAlgorithm;

                    int i = 0;
                    while (true)
                    {
                        string targetPath = Path.Combine(imgDir, $"{UID}_{i}.png");
                        if (!File.Exists(targetPath))
                            break;

                        Mat windowImage = Cv2.ImRead(targetPath);
                        if (windowImage != null)
                        {
                            AddWindowImage(windowImage);
                        }

                        i++;
                    }
                    IsPatternLearn = false;
                }
            }

            return true;
        }

        //#13_INSP_RESULT#2 검사 결과를 초기화 및 추가 함수
        public void ResetInspResult()
        {
            foreach (var algorithm in AlgorithmList)
            {
                algorithm.ResetResult();
            }

            InspResultList.Clear();
        }

        public void AddInspResult(InspResult inspResult)
        {
            InspResultList.Add(inspResult);
        }
    }
}
