using _22079AI.Properties;
using PLC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public class Receitas
    {
        public bool ReceitaCarregada
        {
            get;
            private set;
        }

        public int ID { get; private set; } = 0;

        public DateTime DataHoraAlteracao
        {
            get;
            private set;
        }


        public string Referencia
        {
            get;
            private set;
        }

        public string Designacao
        {
            get;
            private set;
        }

        public string NumSerie
        {
            get;
            set;
        }

        public int CompRegiaoInsp
        {
            get;
            set;
        }

        public double ComprimentoNominal
        {
            get;
            set;
        }

        public double ToleranciaComprimento
        {
            get;
            set;
        }

        public double DesvioNominal
        {
            get;
            set;
        }

        public double ToleranciaDesvioSuperior
        {
            get;
            set;
        }

        public double ToleranciaDesvioInferior
        {
            get;
            set;
        }

        public int SpOK
        {
            get;
            set;
        }

        public int SpNOK
        {
            get;
            set;
        }

        private string fileNameImagemFaca = string.Empty;

        private int idOperador = 0;

        public string NomeOperador
        {
            get;
            private set;
        }

        private string strConexaoDB = string.Empty;

        private string robotAddress = string.Empty;

        public Receitas(string _strConexaoDB, string _robotAddress)
        {
            this.strConexaoDB = _strConexaoDB;

            this.robotAddress = _robotAddress;

            this.NumSerie = string.Empty; //Trick

            //Verificar se temos uma receita aberta
            this.CarregaDefinicoesFicheiroINI();

            if (this.ReceitaCarregada)
                this.AtualizaReceita(this.ID, this.idOperador, this.NumSerie, this.DataHoraAlteracao, false);
        }

        public bool AtualizaReceita(int _idReceita, int _idOperador, string _numSerie, DateTime _dataHoraInicio, bool _descarregaReceitaPLC)
        {
            try
            {
                if (_idReceita <= 0)
                    throw new Exception("Receita ID == 0");

                if (_idOperador <= 0)
                    throw new Exception("User ID == 0");

                if (this.ReceitaCarregada)
                    this.LimpaDados(false);

                using (SqlConnection sqlConn = new SqlConnection(this.strConexaoDB))
                {
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM RECEITA WHERE ID = @ID", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@ID", SqlDbType.SmallInt).Value = _idReceita;

                        sqlConn.Open();

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr.Read())
                            {
                                this.ID = Convert.ToInt32(dr[0]);
                                this.Referencia = Convert.ToString(dr[1]);
                                this.idOperador = _idOperador;
                                this.Designacao = Convert.ToString(dr[2]);
                                this.CompRegiaoInsp = Convert.ToInt32(dr[3]);
                                this.ComprimentoNominal = Convert.ToDouble(dr[4]);
                                this.ToleranciaComprimento = Convert.ToDouble(dr[5]);
                                this.DesvioNominal = Convert.ToDouble(dr[6]);
                                this.ToleranciaDesvioSuperior = Convert.ToDouble(dr[7]);
                                this.ToleranciaDesvioInferior = Convert.ToDouble(dr[8]);

                                this.fileNameImagemFaca = Convert.ToString(dr[9]);

                                this.SpOK = Convert.ToInt32(dr[10]);
                                this.SpNOK = Convert.ToInt32(dr[11]);

                                this.NumSerie = _numSerie;

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
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT NOME FROM UTILIZADORES WHERE ID = @ID", sqlConn))
                    {
                        sqlCmd.Parameters.Add("@ID", SqlDbType.TinyInt).Value = _idOperador;

                        sqlConn.Open();

                        this.NomeOperador = Convert.ToString(sqlCmd.ExecuteScalar());

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

        public void AlteraNumSerie(string _value)
        {
            if (this.NumSerie != _value)
            {
                this.NumSerie = _value;

                using (FicheiroINI ini = new FicheiroINI(Forms.MainForm.VariaveisAuxiliares.iniPath))
                    ini.EscreveFicheiroINI("Receita", "NumSerie", this.NumSerie);
            }
        }

        public Image CarregaImagemFaca()
        {
            if (this.ReceitaCarregada)
            {
                try
                {
                    if (!File.Exists(this.fileNameImagemFaca))
                        throw new Exception("Ficheiro '" + this.fileNameImagemFaca + "' não encontrado!");
                    else
                        return Image.FromFile(this.fileNameImagemFaca);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("CarregaImagemReceita(): " + ex.Message);
                    return Resources.imagem_nao_disponivel;
                }
            }
            else
                return null;
        }

        private void GravaDefinicoesFicheiroINI()
        {
            using (FicheiroINI ini = new FicheiroINI(Forms.MainForm.VariaveisAuxiliares.iniPath))
            {
                ini.EscreveFicheiroINI("Receita", "ReceitaCarregada", this.ReceitaCarregada ? "1" : "0");
                ini.EscreveFicheiroINI("Receita", "idReceita", Convert.ToString(this.ID));
                ini.EscreveFicheiroINI("Receita", "idOperador", Convert.ToString(this.idOperador));
                ini.EscreveFicheiroINI("Receita", "NumSerie", this.NumSerie);
                ini.EscreveFicheiroINI("Receita", "DataHoraAlteracao", Convert.ToString(Diversos.ConvertDatetimeParaUnix(this.DataHoraAlteracao)));
                ini.EscreveFicheiroINI("Receita", "NomeOperador", this.NomeOperador);
            }
        }

        private void CarregaDefinicoesFicheiroINI()
        {
            using (FicheiroINI ini = new FicheiroINI(Forms.MainForm.VariaveisAuxiliares.iniPath))
            {
                this.ReceitaCarregada = ini.RetornaTrueFalseDeStringFicheiroINI("Receita", "ReceitaCarregada", false);
                this.ID = Convert.ToInt32(ini.RetornaINI("Receita", "idReceita", Convert.ToString(this.ID)));
                this.idOperador = Convert.ToInt32(ini.RetornaINI("Receita", "idOperador", Convert.ToString(this.idOperador)));
                this.NumSerie = ini.RetornaINI("Receita", "NumSerie", string.Empty);
                this.DataHoraAlteracao = Diversos.ConvertUnixParaDatetime(Convert.ToInt32(ini.RetornaINI("Receita", "DataHoraAlteracao", "0")));
                this.NomeOperador = ini.RetornaINI("Receita", "NomeOperador", this.NomeOperador);
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
            this.CompRegiaoInsp = 0;
            this.ComprimentoNominal = this.ToleranciaComprimento = this.DesvioNominal = this.ToleranciaDesvioSuperior = this.ToleranciaDesvioInferior = 0;
            this.Referencia = string.Empty;
            this.Designacao = string.Empty;
            this.NumSerie = string.Empty;
            this.NomeOperador = string.Empty;

            this.DataHoraAlteracao = DateTime.MinValue;

            this.idOperador = 0;
            this.ID = 0;

            this.fileNameImagemFaca = string.Empty;

            this.ReceitaCarregada = false;

            if (gravaINI)
                this.GravaDefinicoesFicheiroINI();
        }

        public bool EvaluateComprimento(double value)
        {
            return !Forms.MainForm.Receita.ReceitaCarregada || Diversos.InRange(value, Forms.MainForm.Receita.ComprimentoNominal - Forms.MainForm.Receita.ToleranciaComprimento, Forms.MainForm.Receita.ComprimentoNominal + Forms.MainForm.Receita.ToleranciaComprimento);
        }

        public bool EvaluateDesvio(double value, double tolerancia, double desvioNominal)
        {
            return !Forms.MainForm.Receita.ReceitaCarregada 
                || Diversos.InRange(value, desvioNominal - tolerancia, desvioNominal + tolerancia);

            // || Diversos.InRange(value, Forms.MainForm.Receita.DesvioNominal - Forms.MainForm.Receita.ToleranciaDesvio, Forms.MainForm.Receita.DesvioNominal + Forms.MainForm.Receita.ToleranciaDesvio);
        }

        public bool DownloadReceitaRobot()
        {
            string message = "27";
            int PORT_NO = 5000;


            using (TcpClient client = new TcpClient(this.robotAddress, PORT_NO))
                try
                {
                    NetworkStream nwStream = client.GetStream();
                    byte[] bytesToSend = Encoding.ASCII.GetBytes(message);

                    //---send the text---
                    Console.WriteLine("Sending : " + message);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);

                    //---read back the text---
                    byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                    Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DownloadReceitaRobot(): " + ex.Message);
                    return false;
                }
                finally
                {
                    client.Close();
                }

        }
    }
}
