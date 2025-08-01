using JidamVision4.Algorithm;
using JidamVision4.Core;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace JidamVision4
{
    //#2_DOCKPANEL#3 CameraForm 클래스 는 도킹 가능하도록 상속을 변경
    //public partial class CameraForm: Form
    public partial class CameraForm : DockContent
    {
        public CameraForm()
        {
            InitializeComponent();
        }


        //#3_CAMERAVIEW_PROPERTY#1 이미지 경로를 받아 PictureBox에 이미지를 로드하는 메서드
        public void LoadImage(string filePath)
        {
            if (File.Exists(filePath) == false)
                return;

            //#4_IMAGE_VIEWER#6 이미지 뷰어 컨트롤을 사용하여 이미지를 로드
            //picMainview.Image = Image.FromFile(filePath);
            Image bitmap = Image.FromFile(filePath);
            imageViewer.LoadBitmap((Bitmap)bitmap);
        }

        private void CameraForm_Resize(object sender, EventArgs e)
        {
            int margin = 0;
            imageViewer.Width = this.Width - margin * 2;
            imageViewer.Height = this.Height - margin * 2;

            imageViewer.Location = new System.Drawing.Point(margin, margin);
        }

        public void UpdateDisplay(Bitmap bitmap = null)
        {
            if (bitmap == null)
            {
                //#6_INSP_STAGE#3 업데이트시 bitmap이 없다면 InspSpace에서 가져온다
                bitmap = Global.Inst.InspStage.GetBitmap(0);
                if (bitmap == null)
                    return;
            }

            if (imageViewer != null)
                imageViewer.LoadBitmap(bitmap);

            //#7_BINARY_PREVIEW#10 현재 선택된 이미지로 Previwe이미지 갱신
            //이진화 프리뷰에서 각 채널별로 설정이 적용되도록, 현재 이미지를 프리뷰 클래스 설정            
            Mat curImage = Global.Inst.InspStage.GetMat();
            Global.Inst.InspStage.PreView.SetImage(curImage);
        }

        public Bitmap GetDisplayImage()
        {
            Bitmap curImage = null;
            
            if(imageViewer != null)
                curImage = imageViewer.GetCurBitmap();

            return curImage;
        }

        public void UpdateImageViewer()
        {
            imageViewer.Invalidate();
        }

        //#8_INSPECT_BINARY#18 imageViewer에 검사 결과 정보를 연결해주기 위한 함수
        public void ResetDisplay()
        {
            imageViewer.ResetEntity();
        }

        //FIXME 검사 결과를 그래픽으로 출력하기 위한 정보를 받는 함수
        public void AddRect(List<DrawInspectInfo> rectInfos)
        {
            imageViewer.AddRect(rectInfos);
        }

    }
}
