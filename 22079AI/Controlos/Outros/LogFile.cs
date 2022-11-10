using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public class LogFile
    {
        private string logPath = Application.StartupPath + @"\Log.txt";
        private int maxKbFileSize = 1024;
        private object locker = new object();

        /// <summary>
        /// Tamanho atual do ficheiro, em kb
        /// </summary>
        public int FileSize
        {
            get { return this.GetFileSize(this.logPath); }
        }

        public LogFile(string _logPath, int _maxKbFileSize)
        {
            logPath = _logPath;
            maxKbFileSize = _maxKbFileSize;

            if (!File.Exists(logPath))
                File.Create(logPath);
        }

        /// <summary>
        /// Adiciona uma nova linha no ficheiro de log
        /// </summary>
        /// <param name="text">Texto a escrever. Não é necessário inserir quebra de linha!</param>
        /// <param name="includeDatetime">Incluir timestamp no inicio da linha?</param>
        public void WriteLine(string text, bool includeDatetime = true)
        {
            lock (locker)
                try
                {
                    if (File.Exists(logPath))
                        this.CheckFileSize(logPath, maxKbFileSize);
                    else
                        File.Create(logPath);

                    //Gravar no ficheiro o texto
                    File.AppendAllText(logPath, (includeDatetime ? (DateTime.Now.ToString("dd/MM/yy HH:mm:ss") + ": ") : string.Empty) + text + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("LogFile.WriteLine(): " + ex.Message);
                }
        }

        /// <summary>
        /// Verifica o tamanho do ficheiro e caso tenha chegado ao setpoint de tamanho cria um ficheiro antigo com os dados e esvazia o atual.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="limitSize"></param>
        private void CheckFileSize(string filePath, int limitSize)
        {
            //Caso o tamanho do ficheiro seja igual ou superior ao SP para o ficheiro
            if (this.GetFileSize(filePath) >= limitSize)
            {
                string newFileName = Path.GetFileNameWithoutExtension(filePath) + "_" + Convert.ToString(Diversos.ObterTempoUnixAtual()) + Path.GetExtension(filePath);

                //Verificar que já não existe um ficheiro criado com o mesmo nome
                if (File.Exists(newFileName))
                    File.Delete(newFileName);

                //Vamos mover o conteudo do ficheiro antigo para um novo ficheiro
                File.Move(filePath, newFileName);

                Debug.WriteLine("Tamanho máximo do ficheiro excedido! Ficheiro movido com o seguinte filename: " + newFileName);
            }
        }

        /// <summary>
        /// Obtem o tamanho do ficheiro, em kb
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private int GetFileSize(string filePath)
        {
            return Convert.ToInt32(new FileInfo(filePath).Length / 1024.0);
        }
    }
}
