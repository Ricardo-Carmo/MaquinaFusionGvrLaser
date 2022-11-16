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

namespace _22079AI
{
    public partial class CamObject : UserControl
    {
        private int lastTick = 0;
        private int lastFrameRate = 0;
        private int frameRate = 0;

        private DateTime lastFrame = DateTime.Now;

        public int FrameRate { get; private set; } = 0;

        private InspLateralDiscos inspKnife;

        public HWindowControl HWindowControl
        {
            get
            {
                return this.WindowControl;
            }
        }

        public HWindow HWindow
        {
            get
            {
                return this.WindowControl.HalconWindow;
            }
        }

        bool showFps = false;

        public CamObject(bool showFps)
        {
            InitializeComponent();

            this.showFps = showFps;

        }

        public HWindow Inicializar(InspLateralDiscos _insp)
        {
            inspKnife = _insp;
            return this.WindowControl.HalconWindow;
        }

        public void ResetSizeImag(int comprimento, int altura)
        {
            this.WindowControl.ImagePart = new System.Drawing.Rectangle(0, 0, comprimento, altura);
        }

        public void UpdateTimestamp(DateTime dateTime)
        {
            if (dateTime > DateTime.MinValue)
                if (this.showFps)
                {
                    this.WindowControl.HalconWindow.DispText(dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff") + " | " + (DateTime.Now - this.lastFrame).TotalMilliseconds.ToString("0") + " ms", "window", 0, 3, "yellow", "box", "false");
                    this.WindowControl.HalconWindow.DispText(this.FrameRate.ToString("0") + " fps", "window", 0, this.Width - 34, "yellow", "box", "false");
                    this.lastFrame = DateTime.Now;

                    if (Environment.TickCount - lastTick >= 1000)
                    {
                        this.lastFrameRate = this.frameRate;
                        this.frameRate = 0;
                        this.lastTick = System.Environment.TickCount;
                    }
                    this.frameRate++;
                    this.FrameRate = this.lastFrameRate;
                }
                else
                    this.WindowControl.HalconWindow.DispText(dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), "window", 0, 3, "yellow", "box", "false");

        }

        public void DisplayImage(HObject hImage, int inspNumber)
        {
            this.WindowControl.HalconWindow.DispObj(hImage);

            this.WindowControl.HalconWindow.DispText("INSPEÇÃO " + inspNumber, "window", 0, this.Width - 80, "red", "box", "false"); //posiciona a direita da img
        }

        public void DisplayImage(HImage hImage)
        {
            this.DisplayImage(hImage, new Point(), new Point(), LabelStatus.None, DateTime.MinValue);
        }

        public void DisplayImage(HImage hImage, DateTime dateTime)
        {
            this.DisplayImage(hImage, new Point(), new Point(), LabelStatus.None, dateTime);
        }

        public void DisplayImage(HImage hImage, LabelStatus status, DateTime dateTime)
        {
            this.DisplayImage(hImage, new Point(), new Point(), status, dateTime);
        }

        public void DisplayImage(HImage hImage, Point p1, Point p2)
        {
            this.DisplayImage(hImage, p1, p2, LabelStatus.None, DateTime.Now);
        }

        public void DisplayImage(HImage hImage, Point p1, Point p2, LabelStatus status, DateTime timestamp)
        {
            if (hImage != null)
            {
                this.WindowControl.HalconWindow.ClearWindow();

                //this.WindowControl.HalconWindow.DispImage(hImage);

                this.WindowControl.HalconWindow.DispObj(hImage);



                #region Escrever a região de pesquisa
                if (!p1.IsEmpty && !p2.IsEmpty)
                {

                    if (p2.X < p1.X)
                    {
                        p2.X = p1.X+10;
                    }
                    HObject rect = null;

                    HOperatorSet.GenRectangle1(out rect, p1.Y, p1.X, p2.Y, p2.X);

                    this.WindowControl.HalconWindow.SetColor("red");
                    this.WindowControl.HalconWindow.SetDraw("margin");
                    this.WindowControl.HalconWindow.DispObj(rect);
                }
                #endregion

                this.UpdateStatus(status);

                this.UpdateTimestamp(timestamp);
            }
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


    }
}
