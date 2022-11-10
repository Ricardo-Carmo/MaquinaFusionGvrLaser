using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class Inspecao : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        private string strConnection = "";


        public Inspecao()
        {
            InitializeComponent();
            
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            //Abre Direita
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 12, 4, 3);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //Abre Esquerda
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 11, 4, 3);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            //Fecha Esquerda
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 11, 4, 4);
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            //Fecha Direita
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 12, 4, 4);
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            //Abre Direita
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 12, 4, 3);
            //Abre Esquerda
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 11, 4, 3);
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            //Fecha Esquerda
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 11, 4, 4);
            //Fecha Direita
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 12, 4, 4);
        }
    }
}
