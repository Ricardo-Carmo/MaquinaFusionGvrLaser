using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{

    public partial class ConfiguracoesAlarmes : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);

        public ConfiguracoesAlarmes()
        {
            InitializeComponent();

            txtMaxAlarms.Text = Convert.ToString(Forms.MainForm.VariaveisAuxiliares.NumeroMaximoFifoAlarmes);

            txtMaxAlarms.Enabled = Forms.MainForm.VerificaAcesso(Sessao.SessaoOperador.Administrador, false);
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button96_Click(object sender, EventArgs e)
        {
            if (Forms.MainForm.VerificaAcesso(Sessao.SessaoOperador.Operador1))
                switch (Diversos.ExportarParaXLS(Forms.MainForm.datagridviewHistoricAlarms))
                {
                    case 0:
                        new CaixaMensagem("Erro ao tentar exportar tabela", "Exportação", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                        break;
                    case 1:
                        new CaixaMensagem("Exportação cancelada pelo utilizador", "Exportação", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                        break;
                    case 2:
                        new CaixaMensagem("Tabela de exportação sem dados", "Exportação", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                        break;
                    case 3:
                        new CaixaMensagem("Alarmes ativos exportados com sucesso", "Exportação", CaixaMensagem.TipoMsgBox.Normal).ShowDialog();
                        break;
                }
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (Forms.MainForm.VerificaAcesso(Sessao.SessaoOperador.Administrador))
                if (new CaixaMensagem("Esta ação é IRREVERSÍVEL. Deseja continuar?" + Environment.NewLine + "Todos os alarmes serão permanentemente eliminados.", "Eliminar Alarmes", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterParent).ShowDialog() == DialogResult.Yes)
                    try
                    {
                        int numberOfAffectedRows = 0;
                        using (SqlConnection connection = new SqlConnection(Forms.MainForm.VariaveisAuxiliares.DatabaseConnectionString))
                        using (SqlCommand sqlCmd = new SqlCommand("DELETE FROM HISTORICO_ALARMES", connection))
                        {
                            connection.Open();

                            numberOfAffectedRows = sqlCmd.ExecuteNonQuery();

                            Debug.WriteLine("Saved alarms deleted. Nr of rows affected: " + numberOfAffectedRows);
                        }

                        new CaixaMensagem("Número de alarmes eliminados: " + numberOfAffectedRows, "Eliminar Alarmes", CaixaMensagem.TipoMsgBox.Normal, FormStartPosition.CenterParent).ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error deleting saved alarms: " + ex.Message);
                        new CaixaMensagem(ex.Message, "Erro ao eliminar alarmes", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterParent).ShowDialog();
                    }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.AlteraSPAlarmes();
        }

        private void AlteraSPAlarmes()
        {
            if (Forms.MainForm.VerificaAcesso(Sessao.SessaoOperador.Administrador))
            {
                if (string.IsNullOrWhiteSpace(txtMaxAlarms.Text))
                {
                    new CaixaMensagem("Todos os campos devem ser preenchidos", "Verificações", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                    return;
                }

                if (!Diversos.IsNumeric(txtMaxAlarms.Text))
                {
                    new CaixaMensagem("O campos devem ser numéricos", "Verificações", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                    return;
                }
                int numMaxAlarmes = int.Parse(txtMaxAlarms.Text);

                if (numMaxAlarmes < 25 || numMaxAlarmes > 10000)
                {
                    new CaixaMensagem("O número de alarmes deverá ser entre 25 e 10000", "Verificações Alarmes", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                    return;
                }

                Forms.MainForm.VariaveisAuxiliares.NumeroMaximoFifoAlarmes = numMaxAlarmes;

                using (FicheiroINI ini = new FicheiroINI(Forms.MainForm.VariaveisAuxiliares.iniPath))
                    ini.EscreveFicheiroINI("Configs", "NumeroMaximoFifoAlarmes", Convert.ToString(Forms.MainForm.VariaveisAuxiliares.NumeroMaximoFifoAlarmes));

            }
        }

        private void txtMaxAlarms_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Char.IsDigit(e.KeyChar) && e.KeyChar != (char)8;

            if (e.KeyChar == 13)
                this.AlteraSPAlarmes();
        }
    }
}
