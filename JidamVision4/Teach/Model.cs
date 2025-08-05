using Common.Util.Helpers;
using JidamVision4.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace JidamVision4.Teach
{
    public class Model
    {
        //모델 정보 저장을 위해 추가한 프로퍼티
        public string ModelName { get; set; } = "";
        public string ModelInfo { get; set; } = "";
        public string ModelPath { get; set; } = "";

        public string InspectImagePath { get; set; } = "";

        [XmlElement("InspWindow")]
        public List<InspWindow> InspWindowList { get; set; }

        public Model()
        {
            InspWindowList = new List<InspWindow>();
        }

        public InspWindow AddInspWindow(InspWindowType windowType)
        {
            InspWindow inspWindow = InspWindowFactory.Inst.Create(windowType);
            InspWindowList.Add(inspWindow);

            return inspWindow;
        }

        public bool AddInspWindow(InspWindow inspWindow)
        {
            if (inspWindow is null)
                return false;

            InspWindowList.Add(inspWindow);

            return true;
        }

        public bool DelInspWindow(InspWindow inspWindow)
        {
            if (InspWindowList.Contains(inspWindow))
            {
                InspWindowList.Remove(inspWindow);
                return true;
            }
            return false;
        }

        public bool DelInspWindowList(List<InspWindow> inspWindowList)
        {
            int before = InspWindowList.Count;
            InspWindowList.RemoveAll(w => inspWindowList.Contains(w));
            return InspWindowList.Count < before;
        }

        //신규 모델 생성
        public void CreateModel(string path, string modelName, string modelInfo)
        {
            ModelPath = path;
            ModelName = modelName;
            ModelInfo = modelInfo;
        }

        //#12_MODEL SAVE#2 모델 파일 Load,Save,SaveAs
        //모델 로딩함수
        public Model Load(string path)
        {
            Model model = XmlHelper.LoadXml<Model>(path);
            if (model == null)
                return null;

            foreach (var window in model.InspWindowList)
            {
                window.LoadInspWindow(model);
            }

            return model;
        }

        //모델 저장함수
        public void Save()
        {
            if (ModelPath == "")
                return;

            XmlHelper.SaveXml(ModelPath, this);

            foreach (var window in InspWindowList)
            {
                window.SaveInspWindow(this);
            }
        }

        //모델 다른 이름으로 저장함수
        public void SaveAs(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (Directory.Exists(filePath) == false)
            {
                ModelPath = Path.Combine(filePath, fileName + ".xml");
                ModelName = fileName;
                Save();
            }
        }
    }
}
