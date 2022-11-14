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
using System.Drawing.Text;

namespace _22079AI
{
    public class InspLateralDiscos
    {
        //0 - nao processado
        //1 - OK
        //2 - Erro de inspeção
        //3 - NOK
        private object aqcLock = new object();
        private object inspLock = new object();
        private object ThreadLock = new object();

        //Estados da classe
        private bool isAlive = true;
        private bool capturaOK = false;
        private bool inspecaoOK = false;
        private string diretorioImagens = @"C:\STREAK\Imagens Gravadas\";
        private string scriptDirPath = Application.StartupPath + @"\Scripts\";

        public string FicheiroIni { get; } = Application.StartupPath + @"\InspectionConfig.ini";

        public string TipoInspecao { get; private set; } = "CORK";
        public string FicheiroConfiguracoes { get; private set; } = Application.StartupPath + @"\ivo_cam.ini";

        private int numMaximoPastaImagens = 50;

        public double numberAqThreads = 0;
        public double numberInspThreads = 0;

        private bool guardarImagemFimInspecao = true;
        private int timeoutToInspection = -1;
        private bool inspectiontimeout = false;

        public int DbNumber { get; private set; } = 47;

        private bool ordemTrigger = false;

        private bool sistemaOff = false;
        public bool CameraAtiva
        {
            get { return this.EstadoVisao && !this.sistemaOff; }
        }

        private bool desenhaLinhas = true;
        private int numeroLinhas = 10;

        public int imageW = 4912;
        public int imageH = 3684;

        private int offsetRotacao = 0;

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
        uint idFaca = 0;

        private double alturaFacaCamera = 1.10;

        public bool FacaDetetada { get; private set; } = false;
        public double ComprimentoFaca { get { return Math.Round(this.comprimentoFaca, 3); } }
        public double AmplitudeDesvioFacaSuperior { get { return Math.Round(this.amplitudeDesvioFacaSuperior, 3); } }
        public double AmplitudeDesvioFacaInferior { get { return Math.Round(this.amplitudeDesvioFacaInferior, 3); } }
        public double PosDesvioMaxFacaSuperior { get { return Math.Round(this.posDesvioMaxFacaSuperior, 3); } }
        public double PosDesvioMaxFacaInferior { get { return Math.Round(this.posDesvioMaxFacaInferior, 3); } }

        public double AlturaFacaCamera { get { return Math.Round(this.alturaFacaCamera, 1); } }

        List<DiscMenbers> ListaDiscosInspecionados = new List<DiscMenbers>();

        /// <summary>
        /// 0 - SEM RESULTADO / 1 - OK / 2 - OK INV (REWORK) / 3 - NOK
        /// </summary>
        public int ResultadoInspecao
        {
            get; private set;
        }

        public bool ResultadoComprimento
        {
            get
            {
                return Forms.MainForm.Receita.EvaluateComprimento(this.comprimentoFaca);
            }
        }

        public bool ResultadoDesvioSuperior
        {
            get
            {
                return Forms.MainForm.Receita.EvaluateDesvio(this.amplitudeDesvioFacaSuperior, Forms.MainForm.Receita.ToleranciaDesvioSuperior, Forms.MainForm.Receita.DesvioNominal);
            }
        }

        public bool ResultadoDesvioInferior
        {
            get
            {
                return Forms.MainForm.Receita.EvaluateDesvio(this.amplitudeDesvioFacaInferior, Forms.MainForm.Receita.ToleranciaDesvioInferior, Forms.MainForm.Receita.DesvioNominal);
            }
        }


        /// <summary>
        /// 0-> OFF / 1-> ALWAYS OK / 2-> ALWAYS NOT OK
        /// </summary>
        public int ResultadosForcados = 0;
        public bool InspecoesSimuladas { get; set; } = false;

        public DateTime TempoInicioInspecao { get; private set; } = DateTime.MinValue;
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
        private CamObject CamInsp { get; set; }

        private HWindow DisplayLive { get; set; }
        private HWindow DisplayInsp { get; set; }

