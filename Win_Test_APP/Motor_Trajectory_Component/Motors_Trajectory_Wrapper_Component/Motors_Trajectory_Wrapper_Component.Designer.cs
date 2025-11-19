namespace Motors_Trajectory_Wrapper_Component
{
    partial class Motors_Trajectory_Wrapper_Component
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
            this.motor_Trajectory_A = new Motor_Trajectory_Component.Motor_Trajectory_Component();
            this.motor_Trajectory_B = new Motor_Trajectory_Component.Motor_Trajectory_Component();
            this.motor_Trajectory_C = new Motor_Trajectory_Component.Motor_Trajectory_Component();
            this.button_Generate_File = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown_BatchMs = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Period_Sec = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_BatchMs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Period_Sec)).BeginInit();
            this.SuspendLayout();
            // 
            // motor_Trajectory_A
            // 
            this.motor_Trajectory_A.Is_Motor_Enabled = true;
            this.motor_Trajectory_A.Location = new System.Drawing.Point(11, 8);
            this.motor_Trajectory_A.Motor_Name = "A";
            this.motor_Trajectory_A.Name = "motor_Trajectory_A";
            this.motor_Trajectory_A.Size = new System.Drawing.Size(233, 432);
            this.motor_Trajectory_A.TabIndex = 0;
            // 
            // motor_Trajectory_B
            // 
            this.motor_Trajectory_B.Is_Motor_Enabled = true;
            this.motor_Trajectory_B.Location = new System.Drawing.Point(250, 8);
            this.motor_Trajectory_B.Motor_Name = "B";
            this.motor_Trajectory_B.Name = "motor_Trajectory_B";
            this.motor_Trajectory_B.Size = new System.Drawing.Size(233, 432);
            this.motor_Trajectory_B.TabIndex = 1;
            // 
            // motor_Trajectory_C
            // 
            this.motor_Trajectory_C.Is_Motor_Enabled = true;
            this.motor_Trajectory_C.Location = new System.Drawing.Point(489, 8);
            this.motor_Trajectory_C.Motor_Name = "C";
            this.motor_Trajectory_C.Name = "motor_Trajectory_C";
            this.motor_Trajectory_C.Size = new System.Drawing.Size(233, 432);
            this.motor_Trajectory_C.TabIndex = 2;
            // 
            // button_Generate_File
            // 
            this.button_Generate_File.Location = new System.Drawing.Point(638, 446);
            this.button_Generate_File.Name = "button_Generate_File";
            this.button_Generate_File.Size = new System.Drawing.Size(75, 23);
            this.button_Generate_File.TabIndex = 3;
            this.button_Generate_File.Text = "Generate";
            this.button_Generate_File.UseVisualStyleBackColor = true;
            this.button_Generate_File.Click += new System.EventHandler(this.button_Generate_File_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 451);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Batch Interval (msec): ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(256, 451);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Period (sec): ";
            // 
            // numericUpDown_BatchMs
            // 
            this.numericUpDown_BatchMs.Enabled = false;
            this.numericUpDown_BatchMs.Location = new System.Drawing.Point(127, 449);
            this.numericUpDown_BatchMs.Name = "numericUpDown_BatchMs";
            this.numericUpDown_BatchMs.Size = new System.Drawing.Size(56, 20);
            this.numericUpDown_BatchMs.TabIndex = 6;
            this.numericUpDown_BatchMs.Value = new decimal(new int[] {
            25,
            0,
            0,
            0});
            // 
            // numericUpDown_Period_Sec
            // 
            this.numericUpDown_Period_Sec.Location = new System.Drawing.Point(331, 449);
            this.numericUpDown_Period_Sec.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_Period_Sec.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_Period_Sec.Name = "numericUpDown_Period_Sec";
            this.numericUpDown_Period_Sec.Size = new System.Drawing.Size(56, 20);
            this.numericUpDown_Period_Sec.TabIndex = 7;
            this.numericUpDown_Period_Sec.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            // 
            // Motors_Trajectory_Wrapper_Component
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numericUpDown_Period_Sec);
            this.Controls.Add(this.numericUpDown_BatchMs);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Generate_File);
            this.Controls.Add(this.motor_Trajectory_C);
            this.Controls.Add(this.motor_Trajectory_B);
            this.Controls.Add(this.motor_Trajectory_A);
            this.Name = "Motors_Trajectory_Wrapper_Component";
            this.Size = new System.Drawing.Size(725, 476);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_BatchMs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Period_Sec)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Motor_Trajectory_Component.Motor_Trajectory_Component motor_Trajectory_A;
        private Motor_Trajectory_Component.Motor_Trajectory_Component motor_Trajectory_B;
        private Motor_Trajectory_Component.Motor_Trajectory_Component motor_Trajectory_C;
        private System.Windows.Forms.Button button_Generate_File;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDown_BatchMs;
        private System.Windows.Forms.NumericUpDown numericUpDown_Period_Sec;
    }
}
