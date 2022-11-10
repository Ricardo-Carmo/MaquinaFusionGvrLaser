#region Usings
using System;
using Microsoft.SqlServer;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using Microsoft.VisualBasic;
using System.IO.Ports;
using System.Security.Cryptography;
using System.Net;
using System.Configuration;
using System.Web;
using System.Net.Mail;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Management;
#endregion

namespace _22079AI
{

    #region Enums

    public enum TipoIP
    {
        IPExterno,
        IPInterno
    }

    public enum TipoMsg
    {
        Informacao,
        Aviso,
        Erro,
        Questao
    }

    public enum ArrayOrder
    {
        ascendente,
        descendente
    }

    #endregion

    public class CycleMultiplier
    {
        private int numberOfTimes = 0;
        private int counter = 0;
        private bool isReached = false;

        public bool IsReached
        {
            get
            {
                return this.isReached;
            }
        }

        public CycleMultiplier(int _numberOfTimes)
        {
            this.numberOfTimes = _numberOfTimes;
        }

        public void UpdateCycleCount(bool enabled)
        {
            if (enabled)
            {
                this.counter++;

                if (this.counter >= this.numberOfTimes)
                {
                    this.isReached = true;
                    this.counter = 0;
                }
                else
                    this.isReached = false;
            }
            else
            {
                this.counter = 0;
                this.isReached = false;
            }
        }

    }

    public class Forms
    {
        //Formulário Principal
        private static MainForm _mainMenu;
        public static MainForm MainForm
        {
            get
            {
                if (_mainMenu == null)
                    _mainMenu = new MainForm();
                return _mainMenu;
            }
        }
    }


    public class GereEstadoComunicacao
    {
        private ulong contadorConexoes = 0;
        private bool ultimoEstadoLigacao = false;
        private int tempoWatchDog = 0;
        private DateTime ultimoTempoMedido = DateTime.MinValue;

        public bool EstadoLigacao
        {
            get
            {
                bool estado = this.TempoPassadoDesdeUltimoHeartBeat < this.tempoWatchDog;

                if (estado && !this.ultimoEstadoLigacao)
                {
                    this.contadorConexoes++;
                    Debug.WriteLine("Contador de conexões: " + this.contadorConexoes);
                }

                this.ultimoEstadoLigacao = estado;

                return estado;
            }
        }

        /// <summary>
        /// Retorna o tempo passado desde o último heartbeat recebido
        /// </summary>
        public double TempoPassadoDesdeUltimoHeartBeat
        {
            get
            {
                return (DateTime.Now - this.ultimoTempoMedido).TotalMilliseconds;
            }
        }

        /// <summary>
        /// Retorna o total de conexões efetuadas
        /// </summary>
        public ulong TotalConexoes
        {
            get { return this.contadorConexoes; }
        }

        /// <summary>
        /// Retorna o total de conexões perdidas
        /// </summary>
        public ulong TotalConexoesPerdidas
        {
            get { return (this.contadorConexoes > 0) ? this.contadorConexoes - 1 : 0; }
        }

        public GereEstadoComunicacao(int _tempoWatchDog)
        {
            this.tempoWatchDog = _tempoWatchDog;
        }

        public void HeartBeat()
        {
            this.ultimoTempoMedido = DateTime.Now;
        }

        private void LimpaValores()
        {
            this.contadorConexoes = 0;
            this.ultimoEstadoLigacao = false;
            this.ultimoTempoMedido = DateTime.MinValue;
        }
    }


    //Class universal com diversas funções
    public static class Diversos
    {

        #region Buscar do config

        //Retorna true /false apartir de uma string 0/1
        public static bool RetornaTrueFalseDeStringFicheiroINI(FicheiroINI iniFile, string seccao, string campo, bool valorDefeito)
        {
            if (iniFile != null)
            {
                if (iniFile.LeFicheiroINI(seccao, campo) == "")
                    iniFile.EscreveFicheiroINI(seccao, campo, Convert.ToString(Convert.ToInt32(valorDefeito)));

                switch (Convert.ToInt32(iniFile.LeFicheiroINI(seccao, campo)))
                {
                    case 0:
                        return false;
                    case 1:
                        return true;
                    default:
                        return true;
                }
            }
            else
                return false;
        }

        public static bool IsOdd(int value)
        {
            return value % 2 != 0;
        }

        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        #endregion

        #region Funções Temporais
        public static DateTime GetInternetTime()
        {
            using (WebResponse response = WebRequest.Create("http://www.google.com").GetResponse())
                return DateTime.ParseExact(response.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.AssumeUniversal);
        }

        //Obtem a hora no seguinte formato 00:00:00
        public static string ObtemHora()
        {
            try
            {
                string hora = DateTime.Now.Hour.ToString();
                string minuto = DateTime.Now.Minute.ToString();
                string segundo = DateTime.Now.Second.ToString();

                if (hora.Length == 1)
                    hora = "0" + hora;

                if (minuto.Length == 1)
                    minuto = "0" + minuto;

                if (segundo.Length == 1)
                    segundo = "0" + segundo;

                return hora + ":" + minuto + ":" + segundo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }

        }

        //Função para calcular tempo de s para 00:00:00
        public static string CalculaTempo(int TempoEmSegundos)
        {
            try
            {
                int tempoSegundos = TempoEmSegundos;
                string dias = "";
                int interacoes = 0;

                while (tempoSegundos > 86400)
                {
                    interacoes++;
                    tempoSegundos = tempoSegundos - 86400;
                }

                if (interacoes > 0)
                    dias = Convert.ToString(interacoes) + "d ";

                string horas = Convert.ToString(tempoSegundos / (60 * 60));
                string minutos = Convert.ToString(((tempoSegundos / 60) - (tempoSegundos / (60 * 60) * 60)));
                string segundos = Convert.ToString(tempoSegundos % 60);

                if (Convert.ToInt32(horas) < 10)
                    horas = "0" + horas;

                if (Convert.ToInt32(minutos) < 10)
                    minutos = "0" + minutos;

                if (Convert.ToInt32(segundos) < 10)
                    segundos = "0" + segundos;

                return dias + horas + ":" + minutos + ":" + segundos;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CalculaTempo(): " + ex.Message);
                return "error date";
            }
        }

