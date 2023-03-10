
using PLC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;

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
        

        private bool isAlive = true;

        private bool FirstConetion = new bool();

        //variaveis a serem lidas e escritas para controlo do PLC
        public DiscCom HmiPlcNewDisc = new DiscCom(), HmiPlcFeedbackdisc = new DiscCom(), PlcHmiNewDisc = new DiscCom(), PlcHmiFeedbackdisc = new DiscCom();
        public Tapetesin StaTapetes = new Tapetesin(), StaRotativo = new Tapetesin();
        public TapetesOut CmdTapetes = new TapetesOut(), CmdRotativo = new TapetesOut();
        public Vibradoresin StaVibrador = new Vibradoresin();
        public VibradoresOut CmdVibrador = new VibradoresOut();
        public Cicloin StaCiclo = new Cicloin();
        public CicloOut CmdCiclo = new CicloOut();
        public CicloOut CmdCicloDummy = new CicloOut();
        public Inputs Dis = new Inputs();
        public Outputs Dos = new Outputs();
        public AlarmesIn PlcAlarmes = new AlarmesIn();
        public bool HmiNewDiscRead
        {
            get
            {
                
                return (HmiPlcFeedbackdisc.menber.ID != PlcHmiNewDisc.menber.ID);
            }
            set
            {

                HmiPlcFeedbackdisc = PlcHmiNewDisc;
            }
        }
        public bool PlcReadyForNewDIsc
        {
            get
            {
                
                return (HmiPlcNewDisc.menber.ID == PlcHmiFeedbackdisc.menber.ID);
            }
            set { }
        }
        //Variaveis privadas de interface com PLC
        private DiscCom _HmiPlcNewDisc = new DiscCom(), _HmiPlcFeedbackdisc = new DiscCom(), _PlcHmiNewDisc = new DiscCom(), _PlcHmiFeedbackdisc = new DiscCom();
        private Tapetesin _StaTapetes = new Tapetesin(), _StaRotativo = new Tapetesin();
        private TapetesOut _CmdTapetes = new TapetesOut(), _CmdRotativo = new TapetesOut();
        private Vibradoresin _StaVibrador = new Vibradoresin();
        private VibradoresOut _CmdVibrador = new VibradoresOut();
        private Cicloin _StaCiclo = new Cicloin();
        private CicloOut _CmdCiclo = new CicloOut();
        private Inputs _Dis = new Inputs();
        private Outputs _Dos = new Outputs();
        private bool SendRequest = new bool();
        private DateTime LastWriteTime = new DateTime(), UpDateHourTick = new DateTime();
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

        public void WriteReadPlc()
        {
            PLC1 = new Siemens(VariaveisAuxiliares.iniPath);

            while (this.isAlive)
            {

                FirstConetion = false;
                //Limpa listagens de variaveis
                ArrayIn.Clear();
                ArrayOut.Clear();
                index = 0;

                //multiplexa listagens de saida de variaveis
                _HmiPlcNewDisc.CreatReadList(ArrayIn);
                _HmiPlcFeedbackdisc.CreatReadList(ArrayIn);
                _CmdTapetes.CreatReadList(ArrayIn);
                _CmdRotativo.CreatReadList(ArrayIn);
                _CmdVibrador.CreatReadList(ArrayIn);
                _CmdCiclo.CreatReadList(ArrayIn);
                //CmdCicloDummy.CreatReadList(ArrayIn);
                //Le chunk de memoria do PLC para a listagem
                PLC1.LeSequenciaTags(Siemens.MemoryArea.DB, ArrayIn.ToArray(), 401, 0);
                index = _HmiPlcNewDisc.ReadVariables(ArrayIn, index);
                index = _HmiPlcFeedbackdisc.ReadVariables(ArrayIn, index);
                index = _CmdTapetes.ReadVariables(ArrayIn, index);
                index = _CmdRotativo.ReadVariables(ArrayIn, index);
                index = _CmdVibrador.ReadVariables(ArrayIn, index);
                index = _CmdCiclo.ReadVariables(ArrayIn, index);
                //index = CmdCicloDummy.ReadVariables(ArrayIn, index);

                ArrayIn.Clear();
                ArrayOut.Clear();

                HmiPlcNewDisc.writeClassValues(_HmiPlcNewDisc.menber);
                HmiPlcFeedbackdisc.writeClassValues(_HmiPlcFeedbackdisc.menber);
                CmdTapetes.writeClassValues (_CmdTapetes.Vars);
                CmdRotativo.writeClassValues( _CmdRotativo.Vars);
                CmdVibrador.writeClassValues(_CmdVibrador.Vars);
                CmdCiclo.writeClassValues(_CmdCiclo.Vars);
                //CmdCiclo.Vars.DateToPlc = DateTime.Now;
                FirstConetion = true;

                while (FirstConetion)
                {

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
                            _StaRotativo.CreatReadList(ArrayIn);
                            _StaVibrador.CreatReadList(ArrayIn);
                            _StaCiclo.CreatReadList(ArrayIn);
                            _Dis.CreatReadList(ArrayIn);
                            _Dos.CreatReadList(ArrayIn);
                            PlcAlarmes.CreatReadList(ArrayIn);
                            //Le chunk de memoria do PLC para a listagem
                            PLC1.LeSequenciaTags(Siemens.MemoryArea.DB, ArrayIn.ToArray(), 400, 0);
                        }
                        );
                        //Thread.Sleep(1);
                       Recieve.Wait();

                        //Calculo final do tempo de ciclo
                        leituras = (DateTime.Now - Startdate).TotalMilliseconds;
                        auxdt = DateTime.Now;



                        index = 0;
                        index = _PlcHmiNewDisc.ReadVariables(ArrayIn, index);
                        index = _PlcHmiFeedbackdisc.ReadVariables(ArrayIn, index);
                        index = _StaTapetes.ReadVariables(ArrayIn, index);
                        index = _StaRotativo.ReadVariables(ArrayIn, index);
                        index = _StaVibrador.ReadVariables(ArrayIn, index);
                        index = _StaCiclo.ReadVariables(ArrayIn, index);
                        index = _Dis.ReadVariables(ArrayIn, index);
                        index = _Dos.ReadVariables(ArrayIn, index);
                        index = PlcAlarmes.ReadVariables(ArrayIn, index);

                        // HmiPlcNewDisc.ID = HmiPlcNewDisc.ID + 1;
                        //if (HmiPlcNewDisc.ID > 100)
                        //    HmiPlcNewDisc.ID = 0;

                        //passa valor para veriaveis globais
                        //Inicializa valores dos outputs
                        //lock (PublicVarLock)
                        //{



                        //SendRequest = (HmiPlcNewDisc.menber.ID == _HmiPlcNewDisc.menber.ID && HmiPlcFeedbackdisc.menber.ID == _HmiPlcFeedbackdisc.menber.ID);




                        //SendRequest = (HmiPlcNewDisc..Equals(_HmiPlcNewDisc.menber) && HmiPlcFeedbackdisc.menber.Equals(_HmiPlcFeedbackdisc.menber))
                        //            && CmdTapetes.Vars.Equals(_CmdTapetes.Vars) && CmdVibrador.Vars.Equals(_CmdVibrador.Vars) && CmdCiclo.Vars.Equals(_CmdCiclo.Vars);

                       // if ((DateTime.Now - UpDateHourTick).TotalHours > 1)
                      //  {
                       ///     CmdCiclo.Vars.DateToPlc = DateTime.Now;
                       //    UpDateHourTick = DateTime.Now;
                       // }

                        SendRequest = HmiPlcNewDisc.CompareClassEq(_HmiPlcNewDisc.menber) && HmiPlcFeedbackdisc.CompareClassEq(_HmiPlcFeedbackdisc.menber)
                                   && CmdTapetes.CompareClassEq(_CmdTapetes.Vars) && CmdVibrador.CompareClassEq(_CmdVibrador.Vars) && CmdCiclo.CompareClassEq(_CmdCiclo.Vars)
                                   && CmdRotativo.CompareClassEq(_CmdRotativo.Vars);

                        

                        lock (PublicVarLock)
                        {
                            


                            _HmiPlcNewDisc.writeClassValues(HmiPlcNewDisc.menber);
                            _HmiPlcFeedbackdisc.writeClassValues(HmiPlcFeedbackdisc.menber);
                            _CmdTapetes.writeClassValues(CmdTapetes.Vars);
                            _CmdRotativo.writeClassValues(CmdRotativo.Vars);
                            _CmdVibrador.writeClassValues(CmdVibrador.Vars);
                            _CmdCiclo.writeClassValues(CmdCiclo.Vars);


                            PlcHmiNewDisc.writeClassValues(_PlcHmiNewDisc.menber);
                            PlcHmiFeedbackdisc.writeClassValues(_PlcHmiFeedbackdisc.menber);
                            StaTapetes.writeClassValues(_StaTapetes.Vars);
                            StaRotativo.writeClassValues(_StaRotativo.Vars);
                            StaVibrador.writeClassValues(_StaVibrador.Vars);
                            StaCiclo.writeClassValues(_StaCiclo.Vars);
                            Dis.writeClassValues(_Dis.Vars);
                            Dos.writeClassValues(_Dos.Vars);

                        }


                        //}

                        if (!(SendRequest))
                            {
                            Task Send = Task.Run(() =>
                             {
                                 //multiplexa listagens de saida de variaveis
                                 ArrayOut.Clear();
                                 _HmiPlcNewDisc.WriteVariables(ArrayOut);
                                 _HmiPlcFeedbackdisc.WriteVariables(ArrayOut);
                                 _CmdTapetes.WriteVariables(ArrayOut);
                                 _CmdRotativo.WriteVariables(ArrayOut);
                                 _CmdVibrador.WriteVariables(ArrayOut);
                                 _CmdCiclo.WriteVariables(ArrayOut);
                                 //CmdCicloDummy.WriteVariables(ArrayOut);
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
                        //if (CycleTime < 40)
                           // Thread.Sleep(40 - (int)CycleTime);

                        //Debug.WriteLine("TempoComPlc: " + (DateTime.Now - Startdate).TotalMilliseconds);
                    }
                    catch
                    {
                        FirstConetion = false;
                    }
                }
            }
        }
        public void Dispose()
        {
            isAlive = false;
        }
    }



    #endregion


    #region PLC structs
    public class DiscMenbers
    {
        public double ID;
        public double PulseEncoderS1;
        public double PulseEncoderS2;
        public double PulseExpel;
        public double DelayExpel;
        public DateTime EntradaS1 ;
        public DateTime InicioInspecao ;
        public DateTime FinalInspecao ;
        public DateTime SaidaS2 ;
        public short result;
        public short reserved_0;
        public short reserved_1;
        public short reserved_2;
        public short reserved_3;
        public short reserved_4;
        public short reserved_5;
        public short reserved_6;
    }
    //classe de informação sobre o disco
    public class DiscCom
    {
        public DiscMenbers menber = new DiscMenbers();

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DInt, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DTL, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DTL, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DTL, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DTL, 0));
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
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, menber.ID, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, menber.PulseEncoderS1, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, menber.PulseEncoderS2, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, menber.PulseExpel, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DInt, menber.DelayExpel, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DTL, menber.EntradaS1, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DTL, menber.InicioInspecao, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DTL, menber.FinalInspecao, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DTL, menber.SaidaS2, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.result, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.reserved_0, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.reserved_1, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.reserved_2, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.reserved_3, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.reserved_4, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.reserved_5, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, menber.reserved_6, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {

            menber.ID = Convert.ToDouble(buffer[index].ObtemVariavel());
            index++;
            menber.PulseEncoderS1 = buffer[index].ObtemVariavel();
            index++;
            menber.PulseEncoderS2 = buffer[index].ObtemVariavel();
            index++;
            menber.PulseExpel = buffer[index].ObtemVariavel();
            index++;
            menber.DelayExpel = buffer[index].ObtemVariavel();
            index++;
            menber.EntradaS1 = (buffer[index].ObtemVariavel());
            index++;
            menber.InicioInspecao = (buffer[index].ObtemVariavel());
            index++;
            menber.FinalInspecao = (buffer[index].ObtemVariavel());
            index++;
            menber.SaidaS2 = (buffer[index].ObtemVariavel());
            index++;
            menber.result = buffer[index].ObtemVariavel();
            index++;
            menber.reserved_0 = buffer[index].ObtemVariavel();
            index++;
            menber.reserved_1 = buffer[index].ObtemVariavel();
            index++;
            menber.reserved_2 = buffer[index].ObtemVariavel();
            index++;
            menber.reserved_3 = buffer[index].ObtemVariavel();
            index++;
            menber.reserved_4 = buffer[index].ObtemVariavel();
            index++;
            menber.reserved_5 = buffer[index].ObtemVariavel();
            index++;
            menber.reserved_6 = buffer[index].ObtemVariavel();
            index++;

            return index;
        }

        public void writeClassValues(DiscMenbers Values)
        {

            menber.ID = Values.ID;
            menber.PulseEncoderS1 = Values.PulseEncoderS1;
            menber.PulseEncoderS2 = Values.PulseEncoderS2;
            menber.PulseExpel = Values.PulseExpel;
            menber.DelayExpel = Values.DelayExpel;
            menber.EntradaS1 = Values.EntradaS1;
            menber.InicioInspecao = Values.InicioInspecao;
            menber.FinalInspecao = Values.FinalInspecao;
            menber.SaidaS2 = Values.SaidaS2;
            menber.result = Values.result;
            menber.reserved_0 = Values.reserved_0;
            menber.reserved_1 = Values.reserved_1;
            menber.reserved_2 = Values.reserved_2;
            menber.reserved_3 = Values.reserved_3;
            menber.reserved_4 = Values.reserved_4;
            menber.reserved_5 = Values.reserved_5;
            menber.reserved_6 = Values.reserved_6;
        }

        public bool CompareClassEq(DiscMenbers Values)
        {
            bool comp1 = menber.ID == Values.ID &&
            menber.PulseEncoderS1 == Values.PulseEncoderS1 &&
            menber.PulseEncoderS2 == Values.PulseEncoderS2 &&
            menber.PulseExpel == Values.PulseExpel &&
            menber.DelayExpel == Values.DelayExpel &&
            menber.EntradaS1 == Values.EntradaS1 &&
            menber.InicioInspecao == Values.InicioInspecao &&
            menber.FinalInspecao == Values.FinalInspecao &&
            menber.SaidaS2 == Values.SaidaS2 &&
            menber.result == Values.result &&
            menber.reserved_0 == Values.reserved_0 &&
            menber.reserved_1 == Values.reserved_1 &&
            menber.reserved_2 == Values.reserved_2 &&
            menber.reserved_3 == Values.reserved_3 &&
            menber.reserved_4 == Values.reserved_4 &&
            menber.reserved_5 == Values.reserved_5 &&
            menber.reserved_6 == Values.reserved_6;

            return comp1;
        }

    }

    public class Inputs
    {
        public class variables
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
        }
        public variables Vars = new variables();

        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Vars.Di0, Vars.Di1, Vars.Di2, Vars.Di3, Vars.Di4, Vars.Di5, Vars.Di6, Vars.Di7);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Vars.Di8, Vars.Di9, Vars.Di10, Vars.Di11, Vars.Di12, Vars.Di13, Vars.Di14, Vars.Di15);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Di0 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Di8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di11 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di12 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di13 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di14 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Di15 = (auxByte & Mask[bit]) != 0;
            index = index + 1;

            return index;
        }

        public void writeClassValues(variables Values)
        {
            Vars.Di0 = Values.Di0;
            Vars.Di1 = Values.Di1;
            Vars.Di2 = Values.Di2;
            Vars.Di3 = Values.Di3;
            Vars.Di4 = Values.Di4;
            Vars.Di5 = Values.Di5;
            Vars.Di6 = Values.Di6;
            Vars.Di7 = Values.Di7;
            Vars.Di8 = Values.Di8;
            Vars.Di9 = Values.Di9;
            Vars.Di10 = Values.Di10;
            Vars.Di11 = Values.Di11;
            Vars.Di12 = Values.Di12;
            Vars.Di13 = Values.Di13;
            Vars.Di14 = Values.Di14;
            Vars.Di15 = Values.Di15;

        }
        public bool CompareClassEq(variables Values)
        {
            bool comp1 = Vars.Di0 == Values.Di0 &&
            Vars.Di1 == Values.Di1 &&
            Vars.Di2 == Values.Di2 &&
            Vars.Di3 == Values.Di3 &&
            Vars.Di4 == Values.Di4 &&
            Vars.Di5 == Values.Di5 &&
            Vars.Di6 == Values.Di6 &&
            Vars.Di7 == Values.Di7 &&
            Vars.Di8 == Values.Di8 &&
            Vars.Di9 == Values.Di9 &&
            Vars.Di10 == Values.Di10 &&
            Vars.Di11 == Values.Di11 &&
            Vars.Di12 == Values.Di12 &&
            Vars.Di13 == Values.Di13 &&
            Vars.Di14 == Values.Di14 &&
            Vars.Di15 == Values.Di15;

            return comp1;
        }

    }

    public class Outputs
    {
        public class variables
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
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Vars.Do0, Vars.Do1, Vars.Do2, Vars.Do3, Vars.Do4, Vars.Do5, Vars.Do6, Vars.Do7);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Vars.Do8, Vars.Do9, Vars.Do10, Vars.Do11, Vars.Do12, Vars.Do13, Vars.Do14, Vars.Do15);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Do0 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Do8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do11 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do12 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do13 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do14 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Do15 = (auxByte & Mask[bit]) != 0;
            index = index + 1;

            return index;
        }
        public bool CompareClassEq(variables Values)
        {
            bool comp1 = Vars.Do0 == Values.Do0 &&
            Vars.Do1 == Values.Do1 &&
            Vars.Do2 == Values.Do2 &&
            Vars.Do3 == Values.Do3 &&
            Vars.Do4 == Values.Do4 &&
            Vars.Do5 == Values.Do5 &&
            Vars.Do6 == Values.Do6 &&
            Vars.Do7 == Values.Do7 &&
            Vars.Do8 == Values.Do8 &&
            Vars.Do9 == Values.Do9 &&
            Vars.Do10 == Values.Do10 &&
            Vars.Do11 == Values.Do11 &&
            Vars.Do12 == Values.Do12 &&
            Vars.Do13 == Values.Do13 &&
            Vars.Do14 == Values.Do14 &&
            Vars.Do15 == Values.Do15;

            return comp1;
        }

        public void writeClassValues(variables Values)
        {
            Vars.Do0 = Values.Do0;
            Vars.Do1 = Values.Do1;
            Vars.Do2 = Values.Do2;
            Vars.Do3 = Values.Do3;
            Vars.Do4 = Values.Do4;
            Vars.Do5 = Values.Do5;
            Vars.Do6 = Values.Do6;
            Vars.Do7 = Values.Do7;
            Vars.Do8 = Values.Do8;
            Vars.Do9 = Values.Do9;
            Vars.Do10 = Values.Do10;
            Vars.Do11 = Values.Do11;
            Vars.Do12 = Values.Do12;
            Vars.Do13 = Values.Do13;
            Vars.Do14 = Values.Do14;
            Vars.Do15 = Values.Do15;
        }
    }

    public class Tapetesin
    {
        public class variables
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
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Int, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Vars.Enabled, Vars.Running, Vars.Fault, Vars.Reserved_1, Vars.Reserved_2, Vars.Reserved_3, Vars.Reserved_4, Vars.Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.Reserved_6, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Int, Vars.actspeed, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Enabled = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Running = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Fault = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_5 = (auxByte & Mask[bit]) != 0;
            index = index + 1;
            Vars.Reserved_6 = (buffer[index].ObtemVariavel());
            index = index + 1;
            Vars.actspeed = buffer[index].ObtemVariavel();
            index = index + 1;

            return index;
        }
        public void writeClassValues(variables Values)
        {
            Vars.Enabled = Values.Enabled;
            Vars.Running = Values.Running;
            Vars.Fault = Values.Fault;
            Vars.Reserved_1 = Values.Reserved_1;
            Vars.Reserved_2 = Values.Reserved_2;
            Vars.Reserved_3 = Values.Reserved_3;
            Vars.Reserved_4 = Values.Reserved_4;
            Vars.Reserved_5 = Values.Reserved_5;
            Vars.Reserved_6 = Values.Reserved_6;
            Vars.actspeed = Values.actspeed;
        }
        public bool CompareClassEq(variables Values)
        {
            bool comp1 = Vars.Enabled == Values.Enabled &&
            Vars.Running == Values.Running &&
            Vars.Fault == Values.Fault &&
            Vars.Reserved_1 == Values.Reserved_1 &&
            Vars.Reserved_2 == Values.Reserved_2 &&
            Vars.Reserved_3 == Values.Reserved_3 &&
            Vars.Reserved_4 == Values.Reserved_4 &&
            Vars.Reserved_5 == Values.Reserved_5 &&
            Vars.Reserved_6 == Values.Reserved_6 &&
            Vars.actspeed == Values.actspeed;

            return comp1;
        }
    }

    public class TapetesOut
    {
        public class variables
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
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Int, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Vars.Enable, Vars.Run, Vars.Reset, Vars.Reserved_1, Vars.Reserved_2, Vars.Reserved_3, Vars.Reserved_4, Vars.Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.Reserved_6, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Int, Vars.setspeed, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Enable = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Run = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reset = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_5 = (auxByte & Mask[bit]) != 0;
            index = index + 1;
            Vars.Reserved_6 = (buffer[index].ObtemVariavel());
            index = index + 1;
            Vars.setspeed = buffer[index].ObtemVariavel();
            index = index + 1;

            return index;
        }
        public void writeClassValues(variables Values)
        {


            Vars.Enable = Values.Enable;
            Vars.Run = Values.Run;
            Vars.Reset = Values.Reset;
            Vars.Reserved_1 = Values.Reserved_1;
            Vars.Reserved_2 = Values.Reserved_2;
            Vars.Reserved_3 = Values.Reserved_3;
            Vars.Reserved_4 = Values.Reserved_4;
            Vars.Reserved_5 = Values.Reserved_5;
            Vars.Reserved_6 = Values.Reserved_6;
            Vars.setspeed = Values.setspeed;
        }
        public bool CompareClassEq(variables Values)
        {

            bool comp1 = Vars.Enable == Values.Enable &&
            Vars.Run == Values.Run &&
            Vars.Reset == Values.Reset &&
            Vars.Reserved_1 == Values.Reserved_1 &&
            Vars.Reserved_2 == Values.Reserved_2 &&
            Vars.Reserved_3 == Values.Reserved_3 &&
            Vars.Reserved_4 == Values.Reserved_4 &&
            Vars.Reserved_5 == Values.Reserved_5 &&
            Vars.Reserved_6 == Values.Reserved_6 &&
            Vars.setspeed == Values.setspeed;

            return comp1;
        }

    }

    public class Vibradoresin
    {
        public class variables
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
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));


        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();
            auxbyte = PLC.Siemens.BitsToByte(Vars.Enabled, Vars.Running, Vars.Fault, Vars.Reserved_1, Vars.Reserved_2, Vars.Reserved_3, Vars.Reserved_4, Vars.Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.Reserved_6, 0));
        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Enabled = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Running = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Fault = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            Vars.Reserved_6 = (buffer[index].ObtemVariavel());
            return index;
        }
        public void writeClassValues(variables Values)
        {
            Vars.Enabled = Values.Enabled;
            Vars.Running = Values.Running;
            Vars.Fault = Values.Fault;
            Vars.Reserved_1 = Values.Reserved_1;
            Vars.Reserved_2 = Values.Reserved_2;
            Vars.Reserved_3 = Values.Reserved_3;
            Vars.Reserved_4 = Values.Reserved_4;
            Vars.Reserved_5 = Values.Reserved_5;
            Vars.Reserved_6 = Values.Reserved_6;
        }
        public bool CompareClassEq(variables Values)
        {
            bool comp1 = Vars.Enabled == Values.Enabled &&
            Vars.Running == Values.Running &&
            Vars.Fault == Values.Fault &&
            Vars.Reserved_1 == Values.Reserved_1 &&
            Vars.Reserved_2 == Values.Reserved_2 &&
            Vars.Reserved_3 == Values.Reserved_3 &&
            Vars.Reserved_4 == Values.Reserved_4 &&
            Vars.Reserved_5 == Values.Reserved_5 &&
            Vars.Reserved_6 == Values.Reserved_6;

            return comp1;
        }
    }

    public class VibradoresOut
    {
        public class variables
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
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Vars.Enable, Vars.Run, Vars.Reset, Vars.Reserved_1, Vars.Reserved_2, Vars.Reserved_3, Vars.Reserved_4, Vars.Reserved_5);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.Reserved_6, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;


            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Enable = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Run = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reset = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_1 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index + 1;
            Vars.Reserved_6 = (buffer[index].ObtemVariavel());
            index = index + 1;

            return index;
        }
        public void writeClassValues(variables Values)
        {


            Vars.Enable = Values.Enable;
            Vars.Run = Values.Run;
            Vars.Reset = Values.Reset;
            Vars.Reserved_1 = Values.Reserved_1;
            Vars.Reserved_2 = Values.Reserved_2;
            Vars.Reserved_3 = Values.Reserved_3;
            Vars.Reserved_4 = Values.Reserved_4;
            Vars.Reserved_5 = Values.Reserved_5;
            Vars.Reserved_6 = Values.Reserved_6;

        }

        public bool CompareClassEq(variables Values)
        {

            bool comp1 = Vars.Enable == Values.Enable &&
            Vars.Run == Values.Run &&
            Vars.Reset == Values.Reset &&
            Vars.Reserved_1 == Values.Reserved_1 &&
            Vars.Reserved_2 == Values.Reserved_2 &&
            Vars.Reserved_3 == Values.Reserved_3 &&
            Vars.Reserved_4 == Values.Reserved_4 &&
            Vars.Reserved_5 == Values.Reserved_5 &&
            Vars.Reserved_6 == Values.Reserved_6;

            return comp1;
        }

    }

    public class Cicloin
    {
        public class variables
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
            public byte Reserved_13;
            public byte Reserved_14;
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Vars.InManual, Vars.InAuto, Vars.CycleOn, Vars.ReadyForAuto, Vars.TapeteReady, Vars.VibradorReady, Vars.AlimentadorReady, Vars.EmergencyOk);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Vars.Reserved_5, Vars.Reserved_6, Vars.Reserved_7, Vars.Reserved_8, Vars.Reserved_9, Vars.Reserved_10, Vars.Reserved_11, Vars.Reserved_12);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.Reserved_13, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.Reserved_14, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;

            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.InManual = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.InAuto = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.CycleOn = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.ReadyForAuto = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.TapeteReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.VibradorReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.AlimentadorReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.EmergencyOk = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index - 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_11 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_12 = (auxByte & Mask[bit]) != 0;
            index = index + 2;
            Vars.Reserved_13 = (buffer[index].ObtemVariavel());
            index = index + 1;
            Vars.Reserved_14 = (buffer[index].ObtemVariavel());
            index = index + 1;

            return index;
        }
        public void writeClassValues(variables Values)
        {
            Vars.InManual = Values.InManual;
            Vars.InAuto = Values.InAuto;
            Vars.CycleOn = Values.CycleOn;
            Vars.ReadyForAuto = Values.ReadyForAuto;
            Vars.TapeteReady = Values.TapeteReady;
            Vars.VibradorReady = Values.VibradorReady;
            Vars.AlimentadorReady = Values.AlimentadorReady;
            Vars.EmergencyOk = Values.EmergencyOk;
            Vars.Reserved_5 = Values.Reserved_5;
            Vars.Reserved_6 = Values.Reserved_6;
            Vars.Reserved_7 = Values.Reserved_7;
            Vars.Reserved_8 = Values.Reserved_8;
            Vars.Reserved_9 = Values.Reserved_9;
            Vars.Reserved_10 = Values.Reserved_10;
            Vars.Reserved_11 = Values.Reserved_11;
            Vars.Reserved_12 = Values.Reserved_12;
            Vars.Reserved_13 = Values.Reserved_13;
            Vars.Reserved_14 = Values.Reserved_14;

        }

        public bool CompareClassEq(variables Values)
        {
            bool comp1 = Vars.InManual == Values.InManual &&
            Vars.InAuto == Values.InAuto &&
            Vars.CycleOn == Values.CycleOn &&
            Vars.ReadyForAuto == Values.ReadyForAuto &&
            Vars.TapeteReady == Values.TapeteReady &&
            Vars.VibradorReady == Values.VibradorReady &&
            Vars.AlimentadorReady == Values.AlimentadorReady &&
            Vars.EmergencyOk == Values.EmergencyOk &&
            Vars.Reserved_5 == Values.Reserved_5 &&
            Vars.Reserved_6 == Values.Reserved_6 &&
            Vars.Reserved_7 == Values.Reserved_7 &&
            Vars.Reserved_8 == Values.Reserved_8 &&
            Vars.Reserved_9 == Values.Reserved_9 &&
            Vars.Reserved_10 == Values.Reserved_10 &&
            Vars.Reserved_11 == Values.Reserved_11 &&
            Vars.Reserved_12 == Values.Reserved_12 &&
            Vars.Reserved_13 == Values.Reserved_13 &&
            Vars.Reserved_14 == Values.Reserved_14;

            return comp1;

        }
    }
    public class AlarmesIn
    {
        public class variables
        {
            public bool[] Alarmes = new bool[128];
            public static int SizeAlm = 16;
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            for (int i = 0; i < variables.SizeAlm; i++)
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;

            for (int i = 0; i < variables.SizeAlm; i++) { 
                auxByte = (buffer[index].ObtemVariavel());
                index = index + 1;
                for (int k = 0; k < 8; k++)
                {
                    Vars.Alarmes[k + (8 * i)] = (auxByte & Mask[k]) != 0;
                }
            }

            //                            for (int k = 0; k < 8; k++)
            //                                VARIAVEIS.Alarmes[k + (8 * i)] = (Convert.ToBoolean(DB400_HMI[i].ObtemVariavel(k)));

            //                            indexAuxI++;
            //                            for (int k = 0; k < 8; k++)
            //                                VARIAVEIS.Alarmes[k + (8 * i)] = (Convert.ToBoolean(DB400_HMI[i].ObtemVariavel(k)));

            //                            indexAuxI++;
            //                        }


            return index;
        }
        

    }

    public class CicloOut
    {
        public class variables
        {
            public bool ManualRequest;
            public bool AutoRequest;
            public bool Stop;
            public bool Start;
            public bool CameraReady;
            public bool VisonReady;
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
            public DateTime DateToPlc;
            public byte NivelSessao;
            public byte Reserved_12;
        }
        public variables Vars = new variables();
        public void CreatReadList(List<PLC.Siemens.ReadMultiVariables> buffer)
        {
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.DTL, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
            buffer.Add(new PLC.Siemens.ReadMultiVariables(Siemens.TipoVariavel.Byte, 0));
        }
        public void WriteVariables(List<PLC.Siemens.WriteMultiVariables> buffer)
        {
            byte auxbyte = new byte();

            auxbyte = PLC.Siemens.BitsToByte(Vars.ManualRequest, Vars.AutoRequest, Vars.Stop, Vars.Start, Vars.CameraReady, Vars.VisonReady, Vars.Reserved_2, Vars.Reserved_3);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            auxbyte = PLC.Siemens.BitsToByte(Vars.Reserved_4, Vars.Reserved_5, Vars.Reserved_6, Vars.Reserved_7, Vars.Reserved_8, Vars.Reserved_9, Vars.Reserved_10, Vars.Reserved_11);
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, auxbyte, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.DTL, Vars.DateToPlc, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.NivelSessao, 0));
            buffer.Add(new PLC.Siemens.WriteMultiVariables(Siemens.TipoVariavel.Byte, Vars.Reserved_12, 0));

        }
        public int ReadVariables(List<PLC.Siemens.ReadMultiVariables> buffer, int index)
        {
            byte[] Mask = { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
            byte auxByte;
            int bit;

            index = index + 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.ManualRequest = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.AutoRequest = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Stop = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Start = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.CameraReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.VisonReady = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_2 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_3 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            index = index - 1;
            bit = 0;
            auxByte = (buffer[index].ObtemVariavel());
            Vars.Reserved_4 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_5 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_6 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_7 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_8 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_9 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_10 = (auxByte & Mask[bit]) != 0;
            bit = bit + 1;
            Vars.Reserved_11 = (auxByte & Mask[bit]) != 0;
            index = index + 2;
            Vars.DateToPlc = (buffer[index].ObtemVariavel());
            index = index + 1;
            Vars.NivelSessao = (buffer[index].ObtemVariavel());
            index = index + 1;
            Vars.Reserved_12 = (buffer[index].ObtemVariavel());
            index = index + 1;

            return index;
        }
        public void writeClassValues(variables Values)
        {
            Vars.ManualRequest = Values.ManualRequest;
            Vars.AutoRequest = Values.AutoRequest;
            Vars.Stop = Values.Stop;
            Vars.Start = Values.Start;
            Vars.CameraReady = Values.CameraReady;
            Vars.VisonReady = Values.VisonReady;
            Vars.Reserved_2 = Values.Reserved_2;
            Vars.Reserved_3 = Values.Reserved_3;
            Vars.Reserved_4 = Values.Reserved_4;
            Vars.Reserved_5 = Values.Reserved_5;
            Vars.Reserved_6 = Values.Reserved_6;
            Vars.Reserved_7 = Values.Reserved_7;
            Vars.Reserved_8 = Values.Reserved_8;
            Vars.Reserved_9 = Values.Reserved_9;
            Vars.Reserved_10 = Values.Reserved_10;
            Vars.Reserved_11 = Values.Reserved_11;
            Vars.DateToPlc = Values.DateToPlc;
            Vars.NivelSessao = Values.NivelSessao;
            Vars.Reserved_12 = Values.Reserved_12;
        }
        public bool CompareClassEq(variables Values)
        {
            bool comp1 =
            Vars.ManualRequest == Values.ManualRequest &&
            Vars.AutoRequest == Values.AutoRequest &&
            Vars.Stop == Values.Stop &&
            Vars.Start == Values.Start &&
            Vars.CameraReady == Values.CameraReady &&
            Vars.VisonReady == Values.VisonReady &&
            Vars.Reserved_2 == Values.Reserved_2 &&
            Vars.Reserved_3 == Values.Reserved_3 &&
            Vars.Reserved_4 == Values.Reserved_4 &&
            Vars.Reserved_5 == Values.Reserved_5 &&
            Vars.Reserved_6 == Values.Reserved_6 &&
            Vars.Reserved_7 == Values.Reserved_7 &&
            Vars.Reserved_8 == Values.Reserved_8 &&
            Vars.Reserved_9 == Values.Reserved_9 &&
            Vars.Reserved_10 == Values.Reserved_10 &&
            Vars.Reserved_11 == Values.Reserved_11 &&
            Vars.DateToPlc == Values.DateToPlc &&
            Vars.NivelSessao == Values.NivelSessao &&
            Vars.Reserved_12 == Values.Reserved_12;

            return comp1;
        }

    }
    #endregion
}
