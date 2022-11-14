using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using System.Collections;
using System.IO;
using PLC;
using static PLC.Siemens;

namespace _22079AI
{
    public class VarsAuxiliares
    {
        /// <summary>
        /// diretório dos ficheiros de configuracoes
        /// </summary>
        private const string _configPath = @"C:\STREAK\Configs";

        /// <summary>
        /// Caminho para o ficheiro INI
        /// </summary>
        public string iniPath = Application.StartupPath + @"\Configs.ini";


        /// <summary>
        /// Directorio do ficheiro de configuração da inspecao
        /// </summary>
        public string InspectionConfigPath = Application.StartupPath + @"\InspectionConfig.ini";


        /// <summary>
        /// Tempo, em formato unix, do arranque da aplicação
        /// </summary>
        public int horaDeArranque = 0;



        /// <summary>
        /// Reiniciar a comunicação com o PLC automáticamente
        /// </summary>
        public bool RestartPLCCommunication { get; set; } = false;

        public int PlcCycleTime { get; } = 200;


        /// <summary>
        /// Retorna o nome da máquina carregado
        /// </summary>
        public string MachineName { get; set; } = "Amorim: Inspeção Lateral Discos";

        public int SelectedTabIndex = 1;

        /// <summary>
        /// Número Máximo de Registos de Alarmes Guardados em base de dados
        /// </summary>
        public int NumeroMaximoFifoAlarmes { get; set; } = 1000;

        public string RobotAddress { get; private set; } = "192.168.10.100";

        #region PLC
        public string plcAddress = "";
        public string plcSlot = "";
        #endregion

        #region Database
        /// <summary>
        /// String de conexão à base de dados
        /// </summary>
        public string DatabaseConnectionString { get; } = string.Empty;

        /// <summary>
        /// Flag that indicate the stat of database connection
        /// </summary>
        public bool DatabaseConnectionState { get; private set; } = false;

        public string IpBaseDados { get; } = string.Empty;

        #endregion

        /// <summary>
        /// Último ID insertido em base de dados
        /// </summary>
        public uint LastInsertedID { get; private set; } = uint.MinValue;

        /// <summary>
        /// Ordem para inserir registos na tabela do ecra de automatico
        /// </summary>
        public bool AuxOrdInsertData { get; set; } = false;

        public VarsAuxiliares()
        {
            horaDeArranque = Diversos.ObterTempoUnixAtual();

            #region Load configurations from INI File
            try
            {
                using (FicheiroINI ini = new FicheiroINI(iniPath))
                {
                    #region Load Database configurations
                    //Forma de conexão local e remota
                    DatabaseConnectionString = @"data source=" + ini.RetornaINI("Database", "Address", "127.0.0.1") + "," + ini.RetornaINI("Database", "Port", "1433") + @"\" + ini.RetornaINI("Database", "Instance", "SQLEXPRESS") + "; Initial Catalog=" + ini.RetornaINI("Database", "DBName", "IVO") + "; MultipleActiveResultSets=True; persist security info=False; UID=" + ini.RetornaINI("Database", "User", "streak") + "; PWD=" + ini.RetornaINI("Database", "Pass", "streak") + ";";
                    IpBaseDados = ini.RetornaINI("Database", "Address", "127.0.0.1");
                    #endregion

                    #region Load PLC configurations
                    //Load PLC configurations
                    plcAddress = ini.RetornaINI("PLC", "plcAddress", "192.168.0.10");
                    plcSlot = ini.RetornaINI("PLC", "plcSlot", "0");
                    RestartPLCCommunication = ini.RetornaTrueFalseDeStringFicheiroINI("PLC", "restartPLCComm", RestartPLCCommunication);
                    PlcCycleTime = Convert.ToInt32(ini.RetornaINI("PLC", "plcCycleTime", Convert.ToString(PlcCycleTime)));
                    #endregion

                    //robot
                    this.RobotAddress = ini.RetornaINI("Robot", "RobotAddress", this.RobotAddress);

                    #region Load General Configurations
                    this.MachineName = ini.RetornaINI("Configs", "machineName", this.MachineName);
                    this.NumeroMaximoFifoAlarmes = Convert.ToInt32(ini.RetornaINI("Configs", "NumeroMaximoFifoAlarmes", Convert.ToString(this.NumeroMaximoFifoAlarmes)));
                    #endregion
                }
            }
            catch (Exception ex)
            {
                new CaixaMensagem(ex.Message, "Error Loading Configs", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                Environment.Exit(0);
            }
            #endregion

            //Start a thread that checks database availability
            new Thread(ThreadCheckDbConnection).Start();

            //Faz o fifo de alarmes
            new Thread(ThreadFifoAlarmes).Start();
        }

        private void ThreadFifoAlarmes()
        {
            //Clock para as interações
            Stopwatch threadClock = Stopwatch.StartNew();
            bool error = false;

            while (VARIAVEIS.FLAG_WHILE_CYCLE)
            {
                if (threadClock.ElapsedMilliseconds >= 10000) //Executar o fifo de alarmes a cada 10sec
                    try
                    {
                        //Reinicar a contagem do clock para a thread
                        threadClock.Restart();

                        if (!DatabaseConnectionState)
                                                                                                                                                             
                            throw new Exception("Sem comunicação com a base de dados");

                        int numOfRows = -1;

                        using (SqlConnection sqlConn = new SqlConnection(DatabaseConnectionString))
                        using (SqlCommand sqlCmd = new SqlCommand("WITH CTE AS (SELECT TOP ((SELECT COUNT (ID) FROM HISTORICO_ALARMES) - @SP) * FROM HISTORICO_ALARMES ORDER BY ID ASC) DELETE FROM CTE", sqlConn))
                        {
                            sqlCmd.Parameters.Add("@SP", SqlDbType.Int).Value = NumeroMaximoFifoAlarmes;

                            sqlConn.Open();

                            numOfRows = sqlCmd.ExecuteNonQuery();
                        }

                        if (numOfRows >= 0)
                            ;// Debug.WriteLine("ThreadFifoAlarmes(): Fifo de alarmes efetuado. Total de resultados afetados: " + numOfRows);
                        else
                            throw new Exception("Erro ao executar query. numOfRows: " + numOfRows);

                        error = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ThreadFifoAlarmes(): " + ex.Message);
                        error = true;
                    }
                //Espera aprox. 500ms entre interações
                Thread.Sleep(500);
            }
        }

        private void ThreadCheckDbConnection()
        {
            bool firstCycle = true;
            while (VARIAVEIS.FLAG_WHILE_CYCLE)
            {
                DatabaseConnectionState = CheckDbConnection();

                if (DatabaseConnectionState)
                    firstCycle = false;

                for (int i = 0; i < 15; i++)
                    if (VARIAVEIS.FLAG_WHILE_CYCLE)
                        Thread.Sleep(200);
                    else
                        break;
            }
        }

        private bool CheckDbConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in DB connection test on CheckDBConnection: " + ex.Message);
                return false; // any error is considered as db connection error for now
            }
        }

