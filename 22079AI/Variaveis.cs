using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _22079AI
{
    public class VARIAVEIS
    {
        /// <summary>
        /// Variables of Alarms
        /// </summary>
        public static bool[] Alarmes = new bool[128];

        public static bool[] FP_JANELAS_AUTOMATICO = new bool[8];

        public static bool FORM_LOADED = false;
        public static bool FLAG_WHILE_CYCLE = true;

        public static bool WINDOW_OPEN = false;

        public static bool ESTADO_CONEXAO_PLC = false;

        public static void STATUS_BUTTON(byte TAG, out Color pos1, out Color pos2)
        {
            /*
            0 - SEM COMANDOS E SEM ESTAR NA POSICAO
            1 - SENSOR DE RECUADO SEM O COMANDO
            2 - SENSOR DE AVANCADO SEM O COMANDO
            3 - ORDEM DE RECUADO SEM O SENSOR
            4 - ORDEM DE AVANCADO SEM O SENSOR
            5 - FALHA RECUADO
            6 - FALHA AVANCADO
            */

            if (ESTADO_CONEXAO_PLC)
                switch (TAG)
                {
                    case 1: // 1 - SENSOR DE RECUADO SEM O COMANDO
                        {
                            pos1 = Color.LimeGreen;
                            pos2 = Color.White;
                            break;
                        }
                    case 2: // 2 - SENSOR DE AVANCADO SEM O COMANDO
                        {
                            pos1 = Color.White;
                            pos2 = Color.LimeGreen;
                            break;
                        }
                    case 3: // 3 - ORDEM DE RECUADO SEM O SENSOR
                        {
                            pos1 = Color.OrangeRed;
                            pos2 = Color.White;
                            break;
                        }
                    case 4: // 4 - ORDEM DE AVANCADO SEM O SENSOR
                        {
                            pos1 = Color.White;
                            pos2 = Color.OrangeRed;
                            break;
                        }
                    case 5: // 5 - FALHA RECUADO
                        {
                            pos1 = Color.Red;
                            pos2 = Color.White;
                            break;
                        }
                    case 6: // 6 - FALHA AVANCADO
                        {
                            pos1 = Color.White;
                            pos2 = Color.Red;
                            break;
                        }
                    default:
                        {
                            pos1 = Color.White;
                            pos2 = Color.White;
                            break;
                        }
                }
            else
                pos1 = pos2 = Color.DimGray;
        }

        public static void STATUS_BUTTON(byte TAG, Color waitColor, out Color pos1, out Color pos2)
        {
            /*
            0 - SEM COMANDOS E SEM ESTAR NA POSICAO
            1 - SENSOR DE RECUADO SEM O COMANDO
            2 - SENSOR DE AVANCADO SEM O COMANDO
            3 - ORDEM DE RECUADO SEM O SENSOR
            4 - ORDEM DE AVANCADO SEM O SENSOR
            5 - FALHA RECUADO
            6 - FALHA AVANCADO
            */

            if (ESTADO_CONEXAO_PLC)
                switch (TAG)
                {
                    case 1: // 1 - SENSOR DE RECUADO SEM O COMANDO
                        {
                            pos1 = Color.LimeGreen;
                            pos2 = waitColor;
                            break;
                        }
                    case 2: // 2 - SENSOR DE AVANCADO SEM O COMANDO
                        {
                            pos1 = waitColor;
                            pos2 = Color.LimeGreen;
                            break;
                        }
                    case 3: // 3 - ORDEM DE RECUADO SEM O SENSOR
                        {
                            pos1 = Color.OrangeRed;
                            pos2 = waitColor;
                            break;
                        }
                    case 4: // 4 - ORDEM DE AVANCADO SEM O SENSOR
                        {
                            pos1 = waitColor;
                            pos2 = Color.OrangeRed;
                            break;
                        }
                    case 5: // 5 - FALHA RECUADO
                        {
                            pos1 = Color.Red;
                            pos2 = waitColor;
                            break;
                        }
                    case 6: // 6 - FALHA AVANCADO
                        {
                            pos1 = waitColor;
                            pos2 = Color.Red;
                            break;
                        }
                    default:
                        {
                            pos1 = waitColor;
                            pos2 = waitColor;
                            break;
                        }
                }
            else
                pos1 = pos2 = Color.DimGray;
        }

        public static Color RB_STATUS_BUTTON(byte TAG)
        {
            /*
            0 - SEM COMANDOS E SEM ESTAR NA POSICAO
            1 - SENSOR DE RECUADO SEM O COMANDO
            3 - ORDEM DE RECUADO SEM O SENSOR
            5 - FALHA RECUADO
            */

            if (ESTADO_CONEXAO_PLC)
                switch (TAG)
                {
                    case 1: // 1 - SENSOR DE RECUADO SEM O COMANDO
                        return Color.LimeGreen;
                    case 3: // 3 - ORDEM DE RECUADO SEM O SENSOR
                        return Color.OrangeRed;
                    case 5: // 5 - FALHA RECUADO
                        return Color.Red;
                    default:
                        return Color.White;
                }
            else
                return Color.DimGray;
        }

        public static Color GetStatusDriveColor(bool inPos, bool inError)
        {
            if (inError)
                return Color.OrangeRed;
            else
                return inPos ? Color.LimeGreen : Color.White;
        }


    }

    public enum TipoCaixa
    {
        Aprovados = 0,
        Reprovados = 1
    }

    public class DB400
    {
        public static bool INF_ALL_INIT_POS_OK = false;
        public static bool INF_RECEITA_CARREGADA = false;
        public static bool INF_TRANSPORTER_READY = false;
        public static bool INF_ROBO_READY = false;
        public static bool ALL_DOORS_CLOSED = false;
        public static bool READY_TO_AUTO = false;
        public static bool MANUAL_AUTO = false;
        public static bool CYCLE_ON = false;

        public static bool USER_SESSION_ACTIVE = false;
        public static bool CLOCK_1HZ = false;
        public static bool CLOCK_2HZ = false;
        public static bool CLOCK_5HZ = false;
        public static bool CLOCK_10HZ = false;
        public static bool BYPASS_SECURITY = false;
        public static bool VISION_LIGHT_ON = false;
        public static bool CAM_TRIGGER_MODE = false;

        public static bool CAM_ORD_TRIGGER = false;
        public static bool FECHO_CAM_CLOSED = false;
        public static bool FECHO_ILUMINACAO_CLOSED = false;
        public static bool FECHO_ROBOT_CLOSED = false;
        public static bool FECHO_ECRA_CLOSED = false;
        public static bool FECHO_BARREIRA_APROV_CLOSED = false;
        public static bool FECHO_BARREIRA_REPROV_CLOSED = false;
        public static bool CARREGAMENTO_EM_CURSO = false;

        public static bool DESCARGA_OK_EM_CURSO = false;
        public static bool DESCARGA_NOK_EM_CURSO = false;
        public static bool DESCARGA_OK_FULL = false;
        public static bool DESCARGA_NOK_FULL = false;
        public static bool SEQUENCIA_NOK = false;
        public static bool STOP_BARREIRA_APROVADOS = false;
        public static bool STOP_BARREIRA_REPROVADOS = false;
        public static bool CAIXA_PRESENTE_APROVADOS = false;

        public static bool CAIXA_PRESENTE_REPROVADOS = false;
        public static bool EMERGENCIA = false;
        public static bool CON_GRAB_CONVOYER = false;
        public static bool CON_TAKE_INSPECTION = false;
        public static bool CON_APROVE_KNIFE = false;
        public static bool CON_REPROVE_KNIFE = false;
        public static bool CON_INI_POSITION = false;
        public static bool RESERVA_40_7 = false;

        public static byte RESERVA_BYTE_41 = 0;

        public static Inspection INSPECTION_RESULT = new Inspection();

        public static ushort RESERVED_INT = 0;

        public static int TOTAL_APROVED_BY_BOX = 0;
        public static int TOTAL_REPROVED_BY_BOX = 0;
        public static int TOTAL_APROVED = 0;
        public static int TOTAL_REPROVED = 0;
        public static int LOTE_TOTAL_NUMBER = 0;
        public static int SEQUENCE_REPROVED = 0;

        public static MOTOR_HMI MOTOR_PASSADEIRA = new MOTOR_HMI(31);

        public static byte CILINDRO_INDEXACAO = 0;
        public static byte PORTA_DIREITA = 0;
        public static byte PORTA_ESQUERDA = 0;


        public static bool STA_GRAB_CONVOYER = false;
        public static bool STA_TAKE_INSPECTION = false;
        public static bool STA_APROVE_KNIFE = false;
        public static bool STA_REPROVE_KNIFE = false;
        public static bool STA_INI_POSITION = false;
        public static bool STA_SAFE_POSITION = false;
        public static bool ALM_GRAB_CONVOYER = false;
        public static bool ALM_TAKE_INSPECTION = false;

        public static bool ALM_APROVE_KNIFE = false;
        public static bool ALM_REPROVE_KNIFE = false;
        public static bool ALM_INI_POSITION = false;
        public static bool RESERVA_104_3 = false;
        public static bool RESERVA_104_4 = false;
        public static bool RESERVA_104_5 = false;
        public static bool RESERVA_104_6 = false;
        public static bool RESERVA_104_7 = false;

        public static ushort PECAS_ULTIMA_HORA = 0;
        public static ushort PECAS_MEDIA_HORA = 0;
        public static ushort TEMPO_DE_CICLO = 0;


        public static ushort COUNTER_KNIFES_ENGRAVED = 0;
        public static ushort SUB_COUNTER_KNIFES = 0;
        public static ushort KNIFES_TO_ENGRAVE = 0;
        public static bool TRANSPORTADOR_COM_FACA = false;
    }

    public class Inspection
    {
        public uint ID { get; set; } = 0;

        public int UNIX_TIME { get; set; } = 0;

        public double COMPRIMENTO { get; set; } = 0;

        public double AMPLITUDE_DESVIO_SUPERIOR { get; set; } = 0;

        public double POS_MAX_DESVIO_SUPERIOR { get; set; } = 0;

        public double AMPLITUDE_DESVIO_INFERIOR { get; set; } = 0;

        public double POS_MAX_DESVIO_INFERIOR { get; set; } = 0;

        public ushort CAPTURE_TIME { get; set; } = 0;
        public ushort INSPECTION_TIME { get; set; } = 0;

        public byte INSPECTION_RESULT { get; set; } = 0;

        public bool CAPTURE_DONE { get; set; } = false;

        public bool INSPECTION_DONE { get; set; } = false;

        /// <summary>
        /// a true quando o anel esta aprovado
        /// </summary>
        public bool APROVED { get; set; } = false;
        /// <summary>
        /// a true quando o anel deve ser lido pela base de dados
        /// </summary>
        public bool READ { get; set; } = false;

        public bool RESERVED4 { get; set; } = false;
        public bool RESERVED5 { get; set; } = false;
        public bool RESERVED6 { get; set; } = false;
        public bool RESERVED7 { get; set; } = false;


        public DateTime DT_INSPECTION { get; set; }


        //public DateTime DT_INSPECTION
        //{
        //    get
        //    {
        //        if (this.UNIX_TIME > 0)
        //            return Diversos.ConvertUnixParaDatetime(this.UNIX_TIME);
        //        else
        //            return DateTime.MinValue;
        //    }

        //    //set { DT_INSPECTION = value; }

        //}

        public HObject INSPECTION_IMAGE { get; set; } = null;

        public Inspection()
        {

        }

        public int UpdateTags(PLC.Siemens.ReadMultiVariables[] vars, int indexOffset)
        {
            int count = 0;

            try
            {
                this.ID = Convert.ToUInt32(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.UNIX_TIME = Convert.ToInt32(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.COMPRIMENTO = Convert.ToDouble(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.AMPLITUDE_DESVIO_SUPERIOR = Convert.ToDouble(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.POS_MAX_DESVIO_SUPERIOR = Convert.ToDouble(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.AMPLITUDE_DESVIO_INFERIOR = Convert.ToDouble(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.POS_MAX_DESVIO_INFERIOR = Convert.ToDouble(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.CAPTURE_TIME = Convert.ToUInt16(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.INSPECTION_TIME = Convert.ToUInt16(vars[indexOffset + count].ObtemVariavel());
                count++;
                this.INSPECTION_RESULT = Convert.ToByte(vars[indexOffset + count].ObtemVariavel());
                count++;

                this.CAPTURE_DONE = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(0));
                this.INSPECTION_DONE = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(1));
                this.APROVED = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(2));
                this.READ = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(3));
                this.RESERVED4 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(4));
                this.RESERVED5 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(5));
                this.RESERVED6 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(6));
                this.RESERVED7 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(7));

                count++;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateTags(): " + ex.Message);
                count = -1;
            }

            return count;
        }
    }

    public class INPUTS
    {
        // PLC
        public static bool BTN_START = false; //M0.0
        public static bool BTN_STOP = false;
        public static bool BTN_RESET = false;
        public static bool BTN_PC_ON = false;
        public static bool SEN_PORTA_DIREITA_FECHADA = false;
        public static bool SEN_PORTA_DIREITA_ABERTA = false;
        public static bool SEN_PORTA_ESQUERDA_FECHADA = false;
        public static bool SEN_PORTA_ESQUERDA_ABERTA = false;

        public static bool SEN_KNIFE_CONVYER_DETECTED = false; //M1.0
        public static bool SEN_INDEX_FACA_BAIXO = false;
        public static bool SEN_ROBOT_GARRA_ABERTA = false;
        public static bool SEN_CILINDRO_CONVOYER_INDEX = false;
        public static bool SEN_PRESENCA_CAIXA_APROVADOS = false;
        public static bool SEN_PRESENCA_CAIXA_REPROVADOS = false;
        public static bool RESERVA_1_6 = false;
        public static bool RESERVA_1_7 = false;

        //1º CARTA 16 DI's
        public static bool STU_THP_VFR = false;//M2.0
        public static bool STU_VFR_FAULT = false;
        public static bool STU_VFR_RUNNING = false;
        public static bool RESERVA_2_3 = false;
        public static bool RESERVA_2_4 = false;
        public static bool RESERVA_2_5 = false;
        public static bool RESERVA_2_6 = false;
        public static bool RESERVA_2_7 = false;

        public static bool STA_GRAB_CONVOYER = false;//M3.0
        public static bool STA_TAKE_INSPECTION = false;
        public static bool STA_APROVE_KNIFE = false;
        public static bool STA_REPROVE_KNIFE = false;
        public static bool STA_INI_POSITION = false;
        public static bool RESERVA_3_5 = false;
        public static bool RESERVA_3_6 = false;
        public static bool RESERVA_3_7 = false;

        //2º CARTA 8 DI's
        public static bool RESERVA_4_0 = false; //M4.0
        public static bool RESERVA_4_1 = false;
        public static bool RESERVA_4_2 = false;
        public static bool RESERVA_4_3 = false;
        public static bool RESERVA_4_4 = false;
        public static bool RESERVA_4_5 = false;
        public static bool RESERVA_4_6 = false;
        public static bool RESERVA_4_7 = false;


        //SAFETY_PLC
        //1º CARTA
        public static bool EMERGENCIA = false; //M1000.0
        public static bool FECHO_CAMERA = false;
        public static bool FECHO_ILUMINACAO = false;
        public static bool FECHO_ROBOT = false;
        public static bool FECHO_ECRA = false;
        public static bool BARREIRA_APROVADAS = false;
        public static bool BARREIRA_REPROVADAS = false;
        public static bool RESERVA_1000_7 = false;


    }

    public class OUTPUTS
    {
        //plc
        public static bool CMD_DESINDEXA_FACA = false; //M10.0
        public static bool CMD_INDEXA_FACA = false;
        public static bool RESERVA_VALVULA_11 = false;
        public static bool RESERVA_VALVULA_12 = false;
        public static bool CMD_PORTA_DIREITA_ABRE = false;
        public static bool CMD_PORTA_DIREITA_FECHA = false;
        public static bool CMD_PORTA_ESQUERDA_ABRE = false;
        public static bool CMD_PORTA_ESQUERDA_FECHA = false;

        public static bool CMD_RES_VALV_5 = false;  //M11.0
        public static bool CMD_RES_VALV_5_ERROR = false;
        public static bool CMD_RES_VALV_11_2 = false;
        public static bool CMD_RES_VALV_11_3 = false;
        public static bool CMD_RES_VALV_11_4 = false;
        public static bool CMD_RES_VALV_11_5 = false;
        public static bool CMD_RES_VALV_11_6 = false;
        public static bool CMD_RES_VALV_11_7 = false;

        //1º CARTA 16 DO's
        public static bool CMD_RES_VALV_12_0 = false; //M12.0
        public static bool CMD_RES_VALV_12_1 = false;
        public static bool CMD_TOWER_GREEN = false;
        public static bool CMD_TOWER_RED = false;
        public static bool CMD_TOWER_YELLOW = false;
        public static bool CMD_TOWER_BUZZER = false;
        public static bool CMD_LED_START = false;
        public static bool CMD_LED_RED = false;

        public static bool CMD_LED_RESET = false;//M13.0
        public static bool CMD_LED_PC_ON = false;
        public static bool CMD_FECHO_CAMERA = false;
        public static bool CMD_FECHO_ILUMINACAO = false;
        public static bool CMD_FECHO_MONITOR = false;
        public static bool CMD_FECHO_JAULA = false;
        public static bool CMD_RUN_VFR = false;
        public static bool CMD_RESET_VFR = false;

        //2º CARTA 8 DO's
        public static bool CMD_VFR_ON = false;//M14.0
        public static bool OUT_RESERVA_14_1 = false;
        public static bool ORD_GRAB_CONVOYER = false;
        public static bool ORD_TAKE_INSPECTION = false;
        public static bool ORD_APROVE_KNIFE = false;
        public static bool ORD_REPROVE_KNIFE = false;
        public static bool ORD_INI_POSITION = false;
        public static bool OUT_RESERVA_14_7 = false;


        //SAFETY_PLC
        //1º CARTA
        public static bool CMD_CORTE_AR = false; //M1010.0
        public static bool EMG_ROBOT = false;
        public static bool PORTAS_ROBOT = false;
        public static bool CMD_RESERVA = false;

    }

    public class MOTOR_HMI
    {
        public int DbNumber { get; private set; } = 0;

        /// <summary>
        /// CONDICOES DE FUNCIONAMENTO
        /// </summary>
        public bool INTERLOCK { get; private set; } = false;
        /// <summary>
        /// O MOTOR ESTÁ A MOVIMENTAR-SE
        /// </summary>
        public bool RUN { get; private set; } = false;
        /// <summary>
        /// 1 - FRENTE | 0 - TRAS
        /// </summary>
        public bool DIRECTION { get; private set; } = false;

        /// <summary>
        /// O MOTOR ESTÁ EM ALARME
        /// </summary>
        public bool ALARM { get; private set; } = false;
        public bool MOTOR_RODANDO_FRENTE
        {
            get
            {
                return !this.ALARM && this.RUN && this.DIRECTION;
            }
        }
        public bool MOTOR_RODANDO_TRAS
        {
            get
            {
                return !this.ALARM && this.RUN && !this.DIRECTION;
            }
        }

        public bool RESERVE_0_4 { get; private set; } = false;
        public bool RESERVE_0_5 { get; private set; } = false;
        public bool RESERVE_0_6 { get; private set; } = false;
        public bool RESERVE_0_7 { get; private set; } = false;

        public byte RESERVE_BYTE_1 { get; private set; } = 0;

        /// <summary>
        /// CONTADOR DE SEGUNDOS DE TRABALHO
        /// </summary>
        public uint WORK_TIME { get; private set; } = 0;

        public TimeSpan TEMPO_TRABALHO
        {
            get
            {
                return TimeSpan.FromSeconds(this.WORK_TIME);
            }
        }

        public string STR_TEMPO_TRABALHO
        {
            get
            {
                return ((int)this.TEMPO_TRABALHO.TotalHours).ToString("00") + ":" + this.TEMPO_TRABALHO.Minutes.ToString("00") + ":" + this.TEMPO_TRABALHO.Seconds.ToString("00");
            }
        }

        public Color STR_TEMPO_TRABALHO_COLOR
        {
            get
            {
                if (this.ALARM)
                    return DB400.CLOCK_1HZ ? Color.Red : Color.Lavender;
                else
                    return this.RUN && DB400.CLOCK_1HZ ? Color.LimeGreen : Color.Lavender;
            }
        }

        public MOTOR_HMI(int DbNumber)
        {
            this.DbNumber = DbNumber;
        }

        public int UpdateTags(PLC.Siemens.ReadMultiVariables[] vars, int indexOffset)
        {
            int count = 0;

            try
            {
                this.INTERLOCK = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(0));
                this.RUN = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(1));
                this.DIRECTION = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(2));
                this.ALARM = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(3));
                this.RESERVE_0_4 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(4));
                this.RESERVE_0_5 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(5));
                this.RESERVE_0_6 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(6));
                this.RESERVE_0_7 = Convert.ToBoolean(vars[indexOffset + count].ObtemVariavel(7));
                count++;

                this.RESERVE_BYTE_1 = Convert.ToByte(vars[indexOffset + count].ObtemVariavel());
                count++;

                this.WORK_TIME = Convert.ToUInt32(vars[indexOffset + count].ObtemVariavel());
                count++;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateTags(): " + ex.Message);
                count = -1;
            }

            return count;
        }

    }


}
