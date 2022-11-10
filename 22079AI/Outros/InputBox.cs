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
    public partial class InputBox : Form
    {
        private Point _start_point = new Point(0, 0);
        private bool _dragging = false;

        public bool ValorSubmetido
        {
            get;
            private set;
        }

        public object Valor
        {
            get;
            private set;
        }

        private bool hasLimits = false;
        private object minValue;
        private object maxValue;

        private TipoValor tipo = TipoValor.String;

        public enum TipoValor
        {
            String = 0, Int = 1, Double = 2, Long = 3
        }

        public InputBox(TipoValor _tipo, string _label)
        {
            InitializeComponent();

            label41.Text = _label;

            this.tipo = _tipo;
        }

        public InputBox(TipoValor _tipo, string _label, int _minValue, int _maxValue)
        {
            InitializeComponent();

            label41.Text = _label;

            hasLimits = true;
            minValue = _minValue;
            maxValue = _maxValue;

            this.tipo = _tipo;
        }

        public InputBox(TipoValor _tipo, string _label, double _minValue, double _maxValue)
        {
            InitializeComponent();

            label41.Text = _label;

            hasLimits = true;
            minValue = _minValue;
            maxValue = _maxValue;

            this.tipo = _tipo;
        }

        public InputBox(TipoValor _tipo, string _label, uint _minValue, uint _maxValue)
        {
            InitializeComponent();

            label41.Text = _label;

            hasLimits = true;
            minValue = _minValue;
            maxValue = _maxValue;

            this.tipo = _tipo;
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SubmeteValores()
        {
            switch (tipo)
            {
                case TipoValor.String:
                    {
                        this.Valor = textBox1.Text;
                        this.ValorSubmetido = true;
                        this.Close();
                        break;
                    }
                case TipoValor.Int:
                    {
                        int aux = 0;

                        if (int.TryParse(textBox1.Text, out aux))
                            if (hasLimits)
                                if (Diversos.InRange(aux, (int)minValue, (int)maxValue))
                                {
                                    this.Valor = aux;
                                    this.ValorSubmetido = true;
                                    this.Close();
                                }
                                else
                                    this.CampoMalPreenchido();
                            else
                            {
                                this.Valor = aux;
                                this.ValorSubmetido = true;
                                this.Close();
                            }
                        else
                            this.CampoMalPreenchido();
                        break;
                    }
                case TipoValor.Long:
                    {
                        long aux = 0;

                        if (long.TryParse(textBox1.Text, out aux))
                            if (hasLimits)
                                if (Diversos.InRange(aux,  Convert.ToInt64(minValue),Convert.ToInt64(  maxValue)))
                                {
                                    this.Valor = aux;
                                    this.ValorSubmetido = true;
                                    this.Close();
                                }
                                else
                                    this.CampoMalPreenchido();
                            else
                            {
                                this.Valor = aux;
                                this.ValorSubmetido = true;
                                this.Close();
                            }
                        else
                            this.CampoMalPreenchido();
                        break;
                    }
                case TipoValor.Double:
                    {
                        double aux = 0;

                        if (double.TryParse(textBox1.Text, out aux))
                        {
                            if (hasLimits)
                                if (Diversos.InRange(aux, (double)minValue, (double)maxValue))
                                {
                                    this.Valor = aux;
                                    this.ValorSubmetido = true;
                                    this.Close();
                                }
                                else
                                    this.CampoMalPreenchido();
                            else
                            {
                                this.Valor = aux;
                                this.ValorSubmetido = true;
                                this.Close();
                            }
                        }
                        else
                            this.CampoMalPreenchido();
                        break;
                    }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SubmeteValores();
        }

        private void CampoMalPreenchido()
        {
            this.ValorSubmetido = false;
            textBox1.BackColor = Color.Red;
            textBox1.ForeColor = Color.White;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                SubmeteValores();
            else
            {
                textBox1.BackColor = Color.White;
                textBox1.ForeColor = Color.Black;
            }
        }

        private void label28_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void label28_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void label28_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

    }
}
