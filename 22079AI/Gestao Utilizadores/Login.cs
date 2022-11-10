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
    public partial class Login : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        private string strConnection = "";
        string[] passwords = new string[250];
        byte[] nivel = new byte[250];
        byte[] idOp = new byte[250];

        public Login(string _strConnection)
        {
            InitializeComponent();
            strConnection = _strConnection;
        }

        private bool PreencheInformacoes()
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(strConnection))
                using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM UTILIZADORES WHERE ID > 0", sqlConn))
                {
                    int i = 0;
                    sqlConn.Open();

                    using (SqlDataReader dr = sqlCmd.ExecuteReader())
                        while (dr.Read())
                        {
                            idOp[i] = Convert.ToByte(dr[0]);
                            metroComboBox1.Items.Add(Convert.ToString(dr[1]));
                            passwords[i] = Convert.ToString(dr[2]);
                            nivel[i] = Convert.ToByte(dr[3]);
                            i++;
                        }

                    if (i == 0)
                        throw new Exception("Não há operadores registados na base de dados!");
                    else //Partimos do principio que a 1º conta de utilizador é ADMIN STREAK!
                        metroComboBox1.SelectedIndex = i > 1 ? 1 : 0;

                    return true;
                }
            }
            catch (Exception ex)
            {
                new CaixaMensagem(ex.Message, "Iniciar Sessão", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                return false;
            }
        }

        private void label28_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void label28_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void label28_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IniciaSessao();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            //Descarregar nome dos operadores e password
            if (!PreencheInformacoes())
                this.Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                IniciaSessao();
        }

        private void IniciaSessao()
        {
            if (metroComboBox1.SelectedIndex >= 0)
            {
                int indexId = metroComboBox1.SelectedIndex;
                if (indexId >= 0)
                    if (textBox1.Text == passwords[indexId])
                        if (Forms.MainForm.UserSession.IniciaSessao(idOp[indexId], metroComboBox1.Text, nivel[indexId]))
                            this.Close();
                        else
                        {
                            //Login incorreto
                            new CaixaMensagem("Password Inválida!", "Iniciar Sessão", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                            textBox1.Clear();
                        }
                    else
                        new CaixaMensagem("Password Inválida!", "Iniciar Sessão", CaixaMensagem.TipoMsgBox.Warning).ShowDialog(); ;
            }
        }

        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                IniciaSessao();
        }
    }
}
