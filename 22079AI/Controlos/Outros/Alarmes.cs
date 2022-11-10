using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _22079AI
{
    /// <summary>
    /// Individual object of alarm class
    /// </summary>
    public class Alarmes
    {
        /// <summary>
        /// # of alarm registed in database
        /// </summary>
        public byte alarmNum = 0;
        /// <summary>
        /// Text of alarm registed in database
        /// </summary>
        private string alarmText = "";

        /// <summary>
        /// Type of alarm (1 - ERROR / 2 - WARNING / 3 - Normal
        /// </summary>
        private byte alarmType = 0;

        private string strAlarmType = string.Empty;

        /// <summary>
        /// Type of alarm (1 - ERROR / 2 - WARNING / 3 - Normal
        /// </summary>
        public byte AlarmType
        {
            get { return alarmType; }
        }

        public string StrAlarmType
        {
            get { return strAlarmType; }
        }

        /// <summary>
        /// Previous boolean state of alarm
        /// </summary>
        private bool fpState = false;
        private bool acknowledgedAlarm = false;

        /// <summary>
        /// Datetime of start/stop of alarm
        /// </summary>
        public DateTime alarmDate = DateTime.MinValue;
        public bool AcknowledgedAlarm
        {
            get { return acknowledgedAlarm; }
            set { acknowledgedAlarm = value; }
        }
        public bool ActiveAlarm
        {
            get { return fpState; }
        }

        public Alarmes(byte _alarmNum, string _alarmText, byte _alarmType, string _strAlarmType)
        {
            alarmNum = _alarmNum;
            alarmText = _alarmText;
            alarmType = _alarmType;
            strAlarmType = _strAlarmType;
        }

        /// <summary>
        /// Checks the state of alarm
        /// </summary>
        /// <param name="_actualState"></param>
        /// <returns></returns>
        public bool CheckState(bool _actualState)
        {
            if (_actualState && !fpState)
            {   //Activate Alarm
                fpState = true;
                alarmDate = DateTime.Now;
                acknowledgedAlarm = false;
                return true;
            }
            else if (!_actualState && fpState)
            {  //Disable Alarm
                fpState = false;
                acknowledgedAlarm = true;
                alarmDate = DateTime.Now;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Function that inserts a new alarm on database
        /// </summary>
        /// <param name="_strDbConnection"></param>
        /// <returns></returns>
        public bool InsertNewAlarmDB(string _strDbConnection)
        {
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(_strDbConnection))
                using (SqlCommand sqlCmd = new SqlCommand("INSERT INTO HISTORICO_ALARMES (ID_ALARME, TEMPO_INICIO) VALUES (@NUM,@DATE)", sqlConn))
                {
                    sqlCmd.Parameters.Add("@NUM", SqlDbType.TinyInt).Value = alarmNum;
                    sqlCmd.Parameters.Add("@DATE", SqlDbType.DateTime).Value = alarmDate;

                    sqlConn.Open();

                    return sqlCmd.ExecuteNonQuery() == 1;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error inserting alarm #" + alarmNum + ": " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns a formated text alarm
        /// </summary>
        /// <returns></returns>
        public string ReturnFormatedTextAlarm()
        {
            string strAlarmNum = "";

            if (alarmNum < 10)
                strAlarmNum = "0" + Convert.ToString(alarmNum);
            else
                strAlarmNum = Convert.ToString(alarmNum);

            return "ALM" + strAlarmNum + " - " + alarmText;
        }
    }


    public class HandleAlarms
    {
        public Alarmes[] alarms;

        private bool classOK = false;
        private const string NUMBER = "Número";
        private const string DATETIME = "Data/Hora";
        private const string TEXT = "Descrição";
        private const string TYPE = "Nível";
        private string strDbConnection = "";
        private byte numberOfAlarms = 0;
        private bool firstCycle = true;

        public bool ClassOK
        {
            get { return classOK; }
        }

        public bool AlarmesNaoReconhecidos
        {
            get
            {
                return this.TotalAlarmesNaoReconhecidos > 0;
            }
        }

        public int TotalAlarmesAtivos
        {
            get
            {
                int returnValue = 0;

                for (int i = 0; i < numberOfAlarms; i++)
                    if (alarms[i].ActiveAlarm)
                        returnValue++;

                return returnValue;
            }
        }

        public int TotalAlarmesNaoReconhecidos
        {
            get
            {
                int returnValue = 0;

                for (int i = 0; i < numberOfAlarms; i++)
                    if (alarms[i].ActiveAlarm && !alarms[i].AcknowledgedAlarm)
                        returnValue++;

                return returnValue;
            }
        }

        public HandleAlarms(string _strDbConnection)
        {
            strDbConnection = _strDbConnection;
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(_strDbConnection))
                using (SqlCommand sqlCmd = new SqlCommand("SELECT COUNT (ID) FROM ALARMES", sqlConn))
                {
                    sqlConn.Open();
                    numberOfAlarms = Convert.ToByte(sqlCmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                numberOfAlarms = 0;
                classOK = false;
                new CaixaMensagem(ex.Message, "Erro obter número de alarmes", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
            }

            if (numberOfAlarms > 0)
            {
                alarms = new Alarmes[numberOfAlarms];

                int i = 0;
                try
                {
                    using (SqlConnection sqlConn = new SqlConnection(_strDbConnection))
                    using (SqlCommand sqlCmd = new SqlCommand("SELECT ALARMES.ID, ALARMES.TEXTO, ALARMES.ID_NIVEL_ALARME,  NIVEL_ALARMES.TEXTO FROM ALARMES INNER JOIN NIVEL_ALARMES ON ALARMES.ID_NIVEL_ALARME = NIVEL_ALARMES.ID ORDER BY ALARMES.ID", sqlConn))
                    {
                        sqlConn.Open();

                        using (SqlDataReader dr = sqlCmd.ExecuteReader())
                            if (dr != null)
                                while (dr.Read())
                                {
                                    if (i == alarms.Length)
                                        break;

                                    alarms[i] = new Alarmes(Convert.ToByte(dr[0]), Convert.ToString(dr[1]), Convert.ToByte(dr[2]), Convert.ToString(dr[3]));
                                    i++;
                                }
                    }
                    classOK = true;
                }
                catch (Exception ex)
                {
                    new CaixaMensagem(ex.Message, "Erro obter texto alarmes", CaixaMensagem.TipoMsgBox.Error).ShowDialog();
                    numberOfAlarms = 0;
                    classOK = false;
                    return;
                }
            }
        }

        public bool UpdateAlarms(bool[] _alarmBits, out DataTable table)
        {
            if (_alarmBits.Length == numberOfAlarms)
                try
                {
                    bool newAlarm = false;

                    table = new DataTable();
                    table.Columns.Add(NUMBER, typeof(string));
                    table.Columns.Add(DATETIME, typeof(string));
                    table.Columns.Add(TEXT, typeof(string));
                    table.Columns.Add(TYPE, typeof(string));

                    for (int i = 0; i < numberOfAlarms; i++)
                    {
                        if (alarms[i].CheckState(_alarmBits[i]))
                        {
                            alarms[i].InsertNewAlarmDB(strDbConnection); //Insert new alarm in DB
                            newAlarm = true;
                        }
                        if (_alarmBits[i])
                        {
                            DataRow row = table.NewRow();
                            row[NUMBER] = Convert.ToString(alarms[i].alarmNum);
                            row[DATETIME] = String.Format("{0:dd/MM HH:mm:ss}", alarms[i].alarmDate);
                            row[TEXT] = alarms[i].ReturnFormatedTextAlarm();
                            row[TYPE] = alarms[i].StrAlarmType;
                            table.Rows.Add(row);
                        }
                    }

                    //Sort table by Datetime column
                    table.DefaultView.Sort = DATETIME + " desc";

                    return newAlarm || firstCycle;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("UpdateAlarms(): " + ex.Message);
                    table = null;
                    return false;
                }
                finally
                {
                    firstCycle = false;
                }
            else
            {
                table = null;
                return false;
            }
        }

        public void AcknowledgeAllActiveAlarms()
        {
            for (int i = 0; i < numberOfAlarms; i++)
                if (alarms[i].ActiveAlarm)
                    alarms[i].AcknowledgedAlarm = true;
        }
    }
}
