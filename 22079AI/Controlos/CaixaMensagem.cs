using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _22079AI
{
    public partial class CaixaMensagem : Form
    {

        private bool _dragging = false;
        private Point _start_point = new Point(0, 0);

        public CaixaMensagem(string _message, string _header = "", TipoMsgBox _tipoMsgBox = TipoMsgBox.Normal)
        {
            InitializeComponent();

            lblMessage.Text = _message;

            if (_header != "")
                lblHeader.Text = _header;
            else
                lblHeader.Text = "Windows";

            this.Text = lblHeader.Text;

            switch (_tipoMsgBox)
            {
                case TipoMsgBox.Normal:
                    {
                        lblHeader.BackColor = Color.MidnightBlue;
                        btnOK.Visible = true;
                        break;
                    }
                case TipoMsgBox.Warning:
                    {
                        lblHeader.BackColor = Color.OrangeRed;
                        btnOK.Visible = true;
                        break;
                    }
                case TipoMsgBox.Error:
                    {
                        lblHeader.BackColor = Color.Red;
                        btnOK.Visible = true;
                        break;
                    }
                case TipoMsgBox.Question:
                    {
                        lblHeader.BackColor = Color.MediumBlue;
                        btnYes.Visible = true;
                        btnNo.Visible = true;
                        break;
                    }
            }

            this.TopMost = true;
            this.BringToFront();
        }

        public CaixaMensagem(string _message, string _header = "", TipoMsgBox _tipoMsgBox = TipoMsgBox.Normal, FormStartPosition _startPos = FormStartPosition.CenterScreen)
        {
            InitializeComponent();

            this.StartPosition = _startPos;

            lblMessage.Text = _message;

            if (_header != "")
                lblHeader.Text = _header;
            else
                lblHeader.Text = "Windows";

            this.Text = lblHeader.Text;

            switch (_tipoMsgBox)
            {
                case TipoMsgBox.Normal:
                    {
                        lblHeader.BackColor = Color.MidnightBlue;
                        btnOK.Visible = true;
                        break;
                    }
                case TipoMsgBox.Warning:
                    {
                        lblHeader.BackColor = Color.OrangeRed;
                        btnOK.Visible = true;
                        break;
                    }
                case TipoMsgBox.Error:
                    {
                        lblHeader.BackColor = Color.Red;
                        btnOK.Visible = true;
                        break;
                    }
                case TipoMsgBox.Question:
                    {
                        lblHeader.BackColor = Color.MediumBlue;
                        btnYes.Visible = true;
                        btnNo.Visible = true;
                        break;
                    }
            }

            this.TopMost = true;
            this.BringToFront();
        }

        public enum TipoMsgBox
        {
            Normal,
            Error,
            Warning,
            Question
        }

        private void GenericFunction(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblCabecalho_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - _start_point.X, p.Y - _start_point.Y);
            }
        }

        private void lblCabecalho_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void lblCabecalho_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;  // _dragging is your variable flag
            _start_point = new Point(e.X, e.Y);
        }

        private void CaixaMensagem_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
        }

        private void CaixaMensagem_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

    }
}
