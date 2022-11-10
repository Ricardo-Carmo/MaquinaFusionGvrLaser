using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace _22079AI
{
    public class FerramentasSistema
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime([In] ref SYSTEMTIME st);

        public static void SetTime(DateTime dataIn)
        {
            try
            {
                // Call the native GetSystemTime method 
                // with the defined structure.
                SYSTEMTIME systime = new SYSTEMTIME();

                systime.wYear = (short)dataIn.Year;
                systime.wMonth = (short)dataIn.Month;
                systime.wDayOfWeek = (short)dataIn.DayOfWeek;
                systime.wDay = (short)dataIn.Day;
                systime.wHour = (short)dataIn.Hour;
                systime.wMinute = (short)dataIn.Minute;
                systime.wSecond = (short)dataIn.Second;
                systime.wMilliseconds = 0;

                SetSystemTime(ref systime);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SetTime(): " + ex.Message);
            }
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

}
