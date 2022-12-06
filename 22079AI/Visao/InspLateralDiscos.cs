using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualBasic;
using HalconDotNet;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using PLC;
using System.Collections.Concurrent;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics.Eventing.Reader;

namespace _22079AI
{
    public class InspLateralDiscos
    {

        //Estados da classe
        private bool isAlive = true;
        private bool capturaOK = false;
        private bool inspecaoOK = false;
        private string diretorioImagens = @"C:\STREAK\Imagens Gravadas\";
        private string scriptDirPath = Application.StartupPath + @"\Scripts\";

        public string FicheiroIni { get; } = Application.StartupPath + @"\InspectionConfig.ini";

        public int CounterInspNoTrigger = 0, CounterInspNoImgae =0;
        public string TipoInspecao { get; private set; } = "CORK";
        public string FicheiroConfiguracoes { get; private set; } = Application.StartupPath + @"\ivo_cam.ini";

        private int numMaximoPastaImagens = 50;

        public double numberAqThreads = 0;
        public double numberInspThreads = 0;

        private bool guardarImagemFimInspecao = true;
        private int timeoutToInspection = -1;
        private bool inspectiontimeout = false;

        private bool sistemaOff = false;
        public bool CameraAtiva
        {
            get { return this.EstadoVisao && !this.sistemaOff; }
        }

        private bool desenhaLinhas = true;
        private int numeroLinhas = 10;

        public int imageW = 4912;
        public int imageH = 3684;

        //Metodo Gravação Imagens
        private bool saveImage = true;
        private bool saveImgByHalcon = true;
        public bool GravacaoEmCurso
        {
            get; private set;
        }

        //Dados última inspeção
        public bool InspecaoAtiva { get; set; } = false;

        public bool NovoAnelProcessado = false;

        private bool jitEnabled = false;

        private double comprimentoFaca = 0;

        private double amplitudeDesvioFacaSuperior = 0;
        private double amplitudeDesvioFacaInferior = 0;

        private double posDesvioMaxFacaSuperior = 0;
        private double posDesvioMaxFacaInferior = 0;

        public ParamExpelConfig ConfigConvoyer = new ParamExpelConfig();

        private double alturaFacaCamera = 1.10;

        public bool FacaDetetada { get; private set; } = false;

        private DiscCom LastInspectedDisc = new DiscCom();

        //Estrutura com todos os dados da ultima inspecao a ser enviado ao visual
        public struct HmiReadData
        {
            public Receitas.ParamEncascado ParamEncascado;
            public DiscCom LastInspectedDisc;
            public HObject Image ;
            public HObject Regions ;
        }
        
        ConcurrentQueue<DiscMenbers> ListaDiscosInspecionados = new ConcurrentQueue<DiscMenbers>();
        ConcurrentQueue<HObject> ListaImagensPorInspecionar = new ConcurrentQueue<HObject>();
        ConcurrentQueue<StructImagens> ListaImagensPorGravar = new ConcurrentQueue<StructImagens>();
        ConcurrentQueue<HmiReadData> ListaHmiResultsLastInsp = new ConcurrentQueue<HmiReadData>();

        public class StructImagens
        {
            public HObject Image = new HObject();
            public HObject Regions = new HObject();
            public String FilePath = "";
            public DiscCom Part = new DiscCom();

        }
        /// <summary>
        /// 0 - SEM RESULTADO / 1 - OK / 2 - OK INV (REWORK) / 3 - NOK
        /// </summary>
        public int ResultadoInspecao
        {
            get; private set;
        }



        // : * pode ser defenido como qualquer tipo de dados
        public enum ResultEnum : int
        {
            NaoProcessado = 0,//Peca nao processada
            Aproved = 1,//resultado aprovado
            Erro = 2,//erro a inspecionar a peca
            Reproved = 3,//Resultado reprovado sem ano encascado
            AnoEncascadoVincado = 4,//Ano encascado vincado detetado
            AnoEncascadoModerado = 5,//Ano Encascado moderado
            AnoEncascadoLeve = 6,//Ano encascado Leve
        }

        //parametros de configuracao da expulsao dos discos do sistema apos inspecoes
        public class ParamExpelConfig
        {
            public int ExpelAprovedPulse = new int(), ExpelAprovedTime = new int();
            public int ExpelReprovedPulse = new int(), ExpelReprovedTime = new int();
        }

        /// <summary>
        /// 0-> OFF / 1-> ALWAYS OK / 2-> ALWAYS NOT OK
        /// </summary>
        public int ResultadosForcados = 0;
        public bool InspecoesSimuladas { get; set; } = false;

        public DateTime TempoInicioInspecao { get; private set; } = DateTime.MinValue;
        public DateTime TempoTotalInspecao = new DateTime();
        public int TempoAquisicao
        {
            get; private set;
        }
        public int TempoInspecao
        {
            get; private set;
        }
        public bool InspecaoEmCurso
        {
            get; private set;
        }
        public bool AcquisicaoEmCurso
        {
            get; private set;
        }
        public bool ErroInspecao
        {
            get; private set;
        }
        public string MensagemErro
        { get; private set; }

        private string imageFullPath;

        //Cameras
        private string serialNumber = "1";

        private bool restartCameraEveryTrigger = false;

        //Contadores
        private uint contadorInspecoes = 0;

        //Displays
        private CamObject CamLive { get; set; }

        //private CamObject CamInsp { get; set; }

        private HWindow DisplayLive { get; set; }

        //private HWindow DisplayInsp { get; set; }

        public HObject LastInspectionImage
        {
            get
            {
                try
                {
                    return this.CamLive.HWindow.DumpWindowImage();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Property 'LastInspectionImage': " + ex.Message);
                    return null;
                }
            }
        }

        private HObject outputImage { get; set; } = new HObject();

        public AuxiliarHalconImageClass UltimaImagemInspecao { get; set; } = new AuxiliarHalconImageClass();

        public string LastInspectionMsgErro { get; private set; } = string.Empty;

        public bool MostraRegiaoPesquisa { get; set; } = true;

        public Point regiaoPesquisaP1 = new Point(100, 1513);
        public Point regiaoPesquisaP2 = new Point(4900, 2125);

        private HObject imagemAtual = new HObject();
        private HObject imagemInspecao = new HObject();
        private HTuple acqHandle = new HTuple();

        private HDevEngine hEngine = null;
        private HDevProcedureCall inspScript = null;
        private bool loadedScript = false;

        private string scriptName = "InspLateralDiscos";

        public bool EstadoVisao { get; private set; } = false;


        private bool externalTrigger = false;

        //logs
        private LogDataService LogNormal;

        private string PathHistoricoInspecoes = @"C:\STREAK\Machine Data\Logs\Inspection.csv";

        private int NumeroHistoricoInspecoes = 100;

        private bool LogHistoricoInspecoesEnabled = true;

        private static readonly Object lockLista = new Object();

        public InspLateralDiscos(string ficheiroINI)
        {
            this.FicheiroIni = ficheiroINI;

            Forms.MainForm.PlcControl.CmdCiclo.Vars.VisonReady = this.CarregarParametrosInspecao();

            new Thread(this.AcquisitionCycle).Start();
            new Thread(this.processaimagens).Start();
            new Thread(this.EnviaResultadosPLC).Start();
           // new Thread(this.GravaImagem).Start();

        }

