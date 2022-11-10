
using PLC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;



namespace _22079AI
{
    public class PlcSendRcv
    {
        public Siemens PLC1;
        List<PLC.Siemens.ReadMultiVariables> ArrayIn = new List<Siemens.ReadMultiVariables>();
        List<PLC.Siemens.WriteMultiVariables> ArrayOut = new List<Siemens.WriteMultiVariables>();
        public VarsAuxiliares VariaveisAuxiliares = new VarsAuxiliares();
        public int index = new int();
        public double CycleTime, leituras, escritas, criacaolistagem1, criacaolistagem2;
        public DateTime Startdate, auxdt;
        private readonly object PublicVarLock = new object();

        private bool FirstConetion = new bool();

        //variaveis a serem lidas e escritas para controlo do PLC
        public Disc HmiPlcNewDisc = new Disc(), HmiPlcFeedbackdisc = new Disc(), PlcHmiNewDisc = new Disc(), PlcHmiFeedbackdisc = new Disc();
        public Tapetesin StaTapetes = new Tapetesin();
        public TapetesOut CmdTapetes = new TapetesOut();
        public Vibradoresin StaVibrador = new Vibradoresin();
        public VibradoresOut CmdVibrador = new VibradoresOut();
        public Cicloin StaCiclo = new Cicloin();
        public CicloOut CmdCiclo = new CicloOut();
        public Inputs Dis = new Inputs();
        public Outputs Dos = new Outputs();
        public bool HmiNewDiscRead { get 
            {
                return (HmiPlcFeedbackdisc.ID != PlcHmiNewDisc.ID);
             }
             set { } 
        }
        public bool PlcReadyForNewDIsc
        {
            get
            {
                return (HmiPlcNewDisc.ID == PlcHmiFeedbackdisc.ID);
            }
            set { }
        }
        //Variaveis privadas de interface com PLC
        private Disc _HmiPlcNewDisc = new Disc(), _HmiPlcFeedbackdisc = new Disc(), _PlcHmiNewDisc = new Disc(), _PlcHmiFeedbackdisc = new Disc();
        private Tapetesin _StaTapetes = new Tapetesin();
        private TapetesOut _CmdTapetes = new TapetesOut();
        private Vibradoresin _StaVibrador = new Vibradoresin();
        private VibradoresOut _CmdVibrador = new VibradoresOut();
        private Cicloin _StaCiclo = new Cicloin();
        private CicloOut _CmdCiclo = new CicloOut();
        private Inputs _Dis = new Inputs();
        private Outputs _Dos = new Outputs();
        private bool SendRequest = new bool();
        private DateTime LastWriteTime;
        private double MinComTime = 1500;

        #region ReadWritePlc

        /*
        quando o plc enviar um novo disco vamos ter esta condicao
        if HmiPlcFeedbackdisc.ID<>PlcHmiNewDisc.ID
         HmiPlcFeedbackdisc.ID=PlcHmiNewDisc

        quando o plc consome discos apos inpecao
        if PlcHmiFeedbackdisc.ID=HmiPlcNewDisc.ID
         //hmi envia proximo disco
         */

