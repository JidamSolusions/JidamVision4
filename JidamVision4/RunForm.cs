using JidamVision4.Core;
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
    public partial class RunForm: DockContent
    {
        public RunForm()
        {
            InitializeComponent();
        }

        private void btnGrab_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.Grab(0);
        }

        //#8_INSPECT_BINARY#20 검사 시작 버튼을 디자인창에서 만들고, 검사 함수 호출
        private void btnStart_Click(object sender, EventArgs e)
        {
            Global.Inst.InspStage.TryInspection();
        }
    }
}
