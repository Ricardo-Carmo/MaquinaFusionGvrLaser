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

        //vai enviar a receita para a classe receitas - a alterar
        public void PreencheDados(int _id)
        {
            Receitas.ParamReceita ReceitaAtualizada = new Receitas.ParamReceita();
            try
            {
                Forms.MainForm.Receita.LeReceitaDb(ref ReceitaAtualizada, (short)_id);


                txtNome.Text = ReceitaAtualizada.NOME  ;
                 txtDescricao.Text = ReceitaAtualizada.DESCRICAO;

                txtComprimentoLeve.Value = (decimal)ReceitaAtualizada.ENCASCADO_LEVE.COMPRIMENTO ;
                 txtEspessuraLeve.Value = (decimal)ReceitaAtualizada.ENCASCADO_LEVE.ESPESSURA;
                  txtTonalidadeMinLeve.Value = (decimal)ReceitaAtualizada.ENCASCADO_LEVE.TONALIDADE_MIN;
                  txtTonalidadeMaxLeve.Value = (decimal)ReceitaAtualizada.ENCASCADO_LEVE.TONALIDADE_MAX;

                  txtComprimentoModerado.Value = (decimal)ReceitaAtualizada.ENCASCADO_MODERADO.COMPRIMENTO;
                  txtEspessuraModerado.Value = (decimal)ReceitaAtualizada.ENCASCADO_MODERADO.ESPESSURA;
                  txtTonalidadeMinModerado.Value = (decimal)ReceitaAtualizada.ENCASCADO_MODERADO.TONALIDADE_MIN;
                  txtTonalidadeMaxModerado.Value = (decimal)ReceitaAtualizada.ENCASCADO_MODERADO.TONALIDADE_MAX;

                  txtComprimentoVincado.Value = (decimal)ReceitaAtualizada.ENCASCADO_VINCADO.COMPRIMENTO;
                  txtEspessuraVincado.Value = (decimal)ReceitaAtualizada.ENCASCADO_VINCADO.ESPESSURA;
                  txtTonalidadeMinVincado.Value = (decimal)ReceitaAtualizada.ENCASCADO_VINCADO.TONALIDADE_MIN;
                  txtTonalidadeMaxVincado.Value = (decimal)ReceitaAtualizada.ENCASCADO_VINCADO.TONALIDADE_MAX;

            
                       
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
                    ;
                   // txtRepresentacaoFaca.Text = dialog.FileName;
            }

        }

      /*  private void txtRepresentacaoFaca_TextChanged(object sender, EventArgs e)
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
        }*/
