using PLC;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _22079AI
{
    public class Sessao
    {
        private byte idDB = 0;
        private string nomeOperador = "";
        private byte nivelSessao = 0;
        private string[] nivelSessaoStr = new string[6];

        private bool sessaoIniciada = false;
        private int dataHoraInicioSessao = 0;

        public byte IDOperador
        {
            get
            {
                return (sessaoIniciada) ? this.idDB : Convert.ToByte(0);
            }
        }

        public string NomeOperador
        {
            get
            {
                return (sessaoIniciada) ? this.nomeOperador : "Sem Sessão";
            }

        }

        public bool SessaoIniciada
        {
            get { return this.sessaoIniciada; }
        }

        public string DataHoraInicioSessao
        {
            get { return sessaoIniciada ? Convert.ToString(Diversos.ConvertUnixParaDatetime(dataHoraInicioSessao)) : "00/00/0000 00:00:00"; }
        }

        public string TempoDecorrido
        {
            get { return sessaoIniciada ? Convert.ToString(Diversos.CalculaTempo(Diversos.ObterTempoUnixAtual() - dataHoraInicioSessao)) : "00:00:00"; }
        }

        public SessaoOperador NivelSessao
        {
            get
            {
                if (!this.sessaoIniciada)
                    return SessaoOperador.SemSessao;
                else
                    return (SessaoOperador)this.nivelSessao;
            }
        }

        public string StrNivelSessao
        {
            get { return nivelSessaoStr[nivelSessao]; }
        }

        public enum SessaoOperador
        {
            SemSessao = 0,
            Operador1 = 1,
            Operador2 = 2,
            Manutencao = 3,
            Administrador = 4,
            SuperAdministrador = 5
        }

        /// <summary>
        /// Construtor da classe
        /// </summary>
        /// <param name="strConexao">String de conexão com a base de dados</param>
        public Sessao(string strConexao)
        {
            AtualizaStrNivelSessao(strConexao);
        }

        public bool IniciaSessao(byte _idDb, string _nomeOperador, byte _nivelSessao)
        {
            if (!sessaoIniciada)
            {
                idDB = _idDb;
                nomeOperador = _nomeOperador;
                nivelSessao = _nivelSessao;
                dataHoraInicioSessao = Diversos.ObterTempoUnixAtual();
                sessaoIniciada = true;
                this.AtualizaSessaoPLC();

                return true;
            }
            else
            {
                new CaixaMensagem("Já existe uma sessão ativa!", "Sessão", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                return false;
            }
        }

        public bool TerminaSessao()
        {
            if (sessaoIniciada)
            {
                idDB = 0;
                nomeOperador = string.Empty;
                nivelSessao = 0;
                dataHoraInicioSessao = Diversos.ObterTempoUnixAtual();
                sessaoIniciada = false;
                this.AtualizaSessaoPLC();

                return true;
            }
            else
            {
                new CaixaMensagem("Não existe sessão iniciada!", "Sessão", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                return false;
            }
        }

        public bool TemPermissao(SessaoOperador _nivelRequerido)
        {
            if (_nivelRequerido == SessaoOperador.SemSessao)
                return true;
            else
                return (nivelSessao >= (int)_nivelRequerido);
        }

        private bool AtualizaStrNivelSessao(string _strConexao)
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(_strConexao))
                using (SqlCommand sqlCmd = new SqlCommand("SELECT TOP 6 NIVEL FROM NIVEL_UTILIZADOR ORDER BY ID", sqlConn))
                {
                    int i = 0;
                    sqlConn.Open();

                    using (SqlDataReader dr = sqlCmd.ExecuteReader())
                        while (dr.Read() && i < 6)
                        {
                            nivelSessaoStr[i] = Convert.ToString(dr[0]);
                            i++;
                        }

                    return true;
                }
            }
            catch (Exception ex)
            {
                new CaixaMensagem(ex.Message, "Erro Sessão", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                return false;
            }
        }

        private void AtualizaSessaoPLC()
        {
            if (Forms.MainForm.PLC1 != null && VARIAVEIS.ESTADO_CONEXAO_PLC)
                Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Byte, nivelSessao, 20, 22);

        }

    }
}
