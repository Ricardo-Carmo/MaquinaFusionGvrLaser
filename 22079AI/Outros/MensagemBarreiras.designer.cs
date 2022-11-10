namespace _22079AI
{
    partial class MensagemBarreiras
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MensagemBarreiras));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lblCicloAutomatico = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.lblCicloAutomatico);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(458, 201);
            this.panel1.TabIndex = 0;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(240, 99);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(195, 70);
            this.button2.TabIndex = 87;
            this.button2.Text = "Reset ao Contador da Caixa";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(23, 99);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(195, 70);
            this.button1.TabIndex = 86;
            this.button1.Text = "Continuar o Movimento";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblCicloAutomatico
            // 
            this.lblCicloAutomatico.BackColor = System.Drawing.Color.OrangeRed;
            this.lblCicloAutomatico.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCicloAutomatico.Font = new System.Drawing.Font("Cambria", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCicloAutomatico.ForeColor = System.Drawing.Color.White;
            this.lblCicloAutomatico.Location = new System.Drawing.Point(0, 0);
            this.lblCicloAutomatico.Name = "lblCicloAutomatico";
            this.lblCicloAutomatico.Size = new System.Drawing.Size(456, 68);
            this.lblCicloAutomatico.TabIndex = 85;
            this.lblCicloAutomatico.Text = "Barreira Aprovados Interrompida";
            this.lblCicloAutomatico.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblCicloAutomatico.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lblCicloAutomatico_MouseDown);
            this.lblCicloAutomatico.MouseMove += new System.Windows.Forms.MouseEventHandler(this.lblCicloAutomatico_MouseMove);
            this.lblCicloAutomatico.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lblCicloAutomatico_MouseUp);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 750;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MensagemBarreiras
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(458, 201);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MensagemBarreiras";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblCicloAutomatico;
        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.Button button1;
        public System.Windows.Forms.Button button2;
    }
}