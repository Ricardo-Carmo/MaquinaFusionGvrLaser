namespace _22079AI
{
    partial class ConfiguracoesAlarmes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfiguracoesAlarmes));
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtMaxAlarms = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.button96 = new System.Windows.Forms.Button();
            this.panel56 = new System.Windows.Forms.Panel();
            this.btnSair = new System.Windows.Forms.PictureBox();
            this.label28 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel56.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnSair)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.button96);
            this.panel1.Controls.Add(this.panel56);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(284, 287);
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtMaxAlarms);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(24, 154);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(236, 116);
            this.groupBox1.TabIndex = 97;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Número Máx. de Alarmes";
            // 
            // txtMaxAlarms
            // 
            this.txtMaxAlarms.BackColor = System.Drawing.Color.White;
            this.txtMaxAlarms.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMaxAlarms.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMaxAlarms.Location = new System.Drawing.Point(14, 27);
            this.txtMaxAlarms.MaxLength = 6;
            this.txtMaxAlarms.Multiline = true;
            this.txtMaxAlarms.Name = "txtMaxAlarms";
            this.txtMaxAlarms.Size = new System.Drawing.Size(204, 31);
            this.txtMaxAlarms.TabIndex = 96;
            this.txtMaxAlarms.Text = "1000";
            this.txtMaxAlarms.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtMaxAlarms.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMaxAlarms_KeyPress);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.Black;
            this.button2.Location = new System.Drawing.Point(14, 67);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(204, 33);
            this.button2.TabIndex = 95;
            this.button2.Text = "Alterar nº máx de alarmes";
            this.button2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.Black;
            this.button1.Location = new System.Drawing.Point(24, 106);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(236, 33);
            this.button1.TabIndex = 95;
            this.button1.Text = "Apagar todo o histórico";
            this.button1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button96
            // 
            this.button96.BackColor = System.Drawing.Color.White;
            this.button96.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button96.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button96.ForeColor = System.Drawing.Color.Black;
            this.button96.Location = new System.Drawing.Point(24, 48);
            this.button96.Name = "button96";
            this.button96.Size = new System.Drawing.Size(236, 33);
            this.button96.TabIndex = 96;
            this.button96.Text = "Exportar para Excel";
            this.button96.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button96.UseVisualStyleBackColor = false;
            this.button96.Click += new System.EventHandler(this.button96_Click);
            // 
            // panel56
            // 
            this.panel56.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(204)))), ((int)(((byte)(255)))));
            this.panel56.Controls.Add(this.btnSair);
            this.panel56.Controls.Add(this.label28);
            this.panel56.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel56.Location = new System.Drawing.Point(0, 0);
            this.panel56.Name = "panel56";
            this.panel56.Size = new System.Drawing.Size(282, 35);
            this.panel56.TabIndex = 78;
            // 
            // btnSair
            // 
            this.btnSair.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSair.Image = ((System.Drawing.Image)(resources.GetObject("btnSair.Image")));
            this.btnSair.Location = new System.Drawing.Point(247, 0);
            this.btnSair.Name = "btnSair";
            this.btnSair.Size = new System.Drawing.Size(35, 35);
            this.btnSair.TabIndex = 68;
            this.btnSair.TabStop = false;
            this.btnSair.Click += new System.EventHandler(this.btnSair_Click);
            // 
            // label28
            // 
            this.label28.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
            this.label28.Location = new System.Drawing.Point(7, 5);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(235, 23);
            this.label28.TabIndex = 0;
            this.label28.Text = "Histórico de Alarmes";
            this.label28.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label28_MouseDown);
            this.label28.MouseMove += new System.Windows.Forms.MouseEventHandler(this.label28_MouseMove);
            this.label28.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label28_MouseUp);
            // 
            // ConfiguracoesAlarmes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.ClientSize = new System.Drawing.Size(284, 287);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ConfiguracoesAlarmes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configurações de Alarmes";
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel56.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnSair)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel56;
        private System.Windows.Forms.PictureBox btnSair;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtMaxAlarms;
        public System.Windows.Forms.Button button2;
        public System.Windows.Forms.Button button1;
        public System.Windows.Forms.Button button96;
    }
}