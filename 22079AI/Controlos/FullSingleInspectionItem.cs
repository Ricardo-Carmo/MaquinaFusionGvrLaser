using HalconDotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class FullSingleInspectionItem : Form
    {
        public static bool IsOpened { get; private set; } = false;

        private Inspection Inspection = new Inspection();



        public FullSingleInspectionItem(Inspection inspection, AuxiliarHalconImageClass imagem, string strError)
        {
            IsOpened = true;

            InitializeComponent();

            this.UpdateInfos(inspection, imagem, strError);
        }

        private void UpdateInfos(Inspection data, AuxiliarHalconImageClass imagem, string strError)
        {
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

            HObject imageAux = imagem.outputImage;

            lblId.Text = this.Inspection.ID.ToString();

            label28.Text = this.label28.Text.Replace("[n]", "[" + lblId.Text + "]");

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

            lblError.Text = strError;

            //limpa a janela
            //this.WindowControl.HalconWindow.ClearWindow();
            
            this.WindowControl.HalconWindow.SetPart(0, 0, 100, 664);
            this.WindowControl.HalconWindow.DispText(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), "window", 0, 3, "yellow", "box", "false");

            this.WindowControl.HalconWindow.DispObj(imageAux);

            //resultado insepcao
            this.UpdateStatus(this.Inspection.APROVED ? LabelStatus.Ok : this.Inspection.INSPECTION_RESULT == 3 ? LabelStatus.NokOk : LabelStatus.ErroInspecao);
            this.UpdateTimestamp(this.Inspection.DT_INSPECTION);

            //#region Mostra as Regiões na Imagem
            //if (imagem.ValidRegions())
            //{
            //    //mostra contornos
            //    this.WindowControl.HalconWindow.SetLineWidth(2.5);

            //    for (int i = 0; i < imagem.regions.CountObj(); i++)
            //    {
            //        this.WindowControl.HalconWindow.SetColor((string)imagem.colors[i]);
            //        this.WindowControl.HalconWindow.DispObj(imagem.regions.SelectObj(i + 1));
            //    }
            //}
            //#endregion


        }

        public void UpdateStatus(LabelStatus status)
        {
            switch (status)
            {
                case LabelStatus.EmInspecao:
                    //this.WindowControl.HalconWindow.DispText("EM INSPEÇÃO..", "window", 0, this.Width - 88, "gray", "box", "false");
                    this.WindowControl.HalconWindow.DispText("EM INSPEÇÃO...", "window", 0, 3, "gray", "box", "false");  //posiciona a esquerda da img
                    break;
                case LabelStatus.Ok:
                    this.WindowControl.HalconWindow.DispText("INSPEÇÃO OK", "window", 0, this.Width - 80, "green", "box", "false"); //posiciona a direita da img
                    break;
                case LabelStatus.Rework:
                    this.WindowControl.HalconWindow.DispText("INSPEÇÃO OK INV", "window", 0, this.Width - 108, "orange", "box", "false"); //posiciona a direita da img
                    break;
                case LabelStatus.NokOk:
                    this.WindowControl.HalconWindow.DispText("INSPEÇÃO NOT OK", "window", 0, this.Width - 108, "red", "box", "false"); //posiciona a direita da img
                    break;
                case LabelStatus.ErroInspecao:
                    this.WindowControl.HalconWindow.DispText("ERRO DE INSPEÇÃO", "window", 13, 3, "red", "box", "false"); //posiciona a direita da img
                    break;
            }
        }

        public void UpdateTimestamp(DateTime dateTime)
        {
            if (dateTime > DateTime.MinValue)
                this.WindowControl.HalconWindow.DispText(dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), "window", 0, 3, "yellow", "box", "false");

        }


        private void FullSingleInspectionItem_FormClosed(object sender, FormClosedEventArgs e)
        {
            IsOpened = false;
        }

        private void FullSingleInspectionItem_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }


        private void FullSingleInspectionItem_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.BringToFront();
        }


        private void BtnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FullSingleInspectionItem_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void LblError_TextChanged(object sender, EventArgs e)
        {
            lblError.Visible = !string.IsNullOrWhiteSpace(lblError.Text);
        }
    }
}
