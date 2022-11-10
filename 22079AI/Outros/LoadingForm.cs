using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();

            var version = Assembly.GetEntryAssembly().GetName().Version;
            var buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(TimeSpan.TicksPerDay * version.Build + TimeSpan.TicksPerSecond * 2 * version.Revision));
            label2.Text = buildDateTime.ToShortDateString();
            //label1.Text += " - " + Convert.ToString(version).Replace(",", ".");

            new Thread(FazAnimacao).Start();
        }

        private void FazAnimacao()
        {
            string[] stringsAnimacao = new string[9];
            stringsAnimacao[0] = "Loading System.Configuration.dll ";
            stringsAnimacao[1] = "Loading System.Transactions.dll";
            stringsAnimacao[2] = "Loading System.EnterpriseServices.dll";
            stringsAnimacao[3] = "Loading Microsoft.VisualStudio.OLE.Interop.dll";
            stringsAnimacao[4] = "Loading System.Drawing.dll";
            stringsAnimacao[5] = "Loading System.VisualBasic.dll";
            stringsAnimacao[6] = "Loading System.Core.dll";
            stringsAnimacao[7] = "Loading PLC Communication";
            stringsAnimacao[8] = "Loading Vision Scripts";

            int i = 0;
            int k = -1;
            int j = 0;
            bool sum = true;

            Random rnd = new Random();

            while (!VARIAVEIS.FORM_LOADED && VARIAVEIS.FLAG_WHILE_CYCLE)
            {
                k++;
                if (metroProgressBar1.InvokeRequired)
                    metroProgressBar1.Invoke(new MethodInvoker(() =>
                    {
                        metroProgressBar1.Value = i;
                    }));
                else
                    metroProgressBar1.Value = i;

                if (sum)
                    i++;
                else
                    i--;

                if (i > 100 && sum)
                    sum = false;
                else
                    if (i < 1 && !sum)
                    sum = true;

                Thread.Sleep(20);

                if ((k % rnd.Next(25, 35)) == 0)
                    if (j < (stringsAnimacao.Length - 3))
                        j++;

                if (metroLabel5.InvokeRequired)
                    metroLabel5.Invoke(new MethodInvoker(() =>
                    {
                        metroLabel5.Text = stringsAnimacao[j];
                    }));
                else
                    metroLabel5.Text = stringsAnimacao[j];
            }

            if (this.InvokeRequired)
                this.Invoke(new MethodInvoker(() =>
                {
                    this.Close();
                }));
            else
                this.Close();
        }

        private void LoadingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            VARIAVEIS.FORM_LOADED = true;
        }

    }
}