        public void WriteReadPlc() {
            var tasks = new Task[3];

            
            PLC1 = new Siemens(VariaveisAuxiliares.iniPath);

                FirstConetion = false;
                //Limpa listagens de variaveis
                ArrayIn.Clear();
                ArrayOut.Clear();
                index = 0;
                //multiplexa listagens de saida de variaveis
                _HmiPlcNewDisc.CreatReadList(ArrayIn);
                _HmiPlcFeedbackdisc.CreatReadList(ArrayIn);
                _CmdTapetes.CreatReadList(ArrayIn);
                _CmdVibrador.CreatReadList(ArrayIn);
                _CmdCiclo.CreatReadList(ArrayIn);
                //Le chunk de memoria do PLC para a listagem
                PLC1.LeSequenciaTags(Siemens.MemoryArea.DB, ArrayIn.ToArray(), 401, 0);
                index = _HmiPlcNewDisc.ReadVariables(ArrayIn, index);
                index = _HmiPlcFeedbackdisc.ReadVariables(ArrayIn, index);
                index = _CmdTapetes.ReadVariables(ArrayIn, index);
                index = _CmdVibrador.ReadVariables(ArrayIn, index);
                index = _CmdCiclo.ReadVariables(ArrayIn, index);
                HmiPlcNewDisc = _HmiPlcNewDisc;
                HmiPlcFeedbackdisc = _HmiPlcFeedbackdisc;
                CmdTapetes = _CmdTapetes;
                CmdVibrador = _CmdVibrador;
                CmdCiclo = _CmdCiclo;
                FirstConetion = true;

            while (FirstConetion) {

                try
                {
                    //inicializa contador de tempo
                    Startdate = DateTime.Now;

                    Task Recieve = Task.Run(() =>
                    {
                        //Limpa listagens de variaveis
                        ArrayIn.Clear();
                        //multiplexa variaveis para lista
                        _PlcHmiNewDisc.CreatReadList(ArrayIn);
                        _PlcHmiFeedbackdisc.CreatReadList(ArrayIn);
                        _StaTapetes.CreatReadList(ArrayIn);
                        _StaVibrador.CreatReadList(ArrayIn);
                        _StaCiclo.CreatReadList(ArrayIn);
                        _Dis.CreatReadList(ArrayIn);
                        _Dos.CreatReadList(ArrayIn);
                        //Le chunk de memoria do PLC para a listagem
                        PLC1.LeSequenciaTags(Siemens.MemoryArea.DB, ArrayIn.ToArray(), 400, 0);
                    }
                    );
                    Recieve.Wait();

                    //Calculo final do tempo de ciclo
                    leituras = (DateTime.Now - Startdate).TotalMilliseconds;
                     auxdt = DateTime.Now;
                    


                    index = 0;
                    index=_PlcHmiNewDisc.ReadVariables(ArrayIn, index);
                    index = _PlcHmiFeedbackdisc.ReadVariables(ArrayIn, index);
                    index = _StaTapetes.ReadVariables(ArrayIn, index);
                    index = _StaVibrador.ReadVariables(ArrayIn, index);
                    index = _StaCiclo.ReadVariables(ArrayIn, index);
                    index = _Dis.ReadVariables(ArrayIn, index);
                    index = _Dos.ReadVariables(ArrayIn, index);

                   // HmiPlcNewDisc.ID = HmiPlcNewDisc.ID + 1;
                    //if (HmiPlcNewDisc.ID > 100)
                    //    HmiPlcNewDisc.ID = 0;

                    //passa valor para veriaveis globais
                    //Inicializa valores dos outputs
                    lock (PublicVarLock) {

                    SendRequest = HmiPlcNewDisc.Equals(_HmiPlcFeedbackdisc) && HmiPlcFeedbackdisc.Equals(_HmiPlcFeedbackdisc) && CmdTapetes.Equals(_CmdTapetes) && CmdCiclo.Equals(_CmdCiclo);

                    _HmiPlcNewDisc = HmiPlcNewDisc;
                    _HmiPlcFeedbackdisc = HmiPlcFeedbackdisc;
                    _CmdTapetes = CmdTapetes;
                    _CmdVibrador = CmdVibrador;
                    _CmdCiclo = CmdCiclo;

                         
                    PlcHmiNewDisc = _PlcHmiNewDisc;
                    PlcHmiFeedbackdisc = _PlcHmiFeedbackdisc;
                    StaTapetes = _StaTapetes;
                    StaVibrador= _StaVibrador;
                    StaCiclo = _StaCiclo;
                    Dis = _Dis;
                    Dos = _Dos;
                    }

                    if (!(SendRequest)) { 
                    Task Send = Task.Run(() =>
                    {
                        //multiplexa listagens de saida de variaveis
                        ArrayOut.Clear();
                        _HmiPlcNewDisc.WriteVariables(ArrayOut);
                        _HmiPlcFeedbackdisc.WriteVariables(ArrayOut);
                        _CmdTapetes.WriteVariables(ArrayOut);
                        _CmdVibrador.WriteVariables(ArrayOut);
                        _CmdCiclo.WriteVariables(ArrayOut);
                        //Envia chunk de memoria do PLC
                        PLC1.EnviaSequenciaTagsRT(Siemens.MemoryArea.DB, ArrayOut.ToArray(), 401, 0);
                        LastWriteTime = DateTime.Now;
                        //desmultiplexa listagens de entrada em variaveis de utilizador
                    }
                   );
                    Send.Wait();
                    }
                    //Calculo final do tempo de ciclo
                    CycleTime = (DateTime.Now - Startdate).TotalMilliseconds;
                }
                catch
                {

                    FirstConetion = false;
                }
        
               
              


            }
            }
        }
        

   
    #endregion


