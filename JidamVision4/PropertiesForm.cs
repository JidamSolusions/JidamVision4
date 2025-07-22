using JidamVision4.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace JidamVision4
{
    //#3_CAMERAVIEW_PROPERTY#3 속성창에 사용할 타입 선언
    public enum PropertyType
    {
        Binary,
        Filter,
        AIModule
    }

    //#2_DOCKPANEL#4 PropertiesForm 클래스 는 도킹 가능하도록 상속을 변경

    //public partial class PropertiesForm: Form
    public partial class PropertiesForm : DockContent
    {
        //#3_CAMERAVIEW_PROPERTY#4 속성탭을 관리하기 위한 딕셔너리
        Dictionary<string, TabPage> _allTabs = new Dictionary<string, TabPage>();

        public PropertiesForm()
        {
            InitializeComponent();

            //#3_CAMERAVIEW_PROPERTY#7 속성 탭을 초기화
            LoadOptionControl(PropertyType.Filter);
            LoadOptionControl(PropertyType.Binary);
            LoadOptionControl(PropertyType.AIModule);
        }

        //#3_CAMERAVIEW_PROPERTY#6 속성탭이 있다면 그것을 반환하고, 없다면 생성
        private void LoadOptionControl(PropertyType propType)
        {
            string tabName = propType.ToString();

            // 이미 있는 TabPage인지 확인
            foreach (TabPage tabPage in tabPropControl.TabPages)
            {
                if (tabPage.Text == tabName)
                    return;
            }

            // 딕셔너리에 있으면 추가
            if (_allTabs.TryGetValue(tabName, out TabPage page))
            {
                tabPropControl.TabPages.Add(page);
                return;
            }

            // 새로운 UserControl 생성
            UserControl _inspProp = CreateUserControl(propType);
            if (_inspProp == null)
                return;

            // 새 탭 추가
            TabPage newTab = new TabPage(tabName)
            {
                Dock = DockStyle.Fill
            };
            _inspProp.Dock = DockStyle.Fill;
            newTab.Controls.Add(_inspProp);
            tabPropControl.TabPages.Add(newTab);
            tabPropControl.SelectedTab = newTab; // 새 탭 선택

            _allTabs[tabName] = newTab;
        }

        //#3_CAMERAVIEW_PROPERTY# 5 속성 탭을 생성하는 메서드
        private UserControl CreateUserControl(PropertyType propType)
        {
            UserControl curProp = null;
            switch (propType)
            {
                case PropertyType.Binary:
                    BinaryProp blobProp = new BinaryProp();
                    curProp = blobProp;
                    break;
                case PropertyType.Filter:
                    ImageFilterProp filterProp = new ImageFilterProp();
                    curProp = filterProp;
                    break;
                case PropertyType.AIModule:
                    AIModuleProp aiModuleProp = new AIModuleProp();
                    curProp = aiModuleProp;
                    break;
                default:
                    MessageBox.Show("유효하지 않은 옵션입니다.");
                    return null;
            }
            return curProp;
        }

    }
}
