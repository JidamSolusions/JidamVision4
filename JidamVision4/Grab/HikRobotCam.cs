using JidamVision4.Util;
using MvCameraControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace JidamVision4.Grab
{
    /*
   #5_CAMERA_INTERFACE# - <<<카메라 인터페이스 구현>>> 
   HikRobotCam 클래스를 이용해 카메라 인터페이스를 구현
   1) x64 비트 환경 구성
   2) HikRobotCam 라이브러리 추가 - MyCameraContro.Net 참조 추가
   3) GrabUserBuffer 구조체 정의 - 카메라에서 이미지를 가져오기 위한 버퍼 구조체    
   4) HikRobotCam 클래스 정의 - 카메라 인터페이스 구현   
   */

    //#5_CAMERA_INTERFACE#1 GrabModel로 상속 변경
    //internal class HikRobotCam : IDisposable
    internal class HikRobotCam : GrabModel
    {
        //#5_CAMERA_INTERFACE#2 GrabModel로 내부 변수 이동

        private IDevice _device = null;

        // 이미지 취득 콜백함수
        void FrameGrabedEventHandler(object sender, FrameGrabbedEventArgs e)
        {
            //SLogger.Write("Get one frame: Width[{0}] , Height[{1}] , ImageSize[{2}], FrameNum[{3}]", e.FrameOut.Image.Width, e.FrameOut.Image.Height, e.FrameOut.Image.ImageSize, e.FrameOut.FrameNum);

            IFrameOut frameOut = e.FrameOut;

            // 영상 취득이 완료되었을 때 이벤트 발생
            OnGrabCompleted(BufferIndex);

            if (_userImageBuffer[BufferIndex].ImageBuffer != null)
            {
                if (frameOut.Image.PixelType == MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    if (_userImageBuffer[BufferIndex].ImageBuffer != null)
                    {
                        IntPtr ptrSourceTemp = frameOut.Image.PixelDataPtr;
                        Marshal.Copy(ptrSourceTemp, _userImageBuffer[BufferIndex].ImageBuffer, 0, (int)frameOut.Image.ImageSize);
                    }   
                }
                else
                {
                    IImage inputImage = frameOut.Image;
                    IImage outImage;
                    MvGvspPixelType dstPixelType = MvGvspPixelType.PixelType_Gvsp_BGR8_Packed;

                    // Pixel type convert 
                    int result = _device.PixelTypeConverter.ConvertPixelType(inputImage, out outImage, dstPixelType);
                    if (result != MvError.MV_OK)
                    {
                        SLogger.Write($"Image Convert failed:{result:x8}", SLogger.LogType.Error);
                        return;
                    }

                    if (_userImageBuffer[BufferIndex].ImageBuffer != null)
                    {
                        IntPtr ptrSourceTemp = outImage.PixelDataPtr;
                        Marshal.Copy(ptrSourceTemp, _userImageBuffer[BufferIndex].ImageBuffer, 0, (int)outImage.ImageSize);
                    }
                }
            }

            // 영상 전송이 완료되었을 때 이벤트 발생
            OnTransferCompleted(BufferIndex);

            //IO 트리거 촬상시 최대 버퍼를 넘으면 첫번째 버퍼로 변경
            if (IncreaseBufferIndex)
            {
                BufferIndex++;
                if (BufferIndex >= _userImageBuffer.Count())
                    BufferIndex = 0;
            }
        }

        #region Method

        //#5_CAMERA_INTERFACE#3 GrabModel에서 상속받은 함수를 위해 override 추가
        internal override bool Create(string strIpAddr = null)
        {
            // Initialize SDK
            SDKSystem.Initialize();

            _strIpAddr = strIpAddr;

            try
            {
                const DeviceTLayerType devLayerType = DeviceTLayerType.MvGigEDevice;
                
                List<IDeviceInfo> devInfoList;

                // Enum device
                int ret = DeviceEnumerator.EnumDevices(devLayerType, out devInfoList);
                if (ret != MvError.MV_OK)
                {
                    SLogger.Write($"Enum device failed:{ret:x8}", SLogger.LogType.Error);
                    return false;
                }

                SLogger.Write($"Enum device count : {devInfoList.Count}");

                if (0 == devInfoList.Count)
                {
                    return false;
                }

                int selDevIndex = -1;

                // Print device info
                int devIndex = 0;
                foreach (var devInfo in devInfoList)
                {
                    if (devInfo.TLayerType == DeviceTLayerType.MvGigEDevice || devInfo.TLayerType == DeviceTLayerType.MvVirGigEDevice || devInfo.TLayerType == DeviceTLayerType.MvGenTLGigEDevice)
                    {
                        IGigEDeviceInfo gigeDevInfo = devInfo as IGigEDeviceInfo;
                        uint nIp1 = ((gigeDevInfo.CurrentIp & 0xff000000) >> 24);
                        uint nIp2 = ((gigeDevInfo.CurrentIp & 0x00ff0000) >> 16);
                        uint nIp3 = ((gigeDevInfo.CurrentIp & 0x0000ff00) >> 8);
                        uint nIp4 = (gigeDevInfo.CurrentIp & 0x000000ff);

                        string strIP = nIp1 + "." + nIp2 + "." + nIp3 + "." + nIp4;
                        SLogger.Write($"Device {devIndex}, DevIP : " + strIP);

                        if (_strIpAddr is null || strIP == strIpAddr)
                        {
                            selDevIndex = devIndex;
                            break;
                        }
                    }

                    SLogger.Write("ModelName:" + devInfo.ModelName);
                    SLogger.Write("SerialNumber:" + devInfo.SerialNumber);
                    devIndex++;
                }

                if (selDevIndex < 0 || selDevIndex > devInfoList.Count - 1)
                {
                    SLogger.Write($"Invalid selected device number:{selDevIndex}", SLogger.LogType.Error);
                    return false;
                }

                // Create device
                _device = DeviceFactory.CreateDevice(devInfoList[selDevIndex]);

                _disposed = false;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                ex.ToString();
                return false;
            }
            return true;
        }

        internal override bool Grab(int bufferIndex, bool waitDone)
        {
            if (_device == null)
                return false;

            BufferIndex = bufferIndex;
            bool ret = true;

            if (!HardwareTrigger)
            {
                try
                {
                    int result = _device.Parameters.SetCommandValue("TriggerSoftware");
                    if (result != MvError.MV_OK)
                    {
                        ret = false;
                    }
                }
                catch
                {
                    ret = false;
                }
            }

            return ret;
        }

        internal override bool Close()
        {
            if (_device != null)
            {
                _device.StreamGrabber.StopGrabbing();
                _device.Close();
            }

            return true;
        }

        internal override bool Open()
        {
            try
            {
                if (_device == null)
                    return false;

                if (!_device.IsConnected)
                {
                    int ret = _device.Open();
                    if (MvError.MV_OK != ret)
                    {
                        _device.Dispose();
                        SLogger.Write($"Device open fail! [{ret:x8}]", SLogger.LogType.Error);
                        MessageBox.Show($"Device open fail! {ret:X8}");
                        return false;
                    }

                    if (_device is IGigEDevice)
                    {
                        int packetSize;
                        ret = (_device as IGigEDevice).GetOptimalPacketSize(out packetSize);
                        if (packetSize > 0)
                        {
                            ret = _device.Parameters.SetIntValue("GevSCPSPacketSize", packetSize);
                            if (ret != MvError.MV_OK)
                            {
                                SLogger.Write($"Warning: Set Packet Size failed {ret:x8}", SLogger.LogType.Error);
                            }
                            else
                            {
                                SLogger.Write($"Set PacketSize to {packetSize}");
                            }
                        }
                        else
                        {
                            SLogger.Write($"Warning: Get Packet Size failed {ret:x8}",SLogger.LogType.Error);
                        }
                    }

                    // set trigger mode as off
                    ret = _device.Parameters.SetEnumValue("TriggerMode", 1);
                    if (ret != MvError.MV_OK)
                    {
                        SLogger.Write($"Set TriggerMode failed:{ret:x8}", SLogger.LogType.Error);
                        return false;
                    }

                    if (HardwareTrigger)
                    {
                        _device.Parameters.SetEnumValueByString("TriggerSource", "Line0");                        
                    }
                    else
                    {
                        _device.Parameters.SetEnumValueByString("TriggerSource", "Software");                        
                    }
                    
                    // Register image callback
                    _device.StreamGrabber.FrameGrabedEvent += FrameGrabedEventHandler;

                    // start grab image
                    ret = _device.StreamGrabber.StartGrabbing();
                    if (ret != MvError.MV_OK)
                    {
                        SLogger.Write("$Start grabbing failed:{ret:x8}", SLogger.LogType.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                SLogger.Write(ex.ToString(), SLogger.LogType.Error);
                return false;
            }

            return true;
        }

        internal override bool Reconnect()
        {
            if (_device is null)
            {
                SLogger.Write("_device is null", SLogger.LogType.Error);
                return false;
            }
            Close();
            return Open();
        }

        internal override bool GetPixelBpp(out int pixelBpp)
        {
            pixelBpp = 8;
            if (_device == null)
                return false;

            IEnumValue enumValue;
            int result = _device.Parameters.GetEnumValue("PixelFormat", out enumValue);
            if (result != MvError.MV_OK)
            {
                SLogger.Write($"Get PixelFormat failed:{result:x8}", SLogger.LogType.Error);
                return false;
            }

            if (MvGvspPixelType.PixelType_Gvsp_Mono8 == (MvGvspPixelType)enumValue.CurEnumEntry.Value)
                pixelBpp = 8;
            else
                pixelBpp = 24;

            return true;
        }
        #endregion


        #region Parameter Setting
        internal override bool SetExposureTime(long exposure)
        {
            if (_device == null)
                return false;

            _device.Parameters.SetEnumValue("ExposureAuto", 0);
            int result = _device.Parameters.SetFloatValue("ExposureTime", exposure);
            if (result != MvError.MV_OK)
            {
                SLogger.Write($"Set Exposure Time Fail:{result:x8}", SLogger.LogType.Error);
                return false;
            }

            return true;
        }

        internal override bool GetExposureTime(out long exposure)
        {
            exposure = 0;
            if (_device == null)
                return false;

            IFloatValue floatValue;
            int result = _device.Parameters.GetFloatValue("ExposureTime", out floatValue);
            if (result == MvError.MV_OK)
            {
                exposure = (long)floatValue.CurValue;
            }

            return true;
        }

        internal override bool SetGain(float gain)
        {
            if (_device == null)
                return false;

            _device.Parameters.SetEnumValue("GainAuto", 0);
            int result = _device.Parameters.SetFloatValue("Gain", gain);
            if (result != MvError.MV_OK)
            {
                SLogger.Write($"Set Gain Fail:{result:x8}", SLogger.LogType.Error);
                return false;
            }

            return true;
        }

        internal override bool GetGain(out float gain)
        {
            gain = 0;
            if (_device == null)
                return false;

            IFloatValue floatValue;
            int result = _device.Parameters.GetFloatValue("Gain", out floatValue);
            if (result == MvError.MV_OK)
            {
                gain = floatValue.CurValue;
            }

            return true;
        }

        internal override bool GetResolution(out int width, out int height, out int stride)
        {
            width = 0;
            height = 0;
            stride = 0;

            if (_device == null)
                return false;

            IIntValue intValue;
            IEnumValue enumValue;
            MvGvspPixelType pixelType;

            int result;

            result = _device.Parameters.GetIntValue("Width", out intValue);
            if (result != MvError.MV_OK)
            {
                SLogger.Write($"Get Width Fail:{result:x8}", SLogger.LogType.Error);
                return false;
            }
            width = (int)intValue.CurValue;

            result = _device.Parameters.GetIntValue("Height", out intValue);
            if (result != MvError.MV_OK)
            {
                SLogger.Write($"Get Height Fail:{result:x8}", SLogger.LogType.Error);
                return false;
            }
            height = (int)intValue.CurValue;

            result = _device.Parameters.GetEnumValue("PixelFormat", out enumValue);
            if (result != MvError.MV_OK)
            {
                SLogger.Write($"Get PixelFormat Fail:{result:x8}", SLogger.LogType.Error);
                return false;
            }
            pixelType = (MvGvspPixelType)enumValue.CurEnumEntry.Value;

            if (pixelType == MvGvspPixelType.PixelType_Gvsp_Mono8)
                stride = width * 1;
            else
                stride = width * 3;

            return true;
        }

        internal override bool SetTriggerMode(bool hardwareTrigger)
        {
            if (_device is null)
                return false;

            HardwareTrigger = hardwareTrigger;

            if (HardwareTrigger)
            {
                _device.Parameters.SetEnumValueByString("TriggerSource", "Line0");
            }
            else
            {
                _device.Parameters.SetEnumValueByString("TriggerSource", "Software");
            }

            return true;
        }

        #endregion

        #region Dispose

        private bool _disposed = false;

        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if(_device != null)
                {
                    _device.StreamGrabber.FrameGrabedEvent -= FrameGrabedEventHandler;
                    _device.StreamGrabber.StopGrabbing();
                    _device.Close();
                    _device.Dispose();
                    _device = null;

                    // Finalize SDK
                    SDKSystem.Finalize();
                }
            }
            _disposed = true;
        }

        //#5_CAMERA_INTERFACE#4 Dispose도 GrabModel에서 상속받아 사용
        internal override void Dispose()
        {
            Dispose(disposing: true);
        }
        #endregion //Disposable
    }
}
