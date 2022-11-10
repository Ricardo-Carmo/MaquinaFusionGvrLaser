namespace _22079AI
{
    partial class DetalheAlarme
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetalheAlarme));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel56 = new System.Windows.Forms.Panel();
            this.btnSair = new System.Windows.Forms.PictureBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblOPAOperador = new System.Windows.Forms.Label();
            this.lblAlarmeAtivo = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel56.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSair)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.panel56);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lblOPAOperador);
            this.panel1.Controls.Add(this.lblAlarmeAtivo);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(445, 305);
            this.panel1.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(77, 58);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(348, 81);
            this.label3.TabIndex = 93;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Tahoma", 12F);
            this.textBox1.Location = new System.Drawing.Point(106, 174);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(319, 116);
            this.textBox1.TabIndex = 94;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // panel56
            // 
            this.panel56.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
            this.panel56.Controls.Add(this.btnSair);
            this.panel56.Controls.Add(this.lblHeader);
            this.panel56.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel56.Location = new System.Drawing.Point(0, 0);
            this.panel56.Name = "panel56";
            this.panel56.Size = new System.Drawing.Size(443, 35);
            this.panel56.TabIndex = 80;
            // 
            // btnSair
            // 
            this.btnSair.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSair.Image = ((System.Drawing.Image)(resources.GetObject("btnSair.Image")));
            this.btnSair.Location = new System.Drawing.Point(408, 0);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(35, 35);
            this.btnSair.TabIndex = 68;
            this.btnSair.TabStop = false;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(443, 35);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "   ALM ";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseDown);
            this.lblHeader.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseMove);
            this.lblHeader.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblHeader_MouseUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(14, 174);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 19);
            this.label1.TabIndex = 93;
            this.label1.Text = "Resolução:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOPAOperador
            // 
            this.lblOPAOperador.AutoSize = true;
            this.lblOPAOperador.BackColor = System.Drawing.Color.Transparent;
            this.lblOPAOperador.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOPAOperador.ForeColor = System.Drawing.Color.White;
            this.lblOPAOperador.Location = new System.Drawing.Point(13, 58);
            this.lblOPAOperador.Name = "lblOPAOperador";
            this.lblOPAOperador.Size = new System.Drawing.Size(66, 19);
            this.lblOPAOperador.TabIndex = 93;
            this.lblOPAOperador.Text = "Alarme:";
            this.lblOPAOperador.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAlarmeAtivo
            // 
            this.lblAlarmeAtivo.BackColor = System.Drawing.Color.Red;
            this.lblAlarmeAtivo.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAlarmeAtivo.ForeColor = System.Drawing.Color.Black;
            this.lblAlarmeAtivo.Location = new System.Drawing.Point(18, 142);
            this.lblAlarmeAtivo.Name = "lblAlarmeAtivo";
            this.lblAlarmeAtivo.Size = new System.Drawing.Size(407, 22);
            this.lblAlarmeAtivo.TabIndex = 93;
            this.lblAlarmeAtivo.Text = "Alarme Ativo";
            this.lblAlarmeAtivo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblAlarmeAtivo.Visible = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // DetalheAlarme
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(445, 305);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DetalheAlarme";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.DetalheAlarme_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel56.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnSair)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel56;
        private System.Windows.Forms.PictureBox btnSair;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Label lblAlarmeAtivo;
        private System.Windows.Forms.Label lblOPAOperador;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
    }
}