using JidamVision4.Algorithm;
using JidamVision4.Grab;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JidamVision4.Core
{
    /*
    #6_INSP_STAGE# - <<<비전검사를 위한 클래스 구현>>> 
    InspStage는 비전 검사 시스템의 핵심 클래스로, 
    카메라 인터페이스와 이미지 처리 기능을 통합하여 검사 프로세스를 관리합니다.
    1) ImageSpace 클래스 구현
    2) InspStage 클래스 구현
    3) Global 클래스 구현
    4) RunForm 클래스 구현
    */

    //검사와 관련된 클래스를 관리하는 클래스
    public class InspStage : IDisposable
    {
        public static readonly int MAX_GRAB_BUF = 5;

        private ImageSpace _imageSpace = null;

        //#5_CAMERA_INTERFACE#4 Dispose도 GrabModel에서 상속받아 사용
        //private HikRobotCam _grabManager = null;
        private GrabModel _grabManager = null;
        private CameraType _camType = CameraType.WebCam;
        
        SaigeAI _saigeAI; // SaigeAI 인스턴스

        //#7_BINARY_PREVIEW#1 이진화 프리뷰에 필요한 변수 선언
        BlobAlgorithm _blobAlgorithm = null; // Blob 알고리즘 인스턴스
        private PreviewImage _previewImage = null;

        public InspStage() { }
        public ImageSpace ImageSpace
        {
            get => _imageSpace;
        }

        public SaigeAI AIModule
        {
            get {
                if (_saigeAI is null)
                    _saigeAI = new SaigeAI();
                return _saigeAI;
            }                
        }

        //#7_BINARY_PREVIEW#2 이진화 알고리즘과 프리뷰 변수에 대한 프로퍼티 생성
        public BlobAlgorithm BlobAlgorithm
        {
            get => _blobAlgorithm;
        }

        public PreviewImage PreView
        {
            get => _previewImage;
        }

        public bool Initialize()
        {
            _imageSpace = new ImageSpace();

            //#7_BINARY_PREVIEW#3 이진화 알고리즘과 프리뷰 변수 인스턴스 생성
            _blobAlgorithm = new BlobAlgorithm();
            _previewImage = new PreviewImage();

            switch (_camType)
            {
                //#5_CAMERA_INTERFACE#5 타입에 따른 카메라 인스턴스 생성
                case CameraType.WebCam:
                    {
                        _grabManager = new WebCam();
                        break;
                    }
                case CameraType.HikRobotCam:
                    {
                        _grabManager = new HikRobotCam();
                        break;
                    }
            }

            if (_grabManager != null && _grabManager.InitGrab() == true)
            {
                _grabManager.TransferCompleted += _multiGrab_TransferCompleted;

                InitModelGrab(MAX_GRAB_BUF);
            }

            return true;
        }

        public void InitModelGrab(int bufferCount)
        {
            if (_grabManager == null)
                return;

            int pixelBpp = 8;
            _grabManager.GetPixelBpp(out pixelBpp);

            int inspectionWidth;
            int inspectionHeight;
            int inspectionStride;
            _grabManager.GetResolution(out inspectionWidth, out inspectionHeight, out inspectionStride);

            if (_imageSpace != null)
            {
                _imageSpace.SetImageInfo(pixelBpp, inspectionWidth, inspectionHeight, inspectionStride);
            }

            SetBuffer(bufferCount);

            //_grabManager.SetExposureTime(25000);

            //#7_BINARY_PREVIEW#9 이진화 알고리즘을 속성창에 연동하기 위한 함수 구현            
            UpdateProperty();
        }


        private void UpdateProperty()
        {
            if (BlobAlgorithm is null)
                return;

            PropertiesForm propertiesForm = MainForm.GetDockForm<PropertiesForm>();
            if (propertiesForm is null)
                return;

            propertiesForm.UpdateProperty(BlobAlgorithm);
        }

        public void SetBuffer(int bufferCount)
        {
            if (_grabManager == null)
                return;

            if (_imageSpace.BufferCount == bufferCount)
                return;

            _imageSpace.InitImageSpace(bufferCount);
            _grabManager.InitBuffer(bufferCount);

            for (int i = 0; i < bufferCount; i++)
            {
                _grabManager.SetBuffer(
                    _imageSpace.GetInspectionBuffer(i),
                    _imageSpace.GetnspectionBufferPtr(i),
                    _imageSpace.GetInspectionBufferHandle(i),
                    i);
            }
        }

        //#8_INSPECT_BINARY#19 이진화 검사 함수
        public void TryInspection()
        {
            if (_blobAlgorithm is null)
                return;

            Mat srcImage = Global.Inst.InspStage.GetMat();
            _blobAlgorithm.SetInspData(srcImage);

            _blobAlgorithm.InspRect = new Rect(0,0, srcImage.Width, srcImage.Height);

            if(_blobAlgorithm.DoInspect())
            {
                DisplayResult();
            }
        }

        //검사된 알고리즘이 가지고 있는 검사 결과 정보를 화면에 출력
        private bool DisplayResult()
        {
            if (_blobAlgorithm is null)
                return false;

            List<DrawInspectInfo> resultArea = new List<DrawInspectInfo>();
            int resultCnt = _blobAlgorithm.GetResultRect(out resultArea);
            if (resultCnt > 0)
            {
                //찾은 위치를 이미지상에서 표시
                var cameraForm = MainForm.GetDockForm<CameraForm>();
                if (cameraForm != null)
                {
                    cameraForm.ResetDisplay();
                    cameraForm.AddRect(resultArea);
                }
            }

            return true;
        }

        public void Grab(int bufferIndex)
        {
            if (_grabManager == null)
                return;

            _grabManager.Grab(bufferIndex, true);
        }

        //영상 취득 완료 이벤트 발생시 후처리
        private void _multiGrab_TransferCompleted(object sender, object e)
        {
            int bufferIndex = (int)e;
            Console.WriteLine($"_multiGrab_TransferCompleted {bufferIndex}");

            _imageSpace.Split(bufferIndex);

            DisplayGrabImage(bufferIndex);

            if (_previewImage != null)
            {
                Bitmap bitmap = ImageSpace.GetBitmap(0);
                _previewImage.SetImage(BitmapConverter.ToMat(bitmap));
            }
        }

        private void DisplayGrabImage(int bufferIndex)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateDisplay();
            }
        }

        public void UpdateDisplay(Bitmap bitmap)
        {
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateDisplay(bitmap);
            }
        }

        public Bitmap GetCurrentImage()
        {
            Bitmap bitmap = null;
            var cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                bitmap = cameraForm.GetDisplayImage();                
            }

            return bitmap;
        }

        public Bitmap GetBitmap(int bufferIndex = -1)
        {
            if (Global.Inst.InspStage.ImageSpace is null)
                return null;

            return Global.Inst.InspStage.ImageSpace.GetBitmap();
        }

        //#7_BINARY_PREVIEW#4 이진화 프리뷰를 위해, ImageSpace에서 이미지 가져오기
        public Mat GetMat()
        {
            return Global.Inst.InspStage.ImageSpace.GetMat();
        }

        //#7_BINARY_PREVIEW#5 이진화 임계값 변경시, 프리뷰 갱신
        public void RedrawMainView()
        {
            CameraForm cameraForm = MainForm.GetDockForm<CameraForm>();
            if (cameraForm != null)
            {
                cameraForm.UpdateImageViewer();
            }
        }


        #region Disposable

        private bool disposed = false; // to detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_saigeAI != null)
                    {
                        _saigeAI.Dispose();
                        _saigeAI = null;
                    }
                    if(_grabManager != null)
                    {
                        _grabManager.Dispose();
                        _grabManager = null;
                    }
                }

                // Dispose unmanaged managed resources.

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion //Disposable
    }
}
