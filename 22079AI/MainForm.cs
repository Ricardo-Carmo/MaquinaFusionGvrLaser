using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Reflection;
using System.Drawing.Imaging;
using System.IO;
using _22079AI.Properties;
using System.Security.Permissions;
using PLC;
using static PLC.Siemens;

namespace _22079AI
{
    public partial class MainForm : Form
    {
        public VarsAuxiliares VariaveisAuxiliares = new VarsAuxiliares();
        public DataGridView datagridviewHistoricAlarms;
        public Receitas Receita;
        public LogFile ErrorLogFile = new LogFile(Application.StartupPath + @"\ErrorLog.txt", 1024);
        //public Siemens PLC1;
        public InspLateralDiscos Inspection;
        public PlcSendRcv PlcControl= new PlcSendRcv();
        public HandleAlarms AlarmsHandling;
        public Sessao UserSession;
        public static object myLock = new object();

        public bool[] FP = new bool[16];
        public bool fpManualAuto = false;


        //Variáveis internas
        private bool RESET_ALARMES_ATIVOS = false;

        /// <summary>
        /// 0 - not defined, 1 - in read, 2 - in write, 3 - in sleep
        /// </summary>
        private int State = 0, ReadTime = 0, WriteTime = 0, SleepTime = 0;

        private bool errRead = false, errWrite = false;

        public MainForm()
        {
            InitializeComponent();

            //Esconder o form
            this.Opacity = 0;
            this.ShowInTaskbar = false;

            this.Size = this.MaximumSize;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                //Iniciar comunicação com o PLC
                //this.PLC1 = new Siemens(this.VariaveisAuxiliares.iniPath);

                #region Tricks
                var version = Assembly.GetEntryAssembly().GetName().Version;
                var buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(TimeSpan.TicksPerDay * version.Build + TimeSpan.TicksPerSecond * 2 * version.Revision));
                lblHeader.Text = VariaveisAuxiliares.MachineName;
                this.Text = lblHeader.Text;
                tabControlEx1.SelectedIndex = 1;

                datagridviewHistoricAlarms = dgvHistoricAlarms;

                //cores rotate labels
                rotatedLabel1.Background.BackColor1 = rotatedLabel6.Background.BackColor1 = Color.GreenYellow;
                rotatedLabel1.Background.BackColor2 = rotatedLabel6.Background.BackColor2 = Color.DarkOliveGreen;

                //rotatedLabel2.Background.BackColor1 = Color.Orange;
                //rotatedLabel2.Background.BackColor2 = Color.OrangeRed;

                #endregion

                //Instância alarmes
                this.AlarmsHandling = new HandleAlarms(this.VariaveisAuxiliares.DatabaseConnectionString);

                if (!this.AlarmsHandling.ClassOK)
                    Environment.Exit(0); //Encerra a aplicação caso não consiga descarregar todos os alarmes corretamente (por ex. sem comunicação com base de dados)     

                //Instância sessão de utilizador
                this.UserSession = new Sessao(this.VariaveisAuxiliares.DatabaseConnectionString);

                //Arranca com a thread do ciclo do PLC
                //new Thread(CicloPLC) { Priority = ThreadPriority.Highest }.Start();
                new Thread(PlcControl.WriteReadPlc) { Priority = ThreadPriority.Highest }.Start();

                //Instância Receitas
                this.Receita = new Receitas(this.VariaveisAuxiliares.DatabaseConnectionString, this.VariaveisAuxiliares.RobotAddress);

                //Instância camera
                this.Inspection = new InspLateralDiscos(this.VariaveisAuxiliares.InspectionConfigPath);

                this.Inspection.SetZoomControl(pctImgLive);

                //Atualiza Dados Receita
                this.AtualizaInformacoesReceita();

                //TODO descomentar para encra de manual
                DB400.MANUAL_AUTO = true;
            }
            catch (Exception ex)
            {
                new CaixaMensagem(ex.Message, "Fatal Error", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                Environment.Exit(0);
            }
            finally
            {
                VARIAVEIS.FORM_LOADED = true;
                Debug.WriteLine("Finally Loaded!");
                this.Opacity = 100;
                this.ShowInTaskbar = true;
                this.BringToFront();

                if (Debugger.IsAttached) //inicia sessao se tiver com o compilador ligaod
                    this.UserSession.IniciaSessao(1, "Administrador STREAK", 5);
            }
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (new CaixaMensagem("Deseja realmente terminar a aplicação?", "Fechar Aplicação", CaixaMensagem.TipoMsgBox.Question).ShowDialog() == DialogResult.Yes)
            {
                this.PlcControl.Dispose();

                this.Inspection.Dispose();

                this.VariaveisAuxiliares.Dispose();

                //Close all threads and dispose objects
                VARIAVEIS.FLAG_WHILE_CYCLE = false;
            }
            else
                e.Cancel = true;
        }

        private int GetNumOFTimesCycle(int miliseconds)
        {
            return this.VariaveisAuxiliares.PlcCycleTime >= miliseconds ? 1 : Convert.ToInt32(miliseconds / this.VariaveisAuxiliares.PlcCycleTime);
        }

        //private void UpdatePLCTime()
        //{
        //    try
        //    {
        //        if (this.PLC1 == null)
        //            throw new Exception("PLC == NULL");

        //        this.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.DTL, DateTime.Now.ToUniversalTime(), 90, 0);
        //        this.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, true, 90, 12, 0);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("UpdatePLCTime(): " + ex.Message);
        //    }

        //}

        //private void CicloPLC()
        //{
        //    //Clock para as interações
        //    Stopwatch threadClock = Stopwatch.StartNew();

        //    int e = 0;
        //    bool fpError = false;
        //    bool firstCycle = true;
        //    bool fpAckAlarms = false;
        //    byte lastHourUpdate = byte.MaxValue;

        //    DateTime[] simClocks = new DateTime[4];
        //    for (int i = 0; i < simClocks.Length; i++)
        //        simClocks[i] = DateTime.Now;


        //    #region Multiplicadores de tempo de ciclo
        //    //Multiplicadores de tempo de ciclo
        //    CycleMultiplier _50msCycleTimer = new CycleMultiplier(this.GetNumOFTimesCycle(50));
        //    CycleMultiplier _100msCycleTimer = new CycleMultiplier(this.GetNumOFTimesCycle(100));
        //    CycleMultiplier _250msCycleTimer = new CycleMultiplier(this.GetNumOFTimesCycle(250));
        //    CycleMultiplier _500msCycleTimer = new CycleMultiplier(this.GetNumOFTimesCycle(500));
        //    CycleMultiplier _1000msCycleTimer = new CycleMultiplier(this.GetNumOFTimesCycle(1000));
        //    #endregion

        //    #region DB400_HMI

        //    List<Siemens.ReadMultiVariables> DB400_HMI = new List<Siemens.ReadMultiVariables>();

        //    //alarms
        //    for (int i = 0; i < VARIAVEIS.Alarmes.Length / 8; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Bool));

        //    //inputs
        //    for (int i = 0; i < 10; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Bool));

        //    //outputs
        //    for (int i = 0; i < 10; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Bool));

        //    //bits status
        //    for (int i = 0; i < 5; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Bool));

        //    DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte));


        //    //INSPECTION RESULT struct
        //    DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.UDInt));
        //    DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt));

        //    for (int i = 0; i < 5; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Real));

        //    for (int i = 0; i < 2; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.UInt));

        //    DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte));

        //    DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Bool));

        //    //COUNTERS
        //    DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.UInt));

        //    for (int i = 0; i < 6; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt));

        //    //motor passadeira
        //    for (int i = 0; i < 2; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Bool));

        //    DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.UDInt));


        //    //cilindros 
        //    for (int i = 0; i < 3; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte));

