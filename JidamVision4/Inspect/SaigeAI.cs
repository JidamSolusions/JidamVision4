using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SaigeVision.Net.V2;
using SaigeVision.Net.V2.IAD;

namespace JidamVision4
{
    /*
    #5_SAIGE_SDK# - <<<Saige SDK 모듈 적용>>> 
    1) SaigeVision.Net.Core.V2 참조 추가
    2) SaigeVision.Net.V2 참조 추가
    3) 솔루션 플렛폼 x64 추가
    4) class SaigeAI 코드 구현
    5) 전체 검사를 관리하는 InspStage 클래스 구현
    6) 싱글톤 패턴을 이용하여 전역적으로 접근할 수 있도록 Global 클래스 구현
    7) AIModuleProp UserControl 구현
    8) PropertiesForm에 AIModuleProp UserControl 추가
    */

    public class SaigeAI : IDisposable
    {
        private enum EngineType { IAD, IAD_BATCH, SEG, SEG_BATCH, CLS, CLS_BATCH, DET, OCR, IEN }
        private Dictionary<string, IADResult> _IADResults;

        IADEngine _iADEngine = null;
        IADResult _iADresult = null;
        Bitmap _inspImage = null;

        public SaigeAI()
        {
            // 생성자에서 초기화 작업을 수행할 수 있습니다.
            _IADResults = new Dictionary<string, IADResult>();
        }

        // 엔진을 로드하는 메서드입니다.
        public void LoadEngine(string modelPath)
        {
            if (this._iADEngine != null)
                this._iADEngine.Dispose();
                        
            // 검사하기 위한 엔진에 대한 객체를 생성합니다.
            // 인스턴스 생성 시 모데파일 정보와 GPU Index를 입력해줍니다.
            // 필요에 따라 batch size를 입력합니다
            _iADEngine = new IADEngine(modelPath, 0);

            // 검사 전 option에 대한 설정을 가져옵니다
            IADOption option = _iADEngine.GetInferenceOption();

            option.CalcScoremap = false;

            // 검사 결과에 대한 heatmap 이미지를 가져올 지 선택합니다
            // 약간의 속도차이로 불필요할 경우 false 로 설정합니다
            option.CalcHeatmap = false;

            // 검사 결과에 대한 mask이미지를 가져올 지 선택합니다
            // 약간의 속도차이로 불필요할 경우 false 로 설정합니다
            option.CalcMask = false;

            // 검사 결과에 대한 segmencted object (contour) 에 대한 정보를 가져올 지 선택합니다
            // 약간의 속도차이로 불필요할 경우 false 로 설정합니다
            option.CalcObject = true;

            // Segmented object의 면적이 object area threshold 보다 작으면 최종 결과에서 제외됩니다.
            option.CalcObjectAreaAndApplyThreshold = true;

            // Segmented object의 면적이 object score threshold 보다 작으면 최종 결과에서 제외됩니다.
            option.CalcObjectScoreAndApplyThreshold = true;

            // 추론 API 실행에 소요되는 시간을 세분화하여 출력할지 결정합니다.
            // `true`로 설정하면 이미지를 읽는 시간, 순수 딥러닝 추론 시간, 후처리 시간을 각각 확인할 수 있습니다.
            // `false`로 설정하면 추론 API 실행에 소요된 총 시간만을 확인할 수 있습니다.
            // `true`로 설정하면 전체 추론 시간이 느려질 수 있습니다. 실제 검사 시에는 `false`로 설정하는 것을 권장합니다.
            option.CalcTime = true;

            // option을 적용하여 검사에 대한 조건을 변경할 수 있습니다.
            // 필요에 따라 writeModelFile parameter를 이용하여 모델파일에 정보를 영구적으로 변경할 수 있습니다.
            _iADEngine.SetInferenceOption(option);
        }

        // 입력된 이미지에서 IAD 검사 진행
        public bool InspIAD(Bitmap bmpImage)
        {
            if(_iADEngine == null)
            {
                MessageBox.Show("엔진이 초기화되지 않았습니다. LoadEngine 메서드를 호출하여 엔진을 초기화하세요.");
                return false;
            }

            _inspImage = bmpImage;

            SrImage srImage = new SrImage(bmpImage);

            Stopwatch sw = Stopwatch.StartNew();

            // 검사 후 결과를 받아옵니다.
            _iADresult = _iADEngine.Inspection(srImage);

            //txt_InspectionTime.Text = sw.ElapsedMilliseconds.ToString();
            sw.Stop();

            return true;
        }

        // IADResult를 이용하여 결과를 이미지에 그립니다.
        private void DrawIADResult(IADResult result, Bitmap bmp)
        {
            Graphics g = Graphics.FromImage(bmp);
            int step = 10;

            // outline contour
            foreach (var prediction in result.SegmentedObjects)
            {
                SolidBrush brush = new SolidBrush(Color.FromArgb(127, prediction.ClassInfo.Color));
                //g.DrawString(prediction.ClassInfo.Name + " : " + prediction.Area, new Font(FontFamily.GenericSansSerif, 50), brush, 10, step);
                using (GraphicsPath gp = new GraphicsPath())
                {
                    if (prediction.Contour.Value.Count < 3) continue;
                    gp.AddPolygon(prediction.Contour.Value.ToArray());
                    foreach (var innerValue in prediction.Contour.InnerValue)
                    {
                        gp.AddPolygon(innerValue.ToArray());
                    }
                    g.FillPath(brush, gp);
                }
                step += 50;
            }
        }

        public Bitmap GetResultImage()
        {
            if(_iADresult == null || _inspImage is null)
                return null;

            Bitmap resultImage = _inspImage.Clone(new Rectangle(0, 0, _inspImage.Width, _inspImage.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            DrawIADResult(_iADresult, resultImage);

            return resultImage;
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
                    
                    // 검사완료 후 메모리 해제를 합니다.
                    // 엔진 사용이 완료되면 꼭 dispose 해주세요
                    if(_iADEngine != null)
                        _iADEngine.Dispose();
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
