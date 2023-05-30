namespace demoTello
{
    partial class Form5
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form5));
            this.VECTOR = new System.Windows.Forms.ListBox();
            this.btnVettore = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.parti = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.btn_manualSet = new System.Windows.Forms.Button();
            this.pnl_manualSet = new System.Windows.Forms.Panel();
            this.btn_close = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_createIstruction = new System.Windows.Forms.Button();
            this.txt_insDistance = new System.Windows.Forms.TextBox();
            this.txt_insDegree = new System.Windows.Forms.TextBox();
            this.btn_deleteIstruction = new System.Windows.Forms.Button();
            this.btn_editIstruction = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnl_manualSet.SuspendLayout();
            this.SuspendLayout();
            // 
            // VECTOR
            // 
            this.VECTOR.FormattingEnabled = true;
            this.VECTOR.Location = new System.Drawing.Point(594, 37);
            this.VECTOR.Name = "VECTOR";
            this.VECTOR.Size = new System.Drawing.Size(330, 251);
            this.VECTOR.TabIndex = 28;
            this.VECTOR.SelectedIndexChanged += new System.EventHandler(this.VECTOR_SelectedIndexChanged);
            // 
            // btnVettore
            // 
            this.btnVettore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.btnVettore.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.btnVettore.Location = new System.Drawing.Point(487, 150);
            this.btnVettore.Name = "btnVettore";
            this.btnVettore.Size = new System.Drawing.Size(83, 58);
            this.btnVettore.TabIndex = 27;
            this.btnVettore.Text = "Array";
            this.btnVettore.UseVisualStyleBackColor = false;
            this.btnVettore.Click += new System.EventHandler(this.btnVettore_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(562, 132);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = ".";
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.Yellow;
            this.button3.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.button3.Location = new System.Drawing.Point(501, 91);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(56, 21);
            this.button3.TabIndex = 25;
            this.button3.Text = "Load";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Lime;
            this.button2.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.button2.Location = new System.Drawing.Point(501, 119);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(56, 20);
            this.button2.TabIndex = 24;
            this.button2.Text = "Save";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.button1.Location = new System.Drawing.Point(501, 67);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 20);
            this.button1.TabIndex = 22;
            this.button1.Text = "Clear";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(501, 152);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = ".";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(468, 37);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 20;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // parti
            // 
            this.parti.BackColor = System.Drawing.Color.Lime;
            this.parti.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.parti.Location = new System.Drawing.Point(487, 214);
            this.parti.Name = "parti";
            this.parti.Size = new System.Drawing.Size(83, 58);
            this.parti.TabIndex = 29;
            this.parti.Text = "SET OFF";
            this.parti.UseVisualStyleBackColor = false;
            this.parti.Click += new System.EventHandler(this.parti_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label3.Font = new System.Drawing.Font("Showcard Gothic", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(6, 16);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(517, 18);
            this.label3.TabIndex = 30;
            this.label3.Text = "Select the spots where you want the Tello drone to go";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::demoTello.Properties.Resources.Aula3;
            this.pictureBox1.Location = new System.Drawing.Point(9, 37);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(454, 320);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 23;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseClick);
            // 
            // btn_manualSet
            // 
            this.btn_manualSet.BackColor = System.Drawing.Color.Cyan;
            this.btn_manualSet.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.btn_manualSet.Location = new System.Drawing.Point(487, 279);
            this.btn_manualSet.Name = "btn_manualSet";
            this.btn_manualSet.Size = new System.Drawing.Size(83, 58);
            this.btn_manualSet.TabIndex = 31;
            this.btn_manualSet.Text = "Manual Commands";
            this.btn_manualSet.UseVisualStyleBackColor = false;
            this.btn_manualSet.Click += new System.EventHandler(this.btn_manualSet_Click);
            // 
            // pnl_manualSet
            // 
            this.pnl_manualSet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.pnl_manualSet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_manualSet.Controls.Add(this.btn_close);
            this.pnl_manualSet.Controls.Add(this.label5);
            this.pnl_manualSet.Controls.Add(this.label4);
            this.pnl_manualSet.Controls.Add(this.btn_createIstruction);
            this.pnl_manualSet.Controls.Add(this.txt_insDistance);
            this.pnl_manualSet.Controls.Add(this.txt_insDegree);
            this.pnl_manualSet.Location = new System.Drawing.Point(9, 37);
            this.pnl_manualSet.Margin = new System.Windows.Forms.Padding(2);
            this.pnl_manualSet.Name = "pnl_manualSet";
            this.pnl_manualSet.Size = new System.Drawing.Size(453, 320);
            this.pnl_manualSet.TabIndex = 32;
            this.pnl_manualSet.Visible = false;
            // 
            // btn_close
            // 
            this.btn_close.BackColor = System.Drawing.Color.White;
            this.btn_close.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this.btn_close.ForeColor = System.Drawing.Color.Red;
            this.btn_close.Location = new System.Drawing.Point(368, -2);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(83, 58);
            this.btn_close.TabIndex = 35;
            this.btn_close.Text = "X";
            this.btn_close.UseVisualStyleBackColor = false;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label5.Font = new System.Drawing.Font("Showcard Gothic", 10.8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(12, 167);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 18);
            this.label5.TabIndex = 34;
            this.label5.Text = "Insert Distance";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label4.Font = new System.Drawing.Font("Showcard Gothic", 10.8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(26, 88);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 18);
            this.label4.TabIndex = 33;
            this.label4.Text = "Insert Degree";
            // 
            // btn_createIstruction
            // 
            this.btn_createIstruction.BackColor = System.Drawing.Color.Cyan;
            this.btn_createIstruction.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.btn_createIstruction.Location = new System.Drawing.Point(171, 228);
            this.btn_createIstruction.Name = "btn_createIstruction";
            this.btn_createIstruction.Size = new System.Drawing.Size(83, 58);
            this.btn_createIstruction.TabIndex = 33;
            this.btn_createIstruction.Text = "Create Istruction";
            this.btn_createIstruction.UseVisualStyleBackColor = false;
            this.btn_createIstruction.Click += new System.EventHandler(this.btn_createIstruction_Click);
            // 
            // txt_insDistance
            // 
            this.txt_insDistance.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this.txt_insDistance.Location = new System.Drawing.Point(171, 154);
            this.txt_insDistance.Margin = new System.Windows.Forms.Padding(2);
            this.txt_insDistance.Name = "txt_insDistance";
            this.txt_insDistance.Size = new System.Drawing.Size(254, 38);
            this.txt_insDistance.TabIndex = 1;
            // 
            // txt_insDegree
            // 
            this.txt_insDegree.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F);
            this.txt_insDegree.Location = new System.Drawing.Point(171, 76);
            this.txt_insDegree.Margin = new System.Windows.Forms.Padding(2);
            this.txt_insDegree.Name = "txt_insDegree";
            this.txt_insDegree.Size = new System.Drawing.Size(254, 38);
            this.txt_insDegree.TabIndex = 0;
            // 
            // btn_deleteIstruction
            // 
            this.btn_deleteIstruction.BackColor = System.Drawing.Color.Cyan;
            this.btn_deleteIstruction.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_deleteIstruction.Location = new System.Drawing.Point(668, 299);
            this.btn_deleteIstruction.Name = "btn_deleteIstruction";
            this.btn_deleteIstruction.Size = new System.Drawing.Size(83, 58);
            this.btn_deleteIstruction.TabIndex = 33;
            this.btn_deleteIstruction.Text = "Delete Istruction";
            this.btn_deleteIstruction.UseVisualStyleBackColor = false;
            this.btn_deleteIstruction.Click += new System.EventHandler(this.btn_deleteIstruction_Click);
            // 
            // btn_editIstruction
            // 
            this.btn_editIstruction.BackColor = System.Drawing.Color.Cyan;
            this.btn_editIstruction.Font = new System.Drawing.Font("Stencil", 7.8F, System.Drawing.FontStyle.Bold);
            this.btn_editIstruction.Location = new System.Drawing.Point(757, 299);
            this.btn_editIstruction.Name = "btn_editIstruction";
            this.btn_editIstruction.Size = new System.Drawing.Size(83, 58);
            this.btn_editIstruction.TabIndex = 34;
            this.btn_editIstruction.Text = "Edit Istruction";
            this.btn_editIstruction.UseVisualStyleBackColor = false;
            // 
            // Form5
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(934, 384);
            this.Controls.Add(this.btn_editIstruction);
            this.Controls.Add(this.btn_deleteIstruction);
            this.Controls.Add(this.btn_manualSet);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.parti);
            this.Controls.Add(this.VECTOR);
            this.Controls.Add(this.btnVettore);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.pnl_manualSet);
            this.Controls.Add(this.pictureBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form5";
            this.Text = "MAP";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnl_manualSet.ResumeLayout(false);
            this.pnl_manualSet.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox VECTOR;
        private System.Windows.Forms.Button btnVettore;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button parti;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_manualSet;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_createIstruction;
        private System.Windows.Forms.TextBox txt_insDistance;
        private System.Windows.Forms.TextBox txt_insDegree;
        private System.Windows.Forms.Button btn_close;
        private System.Windows.Forms.Panel pnl_manualSet;
        private System.Windows.Forms.Button btn_deleteIstruction;
        private System.Windows.Forms.Button btn_editIstruction;
    }
}