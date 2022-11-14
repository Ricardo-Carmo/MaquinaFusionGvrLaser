using _22079AI.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class FormularioReceita : Form
    {
        private Point _start_point = new Point(0, 0);
        private bool _dragging = false;
        private static bool formAtivo = false;
        private TipoAcesso tipoAcesso = TipoAcesso.Ver;
        private static int idRegisto = 0;
        public static int compRegiao = 0;

        public static bool FormActivo
        {
            get { return formAtivo; }
            set { formAtivo = value; }

        }

        public static int IDRegisto
        {
            get { return idRegisto; }
        }


        public enum TipoAcesso
        {
            Adicionar = 0, Editar = 1, Ver = 2
        }

        private string strConexaoDb = string.Empty;

        public FormularioReceita(string _strConexaoDb, TipoAcesso _tipoAcesso, int _idRegisto = 0)
        {
            InitializeComponent();

            idRegisto = _idRegisto;

            this.strConexaoDb = _strConexaoDb;

            this.tipoAcesso = _tipoAcesso;

            this.btnSaveAs.Visible = _tipoAcesso == TipoAcesso.Editar;

            formAtivo = true;

        }

        public void PreencheDados(int _id)
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDb))
                using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA WHERE ID = @ID", sqlConn))
                {
                    sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _id;

                    sqlConn.Open();

                    using (SqlDataReader dr = sqlCmd.ExecuteReader())
                        if (dr.Read())
                        {
                            txtNumReferencia.Text = Convert.ToString(dr[1]);
                            txtDescricao.Text = Convert.ToString(dr[2]);
                            txtDistanciaCamFaca.Value = Convert.ToInt32(dr[3]);
                            txtComprimentoNominal.Value = (decimal)Convert.ToDouble(dr[4]);
                            txtToleranciaComprimento.Value = (decimal)Convert.ToDouble(dr[5]);
                            txtDesvioNominal.Value = (decimal)Convert.ToDouble(dr[6]);
                            txtToleranciaDesvio.Value = (decimal)Convert.ToDouble(dr[7]);
                            txtToleranciaDesvioInf.Value = (decimal)Convert.ToDouble(dr[8]);

                            txtRepresentacaoFaca.Text = Convert.ToString(dr[9]);

                            txtSpOK.Value = Convert.ToInt32(dr[10]);
                            txtSpNOK.Value = Convert.ToInt32(dr[11]);

                            
                        }
                        else
                            throw new Exception("Sem dados lidos para o ID: " + _id);
                }
                SpSequenciaReprovados.Value = DB400.SEQUENCE_REPROVED;

            }
            catch (Exception ex)
            {
                new CaixaMensagem(ex.Message, "Carregar Dados", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
            }
        }

        private void HabilitaControlos(bool enabled)
        {
            foreach (Control gp in panel3.Controls)
                if (gp is GroupBox)
                    foreach (Control c in gp.Controls)
                        if (!(c is PictureBox))
                            c.Enabled = enabled;
        }

        private void closeForm()
        {
            formAtivo = false;
            this.Close();           
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

        private void FormularioReceita_FormClosing(object sender, FormClosingEventArgs e)
        {
            formAtivo = false;
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            closeForm();
        }

         private void btnSelecionarRepresentacaoFaca_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.CheckFileExists = true;
                dialog.Title = "Selecionar imagem a carregar...";

                dialog.Filter = "PNG(*.png)|*.png";

                dialog.DefaultExt = ".png"; // Default file extension 

                if (dialog.ShowDialog() == DialogResult.OK)
                    txtRepresentacaoFaca.Text = dialog.FileName;
            }

        }

        private void txtRepresentacaoFaca_TextChanged(object sender, EventArgs e)
        {
            //Verificar se o ficheiro é válido
            if (File.Exists(txtRepresentacaoFaca.Text))
                try
                {
                    pctRepresentacao.Image = Image.FromFile(txtRepresentacaoFaca.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Mostrar Imagem", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    pctRepresentacao.Image = Resources.imagem_nao_disponivel;
                }
        }

        private void pctRepresentacao_Click(object sender, EventArgs e)
        {
            if (pctRepresentacao.Image != null)
                new MostraImagem((Image)pctRepresentacao.Image.Clone()).Show();
        }

        private void FormularioReceita_Load(object sender, EventArgs e)
        {
            switch (tipoAcesso)
            {
                case TipoAcesso.Adicionar:
                    label28.Text = "Adicionar Nova Receita";
                    break;
                case TipoAcesso.Editar:
                    label28.Text = "Editar Receita";
                    this.PreencheDados(idRegisto);
                    break;
                case TipoAcesso.Ver:
                    label28.Text = "Ver Receita";
                    this.PreencheDados(idRegisto);
                    break;
            }

            this.Text = label28.Text;

            this.HabilitaControlos(tipoAcesso != TipoAcesso.Ver);
        }

        private void btnSim_Click(object sender, EventArgs e)
        {
            if (tipoAcesso == TipoAcesso.Ver)
                closeForm();
            else
                if (this.VerificaPreenchimento())
                try
                {
                    string strQuery = string.Empty;
                    int numOfRows = 0;

                    if (tipoAcesso == TipoAcesso.Adicionar)
                        strQuery = "INSERT INTO RECEITA ([NOME], [DESCRICAO], [DISTANCIA_FACA_CAM], [COMPRIMENTO_NOMINAL], [TOLERANCIA_COMPRIMENTO], [DESVIO_NOMINAL], [TOLERANCIA_DESVIO_SUPERIOR],[TOLERANCIA_DESVIO_INFERIOR], [ENDERECO_IMAGEM_1], [SP_OK], [SP_NOT_OK], [ULTIMA_MODIFICACAO]) VALUES (@NOME, @DESCRICAO, @DISTANCIA_FACA_CAM, @COMPRIMENTO_NOMINAL, @TOLERANCIA_COMPRIMENTO, @DESVIO_NOMINAL, @TOLERANCIA_DESVIO_SUPERIOR, @TOLERANCIA_DESVIO_INFERIOR, @ENDERECO_IMAGEM_1, @SP_OK, @SP_NOT_OK, GETDATE())";
                    else if (tipoAcesso == TipoAcesso.Editar)
                        strQuery = "UPDATE RECEITA SET [NOME] = @NOME, [DESCRICAO] = @DESCRICAO, [DISTANCIA_FACA_CAM] = @DISTANCIA_FACA_CAM, [COMPRIMENTO_NOMINAL] = @COMPRIMENTO_NOMINAL, [TOLERANCIA_COMPRIMENTO] = @TOLERANCIA_COMPRIMENTO, [DESVIO_NOMINAL] = @DESVIO_NOMINAL, [TOLERANCIA_DESVIO_SUPERIOR] = @TOLERANCIA_DESVIO_SUPERIOR, [TOLERANCIA_DESVIO_INFERIOR] = @TOLERANCIA_DESVIO_INFERIOR, [ENDERECO_IMAGEM_1] = @ENDERECO_IMAGEM_1, SP_OK = @SP_OK, SP_NOT_OK = @SP_NOT_OK, [ULTIMA_MODIFICACAO] = GETDATE() WHERE ID = @ID";

                    if (!string.IsNullOrEmpty(strQuery))
                        using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDb))
                        using (SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn))
                        {
                            sqlCmd.Parameters.Add("@NOME", SqlDbType.NVarChar).Value = txtNumReferencia.Text;
                            sqlCmd.Parameters.Add("@DESCRICAO", SqlDbType.NVarChar).Value = txtDescricao.Text;
                            sqlCmd.Parameters.Add("@DISTANCIA_FACA_CAM", SqlDbType.Real).Value = Convert.ToInt32(txtDistanciaCamFaca.Value);
                            sqlCmd.Parameters.Add("@COMPRIMENTO_NOMINAL", SqlDbType.Real).Value = Convert.ToDouble(txtComprimentoNominal.Value);
                            sqlCmd.Parameters.Add("@TOLERANCIA_COMPRIMENTO", SqlDbType.Real).Value = Convert.ToDouble(txtToleranciaComprimento.Value);
                            sqlCmd.Parameters.Add("@DESVIO_NOMINAL", SqlDbType.Real).Value = Convert.ToDouble(txtDesvioNominal.Value);
                            sqlCmd.Parameters.Add("@TOLERANCIA_DESVIO_SUPERIOR", SqlDbType.Real).Value = Convert.ToDouble(txtToleranciaDesvio.Value);
                            sqlCmd.Parameters.Add("@TOLERANCIA_DESVIO_INFERIOR", SqlDbType.Real).Value = Convert.ToDouble(txtToleranciaDesvioInf.Value);

                            sqlCmd.Parameters.Add("@ENDERECO_IMAGEM_1", SqlDbType.NVarChar).Value = txtRepresentacaoFaca.Text;

                            sqlCmd.Parameters.Add("@SP_OK", SqlDbType.Int).Value = Convert.ToInt32(txtSpOK.Value);
                            sqlCmd.Parameters.Add("@SP_NOT_OK", SqlDbType.Int).Value = Convert.ToInt32(txtSpNOK.Value);

                            if (tipoAcesso == TipoAcesso.Editar)
                                sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = idRegisto;

                            sqlConn.Open();

                            numOfRows = Convert.ToInt32(sqlCmd.ExecuteNonQuery());

                        }

                    //Forms.MainForm.PLC1.EnviaTagRT(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.DInt, SpSequenciaReprovados.Value, 23, 72);

                    if (numOfRows == 1)
                    {
                        if (tipoAcesso == TipoAcesso.Adicionar)
                            if (new CaixaMensagem("Receita adicionada com sucesso! Deseja adicionar uma nova?", "Adicionar Receita", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() == DialogResult.Yes)
                                this.LimpaCampos();
                            else
                                closeForm();
                        else if (tipoAcesso == TipoAcesso.Editar)
                        {
                            //Alteração para actualizar receita quando se edita a receita que esta a ser usada actualmente - EJ
                            if (idRegisto == Forms.MainForm.Receita.ID)
                            {
                                Forms.MainForm.Receita.AtualizaReceita(Convert.ToInt32(idRegisto), Forms.MainForm.UserSession.IDOperador, string.Empty, DateTime.Now, true);
                                Forms.MainForm.AtualizaInformacoesReceita();
                            }

                            new CaixaMensagem("Receita editada com sucesso!", "Editar Receita", CaixaMensagem.TipoMsgBox.Normal, FormStartPosition.CenterScreen).ShowDialog();
                            closeForm();
                        }
                    }
                    else
                    {
                        if (tipoAcesso == TipoAcesso.Adicionar)
                            new CaixaMensagem("Erro ao adicionar receita!", "Adicionar Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
                        else if (tipoAcesso == TipoAcesso.Editar)
                            new CaixaMensagem("Erro ao editar receita!", "Editar Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("btnSim_Click(): " + ex.Message);
                    new CaixaMensagem(ex.Message, "Submeter Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
                }
        }

        private void LimpaCampos()
        {
            foreach (Control gp in panel3.Controls)
                if (gp is GroupBox)
                    foreach (Control c in gp.Controls)
                        if (c is TextBox || c is ComboBox)
                            c.Text = string.Empty;
                        else if (c is NumericUpDown)
                            (c as NumericUpDown).Value = (c as NumericUpDown).Minimum;
        }

        private bool VerificaPreenchimento()
        {
            if (string.IsNullOrWhiteSpace(txtNumReferencia.Text))
            {
                new CaixaMensagem("Verificar o campo 'Referência'!", "Verificações", CaixaMensagem.TipoMsgBox.Warning, FormStartPosition.CenterScreen).ShowDialog();
                return false;
            }

            if (txtDistanciaCamFaca.Value <= 0)
            {
                if (new CaixaMensagem("A região de inspeção esta defenida para 0 pixeis. Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }

            if (txtComprimentoNominal.Value <= 0)
            {
                new CaixaMensagem("O comprimento da faca não pode ser 0 milímetros!", "Verificações", CaixaMensagem.TipoMsgBox.Warning, FormStartPosition.CenterScreen).ShowDialog();
                return false;
            }

            if (txtSpOK.Value <= 0)
            {
                new CaixaMensagem("O SP de facas na box aprovados não pode ser 0!", "Verificações", CaixaMensagem.TipoMsgBox.Warning, FormStartPosition.CenterScreen).ShowDialog();
                return false;
            }

            if (txtSpNOK.Value <= 0)
            {
                new CaixaMensagem("O SP de facas na box reprovados não pode ser 0!", "Verificações", CaixaMensagem.TipoMsgBox.Warning, FormStartPosition.CenterScreen).ShowDialog();
                return false;
            }

            return true;
        }

        private void button57_Click(object sender, EventArgs e)
        {
            closeForm();
        }

        private void txtRepresentacaoFaca_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void txtRepresentacaoFaca_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                e.Handled = true;
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            if (tipoAcesso == TipoAcesso.Editar)
                if (this.VerificaPreenchimento())
                    try
                    {
                        string strQuery = string.Empty;
                        int numOfRows = 0;

                        strQuery = "INSERT INTO RECEITA ([NOME], [DESCRICAO], [DISTANCIA_FACA_CAM], [COMPRIMENTO_NOMINAL], [TOLERANCIA_COMPRIMENTO], [DESVIO_NOMINAL], [TOLERANCIA_DESVIO_SUPERIOR], [TOLERANCIA_DESVIO_INFERIOR], [ENDERECO_IMAGEM_1], [SP_OK], [SP_NOT_OK], [ULTIMA_MODIFICACAO]) VALUES (@NOME, @DESCRICAO, @DISTANCIA_FACA_CAM, @COMPRIMENTO_NOMINAL, @TOLERANCIA_COMPRIMENTO, @DESVIO_NOMINAL, @TOLERANCIA_DESVIO_SUPERIOR, @TOLERANCIA_DESVIO_INFERIOR, @ENDERECO_IMAGEM_1, @SP_OK, @SP_NOT_OK, GETDATE())";

                        if (!string.IsNullOrEmpty(strQuery))
                            using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDb))
                            using (SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn))
                            {
                                sqlCmd.Parameters.Add("@NOME", SqlDbType.NVarChar).Value = txtNumReferencia.Text;
                                sqlCmd.Parameters.Add("@DESCRICAO", SqlDbType.NVarChar).Value = txtDescricao.Text;
                                sqlCmd.Parameters.Add("@DISTANCIA_FACA_CAM", SqlDbType.Real).Value = Convert.ToInt32(txtDistanciaCamFaca.Value);
                                sqlCmd.Parameters.Add("@COMPRIMENTO_NOMINAL", SqlDbType.Real).Value = Convert.ToDouble(txtComprimentoNominal.Value);
                                sqlCmd.Parameters.Add("@TOLERANCIA_COMPRIMENTO", SqlDbType.Real).Value = Convert.ToDouble(txtToleranciaComprimento.Value);
                                sqlCmd.Parameters.Add("@DESVIO_NOMINAL", SqlDbType.Real).Value = Convert.ToDouble(txtDesvioNominal.Value);
                                sqlCmd.Parameters.Add("@TOLERANCIA_DESVIO_SUPERIOR", SqlDbType.Real).Value = Convert.ToDouble(txtToleranciaDesvio.Value);
                                sqlCmd.Parameters.Add("@TOLERANCIA_DESVIO_INFERIOR", SqlDbType.Real).Value = Convert.ToDouble(txtToleranciaDesvioInf.Value);

                                sqlCmd.Parameters.Add("@ENDERECO_IMAGEM_1", SqlDbType.NVarChar).Value = txtRepresentacaoFaca.Text;

                                sqlCmd.Parameters.Add("@SP_OK", SqlDbType.Int).Value = Convert.ToInt32(txtSpOK.Value);
                                sqlCmd.Parameters.Add("@SP_NOT_OK", SqlDbType.Int).Value = Convert.ToInt32(txtSpNOK.Value);

                                sqlConn.Open();

                                numOfRows = Convert.ToInt32(sqlCmd.ExecuteNonQuery());
                            }

                        if (numOfRows == 1)
                        {
                            new CaixaMensagem("Receita duplicada com sucesso!", "Duplicar Receita", CaixaMensagem.TipoMsgBox.Normal, FormStartPosition.CenterScreen).ShowDialog();
                            closeForm();
                        }
                        else
                            new CaixaMensagem("Erro ao duplicar receita!", "Duplicar Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("btnSim_Click(): " + ex.Message);
                        new CaixaMensagem(ex.Message, "Submeter Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
                    }

        }

        private void TxtDistanciaCamFaca_ValueChanged(object sender, EventArgs e)
        {
            compRegiao=Convert.ToInt32(txtDistanciaCamFaca.Value);
        }
    }
}
