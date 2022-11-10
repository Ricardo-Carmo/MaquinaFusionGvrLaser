using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class ImagemFullScreen : Form
    {
        public ImagemFullScreen()
        {
            InitializeComponent();
            pictureBox1.Controls.Add(label1);
            timer1.Enabled = true;

        }

        public ImagemFullScreen(Image img)
        {
            InitializeComponent();
            pictureBox1.Controls.Add(label1);
            label1.Text = Convert.ToString(DateTime.Now);
            timer1.Enabled = false;
            this.pictureBox1.Image = img;
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                label1.Text = Convert.ToString(DateTime.Now);

                //lock (MainForm.myLock)
                //    this.pictureBox1.Image = Forms.MainForm.IMAGEM.Image.Clone() as Bitmap;
            }
            catch (Exception)
            {
                this.pictureBox1.Image = null;
            }
        }
    }
}