/*
        private void pctRepresentacao_Click(object sender, EventArgs e)
        {
            if (pctRepresentacao.Image != null)
                new MostraImagem((Image)pctRepresentacao.Image.Clone()).Show();
        }
*/
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

        //Botao gravar receita
        private void btnSim_Click(object sender, EventArgs e)
        {
            if (tipoAcesso == TipoAcesso.Ver)
                closeForm();
            else
                if (this.VerificaPreenchimento())
                try
                {
                    string strQuery = string.Empty;
                    Receitas.ParamReceita ReceitaAtualizada = new Receitas.ParamReceita();
                    int numOfRows = 0;

                    ReceitaAtualizada.NOME = txtNome.Text;
                    ReceitaAtualizada.DESCRICAO=txtDescricao.Text;

                    ReceitaAtualizada.ENCASCADO_LEVE.COMPRIMENTO = (float)txtComprimentoLeve.Value;
                    ReceitaAtualizada.ENCASCADO_LEVE.ESPESSURA = (float)txtEspessuraLeve.Value;
                    ReceitaAtualizada.ENCASCADO_LEVE.TONALIDADE_MIN = (float)txtTonalidadeMinLeve.Value;
                    ReceitaAtualizada.ENCASCADO_LEVE.TONALIDADE_MAX = (float)txtTonalidadeMaxLeve.Value;

                    ReceitaAtualizada.ENCASCADO_MODERADO.COMPRIMENTO = (float)txtComprimentoModerado.Value;
                    ReceitaAtualizada.ENCASCADO_MODERADO.ESPESSURA = (float)txtEspessuraModerado.Value;
                    ReceitaAtualizada.ENCASCADO_MODERADO.TONALIDADE_MIN = (float)txtTonalidadeMinModerado.Value;
                    ReceitaAtualizada.ENCASCADO_MODERADO.TONALIDADE_MAX = (float)txtTonalidadeMaxModerado.Value;

                    ReceitaAtualizada.ENCASCADO_VINCADO.COMPRIMENTO = (float)txtComprimentoVincado.Value;
                    ReceitaAtualizada.ENCASCADO_VINCADO.ESPESSURA = (float)txtEspessuraVincado.Value;
                    ReceitaAtualizada.ENCASCADO_VINCADO.TONALIDADE_MIN = (float)txtTonalidadeMinVincado.Value;
                    ReceitaAtualizada.ENCASCADO_VINCADO.TONALIDADE_MAX = (float)txtTonalidadeMaxVincado.Value;

                    numOfRows = Forms.MainForm.Receita.EscreveReceitaDb(ref ReceitaAtualizada, tipoAcesso, (short)idRegisto);

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
                            if (idRegisto == Forms.MainForm.Receita._ReceitaCarregada.ID)
                            {
                                Forms.MainForm.Receita.AtualizaReceita(Convert.ToInt32(idRegisto), Forms.MainForm.UserSession.IDOperador, DateTime.Now, true);
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
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                new CaixaMensagem("Verificar o campo 'Nome'!", "Verificações", CaixaMensagem.TipoMsgBox.Warning, FormStartPosition.CenterScreen).ShowDialog();
                return false;
            }

            #region TabelaLeve
            if (txtEspessuraLeve.Value <= 0)
            {
                if (new CaixaMensagem("A Espessura Leve de inspeção esta defenida para 0 mm. Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtComprimentoLeve.Value <= 0)
            {
                if (new CaixaMensagem("O Comprimento Leve de inspeção esta defenida para 0 mm. Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtTonalidadeMaxLeve.Value <= 0)
            {
                if (new CaixaMensagem("A Tonalidade Maxima Leve de inspeção esta defenida para 0 . Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtTonalidadeMinLeve.Value <= 0)
            {
                if (new CaixaMensagem("A Tonalidade Minima Leve de inspeção esta defenida para 0 . Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            #endregion
            #region TabelaModerada
            if (txtEspessuraModerado.Value <= 0)
            {
                if (new CaixaMensagem("A Espessura Moderada de inspeção esta defenida para 0 mm. Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtComprimentoModerado.Value <= 0)
            {
                if (new CaixaMensagem("O Comprimento Moderado de inspeção esta defenida para 0 mm. Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtTonalidadeMaxModerado.Value <= 0)
            {
                if (new CaixaMensagem("A Tonalidade Maxima Moderada de inspeção esta defenida para 0 . Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtTonalidadeMinModerado.Value <= 0)
            {
                if (new CaixaMensagem("A Tonalidade Minima Moderada de inspeção esta defenida para 0 . Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            #endregion
            #region TabelaVincada
            if (txtEspessuraVincado.Value <= 0)
            {
                if (new CaixaMensagem("A Espessura Vincada de inspeção esta defenida para 0 mm. Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtComprimentoVincado.Value <= 0)
            {
                if (new CaixaMensagem("O Comprimento Vincado de inspeção esta defenida para 0 mm. Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtTonalidadeMaxVincado.Value <= 0)
            {
                if (new CaixaMensagem("A Tonalidade Maxima Vincada de inspeção esta defenida para 0 . Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            if (txtTonalidadeMinVincado.Value <= 0)
            {
                if (new CaixaMensagem("A Tonalidade Minima Vincada de inspeção esta defenida para 0 . Deseja confirmar?", "Verificações", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() != DialogResult.Yes)
                    return false;
            }
            #endregion

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
                        Receitas.ParamReceita ReceitaAtualizada = new Receitas.ParamReceita();
                        int numOfRows = 0;

                        ReceitaAtualizada.NOME = txtNome.Text;
                        ReceitaAtualizada.DESCRICAO = txtDescricao.Text;

                        ReceitaAtualizada.ENCASCADO_LEVE.COMPRIMENTO = (Int32)txtComprimentoLeve.Value;
                        ReceitaAtualizada.ENCASCADO_LEVE.ESPESSURA = (Int32)txtEspessuraLeve.Value;
                        ReceitaAtualizada.ENCASCADO_LEVE.TONALIDADE_MIN = (Int32)txtTonalidadeMinLeve.Value;
                        ReceitaAtualizada.ENCASCADO_LEVE.TONALIDADE_MAX = (Int32)txtTonalidadeMaxLeve.Value;

                        ReceitaAtualizada.ENCASCADO_MODERADO.COMPRIMENTO = (Int32)txtComprimentoModerado.Value;
                        ReceitaAtualizada.ENCASCADO_MODERADO.ESPESSURA = (Int32)txtEspessuraModerado.Value;
                        ReceitaAtualizada.ENCASCADO_MODERADO.TONALIDADE_MIN = (Int32)txtTonalidadeMinModerado.Value;
                        ReceitaAtualizada.ENCASCADO_MODERADO.TONALIDADE_MAX = (Int32)txtTonalidadeMaxModerado.Value;

                        ReceitaAtualizada.ENCASCADO_VINCADO.COMPRIMENTO = (Int32)txtComprimentoVincado.Value;
                        ReceitaAtualizada.ENCASCADO_VINCADO.ESPESSURA = (Int32)txtEspessuraVincado.Value;
                        ReceitaAtualizada.ENCASCADO_VINCADO.TONALIDADE_MIN = (Int32)txtTonalidadeMinVincado.Value;
                        ReceitaAtualizada.ENCASCADO_VINCADO.TONALIDADE_MAX = (Int32)txtTonalidadeMaxVincado.Value;

                        numOfRows = Forms.MainForm.Receita.EscreveReceitaDb(ref ReceitaAtualizada, tipoAcesso, (short)idRegisto);
                    

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
/*
        private void TxtDistanciaCamFaca_ValueChanged(object sender, EventArgs e)
        {
            compRegiao=Convert.ToInt32(txtDistanciaCamFaca.Value);
        }*/
    }
}