        private void EnviaResultadosPLC()
        {
            DateTime TimeBetwenDisc = new DateTime();
            bool Measure1 = true;
            DiscMenbers SendPlcDiscInfo;
            DiscCom NewRcvDiscPlc = new DiscCom(), LastSendDiscPlc = new DiscCom();
            int PlcSleepWriteCom = 0;
            NewRcvDiscPlc = Forms.MainForm.PlcControl.PlcHmiFeedbackdisc;
            LastSendDiscPlc = Forms.MainForm.PlcControl.HmiPlcNewDisc;

            while (this.isAlive)
            {
                if ((NewRcvDiscPlc.menber.ID == LastSendDiscPlc.menber.ID))
                {
                    if (Measure1 == true)
                    {
                        //Debug.WriteLine("##Tempo Entre discos: " + (DateTime.Now - TimeBetwenDisc).TotalMilliseconds.ToString() );
                        Measure1 = false;
                    }
                    if (ListaDiscosInspecionados.TryDequeue(out SendPlcDiscInfo))
                    {
                        //Envia resultados Plc apos verificar a lista
                        SendPlcDiscInfo.SaidaS2 = DateTime.Now;
                        Forms.MainForm.PlcControl.HmiPlcNewDisc.writeClassValues(SendPlcDiscInfo);
                        LastSendDiscPlc.writeClassValues(SendPlcDiscInfo);
                        //Debug.WriteLine("### EnviaResultadosPLC - Enviou Resultado com ID: " + SendPlcDiscInfo.ID);
                        Measure1 = true;
                        TimeBetwenDisc = DateTime.Now;
                    }
                }
                //Thread.Sleep(PlcSleepWriteCom);

            }


        }

        private bool CarregarParametrosInspecao()
        {
            try
            {
                #region Carregar definições do ficheiro INI
                using (FicheiroINI ini = new FicheiroINI(this.FicheiroIni))
                {
                    this.numMaximoPastaImagens = Convert.ToInt32(ini.RetornaINI("Visão", "numMaximoPastaImagens", Convert.ToString(this.numMaximoPastaImagens)));
                    this.diretorioImagens = ini.RetornaINI("Visão", "diretorioImagens", Convert.ToString(this.diretorioImagens));
                    this.scriptDirPath = ini.RetornaINI("Visão", "scriptDirPath", Convert.ToString(this.scriptDirPath));
                    this.scriptName = ini.RetornaINI("Visão", "scriptName", Convert.ToString(this.scriptName));
                    this.serialNumber = ini.RetornaINI("Visão", "serialNumber", "0");

                    this.TipoInspecao = ini.RetornaINI("Visão", "tipoInspecao", Convert.ToString(this.TipoInspecao));

                    this.imageW = Convert.ToInt32(ini.RetornaINI("Visão", "imageW", Convert.ToString(this.imageW)));
                    this.imageH = Convert.ToInt32(ini.RetornaINI("Visão", "imageH", Convert.ToString(this.imageH)));

                    this.FicheiroConfiguracoes = ini.RetornaINI("Visão", "FicheiroConfiguracoes", Convert.ToString(this.FicheiroConfiguracoes));


                    jitEnabled = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "jitEnabled", jitEnabled);
                    this.restartCameraEveryTrigger = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "restartCameraEveryTrigger", this.restartCameraEveryTrigger);

                    this.desenhaLinhas = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "desenhaLinhas", desenhaLinhas);
                    this.numeroLinhas = Convert.ToInt32(ini.RetornaINI("Visão", "numeroLinhas", Convert.ToString(this.numeroLinhas)));