        //    for (int i = 0; i < 3; i++) //bits stats robot
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.Bool));

        //    //CONTADORES
        //    for (int i = 0; i < 6; i++)
        //        DB400_HMI.Add(new Siemens.ReadMultiVariables(Siemens.TipoVariavel.UInt));
        //    #endregion

        //    while (VARIAVEIS.FLAG_WHILE_CYCLE)
        //    {
        //        if (threadClock.ElapsedMilliseconds >= VariaveisAuxiliares.PlcCycleTime)
        //            try
        //            {
        //                //Reinicar a contagem do clock para a thread
        //                threadClock.Restart();

        //                #region Multiplicadores de Ciclo
        //                //Obter o número da página onde estamos
        //                int tabControlNumber = this.VariaveisAuxiliares.SelectedTabIndex;

        //                //Atualiza os multiplicadores de ciclos
        //                _50msCycleTimer.UpdateCycleCount(true);
        //                _100msCycleTimer.UpdateCycleCount(true);
        //                _250msCycleTimer.UpdateCycleCount(true);
        //                _500msCycleTimer.UpdateCycleCount(true);
        //                _1000msCycleTimer.UpdateCycleCount(true);
        //                #endregion

        //                //Iniciar a contagem do tempo de ciclo
        //                this.PLC1.IniciaContagemTempoCiclo();
        //                this.State = 1;

        //                #region Processa as Entradas
        //                //**************************** PROCESSA AS ENTRADAS ****************************
        //                try
        //                {
        //                    if (this.PLC1.LeSequenciaTags(Siemens.MemoryArea.DB, DB400_HMI.ToArray(), 400, 0))
        //                    {   //descodifica as leituras
        //                        int indexAuxI = 0; // auxiliar do ponteiro de leitura

        //                        #region Alarmes
        //                        for (int i = 0; i < VARIAVEIS.Alarmes.Length / 8; i++)
        //                        {
        //                            for (int k = 0; k < 8; k++)
        //                                VARIAVEIS.Alarmes[k + (8 * i)] = (Convert.ToBoolean(DB400_HMI[i].ObtemVariavel(k)));

        //                            indexAuxI++;
        //                        }
        //                        #endregion

        //                        #region Outputs

        //                        //PLC
        //                        OUTPUTS.CMD_DESINDEXA_FACA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        OUTPUTS.CMD_INDEXA_FACA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        OUTPUTS.RESERVA_VALVULA_11 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        OUTPUTS.RESERVA_VALVULA_12 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        OUTPUTS.CMD_PORTA_DIREITA_ABRE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        OUTPUTS.CMD_PORTA_DIREITA_FECHA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        OUTPUTS.CMD_PORTA_ESQUERDA_ABRE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        OUTPUTS.CMD_PORTA_ESQUERDA_FECHA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        OUTPUTS.CMD_RES_VALV_5 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        OUTPUTS.CMD_RES_VALV_5_ERROR = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        OUTPUTS.CMD_RES_VALV_11_2 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        OUTPUTS.CMD_RES_VALV_11_3 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        OUTPUTS.CMD_RES_VALV_11_4 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        OUTPUTS.CMD_RES_VALV_11_5 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        OUTPUTS.CMD_RES_VALV_11_6 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        OUTPUTS.CMD_RES_VALV_11_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        //1º CARTA 16 DO's
        //                        OUTPUTS.CMD_RES_VALV_12_0 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        OUTPUTS.CMD_RES_VALV_12_1 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        OUTPUTS.CMD_TOWER_GREEN = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        OUTPUTS.CMD_TOWER_RED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        OUTPUTS.CMD_TOWER_YELLOW = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        OUTPUTS.CMD_TOWER_BUZZER = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        OUTPUTS.CMD_LED_START = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        OUTPUTS.CMD_LED_RED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        OUTPUTS.CMD_LED_RESET = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        OUTPUTS.CMD_LED_PC_ON = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        OUTPUTS.CMD_FECHO_CAMERA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        OUTPUTS.CMD_FECHO_ILUMINACAO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        OUTPUTS.CMD_FECHO_MONITOR = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        OUTPUTS.CMD_FECHO_JAULA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        OUTPUTS.CMD_RUN_VFR = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        OUTPUTS.CMD_RESET_VFR = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        //2º CARTA 8 DO's
        //                        OUTPUTS.CMD_VFR_ON = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        OUTPUTS.OUT_RESERVA_14_1 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        OUTPUTS.ORD_GRAB_CONVOYER = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        OUTPUTS.ORD_TAKE_INSPECTION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        OUTPUTS.ORD_APROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        OUTPUTS.ORD_REPROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        OUTPUTS.ORD_INI_POSITION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        OUTPUTS.OUT_RESERVA_14_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        //RESERVED
        //                        indexAuxI++;

        //                        //RESERVED
        //                        indexAuxI++;

        //                        //RESERVED
        //                        indexAuxI++;

        //                        //SAFETY
        //                        OUTPUTS.CMD_CORTE_AR = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        OUTPUTS.EMG_ROBOT = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        OUTPUTS.PORTAS_ROBOT = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        OUTPUTS.CMD_RESERVA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        indexAuxI++;

        //                        //RESERVA BYTE
        //                        indexAuxI++;
        //                        #endregion

        //                        #region Inputs

        //                        //PLC
        //                        INPUTS.BTN_START = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        INPUTS.BTN_STOP = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        INPUTS.BTN_RESET = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        INPUTS.BTN_PC_ON = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        INPUTS.SEN_PORTA_DIREITA_FECHADA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        INPUTS.SEN_PORTA_DIREITA_ABERTA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        INPUTS.SEN_PORTA_ESQUERDA_FECHADA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        INPUTS.SEN_PORTA_ESQUERDA_ABERTA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        INPUTS.SEN_KNIFE_CONVYER_DETECTED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        INPUTS.SEN_INDEX_FACA_BAIXO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        INPUTS.SEN_ROBOT_GARRA_ABERTA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        INPUTS.SEN_CILINDRO_CONVOYER_INDEX = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        INPUTS.SEN_PRESENCA_CAIXA_APROVADOS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        INPUTS.SEN_PRESENCA_CAIXA_REPROVADOS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        INPUTS.RESERVA_1_6 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        INPUTS.RESERVA_1_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        //1º CARTA 16 DI's
        //                        INPUTS.STU_THP_VFR = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        INPUTS.STU_VFR_FAULT = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        INPUTS.STU_VFR_RUNNING = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        INPUTS.RESERVA_2_3 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        INPUTS.RESERVA_2_4 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        INPUTS.RESERVA_2_5 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        INPUTS.RESERVA_2_6 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        INPUTS.RESERVA_2_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        INPUTS.STA_GRAB_CONVOYER = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        INPUTS.STA_TAKE_INSPECTION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        INPUTS.STA_APROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        INPUTS.STA_REPROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        INPUTS.STA_INI_POSITION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        INPUTS.RESERVA_3_5 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        INPUTS.RESERVA_3_6 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        INPUTS.RESERVA_3_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        //2º CARTA 8 DI's
        //                        INPUTS.RESERVA_4_0 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        INPUTS.RESERVA_4_1 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        INPUTS.RESERVA_4_2 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        INPUTS.RESERVA_4_3 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        INPUTS.RESERVA_4_4 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        INPUTS.RESERVA_4_5 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        INPUTS.RESERVA_4_6 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        INPUTS.RESERVA_4_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        //RESERVED
        //                        indexAuxI++;

        //                        //RESERVED
        //                        indexAuxI++;

        //                        //RESERVED
        //                        indexAuxI++;

        //                        //safety

        //                        INPUTS.EMERGENCIA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        INPUTS.FECHO_CAMERA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        INPUTS.FECHO_ILUMINACAO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        INPUTS.FECHO_ROBOT = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        INPUTS.FECHO_ECRA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        INPUTS.BARREIRA_APROVADAS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        INPUTS.BARREIRA_REPROVADAS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        INPUTS.RESERVA_1000_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        //RESERVA BYTE
        //                        indexAuxI++;
        //                        #endregion

        //                        #region Read Tags
        //                        DB400.INF_ALL_INIT_POS_OK = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        DB400.INF_RECEITA_CARREGADA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        DB400.INF_TRANSPORTER_READY = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        DB400.INF_ROBO_READY = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        DB400.ALL_DOORS_CLOSED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        DB400.READY_TO_AUTO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        DB400.MANUAL_AUTO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        DB400.CYCLE_ON = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        DB400.USER_SESSION_ACTIVE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        DB400.CLOCK_1HZ = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        DB400.CLOCK_2HZ = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        DB400.CLOCK_5HZ = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        DB400.CLOCK_10HZ = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        DB400.BYPASS_SECURITY = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        DB400.VISION_LIGHT_ON = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        DB400.CAM_TRIGGER_MODE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        DB400.CAM_ORD_TRIGGER = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        DB400.FECHO_CAM_CLOSED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        DB400.FECHO_ILUMINACAO_CLOSED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        DB400.FECHO_ROBOT_CLOSED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        DB400.FECHO_ECRA_CLOSED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        DB400.FECHO_BARREIRA_APROV_CLOSED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        DB400.FECHO_BARREIRA_REPROV_CLOSED = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        DB400.CARREGAMENTO_EM_CURSO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        DB400.DESCARGA_OK_EM_CURSO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        DB400.DESCARGA_NOK_EM_CURSO = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        DB400.DESCARGA_OK_FULL = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        DB400.DESCARGA_NOK_FULL = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        DB400.SEQUENCIA_NOK = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        DB400.STOP_BARREIRA_APROVADOS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        DB400.STOP_BARREIRA_REPROVADOS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        DB400.CAIXA_PRESENTE_APROVADOS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        DB400.CAIXA_PRESENTE_REPROVADOS = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        DB400.EMERGENCIA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        DB400.CON_GRAB_CONVOYER = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        DB400.CON_TAKE_INSPECTION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        DB400.CON_APROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        DB400.CON_REPROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        DB400.CON_INI_POSITION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        DB400.TRANSPORTADOR_COM_FACA = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        DB400.RESERVA_BYTE_41 = Convert.ToByte(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;

        //                        indexAuxI += DB400.INSPECTION_RESULT.UpdateTags(DB400_HMI.ToArray(), indexAuxI);

        //                        DB400.RESERVED_INT = Convert.ToUInt16(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;

        //                        DB400.TOTAL_APROVED_BY_BOX = Convert.ToInt32(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.TOTAL_REPROVED_BY_BOX = Convert.ToInt32(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.TOTAL_APROVED = Convert.ToInt32(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.TOTAL_REPROVED = Convert.ToInt32(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.LOTE_TOTAL_NUMBER = Convert.ToInt32(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.SEQUENCE_REPROVED = Convert.ToInt32(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;

        //                        indexAuxI += DB400.MOTOR_PASSADEIRA.UpdateTags(DB400_HMI.ToArray(), indexAuxI);

        //                        // 0 - SEM COMANDOS / 1 - ORDEM / 2 - / 3 - 
        //                        DB400.CILINDRO_INDEXACAO = Convert.ToByte(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.PORTA_DIREITA = Convert.ToByte(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.PORTA_ESQUERDA = Convert.ToByte(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;

        //                        DB400.STA_GRAB_CONVOYER = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        DB400.STA_TAKE_INSPECTION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        DB400.STA_APROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        DB400.STA_REPROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        DB400.STA_INI_POSITION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        DB400.STA_SAFE_POSITION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        DB400.ALM_GRAB_CONVOYER = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        DB400.ALM_TAKE_INSPECTION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        DB400.ALM_APROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(0));
        //                        DB400.ALM_REPROVE_KNIFE = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(1));
        //                        DB400.ALM_INI_POSITION = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(2));
        //                        DB400.RESERVA_104_3 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(3));
        //                        DB400.RESERVA_104_4 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(4));
        //                        DB400.RESERVA_104_5 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(5));
        //                        DB400.RESERVA_104_6 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(6));
        //                        DB400.RESERVA_104_7 = Convert.ToBoolean(DB400_HMI[indexAuxI].ObtemVariavel(7));
        //                        indexAuxI++;

        //                        DB400.PECAS_ULTIMA_HORA = Convert.ToUInt16(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.PECAS_MEDIA_HORA = Convert.ToUInt16(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.TEMPO_DE_CICLO = Convert.ToUInt16(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;

        //                        DB400.COUNTER_KNIFES_ENGRAVED = Convert.ToUInt16(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.SUB_COUNTER_KNIFES = Convert.ToUInt16(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;
        //                        DB400.KNIFES_TO_ENGRAVE = Convert.ToUInt16(DB400_HMI[indexAuxI].ObtemVariavel()); indexAuxI++;

        //                        #endregion

        //                        this.errRead = false;
        //                    }
        //                    else
        //                        //EJ-COMENT
        //                        //throw new Exception("Reading Error!");
        //                        Debug.WriteLine("CicloPLC() - Processa Entradas: ERRO");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Debug.WriteLine("CicloPLC() - Processa Entradas: " + ex.Message);
        //                    this.errRead = true;
        //                }
        //                finally
        //                {
        //                    this.ReadTime = (int)this.PLC1.TempoCicloDecorrido;
        //                    this.State = 2;
        //                }
        //                #endregion

        //                #region Processa as Saídas
        //                //**************************** PROCESSA AS SAÍDAS ******************************
        //                try
        //                {
        //                    this.errWrite = !this.PLC1.ProcessaListagemEscrita();
        //                }
        //                catch (Exception ex)
        //                {
        //                    Debug.WriteLine("CicloPLC() - Processa Saídas: " + ex.Message);
        //                    this.errWrite = true;
        //                }
        //                finally
        //                {
        //                    this.WriteTime = (int)this.PLC1.TempoCicloDecorrido - this.ReadTime;
        //                    this.State = 3;
        //                }
        //                //******************************************************************************
        //                #endregion

        //                //FP do Botão de Reset, para os alarmes ativos
        //                if (INPUTS.BTN_RESET && !fpAckAlarms)
        //                {
        //                    fpAckAlarms = true;
        //                    this.RESET_ALARMES_ATIVOS = true;
        //                }
        //                else if (!INPUTS.BTN_RESET)
        //                    fpAckAlarms = false;

        //                //trigger a camera
        //                if (this.Inspection != null) this.Inspection.EXTERNAL_TRIGGER = DB400.CAM_ORD_TRIGGER;

        //                //a cada 1seg 
        //                if (_1000msCycleTimer.IsReached)
        //                {
        //                    //envia o software OK
        //                    this.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, true, 401, 6, 0);

        //                    if (this.Inspection != null)
        //                        if (this.Inspection.CameraAtiva != DB400.CAM_TRIGGER_MODE)//envia o status da camera para o PLC
        //                            this.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Int, this.Inspection.CameraAtiva ? 2 : 0, this.Inspection.DbNumber, 6, 0);
        //                }

        //                if ((this.PLC1.EstadoLigacao && !VARIAVEIS.ESTADO_CONEXAO_PLC) || (_1000msCycleTimer.IsReached && lastHourUpdate != DateTime.Now.Hour))
        //                {   //Atualizar a hora do PLC quando ganha comunicação com PLC ou a cada hora que passa
        //                    lastHourUpdate = (byte)DateTime.Now.Hour;
        //                    this.UpdatePLCTime();
        //                }

        //                //verifica se é necessário inserir na base de dados 
        //                if (DB400.INSPECTION_RESULT.READ && (DB400.INSPECTION_RESULT.ID != this.VariaveisAuxiliares.LastInsertedID) && !this.VariaveisAuxiliares.AuxOrdInsertData)
        //                {
        //                    this.VariaveisAuxiliares.AuxOrdInsertData = true;

        //                    new Thread(() => this.VariaveisAuxiliares.InsereRegistoBaseDados(DB400.INSPECTION_RESULT)).Start();
        //                }

        //                //Efetua o pedido de restart à comunicação com o PLC
        //                if (this.PLC1.RestartRequested) this.PLC1.DisconnectFromPLC();

        //                fpError = false; //clear flag error

        //            }
        //            catch (Exception ex)
        //            {
        //                #region Error Handling
        //                if (!fpError)
        //                {
        //                    fpError = true;
        //                    e = 0;
        //                    Debug.WriteLine("PLC Cycle Error (i == " + e + "): " + ex.Message);
        //                }
        //                e++;
        //                if (e % 5 == 0)
        //                    Debug.WriteLine("PLC Cycle Error (i == " + e + "): " + ex.Message);
        //                #endregion

        //                this.State = 0;
        //            }
        //            finally
        //            {
        //                //Terminar a contagem do tempo de ciclo
        //                this.PLC1.TerminaContagemTempoCiclo();

        //                if (this.PLC1.TempoCicloAtual >= this.VariaveisAuxiliares.PlcCycleTime)
        //                    this.SleepTime = 0;
        //                else
        //                    this.SleepTime = this.VariaveisAuxiliares.PlcCycleTime - this.PLC1.TempoCicloAtual;

        //                VARIAVEIS.ESTADO_CONEXAO_PLC = this.PLC1.EstadoLigacao;
        //            }

        //        #region Simulação Clocks - se NAO HOUVER PLC COMUNICATION
        //        //simula os clocks se nao houver comunicação com o PLC
        //        if (!VARIAVEIS.ESTADO_CONEXAO_PLC)
        //        {
        //            DateTime dtNow = DateTime.Now;

        //            if ((dtNow - simClocks[0]).TotalMilliseconds >= 1000)
        //            {
        //                DB400.CLOCK_1HZ = !DB400.CLOCK_1HZ;
        //                simClocks[0] = dtNow;
        //            }

        //            if ((dtNow - simClocks[1]).TotalMilliseconds >= 500)
        //            {
        //                DB400.CLOCK_2HZ = !DB400.CLOCK_2HZ;
        //                simClocks[1] = dtNow;
        //            }

        //            if ((dtNow - simClocks[2]).TotalMilliseconds >= 200)
        //            {
        //                DB400.CLOCK_5HZ = !DB400.CLOCK_5HZ;
        //                simClocks[2] = dtNow;
        //            }

        //            if ((dtNow - simClocks[3]).TotalMilliseconds >= 100)
        //            {
        //                DB400.CLOCK_10HZ = !DB400.CLOCK_10HZ;
        //                simClocks[3] = dtNow;
        //            }
        //        }
        //        #endregion

        //        //so processor can rest for a while
        //        Thread.Sleep(5);
        //    }

        //    //Antes de encerrar a thread, desliga a comunicação com o PLC
        //    if (PLC1 != null)
        //        PLC1.DisconnectFromPLC();
        //}

        private void AtualizaAnimacaoAlarmes()
        {
            if (dgvActiveAlarms != null && AlarmsHandling.ClassOK)
                if (dgvActiveAlarms.RowCount > 0)
                {
                    //Cor para cada tipo de alarme
                    Color[] alarmeTypeColor = new Color[4];
                    alarmeTypeColor[0] = Color.LightYellow;
                    alarmeTypeColor[1] = Color.Red;
                    alarmeTypeColor[2] = Color.Orange;
                    alarmeTypeColor[3] = Color.DodgerBlue;

                    //Seleção das cores para os 3 níveis de alarmes alternando as cores
                    Color[] nonAcknowledgedAlarmColor = new Color[4];
                    for (int i = 0; i < nonAcknowledgedAlarmColor.Length; i++)
                        nonAcknowledgedAlarmColor[i] = (i > 0 ? (DB400.CLOCK_1HZ ? alarmeTypeColor[i] : alarmeTypeColor[0]) : alarmeTypeColor[0]);

                    //Faz um varrimento por todos os alarmes
                    for (int i = 0; i < dgvActiveAlarms.RowCount; i++)
                    {
                        //Obter o número do alarme da tabela
                        int alarmNumber = Convert.ToInt32(dgvActiveAlarms[0, i].Value);

                        //Indicação de alarme não reconhecido
                        bool nonAcknowledgedAlarm = !AlarmsHandling.alarms[alarmNumber - 1].AcknowledgedAlarm;

                        //Tipo de alarme de 1 a 3
                        int alarmTypeNumber = AlarmsHandling.alarms[alarmNumber - 1].AlarmType;

                        dgvActiveAlarms.Rows[i].DefaultCellStyle.BackColor = nonAcknowledgedAlarm ? nonAcknowledgedAlarmColor[alarmTypeNumber] : alarmeTypeColor[alarmTypeNumber]; //Caso o alarme ainda não tenha sido reconhecido vamos alternando as cores, senão colocamos a cor do alarme
                    }


                    //Reconhecer todos os alarmes ativos
                    if (RESET_ALARMES_ATIVOS)
                    {
                        AlarmsHandling.AcknowledgeAllActiveAlarms();
                        RESET_ALARMES_ATIVOS = false;
                    }
                }


        }

        public bool VerificaAcesso(Sessao.SessaoOperador sessao, bool mostraMensagem = true)
        {
            if (!UserSession.TemPermissao(sessao))
            {
                if (mostraMensagem)
                    new CaixaMensagem("Conta sem permissões de acesso.", "Sem Permissões", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();
                return false;
            }
            else
                return true;
        }

        private void ListaTodosOperadores()
        {
            if (VariaveisAuxiliares.DatabaseConnectionState)
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(VariaveisAuxiliares.DatabaseConnectionString))
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT UTILIZADORES.ID, UTILIZADORES.NOME AS 'Nome', REPLICATE('*',LEN(UTILIZADORES.PASSWORD)) AS 'Password', NIVEL_UTILIZADOR.NIVEL AS 'Nível' FROM UTILIZADORES INNER JOIN NIVEL_UTILIZADOR ON UTILIZADORES.ID_NIVEL = NIVEL_UTILIZADOR.ID WHERE UTILIZADORES.ID > 0 AND UTILIZADORES.ID_NIVEL < 5 ORDER BY UTILIZADORES.ID", sqlConn))
                    {
                        sqlConn.Open();

                        using (SqlDataAdapter da = new SqlDataAdapter(sqlCmd))
                        using (DataSet ds = new DataSet())
                        {
                            da.Fill(ds);

                            if (dgvOperadores.InvokeRequired)
                                dgvOperadores.Invoke(new MethodInvoker(() =>
                                {
                                    dgvOperadores.DataSource = ds.Tables[0];
                                }));
                            else
                                dgvOperadores.DataSource = ds.Tables[0];

                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ListaTodosOperadores()" + ex.Message);
                    new CaixaMensagem(ex.Message, "Erro Listar Operadores", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                }
        }

        public void InsertIntoDGV(Inspection insp)
        {
            int spMaxdgv = 12;

            try
            {
                if (insp == null)
                    throw new Exception("INSPECTION OBJECT IS NULL!");

                dgvUltimosAneisProcessados.Rows.Insert(0, new string[] {
                    insp.ID.ToString(),
                    Diversos.NumberToString(insp.COMPRIMENTO,2,true) + " mm",
                    Diversos.NumberToString(insp.AMPLITUDE_DESVIO_SUPERIOR,2,true) + " mm",
                    Diversos.NumberToString(insp.POS_MAX_DESVIO_SUPERIOR,2,true) + " mm",
                    Diversos.NumberToString(insp.AMPLITUDE_DESVIO_INFERIOR,2,true) + " mm",
                    Diversos.NumberToString(insp.POS_MAX_DESVIO_INFERIOR,2,true) + " mm",
                    insp.APROVED ? "OK" : insp.INSPECTION_RESULT == 3 ? "NOT OK" : "ERROR",
                    //insp.INSPECTION_TIME.ToString() + " ms",
                    insp.DT_INSPECTION.ToString("dd/MM HH:mm:ss") }); ;


                if (!insp.APROVED)
                {
                    for (int i = 0; i < dgvUltimosAneisProcessados.Columns.Count; i++)
                        dgvUltimosAneisProcessados.Rows[0].Cells[i].Style.BackColor = insp.INSPECTION_RESULT == 3 ? pnlInspecaoNOK.BackColor : pnlErroInspecao.BackColor;
                }

                //fazer fifo das imagens 
                for (int i = 7; i >= 1; i--)
                {
                    Panel panelAtual = (Panel)this.GetControlByName("pnlInspAuto" + i.ToString());
                    Panel panelAnterior = (Panel)this.GetControlByName("pnlInspAuto" + (i - 1).ToString());

                    if (panelAtual != null && panelAnterior != null)
                    {
                        if (i == 7)
                        {
                            if (panelAtual.Controls.Count == 1)
                            {
                                SingleInspectionItem item = panelAtual.Controls[0] as SingleInspectionItem;
                                item.Dispose(); item = null;
                            }
                        }

                        //limpa o controlo do painel atual
                        panelAtual.Controls.Clear();

                        //faz o shift register
                        if (panelAnterior.Controls.Count == 1)
                        {
                            SingleInspectionItem item = panelAnterior.Controls[0] as SingleInspectionItem;
                            panelAnterior.Controls.Clear();
                            panelAtual.Controls.Add(item);
                        }
                    }
                }

                //instanciar novo objeto
                SingleInspectionItem smallInspItem = new SingleInspectionItem();
                smallInspItem.Dock = System.Windows.Forms.DockStyle.Fill;
                pnlInspAuto0.Controls.Add(smallInspItem);

                smallInspItem.UpdateInfos(DB400.INSPECTION_RESULT, this.Inspection.LastInspectionImage, this.Inspection.UltimaImagemInspecao, this.Inspection.LastInspectionMsgErro, 808, 172);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("InsertIntoDGV(): " + ex.Message);
            }
            finally
            {
                while (dgvUltimosAneisProcessados.Rows.Count > spMaxdgv)
                    dgvUltimosAneisProcessados.Rows.RemoveAt(spMaxdgv);
            }
        }

        Control GetControlByName(string Name)
        {
            foreach (Control c in this.panel39.Controls)
                if (c.Name == Name)
                    return c;

            return null;
        }


        //Este Timer NÂO é de 1Hz mas sim de 0.1Hz(100ms)
        private void Timer1Hz_Tick(object sender, EventArgs e)
        {
            if (VARIAVEIS.FORM_LOADED)
                try
                {
                    Timer01Hz.Stop();

                    bool permissaoBotoes = this.VerificaAcesso(Sessao.SessaoOperador.Operador1, false) && VARIAVEIS.ESTADO_CONEXAO_PLC;

                    //Deteta as memorias de clock
                    //bool FP_CLOCK_1HZ = (DB400.CLOCK_1HZ != FP[0]) && DB400.CLOCK_1HZ;
                    //bool FP_CLOCK_2HZ = (DB400.CLOCK_2HZ != FP[1]) && DB400.CLOCK_2HZ;

                    PlcControl.CmdCiclo.Vars.Reserved_2 = false;
                    //Atualiza o flash dos alarmes
                    //if (FP_CLOCK_1HZ)
                    AtualizaAnimacaoAlarmes();

                    #region Ecrã Manual/Automático
                    if (DB400.MANUAL_AUTO && !this.fpManualAuto)
                    {
                        tabControlEx1.SelectedIndex = 0;
                        this.fpManualAuto = true;
                    }
                    if (!DB400.MANUAL_AUTO && this.fpManualAuto)
                    {
                        tabControlEx1.SelectedIndex = 1;
                        this.fpManualAuto = false;
                    }
                    #endregion

                    #region 2HZ Clock
                    if (true)
                    {
                        #region Atualizar Bottom Bar

                        //Atualizar a data e hora
                        lblDataHora.Text = Convert.ToString(DateTime.Now);

                        // Sessão
                        //Sessão de Operador
                        Diversos.AtualizaBackColor(ledUser, UserSession.SessaoIniciada, Color.LimeGreen, Color.OrangeRed);
                        lblNomeOperadorAtivo.Text = UserSession.NomeOperador;

                        //Tempo de ciclo PLC
                        //lblTempoDeCiclo.Text = Convert.ToString(PLC1.TempoCicloAtual) + " ms";

                        //PLC Led
                        if (PlcControl != null)
                            switch (PlcControl.PLC1.UltimoEstadoPLC)
                            {
                                case Siemens.EstadoPLC.LigadoComLigacao:
                                    ledPlc.BackColor = Color.LimeGreen;
                                    break;
                                case Siemens.EstadoPLC.LigadoSemLigacao:
                                    ledPlc.BackColor = Color.Gray;
                                    break;
                                case Siemens.EstadoPLC.DesligadoSemLigacao:
                                    ledPlc.BackColor = Color.Red;
                                    break;
                                case Siemens.EstadoPLC.DesligadoComLigacao:
                                    ledPlc.BackColor = Color.Orange;
                                    break;
                            }

                        VARIAVEIS.ESTADO_CONEXAO_PLC = this.PlcControl.PLC1.EstadoLigacao;
                        
                        //Led conexão c/ base de dados
                        Diversos.AtualizaBackColor(ledDB, VariaveisAuxiliares.DatabaseConnectionState, Color.LimeGreen, Color.Red);

                        //Led da camera
                        Diversos.AtualizaBackColor(ledCam, this.Inspection.CameraAtiva, Color.LimeGreen, Color.Red);

                        //Led da porta
                        Diversos.AtualizaBackColor(ledPortaSeguranca1, DB400.ALL_DOORS_CLOSED, Color.LimeGreen, Color.Red);

                        //Atualizar led da iluminação
                        Diversos.AtualizaBackColor(btnIluminacao, DB400.VISION_LIGHT_ON, Color.LimeGreen, Color.White);

                        //Atualizar led da iluminação
                        Diversos.AtualizaBackColor(btnTrigger, this.Inspection.InspecaoEmCurso, Color.LimeGreen, Color.White);

                        //Habilitar os botões da trigger/iluminação/guardar imagem se houver uma sessão ativa
                        btnTrigger.Enabled = button3.Enabled = btnIluminacao.Enabled = permissaoBotoes && !this.Inspection.InspecaoEmCurso;
                        if (this.Inspection.GravacaoEmCurso)
                            button3.Enabled = false;


                        #endregion

                        #region Tabela dos Alarmes
                        if (AlarmsHandling.ClassOK)
                        {
                            DataTable tabela = null;

                            //Caso haja alterações nos alarmes atualmente ativos
                            if (AlarmsHandling.UpdateAlarms(VARIAVEIS.Alarmes, out tabela))
                            {
                                dgvActiveAlarms.DataSource = tabela;

                                //Verifica a largura das colunas
                                if (dgvActiveAlarms.ColumnCount == 4 && dgvActiveAlarms.Columns[1].Width != 90)
                                {
                                    for (int i = 0; i < dgvActiveAlarms.ColumnCount; i++)
                                        dgvActiveAlarms.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                                    dgvActiveAlarms.Columns[0].Width = 78;
                                    dgvActiveAlarms.Columns[1].Width = 135;
                                    dgvActiveAlarms.Columns[3].Width = 77;

                                    dgvActiveAlarms.Columns[2].Width = dgvActiveAlarms.Width - dgvActiveAlarms.Columns[0].Width - dgvActiveAlarms.Columns[1].Width - dgvActiveAlarms.Columns[3].Width;

                                }

                                //No final limpa a seleção da tabela
                                dgvActiveAlarms.ClearSelection();
                            }
                        }

                        #endregion

                        switch (tabControlEx1.SelectedIndex)
                        {

                            case 1: //Manual
                                {
                                    #region Status botoes

                                    Diversos.AtualizaBackColor(button27, DB400.PORTA_ESQUERDA == 1 && VARIAVEIS.ESTADO_CONEXAO_PLC, Color.LimeGreen, Color.Transparent);
                                    Diversos.AtualizaBackColor(button28, DB400.PORTA_DIREITA == 1 && VARIAVEIS.ESTADO_CONEXAO_PLC, Color.LimeGreen, Color.Transparent);
                                    Diversos.AtualizaBackColor(button29, DB400.PORTA_DIREITA == 1 && DB400.PORTA_ESQUERDA == 1 && VARIAVEIS.ESTADO_CONEXAO_PLC, Color.LimeGreen, Color.Transparent);
                                    Diversos.AtualizaBackColor(button30, DB400.PORTA_DIREITA == 2 && DB400.PORTA_ESQUERDA == 2 && VARIAVEIS.ESTADO_CONEXAO_PLC, Color.LimeGreen, Color.Transparent);

                                    //robot

                                    //ledBulb3.On = DB400.STA_SAFE_POSITION && VARIAVEIS.ESTADO_CONEXAO_PLC;
                                    //Diversos.AtualizaBackColor(button31, INPUTS.SEN_ROBOT_GARRA_ABERTA && VARIAVEIS.ESTADO_CONEXAO_PLC, Color.LimeGreen, Color.Transparent);
                                    //Diversos.AtualizaBackColor(button32, !INPUTS.SEN_ROBOT_GARRA_ABERTA && VARIAVEIS.ESTADO_CONEXAO_PLC, Color.LimeGreen, Color.Transparent);

                                    //permissoes botoes
                                    button27.Enabled = button28.Enabled = button29.Enabled = button30.Enabled  = permissaoBotoes && !VARIAVEIS.WINDOW_OPEN;
                                    #endregion

                                    //TODO
                                    //btnModoAutomatico.Enabled = permissaoBotoes && !VARIAVEIS.WINDOW_OPEN;

                                    btnModoAutomatico.Enabled = true;

                                    //ledEmergenciaOK.On = DB400.EMERGENCIA && VARIAVEIS.ESTADO_CONEXAO_PLC;
                                    //ledBulb1.On = INPUTS.SEN_KNIFE_CONVYER_DETECTED && VARIAVEIS.ESTADO_CONEXAO_PLC;
                                    //ledBulb2.On = INPUTS.SEN_CILINDRO_CONVOYER_INDEX && VARIAVEIS.ESTADO_CONEXAO_PLC;
                                    button5.Visible = this.VerificaAcesso(Sessao.SessaoOperador.Administrador, false);
                                    button5.Enabled = VARIAVEIS.ESTADO_CONEXAO_PLC;
                                    Diversos.AtualizaBackColor(button5, DB400.BYPASS_SECURITY, Color.LimeGreen, Color.White);

                                    button27.Visible = this.VerificaAcesso(Sessao.SessaoOperador.Administrador, false);
                                    button28.Visible = this.VerificaAcesso(Sessao.SessaoOperador.Administrador, false);

                                    break;
                                }
                        }

                    }
                    #endregion

                    #region Ecra Automatico
                    if (this.VariaveisAuxiliares.SelectedTabIndex == 0)
                    {
                        //inserir status
                        //pctDescarregaNOK.Visible = DB400.DESCARGA_NOK_EM_CURSO;
                        //pctDescarregaOK.Visible = DB400.DESCARGA_OK_EM_CURSO;


                        if (true)
                        {
                            //contadores
                            //lblTotalFacasNOK.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? (DB400.TOTAL_REPROVED_BY_BOX.ToString() + "/" + this.Receita.SpNOK.ToString()) : "###/###";
                            //lblTotalFacasOK.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? (DB400.TOTAL_APROVED_BY_BOX.ToString() + "/" + this.Receita.SpOK.ToString()) : "###/###";
                            //lblTotalPassadeira.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? DB400.LOTE_TOTAL_NUMBER.ToString() : "###";

                            btnModoManual.Enabled = permissaoBotoes && !DB400.CYCLE_ON;

                            //label65.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? Convert.ToString(DB400.SUB_COUNTER_KNIFES) + " de " + Convert.ToString(DB400.KNIFES_TO_ENGRAVE) : PLC1.StringSemComunicacao;

                            // tempo de ciclo
                            //label22.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? DB400.TEMPO_DE_CICLO.ToString() + " Seg" : "### Seg";
                            //labelTotal.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? Convert.ToString(DB400.COUNTER_KNIFES_ENGRAVED) : PLC1.StringSemComunicacao;

                            //labelTotal.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? Convert.ToString(DB400.TOTAL_APROVED+ DB400.TOTAL_REPROVED) : PLC1.StringSemComunicacao;
                            //labelTotalReprovadas.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? Convert.ToString(DB400.TOTAL_REPROVED) : PLC1.StringSemComunicacao;
                            //labelTotalAprovadas.Text = VARIAVEIS.ESTADO_CONEXAO_PLC ? Convert.ToString(DB400.TOTAL_APROVED) : PLC1.StringSemComunicacao;

                            //ledRobot1.On = DB400.INF_ROBO_READY;
                            //ledRobot2.On = INPUTS.SEN_ROBOT_GARRA_ABERTA;
                            //ledRobot3.On = INPUTS.STA_INI_POSITION;
                            //ledRobot6.On = OUTPUTS.ORD_GRAB_CONVOYER;
                            //ledRobot7.On = OUTPUTS.ORD_TAKE_INSPECTION;
                            //ledRobot8.On = OUTPUTS.ORD_APROVE_KNIFE;
                            //ledRobot9.On = OUTPUTS.ORD_REPROVE_KNIFE;
                           
                        }
                    }
                    #endregion

                    #region Ecra Manual
                    if (this.tabControlEx1.SelectedIndex == 1)
                    {
                        //******** blinks alarmes ********

                    }
                    #endregion

                    #region Atualizar Resultados de Inspeção

                    if (true && pnlSemConexao.Visible)
                        if (label16.ForeColor == Color.DarkOrange)
                            label16.ForeColor = Color.Red;
                        else
                            label16.ForeColor = Color.DarkOrange;

                    #region Visiblidade Visao                   
                    if (this.Inspection.InspecaoEmCurso == (lblTempoAcquisicao.ForeColor == Color.White)) //mostra resultados
                        UpdateInspectionStatus(this.Inspection.ResultadoInspecao);


                    //label8.Visible = this.Inspection.ErroInspecao;
                    //label8.Text = this.Inspection.MensagemErro;

                    if (true)
                    {
                        button4.Visible = button10.Visible = button13.Visible = button14.Visible = this.VerificaAcesso(Sessao.SessaoOperador.SuperAdministrador, false);
                        label6.Visible = label10.Visible = lblTempoAcquisicao.Visible = lblTempoInspecao.Visible = this.VerificaAcesso(Sessao.SessaoOperador.Administrador, false);

                        if (button4.Visible)
                        {
                            Diversos.AtualizaBackColor(button10, this.Inspection.ResultadosForcados == 1, Color.LimeGreen, Color.White);
                            Diversos.AtualizaBackColor(button13, this.Inspection.ResultadosForcados == 2, Color.LimeGreen, Color.White);
                            Diversos.AtualizaBackColor(button14, this.Inspection.ResultadosForcados == 0, Color.LimeGreen, Color.White);
                        }
                    }

                    //if (VariaveisAuxiliares.VISIBILIDADE_VISAO)
                    //{
                    //    if (this.vision.InspecaoEmCurso)
                    //    {
                    //        lblInspectionDt.Text = string.Empty;

                    //        lblTotalFacasDetetadas.Text = "EM INSPEÇÃO...";

                    //        lblInspectionDt.ForeColor = lblTotalFacasDetetadas.ForeColor = Color.Gray;
                    //    }
                    //    else
                    //    {
                    //        lblTotalFacasDetetadas.Text = "FACAS DETETADAS: " + this.vision.NumOfKnifesDetected.ToString();
                    //        lblInspectionDt.Text = this.vision.TempoInicioInspecao.ToString("dd/MM/yyyy HH:mm:ss");
                    //        lblInspectionDt.ForeColor = lblTotalFacasDetetadas.ForeColor = this.vision.ResultadoInspecao == 1 ? Color.LimeGreen : Color.Red;
                    //    }

                    //    //verifica se tem uma nova imagem para por na picturebox da inspecao
                    //    if (this.vision.NovaPecaProcessada)
                    //    {
                    //        this.vision.NovaPecaProcessada = false;
                    //        pctImagemInspecao.Image = new Bitmap(this.vision.ImagemInspecaoOCV);
                    //    }
                    //}

                    #endregion

                    #endregion

                    #region Registo da Tabela de Últimos Anéis Processados
                    //if (this.VariaveisAuxiliares.AuxOrdInsertData)
                    //{
                    //    this.InsertIntoDGV(DB400.INSPECTION_RESULT);
                    //    this.VariaveisAuxiliares.AuxOrdInsertData = false;
                    //}
                    #endregion

                    #region Inputs/Outputs
                    if (this.VariaveisAuxiliares.SelectedTabIndex == 6)
                    {
                        #region Inputs
                        Diversos.AtualizaBackColor(ledM0_0, INPUTS.BTN_START);
                        Diversos.AtualizaBackColor(ledM0_1, INPUTS.BTN_STOP);
                        Diversos.AtualizaBackColor(ledM0_2, INPUTS.BTN_RESET);
                        Diversos.AtualizaBackColor(ledM0_3, INPUTS.BTN_PC_ON);
                        Diversos.AtualizaBackColor(ledM0_4, INPUTS.SEN_PORTA_DIREITA_FECHADA);
                        Diversos.AtualizaBackColor(ledM0_5, INPUTS.SEN_PORTA_DIREITA_ABERTA);
                        Diversos.AtualizaBackColor(ledM0_6, INPUTS.SEN_PORTA_ESQUERDA_FECHADA);
                        Diversos.AtualizaBackColor(ledM0_7, INPUTS.SEN_PORTA_ESQUERDA_ABERTA);

                        Diversos.AtualizaBackColor(ledM1_0, INPUTS.SEN_KNIFE_CONVYER_DETECTED);
                        Diversos.AtualizaBackColor(ledM1_1, INPUTS.SEN_INDEX_FACA_BAIXO);
                        Diversos.AtualizaBackColor(ledM1_2, INPUTS.SEN_ROBOT_GARRA_ABERTA);
                        Diversos.AtualizaBackColor(ledM1_3, INPUTS.SEN_CILINDRO_CONVOYER_INDEX);
                        Diversos.AtualizaBackColor(ledM1_4, INPUTS.SEN_PRESENCA_CAIXA_APROVADOS);
                        Diversos.AtualizaBackColor(ledM1_5, INPUTS.SEN_PRESENCA_CAIXA_REPROVADOS);
                        Diversos.AtualizaBackColor(ledM1_6, INPUTS.RESERVA_1_6);
                        Diversos.AtualizaBackColor(ledM1_7, INPUTS.RESERVA_1_7);

                        Diversos.AtualizaBackColor(ledM2_0, INPUTS.STU_THP_VFR);
                        Diversos.AtualizaBackColor(ledM2_1, INPUTS.STU_VFR_FAULT);
                        Diversos.AtualizaBackColor(ledM2_2, INPUTS.STU_VFR_RUNNING);
                        Diversos.AtualizaBackColor(ledM2_3, INPUTS.RESERVA_2_3);
                        Diversos.AtualizaBackColor(ledM2_4, INPUTS.RESERVA_2_4);
                        Diversos.AtualizaBackColor(ledM2_5, INPUTS.RESERVA_2_5);
                        Diversos.AtualizaBackColor(ledM2_6, INPUTS.RESERVA_2_6);
                        Diversos.AtualizaBackColor(ledM2_7, INPUTS.RESERVA_2_7);

                        Diversos.AtualizaBackColor(ledM3_0, INPUTS.STA_GRAB_CONVOYER);
                        Diversos.AtualizaBackColor(ledM3_1, INPUTS.STA_TAKE_INSPECTION);
                        Diversos.AtualizaBackColor(ledM3_2, INPUTS.STA_APROVE_KNIFE);
                        Diversos.AtualizaBackColor(ledM3_3, INPUTS.STA_REPROVE_KNIFE);
                        Diversos.AtualizaBackColor(ledM3_4, INPUTS.STA_INI_POSITION);
                        Diversos.AtualizaBackColor(ledM3_5, INPUTS.RESERVA_3_5);
                        Diversos.AtualizaBackColor(ledM3_6, INPUTS.RESERVA_3_6);
                        Diversos.AtualizaBackColor(ledM3_7, INPUTS.RESERVA_3_7);

                        Diversos.AtualizaBackColor(ledM4_0, INPUTS.RESERVA_4_0);
                        Diversos.AtualizaBackColor(ledM4_1, INPUTS.RESERVA_4_1);
                        Diversos.AtualizaBackColor(ledM4_2, INPUTS.RESERVA_4_2);
                        Diversos.AtualizaBackColor(ledM4_3, INPUTS.RESERVA_4_3);
                        Diversos.AtualizaBackColor(ledM4_4, INPUTS.RESERVA_4_4);
                        Diversos.AtualizaBackColor(ledM4_5, INPUTS.RESERVA_4_5);
                        Diversos.AtualizaBackColor(ledM4_6, INPUTS.RESERVA_4_6);
                        Diversos.AtualizaBackColor(ledM4_7, INPUTS.RESERVA_4_7);

                        Diversos.AtualizaBackColor(ledM1000_0, DB400.EMERGENCIA);
                        Diversos.AtualizaBackColor(ledM1000_1, INPUTS.FECHO_CAMERA);
                        Diversos.AtualizaBackColor(ledM1000_2, INPUTS.FECHO_ILUMINACAO);
                        Diversos.AtualizaBackColor(ledM1000_3, INPUTS.FECHO_ROBOT);
                        Diversos.AtualizaBackColor(ledM1000_4, INPUTS.FECHO_ECRA);
                        Diversos.AtualizaBackColor(ledM1000_5, INPUTS.BARREIRA_APROVADAS);
                        Diversos.AtualizaBackColor(ledM1000_6, INPUTS.BARREIRA_REPROVADAS);
                        Diversos.AtualizaBackColor(ledM1000_7, INPUTS.RESERVA_1000_7);

                        #endregion

                        #region Outputs
                        Diversos.AtualizaBackColor(ledM10_0, OUTPUTS.CMD_DESINDEXA_FACA);
                        Diversos.AtualizaBackColor(ledM10_1, OUTPUTS.CMD_INDEXA_FACA);
                        Diversos.AtualizaBackColor(ledM10_2, OUTPUTS.RESERVA_VALVULA_11);
                        Diversos.AtualizaBackColor(ledM10_3, OUTPUTS.RESERVA_VALVULA_12);
                        Diversos.AtualizaBackColor(ledM10_4, OUTPUTS.CMD_PORTA_DIREITA_ABRE);
                        Diversos.AtualizaBackColor(ledM10_5, OUTPUTS.CMD_PORTA_DIREITA_FECHA);
                        Diversos.AtualizaBackColor(ledM10_6, OUTPUTS.CMD_PORTA_ESQUERDA_ABRE);
                        Diversos.AtualizaBackColor(ledM10_7, OUTPUTS.CMD_PORTA_ESQUERDA_FECHA);

                        Diversos.AtualizaBackColor(ledM11_0, OUTPUTS.CMD_RES_VALV_5);
                        Diversos.AtualizaBackColor(ledM11_1, OUTPUTS.CMD_RES_VALV_5_ERROR);
                        Diversos.AtualizaBackColor(ledM11_2, OUTPUTS.CMD_RES_VALV_11_2);
                        Diversos.AtualizaBackColor(ledM11_3, OUTPUTS.CMD_RES_VALV_11_3);
                        Diversos.AtualizaBackColor(ledM11_4, OUTPUTS.CMD_RES_VALV_11_4);
                        Diversos.AtualizaBackColor(ledM11_5, OUTPUTS.CMD_RES_VALV_11_5);
                        Diversos.AtualizaBackColor(ledM11_6, OUTPUTS.CMD_RES_VALV_11_6);
                        Diversos.AtualizaBackColor(ledM11_7, OUTPUTS.CMD_RES_VALV_11_7);

                        Diversos.AtualizaBackColor(ledM12_0, OUTPUTS.CMD_RES_VALV_12_0);
                        Diversos.AtualizaBackColor(ledM12_1, OUTPUTS.CMD_RES_VALV_12_1);
                        Diversos.AtualizaBackColor(ledM12_2, OUTPUTS.CMD_TOWER_GREEN);
                        Diversos.AtualizaBackColor(ledM12_3, OUTPUTS.CMD_TOWER_RED);
                        Diversos.AtualizaBackColor(ledM12_4, OUTPUTS.CMD_TOWER_YELLOW);
                        Diversos.AtualizaBackColor(ledM12_5, OUTPUTS.CMD_TOWER_BUZZER);
                        Diversos.AtualizaBackColor(ledM12_6, OUTPUTS.CMD_LED_START);
                        Diversos.AtualizaBackColor(ledM12_7, OUTPUTS.CMD_LED_RED);

                        Diversos.AtualizaBackColor(ledM13_0, OUTPUTS.CMD_LED_RESET);
                        Diversos.AtualizaBackColor(ledM13_1, OUTPUTS.CMD_LED_PC_ON);
                        Diversos.AtualizaBackColor(ledM13_2, OUTPUTS.CMD_FECHO_CAMERA);
                        Diversos.AtualizaBackColor(ledM13_3, OUTPUTS.CMD_FECHO_ILUMINACAO);
                        Diversos.AtualizaBackColor(ledM13_4, OUTPUTS.CMD_FECHO_MONITOR);
                        Diversos.AtualizaBackColor(ledM13_5, OUTPUTS.CMD_FECHO_JAULA);
                        Diversos.AtualizaBackColor(ledM13_6, OUTPUTS.CMD_RUN_VFR);
                        Diversos.AtualizaBackColor(ledM13_7, OUTPUTS.CMD_RESET_VFR);

                        Diversos.AtualizaBackColor(ledM14_0, OUTPUTS.CMD_VFR_ON);
                        Diversos.AtualizaBackColor(ledM14_1, OUTPUTS.OUT_RESERVA_14_1);
                        Diversos.AtualizaBackColor(ledM14_2, OUTPUTS.ORD_GRAB_CONVOYER);
                        Diversos.AtualizaBackColor(ledM14_3, OUTPUTS.ORD_TAKE_INSPECTION);
                        Diversos.AtualizaBackColor(ledM14_4, OUTPUTS.ORD_APROVE_KNIFE);
                        Diversos.AtualizaBackColor(ledM14_5, OUTPUTS.ORD_REPROVE_KNIFE);
                        Diversos.AtualizaBackColor(ledM14_6, OUTPUTS.ORD_INI_POSITION);
                        Diversos.AtualizaBackColor(ledM14_7, OUTPUTS.OUT_RESERVA_14_7);

                        Diversos.AtualizaBackColor(ledM1010_0, OUTPUTS.CMD_CORTE_AR);
                        Diversos.AtualizaBackColor(ledM1010_1, OUTPUTS.EMG_ROBOT);
                        Diversos.AtualizaBackColor(ledM1010_2, OUTPUTS.PORTAS_ROBOT);
                        Diversos.AtualizaBackColor(ledM1010_3, OUTPUTS.CMD_RESERVA);

                        #endregion
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception Timer1Hz_Tick: " + ex.Message);
                }
                finally
                {
                    //iguala as memorias dos clocks
                    FP[0] = DB400.CLOCK_1HZ;
                    FP[1] = DB400.CLOCK_2HZ;

                    //Chamar o coletor de memória
                    GC.Collect(); //** para limpar as imagens anteriores da picturebox da imagem adquirida

                    if (VARIAVEIS.FLAG_WHILE_CYCLE)
                        Timer01Hz.Start();
                }
        }

        //Timer de 10ms
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.VariaveisAuxiliares.AuxOrdInsertData)
            {
                this.InsertIntoDGV(DB400.INSPECTION_RESULT);
                this.VariaveisAuxiliares.AuxOrdInsertData = false;
            }
        }
        private void AtualizaHistoricoAlarmes()
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(VariaveisAuxiliares.DatabaseConnectionString))
                using (SqlCommand sqlCmd = new SqlCommand("SELECT HISTORICO_ALARMES.ID, 'ALM' + CASE WHEN ALARMES.ID < 10 THEN RIGHT( '0' + CAST(ALARMES.ID AS nvarchar(2)),2) + ' - ' + ALARMES.TEXTO else RIGHT(CAST(ALARMES.ID AS nvarchar(3)),3) + ' - ' + ALARMES.TEXTO END AS 'Descrição', HISTORICO_ALARMES.TEMPO_INICIO AS 'Data/Hora', NIVEL_ALARMES.TEXTO AS 'Tipo' FROM HISTORICO_ALARMES INNER JOIN ALARMES ON HISTORICO_ALARMES.ID_ALARME = ALARMES.ID INNER JOIN NIVEL_ALARMES ON ALARMES.ID_NIVEL_ALARME = NIVEL_ALARMES.ID ORDER BY HISTORICO_ALARMES.ID DESC", sqlConn))
                {
                    sqlConn.Open();

                    using (SqlDataAdapter da = new SqlDataAdapter(sqlCmd))
                    using (DataSet ds = new DataSet())
                    {
                        da.Fill(ds);

                        dgvHistoricAlarms.DataSource = ds.Tables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                new CaixaMensagem(ex.Message, "Histórico de Alarmes", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
            }
            finally
            {
                if (dgvHistoricAlarms.ColumnCount > 2)
                {
                    dgvHistoricAlarms.Columns[0].Width = 80;
                    dgvHistoricAlarms.Columns[1].Width = 515;
                    dgvHistoricAlarms.Columns[2].Width = 135;
                }

                //No final limpa a seleção da tabela
                dgvHistoricAlarms.ClearSelection();

                AtualizaCoresHistoricoAlarmes();
            }

        }

        private void AtualizaCoresHistoricoAlarmes()
        {
            if (dgvHistoricAlarms.RowCount > 0)
            {
                Color CorPar1 = Color.Red;
                Color CorPar2 = Color.Orange;
                Color CorPar3 = Color.DodgerBlue;

                for (int i = 0; i < dgvHistoricAlarms.RowCount; i++)
                {
                    int alarmNumber = Convert.ToInt32(Convert.ToString(dgvHistoricAlarms[1, i].Value).Substring(3, 2));

                    switch (AlarmsHandling.alarms[alarmNumber - 1].AlarmType)
                    {
                        case 1:
                            {
                                dgvHistoricAlarms.Rows[i].DefaultCellStyle.BackColor = CorPar1;
                                break;
                            }
                        case 2:
                            {
                                dgvHistoricAlarms.Rows[i].DefaultCellStyle.BackColor = CorPar2;
                                break;
                            }
                        case 3:
                            {
                                dgvHistoricAlarms.Rows[i].DefaultCellStyle.BackColor = CorPar3;
                                break;
                            }
                        default:
                            {
                                dgvHistoricAlarms.Rows[i].DefaultCellStyle.BackColor = CorPar1;
                                break;
                            }
                    }
                }
            }
        }

        private void picBoxEstadoSessaoOperador_Click(object sender, EventArgs e)
        {
            if (!UserSession.SessaoIniciada)
                new Login(VariaveisAuxiliares.DatabaseConnectionString).ShowDialog();
            else if (new CaixaMensagem("Deseja Sair?", "Sessão", CaixaMensagem.TipoMsgBox.Question).ShowDialog() == DialogResult.Yes)
                UserSession.TerminaSessao();

            this.AtualizaDadosSessao();
        }

        private void btnSessaoIniciarSessao_Click(object sender, EventArgs e)
        {
            if (!UserSession.SessaoIniciada)
                new Login(VariaveisAuxiliares.DatabaseConnectionString).ShowDialog();
            else new CaixaMensagem("Já existe uma sessão ativa.", "Sessão", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();

            this.AtualizaDadosSessao();
        }

        private void btnSessaoTerminarSessao_Click(object sender, EventArgs e)
        {
            if (UserSession.SessaoIniciada)
            {
                if (new CaixaMensagem("Deseja sair?", "Sessão", CaixaMensagem.TipoMsgBox.Question).ShowDialog() == DialogResult.Yes)
                    UserSession.TerminaSessao();
            }
            else
                new CaixaMensagem("Não há nenhuma sessão iniciada.", "Sessão", CaixaMensagem.TipoMsgBox.Warning).ShowDialog();

            this.AtualizaDadosSessao();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (!VARIAVEIS.WINDOW_OPEN)
                tabControlEx1.SelectedIndex = 1;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (!VARIAVEIS.WINDOW_OPEN)
                tabControlEx1.SelectedIndex = 2;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (!VARIAVEIS.WINDOW_OPEN)
                if (VerificaAcesso(Sessao.SessaoOperador.Administrador))
                    tabControlEx1.SelectedIndex = 3;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!VARIAVEIS.WINDOW_OPEN)
                if (VerificaAcesso(Sessao.SessaoOperador.Operador1))
                    tabControlEx1.SelectedIndex = 4;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void AtualizaDadosSessao()
        {
            //Sessão de Operador
            if (UserSession.SessaoIniciada)
            {
                picBoxEstadoSessaoOperador.BackColor = Color.LimeGreen;
                txtSessaoEstado.Text = "Com sessão iniciada";
            }
            else
            {
                picBoxEstadoSessaoOperador.BackColor = Color.OrangeRed;
                txtSessaoEstado.Text = "Sem sessão iniciada";
            }

            txtSessaoEstado.ForeColor = picBoxEstadoSessaoOperador.BackColor;

            txtSessaoOperador.Text = UserSession.NomeOperador;
            txtSessaoDataHoraInicio.Text = UserSession.DataHoraInicioSessao;
            label63.Text = UserSession.StrNivelSessao;


            //Habilita também a alteração do num de lote
            txtNumSerieReceita.Enabled = VerificaAcesso(Sessao.SessaoOperador.Operador1, false) && Receita.ReceitaCarregada;
        }

        private void tabControlEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlMenuManual.Visible = tabControlEx1.SelectedIndex > 0;
            pnlLegendaAutomatico.Visible = !pnlMenuManual.Visible;

            TimerJanelas.Enabled = tabControlEx1.SelectedIndex == 0;

            VariaveisAuxiliares.SelectedTabIndex = tabControlEx1.SelectedIndex;

            //Atualizar a cor dos botões de estado
            panel40.BackColor = VariaveisAuxiliares.SelectedTabIndex == 1 ? Color.SkyBlue : Color.LightGray;
            panel41.BackColor = VariaveisAuxiliares.SelectedTabIndex == 2 ? Color.SkyBlue : Color.LightGray;
            panel42.BackColor = VariaveisAuxiliares.SelectedTabIndex == 3 ? Color.SkyBlue : Color.LightGray;
            panel44.BackColor = VariaveisAuxiliares.SelectedTabIndex == 4 ? Color.SkyBlue : Color.LightGray;
            panel20.BackColor = VariaveisAuxiliares.SelectedTabIndex == 5 ? Color.SkyBlue : Color.LightGray;
            panel14.BackColor = VariaveisAuxiliares.SelectedTabIndex == 6 ? Color.SkyBlue : Color.LightGray;

            switch (VariaveisAuxiliares.SelectedTabIndex)
            {
                case 0: //Automático
                    {
                        #region Itens da Receita
                        label68.Text = Receita.Referencia;
                        label66.Text = Receita.Designacao;
                        label113.Text = Diversos.NumberToString(this.Receita.ComprimentoNominal, 2, true) + " mm";
                        label43.Text = Diversos.NumberToString(this.Receita.ToleranciaComprimento, 2, true) + " mm";
                        label69.Text = Receita.NumSerie;
                        label44.Text = Diversos.NumberToString(this.Receita.ToleranciaDesvioSuperior, 2, true) + " mm";
                        label45.Text = Diversos.NumberToString(this.Receita.ToleranciaDesvioInferior, 2, true) + " mm";

                        #endregion

                        break;
                    }
                case 1: //Manual
                    {

                        break;
                    }
                case 2: //Sessão Receitas
                    {
                        //Atualizar os dados de sessão
                        this.AtualizaDadosSessao();

                        //Atualiza Dados Receita
                        this.AtualizaInformacoesReceita();

                        break;
                    }
                case 3: //Gestão Users
                    {
                        this.LimpaCampos();

                        this.ListaTodosOperadores();

                        break;
                    }
                case 4: //Historico Alarmes
                    {
                        this.AtualizaHistoricoAlarmes();
                        break;
                    }
                case 5: //Ios
                    {

                        break;
                    }
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!VARIAVEIS.WINDOW_OPEN)
                if (VerificaAcesso(Sessao.SessaoOperador.Operador1))
                    tabControlEx1.SelectedIndex = 6;
        }

        private void button37_Click(object sender, EventArgs e)
        {
            if (DB400.MANUAL_AUTO)
                if (new CaixaMensagem("Deseja ir para o modo manual?", "Modo Manual", CaixaMensagem.TipoMsgBox.Question).ShowDialog() == DialogResult.Yes)
                {
                    PlcControl.CmdCiclo.Vars.ManualRequest = true;
                }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (VerificaAcesso(Sessao.SessaoOperador.Operador1))
                //Apresentar a janela de passagem para modo automático
                if (new ModoAutomatico().ShowDialog() == DialogResult.Yes)
                {
                    PlcControl.CmdCiclo.Vars.AutoRequest = true;
                }

        }

        private void cmbNivelOperador_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void cmbNivelOperador_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                e.Handled = true;
        }

        private void button41_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Manutencao))
                switch (Diversos.ExportarParaXLS(dgvOperadores))
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

        private void button97_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Operador1))
            {
                new ConfiguracoesAlarmes().ShowDialog();
                this.AtualizaHistoricoAlarmes();
            }
        }

        private void button51_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Manutencao))
                this.LimpaCampos();
        }

        private void LimpaCampos()
        {
            lblIdOperadorSelecionado.Text = "";
            label87.ForeColor = Color.White;
            label87.Text = "";
            txtNomeOperador.Text = "";
            cmbNivelOperador.SelectedIndex = 0;

            if (VerificaAcesso(Sessao.SessaoOperador.Manutencao, false))
                new Thread(this.ListaTodosOperadores).Start();
        }

        private void EditaOperadorSelecionado()
        {
            try
            {
                if (Diversos.IsNumeric(lblIdOperadorSelecionado.Text))
                    if (Convert.ToInt32(lblIdOperadorSelecionado.Text) > 0)
                        new AdicionarOperador("Editar Utilizador", Convert.ToInt32(lblIdOperadorSelecionado.Text)).ShowDialog();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EditaOperadorSelecionado() " + ex.Message);
                new CaixaMensagem(ex.Message, "Erro Listar Utilizador", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
            }

        }

        private void button42_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Manutencao))
                if (string.IsNullOrWhiteSpace(txtNomeOperador.Text) && cmbNivelOperador.SelectedIndex == 0)
                    ListaTodosOperadores();
                else
                    try
                    {
                        bool Nome = !string.IsNullOrWhiteSpace(txtNomeOperador.Text);
                        bool Nivel = cmbNivelOperador.SelectedIndex > 0;

                        StringBuilder strBase = new StringBuilder().Append("SELECT UTILIZADORES.ID, UTILIZADORES.NOME AS 'Nome', '****' AS 'Password', NIVEL_UTILIZADOR.NIVEL AS 'Nível' FROM UTILIZADORES INNER JOIN NIVEL_UTILIZADOR ON UTILIZADORES.ID_NIVEL = NIVEL_UTILIZADOR.ID WHERE UTILIZADORES.ID > 0 AND UTILIZADORES.ID_NIVEL < 5 AND ");

                        if (Nome)
                        {
                            strBase.Append("UTILIZADORES.NOME LIKE @NOME ");
                            if (Nivel)
                                strBase.Append("AND ");
                        }
                        if (Nivel)
                            strBase.Append("UTILIZADORES.ID_NIVEL = @NIVEL");

                        using (SqlConnection sqlConn = new SqlConnection(VariaveisAuxiliares.DatabaseConnectionString))
                        using (SqlCommand sqlCmd = new SqlCommand(strBase.ToString(), sqlConn))
                        {
                            if (Nome)
                                sqlCmd.Parameters.AddWithValue("@NOME", "%" + txtNomeOperador.Text + "%");
                            if (Nivel)
                                sqlCmd.Parameters.Add("@NIVEL", SqlDbType.TinyInt).Value = cmbNivelOperador.SelectedIndex == 1 ? 1 : 4;

                            sqlConn.Open();

                            using (SqlDataAdapter da = new SqlDataAdapter(sqlCmd))
                            using (DataSet ds = new DataSet())
                            {
                                da.Fill(ds);

                                if (dgvOperadores.InvokeRequired)
                                    dgvOperadores.Invoke(new MethodInvoker(() =>
                                    {
                                        dgvOperadores.DataSource = ds.Tables[0];
                                    }));
                                else
                                    dgvOperadores.DataSource = ds.Tables[0];

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        new CaixaMensagem(ex.Message, "Erro Pesquisar Operador", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                    }
        }

        private void dgvOperadores_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvOperadores.RowCount > 0)
            {
                try
                {
                    lblIdOperadorSelecionado.Text = Convert.ToString(dgvOperadores[0, dgvOperadores.CurrentRow.Index].Value);
                    label87.Text = Convert.ToString(dgvOperadores[1, dgvOperadores.CurrentRow.Index].Value);
                    label87.ForeColor = Color.LimeGreen;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Timer 2Hz -> case 3: //Users e Receitas" + ex.Message);
                }
            }
            else
            {
                lblIdOperadorSelecionado.Text = "0";
                label87.ForeColor = Color.White;
                label87.Text = "";
            }
        }

        private void button53_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Administrador))
            {
                new AdicionarOperador("Adicionar Utilizador").ShowDialog();
                ListaTodosOperadores();
            }
        }

        private void button52_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Administrador))
            {
                EditaOperadorSelecionado();
                ListaTodosOperadores();
            }
        }

        private void button54_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Administrador))
            {
                EliminaOperadorSelecionado();
                ListaTodosOperadores();
            }
        }

        private void EliminaOperadorSelecionado()
        {
            if (dgvOperadores.RowCount != 0)
                if (Diversos.IsNumeric(lblIdOperadorSelecionado.Text))
                {
                    try
                    {
                        int idOP = Convert.ToInt32(lblIdOperadorSelecionado.Text);
                        if (idOP > 0)
                        {
                            int resQuery = 0;
                            string nomeOperador = label87.Text;

                            if (new CaixaMensagem("Deseja realmente eliminar o utilizador " + label87.Text + " (" + lblIdOperadorSelecionado.Text + ")?", "Eliminar Utilizadores", CaixaMensagem.TipoMsgBox.Question).ShowDialog() == DialogResult.Yes)
                            {
                                using (SqlConnection sqlConn = new SqlConnection(VariaveisAuxiliares.DatabaseConnectionString))
                                using (SqlCommand sqlCmd = new SqlCommand("DELETE FROM UTILIZADORES WHERE ID = @ID", sqlConn))
                                {
                                    sqlCmd.Parameters.Add("@ID", SqlDbType.TinyInt).Value = idOP;

                                    sqlConn.Open();

                                    resQuery = sqlCmd.ExecuteNonQuery();
                                }

                                if (resQuery == 1)
                                    new CaixaMensagem("Utilizador " + nomeOperador + " eliminado com sucesso", "Eliminar Utilizadores", CaixaMensagem.TipoMsgBox.Normal).ShowDialog();
                                else
                                    throw new Exception("Erro ao eliminar utilizador " + nomeOperador);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("EliminaOperadorSelecionado() " + ex.Message);
                        new CaixaMensagem(ex.Message, "Erro Eliminar Utilizadores", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                    }
                }
        }

        public void AtualizaInformacoesReceita()
        {
            try
            {
                lblReferenciaReceita.Text = Receita.Referencia;
                lblReceitaCarregadaBottomBar.Text = Receita.Referencia;
                txtNumSerieReceita.Text = Receita.NumSerie;
                lblUtilizadorReceita.Text = Receita.NomeOperador;
                lblDesignacaoReceita.Text = Receita.Designacao;
                lblDistanciaCamFaca.Text = Diversos.NumberToString(Receita.CompRegiaoInsp, 0, true) + " px";
                lblComprimentoNominal.Text = Diversos.NumberToString(Receita.ComprimentoNominal, 3, true) + " mm";
                lblToleranciaComprimento.Text = Diversos.NumberToString(Receita.ToleranciaComprimento, 3, true) + " mm";
                lblDesvioNominal.Text = Diversos.NumberToString(Receita.DesvioNominal, 3, true) + " mm";
                lblToleranciaDesvio.Text = Diversos.NumberToString(Receita.ToleranciaDesvioSuperior, 3, true) + " mm";

                lblDataHoraInicioReceita.Text = Convert.ToString(Receita.DataHoraAlteracao);

                label53.Text = this.Receita.SpOK.ToString();
                label57.Text = this.Receita.SpNOK.ToString();

                //Preencher a imagem guardada
                pctImagemFaca.Image = Receita.CarregaImagemFaca();

            }
            catch (Exception ex)
            {
                Debug.WriteLine("AtualizaInformacoesReceita(): " + ex.Message);
            }
        }

        private void dgvHistoricAlarms_SelectionChanged(object sender, EventArgs e)
        {
            dgvHistoricAlarms.ClearSelection();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.Inspection != null)
                this.Inspection.SaveAtualImageAs();
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Operador1))
            {
                new AlterarReceita(VariaveisAuxiliares.DatabaseConnectionString).ShowDialog();
                this.AtualizaInformacoesReceita();
            }
        }

        private void txtNumSerieReceita_Leave(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Operador1, false))
                if (Receita.ReceitaCarregada)
                    Receita.AlteraNumSerie(txtNumSerieReceita.Text);

            this.AtualizaInformacoesReceita();
        }

        private void txtNumSerieReceita_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Operador1, false))
            {
                if (e.KeyChar == 13)
                {
                    e.Handled = true;

                    if (Receita.ReceitaCarregada)
                        Receita.AlteraNumSerie(txtNumSerieReceita.Text);
                }
            }
            else
                e.Handled = true;
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Operador1, true))
                new GestaoReceitas(VariaveisAuxiliares.DatabaseConnectionString).ShowDialog();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            new About(VariaveisAuxiliares.MachineName).ShowDialog();
        }


        private void dgvActiveAlarms_Leave(object sender, EventArgs e)
        {
            dgvActiveAlarms.ClearSelection();
        }

        private void dgvActiveAlarms_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvActiveAlarms.RowCount > 0 && dgvActiveAlarms.SelectedRows.Count > 0)
                new DetalheAlarme(VariaveisAuxiliares.DatabaseConnectionString, Convert.ToByte(dgvActiveAlarms[0, dgvActiveAlarms.CurrentRow.Index].Value)).ShowDialog();
        }

        private void dgvHistoricAlarms_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvHistoricAlarms.RowCount > 0 && dgvHistoricAlarms.SelectedRows.Count > 0)
                new DetalheAlarme(VariaveisAuxiliares.DatabaseConnectionString, Convert.ToInt64(dgvHistoricAlarms[0, dgvHistoricAlarms.CurrentRow.Index].Value)).ShowDialog();

        }

        private void dgvHistoricAlarms_Leave(object sender, EventArgs e)
        {
            dgvHistoricAlarms.ClearSelection();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            
           // if (new CaixaMensagem("Limpar Contador Total de Discos?", "Limpar Contador", CaixaMensagem.TipoMsgBox.Question).ShowDialog() == DialogResult.Yes) 
            PlcControl.CmdCiclo.Vars.Reserved_2 = true;
            Inspection.numberInspThreads = 0;
            Inspection.numberAqThreads = 0;
            PlcControl.HmiPlcNewDisc.menber.ID = 0;
            PlcControl.HmiPlcFeedbackdisc.menber.ID = 0;
        }

        private void Button4_Click_1(object sender, EventArgs e)
        {
            this.Inspection.FazTrigger();
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            this.Inspection.ResultadosForcados = 1;
        }

        private void LedCam_BackColorChanged(object sender, EventArgs e)
        {
            pnlSemConexao.Visible = ledCam.BackColor == Color.Red;
        }

        public void UpdateInspectionStatus(int status)
        {
            if (status == 1 || status == 3)
            {
                lblTempoAcquisicao.Text = this.Inspection.TempoAquisicao.ToString() + " ms";
                lblTempoInspecao.Text = this.Inspection.TempoInspecao.ToString() + " ms";
            }
            else
            {

            }
        }

        private void BtnTrigger_Click(object sender, EventArgs e)
        {

        }

        private void DgvUltimosAneisProcessados_SelectionChanged(object sender, EventArgs e)
        {
            dgvUltimosAneisProcessados.ClearSelection();
        }

        private void Button16_Click(object sender, EventArgs e)
        {
            if (!VARIAVEIS.WINDOW_OPEN)
                if (VerificaAcesso(Sessao.SessaoOperador.Operador1))
                    tabControlEx1.SelectedIndex = 5;
        }

        private void ComboBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void ComboBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                e.Handled = true;
        }

        private void Button2_MouseEnter(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Hand;
        }

        private void Label47_MouseLeave(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        }

        private void Button29_Click(object sender, EventArgs e)
        {

        }

        private void Button30_Click(object sender, EventArgs e)
        {

        }

        private void Button27_Click(object sender, EventArgs e)
        {

        }

        private void Button28_Click(object sender, EventArgs e)
        {

        }


        private void TimerJanelas_Tick(object sender, EventArgs e)
        {
            try
            {
                ////TODO verificar o porque do codigo enpancar no ShowDialog
                //if (DB400.DESCARGA_OK_FULL && !VARIAVEIS.FP_JANELAS_AUTOMATICO[0])
                //{ 
                //VARIAVEIS.FP_JANELAS_AUTOMATICO[0] = DB400.DESCARGA_OK_FULL;
                //new MensagemCaixaCheia(TipoCaixa.Aprovados).ShowDialog();
                // }

                //if (DB400.DESCARGA_NOK_FULL && !VARIAVEIS.FP_JANELAS_AUTOMATICO[1])
                //{
                //    VARIAVEIS.FP_JANELAS_AUTOMATICO[1] = DB400.DESCARGA_NOK_FULL;
                //    new MensagemCaixaCheia(TipoCaixa.Reprovados).ShowDialog();
                //}
                //if (DB400.STOP_BARREIRA_APROVADOS && !VARIAVEIS.FP_JANELAS_AUTOMATICO[2])
                //{
                //    VARIAVEIS.FP_JANELAS_AUTOMATICO[2] = DB400.STOP_BARREIRA_APROVADOS;
                //    new MensagemBarreiras(TipoCaixa.Aprovados).ShowDialog();
                //}
                //if (DB400.STOP_BARREIRA_REPROVADOS && !VARIAVEIS.FP_JANELAS_AUTOMATICO[3])
                //{
                //    VARIAVEIS.FP_JANELAS_AUTOMATICO[3] = DB400.STOP_BARREIRA_REPROVADOS;
                //    new MensagemBarreiras(TipoCaixa.Reprovados).ShowDialog();
                //}

                //if (DB400.SEQUENCIA_NOK && !VARIAVEIS.FP_JANELAS_AUTOMATICO[4])
                //{
                //    VARIAVEIS.FP_JANELAS_AUTOMATICO[4] = DB400.SEQUENCIA_NOK;
                //    new MensagemSequenciaReprovados().ShowDialog();
                //}


            }
            catch (Exception ex)
            {
                Debug.WriteLine("TimerJanelas_Tick(): " + ex.Message);
            }
            finally
            {
                VARIAVEIS.FP_JANELAS_AUTOMATICO[0] = DB400.DESCARGA_OK_FULL;
                VARIAVEIS.FP_JANELAS_AUTOMATICO[1] = DB400.DESCARGA_NOK_FULL;
                VARIAVEIS.FP_JANELAS_AUTOMATICO[2] = DB400.STOP_BARREIRA_APROVADOS;
                VARIAVEIS.FP_JANELAS_AUTOMATICO[3] = DB400.STOP_BARREIRA_REPROVADOS;
                VARIAVEIS.FP_JANELAS_AUTOMATICO[4] = DB400.SEQUENCIA_NOK;

            }
        }


        private void Button34_Click(object sender, EventArgs e)
        {
            if (VerificaAcesso(Sessao.SessaoOperador.Operador1))
                using (var form = new InputBox(InputBox.TipoValor.Int, "Definir Lote de Produção", 0, int.MaxValue))
                {
                    form.ShowDialog();

                    if (form.ValorSubmetido);
                        //PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.DInt, Convert.ToInt32(form.Valor), 23, 8);
                }
        }



        private void Button35_Click(object sender, EventArgs e)
        {
            //Forms.MainForm.PLC1.EnviaTag(PLC.Siemens.MemoryArea.DB, PLC.Siemens.TipoVariavel.Bool, true, 45, 2, 4);
        }


        private void Button13_Click_1(object sender, EventArgs e)
        {
            this.Inspection.ResultadosForcados = 2;
        }

        private void Button14_Click_1(object sender, EventArgs e)
        {
            this.Inspection.ResultadosForcados = 0;
        }

        private void Button5_Click_1(object sender, EventArgs e)
        {
            //this.PLC1.EnviaTag(Siemens.MemoryArea.M, Siemens.TipoVariavel.Bool, !DB400.BYPASS_SECURITY, 0, 101, 1);
        }

    }
}
