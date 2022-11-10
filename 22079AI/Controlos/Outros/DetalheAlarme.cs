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
    public partial class DetalheAlarme : Form
    {
        private byte id = 0;
        private bool alarmeAtivo = false;
        private bool dadosDescarregados = false;
        private Color corAlarme = Color.Transparent;
        private string strConexao = string.Empty;
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        private bool getActiveAlarm = false;

        public DetalheAlarme(string _strConexao, byte _id)
        {
            this.InitializeComponent();
            this.strConexao = _strConexao;
            this.id = _id;
            this.lblHeader.Text += string.Format("{0:00}", this.id);
            this.getActiveAlarm = true;
            this.PreencheInformacoes();
        }

        public DetalheAlarme(string _strConexao, long _idHistorico)
        {
            this.InitializeComponent();
            this.strConexao = _strConexao;
            this.id = this.ObtemIdAlarme(_idHistorico);
            this.lblHeader.Text += string.Format("{0:00}", this.id);
            this.getActiveAlarm = false;
            this.PreencheInformacoes();
        }

        private byte ObtemIdAlarme(long idHistorico)
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.strConexao))
                using (SqlCommand sqlCmd = new SqlCommand("SELECT TOP 1 ID_ALARME FROM HISTORICO_ALARMES WHERE ID = @ID", sqlConn))
                {
                    sqlCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = idHistorico;

                    sqlConn.Open();

                    return Convert.ToByte(sqlCmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ObtemIdAlarme(): " + ex.Message);
                return (byte)0;
            }
        }

        private void PreencheInformacoes()
        {
            try
            {
                if (this.id < 1)
                    throw new Exception("ID incorreto!");

                using (SqlConnection sqlConn = new SqlConnection(this.strConexao))
                using (SqlCommand sqlCmd = new SqlCommand("SELECT TOP 1 * FROM ALARMES WHERE ID = @ID", sqlConn))
                {
                    sqlCmd.Parameters.Add("@ID", SqlDbType.TinyInt).Value = this.id;

                    sqlConn.Open();

                    using (SqlDataReader dr = sqlCmd.ExecuteReader())
                        if (dr.Read())
                        {
                            label3.Text = Convert.ToString(dr[1]);

                            switch (Convert.ToByte(dr[2]))
                            {
                                case 1:
                                    corAlarme = Color.Red;
                                    break;
                                case 2:
                                    corAlarme = Color.Orange;
                                    break;
                                default:
                                    corAlarme = Color.DodgerBlue;
                                    break;
                            }

                            this.textBox1.Text = Convert.ToString(dr[3]);

                            if (string.IsNullOrWhiteSpace(this.textBox1.Text))
                                this.textBox1.Text = "Sem resoluções disponíveis";
                        }
                        else
                            throw new Exception("Sem dados lidos no ID '" + this.id + "'");
                }
                this.dadosDescarregados = true;
            }
            catch (Exception ex)
            {
                this.dadosDescarregados = false;
                new CaixaMensagem(ex.Message, "Erro ao preencher dados", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
            }
            finally
            {
                textBox1.Select(0, 0);
            }
        }

        private void lblHeader_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void lblHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void lblHeader_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Verifica alarme ativo
            if (id > 0)
                this.alarmeAtivo = Forms.MainForm.AlarmsHandling.alarms[id - 1].ActiveAlarm;

            if (dadosDescarregados)
            {
                this.lblAlarmeAtivo.Visible = this.alarmeAtivo;

                if (this.alarmeAtivo)
                    if (this.lblAlarmeAtivo.BackColor == corAlarme)
                        this.lblAlarmeAtivo.BackColor = Color.White;
                    else
                        this.lblAlarmeAtivo.BackColor = corAlarme;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                e.Handled = true;
        }

        private void DetalheAlarme_Load(object sender, EventArgs e)
        {
            timer1.Enabled = this.getActiveAlarm;
         }
    }
}
