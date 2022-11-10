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
    public partial class GestaoReceitas : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        private string strConexaoDb = string.Empty;

        public GestaoReceitas(string _strConexaoDb)
        {
            InitializeComponent();

            this.strConexaoDb = _strConexaoDb;
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
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

        private void button41_Click(object sender, EventArgs e)
        {
            switch (Diversos.ExportarParaXLS(dgvReceitas))
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
                    new CaixaMensagem("Tabela exportada com sucesso", "Exportação", CaixaMensagem.TipoMsgBox.Normal).ShowDialog();
                    break;
            }
        }

         private void AtualizaListaReceitas(string value)
        {
            bool firstRun = false;
            string strQuery = "SELECT ID, NOME AS 'Referência', DESCRICAO AS 'Descrição', COMPRIMENTO_NOMINAL AS 'Comprimento Nominal', ULTIMA_MODIFICACAO AS 'Última Modificação' FROM RECEITA WHERE ID > 0 ";

            if (!string.IsNullOrWhiteSpace(value))
                strQuery += " AND NOME LIKE @NAME ";

            strQuery += "ORDER BY ID";
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDb))
                using (SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn))
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        sqlCmd.Parameters.Add("@NAME", SqlDbType.NVarChar).Value = "%" + value + "%";

                    sqlConn.Open();

                    using (SqlDataAdapter da = new SqlDataAdapter(sqlCmd))
                    using (DataSet ds = new DataSet())
                    {
                        da.Fill(ds);

                        firstRun = dgvReceitas.DataSource == null;

                        dgvReceitas.DataSource = ds.Tables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AtualizaListaReceitas(): " + ex.Message);
                new CaixaMensagem(ex.Message, "AtualizaListaReceitas()", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
            }
            finally
            {
                if (firstRun && dgvReceitas.RowCount > 0 && dgvReceitas.ColumnCount == 5)
                {
                    //Selecionar o tamanho das colunas
                    dgvReceitas.Columns[0].Width = 20; //ID
                    dgvReceitas.Columns[2].Width = 40; //Facas/Celulas
                    dgvReceitas.Columns[3].Width = 40; //NºCelulas
                    dgvReceitas.Columns[4].Width = 135; //Última modificação

                }

                dgvReceitas.ClearSelection();

                //Selecionar o row consoante o ID
                for (int i = 0; i < dgvReceitas.RowCount; i++)
                {
                    if (Forms.MainForm.Receita.ID == Convert.ToInt32(dgvReceitas.Rows[i].Cells[0].Value))
                    {
                        dgvReceitas.Rows[i].Selected = true;
                        break;
                    }
                }


                label2.Text = Convert.ToString(dgvReceitas.RowCount);
            }
        }


        private void GestaoReceitas_Load(object sender, EventArgs e)
        {
            this.AtualizaListaReceitas(string.Empty);
        }

        private void dgvReceitas_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvReceitas.RowCount > 0 && dgvReceitas.SelectedRows.Count > 0)
                label87.Text = Convert.ToString(dgvReceitas.SelectedRows[0].Cells[1].Value);
            else
                label87.Text = "Sem seleção";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgvReceitas.RowCount > 0 && dgvReceitas.SelectedRows.Count > 0)
                new FormularioReceita(this.strConexaoDb, FormularioReceita.TipoAcesso.Ver, Convert.ToInt32(dgvReceitas.SelectedRows[0].Cells[0].Value)).ShowDialog();
        }

        private void button52_Click(object sender, EventArgs e)
        {
            if (dgvReceitas.RowCount > 0 && dgvReceitas.SelectedRows.Count > 0)
            {
                //Coloca camera em modo video
                Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Int, 1, 47, 6);
                
                new FormularioReceita(this.strConexaoDb, FormularioReceita.TipoAcesso.Editar, Convert.ToInt32(dgvReceitas.SelectedRows[0].Cells[0].Value)).ShowDialog();
            }
            this.AtualizaListaReceitas(txtNumReferencia.Text);
            //coloca camera em modo trigger
            Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Int, 2, 47, 6);


        }

        private void button53_Click(object sender, EventArgs e)
        {
            new FormularioReceita(this.strConexaoDb, FormularioReceita.TipoAcesso.Adicionar).ShowDialog();

            this.AtualizaListaReceitas(txtNumReferencia.Text);
        }

        private void button54_Click(object sender, EventArgs e)
        {
            if (dgvReceitas.RowCount > 0 && dgvReceitas.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvReceitas.SelectedRows[0].Cells[0].Value);
                int numOfRows = 0;

                if (id <= 0)
                {
                    new CaixaMensagem("A receita selecionada não é válida!", "Verificações", CaixaMensagem.TipoMsgBox.Warning, FormStartPosition.CenterScreen).ShowDialog();
                    return;
                }

                if (Forms.MainForm.Receita.ReceitaCarregada && Forms.MainForm.Receita.ID == id)
                    new CaixaMensagem("A receita selecionada está atualmente em uso. Por favor carregue outra receita e tente novamente!", "Receita em Uso", CaixaMensagem.TipoMsgBox.Warning, FormStartPosition.CenterScreen).ShowDialog();
                else
                  if (new CaixaMensagem("Deseja eliminar a receita '" + label87.Text + "'? Esta ação é irreversível!", "Eliminar Receita", CaixaMensagem.TipoMsgBox.Question, FormStartPosition.CenterScreen).ShowDialog() == DialogResult.Yes)
                    try
                    {
                        using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDb))
                        using (SqlCommand sqlCmd = new SqlCommand("DELETE FROM RECEITA WHERE ID = @ID", sqlConn))
                        {
                            sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = id;

                            sqlConn.Open();

                            numOfRows = sqlCmd.ExecuteNonQuery();
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("button54_Click(): " + ex.Message);
                        new CaixaMensagem(ex.Message, "Eliminar Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
                    }
                    finally
                    {
                        if (numOfRows == 1)
                        {
                            new CaixaMensagem("Receita '" + label87.Text + "' eliminada com sucesso!", "Receita Eliminada", CaixaMensagem.TipoMsgBox.Normal, FormStartPosition.CenterScreen).ShowDialog();

                            //Atualiza a lista das receitas
                            this.AtualizaListaReceitas(txtNumReferencia.Text);
                        }
                        else
                            new CaixaMensagem("Erro ao eliminar receita em base de dados!", "Eliminar Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
                    }

            }

        }

        private void label87_TextChanged(object sender, EventArgs e)
        {
            label87.ForeColor = label87.Text == "Sem seleção" ? Color.White : Color.LimeGreen;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            label2.Text = dgvReceitas.Rows.Count.ToString();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.AtualizaListaReceitas(txtNumReferencia.Text);
        }
    }
}
