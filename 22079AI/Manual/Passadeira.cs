using System;
using System.Drawing;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class Passadeira : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        private string strConnection = "";


        public Passadeira()
        {
            InitializeComponent();

        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            //Avança Passadeira
            //Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 31, 4, 0);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //Pára Passadeira
            //Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, false, 31, 12, 0);
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            //Avança Indexador
            //Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 31, 56, 3);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            //Recua Indexador
            //Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 31, 56, 4);
        }


    }
}
