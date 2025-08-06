using JidamVision4.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JidamVision4.Grab
{
    /*
   #6_CAMERA_ABSTRACT# - <<<카메라 인터페이스 추상화>>> 
   HikRobotCam 클래스에서 카메라의 일반적인 기능(함수)를 GrabModel 클래스를 만들어 추상화하고,
    WebCam을 추가로 구현하여, 외부에서는 같은 함수를 호출하지만, 옵션에 따라 카메라를 동작하도록 구현
   1) Grab 폴더 밑에 GrabModel 클래스 추가
   2) HikRobotCam 함수 중에, 공용으로 사용할 수 있는 함수를 GrabModel로 이동
   3) HikRobotCam 클래스는 GrabModel로 부터 상속 받도록 변경하고, 상속받은 함수를 오버라이드 하여 구현
   4) WebCam 클래스를 추가하고  , GrabModel 클래스를 상속받아 구현
   5) #5_CAMERA_INTERFACE#1~5 코드 구현
   */

    public enum CameraType
    {
        [Description("사용안함")]
        None = 0,
        [Description("웹캠")]
        WebCam,
        [Description("HikRobot 카메라")]
        HikRobotCam
    }

    struct GrabUserBuffer
    {
        //실제 이미지 데이터를 보관하는 배열(메모리 내 이미지 처리, 파일 읽기)
        private byte[] _imageBuffer;
        //네이티브 코드에 넘기기 위한 포인터(PInvoke, OpenCVSharp, Native SDK)
        private IntPtr _imageBufferPtr;
        //배열을 고정시켜 포인터 안정성 확보(배열 → 포인터 변환 시 메모리 고정)
        private GCHandle _imageHandle;

        public byte[] ImageBuffer
        {
            get
            {
                return _imageBuffer;
            }
            set
            {
                _imageBuffer = value;
            }
        }
        public IntPtr ImageBufferPtr
        {
            get
            {
                return _imageBufferPtr;
            }
            set
            {
                _imageBufferPtr = value;
            }
        }
        public GCHandle ImageHandle
        {
            get
            {
                return _imageHandle;
            }
            set
            {
                _imageHandle = value;
            }
        }
    }

    internal abstract class GrabModel
    {
        public delegate void GrabEventHandler<T>(object sender, T obj = null) where T : class;

        public event GrabEventHandler<object> GrabCompleted;
        public event GrabEventHandler<object> TransferCompleted;

        protected GrabUserBuffer[] _userImageBuffer = null;
        public int BufferIndex { get; set; } = 0;

        protected string _strIpAddr = "";

        internal bool HardwareTrigger { get; set; } = false;
        internal bool IncreaseBufferIndex { get; set; } = false;

        protected AutoResetEvent _grabDoneEvent = new AutoResetEvent(false);

        internal abstract bool Create(string strIpAddr = null);

        internal abstract bool Grab(int bufferIndex, bool waitDone = true);

        internal abstract bool Close();

        internal abstract bool Open();

        internal virtual bool Reconnect() { return true; }

        internal abstract bool GetPixelBpp(out int pixelBpp);

        internal abstract bool SetExposureTime(long exposure);

        internal abstract bool GetExposureTime(out long exposure);

        internal abstract bool SetGain(float gain);

        internal abstract bool GetGain(out float gain);

        internal abstract bool GetResolution(out int width, out int height, out int stride);

        internal virtual bool SetTriggerMode(bool hardwareTrigger) { return true; }

        internal virtual bool SetWhiteBalance(bool auto, float redGain = 1.0f, float blueGain = 1.0f) { return true; }

        internal bool InitGrab()
        {
            SLogger.Write("Grab 초기화 시작!");

            if (!Create())
                return false;

            if (!Open())
            {
                if (!Reconnect())
                    return false;
            }

            SLogger.Write("Grab 초기화 성공!");
            return true;
        }

        internal bool InitBuffer(int bufferCount = 1)
        {
            if (bufferCount < 1)
                return false;

            _userImageBuffer = new GrabUserBuffer[bufferCount];
            return true;
        }

        internal bool SetBuffer(byte[] buffer, IntPtr bufferPtr, GCHandle bufferHandle, int bufferIndex = 0)
        {
            _userImageBuffer[bufferIndex].ImageBuffer = buffer;
            _userImageBuffer[bufferIndex].ImageBufferPtr = bufferPtr;
            _userImageBuffer[bufferIndex].ImageHandle = bufferHandle;

            return true;
        }
        protected virtual void OnGrabCompleted(object obj = null)
        {
            GrabCompleted?.Invoke(this, obj);
        }
        protected virtual void OnTransferCompleted(object obj = null)
        {
            TransferCompleted?.Invoke(this, obj);
        }

        internal abstract void Dispose();
    }
}