        public void Dispose()
        {


        }

        public bool InsereRegistoBaseDados(Inspection data)
        {
            bool returnValue = false;
            try
            {
                if (data == null)
                    throw new Exception("Estrutura de dados vazia!");

                if (data.UNIX_TIME <= 0)
                    throw new Exception("UNIX TIME inválido!");

                if (data.ID == this.LastInsertedID)
                    throw new Exception("DADOS REPETIDOS!!");
                else
                    this.LastInsertedID = data.ID;

                string strQuery = "INSERT INTO PRODUCAO (IdPLC, UnixPLC, IdUser, IdReceita, Comprimento, AmplitudeDesvioSuperior, PosMaxDesvioSuperior, AmplitudeDesvioInferior, PosMaxDesvioInferior, Resultado, TempoCaptura, TempoInspecao, ManualAuto) VALUES (@IdPLC, @UnixPLC, @IdUser, @IdReceita, @Comprimento, @AmplitudeDesvioSuperior, @PosMaxDesvioSuperior, @AmplitudeDesvioInferior, @PosMaxDesvioInferior, @Resultado, @TempoCaptura, @TempoInspecao, @ManualAuto)";

                using (SqlConnection sqlConn = new SqlConnection(this.DatabaseConnectionString))
                using (SqlCommand sqlCmd = new SqlCommand(strQuery, sqlConn))
                {
                    sqlCmd.Parameters.Add("@IdPLC", SqlDbType.BigInt).Value = data.ID;
                    sqlCmd.Parameters.Add("@UnixPLC", SqlDbType.Int).Value = data.UNIX_TIME;
                    sqlCmd.Parameters.Add("@IdUser", SqlDbType.TinyInt).Value = Forms.MainForm.UserSession.IDOperador;
                    sqlCmd.Parameters.Add("@IdReceita", SqlDbType.SmallInt).Value = Forms.MainForm.Receita.ID;
                    sqlCmd.Parameters.Add("@Comprimento", SqlDbType.Real).Value = data.COMPRIMENTO;
                    sqlCmd.Parameters.Add("@AmplitudeDesvioSuperior", SqlDbType.Real).Value = data.AMPLITUDE_DESVIO_SUPERIOR;
                    sqlCmd.Parameters.Add("@PosMaxDesvioSuperior", SqlDbType.Real).Value = data.POS_MAX_DESVIO_SUPERIOR;
                    sqlCmd.Parameters.Add("@AmplitudeDesvioInferior", SqlDbType.Real).Value = data.AMPLITUDE_DESVIO_INFERIOR;
                    sqlCmd.Parameters.Add("@PosMaxDesvioInferior", SqlDbType.Real).Value = data.POS_MAX_DESVIO_INFERIOR;
                    sqlCmd.Parameters.Add("@Resultado", SqlDbType.TinyInt).Value = data.INSPECTION_RESULT;
                    sqlCmd.Parameters.Add("@TempoCaptura", SqlDbType.Int).Value = data.CAPTURE_TIME;
                    sqlCmd.Parameters.Add("@TempoInspecao", SqlDbType.Int).Value = data.INSPECTION_TIME;
                    sqlCmd.Parameters.Add("@ManualAuto", SqlDbType.Bit).Value = DB400.MANUAL_AUTO;

                    sqlConn.Open();

                    returnValue = sqlCmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InsereRegistoBaseDados(): " + ex.Message);
                return false;
            }


            if (returnValue) ;
                //Forms.MainForm.PLC1.EnviaTag(MemoryArea.M, TipoVariavel.Bool, true, 0, 90, 0);

            return returnValue;
        }
    }
}
