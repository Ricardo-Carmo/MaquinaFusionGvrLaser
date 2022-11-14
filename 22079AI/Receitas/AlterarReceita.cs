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
    public partial class AlterarReceita : Form
    {
        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);
        private string strConexaoDb = string.Empty;


        public AlterarReceita(string _strConexaoDb)
        {
            InitializeComponent();

            this.strConexaoDb = _strConexaoDb;
        }


        private void AtualizaListaReceitas(string value)
        {
            bool firstRun = false;
            string strQuery = "SELECT ID, NOME AS 'Referência', DESCRICAO AS 'Descrição', COMPRIMENTO_NOMINAL AS 'Comprimento Nominal',  ULTIMA_MODIFICACAO AS 'Última Modificação' FROM RECEITA WHERE ID > 0 ";

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
                    dgvReceitas.Columns[2].Width = 40; //descricao
                    dgvReceitas.Columns[3].Width = 40; //comprimento
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


        private void AlterarReceita_Load(object sender, EventArgs e)
        {
            this.AtualizaListaReceitas(string.Empty);
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dgvReceitas.RowCount > 0 && dgvReceitas.SelectedRows.Count > 0)
                if (Forms.MainForm.Receita.AtualizaReceita(Convert.ToInt32(dgvReceitas.SelectedRows[0].Cells[0].Value), Forms.MainForm.UserSession.IDOperador, string.Empty, DateTime.Now, true))
                {
                    //Enviar para o PLC a flag de nova receita
                    //Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 20, 4, 0);

                    //limpa os aneis processados
                    Forms.MainForm.dgvUltimosAneisProcessados.Rows.Clear();

                    this.Close();
                }
                else
                    new CaixaMensagem("Erro ao tentar alterar a receita.", "Alterar Receita", CaixaMensagem.TipoMsgBox.Error, FormStartPosition.CenterScreen).ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dgvReceitas.RowCount > 0 && dgvReceitas.SelectedRows.Count > 0)
                new FormularioReceita(this.strConexaoDb, FormularioReceita.TipoAcesso.Ver, Convert.ToInt32(dgvReceitas.SelectedRows[0].Cells[0].Value)).ShowDialog();

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

        private void button3_Click(object sender, EventArgs e)
        {
            this.AtualizaListaReceitas(this.txtNumReferencia.Text);
        }

        private void txtNumReferencia_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                e.Handled = true;

                this.AtualizaListaReceitas(this.txtNumReferencia.Text);
            }
        }
    }
}
