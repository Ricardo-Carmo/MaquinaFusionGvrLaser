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
    public partial class MostraImagem : Form
    {
        public MostraImagem(Image _imagem)
        {
            InitializeComponent();

            pictureBox1.Image = _imagem;
        }

        private void MostraImagem_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