        //Função para contar dias úteis entre duas datas
        public static int ObtemDiasUteis(DateTime initialDate, DateTime finalDate)
        {
            try
            {
                int days = 0;
                int daysCount = 0;
                days = initialDate.Subtract(finalDate).Days;

                if (days < 0)

                    days = days * -1;
                for (int i = 1; i <= days; i++)
                {
                    initialDate = initialDate.AddDays(1);
                    //Conta apenas dias da semana.
                    if (initialDate.DayOfWeek != DayOfWeek.Sunday && initialDate.DayOfWeek != DayOfWeek.Saturday)
                        daysCount++;
                }
                return daysCount;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        //Função que obtem o tempo unix atual
        public static int ObterTempoUnixAtual()
        {
            return Convert.ToInt32(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        //Converte uma data para tempo unix
        public static int ConvertDatetimeParaUnix(DateTime data)
        {
            return Convert.ToInt32(data.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        //Converte um tempo unix para uma datetime
        public static DateTime ConvertUnixParaDatetime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            return new DateTime(1970, 1, 1).AddSeconds(unixTimeStamp);
        }

        //Obtem por extenso o dia da semana em PT-PT
        public static string ObtemDiaSemana(DateTime _datetime)
        {
            return _datetime.ToString("dddd", new CultureInfo("pt-PT"));
        }

        #endregion

        #region Manipulação de Strings/Números

        public static bool InRange(int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }
        public static bool InRange(double value, double minimum, double maximum)
        {
            return value >= minimum && value <= maximum;
        }
        public static bool InRange(long value, long minimum, long maximum)
        {
            return value >= minimum && value <= maximum;
        }


        /// <summary>
        /// Calcula a percentagem de 1 determinado número
        /// </summary>
        /// <param name="_valMaximo">Valor Máximo (Correspondente a 100%)</param>
        /// <param name="_valAtual">Valor Atual a calcular</param>
        /// <returns></returns>
        public static double CalculaPercentagem(double _valMaximo, double _valAtual)
        {
            return ((_valAtual * 100) / _valMaximo);
        }

        /// <summary>
        /// Calcula o valor absoluto apartir da percentagem
        /// </summary>
        /// <param name="_valorMaximo"></param>
        /// <param name="_valorEmPercentagem"></param>
        /// <returns></returns>
        public static double CalculaValorApartirDaPercentagem(double _valorMaximo, double _valorEmPercentagem)
        {
            return ((_valorMaximo * _valorEmPercentagem) / 100);
        }

        /// <summary>
        /// Função que descobre se os numeros são multiplos
        /// </summary>
        /// <param name="valA"></param>
        /// <param name="valB"></param>
        /// <returns></returns>
        public static bool DescobreMultiplo(int valA, int valB)
        {
            return (valA % valB) == 0;
        }

        //Função que limita determinado/os caracter/es indicados 
        public static bool LimitaCaracteres(KeyPressEventArgs e, string CaracterLimitado)
        {
            try
            {
                for (int i = 0; i < CaracterLimitado.Length; i++)
                {
                    if (e.KeyChar == CaracterLimitado[i])
                    {
                        e.Handled = true;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }


        }

        //Função para verificar se valor é numerico
        public static bool IsNumeric(object value)
        {
            try
            {
                if (value == null || value is DateTime)
                    return false;

                if (value is Int16 || value is Int32 || value is Int64 || value is Decimal || value is Single || value is Double || value is System.Boolean)
                    return true;

                if (value is string)
                    Double.Parse(value as string);
                else
                    Double.Parse(value.ToString());
                return true;
            }
            catch { return false; }
        }

        //Função que percorre todo uma string e retira todos os acentos/caracteres especiais
        public static string TirarAcentos(string texto)
        {
            try
            {
                string textor = "";

                for (int i = 0; i < texto.Length; i++)
                {
                    if (texto[i].ToString() == "ã")
                        textor += "a";
                    else if (texto[i].ToString() == "á")
                        textor += "a";
                    else if (texto[i].ToString() == "à")
                        textor += "a";
                    else if (texto[i].ToString() == "â")
                        textor += "a";
                    else if (texto[i].ToString() == "ä")
                        textor += "a";
                    else if (texto[i].ToString() == "é")
                        textor += "e";
                    else if (texto[i].ToString() == "è")
                        textor += "e";
                    else if (texto[i].ToString() == "ê")
                        textor += "e";
                    else if (texto[i].ToString() == "ë")
                        textor += "e";
                    else if (texto[i].ToString() == "í")
                        textor += "i";
                    else if (texto[i].ToString() == "ì")
                        textor += "i";
                    else if (texto[i].ToString() == "ï")
                        textor += "i";
                    else if (texto[i].ToString() == "õ")
                        textor += "o";
                    else if (texto[i].ToString() == "ó")
                        textor += "o";
                    else if (texto[i].ToString() == "ò")
                        textor += "o";
                    else if (texto[i].ToString() == "ö")
                        textor += "o";
                    else if (texto[i].ToString() == "ú")
                        textor += "u";
                    else if (texto[i].ToString() == "ù")
                        textor += "u";
                    else if (texto[i].ToString() == "ü")
                        textor += "u";
                    else if (texto[i].ToString() == "ç")
                        textor += "c";
                    else if (texto[i].ToString() == "Ã")
                        textor += "A";
                    else if (texto[i].ToString() == "Á")
                        textor += "A";
                    else if (texto[i].ToString() == "À")
                        textor += "A";
                    else if (texto[i].ToString() == "Â")
                        textor += "A";
                    else if (texto[i].ToString() == "Ä")
                        textor += "A";
                    else if (texto[i].ToString() == "É")
                        textor += "E";
                    else if (texto[i].ToString() == "È")
                        textor += "E";
                    else if (texto[i].ToString() == "Ê")
                        textor += "E";
                    else if (texto[i].ToString() == "Ë")
                        textor += "E";
                    else if (texto[i].ToString() == "Í")
                        textor += "I";
                    else if (texto[i].ToString() == "Ì")
                        textor += "I";
                    else if (texto[i].ToString() == "Ï")
                        textor += "I";
                    else if (texto[i].ToString() == "Õ")
                        textor += "O";
                    else if (texto[i].ToString() == "Ó")
                        textor += "O";
                    else if (texto[i].ToString() == "Ò")
                        textor += "O";
                    else if (texto[i].ToString() == "Ö")
                        textor += "O";
                    else if (texto[i].ToString() == "Ú")
                        textor += "U";
                    else if (texto[i].ToString() == "Ù")
                        textor += "U";
                    else if (texto[i].ToString() == "Ü")
                        textor += "U";
                    else if (texto[i].ToString() == "Ç")
                        textor += "C";
                    else
                        textor += texto[i];
                }
                return textor;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro função retira acentos: " + ex.ToString());
                return null;
            }

        }

        //Obter número inteiro32 aleatóriamente
        public static int ObterIntAleatorio(int rangeMin, int rangeMax)
        {
            return new Random().Next(rangeMin, rangeMax);
        }

        //Ordenar array de inteiros
        public static int[] OrdenarArrayInt(int[] arrayInt, ArrayOrder arrayorder)
        {
            try
            {
                if (Convert.ToInt32(arrayorder) == 0)
                    Array.Sort(arrayInt);
                else if (Convert.ToInt32(arrayorder) == 1)
                    Array.Reverse(arrayInt);
                return arrayInt;
            }
            catch (Exception)
            {
                int[] arr = { };
                return arr;
            }

        }

        //Remover valores repetidos ArrayList
        public static ArrayList RemoverRepetidosArrayList(ArrayList items)
        {
            ArrayList noDups = new ArrayList();

            foreach (string strItem in items)
            {

                if (!noDups.Contains(strItem.Trim()))
                {

                    noDups.Add(strItem.Trim());
                }
            }

            noDups.Sort();

            return noDups;

        }

        //Converter decimal para binário
        public static string DecimalParaBinario(string data)
        {
            string result = string.Empty;
            int rem = 0;
            try
            {
                if (!IsNumeric(data))
                    return "Invalid Value - This is not a numeric value";
                else
                {
                    int num = int.Parse(data);
                    while (num > 0)
                    {
                        rem = num % 2;
                        num = num / 2;
                        result = rem.ToString() + result;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
            return result;
        }

        //Converter binário para decimal
        public static int BinarioParaDecimal(string valorBinario)
        {
            int expoente = 0;
            int numero;
            int soma = 0;

            string numeroInvertido = InverterString(valorBinario);

            try
            {
                for (int i = 0; i < numeroInvertido.Length; i++)
                {
                    //pega dígito por dígito do número digitado
                    numero = Convert.ToInt32(numeroInvertido.Substring(i, 1));

                    //multiplica o dígito por 2 elevado ao expoente, e armazena o resultado em soma
                    soma += numero * (int)Math.Pow(2, expoente);

                    // incrementa o expoente
                    expoente++;
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return soma;

        }

        //Inverte uma string
        public static string InverterString(string str)
        {

            int tamanho = str.Length;

            char[] caracteres = new char[tamanho];

            for (int i = 0; i < tamanho; i++)
            {

                caracteres[i] = str[tamanho - 1 - i];

            }

            return new string(caracteres);

        }

        //Encontra número máximo num array
        public static double EncontraMaximoArray(double[] array)
        {
            return array.Max();
        }

        //Encontra número minimo num array
        public static double EncontraMinimoArray(double[] array)
        {
            return array.Min();
        }

        //Função que faz uma regra de 3 simples e retorna o valor pretendido
        public static double FazRegra3Simples(double valMaxSemUnidade, double valAtualsemUnidade, double valMaxEmMM)
        {
            try
            {
                double x = 0;
                x = (valAtualsemUnidade * valMaxEmMM);
                x = (x / valMaxSemUnidade);
                return x;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro função FazRegra3Simples: " + ex.ToString());
                return 0;
            }


        }

        //Função para fazer interpolação de pontos
        public static double InterpolacaoPontos(double x0, double y0, double x1, double y1, bool Ordem)
        {
            try
            {

                if (Ordem)
                {
                    //Se true salta em ordem a Y
                    Debug.WriteLine("(((" + y1 + "-" + y0 + ") / (" + x1 + "-" + x0 + ")) * (" + x1 + "-" + x0 + ") + " + y0 + ")");
                    return (((y1 - y0) / (x1 - x0)) * (x1 - x0) + y0);
                }

                else
                {
                    //Se falso salta em ordem a X
                    return (((x1 - x0) / (y1 - y0)) * (y1 - y0) + x0);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro função InterpolacaoPontos: " + ex.ToString());
                return 0;
            }
        }

        //Retorna o tamanho de A4 conforme a resolução pretendida
        public static Size A4SizeInPixels(int dpi)
        {
            int width = (int)Math.Round(210 / 25.4 * dpi);
            int height = (int)Math.Round(297 / 25.4 * dpi);
            return new Size(width, height);
        }


        /// <summary>
        /// Função que quebra uma string em várias quebras de linhas após x caracteres
        /// </summary>
        public static string DivideString(string _strIn, int _lineLength)
        {
            try
            {
                int counter = 0;
                StringBuilder strTratada = new StringBuilder();
                foreach (var c in _strIn)
                {
                    counter++;
                    if (char.IsWhiteSpace(c))
                        if (counter >= _lineLength)
                        {
                            counter = 0;
                            strTratada.Append(c + Environment.NewLine);
                        }
                        else
                            strTratada.Append(c);
                    else
                        strTratada.Append(c);
                }
                return strTratada.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return "Erro ao tratar string";
            }

        }
        #endregion

        #region Manipulação de Ficheiros

        /// <summary>
        /// Check if a directory is Windows valid
        /// </summary>
        /// <param name="path">Directory to check</param>
        /// <returns>Return TRUE if is valid</returns>
        public static bool IsValidPath(string path)
        {
            Regex driveCheck = new Regex(@"^[a-zA-Z]:\\$");
            if (!driveCheck.IsMatch(path.Substring(0, 3)))
                return false;
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidPathChars());
            strTheseAreInvalidFileNameChars += @":/?*" + "\"";
            Regex containsABadCharacter = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");
            if (containsABadCharacter.IsMatch(path.Substring(3, path.Length - 3)))
                return false;

            DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(path));
            if (!dir.Exists)
                dir.Create();
            return true;
        }

        //Copia uma pasta e respetivos sub folders de um diretorio para outro
        public static bool CopiaDiretorio(string sourceDirName, string destDirName, bool copySubDirs)
        {
            try
            {
                var dir = new DirectoryInfo(sourceDirName);
                var dirs = dir.GetDirectories();


                // If the destination directory does not exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                //Checks if last character is "\"
                if (destDirName.Substring(destDirName.Length - 1, 1) != @"\")
                {
                    destDirName += @"\";
                }

                //Gets folder name
                string nomePasta = sourceDirName.Substring(sourceDirName.LastIndexOf(@"\"), sourceDirName.Length - sourceDirName.LastIndexOf(@"\"));

                //Create the folder in the directory
                destDirName += nomePasta;

                //create the directory
                Directory.CreateDirectory(destDirName);

                // Get the file contents of the directory to copy.
                var files = dir.GetFiles();

                foreach (var file in files)
                {
                    // Create the path to the new copy of the file.
                    var temppath = Path.Combine(destDirName, file.Name);

                    // Copy the file.
                    file.CopyTo(temppath, true);
                }

                // If copySubDirs is true, copy the subdirectories.
                if (!copySubDirs)
                    return true;

                foreach (var subdir in dirs)
                {
                    // Create the subdirectory.
                    var temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    CopiaDiretorio(subdir.FullName, temppath, copySubDirs);
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

        //Abre Ficheiro especificado
        public static bool AbreFicheiro(string diretorioFicheiro, bool MostraMensagemErro)
        {
            try
            {
                Process.Start(diretorioFicheiro);
                return true;
            }
            catch (System.Exception ex)
            {
                if (MostraMensagemErro)
                    System.Windows.Forms.MessageBox.Show("Erro ao abrir ficheiro: " + ex.ToString(), "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

        //Obtem a informação do ficheiro mais recente num directório
        public static FileInfo GetNewestFile(DirectoryInfo directory)
        {
            try
            {
                return directory.GetFiles()
                    .Union(directory.GetDirectories().Select(d => GetNewestFile(d)))
                    .OrderByDescending(f => (f == null ? DateTime.MinValue : f.LastWriteTime))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao obter ficheiro mais recente:" + ex.Message);
                //     FuncoesLogFile.EscreveLog("Erro ao obter ficheiro mais recente. Mensagem original: " + ex.Message, true);
                return null;
            }
        }

        //Função que abre o Explorer do Windows num determinado diretório
        public static bool AbreExplorador(string filePath)
        {
            try
            {
                Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        //Lê todo o texto de um arquivo
        public static string LerArquivoTexto(string nomeArquivo)
        {
            try
            {
                if (File.Exists(nomeArquivo))
                    return File.ReadAllText(nomeArquivo);
                else
                    return String.Empty;
            }
            catch (Exception)
            {
                return null;
            }

        }

        //Escreve texto num arquivo
        public static bool EscreveArquivoTexto(string diretorio, string texto)
        {
            try
            {
                if (!File.Exists(diretorio))
                {
                    File.Create(diretorio);
                    using (TextWriter tw = new StreamWriter(diretorio))
                    {
                        tw.WriteLine(texto);
                        tw.Close();
                    }
                    return true;
                }
                else if (File.Exists(diretorio))
                {
                    using (TextWriter tw = new StreamWriter(diretorio, true))
                    {
                        tw.WriteLine(texto);
                        tw.Close();
                    }
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao escrever em ficheiro: " + ex.ToString());
                return false;
            }
        }

        //Apaga um diretório e todo o seu conteudo
        public static bool ApagaDiretorio(string path)
        {
            try
            {
                string caminho = @path;
                DirectoryInfo diretorio = new DirectoryInfo(caminho);
                diretorio.Delete(true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao eliminar diretorio: " + ex.ToString());
                return false;
            }

        }

        //Redimensiona uma imagem para o size especificado
        public static System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, Size size)
        {
            int sourceWidth = imgToResize.Width;
            int sourceHeight = imgToResize.Height;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)size.Width / (float)sourceWidth);
            nPercentH = ((float)size.Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();

            return (System.Drawing.Image)b;
        }

        //Imprime um panel
        public static void PrintPanel(System.Windows.Forms.Panel pnl)
        {
            PrintDialog myPrintDialog = new PrintDialog();
            PrintDocument printDocument1 = new PrintDocument();
            System.Drawing.Bitmap memoryImage = new System.Drawing.Bitmap(pnl.Width, pnl.Height);
            pnl.DrawToBitmap(memoryImage, pnl.ClientRectangle);
            if (myPrintDialog.ShowDialog() == DialogResult.OK)
            {
                System.Drawing.Printing.PrinterSettings values;
                values = myPrintDialog.PrinterSettings;
                myPrintDialog.Document = printDocument1;
                printDocument1.PrintController = new StandardPrintController();
                printDocument1.Print();
            }
            printDocument1.Dispose();
        }
        #endregion

        #region Obter Dados PC/Internet

        public static string GetIpRemoto(string url)
        {
            try
            {
                //Cria uma requisição para a URL
                WebRequest rq = WebRequest.Create(url);

                //obtém o response a partir do request
                HttpWebResponse rp = (HttpWebResponse)rq.GetResponse();
                //obtém um stream contendo a resposta retornada pelo servidor
                Stream ds = rp.GetResponseStream();
                //Cria um StreamReader para leitura
                StreamReader rd = new StreamReader(ds);
                //Lê os dados
                string responseFromServer = rd.ReadToEnd();
                //fecha os objetos
                rd.Close();
                ds.Close();
                rp.Close();
                //procura por indexafor fixo no resultado 
                string URL = "IP";
                int i = responseFromServer.IndexOf(URL) + URL.Length + 2;
                //captura o IP descoberto
                URL = string.Empty;
                while (!(responseFromServer[i].ToString() == "<"))
                {
                    URL += responseFromServer[i];
                    i += 1;
                }

                return URL.Trim();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao obter ip remoto: " + ex.ToString());
                return null;
            }

        }

        public static bool VerificaLigacaoInternet()
        {

            try
            {
                System.Uri Url = new System.Uri("http://www.google.com"); //é sempre bom por um site que costuma estar sempre on, para n haver problemas

                System.Net.WebRequest WebReq;
                System.Net.WebResponse Resp;
                WebReq = System.Net.WebRequest.Create(Url);

                try
                {
                    Resp = WebReq.GetResponse();
                    Resp.Close();
                    WebReq = null;
                    return true;
                }

                catch
                {
                    WebReq = null;
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static string ObtemNomePC()
        {
            try
            {
                return System.Windows.Forms.SystemInformation.ComputerName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ObtemNomeUtilizadorLoggado()
        {
            try
            {
                return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static string ObtemConteudoPaginaWeb(string url)
        {
            string result = "";
            try
            {
                System.Net.HttpWebRequest wreq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                wreq.Method = "GET";
                wreq.Timeout = 3000;
                System.Net.HttpWebResponse wr = (System.Net.HttpWebResponse)wreq.GetResponse();

                if (wr.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    System.IO.Stream s = wr.GetResponseStream();
                    System.Text.Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                    System.IO.StreamReader readStream = new System.IO.StreamReader(s, enc);
                    result = readStream.ReadToEnd();
                }
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Função obtem conteudo pagina web: " + ex.ToString());
                return null;
            }

        }

        public static string[] ListarTodosOsDrivesPC()
        {
            //Listagem de todos os drivers DVD/CD existentes no PC
            try
            {
                string[] drives = Directory.GetLogicalDrives();
                return drives;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static long VerMemoriaUsadaPrograma()
        {
            try
            {
                return GC.GetTotalMemory(true);
            }
            catch (Exception)
            {
                return 0;
            }

        }

        public static string GetOSName()
        {
            System.OperatingSystem os = System.Environment.OSVersion;
            string osName = "Unknown";


            switch (os.Platform)
            {
                case System.PlatformID.Win32Windows:
                    switch (os.Version.Minor)
                    {
                        case 0:
                            osName = "Windows 95";
                            break;
                        case 10:
                            osName = "Windows 98";
                            break;
                        case 90:
                            osName = "Windows ME";
                            break;
                    }
                    break;
                case System.PlatformID.Win32NT:
                    switch (os.Version.Major)
                    {
                        case 3:
                            osName = "Windws NT 3.51";
                            break;
                        case 4:
                            osName = "Windows NT 4";
                            break;
                        case 5:
                            if (os.Version.Minor == 0)
                                osName = "Windows 2000";
                            else if (os.Version.Minor == 1)
                                osName = "Windows XP";
                            else if (os.Version.Minor == 2)
                                osName = "Windows Server 2003";
                            break;
                        case 6:
                            osName = "Windows Vista";
                            break;
                    }
                    break;
            }

            return osName + ", " + os.Version.ToString();
        }

        #endregion

        public static string NumberToString(double number, int numberOfDecimals, bool commaAsDecimalSeparator)
        {
            try
            {
                string returnString = string.Empty;
                string decimals = string.Empty;

                for (int i = 0; i < numberOfDecimals; i++)
                    decimals += "0";

                number = Math.Round(number, numberOfDecimals);
                returnString = number.ToString("0." + decimals); //Faz o string.format ao double
                if (!commaAsDecimalSeparator)
                    returnString = returnString.Replace(",", ".");

                return returnString;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("NumberToString(): " + ex.Message);
                return "####";
            }
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
                throw new NullReferenceException("The string cannot be null or empty");

            hexString = hexString.Replace(":", "");
            hexString = hexString.Replace("-", "");

            if (hexString.Length % 2 != 0)
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));

            byte[] HexAsBytes = new byte[hexString.Length / 2];

            for (int index = 0; index < HexAsBytes.Length; index++)
                HexAsBytes[index] = byte.Parse(hexString.Substring(index * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);


            return HexAsBytes;
        }


        /// <summary>
        /// Função que indica se uma aplicação com o mesmo nome está a correr no sistema
        /// </summary>
        /// <param name="mostraMSG"></param>
        /// <returns></returns>
        public static bool PrevineDuplaExecucao(bool mostraMSG = true)
        {
            try
            {
                //Obter nome do processo a decorrer
                var currentProc = System.Diagnostics.Process.GetCurrentProcess();
                string name = currentProc.ProcessName;

                //Verificar nos processos activos se já existe algum com o mesmo nome a correr
                Process[] result = Process.GetProcessesByName(name);
                if (result.Length > 1)
                {
                    if (mostraMSG)
                        System.Windows.Forms.MessageBox.Show("Failed to start application. The app is already running on this computer", "Information", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                if (mostraMSG)
                    System.Windows.Forms.MessageBox.Show("Erro fatal inicialização: " + ex.ToString());
                return true;
            }
            finally
            {
                GC.Collect();
            }
        }

        public static bool TextBoxENula(TextBox txtIn)
        {
            try
            {
                bool retorno = false;
                if (txtIn.InvokeRequired)
                    txtIn.Invoke(new MethodInvoker(() =>
                    {
                        if (string.IsNullOrWhiteSpace(txtIn.Text))
                            retorno = true;
                        else
                            retorno = false;
                    }));
                else
                {
                    if (string.IsNullOrWhiteSpace(txtIn.Text))
                        retorno = true;
                    else
                        retorno = false;
                }
                return retorno;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }

        }

        /// <summary>
        /// Executa um ping ao IP especificado
        /// </summary>
        /// <param name="_enderecoIP">Retorna TRUE se ping for efetuada, FALSE se não</param>
        /// <returns></returns>
        public static bool FazPing(string _enderecoIP, bool mostraInfoDebug = true)
        {
            try
            {
                using (Ping pingSender = new Ping())
                {
                    PingReply reply = pingSender.Send(IPAddress.Parse(_enderecoIP));

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
                    {
                        if (mostraInfoDebug)
                            Debug.WriteLine("Função Ping-> IP Address: " + reply.Address.ToString() + " sem resposta");
                        throw new Exception(Convert.ToString(reply.Status));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Função que limita textboxs, só permitindo numeros e virgula
        /// </summary>
        /// <param name="_txtIn"></param>
        /// <param name="_e"></param>
        public static void LimitaTextBox(TextBox _txtIn, KeyPressEventArgs _e)
        {
            const char Delete = (char)8;
            const char Virgula = (char)44;
            if (_txtIn.Text.IndexOf(",") == -1)
                _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete && _e.KeyChar != Virgula;
            else
                _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete;
        }

        public static void LimitaTextBox(TextBox _txtIn, KeyPressEventArgs _e, bool _aceitaVirgula, bool _aceitaNumerosNegativos)
        {
            const char Delete = (char)8;
            const char Virgula = (char)44;
            const char Negativo = (char)45;

            if (!_aceitaVirgula && !_aceitaNumerosNegativos)
                _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete;
            else if (_aceitaVirgula && !_aceitaNumerosNegativos)
                if (_txtIn.Text.IndexOf(",") == -1)
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete && _e.KeyChar != Virgula;
                else
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete;
            else if (!_aceitaVirgula && _aceitaNumerosNegativos)
                if (_txtIn.Text.IndexOf("-") == -1)
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete && _e.KeyChar != Negativo;
                else
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete;
            else
            {
                if (_txtIn.Text.IndexOf("-") == -1 && _txtIn.Text.IndexOf(",") == -1)
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete && _e.KeyChar != Negativo && _e.KeyChar != Virgula;
                else if (_txtIn.Text.IndexOf("-") != -1 && _txtIn.Text.IndexOf(",") == -1)
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete && _e.KeyChar != Virgula;
                else if (_txtIn.Text.IndexOf("-") == -1 && _txtIn.Text.IndexOf(",") != -1)
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete && _e.KeyChar != Negativo;
                else
                    _e.Handled = !Char.IsDigit(_e.KeyChar) && _e.KeyChar != Delete;
            }

        }


        /// <summary>
        /// Função que faz o controlo de Text/BackColors do controlo passado por parametro 
        /// </summary>
        /// <param name="_controlo">Controlo (TextBox, Button, Label, etc)</param>
        /// <param name="_estado">Estado (ON/OFF)</param>
        /// <param name="_backColorOn">Cor de fundo quando ON</param>
        /// <param name="_backColorOff">Cor de fundo quando OFF</param>
        /// <param name="_textOn">Texto quando ON (Opcional)</param>
        /// <param name="_textOff">Texto quando OFF (Opcional)</param>
        /// <returns>Retorna TRUE se bem executado. FALSE se ocorrer um erro ou o controlo for == null</returns>
        public static bool ConfiguraControl(Control _controlo, bool _estado, Color _backColorOn, Color _backColorOff, string _textOn = "", string _textOff = "")
        {
            try
            {
                if (_controlo != null)
                {
                    if (_estado)
                    {
                        if (_textOn != "")
                            _controlo.Text = _textOn;

                        _controlo.BackColor = _backColorOn;

                        if (_controlo is Label || _controlo is TextBox)
                            _controlo.ForeColor = Color.Black;
                    }
                    else
                    {
                        if (_textOff != "")
                            _controlo.Text = _textOff;

                        _controlo.BackColor = _backColorOff;

                        if (_controlo is Label || _controlo is TextBox)
                            _controlo.ForeColor = Color.White;
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Função que atualiza a backColor do control conforme o booleano que for passado
        /// </summary>
        /// <param name="_controlo">Objecto a alterar</param>
        /// <param name="_estado">Bool de controlo</param>
        /// <returns></returns>
        public static bool AtualizaBackColor(Control _controlo, bool _estado)
        {
            bool valorRetorno = false;
            try
            {
                if (_controlo.InvokeRequired)
                    _controlo.Invoke(new MethodInvoker(() =>
                    {
                        if (_controlo != null)
                        {
                            if (_estado)
                            {
                                if (_controlo.BackColor != Color.LimeGreen)
                                    _controlo.BackColor = Color.LimeGreen;
                            }
                            else
                                if (_controlo.BackColor != Color.Gray)
                                _controlo.BackColor = Color.Gray;
                            valorRetorno = true;
                        }
                    }));
                else
                {
                    if (_controlo != null)
                    {
                        if (_estado)
                        {
                            if (_controlo.BackColor != Color.LimeGreen)
                                _controlo.BackColor = Color.LimeGreen;
                        }
                        else
                                if (_controlo.BackColor != Color.Gray)
                            _controlo.BackColor = Color.Gray;
                        valorRetorno = true;
                    }
                }
                return valorRetorno;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Função que atualiza a backColor do control conforme o inteiro que for passado
        /// </summary>
        /// <param name="_controlo">Objecto a alterar</param>
        /// <param name="_estado">0 - Sem Resultados; 1 - OK; 2 - OK INV; 3 - NOK; </param>
        /// <returns></returns>
        public static bool AtualizaBackColor(Control _controlo, int _estado)
        {
            bool valorRetorno = false;
            try
            {
                if (_controlo.InvokeRequired)
                    _controlo.Invoke(new MethodInvoker(() =>
                    {

                        switch (_estado)
                        {
                            case 0:
                                {
                                    if (_controlo.BackColor != Color.Gray)
                                        _controlo.BackColor = Color.Gray;
                                    break;
                                }
                            case 1:
                                {
                                    if (_controlo.BackColor != Color.LimeGreen)
                                        _controlo.BackColor = Color.LimeGreen;
                                    break;
                                }
                            case 2:
                                {
                                    if (_controlo.BackColor != Color.LimeGreen)
                                        _controlo.BackColor = Color.LimeGreen;
                                    break;
                                }
                            case 3:
                                {
                                    if (_controlo.BackColor != Color.Red)
                                        _controlo.BackColor = Color.Red;
                                    break;
                                }
                            default:
                                {
                                    if (_controlo.BackColor != Color.WhiteSmoke)
                                        _controlo.BackColor = Color.WhiteSmoke;
                                    break;
                                }

                        }
                        valorRetorno = true;

                    }));
                else
                {
                    switch (_estado)
                    {
                        case 0:
                            {
                                if (_controlo.BackColor != Color.Gray)
                                    _controlo.BackColor = Color.Gray;
                                break;
                            }
                        case 1:
                            {
                                if (_controlo.BackColor != Color.LimeGreen)
                                    _controlo.BackColor = Color.LimeGreen;
                                break;
                            }
                        case 2:
                            {
                                if (_controlo.BackColor != Color.Red)
                                    _controlo.BackColor = Color.Red;
                                break;
                            }
                        case 3:
                            {
                                if (_controlo.BackColor != Color.Red)
                                    _controlo.BackColor = Color.Red;
                                break;
                            }
                        default:
                            {
                                if (_controlo.BackColor != Color.WhiteSmoke)
                                    _controlo.BackColor = Color.WhiteSmoke;
                                break;
                            }

                    }
                    valorRetorno = true;
                }
                return valorRetorno;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        ///  Função que atualiza a backColor do control conforme o booleano que for passado
        /// </summary>
        /// <param name="_controlo">Objecto a alterar</param>
        /// <param name="_estado">Bool de controlo</param>
        /// <param name="_corOn">Cor para quando variável de estado estiver TRUE</param>
        /// <param name="_corOff">Cor para quando variável de estado estiver FALSE</param>
        /// <returns></returns>
        public static bool AtualizaBackColor(Control _controlo, bool _estado, Color _corOn, Color _corOff)
        {
            bool valorRetorno = false;
            try
            {
                if (_controlo.InvokeRequired)
                    _controlo.Invoke(new MethodInvoker(() =>
                    {
                        if (_controlo != null)
                        {
                            if (_estado)
                            {
                                if (_controlo.BackColor != _corOn)
                                    _controlo.BackColor = _corOn;
                            }
                            else
                                if (_controlo.BackColor != _corOff)
                                _controlo.BackColor = _corOff;
                            valorRetorno = true;
                        }
                    }));
                else
                {
                    if (_controlo != null)
                    {
                        if (_estado)
                        {
                            if (_controlo.BackColor != _corOn)
                                _controlo.BackColor = _corOn;
                        }
                        else
                            if (_controlo.BackColor != _corOff)
                            _controlo.BackColor = _corOff;
                        valorRetorno = true;
                    }
                }
                return valorRetorno;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        ///  Função que atualiza a backColor do control conforme o booleano que for passado
        /// </summary>
        /// <param name="_controlo">Objecto a alterar</param>
        /// <param name="_estado">Bool de controlo</param>
        /// <param name="_corOn">Cor para quando variável de estado estiver TRUE</param>
        /// <param name="_corOff">Cor para quando variável de estado estiver FALSE</param>
        /// <returns></returns>
        public static bool AtualizaBackColor(ToolStripStatusLabel _controlo, bool _estado, Color _corOn, Color _corOff)
        {
            bool valorRetorno = false;
            try
            {
                if (_controlo != null)
                {
                    if (_estado)
                    {
                        if (_controlo.BackColor != _corOn)
                            _controlo.BackColor = _corOn;
                    }
                    else
                        if (_controlo.BackColor != _corOff)
                        _controlo.BackColor = _corOff;
                    valorRetorno = true;

                }
                return valorRetorno;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Atualiza o CheckState de um controlo especifico
        /// </summary>
        /// <param name="_controlo">Controlo a tratar</param>
        /// <param name="_estado">Estado</param>
        /// <returns></returns>
        public static bool AtualizaCheckedState(CheckBox _controlo, bool _estado)
        {
            try
            {
                if (_controlo.InvokeRequired)
                    _controlo.Invoke(new MethodInvoker(() =>
                    {
                        _controlo.Checked = _estado;
                    }));
                else
                    _controlo.Checked = _estado;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Atualiza o CheckState de um controlo especifico
        /// </summary>
        /// <param name="_controlo">Controlo a tratar</param>
        /// <param name="_estado">Estado</param>
        /// <returns></returns>
        public static bool AtualizaCheckedState(RadioButton _controlo, bool _estado)
        {
            try
            {
                if (_controlo.InvokeRequired)
                    _controlo.Invoke(new MethodInvoker(() =>
                    {
                        _controlo.Checked = _estado;
                    }));
                else
                    _controlo.Checked = _estado;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public static bool RetribuiAvaliacao(int _avaliacao)
        {
            return (_avaliacao == 1) ? true : false;
        }

        /// <summary>
        /// Função que atualiza a backColor do control conforme o booleano que for passado
        /// </summary>
        /// <param name="_controlo">Objecto a alterar</param>
        /// <param name="_estado">Bool de controlo</param>
        /// <returns></returns>
        public static bool AtualizaBackColor(Control _controlo, int _estado, int _estadoOn, Color _corOn, Color _corOff)
        {
            bool valorRetorno = false;
            try
            {
                if (_controlo.InvokeRequired)
                    _controlo.Invoke(new MethodInvoker(() =>
                    {
                        if (_controlo != null)
                        {
                            if (_estado == _estadoOn)
                            {
                                if (_controlo.BackColor != _corOn)
                                    _controlo.BackColor = _corOn;
                            }
                            else
                                if (_controlo.BackColor != _corOff)
                                _controlo.BackColor = _corOff;
                            valorRetorno = true;
                        }
                    }));
                else
                {
                    if (_controlo != null)
                    {
                        if (_estado == _estadoOn)
                        {
                            if (_controlo.BackColor != _corOn)
                                _controlo.BackColor = _corOn;
                        }
                        else
                            if (_controlo.BackColor != _corOff)
                            _controlo.BackColor = _corOff;
                        valorRetorno = true;
                    }
                }
                return valorRetorno;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Copia DGV datasource to datatable
        /// </summary>
        /// <param name="dgvIn"></param>
        /// <param name="MostraMensagens"></param>
        /// <returns></returns>
        public static DataTable CopiaDGVToDT(DataGridView dgvIn, bool MostraMensagens = false)
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (DataGridViewColumn col in dgvIn.Columns)
                    dt.Columns.Add(col.HeaderText);


                foreach (DataGridViewRow row in dgvIn.Rows)
                {
                    DataRow dRow = dt.NewRow();
                    foreach (DataGridViewCell cell in row.Cells)
                        dRow[cell.ColumnIndex] = cell.Value;

                    dt.Rows.Add(dRow);
                }
                return dt;
            }
            catch (Exception ep)
            {
                if (MostraMensagens)
                    System.Windows.Forms.MessageBox.Show(Convert.ToString(ep), "Erro função CopiaDGVToDT", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return null;
            }
        }

        /// <summary>
        /// Comparar 2 datatables 
        /// </summary>
        /// <param name="DT1">DataTable 1</param>
        /// <param name="DT2">DataTable 2</param>
        /// <returns></returns>
        public static bool CompareDataTables(DataTable DT1, DataTable DT2)
        {
            try
            {
                if ((DT1 == null) && (DT2 == null))
                    return true;
                else if ((DT1 != null) && (DT2 != null))
                {
                    if (DT1.Rows.Count == DT2.Rows.Count)
                    {
                        if (DT1.Columns.Count == DT2.Columns.Count)
                        {
                            for (int i = 0; i < DT1.Rows.Count; i++)
                            {
                                for (int j = 0; j < DT1.Columns.Count; j++)
                                {
                                    if (DT1.Rows[i][j].ToString() != DT2.Rows[i][j].ToString())
                                        return false;
                                }
                            }
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }

        }

        public static int ExportarParaXLS(DataGridView dGV, bool MostrarMensagens = false, string diretorio = "")
        {
            try
            {
                int rowCount = 0;
                int columnCount = 0;
                if (dGV.InvokeRequired)
                    dGV.Invoke(new MethodInvoker(() =>
                    {
                        rowCount = dGV.RowCount;
                        columnCount = dGV.Columns.Count;
                    }));
                else
                {
                    rowCount = dGV.RowCount;
                    columnCount = dGV.Columns.Count;
                }

                if (diretorio == "")
                {
                    using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
                    {
                        saveFileDialog1.Reset();
                        saveFileDialog1.InitialDirectory = (Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                        saveFileDialog1.Filter = "Excel File (*.xls)|*.xls";
                        saveFileDialog1.Title = "Export to Excel";
                        saveFileDialog1.FilterIndex = 0;
                        saveFileDialog1.RestoreDirectory = true;
                        saveFileDialog1.CreatePrompt = true;

                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            if (saveFileDialog1.FileName != "")
                            {
                                if (rowCount == 0)
                                {
                                    if (MostrarMensagens)
                                        System.Windows.Forms.MessageBox.Show("Tabela de exportação sem dados", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return 2;
                                }

                                StringBuilder stOutput = new StringBuilder();
                                StringBuilder sHeaders = new StringBuilder();
                                // Export titles:

                                for (int j = 0; j < columnCount; j++)
                                    if (dGV.InvokeRequired)
                                        dGV.Invoke(new MethodInvoker(() =>
                                        {
                                            sHeaders.Append(Convert.ToString(dGV.Columns[j].HeaderText) + "\t");
                                        }));
                                    else
                                        sHeaders.Append(Convert.ToString(dGV.Columns[j].HeaderText) + "\t");
                                stOutput.Append(sHeaders + "\r\n");

                                // Export data.
                                for (int i = 0; i < rowCount; i++)
                                {
                                    if (dGV.InvokeRequired)
                                        dGV.Invoke(new MethodInvoker(() =>
                                        {
                                            StringBuilder stLine = new StringBuilder();
                                            for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                                                stLine.Append(Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t");
                                            stOutput.Append(stLine + "\r\n");
                                        }));
                                    else
                                    {
                                        StringBuilder stLine = new StringBuilder();
                                        for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                                            stLine.Append(Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t");
                                        stOutput.Append(stLine + "\r\n");
                                    }

                                }
                                Encoding utf16 = Encoding.GetEncoding(1254);
                                byte[] output = utf16.GetBytes(stOutput.ToString());
                                using (FileStream fs = new FileStream(saveFileDialog1.FileName, FileMode.Create))
                                {
                                    using (BinaryWriter bw = new BinaryWriter(fs))
                                    {
                                        bw.Write(output, 0, output.Length); //write the encoded file
                                        bw.Flush();
                                        bw.Close();
                                        fs.Close();
                                    }
                                }
                                if (MostrarMensagens)
                                    System.Windows.Forms.MessageBox.Show("Dados exportados com sucesso", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return 3;
                            }
                            else
                            {
                                if (MostrarMensagens)
                                    System.Windows.Forms.MessageBox.Show("Exportação cancelada pelo utilizador", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return 1;
                            }
                        }
                        else
                        {
                            if (MostrarMensagens)
                                System.Windows.Forms.MessageBox.Show("Exportação cancelada pelo utilizador", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return 1;
                        }
                    }
                }
                else
                {
                    if (rowCount == 0)
                    {
                        if (MostrarMensagens)
                            System.Windows.Forms.MessageBox.Show("Tabela de exportação sem dados", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return 2;
                    }

                    StringBuilder strOutput = new StringBuilder();
                    // Export titles:
                    StringBuilder sHeaders = new StringBuilder();

                    for (int j = 0; j < columnCount; j++)
                        if (dGV.InvokeRequired)
                            dGV.Invoke(new MethodInvoker(() =>
                            {
                                sHeaders.Append(Convert.ToString(dGV.Columns[j].HeaderText) + "\t");
                            }));
                        else
                            sHeaders.Append(Convert.ToString(dGV.Columns[j].HeaderText) + "\t");

                    strOutput.Append(sHeaders + "\r\n");

                    // Export data.
                    for (int i = 0; i < rowCount; i++)
                    {
                        StringBuilder stLine = new StringBuilder();
                        if (dGV.InvokeRequired)
                            dGV.Invoke(new MethodInvoker(() =>
                            {
                                for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                                    stLine.Append(Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t");
                            }));
                        else
                            for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                                stLine.Append(Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t");

                        strOutput.Append(stLine + "\r\n");
                    }
                    Encoding utf16 = Encoding.GetEncoding(1254);
                    byte[] output = utf16.GetBytes(strOutput.ToString());
                    using (FileStream fs = new FileStream(diretorio, FileMode.Create))
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(output, 0, output.Length); //write the encoded file
                        bw.Flush();
                        bw.Close();
                        fs.Close();
                    }

                    if (MostrarMensagens)
                        System.Windows.Forms.MessageBox.Show("Dados exportados com sucesso", "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return 3;
                }
            }

            catch (Exception ex)
            {
                if (MostrarMensagens)
                    System.Windows.Forms.MessageBox.Show("Erro ao tentar exportar: " + Convert.ToString(ex), "Exportação", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
            finally
            {
                GC.Collect();
            }

        }

        public static void InverteCores(Control ctrl, bool Enabled, Color corOn, Color corOff)
        {
            if (Enabled)
                if (ctrl.BackColor == corOn)
                    ctrl.BackColor = corOff;
                else
                    ctrl.BackColor = corOn;
        }

        public static void InverteCores(Control ctrl)
        {
            Color corFore = ctrl.ForeColor;
            ctrl.ForeColor = ctrl.BackColor;
            ctrl.BackColor = corFore;
        }


        public static void openOnScreenKeyboard()
        {
            try
            {
                Process.Start("osk.exe");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error opening OSKeyboard: " + ex.Message);
            }

        }

        public static void killOnScreenKeyboard()
        {
            if (Process.GetProcessesByName("osk").Count() > 0)
                Process.GetProcessesByName("osk").First().Kill();

        }

        /// <summary>
        /// Generates a percentage, formatted with "places" decimal places.
        /// </summary>
        /// <param name="value">Value for which a percentage is needed</param>
        /// <param name="total">Total from which to generate a percentage</param>
        /// <param name="places">Decimal places to return in the percentage string</param>
        /// <returns>string with the percentage value</returns>
        public static string GetPercentage(int value, int total, int places)
        {
            decimal percent = 0;
            string retval = string.Empty;
            string strplaces = new string('0', places);

            if (value == 0 || total == 0)
                percent = 0;
            else
            {
                percent = decimal.Divide(value, total) * 100;

                if (places > 0)
                    strplaces = "." + strplaces;
            }

            retval = percent.ToString("#" + strplaces);

            return retval;
        }

        public static bool SqlDataReaderToCsv(SqlDataReader dataReader, string fileName, bool includeHeaderAsFirstRow, bool sepFirst)
        {
            try
            {
                const string Separator = ",";
                StringBuilder sb = null;

                using (StreamWriter streamWriter = new StreamWriter(fileName, true, Encoding.Default))
                {
                    if (sepFirst)
                        streamWriter.WriteLine("sep=" + Separator);

                    if (includeHeaderAsFirstRow)
                    {
                        sb = new StringBuilder();
                        for (int index = 0; index < dataReader.FieldCount; index++)
                        {
                            if (dataReader.GetName(index) != null)
                                sb.Append(dataReader.GetName(index));

                            if (index < dataReader.FieldCount - 1)
                                sb.Append(Separator);
                        }
                        streamWriter.WriteLine(sb.ToString());
                    }

                    while (dataReader.Read())
                    {
                        sb = new StringBuilder();
                        for (int index = 0; index < dataReader.FieldCount - 1; index++)
                        {
                            if (!dataReader.IsDBNull(index))
                            {
                                string value = dataReader.GetValue(index).ToString();
                                if (dataReader.GetFieldType(index) == typeof(String))
                                {
                                    if (value.IndexOf("\"") >= 0)
                                        value = value.Replace("\"", "\"\"");

                                    if (value.IndexOf(Separator) >= 0)
                                        value = "\"" + value + "\"";
                                }
                                sb.Append(value);
                            }

                            if (index < dataReader.FieldCount - 1)
                                sb.Append(Separator);
                        }

                        if (!dataReader.IsDBNull(dataReader.FieldCount - 1))
                            sb.Append(dataReader.GetValue(dataReader.FieldCount - 1).ToString().Replace(Separator, " "));

                        streamWriter.WriteLine(sb.ToString());
                    }
                    dataReader.Close();
                    streamWriter.Close();
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ToCsv(): " + ex.Message);
                return false;
            }
        }

        public static bool SaveDataGridViewToCSV(DataGridView dgv, string filename, bool sepFirst)
        {
            if (dgv != null)
                if (dgv.DataSource != null)
                    if (dgv.RowCount > 0)
                        try
                        { // Save the current state of the clipboard so we can restore it after we are done
                            IDataObject objectSave = Clipboard.GetDataObject();

                            // Choose whether to write header. Use EnableWithoutHeaderText instead to omit header.
                            dgv.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                            // Select all the cells
                            dgv.SelectAll();
                            // Copy (set clipboard)
                            Clipboard.SetDataObject(dgv.GetClipboardContent());

                            // Paste (get the clipboard and serialize it to a file)
                            File.WriteAllText(filename, (sepFirst ? "sep=," + Environment.NewLine : string.Empty) + Clipboard.GetText(TextDataFormat.CommaSeparatedValue), Encoding.Unicode);
                            // Clear Cells selection
                            dgv.ClearSelection();

                            // Restore the current state of the clipboard so the effect is seamless
                            if (objectSave != null) // If we try to set the Clipboard to an object that is null, it will throw...
                                Clipboard.SetDataObject(objectSave);

                            return true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "SaveDataGridViewToCSV()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    else
                        return true;

            return false;
        }
    }


    /// <summary>
    /// Objecto para gerar uma cor aleatória
    /// </summary>
    public class RandomColor
    {
        Type colorType = typeof(Color);
        PropertyInfo[] proInfos;


        public RandomColor()
        {
            proInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.Public);
        }

        /// <summary>
        /// Gera uma cor aleatória
        /// </summary>
        /// <returns></returns>
        public Color NextColor()
        {
            return Color.FromName(proInfos[new Random().Next(0, proInfos.Length)].Name);
        }
    }


    /// <summary>
    /// Class do Ficheiro INI
    /// </summary>
    public class FicheiroINI : IDisposable
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
        public FicheiroINI(string INIPath)
        {
            //Alteramos todas as \ que possam estar ao contrário
            INIPath.Replace(@"/", @"\");
            //Associação do caminho
            path = INIPath;
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
        public string LeFicheiroINI(string Secao, string Campo)
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
            {
                EscreveFicheiroINI(_seccao, _campo, _strAPrencherCasoNulo);
                return _strAPrencherCasoNulo;
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
                    {
                        GC.Collect();
                    }

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


    //Class só com funções de encriptação (MD5)
    public class Licenciamento
    {
        private const string Password = "STREAK123"; //Salt usado na encriptação - Não mexer

        private string cpuID = string.Empty;
        private bool softwareLicenciado = false;

        private string username = string.Empty;
        private string organization = string.Empty;
        private string registeredkey = string.Empty;
        private int datetime = 0;

        public DateTime DataHoraAtivacao
        {
            get
            {
                return Diversos.ConvertUnixParaDatetime(datetime);
            }
        }

        public bool SoftwareLicenciado
        {
            get { return softwareLicenciado; }
        }
        public string CpuID
        {
            get { return cpuID; }
        }

        public Licenciamento()
        {
            cpuID = GetCPUID();

            //Verificar se o software está licenciado
            using (FicheiroINI ini = new FicheiroINI(Application.StartupPath + @"\lic.license"))
            {
                username = ini.RetornaINI("License", "username", username);
                organization = ini.RetornaINI("License", "organization", organization);
                registeredkey = ini.RetornaINI("License", "registeredkey", registeredkey);
                datetime = Convert.ToInt32(ini.RetornaINI("License", "datetime", Convert.ToString(Diversos.ObterTempoUnixAtual())));
            }
            softwareLicenciado = (Desencripta(registeredkey) == cpuID);
        }

        private byte[] Encrypt(byte[] clearText, byte[] Key, byte[] IV)
        {
            try
            {
                byte[] encryptedData;
                using (MemoryStream ms = new MemoryStream())
                using (Rijndael alg = Rijndael.Create())
                {
                    alg.Key = Key;
                    alg.IV = IV;
                    using (CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearText, 0, clearText.Length);
                        cs.Close();
                    }
                    encryptedData = ms.ToArray();
                }
                return encryptedData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao encriptar: " + ex.Message);
                return null;
            }
        }

        public string Encripta(string stringParaEncriptar)
        {
            try
            {
                byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(stringParaEncriptar);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                return Convert.ToBase64String(encryptedData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao encriptar string: " + ex.Message);
                return null;
            }
        }

        private byte[] Decrypt(byte[] cipherData, byte[] Key, byte[] IV)
        {
            try
            {
                byte[] decryptedData;
                using (MemoryStream ms = new MemoryStream())
                using (Rijndael alg = Rijndael.Create())
                {
                    alg.Key = Key;
                    alg.IV = IV;
                    using (CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherData, 0, cipherData.Length);
                        cs.Close();
                    }
                    decryptedData = ms.ToArray();
                }
                return decryptedData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao recuperar a string erncriptada: " + ex.Message);
                return null;
            }
        }

        public string Desencripta(string stringParaDesencriptar)
        {
            try
            {
                byte[] cipherBytes = Convert.FromBase64String(stringParaDesencriptar);
                PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                byte[] decryptedData = Decrypt(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro ao desencriptar string: " + ex.Message);
                return null;
            }
        }

        private string GetCPUID()
        {
            string returnStr = string.Empty;
            //Checks if motherboard serial number is equal to saved in ini file
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
            foreach (ManagementObject item in searcher.Get())
                returnStr = Convert.ToString(item["SerialNumber"]).Trim();

            return returnStr;
        }
        public void ClearActivation()
        {
            softwareLicenciado = false;
            username = string.Empty;
            organization = string.Empty;
            registeredkey = string.Empty;
            datetime = 0;
            using (FicheiroINI ini = new FicheiroINI(Application.StartupPath + @"\lic.license"))
            {
                ini.EscreveFicheiroINI("License", "username", username);
                ini.EscreveFicheiroINI("License", "organization", organization);
                ini.EscreveFicheiroINI("License", "registeredkey", registeredkey);
                ini.EscreveFicheiroINI("License", "datetime", Convert.ToString(datetime));
            }
        }

        private string GetProductKey()
        {
            return Encripta(cpuID);
        }

        public bool CheckActivation(string _registeredkey, string _username, string _organization)
        {
            if (_registeredkey == GetProductKey())
            {
                softwareLicenciado = true;

                username = _username;
                organization = _organization;
                registeredkey = _registeredkey;
                datetime = Diversos.ObterTempoUnixAtual();
                using (FicheiroINI ini = new FicheiroINI(Application.StartupPath + @"\lic.license"))
                {
                    ini.EscreveFicheiroINI("License", "username", username);
                    ini.EscreveFicheiroINI("License", "organization", organization);
                    ini.EscreveFicheiroINI("License", "registeredkey", registeredkey);
                    ini.EscreveFicheiroINI("License", "datetime", Convert.ToString(datetime));
                }

                return true;
            }
            else
                return false;
        }

    }

}