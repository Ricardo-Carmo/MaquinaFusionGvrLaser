using _22079AI.Properties;
using PLC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace _22079AI
{
    public class Receitas
    {
        //**Variaveis**
        //Ultima receita carregada na maquina
        public ParamReceita _ReceitaCarregada = new ParamReceita();
        //Ultima Receita a editar na maquina
        public ParamReceita _ReceitaEditar = new ParamReceita();
        //ultimo utilizador carregadp
        public UserData _UserLoaded = new UserData();
        //Estrutura limpa para limpar campos antes de escrita ou leitura
        public ParamReceita Clear = new ParamReceita();
        //Variavel global indica que a receita foi carregada na maquina
        public bool ReceitaCarregada
        {
            get;
            private set;
        }
        //Data hora da alteracao a ser submetida
        public DateTime DataHoraAlteracao
        {
            get;
            private set;
        }
        //string de ligacao a base de dados 
        private string strConexaoDB = string.Empty;


        //**Definicoes**
        //Parametros relativos a receita na maquina
        public struct ParamReceita
        {

            public int ID;
            public string NOME;
            public string DESCRICAO;
            public DateTime DATA_MODIFICACAO;
            public DateTime DATA_CRIACAO;
            public ParamEncascado ENCASCADO_LEVE;
            public ParamEncascado ENCASCADO_MODERADO;
            public ParamEncascado ENCASCADO_VINCADO;

        }
        //parametros relativos a estrutura do encascado
        public struct ParamEncascado
        {
            public int ID;
            public float COMPRIMENTO;
            public float ESPESSURA;
            public float TONALIDADE_MAX;
            public float TONALIDADE_MIN;

        }
        //parametros associados a tabela de utilizadores 
        public struct UserData
        {
            public short ID;
            public string NOME;
            public string PASSWORD;
            public short ID_NIVEL;
            public DateTime DATA_HORA;
        }
        //salva dados de carregamento de receita
 

        public Receitas(string _strConexaoDB)
        {
            this.strConexaoDB = _strConexaoDB;

            //Verificar se temos uma receita aberta
            this.CarregaDefinicoesFicheiroINI();

            if (this.ReceitaCarregada)
                this.AtualizaReceita(_ReceitaCarregada.ID, _UserLoaded.ID, this.DataHoraAlteracao, false);
        }

        public bool AtualizaReceita(int _idReceita, int _idOperador, DateTime _dataHoraInicio, bool _descarregaReceitaPLC)
        {
            try
            {
                if (_idReceita <= 0)
                    throw new Exception("Receita ID == 0");

                if (_idOperador <= 0)
                    throw new Exception("User ID == 0");

                if (this.ReceitaCarregada)
                    this.LimpaDados(false);


                //comandos SQL efetuados para carregar as receitas no inicio da maquina
                using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDB))
                {
                    //Preenche os dados da 1 tabela da estrutura da receita
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA WHERE ID = @ID", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                        sqlConn.Open();

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr.Read())
                            {
                                _ReceitaCarregada = Clear;

                                _ReceitaCarregada.ID = Convert.ToInt32(dr[0]);
                                _ReceitaCarregada.NOME = Convert.ToString(dr[1]);
                                _ReceitaCarregada.DESCRICAO = Convert.ToString(dr[2]);
                                _ReceitaCarregada.DATA_MODIFICACAO = Convert.ToDateTime(dr[3]);
                                _ReceitaCarregada.DATA_CRIACAO = Convert.ToDateTime(dr[4]);

                                this.DataHoraAlteracao = _dataHoraInicio;
                                this.ReceitaCarregada = true;
                            }
                            else
                            {
                                this.LimpaDados(true);

                                throw new Exception("Sem dados lidos. ID: " + _idReceita);
                            }

                        sqlConn.Close();
                    }
                    //Preenche os dados da tabela Encascado LEVE
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA_ENCASCADO_LEVE WHERE KEY_ID_RECEITA = @ID", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                        sqlConn.Open();

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr.Read())
                            {
                                

                                _ReceitaCarregada.ENCASCADO_LEVE.ID = Convert.ToInt32(dr[1]);
                                _ReceitaCarregada.ENCASCADO_LEVE.COMPRIMENTO = (float)Convert.ToDecimal(dr[2]);
                                _ReceitaCarregada.ENCASCADO_LEVE.ESPESSURA = (float)Convert.ToDecimal(dr[3]);
                                _ReceitaCarregada.ENCASCADO_LEVE.TONALIDADE_MAX = (float)Convert.ToDecimal(dr[4]);
                                _ReceitaCarregada.ENCASCADO_LEVE.TONALIDADE_MIN = (float)Convert.ToDecimal(dr[5]);

                                this.DataHoraAlteracao = _dataHoraInicio;
                                this.ReceitaCarregada = true;
                            }
                            else
                            {
                                this.LimpaDados(true);

                                throw new Exception("Sem dados lidos. ID: " + _idReceita);
                            }

                        sqlConn.Close();
                    }
                    //Preenche os dados da tabela Encascado MODERADO
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA_ENCASCADO_MODERADO WHERE KEY_ID_RECEITA = @ID", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                        sqlConn.Open();

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr.Read())
                            {
                                

                                _ReceitaCarregada.ENCASCADO_MODERADO.ID = Convert.ToInt32(dr[1]);
                                _ReceitaCarregada.ENCASCADO_MODERADO.COMPRIMENTO = (float)Convert.ToDecimal(dr[2]);
                                _ReceitaCarregada.ENCASCADO_MODERADO.ESPESSURA = (float)Convert.ToDecimal(dr[3]);
                                _ReceitaCarregada.ENCASCADO_MODERADO.TONALIDADE_MAX = (float)Convert.ToDecimal(dr[4]);
                                _ReceitaCarregada.ENCASCADO_MODERADO.TONALIDADE_MIN = (float)Convert.ToDecimal(dr[5]);

                                this.DataHoraAlteracao = _dataHoraInicio;
                                this.ReceitaCarregada = true;
                            }
                            else
                            {
                                this.LimpaDados(true);

                                throw new Exception("Sem dados lidos. ID: " + _idReceita);
                            }

                        sqlConn.Close();
                    }
                    //Preenche os dados da tabela Encascado VINCADO
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA_ENCASCADO_VINCADO WHERE KEY_ID_RECEITA = @ID", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                        sqlConn.Open();

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr.Read())
                            {
                               

                                _ReceitaCarregada.ENCASCADO_VINCADO.ID = Convert.ToInt32(dr[1]);
                                _ReceitaCarregada.ENCASCADO_VINCADO.COMPRIMENTO = ((float)Convert.ToDecimal(dr[2]));
                                _ReceitaCarregada.ENCASCADO_VINCADO.ESPESSURA = ((float)Convert.ToDecimal(dr[3]));
                                _ReceitaCarregada.ENCASCADO_VINCADO.TONALIDADE_MAX = (float)Convert.ToDecimal(dr[4]);
                                _ReceitaCarregada.ENCASCADO_VINCADO.TONALIDADE_MIN = (float)Convert.ToDecimal(dr[5]);

                                this.DataHoraAlteracao = _dataHoraInicio;
                                this.ReceitaCarregada = true;
                            }
                            else
                            {
                                this.LimpaDados(true);

                                throw new Exception("Sem dados lidos. ID: " + _idReceita);
                            }

                        sqlConn.Close();
                    }
                    //Descarregar o nome do operador
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM UTILIZADORES WHERE ID = @ID", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@ID", SqlDbType.TinyInt).Value = _idOperador;

                        sqlConn.Open();

                        //_UserLoaded.NOME = Convert.ToString(sqlCmd.ExecuteScalar());

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr.Read())
                            {


                                _UserLoaded.ID = Convert.ToInt16(dr[0]);
                                _UserLoaded.NOME = dr[1].ToString();
                                _UserLoaded.PASSWORD = dr[2].ToString();
                                _UserLoaded.ID_NIVEL = Convert.ToInt16(dr[3]);
                                _UserLoaded.DATA_HORA = Convert.ToDateTime(dr[4]);

                                this.DataHoraAlteracao = _dataHoraInicio;
                                this.ReceitaCarregada = true;
                            }


                        sqlConn.Close();
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Atualizar Receita", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (this.ReceitaCarregada)
                {
                    this.GravaDefinicoesFicheiroINI();

                    if (_descarregaReceitaPLC)
                        this.DescarregaReceitaPLC();
                }
            }
            return this.ReceitaCarregada;
        }

        public int EscreveReceitaDb(ref ParamReceita FlagRecipie, FormularioReceita.TipoAcesso Acesso,int _idReceita )
        {
            string strQueryNormalTable = string.Empty;
            string strQueryLeveTable = string.Empty;
            string strQueryModeradoTable = string.Empty;
            string strQueryVinacadoTable = string.Empty;
            int NovoIdCriado = 0;
            int rows = 0;
            try 
            { 
                //comandos SQL efetuados para carregar as receitas no inicio da maquina
                using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDB))
                {

                    if (Acesso == FormularioReceita.TipoAcesso.Adicionar)
                    {
                        strQueryNormalTable = "INSERT INTO RECEITA ([NOME], [DESCRICAO], [DATA_MODIFICACAO], [DATA_CRIACAO] ) VALUES (@NOME, @DESCRICAO, GETDATE(), GETDATE())";
                        strQueryLeveTable = "INSERT INTO RECEITA_ENCASCADO_LEVE ([COMPRIMENTO], [ESPESSURA], [TONALIDADE_MAX], [TONALIDADE_MIN], [KEY_ID_RECEITA] ) VALUES (@COMPRIMENTO, @ESPESSURA, @TONALIDADE_MAX, @TONALIDADE_MIN, @KEY_ID_RECEITA)";
                        strQueryModeradoTable = "INSERT INTO RECEITA_ENCASCADO_MODERADO ([COMPRIMENTO], [ESPESSURA], [TONALIDADE_MAX], [TONALIDADE_MIN], [KEY_ID_RECEITA] ) VALUES (@COMPRIMENTO, @ESPESSURA, @TONALIDADE_MAX, @TONALIDADE_MIN, @KEY_ID_RECEITA)";
                        strQueryVinacadoTable = "INSERT INTO RECEITA_ENCASCADO_VINCADO ([COMPRIMENTO], [ESPESSURA], [TONALIDADE_MAX], [TONALIDADE_MIN], [KEY_ID_RECEITA] ) VALUES (@COMPRIMENTO, @ESPESSURA, @TONALIDADE_MAX, @TONALIDADE_MIN, @KEY_ID_RECEITA)";
                    }
                        
                    else if (Acesso == FormularioReceita.TipoAcesso.Editar)
                    {
                        strQueryNormalTable = "UPDATE RECEITA SET [NOME] = @NOME, [DESCRICAO] = @DESCRICAO, [DATA_MODIFICACAO] = GETDATE() WHERE ID = @ID";
                        strQueryLeveTable = "UPDATE RECEITA_ENCASCADO_LEVE SET [COMPRIMENTO] = @COMPRIMENTO, [ESPESSURA] = @ESPESSURA, [TONALIDADE_MAX]=@TONALIDADE_MAX, [TONALIDADE_MIN]=@TONALIDADE_MIN  WHERE KEY_ID_RECEITA = @ID";
                        strQueryModeradoTable = "UPDATE RECEITA_ENCASCADO_MODERADO SET [COMPRIMENTO] = @COMPRIMENTO, [ESPESSURA] = @ESPESSURA, [TONALIDADE_MAX]=@TONALIDADE_MAX, [TONALIDADE_MIN]=@TONALIDADE_MIN  WHERE KEY_ID_RECEITA = @ID";
                        strQueryVinacadoTable = "UPDATE RECEITA_ENCASCADO_VINCADO SET [COMPRIMENTO] = @COMPRIMENTO, [ESPESSURA] = @ESPESSURA, [TONALIDADE_MAX]=@TONALIDADE_MAX, [TONALIDADE_MIN]=@TONALIDADE_MIN  WHERE KEY_ID_RECEITA = @ID";
                    }
                        
                    if (!string.IsNullOrEmpty(strQueryNormalTable) && !string.IsNullOrEmpty(strQueryLeveTable) && !string.IsNullOrEmpty(strQueryModeradoTable) && !string.IsNullOrEmpty(strQueryVinacadoTable)) {
                        //Preenche os dados da 1 tabela da estrutura da receita
                        using (SqlCommand sqlCmd = new SqlCommand(strQueryNormalTable, sqlConn))
                            {
                        
                            sqlCmd.Parameters.Add("@NOME", SqlDbType.NVarChar).Value = FlagRecipie.NOME;
                            sqlCmd.Parameters.Add("@DESCRICAO", SqlDbType.NVarChar).Value = FlagRecipie.DESCRICAO;

                            if (Acesso == FormularioReceita.TipoAcesso.Editar)
                                sqlCmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = _idReceita;

                            sqlConn.Open();
                            rows = Convert.ToInt32(sqlCmd.ExecuteNonQuery());

                            sqlConn.Close();
                            Debug.WriteLine("adicionado tabela receitas ID: " + _idReceita.ToString() + "rows: " + rows);
                        }

                        

                        if (Acesso == FormularioReceita.TipoAcesso.Adicionar)
                        {
                            using (SqlCommand sqlCmd = new SqlCommand("SELECT MAX(ID) AS LastID FROM RECEITA", sqlConn))
                            {
                                //sqlCmd.Parameters.Add("@NOME", SqlDbType.SmallInt).Value = FlagRecipie.NOME;

                                sqlConn.Open();

                                using (SqlDataReader dr = sqlCmd.ExecuteReader())
                                    if (dr.Read())
                                    {
                                        _idReceita = Convert.ToInt32(dr[0]);
                                    }
                                    else
                                    {
                                        this.LimpaDados(true);

                                        throw new Exception("Sem dados lidos. ID: " + _idReceita);
                                    }
                                Debug.WriteLine("verificado que foi introduzido o ID: " + _idReceita.ToString() );
                                sqlConn.Close();
                            }
                        }

                        

                        //Preenche os dados da tabela Encascado LEVE
                        using (SqlCommand sqlCmd = new SqlCommand(strQueryLeveTable, sqlConn))
                    {                      

                                sqlCmd.Parameters.Add("@COMPRIMENTO", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_LEVE.COMPRIMENTO;
                                sqlCmd.Parameters.Add("@ESPESSURA", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_LEVE.ESPESSURA;
                                sqlCmd.Parameters.Add("@TONALIDADE_MAX", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_LEVE.TONALIDADE_MAX;
                                sqlCmd.Parameters.Add("@KEY_ID_RECEITA", SqlDbType.BigInt).Value = _idReceita;
                                sqlCmd.Parameters.Add("@TONALIDADE_MIN", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_LEVE.TONALIDADE_MIN;

                                if (Acesso == FormularioReceita.TipoAcesso.Editar)
                                    sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                                sqlConn.Open();
                            rows = Convert.ToInt32(sqlCmd.ExecuteNonQuery());
                            Debug.WriteLine("adicionado tabela Encascado LEVE ID: " + _idReceita.ToString()  + "rows: " + rows);
                            sqlConn.Close();
                        }

                        

                        //Preenche os dados da tabela Encascado MODERADO
                        using (SqlCommand sqlCmd = new SqlCommand(strQueryModeradoTable, sqlConn))
                        {

                            sqlCmd.Parameters.Add("@COMPRIMENTO", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.COMPRIMENTO;
                            sqlCmd.Parameters.Add("@ESPESSURA", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.ESPESSURA;
                            sqlCmd.Parameters.Add("@TONALIDADE_MAX", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.TONALIDADE_MAX;
                            sqlCmd.Parameters.Add("@KEY_ID_RECEITA", SqlDbType.BigInt).Value = _idReceita;
                            sqlCmd.Parameters.Add("@TONALIDADE_MIN", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.TONALIDADE_MIN;

                            if (Acesso == FormularioReceita.TipoAcesso.Editar)
                                sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                            sqlConn.Open();
                            rows = Convert.ToInt32(sqlCmd.ExecuteNonQuery());
                            Debug.WriteLine("adicionado tabela Encascado MODERADO ID: " + _idReceita.ToString() + "rows: " + rows);
                            sqlConn.Close();
                        }

                        
                        //Preenche os dados da tabela Encascado VINCADO
                        using (SqlCommand sqlCmd = new SqlCommand(strQueryVinacadoTable, sqlConn))
                        {

                            sqlCmd.Parameters.Add("@COMPRIMENTO", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.COMPRIMENTO;
                            sqlCmd.Parameters.Add("@ESPESSURA", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.ESPESSURA;
                            sqlCmd.Parameters.Add("@TONALIDADE_MAX", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.TONALIDADE_MAX;
                            sqlCmd.Parameters.Add("@KEY_ID_RECEITA", SqlDbType.BigInt).Value = _idReceita;
                            sqlCmd.Parameters.Add("@TONALIDADE_MIN", SqlDbType.Float).Value = FlagRecipie.ENCASCADO_MODERADO.TONALIDADE_MIN;

                            if (Acesso == FormularioReceita.TipoAcesso.Editar)
                                sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                            sqlConn.Open();
                            rows = Convert.ToInt32(sqlCmd.ExecuteNonQuery());
                            Debug.WriteLine("adicionado tabela Encascado VINCADO ID: " + _idReceita.ToString() + "rows: " + rows);
                            sqlConn.Close();
                        }

                       
                    }
                    else
                        throw new Exception("Query nula com o id receita:  " + _idReceita);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "EscreveReceitaDb - erro na escrita", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //if (this.ReceitaCarregada)
                //{
                //    this.GravaDefinicoesFicheiroINI();

                    //if (_descarregaReceitaPLC)
                    //    this.DescarregaReceitaPLC();
                //}
            }
            return rows;
        }

        public int LeReceitaDb(ref ParamReceita FlagRecipie, short _idReceita)
        {
            string strQueryNormalTable = string.Empty;
            string strQueryLeveTable = string.Empty;
            string strQueryModeradoTable = string.Empty;
            string strQueryVinacadoTable = string.Empty;
            int rows = 0;
            try
            {

                
                
                    //Preenche os dados da 1 tabela da estrutura da receita
                    //comandos SQL efetuados para carregar as receitas no inicio da maquina
                    using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDB))
                    {
                        //Preenche os dados da 1 tabela da estrutura da receita
                        using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA WHERE ID = @ID", sqlConn))
                        {
                            sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                            sqlConn.Open();

                            using (SqlDataReader dr = sqlCmd.ExecuteReader())
                                if (dr.Read())
                                {
                                    FlagRecipie = Clear;

                                    FlagRecipie.ID = Convert.ToInt32(dr[0]);
                                    FlagRecipie.NOME = Convert.ToString(dr[1]);
                                    FlagRecipie.DESCRICAO = Convert.ToString(dr[2]);
                                    FlagRecipie.DATA_MODIFICACAO = Convert.ToDateTime(dr[3]);
                                    FlagRecipie.DATA_CRIACAO = Convert.ToDateTime(dr[4]);

                                }
                                else
                                {
                                    this.LimpaDados(true);

                                    throw new Exception("Sem dados lidos. ID: " + _idReceita);
                                }

                            sqlConn.Close();
                        }
                        //Preenche os dados da tabela Encascado LEVE
                        using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA_ENCASCADO_LEVE WHERE KEY_ID_RECEITA = @ID", sqlConn))
                        {
                            sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                            sqlConn.Open();

                            using (SqlDataReader dr = sqlCmd.ExecuteReader())
                                if (dr.Read())
                                {


                                    FlagRecipie.ENCASCADO_LEVE.ID = Convert.ToInt32(dr[0]);
                                    FlagRecipie.ENCASCADO_LEVE.COMPRIMENTO = (float)Convert.ToDecimal(dr[2]);
                                    FlagRecipie.ENCASCADO_LEVE.ESPESSURA = (float)Convert.ToDecimal(dr[3]);
                                    FlagRecipie.ENCASCADO_LEVE.TONALIDADE_MAX = (float)Convert.ToDecimal(dr[4]);
                                    FlagRecipie.ENCASCADO_LEVE.TONALIDADE_MIN = (float)Convert.ToDecimal(dr[5]);


                                    this.ReceitaCarregada = true;
                                }
                                else
                                {
                                    this.LimpaDados(true);

                                    throw new Exception("Sem dados lidos. ID: " + _idReceita);
                                }

                            sqlConn.Close();
                        }
                        //Preenche os dados da tabela Encascado MODERADO
                        using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA_ENCASCADO_MODERADO WHERE KEY_ID_RECEITA = @ID", sqlConn))
                        {
                            sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                            sqlConn.Open();

                            using (SqlDataReader dr = sqlCmd.ExecuteReader())
                                if (dr.Read())
                                {


                                    FlagRecipie.ENCASCADO_MODERADO.ID = Convert.ToInt32(dr[0]);
                                    FlagRecipie.ENCASCADO_MODERADO.COMPRIMENTO = (float)Convert.ToDecimal(dr[2]);
                                    FlagRecipie.ENCASCADO_MODERADO.ESPESSURA = (float)Convert.ToDecimal(dr[3]);
                                    FlagRecipie.ENCASCADO_MODERADO.TONALIDADE_MAX = (float)Convert.ToDecimal(dr[4]);
                                    FlagRecipie.ENCASCADO_MODERADO.TONALIDADE_MIN = (float)Convert.ToDecimal(dr[5]);


                                    this.ReceitaCarregada = true;
                                }
                                else
                                {
                                    this.LimpaDados(true);

                                    throw new Exception("Sem dados lidos. ID: " + _idReceita);
                                }

                            sqlConn.Close();
                        }
                        //Preenche os dados da tabela Encascado VINCADO
                        using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA_ENCASCADO_VINCADO WHERE KEY_ID_RECEITA = @ID", sqlConn))
                        {
                            sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                            sqlConn.Open();

                            using (SqlDataReader dr = sqlCmd.ExecuteReader())
                                if (dr.Read())
                                {


                                    FlagRecipie.ENCASCADO_VINCADO.ID = Convert.ToInt32(dr[0]);
                                    FlagRecipie.ENCASCADO_VINCADO.COMPRIMENTO = (float)Convert.ToDecimal(dr[2]);
                                    FlagRecipie.ENCASCADO_VINCADO.ESPESSURA = (float)Convert.ToDecimal(dr[3]);
                                    FlagRecipie.ENCASCADO_VINCADO.TONALIDADE_MAX = (float)Convert.ToDecimal(dr[4]);
                                    FlagRecipie.ENCASCADO_VINCADO.TONALIDADE_MIN = (float)Convert.ToDecimal(dr[5]);


                                    this.ReceitaCarregada = true;
                                }
                                else
                                {
                                    this.LimpaDados(true);

                                    throw new Exception("Sem dados lidos. ID: " + _idReceita);
                                }

                            sqlConn.Close();
                        }

                    }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "EscreveReceitaDb - erro na escrita", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //if (this.ReceitaCarregada)
                //{
                //    this.GravaDefinicoesFicheiroINI();

                //if (_descarregaReceitaPLC)
                //    this.DescarregaReceitaPLC();
                //}
            }
            return rows;
        }

        public static class SQLHelper
        {

            public static DataTable GetDataTable(SqlConnection sqlConn, string sql, List<SqlParameter> list)
            {
                DataTable dt = new DataTable();
                try
                {
                    using (SqlCommand sqlCmd = new SqlCommand(sql, sqlConn))
                    {
                        for (int i = 0; i < list.Count; i++)
                            sqlCmd.Parameters.Add(list[i]);


                        sqlConn.Open();

                        using (SqlDataReader reader = sqlCmd.ExecuteReader())
                            dt.Load(reader);
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQLHelper - GetDataTable(): " + ex.Message);



                    return null;
                }
                finally
                {
                    sqlConn.Close();
                }
                return dt;
            }

            public static int ExecuteNonQuery(string sql, string strConexao)
            {
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(strConexao))
                    using (SqlCommand sqlCmd = new SqlCommand(sql, sqlConn))
                    {
                        sqlConn.Open();

                        return sqlCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQLHelper - ExecuteNonQuery(): " + ex.Message);



                    return -1;
                }
            }

            public static int ExecuteNonQuery(string sql, string strConexao, List<SqlParameter> list, bool showErrorMsg = true)
            {
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(strConexao))
                    using (SqlCommand sqlCmd = new SqlCommand(sql, sqlConn))
                    {
                        for (int i = 0; i < list.Count; i++)
                            sqlCmd.Parameters.Add(list[i]);

                        sqlConn.Open();

                        return sqlCmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQLHelper - ExecuteNonQuery(): " + ex.Message);

                    if (showErrorMsg)
                    {
                        ;
                    }

                    return -1;
                }
            }


            public static int[] ExecuteNonQuery(string[] sql, string strConexao)
            {
                List<int> numOfRows = new List<int>();

                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(strConexao))
                        foreach (string str in sql)
                            using (SqlCommand sqlCmd = new SqlCommand(str, sqlConn))
                            {
                                sqlConn.Open();

                                numOfRows.Add(sqlCmd.ExecuteNonQuery());

                                sqlConn.Close();
                            }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQLHelper - ExecuteNonQuery(): " + ex.Message);


                }

                return numOfRows.ToArray();
            }

            public static int[] ExecuteNonQuery(string[] sql, string strConexao, List<SqlParameter> list)
            {
                List<int> numOfRows = new List<int>();

                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(strConexao))
                        foreach (string str in sql)
                            using (SqlCommand sqlCmd = new SqlCommand(str, sqlConn))
                            {
                                for (int i = 0; i < list.Count; i++)
                                    sqlCmd.Parameters.Add(list[i]);

                                sqlConn.Open();

                                numOfRows.Add(sqlCmd.ExecuteNonQuery());

                                sqlConn.Close();
                            }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQLHelper - ExecuteNonQuery(): " + ex.Message);

                }

                return numOfRows.ToArray();
            }


            public static object ExecuteScalar(string sql, string strConexao)
            {
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(strConexao))
                    using (SqlCommand sqlCmd = new SqlCommand(sql, sqlConn))
                    {
                        sqlConn.Open();

                        return sqlCmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQLHelper - ExecuteScalar(): " + ex.Message);


                    return 0;
                }
            }
            public static object ExecuteScalar(string sql, string strConexao, List<SqlParameter> list)
            {
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(strConexao))
                    using (SqlCommand sqlCmd = new SqlCommand(sql, sqlConn))
                    {
                        for (int i = 0; i < list.Count; i++)
                            sqlCmd.Parameters.Add(list[i]);

                        sqlConn.Open();

                        return sqlCmd.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("SQLHelper - ExecuteScalar(): " + ex.Message);



                    return 0;
                }

            }


        }

        public void AlteraNumSerie(int _value)
        {
            if (_ReceitaCarregada.ID != _value)
            {
                _ReceitaCarregada.ID = _value;

                using (FicheiroINI ini = new FicheiroINI(Forms.MainForm.VariaveisAuxiliares.iniPath))
                    ini.EscreveFicheiroINI("Receita", "NumSerie", _ReceitaCarregada.ID.ToString());
            }
        }

        private void GravaDefinicoesFicheiroINI()
        {
            using (FicheiroINI ini = new FicheiroINI(Forms.MainForm.VariaveisAuxiliares.iniPath))
            {
                ini.EscreveFicheiroINI("Receita", "ReceitaCarregada", this.ReceitaCarregada ? "1" : "0");
                ini.EscreveFicheiroINI("Receita", "idReceita", _ReceitaCarregada.ID.ToString());
                ini.EscreveFicheiroINI("Receita", "idOperador", _UserLoaded.ID.ToString());
                ini.EscreveFicheiroINI("Receita", "NomeReceita", _ReceitaCarregada.NOME);
                ini.EscreveFicheiroINI("Receita", "DataHoraAlteracao", _ReceitaCarregada.DATA_MODIFICACAO.ToString(@"dd\_MM\_yyyy\_HH\_mm\_ss\_fff"));
                ini.EscreveFicheiroINI("Receita", "NomeOperador", _UserLoaded.NOME);
            }
        }

        private void CarregaDefinicoesFicheiroINI()
        {
            using (FicheiroINI ini = new FicheiroINI(Forms.MainForm.VariaveisAuxiliares.iniPath))
            {
                this.ReceitaCarregada = ini.RetornaTrueFalseDeStringFicheiroINI("Receita", "ReceitaCarregada", false);
                _ReceitaCarregada.ID = Convert.ToInt32(ini.RetornaINI("Receita", "idReceita", Convert.ToString(_ReceitaCarregada.ID)));
                _UserLoaded.ID = Convert.ToInt16(ini.RetornaINI("Receita", "idOperador", Convert.ToString(_UserLoaded.ID)));
                _ReceitaCarregada.NOME = ini.RetornaINI("Receita", "NomeReceita", _ReceitaCarregada.NOME);
                //_ReceitaCarregada.DATA_MODIFICACAO = Diversos.ConvertUnixParaDatetime(Convert.ToInt32(ini.RetornaINI("Receita", "DataHoraAlteracao", "0")));
                _UserLoaded.NOME = ini.RetornaINI("Receita", "NomeOperador", _UserLoaded.NOME);
            }
        }

        private void DescarregaReceitaPLC()
        {
            try
            {
                if (!this.ReceitaCarregada)
                    throw new Exception("Sem receita carregada!");

                //Forms.MainForm.PLC1.EnviaTagRT(Siemens.MemoryArea.DB, Siemens.TipoVariavel.DInt, this.ComprimentoNominal, 23, 4);
                //Forms.MainForm.PLC1.EnviaTagRT(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Int, this.SpOK, 23, 12);
                //Forms.MainForm.PLC1.EnviaTagRT(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Int, this.SpNOK, 23, 14);

                //Tag de valores de recita actualizados
                //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, true, 23, 0, 2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DescarregaReceitaPLC(): " + ex.Message);
            }
        }

        private void LimpaDados(bool gravaINI)
        {
            _ReceitaCarregada = Clear;

            this.ReceitaCarregada = false;

            if (gravaINI)
                this.GravaDefinicoesFicheiroINI();
        }


    }
}
