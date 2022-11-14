using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sharp7;
using static PLC.Siemens.ReadMultiVariables;

namespace PLC
{
    public class Siemens
    {

        private S7Client Client;
        private object clientLock = new object();
        private object writeLock = new object();
        private const string strSemComunicacao = "####";
        private bool disconnectDone = false;

        private GereTempos TemposCiclo = new GereTempos(1000);
        private Stopwatch timeCounter = new Stopwatch();

        public bool RestartRequested = false;

        private List<AuxVariable> writeList = new List<AuxVariable>();

        public int NumOfElementsToWrite
        {
            get
            {
                return this.writeList.Count;
            }
        }

        #region Propriedades
        public long TempoCicloDecorrido
        {
            get { return timeCounter.IsRunning ? timeCounter.ElapsedMilliseconds : 0; }
        }

        public string StringSemComunicacao
        {
            get { return strSemComunicacao; }
        }

        /// <summary>
        /// Endereço de IP do PLC
        /// </summary>
        public IPAddress EnderecoIP { get; set; } = IPAddress.Parse("192.168.0.10");

        /// <summary>
        /// Slot de Conexão. Por defeito = 0
        /// </summary>
        public int Slot { get; set; } = 0;

        /// <summary>
        /// Retorna o estado da ligação ao PLC
        /// </summary>
        public bool EstadoLigacao
        {
            get { return Client != null ? (this.Client.Connected && UltimoEstadoPLC == EstadoPLC.LigadoComLigacao) : false; }
        }

        /// <summary>
        /// Retorna o Último Estado do PLC
        /// </summary>
        public EstadoPLC UltimoEstadoPLC { get; private set; } = EstadoPLC.DesligadoSemLigacao;

        public int TempoCicloAtual
        {
            get { return TemposCiclo.TempoAtual; }
        }
        public int TempoCicloMedio
        {
            get { return TemposCiclo.TempoMedio; }
        }
        public int TempoCicloMinimo
        {
            get { return TemposCiclo.TempoMinimo; }
        }
        public int TempoCicloMaximo
        {
            get { return TemposCiclo.TempoMaximo; }
        }

        #endregion


        public Siemens(IPAddress _enderecoIP, int _slot)
        {
            EnderecoIP = _enderecoIP;
            Slot = _slot;
        }

        public Siemens(IPAddress _enderecoIP)
        {
            EnderecoIP = _enderecoIP;
        }

