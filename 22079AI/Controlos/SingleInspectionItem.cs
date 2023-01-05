using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using System.IO;
using System.Drawing.Imaging;

namespace _22079AI
{
    public partial class SingleInspectionItem : UserControl
    {
        public Inspection Inspection = new Inspection();

        public string MsgError { get; private set; } = string.Empty;

        public AuxiliarHalconImageClass ImagemInspecao { get; set; } = new AuxiliarHalconImageClass();

        public HWindow Display
        {
            get
            {
                return this.WindowControl.HalconWindow;
            }
        }

        public SingleInspectionItem()
        {
            InitializeComponent();
        }

        public SingleInspectionItem(Inspection data, AuxiliarHalconImageClass imagemInspecao, string error, int comprimento, int altura)
        {
            InitializeComponent();

            this.UpdateInfos(data, imagemInspecao, error, comprimento, altura);
        }

        public void UpdateInfos(Inspection data, AuxiliarHalconImageClass imagemInspecao, string error, int comprimento, int altura)
        {
            this.ImagemInspecao = imagemInspecao;

            //move os valores
            this.Inspection.ID = data.ID;
            this.Inspection.UNIX_TIME = data.UNIX_TIME;
            this.Inspection.COMPRIMENTO = data.COMPRIMENTO;
            this.Inspection.AMPLITUDE_DESVIO_SUPERIOR = data.AMPLITUDE_DESVIO_SUPERIOR;
            this.Inspection.POS_MAX_DESVIO_SUPERIOR = data.POS_MAX_DESVIO_SUPERIOR;
            this.Inspection.AMPLITUDE_DESVIO_INFERIOR = data.AMPLITUDE_DESVIO_INFERIOR;
            this.Inspection.POS_MAX_DESVIO_INFERIOR = data.POS_MAX_DESVIO_INFERIOR;
            this.Inspection.CAPTURE_TIME = data.CAPTURE_TIME;
            this.Inspection.INSPECTION_TIME = data.INSPECTION_TIME;
            this.Inspection.INSPECTION_RESULT = data.INSPECTION_RESULT;
            this.Inspection.CAPTURE_DONE = data.CAPTURE_DONE;
            this.Inspection.INSPECTION_DONE = data.INSPECTION_DONE;
            this.Inspection.APROVED = data.APROVED;
            this.Inspection.READ = data.READ;
            this.Inspection.RESERVED4 = data.RESERVED4;
            this.Inspection.RESERVED5 = data.RESERVED5;
            this.Inspection.RESERVED6 = data.RESERVED6;
            this.Inspection.RESERVED7 = data.RESERVED7;

            this.MsgError = error;

            lblId.Text = this.Inspection.ID.ToString();

            switch (this.Inspection.INSPECTION_RESULT)
            {
                case 1:
                    lblId.ForeColor = Forms.MainForm.pnlInspecaoOK.BackColor;
                    break;
                case 3:
                    lblId.ForeColor = Forms.MainForm.pnlInspecaoNOK.BackColor;
                    break;
                default:
                    lblId.ForeColor = Forms.MainForm.pnlErroInspecao.BackColor;
                    break;
            }

            this.Display.ClearWindow();

            if (imagemInspecao.outputImage != null)
            {
                //this.WindowControl.ImagePart = new Rectangle(0, 0, comprimento, altura);
                this.WindowControl.HalconWindow.SetPart(0, 0, 100, 664);
                //this.Display.SetPart(0, 0, -2, -2);
                this.Display.DispObj(imagemInspecao.outputImage);
                //this.WindowControl.HalconWindow.SetPart(0, 0, comprimento, altura);
            }

        }
        private void WindowControl_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void WindowControl_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        public void Dispose()
        {
            this.WindowControl.HalconWindow.Dispose();
            this.WindowControl.Dispose();
            this.WindowControl = null;
        }

        private void WindowControl_HMouseUp(object sender, HMouseEventArgs e)
        {
            if (!FullSingleInspectionItem.IsOpened) new FullSingleInspectionItem(this.Inspection, this.ImagemInspecao, this.MsgError).Show();
        }
    }
}
