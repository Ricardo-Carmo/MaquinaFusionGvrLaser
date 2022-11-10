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
    public partial class AdicionarOperador : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        int idOP = 0;
        bool novoUtilizador = false;
        string header = "";

        public AdicionarOperador(string _headerText, int _idOP = -1)
        {
            InitializeComponent();
            lblHeader.Text += _headerText;
            idOP = _idOP;
            header = _headerText;
            lstBoxNivelPermissao.SelectedIndex = 0;
            novoUtilizador = (idOP == -1);
        }

        private void AdicionarOperador_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private bool CarregaDefinicoesOperador()
        {
            if (idOP > 0)
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(Forms.MainForm.VariaveisAuxiliares.DatabaseConnectionString))
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT NOME, PASSWORD, ID_NIVEL FROM UTILIZADORES WHERE ID = @IDOP", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@IDOP", SqlDbType.TinyInt).Value = idOP;

                        sqlConn.Open();

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr.Read())
                                if (dr.FieldCount == 3)
                                {
                                    txtNomeOperador.Text = Convert.ToString(dr[0]);
                                    txtPassword.Text = Convert.ToString(dr[1]);

                                    switch (Convert.ToInt32(dr[2]))
                                    {
                                        case 1:
                                            {
                                                lstBoxNivelPermissao.SelectedIndex = 0;
                                                break;
                                            }
                                        case 4:
                                            {
                                                lstBoxNivelPermissao.SelectedIndex = 1;
                                                break;
                                            }
                                        default:
                                            {
                                                lstBoxNivelPermissao.SelectedIndex = -1;
                                                break;
                                            }
                                    }
                                }
                                else
                                    throw new Exception("Número de colunas incorreto em base de dados");
                            else
                                throw new Exception("Nenhum utilizador encontrado na base de dados. (ID = " + idOP + ")");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    new CaixaMensagem(ex.Message, "Erro Fatal", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                    return false;
                }
            else
            {
                new CaixaMensagem("Não é possível editar o utilizador selecionado (ID = " + idOP + ")", "Editar utilizador", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtTempoFimSessao_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !Char.IsDigit(e.KeyChar) && e.KeyChar != (char)8;
        }

        private void btnOPAIniciarOPA_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text) || string.IsNullOrWhiteSpace(txtNomeOperador.Text))
                new CaixaMensagem("Todos os campos devem ser preenchidos", "Verificações", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
            else
            {
                try
                {
                    //Criar/Atualizar Operador em base de dados
                    using (SqlConnection sqlConn = new SqlConnection(Forms.MainForm.VariaveisAuxiliares.DatabaseConnectionString))
                    {
                        string strQuery = "";

                        if (novoUtilizador)
                            strQuery = "INSERT INTO UTILIZADORES (NOME, PASSWORD, ID_NIVEL, DATA_HORA) VALUES (@NOME, @PASSWORD, @ID_NIVEL, GETDATE())";
                        else
                            strQuery = "UPDATE UTILIZADORES SET NOME = @NOME, PASSWORD = @PASSWORD, ID_NIVEL = @ID_NIVEL, DATA_HORA = GETDATE() WHERE ID = @IDOP";

                        using (SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn))
                        {
                            sqlCmd.Parameters.Add("@NOME", SqlDbType.NVarChar).Value = txtNomeOperador.Text;
                            sqlCmd.Parameters.Add("@PASSWORD", SqlDbType.NVarChar).Value = txtPassword.Text;
                            sqlCmd.Parameters.Add("@ID_NIVEL", SqlDbType.TinyInt).Value = lstBoxNivelPermissao.SelectedIndex == 1 ? 4 : 1;


                            if (!novoUtilizador)
                                sqlCmd.Parameters.Add("@IDOP", SqlDbType.TinyInt).Value = idOP;

                            sqlConn.Open();

                            if (sqlCmd.ExecuteNonQuery() == 1)
                            {
                                new CaixaMensagem(novoUtilizador ? "Utilizador " + txtNomeOperador.Text + " criado com sucesso" : "Utilizador " + txtNomeOperador.Text + " editado com sucesso", header, CaixaMensagem.TipoMsgBox.Normal).ShowDialog();
                                this.Close();
                            }
                            else
                                throw new Exception(novoUtilizador ? "Erro ao inserir novo utilizador em base de dados" : "Erro ao editar utilizador " + txtNomeOperador.Text + " em base de dados");
                        }
                    }
                }
                catch (Exception ex)
                {
                    new CaixaMensagem(ex.Message, header, CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                }
            }
        }

        private void AdicionarOperador_Load(object sender, EventArgs e)
        {
            if (!novoUtilizador)
                if (!CarregaDefinicoesOperador())
                    this.Close();
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
    }
}
