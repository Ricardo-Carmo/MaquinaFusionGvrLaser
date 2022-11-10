using PLC;
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
    public partial class MensagemSequenciaReprovados : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);

        public MensagemSequenciaReprovados()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!DB400.MANUAL_AUTO || !DB400.SEQUENCIA_NOK)
                this.Close();

            Diversos.InverteCores(lblCicloAutomatico);
        }
        private void lblCicloAutomatico_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void lblCicloAutomatico_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void lblCicloAutomatico_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool,true, 45, 2, 7);
            this.Close();
        }

 
    }
}
