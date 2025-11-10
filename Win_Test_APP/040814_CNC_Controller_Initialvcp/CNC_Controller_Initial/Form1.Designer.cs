namespace CNC_Controller_Initial
{
    partial class Form1
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.label_A = new System.Windows.Forms.Label();
            this.label_f = new System.Windows.Forms.Label();
            this.textBox_A = new System.Windows.Forms.TextBox();
            this.textBox_Frequency = new System.Windows.Forms.TextBox();
            this.button_Set = new System.Windows.Forms.Button();
            this.textBox_Info = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer_UpdateUI = new System.Windows.Forms.Timer(this.components);
            this.button_Go_Home = new System.Windows.Forms.Button();
            this.calibration_Component = new Reel_R_Theta_Relation.Calibration_Component();
            this.button_Go_to_User = new System.Windows.Forms.Button();
            this.button_Go_to_Center = new System.Windows.Forms.Button();
            this.button_Stop_Waver_Mode = new System.Windows.Forms.Button();
            this.textBox_TX = new System.Windows.Forms.TextBox();
            this.chart_Position = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.vcp = new Auto_Detect_VCP_Control.Auto_Detect_VCP_Control();
            ((System.ComponentModel.ISupportInitialize)(this.chart_Position)).BeginInit();
            this.SuspendLayout();
            // 
            // label_A
            // 
            this.label_A.AutoSize = true;
            this.label_A.Location = new System.Drawing.Point(22, 24);
            this.label_A.Name = "label_A";
            this.label_A.Size = new System.Drawing.Size(20, 13);
            this.label_A.TabIndex = 0;
            this.label_A.Text = "A :";
            // 
            // label_f
            // 
            this.label_f.AutoSize = true;
            this.label_f.Location = new System.Drawing.Point(22, 50);
            this.label_f.Name = "label_f";
            this.label_f.Size = new System.Drawing.Size(63, 13);
            this.label_f.TabIndex = 1;
            this.label_f.Text = "Frequency :";
            // 
            // textBox_A
            // 
            this.textBox_A.Location = new System.Drawing.Point(95, 17);
            this.textBox_A.Name = "textBox_A";
            this.textBox_A.Size = new System.Drawing.Size(100, 20);
            this.textBox_A.TabIndex = 2;
            this.textBox_A.Text = "3000";
            this.textBox_A.TextChanged += new System.EventHandler(this.textBox_A_TextChanged);
            // 
            // textBox_Frequency
            // 
            this.textBox_Frequency.Location = new System.Drawing.Point(95, 43);
            this.textBox_Frequency.Name = "textBox_Frequency";
            this.textBox_Frequency.Size = new System.Drawing.Size(100, 20);
            this.textBox_Frequency.TabIndex = 3;
            this.textBox_Frequency.Text = "0.01";
            // 
            // button_Set
            // 
            this.button_Set.Location = new System.Drawing.Point(95, 79);
            this.button_Set.Name = "button_Set";
            this.button_Set.Size = new System.Drawing.Size(75, 23);
            this.button_Set.TabIndex = 4;
            this.button_Set.Text = "Set";
            this.button_Set.UseVisualStyleBackColor = true;
            this.button_Set.Click += new System.EventHandler(this.button_Set_Click);
            // 
            // textBox_Info
            // 
            this.textBox_Info.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_Info.Location = new System.Drawing.Point(17, 213);
            this.textBox_Info.Multiline = true;
            this.textBox_Info.Name = "textBox_Info";
            this.textBox_Info.Size = new System.Drawing.Size(56, 406);
            this.textBox_Info.TabIndex = 5;
            this.textBox_Info.TextChanged += new System.EventHandler(this.textBox_Height_Array_TextChanged);
            // 
            // timer1
            // 
            this.timer1.Interval = 250;
            // 
            // timer_UpdateUI
            // 
            this.timer_UpdateUI.Enabled = true;
            this.timer_UpdateUI.Tick += new System.EventHandler(this.timer_UpdateUI_Tick);
            // 
            // button_Go_Home
            // 
            this.button_Go_Home.Location = new System.Drawing.Point(25, 111);
            this.button_Go_Home.Name = "button_Go_Home";
            this.button_Go_Home.Size = new System.Drawing.Size(75, 23);
            this.button_Go_Home.TabIndex = 9;
            this.button_Go_Home.Text = "Home";
            this.button_Go_Home.UseVisualStyleBackColor = true;
            this.button_Go_Home.Click += new System.EventHandler(this.General_button_Manual);
            // 
            // calibration_Component
            // 
            this.calibration_Component.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.calibration_Component.Cable_Thickness = 0.001D;
            this.calibration_Component.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.calibration_Component.Location = new System.Drawing.Point(95, 213);
            this.calibration_Component.Minimum_Reel_Radius = 0.025D;
            this.calibration_Component.Name = "calibration_Component";
            this.calibration_Component.Point_Counts = 10000;
            this.calibration_Component.Size = new System.Drawing.Size(47, 547);
            this.calibration_Component.Stepper_Name = null;
            this.calibration_Component.TabIndex = 8;
            this.calibration_Component.Theta_Step = 0.05D;
            // 
            // button_Go_to_User
            // 
            this.button_Go_to_User.Location = new System.Drawing.Point(25, 169);
            this.button_Go_to_User.Name = "button_Go_to_User";
            this.button_Go_to_User.Size = new System.Drawing.Size(75, 23);
            this.button_Go_to_User.TabIndex = 11;
            this.button_Go_to_User.Text = "Go to User";
            this.button_Go_to_User.UseVisualStyleBackColor = true;
            this.button_Go_to_User.Click += new System.EventHandler(this.General_button_Manual);
            // 
            // button_Go_to_Center
            // 
            this.button_Go_to_Center.Location = new System.Drawing.Point(25, 140);
            this.button_Go_to_Center.Name = "button_Go_to_Center";
            this.button_Go_to_Center.Size = new System.Drawing.Size(75, 23);
            this.button_Go_to_Center.TabIndex = 12;
            this.button_Go_to_Center.Text = "Center";
            this.button_Go_to_Center.UseVisualStyleBackColor = true;
            this.button_Go_to_Center.Click += new System.EventHandler(this.General_button_Manual);
            // 
            // button_Stop_Waver_Mode
            // 
            this.button_Stop_Waver_Mode.Location = new System.Drawing.Point(176, 79);
            this.button_Stop_Waver_Mode.Name = "button_Stop_Waver_Mode";
            this.button_Stop_Waver_Mode.Size = new System.Drawing.Size(69, 23);
            this.button_Stop_Waver_Mode.TabIndex = 13;
            this.button_Stop_Waver_Mode.Text = "Stop";
            this.button_Stop_Waver_Mode.UseVisualStyleBackColor = true;
            this.button_Stop_Waver_Mode.Click += new System.EventHandler(this.button_Stop_Waver_Mode_Click);
            // 
            // textBox_TX
            // 
            this.textBox_TX.Location = new System.Drawing.Point(880, 50);
            this.textBox_TX.Multiline = true;
            this.textBox_TX.Name = "textBox_TX";
            this.textBox_TX.Size = new System.Drawing.Size(407, 731);
            this.textBox_TX.TabIndex = 15;
            // 
            // chart_Position
            // 
            this.chart_Position.BackColor = System.Drawing.Color.LightGray;
            chartArea4.AxisX.Interval = 0.5D;
            chartArea4.AxisX.Maximum = 10D;
            chartArea4.AxisX.Minimum = 0D;
            chartArea4.Name = "ChartArea1";
            this.chart_Position.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            this.chart_Position.Legends.Add(legend4);
            this.chart_Position.Location = new System.Drawing.Point(194, 140);
            this.chart_Position.Name = "chart_Position";
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.Legend = "Legend1";
            series4.Name = "Series1";
            this.chart_Position.Series.Add(series4);
            this.chart_Position.Size = new System.Drawing.Size(912, 567);
            this.chart_Position.TabIndex = 16;
            this.chart_Position.Text = "chart1";
            // 
            // vcp
            // 
            this.vcp.Baud_Rate = 9600;
            this.vcp.Close_Serialport = false;
            this.vcp.Communication_Response = 0;
            this.vcp.Communication_Response_Byte_Index = 0;
            this.vcp.Is_Minimised = true;
            this.vcp.Location = new System.Drawing.Point(279, 8);
            this.vcp.Name = "vcp";
            this.vcp.Rx_Byte_Count = 56;
            this.vcp.Rx_Bytes = new byte[] {
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0)),
        ((byte)(0))};
            this.vcp.Size = new System.Drawing.Size(480, 160);
            this.vcp.Start_Communication_Byte = 0;
            this.vcp.Start_Communication_Byte_Index = 0;
            this.vcp.Start_VCP_Connection = false;
            this.vcp.TabIndex = 17;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1314, 793);
            this.Controls.Add(this.vcp);
            this.Controls.Add(this.chart_Position);
            this.Controls.Add(this.textBox_TX);
            this.Controls.Add(this.calibration_Component);
            this.Controls.Add(this.button_Stop_Waver_Mode);
            this.Controls.Add(this.button_Go_to_Center);
            this.Controls.Add(this.button_Go_to_User);
            this.Controls.Add(this.button_Go_Home);
            this.Controls.Add(this.textBox_Info);
            this.Controls.Add(this.button_Set);
            this.Controls.Add(this.textBox_Frequency);
            this.Controls.Add(this.textBox_A);
            this.Controls.Add(this.label_f);
            this.Controls.Add(this.label_A);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart_Position)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_A;
        private System.Windows.Forms.Label label_f;
        private System.Windows.Forms.TextBox textBox_A;
        private System.Windows.Forms.TextBox textBox_Frequency;
        private System.Windows.Forms.Button button_Set;
        private System.Windows.Forms.TextBox textBox_Info;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer_UpdateUI;
        private Reel_R_Theta_Relation.Calibration_Component calibration_Component;
        private System.Windows.Forms.Button button_Go_Home;
        private System.Windows.Forms.Button button_Go_to_User;
        private System.Windows.Forms.Button button_Go_to_Center;
        private System.Windows.Forms.Button button_Stop_Waver_Mode;
        private System.Windows.Forms.TextBox textBox_TX;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart_Position;
        private Auto_Detect_VCP_Control.Auto_Detect_VCP_Control vcp;
    }
}

