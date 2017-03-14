namespace WakeMapper
{
    partial class WakeMapper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WakeMapper));
            this.startButton = new System.Windows.Forms.Button();
            this.displayOutput = new System.Windows.Forms.Label();
            this.onlineCheckbox = new System.Windows.Forms.CheckBox();
            this.onlineDisplay = new System.Windows.Forms.LinkLabel();
            this.map = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.map)).BeginInit();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(437, 518);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 0;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // displayOutput
            // 
            this.displayOutput.AutoSize = true;
            this.displayOutput.Location = new System.Drawing.Point(12, 518);
            this.displayOutput.Name = "displayOutput";
            this.displayOutput.Size = new System.Drawing.Size(44, 13);
            this.displayOutput.TabIndex = 1;
            this.displayOutput.Text = "Ready?";
            this.displayOutput.Click += new System.EventHandler(this.label1_Click);
            // 
            // onlineCheckbox
            // 
            this.onlineCheckbox.AutoSize = true;
            this.onlineCheckbox.Location = new System.Drawing.Point(377, 547);
            this.onlineCheckbox.Name = "onlineCheckbox";
            this.onlineCheckbox.Size = new System.Drawing.Size(135, 17);
            this.onlineCheckbox.TabIndex = 3;
            this.onlineCheckbox.Text = "Enable Online Updates";
            this.onlineCheckbox.UseVisualStyleBackColor = true;
            this.onlineCheckbox.Visible = false;
            this.onlineCheckbox.CheckedChanged += new System.EventHandler(this.onlineCheckbox_CheckedChanged);
            // 
            // onlineDisplay
            // 
            this.onlineDisplay.AutoSize = true;
            this.onlineDisplay.Location = new System.Drawing.Point(12, 547);
            this.onlineDisplay.Name = "onlineDisplay";
            this.onlineDisplay.Size = new System.Drawing.Size(0, 13);
            this.onlineDisplay.TabIndex = 5;
            this.onlineDisplay.Visible = false;
            this.onlineDisplay.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.onlineDisplay_LinkClicked);
            // 
            // map
            // 
            this.map.Image = global::WakeMapper.Properties.Resources.WakeMap;
            this.map.ImageLocation = "";
            this.map.InitialImage = ((System.Drawing.Image)(resources.GetObject("map.InitialImage")));
            this.map.Location = new System.Drawing.Point(12, 12);
            this.map.Name = "map";
            this.map.Size = new System.Drawing.Size(500, 500);
            this.map.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.map.TabIndex = 2;
            this.map.TabStop = false;
            this.map.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // WakeMapper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 570);
            this.Controls.Add(this.onlineDisplay);
            this.Controls.Add(this.onlineCheckbox);
            this.Controls.Add(this.map);
            this.Controls.Add(this.displayOutput);
            this.Controls.Add(this.startButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WakeMapper";
            this.Text = "Wake Mapper";
            this.Load += new System.EventHandler(this.WakeMapper_Load);
            ((System.ComponentModel.ISupportInitialize)(this.map)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Label displayOutput;
        private System.Windows.Forms.PictureBox map;
        private System.Windows.Forms.CheckBox onlineCheckbox;
        private System.Windows.Forms.LinkLabel onlineDisplay;
    }
}

