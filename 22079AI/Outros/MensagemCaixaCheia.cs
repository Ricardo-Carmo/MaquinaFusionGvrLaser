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
    public partial class MensagemCaixaCheia : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);


        private TipoCaixa tipoCaixa = TipoCaixa.Aprovados;


        public MensagemCaixaCheia(TipoCaixa tipoCaixa)
        {
            InitializeComponent();

            this.tipoCaixa = tipoCaixa;

            switch (tipoCaixa)
            {
                case TipoCaixa.Aprovados:
                    label77.Text = "Caixa Aprovados Cheia";
                    label77.BackColor = Color.Green;
                    break;
                case TipoCaixa.Reprovados:
                    label77.Text = "Caixa Reprovados Cheia";
                    label77.BackColor = Color.Red;
                    break;
            }
        }


        private void label77_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void label77_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void label77_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void button40_Click(object sender, EventArgs e)
        {
            //*** TODO *** VER COM O RICARDO OS ENDEREÇOS!!
            //if (this.tipoCaixa == TipoCaixa.Aprovados)
            //    Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 30, 0, 5);
            //else if (this.tipoCaixa == TipoCaixa.Reprovados)
            //    Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 30, 0, 5);
            MessageBox.Show("Ver com ricardo os enderecos!!");

            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!DB400.MANUAL_AUTO || (this.tipoCaixa == TipoCaixa.Aprovados && !DB400.DESCARGA_OK_FULL) || (this.tipoCaixa == TipoCaixa.Reprovados && !DB400.DESCARGA_NOK_FULL) || 
                (this.tipoCaixa == TipoCaixa.Aprovados && DB400.STOP_BARREIRA_APROVADOS) || (this.tipoCaixa == TipoCaixa.Reprovados && DB400.STOP_BARREIRA_REPROVADOS))
                this.Close();

            Diversos.InverteCores(label77);
        }

    }
}
