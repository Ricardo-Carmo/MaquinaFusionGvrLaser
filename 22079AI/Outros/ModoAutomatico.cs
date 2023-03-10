using PLC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class ModoAutomatico : Form
    {
        private Point _start_point = new Point(0, 0);
        private bool _dragging = false;
        private bool formAtivo = true;


        public ModoAutomatico()
        {
            InitializeComponent();

            this.Text = label28.Text;
        }

        private void label28_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void label28_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void label28_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();

            //Caso se perca a sessão vamos fechar o formulário
            if (!Forms.MainForm.UserSession.TemPermissao(Sessao.SessaoOperador.Operador1))
                this.Close();

            //Caso estejamos em modo automatico vamos fechar o formulário
            if (DB400.MANUAL_AUTO)
                this.Close();


            #region Habilitar botões com o estado da ligação ao PLC
            button1.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
            button2.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
            button3.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
            button4.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
            button5.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
            button5.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
            button7.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
            #endregion

            Forms.MainForm.PlcControl.CmdCiclo.Vars.Reserved_5 = false;

            Diversos.AtualizaBackColor(button1, Forms.MainForm.PlcControl.StaCiclo.Vars.EmergencyOk, Color.LimeGreen, Color.White);
            Diversos.AtualizaBackColor(button2, Forms.MainForm.Receita.ReceitaCarregada, Color.LimeGreen, Color.White);
            Diversos.AtualizaBackColor(button3, Forms.MainForm.PlcControl.StaCiclo.Vars.ReadyForAuto, Color.LimeGreen, Color.White); //Ar OK
            Diversos.AtualizaBackColor(button4, Forms.MainForm.PlcControl.StaCiclo.Vars.TapeteReady, Color.LimeGreen, Color.White);
            Diversos.AtualizaBackColor(button5, Forms.MainForm.PlcControl.StaCiclo.Vars.AlimentadorReady, Color.LimeGreen, Color.White);
            Diversos.AtualizaBackColor(button6, Forms.MainForm.PlcControl.StaCiclo.Vars.ReadyForAuto, Color.LimeGreen, Color.White);
            Diversos.AtualizaBackColor(button7, Forms.MainForm.PlcControl.StaCiclo.Vars.ReadyForAuto, Color.LimeGreen, Color.White);




            //Botão de Confirmação
            Diversos.AtualizaBackColor(btnSim, Forms.MainForm.PlcControl.StaCiclo.Vars.ReadyForAuto && Forms.MainForm.Receita.ReceitaCarregada , Color.LimeGreen, Color.White);

            //TODO
            btnSim.Enabled = btnSim.BackColor == Color.LimeGreen;
            //btnSim.Enabled = true;

            if (formAtivo)
                timer1.Start();
        }

        private void ModoAutomatico_FormClosing(object sender, FormClosingEventArgs e)
        {
            formAtivo = false;
        }

        private void button57_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSim_Click(object sender, EventArgs e)
        {
            Forms.MainForm.PlcControl.CmdCiclo.Vars.AutoRequest = true;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Forms.MainForm.PlcControl.CmdCiclo.Vars.Reserved_5 = true;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, true, 20, 2, 3);
            Forms.MainForm.PlcControl.CmdCiclo.Vars.Reserved_5 = true;
        }

        private void Button6_1_Click(object sender, EventArgs e)
        {
            //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, false, 23, 0, 0);
            Forms.MainForm.PlcControl.CmdCiclo.Vars.Reserved_5 = true;
        }

        private void Button6_0_Click(object sender, EventArgs e)
        {
            //     Forms.MainForm.cam.Status = EnumAndVar.systemStatus.VideoMode;
        }


        private void Button5_Click(object sender, EventArgs e)
        {
            //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, true, 30, 0, 0);
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, false, 400, 38, 4);
        }

        private void Button7_Click(object sender, EventArgs e)
        {

        }

        private void Button8_0_Click(object sender, EventArgs e)
        {
            //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, !DB400.CAIXA_PRESENTE_APROVADOS, 20, 3, 3);

        }

        private void Button8_1_Click(object sender, EventArgs e)
        {
            //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, !DB400.CAIXA_PRESENTE_REPROVADOS, 20, 3, 4);
        }
    }
}
