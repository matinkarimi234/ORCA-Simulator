namespace Motor_Trajectory_Component
{
    partial class Motor_Trajectory_Component
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBox_Enable = new System.Windows.Forms.CheckBox();
            this.groupBox_Motor = new System.Windows.Forms.GroupBox();
            this.groupBox_Sine = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox_Enable_Sine = new System.Windows.Forms.CheckBox();
            this.numericUpDown_Sine_Frequency = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Sine_Amplitude = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Sine_Offset = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Sine_Phase_Deg = new System.Windows.Forms.NumericUpDown();
            this.groupBox_Cosine = new System.Windows.Forms.GroupBox();
            this.numericUpDown_Cosine_Phase_Deg = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Cosine_Offset = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Cosine_Amplitude = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Cosine_Frequency = new System.Windows.Forms.NumericUpDown();
            this.checkBox_Enable_Cosine = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox_Motor.SuspendLayout();
            this.groupBox_Sine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Frequency)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Amplitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Offset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Phase_Deg)).BeginInit();
            this.groupBox_Cosine.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Phase_Deg)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Offset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Amplitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Frequency)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBox_Enable
            // 
            this.checkBox_Enable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_Enable.AutoSize = true;
            this.checkBox_Enable.Checked = true;
            this.checkBox_Enable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Enable.Location = new System.Drawing.Point(3, 3);
            this.checkBox_Enable.Name = "checkBox_Enable";
            this.checkBox_Enable.Size = new System.Drawing.Size(99, 17);
            this.checkBox_Enable.TabIndex = 0;
            this.checkBox_Enable.Text = "Enable Motor A";
            this.checkBox_Enable.UseVisualStyleBackColor = true;
            this.checkBox_Enable.CheckedChanged += new System.EventHandler(this.checkBox_Enable_CheckedChanged);
            // 
            // groupBox_Motor
            // 
            this.groupBox_Motor.Controls.Add(this.groupBox_Cosine);
            this.groupBox_Motor.Controls.Add(this.groupBox_Sine);
            this.groupBox_Motor.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox_Motor.Location = new System.Drawing.Point(0, 27);
            this.groupBox_Motor.Margin = new System.Windows.Forms.Padding(20);
            this.groupBox_Motor.Name = "groupBox_Motor";
            this.groupBox_Motor.Size = new System.Drawing.Size(225, 396);
            this.groupBox_Motor.TabIndex = 1;
            this.groupBox_Motor.TabStop = false;
            this.groupBox_Motor.Text = "Motor A";
            // 
            // groupBox_Sine
            // 
            this.groupBox_Sine.Controls.Add(this.numericUpDown_Sine_Phase_Deg);
            this.groupBox_Sine.Controls.Add(this.numericUpDown_Sine_Offset);
            this.groupBox_Sine.Controls.Add(this.numericUpDown_Sine_Amplitude);
            this.groupBox_Sine.Controls.Add(this.numericUpDown_Sine_Frequency);
            this.groupBox_Sine.Controls.Add(this.checkBox_Enable_Sine);
            this.groupBox_Sine.Controls.Add(this.label4);
            this.groupBox_Sine.Controls.Add(this.label3);
            this.groupBox_Sine.Controls.Add(this.label2);
            this.groupBox_Sine.Controls.Add(this.label1);
            this.groupBox_Sine.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox_Sine.Location = new System.Drawing.Point(3, 16);
            this.groupBox_Sine.Name = "groupBox_Sine";
            this.groupBox_Sine.Size = new System.Drawing.Size(219, 173);
            this.groupBox_Sine.TabIndex = 0;
            this.groupBox_Sine.TabStop = false;
            this.groupBox_Sine.Text = "Sine";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Frequency (Hz): ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Amplitude (m): ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(58, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Offset (m): ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 147);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(70, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Phase (deg): ";
            // 
            // checkBox_Enable_Sine
            // 
            this.checkBox_Enable_Sine.AutoSize = true;
            this.checkBox_Enable_Sine.Checked = true;
            this.checkBox_Enable_Sine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Enable_Sine.Location = new System.Drawing.Point(9, 19);
            this.checkBox_Enable_Sine.Name = "checkBox_Enable_Sine";
            this.checkBox_Enable_Sine.Size = new System.Drawing.Size(59, 17);
            this.checkBox_Enable_Sine.TabIndex = 4;
            this.checkBox_Enable_Sine.Text = "Enable";
            this.checkBox_Enable_Sine.UseVisualStyleBackColor = true;
            this.checkBox_Enable_Sine.CheckedChanged += new System.EventHandler(this.Enable_Check_Change);
            // 
            // numericUpDown_Sine_Frequency
            // 
            this.numericUpDown_Sine_Frequency.DecimalPlaces = 2;
            this.numericUpDown_Sine_Frequency.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Sine_Frequency.Location = new System.Drawing.Point(160, 44);
            this.numericUpDown_Sine_Frequency.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDown_Sine_Frequency.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numericUpDown_Sine_Frequency.Name = "numericUpDown_Sine_Frequency";
            this.numericUpDown_Sine_Frequency.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Sine_Frequency.TabIndex = 5;
            this.numericUpDown_Sine_Frequency.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Sine_Frequency.ValueChanged += new System.EventHandler(this.Numerics_Sine_Params_Changed);
            // 
            // numericUpDown_Sine_Amplitude
            // 
            this.numericUpDown_Sine_Amplitude.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown_Sine_Amplitude.Location = new System.Drawing.Point(160, 77);
            this.numericUpDown_Sine_Amplitude.Maximum = new decimal(new int[] {
            64000,
            0,
            0,
            0});
            this.numericUpDown_Sine_Amplitude.Name = "numericUpDown_Sine_Amplitude";
            this.numericUpDown_Sine_Amplitude.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Sine_Amplitude.TabIndex = 5;
            this.numericUpDown_Sine_Amplitude.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_Sine_Amplitude.ValueChanged += new System.EventHandler(this.Numerics_Sine_Params_Changed);
            // 
            // numericUpDown_Sine_Offset
            // 
            this.numericUpDown_Sine_Offset.Location = new System.Drawing.Point(160, 110);
            this.numericUpDown_Sine_Offset.Maximum = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            this.numericUpDown_Sine_Offset.Name = "numericUpDown_Sine_Offset";
            this.numericUpDown_Sine_Offset.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Sine_Offset.TabIndex = 5;
            this.numericUpDown_Sine_Offset.ValueChanged += new System.EventHandler(this.Numerics_Sine_Params_Changed);
            // 
            // numericUpDown_Sine_Phase_Deg
            // 
            this.numericUpDown_Sine_Phase_Deg.DecimalPlaces = 1;
            this.numericUpDown_Sine_Phase_Deg.Location = new System.Drawing.Point(160, 143);
            this.numericUpDown_Sine_Phase_Deg.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown_Sine_Phase_Deg.Name = "numericUpDown_Sine_Phase_Deg";
            this.numericUpDown_Sine_Phase_Deg.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Sine_Phase_Deg.TabIndex = 5;
            this.numericUpDown_Sine_Phase_Deg.ValueChanged += new System.EventHandler(this.Numerics_Sine_Params_Changed);
            // 
            // groupBox_Cosine
            // 
            this.groupBox_Cosine.Controls.Add(this.numericUpDown_Cosine_Phase_Deg);
            this.groupBox_Cosine.Controls.Add(this.numericUpDown_Cosine_Offset);
            this.groupBox_Cosine.Controls.Add(this.numericUpDown_Cosine_Amplitude);
            this.groupBox_Cosine.Controls.Add(this.numericUpDown_Cosine_Frequency);
            this.groupBox_Cosine.Controls.Add(this.checkBox_Enable_Cosine);
            this.groupBox_Cosine.Controls.Add(this.label5);
            this.groupBox_Cosine.Controls.Add(this.label6);
            this.groupBox_Cosine.Controls.Add(this.label7);
            this.groupBox_Cosine.Controls.Add(this.label8);
            this.groupBox_Cosine.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox_Cosine.Location = new System.Drawing.Point(3, 189);
            this.groupBox_Cosine.Name = "groupBox_Cosine";
            this.groupBox_Cosine.Size = new System.Drawing.Size(219, 173);
            this.groupBox_Cosine.TabIndex = 1;
            this.groupBox_Cosine.TabStop = false;
            this.groupBox_Cosine.Text = "Cosine";
            // 
            // numericUpDown_Cosine_Phase_Deg
            // 
            this.numericUpDown_Cosine_Phase_Deg.DecimalPlaces = 1;
            this.numericUpDown_Cosine_Phase_Deg.Location = new System.Drawing.Point(160, 143);
            this.numericUpDown_Cosine_Phase_Deg.Maximum = new decimal(new int[] {
            360,
            0,
            0,
            0});
            this.numericUpDown_Cosine_Phase_Deg.Name = "numericUpDown_Cosine_Phase_Deg";
            this.numericUpDown_Cosine_Phase_Deg.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Cosine_Phase_Deg.TabIndex = 5;
            // 
            // numericUpDown_Cosine_Offset
            // 
            this.numericUpDown_Cosine_Offset.Location = new System.Drawing.Point(160, 110);
            this.numericUpDown_Cosine_Offset.Maximum = new decimal(new int[] {
            32000,
            0,
            0,
            0});
            this.numericUpDown_Cosine_Offset.Name = "numericUpDown_Cosine_Offset";
            this.numericUpDown_Cosine_Offset.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Cosine_Offset.TabIndex = 5;
            // 
            // numericUpDown_Cosine_Amplitude
            // 
            this.numericUpDown_Cosine_Amplitude.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDown_Cosine_Amplitude.Location = new System.Drawing.Point(160, 77);
            this.numericUpDown_Cosine_Amplitude.Maximum = new decimal(new int[] {
            64000,
            0,
            0,
            0});
            this.numericUpDown_Cosine_Amplitude.Name = "numericUpDown_Cosine_Amplitude";
            this.numericUpDown_Cosine_Amplitude.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Cosine_Amplitude.TabIndex = 5;
            this.numericUpDown_Cosine_Amplitude.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // numericUpDown_Cosine_Frequency
            // 
            this.numericUpDown_Cosine_Frequency.DecimalPlaces = 2;
            this.numericUpDown_Cosine_Frequency.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Cosine_Frequency.Location = new System.Drawing.Point(160, 44);
            this.numericUpDown_Cosine_Frequency.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numericUpDown_Cosine_Frequency.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numericUpDown_Cosine_Frequency.Name = "numericUpDown_Cosine_Frequency";
            this.numericUpDown_Cosine_Frequency.Size = new System.Drawing.Size(59, 20);
            this.numericUpDown_Cosine_Frequency.TabIndex = 5;
            this.numericUpDown_Cosine_Frequency.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numericUpDown_Cosine_Frequency.ValueChanged += new System.EventHandler(this.Numerics_Cosine_Params_Changed);
            // 
            // checkBox_Enable_Cosine
            // 
            this.checkBox_Enable_Cosine.AutoSize = true;
            this.checkBox_Enable_Cosine.Checked = true;
            this.checkBox_Enable_Cosine.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Enable_Cosine.Location = new System.Drawing.Point(9, 19);
            this.checkBox_Enable_Cosine.Name = "checkBox_Enable_Cosine";
            this.checkBox_Enable_Cosine.Size = new System.Drawing.Size(59, 17);
            this.checkBox_Enable_Cosine.TabIndex = 4;
            this.checkBox_Enable_Cosine.Text = "Enable";
            this.checkBox_Enable_Cosine.UseVisualStyleBackColor = true;
            this.checkBox_Enable_Cosine.CheckedChanged += new System.EventHandler(this.Enable_Check_Change);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Phase (deg): ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 114);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Offset (m): ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 81);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 13);
            this.label7.TabIndex = 1;
            this.label7.Text = "Amplitude (m): ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(85, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "Frequency (Hz): ";
            // 
            // Motor_Trajectory_Component
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox_Motor);
            this.Controls.Add(this.checkBox_Enable);
            this.Name = "Motor_Trajectory_Component";
            this.Size = new System.Drawing.Size(225, 423);
            this.Load += new System.EventHandler(this.Motor_Trajectory_Load);
            this.groupBox_Motor.ResumeLayout(false);
            this.groupBox_Sine.ResumeLayout(false);
            this.groupBox_Sine.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Frequency)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Amplitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Offset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Sine_Phase_Deg)).EndInit();
            this.groupBox_Cosine.ResumeLayout(false);
            this.groupBox_Cosine.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Phase_Deg)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Offset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Amplitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Cosine_Frequency)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_Enable;
        private System.Windows.Forms.GroupBox groupBox_Motor;
        private System.Windows.Forms.GroupBox groupBox_Sine;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox_Enable_Sine;
        private System.Windows.Forms.NumericUpDown numericUpDown_Sine_Phase_Deg;
        private System.Windows.Forms.NumericUpDown numericUpDown_Sine_Offset;
        private System.Windows.Forms.NumericUpDown numericUpDown_Sine_Amplitude;
        private System.Windows.Forms.NumericUpDown numericUpDown_Sine_Frequency;
        private System.Windows.Forms.GroupBox groupBox_Cosine;
        private System.Windows.Forms.NumericUpDown numericUpDown_Cosine_Phase_Deg;
        private System.Windows.Forms.NumericUpDown numericUpDown_Cosine_Offset;
        private System.Windows.Forms.NumericUpDown numericUpDown_Cosine_Amplitude;
        private System.Windows.Forms.NumericUpDown numericUpDown_Cosine_Frequency;
        private System.Windows.Forms.CheckBox checkBox_Enable_Cosine;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}