        public Siemens(string _ficheiroConfiguracoes, bool _mostraMsgError = true)
        {
            try
            {
                if (!File.Exists(_ficheiroConfiguracoes))
                    File.Create(_ficheiroConfiguracoes);

                using (Ini iniFile = new Ini(_ficheiroConfiguracoes))
                {
                    EnderecoIP = IPAddress.Parse(iniFile.RetornaINI("PLC", "plcAddress", "192.168.10.10"));
                    Slot = Convert.ToInt32(iniFile.RetornaINI("PLC", "plcSlot", "0"));
                }
            }
            catch (Exception ex)
            {
                if (_mostraMsgError)
                    System.Windows.Forms.MessageBox.Show("Erro ao iniciar comunicação com PLC: " + ex.Message, "PLC", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine("PLC Constructor: " + ex.Message);
            }
        }

        /// <summary>
        /// Estabelece uma comunicação com o PLC
        /// </summary>
        /// <returns></returns>
        public bool ConnectToPLC()
        {
            lock (clientLock)
            {

                if (Client == null)
                    Client = new S7Client();

                if (!Client.Connected)
                    try
                    {
                        if (!Ping(EnderecoIP.ToString()))
                            throw new Exception("Falha ao fazer Ping ao IP: " + (EnderecoIP.ToString()));
                        else
                        {
                            int returnCode = Client.ConnectTo(EnderecoIP.ToString(), 0, Slot);

                            if (returnCode != 0)
                                throw new Exception("Erro ao ligar ao PLC. Error Code: " + Convert.ToString(returnCode) + " " + Client.ErrorText(returnCode));
                            else UltimoEstadoPLC = EstadoPLC.LigadoComLigacao;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ConnectToPLC(): " + ex.Message);
                    }
                    finally
                    {
                        if (Client != null)
                            if (Client.Connected)
                                Debug.WriteLine("CONEXÃO INICIADA COM O PLC");
                        RestartRequested = false;
                    }

                return Client.Connected;
            }
        }

        /// <summary>
        /// Desliga uma conexão ao PLC
        /// </summary>
        /// <returns></returns>
        public bool DisconnectFromPLC()
        {
            lock (clientLock)
            {
                if (Client != null)
                    try
                    {
                        if (Client.Connected)
                        {
                            int returnCode = Client.Disconnect();

                            if (returnCode == 0)
                                return true;
                            else
                                throw new Exception("Erro ao desligar do PLC. Error Code: " + Convert.ToString(returnCode) + " " + Client.ErrorText(returnCode));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("DisconnectFromPLC(): " + ex.Message);

                        return false;
                    }
                    finally
                    {
                        Client = null;

                        Debug.WriteLine("CONEXÃO DESLIGADA COM O PLC.  RestartRequested == " + RestartRequested);
                        RestartRequested = false;

                        disconnectDone = true;

                        UltimoEstadoPLC = EstadoPLC.LigadoSemLigacao;
                    }

                return true;
            }
        }

        #region Funções de Leitura

        private int MakeRead(MemoryArea area, int _dbNumber, int _offset, ref byte[] buffer)
        {
            lock (clientLock)
                switch (area)
                {
                    case MemoryArea.DB: return Client.DBRead(_dbNumber, _offset, buffer.Length, buffer);
                    case MemoryArea.M: return Client.MBRead(_offset, buffer.Length, buffer);
                    case MemoryArea.I: return Client.EBRead(_offset, buffer.Length, buffer);
                    case MemoryArea.Q: return Client.ABRead(_offset, buffer.Length, buffer);
                }

            return -1;
        }

        public int MakeGeneralRead(MemoryArea area, int _dbNumber, int _offset, ref byte[] buffer)
        {
            //Faz as verificações necessárias ao PLC
            if (UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                VerificaPLC();

            if (UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    if (Client.Connected)
                        lock (clientLock)
                       {
                            int res = this.MakeRead(area, _dbNumber, _offset, ref buffer);
                            if (res == 0)
                                return res;
                            else
                                throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                        }
                    else
                       throw new Exception("Sem comunicação com PLC");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MakeGeneralRead(): " + ex.Message);
                    VerificaPLC();
                    return -1;
                }
            else
                return -1;
        }

        /// <summary>
        /// NOTA: No caso das memorias, o dbNumber deverá ser 0 e o endereço é offset (e lenght para os bits)
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tipoVariavel"></param>
        /// <param name="_dbNumber"></param>
        /// <param name="_offset"></param>
        /// <param name="_lenght"></param>
        /// <returns></returns>
        public dynamic LeTag(MemoryArea area, TipoVariavel tipoVariavel, int _dbNumber, int _offset, ushort _lenght = 0)
        {
            //Faz as verificações necessárias ao PLC
            if (UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                VerificaPLC();

            if (UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    if (Client.Connected)
                    {
                        if (area == MemoryArea.DB && _dbNumber < 0)
                            throw new Exception("Incorrect DB number!");

                        byte[] buffer = new byte[ObtemNBytesVariavel(tipoVariavel, _lenght)];
                        int res = this.MakeRead(area, _dbNumber, _offset, ref buffer);

                        if (res == 0)
                            return GetValueAt(tipoVariavel, buffer, _lenght);
                        else
                            throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));

                    }
                    else
                        throw new Exception("Sem comunicação com PLC");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("LeTag(): " + ex.Message);
                    VerificaPLC();
                    return null;
                }
            else
                return null;
        }

        public bool LeSequenciaTags(MemoryArea area, ReadMultiVariables[] variaveis, int _dbNumber, int _startAddress)
        {
            //Faz as verificações necessárias ao PLC
            if (this.UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                this.VerificaPLC();

            if (this.UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    int auxBytes = 0;
                    for (int i = 0; i < variaveis.Length; i++)
                        auxBytes += variaveis[i].ByteCount;

                    byte[] buffer = new byte[auxBytes];

                    int res = this.MakeGeneralRead(area, _dbNumber, _startAddress, ref buffer);

                    if (res == 0)
                    {
                        auxBytes = 0;

                        for (int i = 0; i < variaveis.Length; i++)
                        {
                            variaveis[i].LeVariavel(buffer, auxBytes);
                            auxBytes += variaveis[i].ByteCount;
                        }
                        return true;
                    }
                    else
                        throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("LeSequenciaTags(): " + ex.Message);
                    this.VerificaPLC();
                    return false;
                }
            else
                return false;
        }

        public dynamic[] MultiRead(AuxVariable[] variaveis)
        {
            //Faz as verificações necessárias ao PLC
            if (this.UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                this.VerificaPLC();

            if (this.UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    List<int> res = new List<int>();
                    int indexTag = 0;
                    byte[][] buffer = new byte[variaveis.Length][];

                    if (variaveis == null || variaveis.Count() == 0)
                        throw new Exception("No Vars to Read!");

                    S7MultiVar vars = new S7MultiVar(this.Client);

                    lock (clientLock)
                        while (indexTag < variaveis.Length)
                        {
                            buffer[indexTag] = new byte[ObtemNBytesVariavel(variaveis[indexTag].TipoVariavel, variaveis[indexTag].Lenght)];
                            vars.Add(MemoryAreaToS7Const(variaveis[indexTag].MemoryArea), S7Consts.S7WLByte, variaveis[indexTag].DbNumber, variaveis[indexTag].Offset, buffer[indexTag].Length, ref buffer[indexTag]);

                            indexTag++;

                            if (vars.Count == S7Client.MaxVars || (indexTag == variaveis.Length))
                            {
                                res.Add(vars.Read());
                                vars.Clear();
                            }
                        }

                    for (int i = 0; i < res.Count; i++)
                        if (res[i] != 0)
                            throw new Exception("Error Code: " + res[i] + " " + Client.ErrorText(res[i]));


                    Debug.WriteLine("MultiRead(): Nº of reads - " + res.Count.ToString());


                    List<object> returnList = new List<object>();

                    for (int i = 0; i < buffer.Length; i++)
                        returnList.Add(GetValueAt(variaveis[i].TipoVariavel, buffer[i], variaveis[i].Lenght));

                    return returnList.ToArray();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MultiRead(): " + ex.Message);
                    this.VerificaPLC();
                    return null;
                }
            else
                return null;
        }

        #region Diagnósticos de Sistema
        /// <summary>
        /// Returns the last job execution time in milliseconds
        /// </summary>
        /// <param name="">Forçar Atualização do estado do PLC?</param>
        /// <returns>Returns the last job execution time in milliseconds</returns>
        public int LeTempoCicloPLC()
        {
            //Faz as verificações necessárias ao PLC
            if (UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                VerificaPLC();

            if (UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    if (Client.Connected)
                        lock (clientLock)
                            return Client.ExecTime();
                    else
                        throw new Exception("Sem comunicação com PLC");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("LeTempoCicloPLC(): " + ex.Message);
                    VerificaPLC();
                    return -2;
                }
            else
                return -2;
        }

        /// <summary>
        /// Returns the PLC date/time.
        /// </summary>
        /// <param name="">Forçar Atualização do estado do PLC?</param>
        /// <returns>Returns the PLC date/time.</returns>
        public DateTime LeDataHoraPLC()
        {
            //Faz as verificações necessárias ao PLC
            if (UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                VerificaPLC();

            DateTime dateTime = DateTime.MinValue;

            if (UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    if (Client.Connected)
                        lock (clientLock)
                        {
                            int res = Client.GetPlcDateTime(ref dateTime);

                            if (res == 0)
                                return dateTime;
                            else
                                throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                        }
                    else
                        throw new Exception("Sem comunicação com PLC");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("LeDataHoraPLC(): " + ex.Message);
                    VerificaPLC();
                    return dateTime;
                }
            else
                return dateTime;
        }

        /// <summary>
        /// Atualiza a Hora de PLC de acordo com a hora do PC
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public bool AtualizaHoraPLC()
        {
            //Faz as verificações necessárias ao PLC
            if (UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                VerificaPLC();

            if (UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    if (Client.Connected)
                        lock (clientLock)
                        {
                            int res = Client.SetPlcSystemDateTime();

                            if (res == 0)
                                return true;
                            else
                                throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                        }
                    else
                        throw new Exception("Sem comunicação com PLC");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AtualizaHoraPLC(): " + ex.Message);
                    VerificaPLC();
                    return false;
                }
            else
                return false;

        }

        /// <summary>
        /// Atualiza a Hora de PLC de acordo com a hora desejada
        /// </summary>
        /// <param name="_dataHora">SP de hora a alterar</param>
        /// <param name="">Forçar atualização do estado do PLC</param>
        /// <returns></returns>
        public bool AtualizaHoraPLC(DateTime _dataHora)
        {
            //Faz as verificações necessárias ao PLC
            if (UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                VerificaPLC();

            if (UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    if (Client.Connected)
                        lock (clientLock)
                        {
                            int res = Client.SetPlcDateTime(_dataHora);

                            if (res == 0)
                                return true;
                            else
                                throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                        }
                    else
                        throw new Exception("Sem comunicação com PLC");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("AtualizaHoraPLC(): " + ex.Message);
                    VerificaPLC();
                    return false;
                }
            else
                return false;

        }
        #endregion

        #endregion

        #region Funções de Escrita
        private int MakeWrite(MemoryArea area, int dbNumber, int offset, byte[] buffer)
        {
            lock (clientLock)
                switch (area)
                {
                    case MemoryArea.DB: return Client.DBWrite(dbNumber, offset, buffer.Length, buffer);
                    case MemoryArea.M: return Client.MBWrite(offset, buffer.Length, buffer);
                    case MemoryArea.I: return Client.EBWrite(offset, buffer.Length, buffer);
                    case MemoryArea.Q: return Client.ABWrite(offset, buffer.Length, buffer);
                }

            return -1;
        }
        private int MakeWriteBit(MemoryArea area, int dbNumber, int offset, byte[] buffer)
        {
            lock (clientLock)
                switch (area)
                {
                    case MemoryArea.DB: return Client.WriteArea(S7Consts.S7AreaDB, dbNumber, offset, buffer.Length, S7Consts.S7WLBit, buffer);
                    case MemoryArea.M: return Client.WriteArea(S7Consts.S7AreaMK, dbNumber, offset, buffer.Length, S7Consts.S7WLBit, buffer);
                    case MemoryArea.I: return Client.WriteArea(S7Consts.S7AreaPE, dbNumber, offset, buffer.Length, S7Consts.S7WLBit, buffer);
                    case MemoryArea.Q: return Client.WriteArea(S7Consts.S7AreaPA, dbNumber, offset, buffer.Length, S7Consts.S7WLBit, buffer);
                }

            return -1;
        }

        /// <summary>
        /// NOTA: No caso das memorias, o dbNumber deverá ser 0 e o endereço é offset (e lenght para os bits)
        /// </summary>
        /// <param name="area"></param>
        /// <param name="tipoVariavel"></param>
        /// <param name="value"></param>
        /// <param name="dbNumber"></param>
        /// <param name="offset"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public bool EnviaTag(MemoryArea area, TipoVariavel tipoVariavel, object value, int dbNumber, int offset, ushort lenght = 0)
        {
            if (this.EstadoLigacao && value != null)
            {
                lock (this.writeLock)
                    this.writeList.Add(new AuxVariable(area, tipoVariavel, dbNumber, offset, lenght, value));
                return true;
            }
            else
                return false;
        }
        public bool EnviaTag(AuxVariable tag)
        {
            if (this.EstadoLigacao && tag != null && tag.WriteMode)
            {
                lock (this.writeLock)
                    this.writeList.Add(tag);
                return true;
            }
            else
                return false;
        }
        public void EnviaPulso(MemoryArea area, int dbNumber, int offset, ushort lenght, int pulseTime, bool startCmd = true)
        {
            Task.Factory.StartNew(() =>
            {
                //faz set RT
                this.EnviaTagRT(area, TipoVariavel.Bool, startCmd, dbNumber, offset, lenght);

                //aguarda o tempo do pulso
                Thread.Sleep(pulseTime);

                //faz reset RT
                this.EnviaTagRT(area, TipoVariavel.Bool, !startCmd, dbNumber, offset, lenght);
            });
        }

        public bool EnviaTagRT(MemoryArea area, TipoVariavel tipoVariavel, object value, int dbNumber, int offset, ushort lenght = 0)
        {
            //Faz as verificações necessárias ao PLC
            if (UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                VerificaPLC();

            if (UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    if (Client.Connected)
                    {
                        if (area == MemoryArea.DB && dbNumber < 0)
                            throw new Exception("Incorrect DB number!");

                        if (offset < 0)
                            throw new Exception("Byte < 0");

                        if (tipoVariavel == TipoVariavel.Bool)
                            if (lenght < 0 || lenght > 7)
                                throw new Exception("Bit < 0 || Bit > 7");

                        byte[] buffer = new byte[ObtemNBytesVariavel(tipoVariavel, lenght)];
                        int res = 0;

                        Siemens.SetValueAt(tipoVariavel, ref buffer, value, lenght);

                        if (tipoVariavel == TipoVariavel.Bool)
                            res = this.MakeWriteBit(area, dbNumber, (offset * 8) + lenght, buffer);
                        else
                            res = this.MakeWrite(area, dbNumber, offset, buffer);

                        if (res == 0)
                            return true;
                        else
                            throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                    }
                    else
                        throw new Exception("Sem comunicação com PLC");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EnviaTagRT(): " + ex.Message);
                    VerificaPLC();
                    return false;
                }
            else
                return false;
        }

        public bool EnviaSequenciaTagsRT(MemoryArea area, WriteMultiVariables[] variaveis, int dbNumber, int startAddress)
        {
            //Faz as verificações necessárias ao PLC
            if (this.UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                this.VerificaPLC();

            if (this.UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    List<byte> buffer = new List<byte>();

                    for (int i = 0; i < variaveis.Length; i++)
                        buffer.AddRange(variaveis[i].data);

                    int res = this.MakeWrite(area, dbNumber, startAddress, buffer.ToArray());

                    if (res == 0)
                        return true;
                    else
                        throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EnviaSequenciaTagsRT(): " + ex.Message);
                    this.VerificaPLC();
                    return false;
                }
            else
                return false;
        }

        public bool EnviaBytesRT(MemoryArea area, byte[] buffer, int dbNumber, int startAddress)
        {
            //Faz as verificações necessárias ao PLC
            if (this.UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                this.VerificaPLC();

            if (this.UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    int res = this.MakeWrite(area, dbNumber, startAddress, buffer.ToArray());

                    if (res == 0)
                        return true;
                    else
                        throw new Exception("Error Code: " + res + " " + Client.ErrorText(res));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("EnviaBytesRT(): " + ex.Message);
                    this.VerificaPLC();
                    return false;
                }
            else
                return false;
        }


        public void LimpaListagemEscrita()
        {
            lock (this.writeLock)
                this.writeList.Clear();
        }

        public bool ProcessaListagemEscrita()
        {
            bool res = this.EstadoLigacao;
            if (this.EstadoLigacao)
                if (NumOfElementsToWrite > 0)
                    lock (this.writeLock)
                    {
                        res = this.MultiWrite(this.writeList.ToArray());
                        this.writeList.Clear();
                    }

            return res;
        }

        public bool MultiWrite(AuxVariable[] variaveis)
        {
            //Faz as verificações necessárias ao PLC
            if (this.UltimoEstadoPLC != EstadoPLC.LigadoComLigacao)
                this.VerificaPLC();

            if (this.UltimoEstadoPLC == EstadoPLC.LigadoComLigacao)
                try
                {
                    List<int> res = new List<int>();
                    int indexTag = 0;
                    byte[][] buffer = new byte[variaveis.Length][];

                    if (variaveis == null)
                        throw new Exception("No Vars to Write!");
                    else if (variaveis.Count() == 0)
                        return true;

                    S7MultiVar vars = new S7MultiVar(this.Client);

                    while (indexTag < variaveis.Length)
                    {
                        if (variaveis[indexTag].WriteMode)
                        {
                            buffer[indexTag] = new byte[ObtemNBytesVariavel(variaveis[indexTag].TipoVariavel, variaveis[indexTag].Lenght)];

                            SetValueAt(variaveis[indexTag].TipoVariavel, ref buffer[indexTag], variaveis[indexTag].Value, variaveis[indexTag].Lenght);

                            if (variaveis[indexTag].TipoVariavel == TipoVariavel.Bool)
                                vars.Add(MemoryAreaToS7Const(variaveis[indexTag].MemoryArea), S7Consts.S7WLBit, variaveis[indexTag].DbNumber, (variaveis[indexTag].Offset * 8) + variaveis[indexTag].Lenght, ObtemNBytesVariavel(variaveis[indexTag].TipoVariavel), ref buffer[indexTag]);
                            else
                                vars.Add(MemoryAreaToS7Const(variaveis[indexTag].MemoryArea), S7Consts.S7WLByte, variaveis[indexTag].DbNumber, variaveis[indexTag].Offset, ObtemNBytesVariavel(variaveis[indexTag].TipoVariavel), ref buffer[indexTag]);

                        }

                        indexTag++;

                        if (vars.Count == S7Client.MaxVars || (indexTag == variaveis.Length))
                        {
                            lock (this.clientLock)
                                res.Add(vars.Write());
                            vars.Clear();
                        }
                    }

                    for (int i = 0; i < res.Count; i++)
                        if (res[i] != 0)
                            throw new Exception("Error Code: " + res[i] + " " + Client.ErrorText(res[i]));

                    // Debug.WriteLine("MultiWrite(): Total Tags - " + indexTag.ToString() + "| Nº of writes - " + res.Count.ToString());

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MultiWrite(): " + ex.Message);
                    this.VerificaPLC();
                    return false;
                }
            else
                return false;
        }

 

        #endregion

        #region Funções Auxiliares

        /// <summary>
        /// Efetua a contagem do tempo de ciclo
        /// </summary>
        public void IniciaContagemTempoCiclo()
        {
            timeCounter.Restart();
        }

        /// <summary>
        /// Termina a contagem do tempo de ciclo
        /// </summary>
        public void TerminaContagemTempoCiclo()
        {
            timeCounter.Stop();

            if (!this.disconnectDone)
                this.TemposCiclo.UltimoValor(timeCounter.ElapsedMilliseconds);
            else
            {
                this.TemposCiclo.ResetContador();

                //Limpar o sinal RestartDone
                this.disconnectDone = false;
            }
        }

        /// <summary>
        /// Executa um ping ao IP especificado
        /// </summary>
        /// <param name="_enderecoIP">Retorna TRUE se ping for efetuada, FALSE se não</param>
        /// <param name="timeout">Tempo em MS de timout ao ping</param>
        /// <param name="mostraInfoDebug">Mostrar informação de ping</param>
        /// <returns></returns>
        public bool Ping(string _enderecoIP, int timeout = 5000, bool mostraInfoDebug = false)
        {
            try
            {
                using (Ping pingSender = new Ping())
                {
                    IPAddress ip = IPAddress.Parse(_enderecoIP);

                    PingReply reply = pingSender.Send(ip, timeout);

                    if (reply.Status == IPStatus.Success)
                    {
                        if (mostraInfoDebug)
                        {
                            Debug.WriteLine("Função Ping-> IP Address: " + reply.Address.ToString());
                            Debug.WriteLine("Função Ping-> Trip Time : " + reply.RoundtripTime + "ms");
                            Debug.WriteLine("Função Ping-> Time to live: " + reply.Options.Ttl);
                            Debug.WriteLine("Função Ping-> Don't fragment: " + reply.Options.DontFragment);
                            Debug.WriteLine("Função Ping-> Buffer Size: " + reply.Buffer.Length);
                        }
                        return true;
                    }
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                if (mostraInfoDebug)
                    Debug.WriteLine(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// Verifica se o PLC está disponível, se não estiver e a comunicação estiver aberta, fehca a mesma. Se tiver fechada e o PLC está disponível, abre a comunicação com o mesmo
        /// </summary>
        public EstadoPLC VerificaPLC()
        {
            try
            {
                if (Client == null)
                    ConnectToPLC();

                if (Ping(EnderecoIP.ToString(), 1000))
                    if (!Client.Connected)
                        if (ConnectToPLC())
                            UltimoEstadoPLC = EstadoPLC.LigadoComLigacao;
                        else
                            UltimoEstadoPLC = EstadoPLC.LigadoSemLigacao;
                    else
                        UltimoEstadoPLC = EstadoPLC.LigadoComLigacao;
                else
                    if (Client.Connected)
                    if (!Ping(EnderecoIP.ToString(), 1000)) //Volta a fazer 2nd Ping de Confirmação, por ex. o 1st não responde e mandava a comunicação abaixo
                        if (DisconnectFromPLC())
                            UltimoEstadoPLC = EstadoPLC.DesligadoSemLigacao;
                        else
                            UltimoEstadoPLC = EstadoPLC.DesligadoComLigacao;
                    else
                        UltimoEstadoPLC = EstadoPLC.LigadoComLigacao;
                else
                    UltimoEstadoPLC = EstadoPLC.DesligadoSemLigacao;

                return UltimoEstadoPLC;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return UltimoEstadoPLC;
            }
        }

        /// <summary>
        /// Determina o estado da comunicação e do PLC
        /// </summary>
        public enum EstadoPLC
        {
            /// <summary>
            /// Ligado Fisicamente mas sem Ligaçao LibNoDave
            /// </summary>
            LigadoSemLigacao,
            /// <summary>
            /// Ligado Fisicamente e com Ligaçao LibNoDave
            /// </summary>
            LigadoComLigacao,
            /// <summary>
            /// Desligado Fisicamente mas e sem Ligaçao LibNoDave
            /// </summary>
            DesligadoSemLigacao,
            /// <summary>
            /// Desligado Fisicamente e com Ligaçao LibNoDave
            /// </summary>
            DesligadoComLigacao
        }

        public enum MemoryArea
        {
            DB = 0, M = 1, I = 2, Q = 3
        }

        public enum TipoVariavel
        {
            Bool, Byte, Sint, USint, Char, //1
            Int, UInt, Word, Date, //2
            DInt, UDInt, Real, DWord, Time_Of_Day, //4
            LInt, ULInt, LReal, LWord, LTime_Of_Day, LDT, Date_And_Time, //8
            DTL, //12
            String //256
        }

        public static int MemoryAreaToS7Const(MemoryArea area)
        {
            switch (area)
            {
                case MemoryArea.DB:
                    return S7Consts.S7AreaDB;
                case MemoryArea.M:
                    return S7Consts.S7AreaMK;
                case MemoryArea.I:
                    return S7Consts.S7AreaPE;
                case MemoryArea.Q:
                    return S7Consts.S7AreaPA;
                default:
                    return 0;
            }
        }

        public static int ObtemNBytesVariavel(TipoVariavel _tipoVar, ushort _lenght = 0)
        {
            switch (_tipoVar)
            {
                case TipoVariavel.Bool:
                case TipoVariavel.Byte:
                case TipoVariavel.Sint:
                case TipoVariavel.USint:
                    return 1;

                case TipoVariavel.Char:
                    return _lenght > 0 ? _lenght : 1;

                case TipoVariavel.Int:
                case TipoVariavel.UInt:
                case TipoVariavel.Word:
                case TipoVariavel.Date:
                    return 2;

                case TipoVariavel.DInt:
                case TipoVariavel.UDInt:
                case TipoVariavel.Real:
                case TipoVariavel.DWord:
                case TipoVariavel.Time_Of_Day:
                    return 4;

                case TipoVariavel.LInt:
                case TipoVariavel.ULInt:
                case TipoVariavel.LReal:
                case TipoVariavel.LWord:
                case TipoVariavel.LTime_Of_Day:
                case TipoVariavel.LDT:
                case TipoVariavel.Date_And_Time:
                    return 8;


                case TipoVariavel.DTL:
                    return 12;

                case TipoVariavel.String:
                    {
                        int count = _lenght > 0 ? (_lenght + 2) : 256;

                        if (count % 2 != 0)
                            count++; //adiciona o 'byte' fantasma no final
                        return count;

                    }
                default: return 0;
            }

        }

        public static dynamic GetValueAt(TipoVariavel tipoVar, byte[] data, int posBit = 0)
        {
            switch (tipoVar)
            {
                case TipoVariavel.Bool: return S7.GetBitAt(data, 0, posBit);
                case TipoVariavel.Byte: return S7.GetByteAt(data, 0);
                case TipoVariavel.Sint: return S7.GetSIntAt(data, 0);
                case TipoVariavel.USint: return S7.GetUSIntAt(data, 0);
                case TipoVariavel.Char: return S7.GetCharsAt(data, 0, data.Length);
                case TipoVariavel.Int: return S7.GetIntAt(data, 0);
                case TipoVariavel.UInt: return S7.GetUIntAt(data, 0);
                case TipoVariavel.Word: return S7.GetWordAt(data, 0);
                case TipoVariavel.Date: return S7.GetDateAt(data, 0);
                case TipoVariavel.DInt: return S7.GetDIntAt(data, 0);
                case TipoVariavel.UDInt: return S7.GetUDIntAt(data, 0);
                case TipoVariavel.Real: return S7.GetRealAt(data, 0);
                case TipoVariavel.DWord: return S7.GetDWordAt(data, 0);
                case TipoVariavel.Time_Of_Day: return S7.GetTODAt(data, 0);
                case TipoVariavel.LInt: return S7.GetLIntAt(data, 0);
                case TipoVariavel.ULInt: return S7.GetULIntAt(data, 0);
                case TipoVariavel.LReal: return S7.GetLRealAt(data, 0);
                case TipoVariavel.LWord: return S7.GetLWordAt(data, 0);
                case TipoVariavel.LTime_Of_Day: return S7.GetLTODAt(data, 0);
                case TipoVariavel.LDT: return S7.GetLDTAt(data, 0);
                case TipoVariavel.Date_And_Time: return S7.GetDateTimeAt(data, 0);
                case TipoVariavel.DTL: return S7.GetDTLAt(data, 0);
                case TipoVariavel.String: return S7.GetStringAt(data, 0);
                default: return null;
            }
        }

        public static void SetValueAt(TipoVariavel tipoVariavel, ref byte[] buffer, object value, int lenght = 0)
        {
            switch (tipoVariavel)
            {
                case TipoVariavel.Bool: S7.SetBitAt(ref buffer, 0, lenght, Convert.ToBoolean(value)); break;
                case TipoVariavel.Byte: S7.SetByteAt(buffer, 0, Convert.ToByte(value)); break;
                case TipoVariavel.Sint: S7.SetSIntAt(buffer, 0, Convert.ToSByte(value)); break;
                case TipoVariavel.USint: S7.SetUSIntAt(buffer, 0, Convert.ToByte(value)); break;
                case TipoVariavel.Char: S7.SetCharsAt(buffer, 0, Convert.ToString(value)); break;
                case TipoVariavel.Int: S7.SetIntAt(buffer, 0, Convert.ToInt16(value)); break;
                case TipoVariavel.UInt: S7.SetUIntAt(buffer, 0, Convert.ToUInt16(value)); break;
                case TipoVariavel.Word: S7.SetWordAt(buffer, 0, Convert.ToUInt16(value)); break;
                case TipoVariavel.Date: S7.SetDateAt(buffer, 0, Convert.ToDateTime(value)); break;
                case TipoVariavel.DInt: S7.SetDIntAt(buffer, 0, Convert.ToInt32(value)); break;
                case TipoVariavel.UDInt: S7.SetUDIntAt(buffer, 0, Convert.ToUInt32(value)); break;
                case TipoVariavel.Real: S7.SetRealAt(buffer, 0, Convert.ToSingle(value)); break;
                case TipoVariavel.DWord: S7.SetDWordAt(buffer, 0, Convert.ToUInt32(value)); break;
                case TipoVariavel.Time_Of_Day: S7.SetTODAt(buffer, 0, Convert.ToDateTime(value)); break;
                case TipoVariavel.LInt: S7.SetLIntAt(buffer, 0, Convert.ToInt64(value)); break;
                case TipoVariavel.ULInt: S7.SetULintAt(buffer, 0, Convert.ToUInt64(value)); break;
                case TipoVariavel.LReal: S7.SetLRealAt(buffer, 0, Convert.ToDouble(value)); break;
                case TipoVariavel.LWord: S7.SetLWordAt(buffer, 0, Convert.ToUInt64(value)); break;
                case TipoVariavel.LTime_Of_Day: S7.SetLTODAt(buffer, 0, Convert.ToDateTime(value)); break;
                case TipoVariavel.LDT: S7.SetLDTAt(buffer, 0, Convert.ToDateTime(value)); break;
                case TipoVariavel.Date_And_Time: S7.SetDateTimeAt(buffer, 0, Convert.ToDateTime(value)); break;
                case TipoVariavel.DTL: S7.SetDTLAt(buffer, 0, Convert.ToDateTime(value)); break;
                case TipoVariavel.String: S7.SetStringAt(buffer, 0, lenght, Convert.ToString(value)); break;
                default: throw new Exception("Without defined data type!");
            }
        }

        public static byte BitsToByte(bool bit0, bool bit1 = false, bool bit2 = false, bool bit3 = false, bool bit4 = false, bool bit5 = false, bool bit6 = false, bool bit7 = false)
        {
            byte[] buffer = new byte[1];
            S7.SetBitAt(ref buffer, 0, 0, bit0);
            S7.SetBitAt(ref buffer, 0, 1, bit1);
            S7.SetBitAt(ref buffer, 0, 2, bit2);
            S7.SetBitAt(ref buffer, 0, 3, bit3);
            S7.SetBitAt(ref buffer, 0, 4, bit4);
            S7.SetBitAt(ref buffer, 0, 5, bit5);
            S7.SetBitAt(ref buffer, 0, 6, bit6);
            S7.SetBitAt(ref buffer, 0, 7, bit7);

            return buffer[0];
        }

        #endregion

        #region Classes Auxiliares

        public class AuxVariable
        {
            public MemoryArea MemoryArea { get; private set; } = MemoryArea.DB;
            public TipoVariavel TipoVariavel { get; private set; } = TipoVariavel.Bool;
            public int DbNumber { get; private set; } = 0;
            public int Offset { get; private set; } = 0;
            public ushort Lenght { get; private set; } = 0;

            public object Value { get; private set; } = null;

            public bool ReadMode
            {
                get
                {
                    return this.Value == null;
                }
            }
            public bool WriteMode
            {
                get
                {
                    return !this.ReadMode;
                }
            }

            public string TagName { get; set; } = string.Empty;

            public AuxVariable(MemoryArea area, TipoVariavel tipoVar, int dbNumber, int offset, ushort lenght = 0, object value = null, string tagName = "")
            {
                this.MemoryArea = area;
                this.TipoVariavel = tipoVar;
                this.DbNumber = dbNumber;
                this.Offset = offset;
                this.Value = value;

                this.TagName = tagName;

                switch (this.TipoVariavel)
                {
                    case TipoVariavel.String:
                        {
                            this.Lenght = (ushort)(lenght > 0 ? lenght : 256);
                            break;
                        }
                    case TipoVariavel.Char:
                        {
                            this.Lenght = (ushort)(lenght > 0 ? lenght : 1);
                            break;
                        }
                    case TipoVariavel.Bool:
                        {
                            if (lenght < 0)
                                this.Lenght = 0;
                            else if (lenght > 7)
                                this.Lenght = 7;
                            else
                                this.Lenght = lenght;
                            break;
                        }
                    default:
                        {
                            this.Lenght = 0;
                            break;
                        }
                }
            }
        }

        public class ReadMultiVariables
        {
            public ushort ByteCount
            {
                get
                {
                    return (ushort)(this.data != null ? this.data.Length : 0);
                }
            }

            private byte[] data = null;
            private TipoVariavel tipoVariavel = TipoVariavel.Bool;

            public ReadMultiVariables(TipoVariavel _tipoVar, ushort _lenght = 0)
            {
                this.tipoVariavel = _tipoVar;

                this.data = new byte[ObtemNBytesVariavel(this.tipoVariavel, _lenght)];
            }

            public void LeVariavel(byte[] _buffer, int _startAddress)
            {
                Array.Copy(_buffer, _startAddress, this.data, 0, this.data.Length);
            }

            public dynamic ObtemVariavel(int posBit = 0)
            {
                return (this.data != null) ? GetValueAt(this.tipoVariavel, this.data, posBit) : null;
            }
        }

        public class WriteMultiVariables
        {
            public ushort ByteCount
            {
                get
                {
                    return (ushort)(this.data != null ? this.data.Length : 0);
                }
            }

            public byte[] data = null;

            public WriteMultiVariables(TipoVariavel tipoVar, object value, ushort lenght = 0)
            {
                this.data = new byte[ObtemNBytesVariavel(tipoVar, lenght)];
                Siemens.SetValueAt(tipoVar, ref this.data, value, lenght);
            }

        }

        /// <summary>
        /// Class do Ficheiro INI
        /// </summary>
        private class Ini : IDisposable
        {
            public string path;

            [DllImport("kernel32")]
            private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
            [DllImport("kernel32")]
            private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

            /// <summary>
            /// INIFile Constructor.
            /// </summary>
            /// <PARAM name="INIPath"></PARAM>
            public Ini(string _path)
            {
                //Alteramos todas as \ que possam estar ao contrário
                _path.Replace(@"/", @"\");
                //Associação do caminho
                path = _path;
            }

            /// <summary>
            /// Write Data to the INI File
            /// </summary>
            /// <PARAM name="Section"></PARAM>
            /// Section name
            /// <PARAM name="Key"></PARAM>
            /// Key Name
            /// <PARAM name="Value"></PARAM>
            /// Value Name
            public void EscreveFicheiroINI(string Secao, string Campo, string Valor)
            {
                WritePrivateProfileString(Secao, Campo, Valor, this.path);
            }

            /// <summary>
            /// Read Data Value From the Ini File
            /// </summary>
            /// <PARAM name="Section"></PARAM>
            /// <PARAM name="Key"></PARAM>
            /// <PARAM name="Path"></PARAM>
            /// <returns></returns>
            private string LeFicheiroINI(string Secao, string Campo)
            {
                StringBuilder temp = new StringBuilder(255);
                int i = GetPrivateProfileString(Secao, Campo, "", temp, 255, this.path);
                return temp.ToString();
            }

            /// <summary>
            ///  //Retorna true /false apartir de uma string 0/1
            /// </summary>
            /// <param name="seccao"></param>
            /// <param name="campo"></param>
            /// <param name="valorDefeito"></param>
            /// <returns></returns>
            public bool RetornaTrueFalseDeStringFicheiroINI(string seccao, string campo, bool valorDefeito)
            {
                try
                {
                    if (LeFicheiroINI(seccao, campo) == "")
                        EscreveFicheiroINI(seccao, campo, Convert.ToString(Convert.ToInt32(valorDefeito)));

                    switch (Convert.ToInt32(LeFicheiroINI(seccao, campo)))
                    {
                        case 0:
                            return false;
                        case 1:
                            return true;
                        default:
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("RetornaTrueFalseDeStringFicheiroINI(): " + ex.Message);
                    EscreveFicheiroINI(seccao, campo, Convert.ToString(Convert.ToInt32(valorDefeito)));
                    return valorDefeito;
                }

            }

            /// <summary>
            /// Retorna texto de um ficheiro INI e caso o campo nao exista no ficheiro cria-o
            /// </summary>
            /// <param name="_seccao"></param>
            /// <param name="_campo"></param>
            /// <param name="_strAPrencherCasoNulo"></param>
            /// <returns></returns>
            public string RetornaINI(string _seccao, string _campo, string _strAPrencherCasoNulo = "")
            {
                if (LeFicheiroINI(_seccao, _campo) != "")
                    return Convert.ToString(LeFicheiroINI(_seccao, _campo));
                else
                    if (_strAPrencherCasoNulo != "")
                {
                    EscreveFicheiroINI(_seccao, _campo, _strAPrencherCasoNulo);
                    return _strAPrencherCasoNulo;
                }
                else
                {
                    EscreveFicheiroINI(_seccao, _campo, "0");
                    return "0";
                }
            }

            #region Disposing Methods

            private bool disposed = false; // to detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                try
                {
                    if (!disposed)
                    {
                        if (disposing)
                            GC.Collect();

                        disposed = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }

            public void Dispose()
            {
                Dispose(true);
            }
            #endregion

        }

        private class GereTempos
        {
            public int TempoAtual { get; private set; } = 0;
            public int TempoMaximo { get; private set; } = 0;
            public int TempoMinimo { get; private set; } = int.MaxValue;
            public int TempoMedio
            {
                get
                {
                    return mediaValores.Count > 0 ? Convert.ToInt32(mediaValores.Average()) : TempoAtual;
                }
            }

            private int amostragemMedia = 0;
            private List<int> mediaValores = new List<int>();

            public GereTempos(int _amostragemMedia)
            {
                amostragemMedia = _amostragemMedia;
            }

            public void UltimoValor(long value)
            {
                if (value < Int32.MaxValue)
                    TempoAtual = Convert.ToInt32(value);
                else
                    TempoAtual = Int32.MaxValue;

                if (TempoAtual > TempoMaximo)
                    TempoMaximo = TempoAtual;

                if (TempoAtual < TempoMinimo)
                    TempoMinimo = TempoAtual;

                if (amostragemMedia > 0)
                {
                    mediaValores.Add(TempoAtual);

                    if (mediaValores.Count > amostragemMedia)
                        mediaValores.RemoveRange(0, mediaValores.Count - amostragemMedia);
                }
            }

            public void ResetContador()
            {
                mediaValores.Clear();
                TempoAtual = 0;
                TempoMinimo = int.MaxValue;
                TempoMaximo = 0;
            }
        }

        #endregion

    }
}