        public HObject LastInspectionImage
        {
            get
            {
                try
                {
                    return this.outputImage;
                    //return this.CamInsp.HWindow.DumpWindowImage();
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

        public bool EXTERNAL_TRIGGER
        {
            get
            {
                return this.externalTrigger;
            }
            set
            {
                if (value && !this.externalTrigger)
                    this.FazTrigger(); //fp trigger

                this.externalTrigger = value;
            }
        }

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

            new Thread(this.EnviaResultadosPLC).Start();
        }

        private void EnviaResultadosPLC()
        {
            while (this.isAlive)
            {
                if (ListaDiscosInspecionados.Count() > 0 && Forms.MainForm.PlcControl.PlcReadyForNewDIsc)
                {
                    lock (lockLista)
                    {
                        Forms.MainForm.PlcControl.HmiPlcNewDisc.writeClassValues(ListaDiscosInspecionados.First());
                        ListaDiscosInspecionados.RemoveAt(0);

                        Debug.WriteLine("### EnviaResultadosPLC - Enviou Resultado com ID: " + Forms.MainForm.PlcControl.HmiPlcNewDisc.menber.ID);
                    }

                }

            }

        }

        public void FazTrigger()
        {
            if (!this.sistemaOff && !this.InspecaoEmCurso)
                this.ordemTrigger = true;
        }

        //public void FazTriggerPLC()
        //{
        //    if (!this.sistemaOff && !this.InspecaoEmCurso)
        //        Forms.MainForm.PLC1.EnviaTag(Siemens.MemoryArea.DB, Siemens.TipoVariavel.Bool, true, this.DbNumber, 10, 2);
        //}

        private void AdicionaRegistoLog(uint id, bool result, double value1, double value2, double value3, double value4, double value5)
        {
            if (this.LogHistoricoInspecoesEnabled)
                if (LogNormal != null) //Significa que temos o log ativado
                    this.LogNormal.DataToSend(DateTime.Now.ToString(@"dd\_MM\_yyyy\_HH\_mm\_ss") + ";" + id.ToString() + ";" + (result ? "OK" : "NOK") + ";" + value1.ToString() + ";" + value2.ToString() + ";" + value3.ToString() + ";" + value4.ToString() + ";" + value5.ToString() + ";");
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
                    this.DbNumber = Convert.ToInt32(ini.RetornaINI("Visão", "DbNumber", Convert.ToString(this.DbNumber)));
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
                }


                if (this.LogHistoricoInspecoesEnabled)
                {
                    if (!Directory.Exists(Path.GetDirectoryName(PathHistoricoInspecoes)))  //Caso o diretório do log do histórico de inspeções não exista cria-o
                        Directory.CreateDirectory(Path.GetDirectoryName(PathHistoricoInspecoes));

                    this.LogNormal = new LogDataService("Index;Datetime;ID;Result;Comprimento;Curvatura Superior;Curvatura Inferior;Pos. Curvatura Inf;Pos. Curvatura Sup", this.PathHistoricoInspecoes, this.NumeroHistoricoInspecoes);
                }

                #endregion

                //Iniciar o objecto display de imagem
                this.CamLive = new CamObject(true);
                this.DisplayLive = this.CamLive.Inicializar(this);

                this.CamInsp = new CamObject(false);
                this.DisplayInsp = this.CamInsp.Inicializar(this);

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

                            //Roda a imagem, se necessário
                            if (this.offsetRotacao != 0)
                                HOperatorSet.RotateImage(tempImg, out tempImg, this.offsetRotacao, "constant");

                            //copia a imagem adquirida para a atual
                            lock (this.aqcLock)
                                HOperatorSet.CopyImage(tempImg, out this.imagemAtual);


                            //Mostra a imagem live no ecrã
                            //Mostra a região
                            if (FormularioReceita.FormActivo && Forms.MainForm.Receita.ID == FormularioReceita.IDRegisto)
                                regiaoPesquisaP2.X = FormularioReceita.compRegiao;
                            else
                                regiaoPesquisaP2.X = Forms.MainForm.Receita.CompRegiaoInsp;



                            if (this.MostraRegiaoPesquisa)
                                this.CamInsp.DisplayImage(HalconFunctions.HobjectToHimage(this.imagemAtual), this.regiaoPesquisaP1, this.regiaoPesquisaP2);
                            else
                                this.CamLive.DisplayImage(this.imagemAtual, (int)this.numberInspThreads);

                            this.numberInspThreads++;
                            //Vai buscar o ID ao PLC
                            //Envia a imagem e ID para a thread de inspeção
                            NumeroThreads ++;

                            this.InspecaoEmCurso = true;
                            //new Thread(this.CallInspection).Start(); //inicia uma instância da inspeção

                            Debug.WriteLine("InspNumber antes da Thread: " + this.numberInspThreads);
                            int Auxnumber = (int)this.numberInspThreads;
                            HObject AuxImage = this.imagemAtual;
                            Thread NewThread = new Thread(() => this.CallInspection(AuxImage, Auxnumber, (int)this.numberInspThreads));
                            NewThread.Start();




                           
                            //trigger à camera se não tiver em trigger
                            //if (!this.sistemaOff)
                            //    if (!this.InspecaoEmCurso && this.ordemTrigger)
                            //    {
                            //        this.InspecaoEmCurso = true;

                            //        new Thread(this.CallInspection).Start(); //inicia uma instância da inspeção
                            //    }

                            //liberta os recursos da imagem temporária
                            tempImg.Dispose(); tempImg = null;

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


        private void CallInspection(HObject ImagemInInspecao, int InspNumber, int TotalInspNumber)
        {
            int _InspNumber = InspNumber;
            HObject ImagemActInspecao = new HObject();
            DateTime startTime = DateTime.Now, AuxDT;
            //lock (this.inspLock)
            try
            {


                this.InspecaoEmCurso = true;

                this.ProcessaNovaOrdemInspecao();


                //******* AQUISIÇÃO DA IMAGEM *******
                this.AcquisicaoEmCurso = true;

                lock (this.aqcLock)
                { //copia a imagem atual para a imagem de inspecao
                    HOperatorSet.CopyImage(ImagemInInspecao, out ImagemActInspecao);  //HOperatorSet.CopyImage(this.imagemAtual, out this.imagemInspecao);
                    HOperatorSet.CopyImage(ImagemInInspecao, out this.imagemInspecao);
                }

                //limpa a janela
                this.CamInsp.HWindow.ClearWindow();

                //Mostra a imagem inspeção
                this.CamInsp.DisplayImage(HalconFunctions.HobjectToHimage(ImagemActInspecao), 0, DateTime.MinValue);

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
                        if (this.saveImage && !this.guardarImagemFimInspecao)
                        {
                            //Cria o caminho geral do diretório das imagens
                            //string filename = this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH-mm-ss") + "_" + this.TipoInspecao + ".bmp";

                            string filename = DB400.INSPECTION_RESULT.ID.ToString() + "_" + this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH-mm-ss") + "_" + this.TipoInspecao + ".bmp";

                            //Juntas o diretório num só
                            this.imageFullPath = this.diretorioImagens + filename;

                            //Em thread, grava a imagem adquirida, de acordo com o metodo pretendido
                            if (this.saveImgByHalcon)
                                new Thread(() => this.GravaImagem1(ImagemActInspecao.Clone(), this.imageFullPath)).Start();
                            else
                                new Thread(() => this.GravaImagem2(ImagemActInspecao.Clone(), this.imageFullPath)).Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("CallInspection() - Gravar a imagem: " + ex.Message);
                    }

                    #endregion

                    //prepara outputs da função da inspeção
                    bool erroInspecao = false, facaDetetada = false;
                    string msgInspecao = string.Empty;
                    double comprimentoFaca = 0, amplitudeDesvioSuperior = 0, amplitudeDesvioInferior = 0, posMaxDesvioSuperior = 0, posMaxDesvioInferior = 0;

                    //Chamar o módulo de inspeção
                    this.CallInspectionScript(ImagemActInspecao, this.ResultadoInspecao, Forms.MainForm.Receita.CompRegiaoInsp, this.InspecoesSimuladas, out facaDetetada, out comprimentoFaca, out amplitudeDesvioSuperior, out posMaxDesvioSuperior, out amplitudeDesvioInferior, out posMaxDesvioInferior, out erroInspecao, out msgInspecao);

                    //Processar os dados obtidos da visão
                    if (this.ResultadosForcados == 0)
                        this.ProcessaDadosInspecao(facaDetetada, comprimentoFaca, amplitudeDesvioSuperior, posMaxDesvioSuperior, amplitudeDesvioInferior, posMaxDesvioInferior, erroInspecao, msgInspecao);
                    else
                        this.ResultadoInspecao = this.ResultadosForcados == 1 ? 1 : 3; //força OK / NOK consoante o valor 'ResultadosForcados'

                    //mostra o resultado na imagem da inspeção
                    this.CamInsp.UpdateStatus(this.ErroInspecao ? LabelStatus.ErroInspecao : (LabelStatus)this.ResultadoInspecao);

                    //Enviar os resultados no PLC - Processa o fim da inspeção


                    Forms.MainForm.VariaveisAuxiliares.AuxOrdInsertData = true;


                    #region Gravar a imagem ** Se tiver ordem para gravar a imagem apos A inspeção **
                    //Se tiver ordem para gravar a imagem apos a inspeção
                    if (this.saveImage && this.guardarImagemFimInspecao)
                    {
                        //Cria o caminho geral do diretório das imagens
                        //string filename = this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH-mm-ss") + "_" + this.TipoInspecao + "_" + (this.ResultadoInspecao == 1 ? "OK" : "NOT_OK") + ".bmp";

                        //string filename = "ID" + idFaca.ToString() + "_" + Diversos.NumberToString(DB400.INSPECTION_RESULT.AMPLITUDE_DESVIO, 2, true) + "_" + Diversos.NumberToString(DB400.INSPECTION_RESULT.POS_MAX_DESVIO, 2, true) + "_" + this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH-mm-ss") + "_" + (this.ResultadoInspecao == 1 ? "OK" : "NOT_OK") + ".bmp";
                        string filename = "ID" + idFaca.ToString() + "_" + Diversos.NumberToString(comprimentoFaca, 2, true) + "_" + Diversos.NumberToString(amplitudeDesvioSuperior, 2, true) + "_" + Diversos.NumberToString(posMaxDesvioSuperior, 2, true) + "_" + this.TempoInicioInspecao.ToString("dd-MM-yyyy") + "_" + this.TempoInicioInspecao.ToString("HH-mm-ss");

                        //Juntas o diretório num só
                        this.imageFullPath = this.diretorioImagens + filename;

                        //Em thread, grava a imagem adquirida, de acordo com o metodo pretendido
                        if (this.saveImgByHalcon)
                            new Thread(() => this.GravaImagem1(ImagemActInspecao.Clone(), this.imageFullPath)).Start();
                        else
                            new Thread(() => this.GravaImagem2(ImagemActInspecao.Clone(), this.imageFullPath)).Start();
                    }

                    /*calcula numero atual de threads*/
                    AuxDT = DateTime.Now;
                    Debug.WriteLine("# InspNumber: " + _InspNumber);
                    while ((_InspNumber - 1) != (this.numberAqThreads)) { }
                    while ((Forms.MainForm.PlcControl.HmiPlcFeedbackdisc.menber.ID == Forms.MainForm.PlcControl.PlcHmiNewDisc.menber.ID)
                           )
                    {
                    }

                    Debug.WriteLine("#terminou Nova Inspeção com ID: " + Forms.MainForm.PlcControl.PlcHmiNewDisc.menber.ID +" #Numero Threads: " + NumeroThreads + " # Tempo PLC: " + (DateTime.Now - AuxDT).TotalMilliseconds + " # CallInspection - Elapsed time: " + Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds) + " ms ***");
                    this.ProcessaFimInspecaoPLC(Forms.MainForm.PlcControl.PlcHmiNewDisc.menber);
                    Forms.MainForm.PlcControl.HmiPlcFeedbackdisc.writeClassValues(Forms.MainForm.PlcControl.PlcHmiNewDisc.menber);
                    lock (this.ThreadLock)
                    {
                        while ((Forms.MainForm.PlcControl.HmiPlcFeedbackdisc.menber.ID != Forms.MainForm.PlcControl.PlcHmiNewDisc.menber.ID)
                           )
                        {
                        }
                        this.numberAqThreads = _InspNumber;
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

        public void ProcessaDadosInspecao(bool facaDetetada, double comprimentoFaca, double amplitudeDesvioSuperior, double posMaxDesvioSuperior, double amplitudeDesvioInferior, double posMaxDesvioInferior, bool erroInspecao, string msgInspecao)
        {
            //Processar os dados de inspecao
            this.ErroInspecao = erroInspecao;
            this.MensagemErro = erroInspecao ? msgInspecao : string.Empty;

            if (!facaDetetada)
            {
                this.ErroInspecao = true;

                if (string.IsNullOrWhiteSpace(this.MensagemErro))
                    this.MensagemErro = "Faca não detetada";
            }


            if (!this.ErroInspecao && !this.inspectiontimeout) //se não houver erro de inspeção compara o valor lido pelos limites
            {
                //move os valores DA INSPEcAo para as variáveis
                this.FacaDetetada = facaDetetada;
                this.comprimentoFaca = comprimentoFaca;
                this.amplitudeDesvioFacaSuperior = amplitudeDesvioSuperior;
                this.posDesvioMaxFacaSuperior = posMaxDesvioSuperior;
                this.amplitudeDesvioFacaInferior = amplitudeDesvioInferior;
                this.posDesvioMaxFacaInferior = posMaxDesvioInferior;

                //atribui a avaliação do anel
                if (this.ResultadoComprimento && this.ResultadoDesvioSuperior && this.ResultadoDesvioInferior)
                    this.ResultadoInspecao = 1;
                else
                    this.ResultadoInspecao = 3;
            }
            else
            {
                this.ResultadoInspecao = 2; //caracteriza o anel como recuperavel!
                this.ErroInspecao = true;

                if (this.inspectiontimeout) this.MensagemErro = "Inspection timeout!";
            }

            this.LastInspectionMsgErro = this.MensagemErro;

            //adiciona um registo na base de dados
            this.AdicionaRegistoLog(idFaca, this.ResultadoInspecao == 1, this.ComprimentoFaca, this.AmplitudeDesvioFacaSuperior, this.PosDesvioMaxFacaSuperior, this.AmplitudeDesvioFacaInferior, this.PosDesvioMaxFacaInferior);
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

        private void ProcessaFimInspecaoPLC(DiscMenbers Disc)
        {
            //Enviar o resultado de inspeção
            Disc.result = (short)this.ResultadoInspecao;

            lock (lockLista)
            {
                ListaDiscosInspecionados.Add(Disc);
            }



            this.TempoInspecao = Convert.ToInt32((DateTime.Now - this.TempoInicioInspecao).TotalMilliseconds) - this.TempoAquisicao;

            if (this.inspecaoOK)
                Debug.WriteLine("*** Resultado + Sinal de Inspeção OK " + this.TipoInspecao + " enviado **");
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
            this.ordemTrigger = false;
            this.MensagemErro = string.Empty;
            this.FacaDetetada = false;
            this.comprimentoFaca = this.amplitudeDesvioFacaSuperior = this.amplitudeDesvioFacaInferior = this.posDesvioMaxFacaSuperior = this.posDesvioMaxFacaInferior = 0;
            this.inspectiontimeout = false;

            this.imageFullPath = string.Empty;

            return true;
        }

        private void CallInspectionScript(HObject img, double resultadoInsp, int compRegiaoInsp, bool modoSimulacao, out bool facaDetetada, out double valorComprimentoFaca, out double valorAmplitudeDesvioSuperior, out double valorPosMaxDesvioSuperior, out double valorAmplitudeDesvioInferior, out double valorPosMaxDesvioInferior, out bool erroInspecao, out string msgErro)
        {
            DateTime startTime = DateTime.Now;

            //instancia variaveis com default values
            valorComprimentoFaca = valorAmplitudeDesvioSuperior = valorAmplitudeDesvioInferior = valorPosMaxDesvioSuperior = valorPosMaxDesvioInferior = 0;
            facaDetetada = erroInspecao = false;
            msgErro = string.Empty;

            try
            {
                if (!this.loadedScript)
                    throw new Exception("Script Loading Error!");

                if (!modoSimulacao)
                {
                    //Configura as inputs do script
                    this.inspScript.SetInputIconicParamObject("InputImage", img);

                    //Executa o script
                    if (this.timeoutToInspection <= 0)
                        this.inspScript.Execute();
                    else
                        this.RunMethodWithTimeout(() => this.inspScript.Execute(), this.timeoutToInspection);


                    //Obtem as outputs do script
                    outputImage = this.inspScript.GetOutputIconicParamObject("OutputImage");
                    resultadoInsp = 1;//this.inspScript.GetOutputCtrlParamTuple("Resultado").D;
                    erroInspecao = false;// this.inspScript.GetOutputCtrlParamTuple("Erro") == 1;

                    //msgErro = this.inspScript.GetOutputCtrlParamTuple("MsgErro")[0];

                    msgErro = "";//o emanuel nao colocou menssagens em todos os caos :) RC

                    //facaDetetada = this.inspScript.GetOutputCtrlParamTuple("FacaDetetada") == 1;
                    //valorComprimentoFaca = this.inspScript.GetOutputCtrlParamTuple("ComprimentoLamina").D;
                    //valorAmplitudeDesvioSuperior = this.inspScript.GetOutputCtrlParamTuple("DesvioCurvaturaSuperior").D;
                    //valorPosMaxDesvioSuperior = this.inspScript.GetOutputCtrlParamTuple("PosDesvioSuperior").D;
                    //valorAmplitudeDesvioInferior = this.inspScript.GetOutputCtrlParamTuple("DesvioCurvaturaInferior").D;
                    //valorPosMaxDesvioInferior = this.inspScript.GetOutputCtrlParamTuple("PosDesvioInferior").D;



                    //Se não tiver erro de inspeção, adiciona a imagem tratada do halcon e adiciona as regiões
                    if (true)
                    {
                        //limpa a janela
                        this.CamInsp.HWindow.ClearWindow();

                        //carrega os resultados visuais da inspeção para uma estrutura de dados
                        this.UltimaImagemInspecao.UpdateDados(this.inspScript.GetOutputIconicParamImage("OutputImage"));

                        //mostra imagem rodada vinda da inspeção
                        this.CamInsp.DisplayImage(this.UltimaImagemInspecao.hImage, this.TempoInicioInspecao);

                        #region Mostra as Regiões na Imagem
                        if (false)//this.UltimaImagemInspecao.ValidRegions())
                        {
                            //mostra contornos
                            this.DisplayInsp.SetLineWidth(2.5);

                            for (int i = 0; i < this.UltimaImagemInspecao.regions.CountObj(); i++)
                            {
                                this.DisplayInsp.SetColor((string)this.UltimaImagemInspecao.colors[i]);
                                this.DisplayInsp.DispObj(this.UltimaImagemInspecao.regions.SelectObj(i + 1));
                            }
                        }
                        #endregion
                    }
                    else
                        throw new Exception(!string.IsNullOrWhiteSpace(msgErro) ? msgErro : "Erro de inspeção não especificado! [C#]");

                }
                #region Modo de Simulação
                //else
                //{
                //    Random rand = new Random();

                //    bool approved = rand.NextDouble() >= 0.15;
                //    Thread.Sleep(rand.Next(100, 300)); //espera a simular a inspeção

                //    valorLido = Diversos.GetRandomNumber(this.LimiteInferior, this.LimiteSuperior);

                //    double increment = rand.NextDouble();

                //    if (rand.NextDouble() >= 0.5)
                //        increment = increment * -1.0;

                //    if (!approved)
                //        valorLido += increment;

                //    //Obtem as outputs aleatorias


                //    _offsetCorrecao = 0;
                //    ferramental_topo = 0;
                //    ferramental_inferior = 0;
                //    erroInspecao = false;
                //    msgErro = erroInspecao ? "Erro Simulação!" : string.Empty;
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Debug.Write("CallInspectionScript(" + this.TipoInspecao + "): " + ex.Message);

                //força o erro de inspeção
                erroInspecao = true;

                //Mensagem de erro
                msgErro = ex.Message;

                //limpa a janela
                this.CamInsp.HWindow.ClearWindow();
                //Volta a mostrar a imagem original com o timestamp original e a label de erro de ispecao
                this.CamInsp.DisplayImage(HalconFunctions.HobjectToHimage(this.imagemInspecao), LabelStatus.ErroInspecao, this.TempoInicioInspecao);
            }
            finally
            {
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

                    if (sfd.ShowDialog() == DialogResult.OK)
                        lock (this.aqcLock)
                            this.GravaImagem1(this.imagemAtual.Clone(), sfd.FileName);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SaveAtualImageAs(): " + ex.Message);
                return false;
            }

        }

        private bool GravaImagem1(HObject hOImage, string imageFullPath)
        {
            this.GravacaoEmCurso = true;

            DateTime dataInicial = DateTime.Now;

            try
            {
                HOperatorSet.WriteImage(hOImage, "bmp", 0, imageFullPath);

                //Debug.WriteLine("GravaImagem1(" + this.TipoInspecao + "): Image saved sucessufully on " + imageFullPath + " ** Elapsed Time: " + Convert.ToInt32((DateTime.Now - dataInicial).TotalMilliseconds) + " ms");
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("*** Exception Thread GravaImagem1 " + this.TipoInspecao + ": Error Saving Image - " + e.Message);
                return false;
            }
            finally
            {
                if (hOImage != null) hOImage.Dispose();

                //Faz o fifo de imagens
                this.FazFifo();

                this.GravacaoEmCurso = false;

            }
        }

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RunMethodWithTimeout(" + this.TipoInspecao + "): " + ex.Message);
                GC.Collect();
                this.inspectiontimeout = true;
            }

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