    #region PLC structs

    //classe de informação sobre o disco
    public class Disc
        {
        public double ID;
        public double PulseEncoderS1;
        public double PulseEncoderS2;
        public double PulseExpel;
        public double DelayExpel;
        public short result;
        public short reserved_0;
        public short reserved_1;
        public short reserved_2;
        public short reserved_3;
        public short reserved_4;
        public short reserved_5;
        public short reserved_6;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer )
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, ID ,0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, PulseEncoderS1, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, PulseEncoderS2, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, PulseExpel, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, DelayExpel, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, result, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, reserved_0, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, reserved_1, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, reserved_2, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, reserved_3, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, reserved_4, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, reserved_5, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, reserved_6, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {

            ID = Convert.ToDouble(buffer[index].ObtemVariavel());
            index++;
            PulseEncoderS1 = buffer[index].ObtemVariavel();
            index++;
            PulseEncoderS2 = buffer[index].ObtemVariavel();
            index++;
            PulseExpel = buffer[index].ObtemVariavel();
            index++;
            DelayExpel = buffer[index].ObtemVariavel();
            index++;
            result = buffer[index].ObtemVariavel();
            index++;
            reserved_0 = buffer[index].ObtemVariavel();
            index++;
            reserved_1 = buffer[index].ObtemVariavel();
            index++;
            reserved_2 = buffer[index].ObtemVariavel();
            index++;
            reserved_3 = buffer[index].ObtemVariavel();
            index++;
            reserved_4 = buffer[index].ObtemVariavel();
            index++;
            reserved_5 = buffer[index].ObtemVariavel();
            index++;
            reserved_6 = buffer[index].ObtemVariavel();
            index++;

            return index;
        }
        
  

    }

    public class Inputs
        {

        public bool Di0;
        public bool Di1;
        public bool Di2;
        public bool Di3;
        public bool Di4;
        public bool Di5;
        public bool Di6;
        public bool Di7;
        public bool Di8;
        public bool Di9;
        public bool Di10;
        public bool Di11;
        public bool Di12;
        public bool Di13;
        public bool Di14;
        public bool Di15;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Di0, Di1, Di2, Di3, Di4, Di5, Di6, Di7);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Di8, Di9, Di10, Di11, Di12, Di13, Di14, Di15);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[]  Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel()) ;
            Di0 = (auxByte & Mask[bit])!=0;
            bit = bit + 1;
            Di1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Di8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di11 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di12 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di13 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di14 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Di15 = (auxByte & Mask[bit]) != 0;
            index = index + 1;

            return index;
        }
    }

    public class Outputs
    {

        public bool Do0;
        public bool Do1;
        public bool Do2;
        public bool Do3;
        public bool Do4;
        public bool Do5;
        public bool Do6;
        public bool Do7;
        public bool Do8;
        public bool Do9;
        public bool Do10;
        public bool Do11;
        public bool Do12;
        public bool Do13;
        public bool Do14;
        public bool Do15;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Do0, Do1, Do2, Do3, Do4, Do5, Do6, Do7);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Do8, Do9, Do10, Do11, Do12, Do13, Do14, Do15);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Do0 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Do8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do11 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do12 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do13 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do14 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Do15 = (auxByte & Mask[bit]) != 0;
            index = index + 1;

            return index;
        }
    }

    public class Tapetesin
    {
        public bool Enabled;
        public bool Running;
        public bool Fault;
        public bool Reserved_1;
        public bool Reserved_2;
        public bool Reserved_3;
        public bool Reserved_4;
        public bool Reserved_5;
        public byte Reserved_6;
        public int actspeed;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Int, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Enabled, Running, Fault, Reserved_1, Reserved_2, Reserved_3, Reserved_4, Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Reserved_6, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Int, actspeed, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Enabled = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Running = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Fault = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_5 = (auxByte & Mask[bit]) != 0;
            index = index + 1;
            Reserved_6 = (buffer[index].ObtemVariavel());
            index = index + 1;
            actspeed = buffer[index].ObtemVariavel();
            index = index + 1;

            return index;
        }
    }

    public class TapetesOut
    {
        public bool Enable;
        public bool Run;
        public bool Reset;
        public bool Reserved_1;
        public bool Reserved_2;
        public bool Reserved_3;
        public bool Reserved_4;
        public bool Reserved_5;
        public byte Reserved_6;
        public int setspeed;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Int, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Enable, Run, Reset, Reserved_1, Reserved_2, Reserved_3, Reserved_4, Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Reserved_6, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Int, setspeed, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Enable = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Run = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reset = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_5 = (auxByte & Mask[bit]) != 0;
            index = index + 1;
            Reserved_6 = (buffer[index].ObtemVariavel());
            index = index + 1;
            setspeed = buffer[index].ObtemVariavel();
            index = index + 1;

            return index;
        }
    }

    public class Vibradoresin
    {
        public bool Enabled;
        public bool Running;
        public bool Fault;
        public bool Reserved_1;
        public bool Reserved_2;
        public bool Reserved_3;
        public bool Reserved_4;
        public bool Reserved_5;
        public byte Reserved_6;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));


        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Enabled, Running, Fault, Reserved_1, Reserved_2, Reserved_3, Reserved_4, Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = Reserved_6;
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));


        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Enabled = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Running = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Fault = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            Reserved_6 = (buffer[index].ObtemVariavel());
            return index;
        }
    }

    public class VibradoresOut
    {
        public bool Enable;
        public bool Run;
        public bool Reset;
        public bool Reserved_1;
        public bool Reserved_2;
        public bool Reserved_3;
        public bool Reserved_4;
        public bool Reserved_5;
        public byte Reserved_6;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Enable, Run, Reset, Reserved_1, Reserved_2, Reserved_3, Reserved_4, Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Reserved_6, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Enable = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Run = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reset = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            Reserved_6 = (buffer[index].ObtemVariavel());
            index = index + 1;

            return index;
        }
    }

    public class Cicloin
    {
        public bool InManual;
        public bool InAuto;
        public bool CycleOn;
        public bool ReadyForAuto;
        public bool TapeteReady;
        public bool VibradorReady;
        public bool AlimentadorReady;
        public bool EmergencyOk;
        public bool Reserved_5;
        public bool Reserved_6;
        public bool Reserved_7;
        public bool Reserved_8;
        public bool Reserved_9;
        public bool Reserved_10;
        public bool Reserved_11;
        public bool Reserved_12;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(InManual, InAuto, CycleOn, ReadyForAuto, TapeteReady, VibradorReady, AlimentadorReady, EmergencyOk);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Reserved_5, Reserved_6, Reserved_7, Reserved_8, Reserved_9, Reserved_10, Reserved_11, Reserved_12);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            InManual = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            InAuto = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            CycleOn = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            ReadyForAuto = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            TapeteReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            VibradorReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            AlimentadorReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            EmergencyOk = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_11 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_12 = (auxByte & Mask[bit]) != 0;
            index = index + 1;


            return index;
        }
    }

    public class CicloOut
    {
        public bool ManualRequest;
        public bool AutoRequest;
        public bool Stop;
        public bool Start;
        public bool Reserved_0;
        public bool Reserved_1;
        public bool Reserved_2;
        public bool Reserved_3;
        public bool Reserved_4;
        public bool Reserved_5;
        public bool Reserved_6;
        public bool Reserved_7;
        public bool Reserved_8;
        public bool Reserved_9;
        public bool Reserved_10;
        public bool Reserved_11;

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer) 
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(ManualRequest, AutoRequest, Stop, Start, Reserved_0, Reserved_1, Reserved_2, Reserved_3);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Reserved_4, Reserved_5, Reserved_6, Reserved_7, Reserved_8, Reserved_9, Reserved_10, Reserved_11);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            ManualRequest = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            AutoRequest = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Stop = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Start = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_0 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Reserved_11 = (auxByte & Mask[bit]) != 0;
            index = index + 1;

            return index;
        }
    }
    #endregion

}