                    this.saveImage = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "saveImage", saveImage);
                    this.saveImgByHalcon = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "saveImgByHalcon", saveImgByHalcon);

                    this.timeoutToInspection = Convert.ToInt32(ini.RetornaINI("Visão", "spTimeoutInspecao", Convert.ToString(this.timeoutToInspection)));

                    this.InspecoesSimuladas = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "InspecoesSimuladas", this.InspecoesSimuladas);

                    this.alturaFacaCamera = Convert.ToDouble(ini.RetornaINI("Visão", "AlturaFacaCamera", Convert.ToString(this.alturaFacaCamera)));


                    //Região de Pesquisa
                    this.MostraRegiaoPesquisa = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "MostraRegiaoPesquisa", this.MostraRegiaoPesquisa);

                    int auxX = 0;
                    int auxY = 0;

                    auxX = Convert.ToInt32(ini.RetornaINI("Visão", "RegiaoPesquisaP1X", this.regiaoPesquisaP1.X.ToString()));
                    auxY = Convert.ToInt32(ini.RetornaINI("Visão", "RegiaoPesquisaP1Y", this.regiaoPesquisaP1.Y.ToString()));

                    this.regiaoPesquisaP1 = new Point(auxX, auxY);

                    auxX = Convert.ToInt32(ini.RetornaINI("Visão", "RegiaoPesquisaP2X", this.regiaoPesquisaP2.X.ToString()));
                    auxY = Convert.ToInt32(ini.RetornaINI("Visão", "RegiaoPesquisaP2Y", this.regiaoPesquisaP2.Y.ToString()));

                    this.regiaoPesquisaP2 = new Point(auxX, auxY);


                    //log
                    this.LogHistoricoInspecoesEnabled = ini.RetornaTrueFalseDeStringFicheiroINI("Logs", "LogHistoricoInspecoesEnabled", true);
                    this.PathHistoricoInspecoes = ini.RetornaINI("Logs", "PathHistoricoInspecoes", this.PathHistoricoInspecoes);
                    this.NumeroHistoricoInspecoes = Convert.ToInt32(ini.RetornaINI("Logs", "NumeroHistoricoInspecoes", "100"));

                    //guardar imagem final inspecao
                    this.guardarImagemFimInspecao = ini.RetornaTrueFalseDeStringFicheiroINI("Visão", "guardarImagemFimInspecao", this.guardarImagemFimInspecao);

                    ConfigConvoyer.ExpelAprovedPulse = Convert.ToInt32(ini.RetornaINI("SETUP", "NumeroPulsosEncoderExpulsaoAprovados", ConfigConvoyer.ExpelAprovedPulse.ToString()));
                    ConfigConvoyer.ExpelReprovedPulse = Convert.ToInt32(ini.RetornaINI("SETUP", "NumeroPulsosEncoderExpulsaoReprovados", ConfigConvoyer.ExpelReprovedPulse.ToString()));
                    ConfigConvoyer.ExpelAprovedTime = Convert.ToInt32(ini.RetornaINI("SETUP", "TempoSoproAprovados", ConfigConvoyer.ExpelAprovedTime.ToString()));
                    ConfigConvoyer.ExpelReprovedTime = Convert.ToInt32(ini.RetornaINI("SETUP", "TempoSoproRejetados", ConfigConvoyer.ExpelReprovedTime.ToString()));

                    ini.EscreveFicheiroINI("SETUP", "NumeroPulsosEncoderExpulsaoAprovados", ConfigConvoyer.ExpelAprovedPulse.ToString());
                    ini.EscreveFicheiroINI("SETUP", "NumeroPulsosEncoderExpulsaoReprovados", ConfigConvoyer.ExpelReprovedPulse.ToString());
                    ini.EscreveFicheiroINI("SETUP", "TempoSoproAprovados", ConfigConvoyer.ExpelAprovedTime.ToString());
                    ini.EscreveFicheiroINI("SETUP", "TempoSoproRejetados", ConfigConvoyer.ExpelReprovedTime.ToString());
                    //ini.EscreveFicheiroINI();
                }


                if (this.LogHistoricoInspecoesEnabled)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(PathHistoricoInspecoes)))  //Caso o diretório do log do histórico de inspeções não exista cria-o
                        Directory.CreateDirectory(Path.GetDirectoryName(PathHistoricoInspecoes));

                    this.LogNormal = new LogDataService("Data S1" + ";" + "Data ini Inspecao" + ";" + "Data final Inspecao" + ";" + "Tempo Inspecao" + ";" + "ID" + ";" + "Resultado", PathHistoricoInspecoes, NumeroHistoricoInspecoes);
                }

                #endregion

                //Iniciar o objecto display de imagem
                this.CamLive = new CamObject(true);
                this.DisplayLive = this.CamLive.Inicializar(this);

                //this.CamInsp = new CamObject(false);
                //this.DisplayInsp = this.CamInsp.Inicializar(this);

                Directory.CreateDirectory(this.scriptDirPath); //Criar o diretório dos scripts

                this.hEngine = new HDevEngine();

                this.hEngine.SetProcedurePath(this.scriptDirPath);
                this.hEngine.SetEngineAttribute("execute_procedures_jit_compiled", jitEnabled ? "true" : "false");

                //inicia o script do halcon, se nao iniciado
                this.InitializeHalconScript();


                //if (!File.Exists(this.FicheiroConfiguracoes))
                //    throw new Exception("O ficheiro de configurações da camera não foi encontrado no diretório '" + this.FicheiroConfiguracoes + "'");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CarregarParametrosInspecao(" + this.TipoInspecao + "): " + ex.Message);
                return false;
            }
        }

        public void EscreveParametrosConfigConvoyer()
        {

            try
            {

                using (FicheiroINI ini = new FicheiroINI(this.FicheiroIni)) {
                    ini.EscreveFicheiroINI("SETUP", "NumeroPulsosEncoderExpulsaoAprovados", ConfigConvoyer.ExpelAprovedPulse.ToString());
                    ini.EscreveFicheiroINI("SETUP", "NumeroPulsosEncoderExpulsaoReprovados", ConfigConvoyer.ExpelReprovedPulse.ToString());
                    ini.EscreveFicheiroINI("SETUP", "TempoSoproAprovados", ConfigConvoyer.ExpelAprovedTime.ToString());
                    ini.EscreveFicheiroINI("SETUP", "TempoSoproRejetados", ConfigConvoyer.ExpelReprovedTime.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro na escritas de parametros ficheiro INI" + ex.Message);

            }
        }

        private void InitializeHalconScript()
        {
            if (!this.loadedScript)
                try
                {

                    int iLoadCount = 0;

                    Debug.WriteLine("Inspection[" + this.TipoInspecao + "]: Loading script '" + this.scriptName + "'");

                    //Scripts HDevEngine
                    this.inspScript = new HDevProcedureCall(new HDevProcedure(this.scriptName));

                    while (!this.inspScript.IsInitialized() && iLoadCount < 10)
                    {
                        iLoadCount++;
                        Thread.Sleep(100);
                    }

                    this.loadedScript = this.inspScript.IsInitialized();

                    if (this.loadedScript)
                        Debug.WriteLine("Inspection[" + this.TipoInspecao + "]: Script '" + this.scriptName + "' LOADED!!");
                    else
                        throw new Exception("** ERROR LOADING!! **");


                }
                catch (Exception ex)
                {
                    Debug.WriteLine("InitializeHalconScript(" + this.TipoInspecao + ") ERROR: " + ex.Message);
                    this.DisposeHalconScript();
                }
                finally
                {
                    GC.Collect();
                }
        }

        private void DisposeHalconScript()
        {
            if (this.loadedScript)
            {
                if (this.inspScript != null)
                {
                    // this.InspScript.GetProcedure().Dispose();
                    this.inspScript.Dispose();
                    this.inspScript = null;
                }

                this.loadedScript = false;

                for (int i = 0; i < 50; i++) //faz uma espera de 500ms
                {
                    Thread.Sleep(10);
                }

                GC.Collect();
            }
        }

        private bool IniciaCamera()
        {
            DateTime tempoInicio = DateTime.Now;
            bool adquireFirstImage = false;

            if (string.IsNullOrWhiteSpace(this.serialNumber) || this.serialNumber == "0")
                return false;

            //inserir parametros iniciais de configuração da câmara
            Debug.WriteLine(this.TipoInspecao + " - Arranque da câmara com o s/n '" + this.serialNumber + "'");

            try
            {
                HOperatorSet.OpenFramegrabber("GenICamTL", 0, 0, 0, 0, 0, 0, "progressive", -1, "default", -1, "false", "default", this.serialNumber, 0, -1, out acqHandle);

                HTuple Value = new HTuple();
                HOperatorSet.GetFramegrabberParam(acqHandle, "grab_timeout", out Value);
                HOperatorSet.SetFramegrabberParam(acqHandle, "grab_timeout", -1);
                HOperatorSet.GetFramegrabberParam(acqHandle, "grab_timeout", out Value);

                HOperatorSet.GrabImageStart(this.acqHandle, -1);

                if (adquireFirstImage)
                #region Adquire uma 1ª imagem após conectar à camera
                {
                    HObject tempImg = new HObject();
                    tempImg.GenEmptyObj();

                    HOperatorSet.GrabImage(out tempImg, this.acqHandle);

                    tempImg.Dispose();
                    tempImg = null;
                }
                #endregion


                this.EstadoVisao = true;

                if (this.EstadoVisao)
                    this.sistemaOff = false;

                Debug.WriteLine(this.TipoInspecao + " - Arranque da câmara OK");
                Debug.WriteLine("*** Camera ligada (" + this.TipoInspecao + ") ***");

            }
            catch (Exception ex)
            {
                Debug.WriteLine("IniciaCamera(" + this.TipoInspecao + "): " + ex.Message);
                this.EstadoVisao = false;
            }
            finally
            {
                Debug.WriteLine("IniciaCamera " + this.TipoInspecao + " - Tempo decorrido: " + Convert.ToInt32((DateTime.Now - tempoInicio).TotalMilliseconds) + " ms");
            }
            return this.EstadoVisao;
        }

        public bool DesligaCamera()
        {
            DateTime tempoInicio = DateTime.Now;

            if (this.EstadoVisao)
                try
                {
                    HOperatorSet.SetFramegrabberParam(acqHandle, "do_abort_grab", "true");
                    HOperatorSet.CloseFramegrabber(this.acqHandle);
                    this.acqHandle = null;
                    Debug.WriteLine("*** Camera desligada " + this.TipoInspecao + " ***");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("DesligaCamera(" + this.TipoInspecao + "): " + ex.Message);
                }
                finally
                {
                    Debug.WriteLine("DesligaCamera " + this.TipoInspecao + " - Tempo decorrido: " + Convert.ToInt32((DateTime.Now - tempoInicio).TotalMilliseconds) + " ms");
                }

            return this.EstadoVisao = false;
        }

        int NumeroThreads = 0;
        private void AcquisitionCycle()
        {
            bool firstCycle = true;
            this.numberAqThreads = 0;
            this.numberInspThreads = 0;
            HObject ClearList;

            int numeroAquisicoes = 1;

            while (ListaImagensPorInspecionar.TryDequeue(out ClearList)) { };

            while (this.isAlive)
                try
                {
                    //Verifica o estado da camera, caso esteja desligada tenta liga-la automaticamente
                    if (!this.EstadoVisao && !this.sistemaOff)// (!this.EstadoVisao && !firstCycle && !this.sistemaOff)
                        Forms.MainForm.PlcControl.CmdCiclo.Vars.CameraReady = this.IniciaCamera();


                    if (this.EstadoVisao && !this.sistemaOff) //acquisição contínua da imagem
                        try
                        {
                            HObject tempImg = new HObject();
                            tempImg.GenEmptyObj();

                            //Adquire a imagem
                            HOperatorSet.GrabImageAsync(out tempImg, this.acqHandle, -1);

                            this.CamLive.DisplayImage(tempImg.Clone(), numeroAquisicoes);

                            Debug.WriteLine("Adquiriu a imagem: " + numeroAquisicoes);

                            ListaImagensPorInspecionar.Enqueue(tempImg.Clone());

                            numeroAquisicoes++;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("AcquisicaoImagem - CicloTrabalho(" + this.TipoInspecao + ") - DB50.MODO_VIDEO: " + ex.Message);

                            //em caso de falha desliga a camera
                            //this.DesligaCamera();
                        }

                    if (this.EstadoVisao && this.sistemaOff) //caso tenha ordem para encerrar a camera!
                        this.DesligaCamera();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AcquisicaoImagem(): " + ex.Message);
                }
                finally
                {
                    firstCycle = false;

                    //Aguarda 10ms entre interações, caso a camera esteja conectada caso contrário faz interações de 500ms
                    Thread.Sleep((this.EstadoVisao) ? 10 : 500);
                }

            //liberta recursos
            try
            {
                //Caso a camera esteja ligada vai desliga-la para encerrar a thread
                this.DesligaCamera();

                this.sistemaOff = true;

                //desliga os scripts
                this.DisposeHalconScript();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AcquisicaoImagem(DISPOSE): " + ex.Message);
            }
        }

        private void processaimagens()
        {
            HObject AuxImage = new HObject();
            DateTime Start1 = new DateTime(), TimeOutAq = new DateTime(), TimeOutNewID = new DateTime();
            DiscCom _DiscId = new DiscCom();
            Thread[] InspectionThread = new Thread[10]; 
            bool PeekElement = false;// flag para verificar que temos uma imagem adquirida sem o PLC enviar novo ID
            int TimeOutSp = 400;// tempo maximo para rececao do ID caso contrario limpa a imagem
            int SpTimeOutNewID = 400;
            bool Measure1 = true;
            //Limpeza para o PLC
            this.LastInspectedDisc.writeClassValues(Forms.MainForm.PlcControl.PlcHmiNewDisc.menber);
            Forms.MainForm.PlcControl.HmiPlcNewDisc.writeClassValues(Forms.MainForm.PlcControl.PlcHmiNewDisc.menber);

            while (this.isAlive)
            {

                if (Forms.MainForm.PlcControl.HmiPlcFeedbackdisc.menber.ID != Forms.MainForm.PlcControl.PlcHmiNewDisc.menber.ID)
                {
                    

                    if (Measure1 == true)
                    {
                        Debug.WriteLine("Vai inspecionar imagem: " + Forms.MainForm.PlcControl.PlcHmiNewDisc.menber.ID);
                        //Debug.WriteLine("Tempo Entre Id Entrada PLC: " + (DateTime.Now-Start1).TotalMilliseconds.ToString());
                        TimeOutNewID = DateTime.Now;
                        Measure1 = false;
                    }
                    if (ListaImagensPorInspecionar.TryDequeue(out AuxImage))
                    {
                        if (AuxImage != null) {
                            
                            //Incrementa numero de treads ativas
                            this.numberInspThreads++;
                            NumeroThreads++;
                            Debug.WriteLine("cahama script inspecionar imagem: " + Forms.MainForm.PlcControl.PlcHmiNewDisc.menber.ID);
                            this.InspecaoEmCurso = true;
                            //new Thread(this.CallInspection).Start(); //inicia uma instância da inspeção

                            //Debug.WriteLine("InspNumber antes da Thread: " + this.numberInspThreads);
                            int Auxnumber = (int)this.numberInspThreads;
                            //Leitura variavel PLC
                            DiscCom DiscId = new DiscCom();

                            DiscId.writeClassValues(Forms.MainForm.PlcControl.PlcHmiNewDisc.menber);
                            Forms.MainForm.PlcControl.CmdCiclo.Vars.DateToPlc = DateTime.Now;
                            Forms.MainForm.PlcControl.HmiPlcFeedbackdisc.writeClassValues(DiscId.menber);
                            Debug.WriteLine("#Start ID: " + DiscId.menber.ID + "Tempo Ate Ter Imagem" + (DateTime.Now - TimeOutNewID).TotalMilliseconds );

                            this.LastInspectedDisc.writeClassValues(DiscId.menber);

                            NumeroThreads = 0;
                           // new Thread(() => CallInspection1(AuxImage.Clone(), Auxnumber, (int)this.numberInspThreads, DiscId)).Start();
                            DiscId.menber.InicioInspecao = DateTime.Now;

                            PeekElement = false;
                            for (int i = 0; i < 10; i++)
                            {
                                if (InspectionThread[i]!=null)
                                {
                                   if ( InspectionThread[i].IsAlive )
                                    NumeroThreads++;
                                
                                }
                            }

                            InspectionThread[NumeroThreads] = new Thread(delegate ()
                             {
                                CallInspection1(AuxImage.Clone(), Auxnumber, (int)this.numberInspThreads, DiscId);
                            }
                            );
                            InspectionThread[NumeroThreads].Start();   
                            
                           

                                Measure1 = true;

                        }
                    }

                    //PlcEnviou ID mas nenhuma imagem foi adquirida
                    if ((DateTime.Now - TimeOutNewID).TotalMilliseconds> SpTimeOutNewID)
                    {
                        DiscCom  ErrorDisc = new DiscCom(),DiscId = new DiscCom();

                        DiscId.writeClassValues(Forms.MainForm.PlcControl.PlcHmiNewDisc.menber);
                        Forms.MainForm.PlcControl.CmdCiclo.Vars.DateToPlc = DateTime.Now;
                        Forms.MainForm.PlcControl.HmiPlcFeedbackdisc.writeClassValues(DiscId.menber);
                        this.LastInspectedDisc.writeClassValues(DiscId.menber);
                        ErrorDisc.writeClassValues(DiscId.menber);
                        
                        //Disco de erro sem imagem
                        ErrorDisc.menber.FinalInspecao = DateTime.Now;
                        ErrorDisc.menber.result = 3;
                        ListaDiscosInspecionados.Enqueue(ErrorDisc.menber);
                        //lock (this)
                        CounterInspNoImgae++;
                    }
                    //Thread.Sleep(0);
                    Start1 = DateTime.Now;
                }

                HObject AuxImage2 = new HObject();
                if (ListaImagensPorInspecionar.TryPeek(out AuxImage2) && !PeekElement)
                {
                    TimeOutAq = DateTime.Now;
                    PeekElement = true;
                }

                if (((DateTime.Now - TimeOutAq).TotalMilliseconds > TimeOutSp) && PeekElement && ListaImagensPorInspecionar.TryPeek(out AuxImage2))
                {
                    if (ListaImagensPorInspecionar.TryDequeue(out AuxImage)) {

                        _DiscId.writeClassValues(Forms.MainForm.PlcControl.PlcHmiNewDisc.menber);
                        _DiscId.menber.result = 666;
                        _DiscId.menber.FinalInspecao = DateTime.Now;
                        _DiscId.menber.InicioInspecao = TimeOutAq;
                        PeekElement = false;
                        string filename = "ID_" + _DiscId.menber.ID + "_Result_" + _DiscId.menber.result + "_" + TimeOutAq.ToString("dd-MM-yyyy") + "_" + DateTime.Now.ToString("HH_mm_ss_ms");
                        StructImagens NewElement = new StructImagens();
                        NewElement.Image = AuxImage2.Clone();
                        NewElement.FilePath = filename;
                        NewElement.Part.writeClassValues(_DiscId.menber);
                        ListaImagensPorGravar.Enqueue(NewElement);
                        //lock (this)
                        Debug.WriteLine("Falhou Task com o ID: " + NewElement.Part.menber.ID + "Tempo Ate Trigger" + (DateTime.Now - TimeOutAq).TotalMilliseconds);
                        CounterInspNoTrigger++;
                    }
                }

                if (PeekElement && !ListaImagensPorInspecionar.TryPeek(out AuxImage2))
                {
                    PeekElement = false;
                }
            }

        }

        private void CallInspection1(HObject ImagemInInspecao, int InspNumber, int TotalInspNumber, DiscCom DiscId)
        {
            DiscCom _DiscId = new DiscCom();
            HObject AuxTest = new HObject();
            int _InspNumber = InspNumber;
            int _ResultadoInspecao = new int();
            HObject ImagemActInspecao = new HObject();

            DateTime startTime = DateTime.Now, AuxDT;

            HmiReadData ReturnInspInfo = new HmiReadData();
            ReturnInspInfo.LastInspectedDisc = new DiscCom();
            ReturnInspInfo.Regions = new HObject();
            ReturnInspInfo.Image = new HObject();
            ReturnInspInfo.ParamEncascado = new Receitas.ParamEncascado();

            Thread SaveImagesFile=new Thread(GravaImagem); 

            _DiscId.writeClassValues(DiscId.menber);
            //lock (this.inspLock)
            try
            {


                this.InspecaoEmCurso = true;

                this.ProcessaNovaOrdemInspecao();

                //******* AQUISIÇÃO DA IMAGEM *******
                this.AcquisicaoEmCurso = true;

                // lock (this.aqcLock)
                // { //copia a imagem atual para a imagem de inspecao
                HOperatorSet.CopyImage(ImagemInInspecao, out ImagemActInspecao);  //HOperatorSet.CopyImage(this.imagemAtual, out this.imagemInspecao);
                HOperatorSet.CopyImage(ImagemInInspecao, out AuxTest);  //HOperatorSet.CopyImage(this.imagemAtual, out this.imagemInspecao);
                HOperatorSet.CopyImage(ImagemInInspecao, out this.imagemInspecao);
                HOperatorSet.CopyImage(ImagemInInspecao, out ReturnInspInfo.Image);
                
                // }

                //limpa a janela
                //this.CamInsp.HWindow.ClearWindow();

                //Mostra a imagem inspeção
                //this.CamInsp.DisplayImage(HalconFunctions.HobjectToHimage(ImagemActInspecao.Clone()), 0, DateTime.MinValue);

                //enviar o captura ok
                //Debug.WriteLine("*** Imagem Elapsed Time " + this.TipoInspecao + " " + this.TipoInspecao + ": " + Convert.ToInt32((DateTime.Now - this.TempoInicioInspecao).TotalMilliseconds) + " ms ***");

                //Enviar o sinal de captura efetuada
                if (true) //DB55.DBX10.4 - DONE CAPTURE Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, true, this.DbNumber, 10, 4)
                {
                    this.TempoAquisicao = Convert.ToInt32((DateTime.Now - this.TempoInicioInspecao).TotalMilliseconds);
                    this.capturaOK = true;
                    this.AcquisicaoEmCurso = false;
                    //Debug.WriteLine("*** Sinal de Captura OK " + this.TipoInspecao + " enviado ***");
                }

                //******* INSPEÇÃO DA IMAGEM *******
                if (true)// if (this.InspecaoEmCurso && this.capturaOK)
                {
                    #region Gravar a imagem ** Se tiver ordem para gravar a imagem antes da inspeção **
                    //Se tiver ordem para gravar a imagem antes da inspeção
                    try
                    {
                        if (false)//if (this.saveImage && !this.guardarImagemFimInspecao)
                        {
                            //Cria o caminho geral do diretório das imagens
                            //string filename = this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH-mm-ss") + "_" + this.TipoInspecao + ".bmp";

                            string filename = DB400.INSPECTION_RESULT.ID.ToString() + "_" + this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH-mm-ss") + "_" + this.TipoInspecao + ".bmp";

                            //Juntas o diretório num só
                            this.imageFullPath = this.diretorioImagens + filename;

                            //Em thread, grava a imagem adquirida, de acordo com o metodo pretendido
                            //if (this.saveImgByHalcon)
                            //    new Thread(() => this.GravaImagem1(ImagemActInspecao, this.imageFullPath)).Start();
                            //else
                            //    new Thread(() => this.GravaImagem2(ImagemActInspecao, this.imageFullPath)).Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("CallInspection() - Gravar a imagem: " + ex.Message);
                    }

                    #endregion

                    //prepara outputs da função da inspeção
                    bool erroInspecao = false;
                    string msgInspecao = string.Empty;
                    
                    //Chamar o módulo de inspeção
                    //Receitas.ParamEncascado  = new Receitas.ParamEncascado();
                    this.CallInspectionScript(ImagemActInspecao, out _ResultadoInspecao, out erroInspecao, out msgInspecao, out ReturnInspInfo.ParamEncascado, out ReturnInspInfo.Regions);
                    this.ResultadoInspecao = _ResultadoInspecao;

                    //Debug.WriteLine("# InspNumber: " + _InspNumber);
                    
                    _DiscId.menber.FinalInspecao = DateTime.Now;
                    _DiscId.menber.result = (short)_ResultadoInspecao;

                    if (_ResultadoInspecao != (short)ResultEnum.Aproved) {
                        _DiscId.menber.DelayExpel = ConfigConvoyer.ExpelReprovedTime;
                        _DiscId.menber.PulseExpel = ConfigConvoyer.ExpelReprovedPulse;
                    }
                    else
                    {
                        _DiscId.menber.DelayExpel = ConfigConvoyer.ExpelAprovedTime;
                        _DiscId.menber.PulseExpel = ConfigConvoyer.ExpelAprovedPulse;
                    }
                    
                    //ReturnInspInfo.LastInspectedDisc.writeClassValues(_DiscId.menber);
                    //lista toda a informacao de um disco
                    //ListaHmiResultsLastInsp.Enqueue(ReturnInspInfo);
                    
                    ListaDiscosInspecionados.Enqueue(_DiscId.menber);
                    Debug.WriteLine("### Terminou Inspeção com ID: " + _DiscId.menber.ID + " ### Numero Threads: " + NumeroThreads + " ### Tempo PLC: " + (DateTime.Now - startTime).TotalMilliseconds.ToString("F1") + "ms" + " ### Resultado: " + _DiscId.menber.result);

                    ////Processar os dados obtidos da visão
                    //if (this.ResultadosForcados == 0)
                    //    this.ProcessaDadosInspecao(facaDetetada, comprimentoFaca, amplitudeDesvioSuperior, posMaxDesvioSuperior, amplitudeDesvioInferior, posMaxDesvioInferior, erroInspecao, msgInspecao);
                    //else
                    //    this.ResultadoInspecao = this.ResultadosForcados == 1 ? 1 : 3; //força OK / NOK consoante o valor 'ResultadosForcados'

                    //mostra o resultado na imagem da inspeção
                    //this.CamInsp.UpdateStatus(this.ErroInspecao ? LabelStatus.ErroInspecao : (LabelStatus)_ResultadoInspecao);

                    //Enviar os resultados no PLC - Processa o fim da inspeção
                    Forms.MainForm.VariaveisAuxiliares.AuxOrdInsertData = true;

                    DB400.INSPECTION_RESULT.ID = (uint)_DiscId.menber.ID;
                    DB400.INSPECTION_RESULT.INSPECTION_RESULT = (byte)_ResultadoInspecao;
                    DB400.INSPECTION_RESULT.DT_INSPECTION = _DiscId.menber.EntradaS1;

                    if (_ResultadoInspecao == (short)ResultEnum.Aproved)
                    {
                        DB400.INSPECTION_RESULT.APROVED = true;
                    }
                    else
                    {
                        DB400.INSPECTION_RESULT.APROVED = false;
                    }

                    #region Gravar a imagem ** Se tiver ordem para gravar a imagem apos A inspeção **
                    //Se tiver ordem para gravar a imagem apos a inspeção
                    if (this.saveImage && this.guardarImagemFimInspecao)
                    {
                        //Cria o caminho geral do diretório das imagens
                        string filename = "ID_" + _DiscId.menber.ID.ToString() + "_Result_" + _ResultadoInspecao.ToString() + "_" + this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH_mm_ss_ms");

                        //Juntas o diretório num só
                        this.imageFullPath = this.diretorioImagens + filename;
                        filename = this.diretorioImagens + filename;

                        //Envio de estrutura para a lista
                        StructImagens NewElement = new StructImagens();
                        NewElement.Image = ImagemActInspecao.Clone();
                        NewElement.FilePath = filename;
                        NewElement.Part.writeClassValues(_DiscId.menber);
                        ListaImagensPorGravar.Enqueue(NewElement);

                        //Thread t = new Thread(() => GravaImagem());
                        //t.Start();
                        

                        if (!SaveImagesFile.IsAlive)
                        SaveImagesFile.Start();
                    }
                    #endregion
                    NumeroThreads--;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CallInspection(): " + ex.Message);
            }
            finally
            {
                this.inspecaoOK = true;

                this.AcquisicaoEmCurso = this.InspecaoEmCurso = false;

                //Dá a indicação ao HMI que temos um novo anel processado
                this.NovoAnelProcessado = true;

                //Debug.WriteLine();

                GC.Collect(); //chama o coletor de memoria apos cada inspecao
            }
        }

        public void SetZoomControl(PictureBox _picBoxDisplay)
        {
            if (_picBoxDisplay != null)
            {
                this.CamLive.ResetSizeImag(this.imageW, this.imageH);
                _picBoxDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
                _picBoxDisplay.Controls.Add(this.CamLive);
                this.CamLive.Dock = DockStyle.Fill;
            }
        }

        public void FreeSetZoomControl(System.Windows.Forms.PictureBox _picBoxDisplay)
        {
            if (_picBoxDisplay != null && _picBoxDisplay.Controls.Contains(this.CamLive))
                _picBoxDisplay.Controls.Remove(this.CamLive);
        }

        public void AtualizaEstadoCameraPLC(short valor)
        {
            //Estado Comando Folga
            //Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Int, valor, this.DbNumber, 6);
        }

        private bool ProcessaNovaOrdemInspecao()
        {
            this.TempoInicioInspecao = DateTime.Now;
            this.contadorInspecoes++;
            this.capturaOK = false;
            this.inspecaoOK = false;
            this.TempoInspecao = 0;
            this.ResultadoInspecao = 0;
            this.ErroInspecao = false;
            this.NovoAnelProcessado = false;

            this.MensagemErro = string.Empty;
            this.FacaDetetada = false;
            this.comprimentoFaca = this.amplitudeDesvioFacaSuperior = this.amplitudeDesvioFacaInferior = this.posDesvioMaxFacaSuperior = this.posDesvioMaxFacaInferior = 0;
            this.inspectiontimeout = false;

            this.imageFullPath = string.Empty;

            return true;
        }
        //Convert num array os parametros da receita para enviar para o script
        private HTuple SetArrayParamEncascado(){
            HTuple parametros = new HTuple();
            int i = 0;

            //ENCASCADO_LEVE
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_LEVE.COMPRIMENTO;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_LEVE.ESPESSURA;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_LEVE.TONALIDADE_MAX;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_LEVE.TONALIDADE_MIN;
            i++;
            //ENCASCADO_MODERADO
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_MODERADO.COMPRIMENTO;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_MODERADO.ESPESSURA;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_MODERADO.TONALIDADE_MAX;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_MODERADO.TONALIDADE_MIN;
            i++;
            //ENCASCADO_VINCADO
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_VINCADO.COMPRIMENTO;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_VINCADO.ESPESSURA;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_VINCADO.TONALIDADE_MAX;
            i++;
            parametros[i] = Forms.MainForm.Receita._ReceitaCarregada.ENCASCADO_VINCADO.TONALIDADE_MIN;
            i++;

            return parametros;
        }
        //Le o array do script e coloca numa lista para poderem ser lidos 
        private Receitas.ParamEncascado GetArrayParamEncascado(HTuple LastResult)
        {
            Receitas.ParamEncascado Results = new Receitas.ParamEncascado();
            int i = 0;

            Results.COMPRIMENTO = LastResult[i];
            i++;
            Results.ESPESSURA = LastResult[i];
            i++;
            Results.TONALIDADE_MIN = LastResult[i];
            i++;
            Results.TONALIDADE_MAX = LastResult[i];

            return Results;
            //ListaUltimosParamEncascado.Enqueue(Results);
        }
        private void CallInspectionScript(HObject img, out int resultadoInsp, out bool erroInspecao, out string msgErro, out Receitas.ParamEncascado ParametersUsed, out HObject regions)
        {
            DateTime startTime = DateTime.Now;
            bool modoSimulacao = false;
            regions = new HObject();
            //instancia variaveis com default values
            ParametersUsed = new Receitas.ParamEncascado();
            resultadoInsp = 0;
            erroInspecao = false;
            msgErro = string.Empty;

            //modoSimulacao = true;

            try
            {
                if (!this.loadedScript)
                    throw new Exception("Script Loading Error!");

                if (!modoSimulacao)
                {

                    //Configura as inputs do script
                    this.inspScript.SetInputIconicParamObject("InputImage", img);
                    this.inspScript.SetInputCtrlParamTuple("ParamEncascado", SetArrayParamEncascado());

                    //Executa o script
                    if (this.timeoutToInspection <= 0)
                        this.inspScript.Execute();
                    else
                        this.RunMethodWithTimeout(() => this.inspScript.Execute(), this.timeoutToInspection);


                    //Obtem as outputs do script
                    outputImage = this.inspScript.GetOutputIconicParamObject("OutputImage");
                    regions = this.inspScript.GetOutputIconicParamObject("Region");
                    resultadoInsp = this.inspScript.GetOutputCtrlParamTuple("Resultado").I;
                    erroInspecao = this.inspScript.GetOutputCtrlParamTuple("Erro") == 1;
                    ParametersUsed = GetArrayParamEncascado(this.inspScript.GetOutputCtrlParamTuple("ParamSaida"));


                    //if (this.inspScript.GetOutputCtrlParamTuple("MsgErro").Length>1)
                    // msgErro = "Nao foi enviada informacao do script";
                    // else
                    //msgErro = this.inspScript.GetOutputCtrlParamTuple("MsgErro");

                    //msgErro = "";//o emanuel nao colocou menssagens em todos os caos :) 

                    //Se não tiver erro de inspeção, adiciona a imagem tratada do halcon e adiciona as regiões
                    if (true)
                    {
                        //limpa a janela
                        //this.CamInsp.HWindow.ClearWindow();

                        //carrega os resultados visuais da inspeção para uma estrutura de dados
                        this.UltimaImagemInspecao.UpdateDados(img, regions, outputImage);

                        //mostra imagem rodada vinda da inspeção
                        //this.CamInsp.DisplayImage(this.UltimaImagemInspecao.hImage, 0);

                        #region Mostra as Regiões na Imagem
                        if (false)//this.UltimaImagemInspecao.ValidRegions())
                        {
                            //mostra contornos
                            //this.DisplayInsp.SetLineWidth(2.5);

                            for (int i = 0; i < regions.CountObj(); i++)
                            {
                                //this.DisplayInsp.SetColor((string)this.UltimaImagemInspecao.colors[i]);
                                //this.DisplayInsp.DispObj(regions.SelectObj(i + 1));
                            }
                        }
                        #endregion
                    }
                    //else
                    //    throw new Exception(!string.IsNullOrWhiteSpace(msgErro) ? msgErro : "Erro de inspeção não especificado! [C#]");

                }
                #region Modo de Simulação
                else
                {
                    HOperatorSet.GenEmptyObj(out HObject emptyObject);
                    outputImage = emptyObject;

                    Thread.Sleep(25);
                    this.UltimaImagemInspecao.UpdateDados(img, regions, outputImage);
                    erroInspecao = false;
                    msgErro = erroInspecao ? "Erro Simulação!" : string.Empty;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Debug.Write("CallInspectionScript(" + this.TipoInspecao + "): " + ex.Message);

                //força o erro de inspeção
                erroInspecao = true;

                //Mensagem de erro
                msgErro = ex.Message;
                GC.Collect();
                //limpa a janela
                //this.CamInsp.HWindow.ClearWindow();
                //Volta a mostrar a imagem original com o timestamp original e a label de erro de ispecao
                //this.CamInsp.DisplayImage(HalconFunctions.HobjectToHimage(this.imagemInspecao), LabelStatus.ErroInspecao, this.TempoInicioInspecao);
            }
            finally
            {
                GC.Collect();
                //Debug.WriteLine("*** CallInspectionScript - Elapsed time: " + Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds) + " ms ***");
            }
        }

        public bool FazFifo()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(this.diretorioImagens);

                if (this.numMaximoPastaImagens >= d.GetFiles().Length)
                    return true;
                else
                    for (int i = 0; i < (d.GetFiles().Length - this.numMaximoPastaImagens); i++)
                        d.GetFiles().OrderBy(f => f.CreationTime).First().Delete();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERRO - Função Fifo: " + ex.Message);
                return false;
            }
            finally
            {
                GC.Collect();
            }
        }

        public bool SaveAtualImageAs()
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Gravar imagem capturada como... ";
                    sfd.Filter = "BMP(*.bmp)|*.bmp";

                    sfd.DefaultExt = ".bmp"; // Default file extension 

                    if (sfd.ShowDialog() == DialogResult.OK) { }
                    // lock (this.aqcLock)
                    // this.GravaImagem1(this.imagemAtual.Clone(), sfd.FileName);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveAtualImageAs(): " + ex.Message);
                return false;
            }

        }

        //HObject LastImageSaved = new HObject();

        private void GravaImagem()//(HObject hOImage, string imageFullPath)
        {
            HObject _hOImage = new HObject();
            string _imageFullPath;
            this.GravacaoEmCurso = true;
            DiscCom StPeca = new DiscCom();
            StructImagens NewElement = new StructImagens();

            while (ListaImagensPorGravar.TryDequeue(out NewElement))
            {
                try
                {



                    DateTime dataInicial = DateTime.Now;
                    _hOImage = NewElement.Image;
                    _imageFullPath = NewElement.FilePath;
                    //HOperatorSet.CopyImage(hOImage, out _hOImage);
                    // if (ListaImagensPorGravar.TryDequeue(out _hOImage))
                    HOperatorSet.WriteImage(_hOImage, "tiff", 0, _imageFullPath);
                    StPeca.writeClassValues(NewElement.Part.menber);



                    //Debug.WriteLine("GravaImagem1(" + this.TipoInspecao + "): Image saved sucessufully on " + imageFullPath + " ** Elapsed Time: " + Convert.ToInt32((DateTime.Now - dataInicial).TotalMilliseconds) + " ms");
                    //Debug.WriteLine("GravaImagem1(" + "): Image saved sucessufully on " + _imageFullPath + " ** Elapsed Time: " + Convert.ToInt32((DateTime.Now - dataInicial).TotalMilliseconds) + " ms");
                    LogNormal.DataToSend(StPeca.menber.InicioInspecao.ToString(@"dd\_MM\_yyyy\_HH\_mm\_ss\_fff") + ";" + StPeca.menber.FinalInspecao.ToString(@"dd\_MM\_yyyy\_HH\_mm\_ss\_fff") + ";" + (StPeca.menber.FinalInspecao - StPeca.menber.InicioInspecao).TotalMilliseconds.ToString() + ";" + StPeca.menber.ID.ToString() + ";" + StPeca.menber.result.ToString());
                    //Thread.Sleep(50);
                }
            
            catch (Exception e)
            {
                Debug.WriteLine("*** Exception Thread GravaImagem1 " + this.TipoInspecao + ": Error Saving Image - " + e.Message);
                //return false;
            }
            finally
            {
                // if (hOImage != null) hOImage.Dispose();

                //Faz o fifo de imagens
                this.FazFifo();

                this.GravacaoEmCurso = false;
                GC.Collect();
            }
        }
            }
       // }

        private bool GravaImagem2(HObject hOImage, string imageFullPath)
        {
            this.GravacaoEmCurso = true;

            DateTime dataInicial = DateTime.Now;

            ColorPalette cp_P;
            Image image = null;
            int iWidth, iHeight, i;
            string sType;
            IntPtr ipPtr;
            HImage hImage = new HImage();
            Bitmap bitmap = null;
            try
            {
                hImage.IntegerToObj(hOImage.ObjToInteger(1, -1));

                ipPtr = hImage.GetImagePointer1(out sType, out iWidth, out iHeight);
                bitmap = new Bitmap(iWidth, iHeight, iWidth, PixelFormat.Format8bppIndexed, ipPtr);

                cp_P = bitmap.Palette;

                for (i = 0; i < 256; i++)
                    cp_P.Entries[i] = Color.FromArgb(i, i, i);

                image = (Image)bitmap;
                image.Palette = cp_P;

                image.Save(imageFullPath, ImageFormat.Bmp);

                Debug.WriteLine("GravaImagem2(" + this.TipoInspecao + "): Image saved sucessufully on " + imageFullPath + " ** Elapsed Time: " + Convert.ToInt32((DateTime.Now - dataInicial).TotalMilliseconds) + " ms");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("*** Exception Thread GravaImagem2 " + this.TipoInspecao + ": Error Saving Image - " + e.Message);
                return false;
            }
            finally
            {
                if (hOImage != null) hOImage.Dispose();
                if (image != null) image.Dispose();
                if (hImage != null) hImage.Dispose();
                if (bitmap != null) bitmap.Dispose();

                //Faz o fifo de imagens
                this.FazFifo();

                this.GravacaoEmCurso = false;
            }
        }

        public void Dispose()
        {
            this.isAlive = false;

            DesligaCamera();

            if (this.LogNormal != null)
                this.LogNormal.Dispose();

            this.DisposeHalconScript();

            if (this.hEngine != null)
            {
                this.hEngine.Dispose();
                this.hEngine = null;
            }

            //guardar parametrizacoes
            using (FicheiroINI ini = new FicheiroINI(this.FicheiroIni))
            {
                ini.EscreveFicheiroINI("Visão", "MostraRegiaoPesquisa", this.MostraRegiaoPesquisa ? "1" : "0");
            }

        }

        private void RunMethodWithTimeout(Action _method, int timeout)
        {
            DateTime dt = DateTime.Now;

            try
            {
                Thread thrd = new Thread(new ThreadStart(_method));
                thrd.Start();

                while ((DateTime.Now - dt).TotalMilliseconds <= timeout && thrd.IsAlive && this.isAlive)
                    Thread.Sleep(10);

                if (thrd.IsAlive)
                {
                    thrd.Abort();
                    throw new Exception(_method.ToString() + " method timeout!");
                
                }
                else
                    Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RunMethodWithTimeout(" + this.TipoInspecao + "): " + ex.Message);
                GC.Collect();
                this.inspectiontimeout = true;
            }

        }

        private void AdicionaRegistoLog(DiscMenbers Part)
        {
            //if (this.LogHistoricoInspecoesEnabled)
            // if (LogNormal != null) //Significa que temos o log ativado
            ;// this.LogNormal.DataToSend(DateTime.Now.ToString(@"dd\_MM\_yyyy\_HH\_mm\_ss") + ";" + id.ToString() + ";" + (result ? "OK" : "NOK") + ";" + value1.ToString() + ";" + value2.ToString() + ";" + value3.ToString() + ";" + value4.ToString() + ";" + value5.ToString() + ";");
        }


        class LogDataService
        {
            private List<string> listaDeLogs = new List<string>();
            private string pathLog;
            private string cabecalho;
            private int numeroLog;
            private object thisLock = new object();
            private bool isAlive = true;

            public LogDataService(string _cabecalho, string _pathLog, int _numeroLog)
            {

                pathLog = _pathLog;
                cabecalho = _cabecalho;
                numeroLog = _numeroLog;
                new Thread(ThreadGetSendLog).Start();

            }

            public void DataToSend(string _data)
            {
                //enfiar para a lista 
                this.listaDeLogs.Add(_data);
            }

            private void ThreadGetSendLog()
            {
                while (this.isAlive)
                    try
                    {
                        if (!this.getSendLog())
                            throw new Exception("Erro ao executar função LogDataService()");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Loop Thread LogDataService: " + ex.Message);
                    }
                    finally
                    {
                        Thread.Sleep(350);
                    }
            }

            private bool getSendLog()
            {
                try
                {
                    lock (thisLock)
                    {
                        //olhar para a lista esta vazia sai
                        while (listaDeLogs.Count > 0)
                        {
                            gere_ficheiro(listaDeLogs.First().ToString(), pathLog, numeroLog, cabecalho);
                            listaDeLogs.RemoveAt(0);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ERRO - Função getSendLog: " + ex.Message);
                    return false;
                }
                finally
                {
                    GC.Collect();
                }


            }

            public List<string> array_file_aux { get; set; }

            public void gere_ficheiro(string linha_debug, string _enderecoFicheiro_debug, int _dimensaoFifo_debug, string _cabecalho_debug)
            {
                try
                {
                    if (File.Exists(_enderecoFicheiro_debug) == false)
                        File.Create(_enderecoFicheiro_debug).Dispose();

                    array_file_aux = new List<string>(File.ReadAllLines(_enderecoFicheiro_debug));
                    string str_aux = "";
                    int index = array_file_aux.Count;

                    StreamWriter CSVWriter = default(StreamWriter);
                    if (index != 0)
                    {
                        str_aux = array_file_aux[0];
                    }

                    if (str_aux != _cabecalho_debug || index == 0 || index > _dimensaoFifo_debug)
                    {
                        int Filenum = FileSystem.FreeFile();
                        FileSystem.FileOpen(Filenum, _enderecoFicheiro_debug, OpenMode.Output);
                        FileSystem.FileClose();
                        //ficheiro limpo agora há que criar os novos cabeçalhos
                        //abre o ficheiro
                        CSVWriter = Microsoft.VisualBasic.FileIO.FileSystem.OpenTextFileWriter(_enderecoFicheiro_debug, true);
                        CSVWriter.WriteLine(_cabecalho_debug);
                        if (index > _dimensaoFifo_debug)
                        {
                            string a;
                            int index_r = 0;
                            for (int i = 2; i <= index - 1; i++)
                            {
                                //limpa o índice presente na linha
                                index_r = array_file_aux[i].IndexOf(";");
                                a = Convert.ToString(i - 1) + array_file_aux[i].Remove(0, index_r);
                                CSVWriter.WriteLine(a);
                            }
                        }
                        CSVWriter.Close();
                    }
                    // 'abre o ficheiro
                    CSVWriter = Microsoft.VisualBasic.FileIO.FileSystem.OpenTextFileWriter(_enderecoFicheiro_debug, true);
                    if (index == 0)
                        index = 1;
                    if (index >= _dimensaoFifo_debug)
                        index = _dimensaoFifo_debug;
                    CSVWriter.WriteLine(Convert.ToString(index) + ";" + linha_debug);
                    //Debug.WriteLine("index=" + index);

                    CSVWriter.Close();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Log write Fail; Path: " + _enderecoFicheiro_debug.ToString() + " error: " + ex.Message);
                }
            }

            public void Dispose()
            {
                this.isAlive = false;
            }

        }

    }

}