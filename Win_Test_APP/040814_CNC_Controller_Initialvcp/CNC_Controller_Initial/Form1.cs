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
using Farand_Chart_2023;
using System.IO;
using System.IO.Ports;

namespace CNC_Controller_Initial
{
    public partial class Form1 : Form
    {
        SerialClient serial_client;
        double amplitude = 0;
        double frequency = 0;
        double w = 0;
        double graph_time = 0;
        double time = 0;
        byte[] command_Bytes = new byte[512];
        double[] L_Reel = new double[10];
        double[] theta = new double[10];
        string info_String = "";
        bool go_to_Center_Position = false;
        bool go_to_User_Position = false;
        bool go_Home_Position = false;
        bool report_Flag = true;
        int pulse_Counter = 0;
        Int32[] position_History = new Int32[10];
        Int32[] pulse_Counter_Array_Total = new Int32[100000];
        int rx_Counter = 0;
        double[] desired_Pulse = new double[10];
        int[] desired_Pulse_int = new int[10];
        int[] desired_Pulse_int_Futured = new int[10];
        int com_Counter = 0;
        byte[] framecopy_Global = new byte[128];
        byte[] decoded_framecopy_Global = new byte[128];
        string s;
        System.Threading.Timer timer;


        bool is_new_data_ready = false;
        public enum Mode
        {
            Waver_Frequency,
            No_Command,
            Manual
        }
        Mode current_Mode = Mode.No_Command;

        public enum state
        {
            First_command_Packet,
            Second_command_Packet,
            IDLE
        }
        state system_State = state.IDLE;

        public Form1()
        {
            InitializeComponent();
            calibration_Component.Start_Live_Chart();
            Initialize_VCP();
            calibration_Component.Calculate_Default_Points();
            Initialize_Chart();

        }
        private void Initialize_Chart()
        {

            //farand_Chart.XAxis.Initial_Minimum = 0;
            //farand_Chart.XAxis.Initial_Maximum = 10;
            //farand_Chart.XAxis.MajorGrid.Interval = 1;
            //farand_Chart.XAxis.MajorGrid.Labels.DecimalPlaces = 0;
            //farand_Chart.XAxis.Title.FrameSize = new SizeF(100F, 20F);
            //farand_Chart.XAxis.Title.TopMargin = -20F;
            //farand_Chart.XAxis.Title.LabelStyle.Font = new Font("Arial", 10.0F);
            //farand_Chart.XAxis.Title.Text = "time";

            //farand_Chart.YAxis.Initial_Minimum = 0;
            //farand_Chart.YAxis.Initial_Maximum = 10000000;
            //farand_Chart.YAxis.MajorGrid.Interval = 200000;
            //farand_Chart.YAxis.MajorGrid.Labels.DecimalPlaces = 0;
            //farand_Chart.YAxis.Title.FrameSize = new SizeF(100F, 20F);
            //farand_Chart.YAxis.Title.RightMargin = -100F;
            //farand_Chart.YAxis.Title.LabelStyle.Font = new Font("Arial", 10.0F);
            //farand_Chart.YAxis.Title.Text = "Pulse Counter";

            //farand_Chart.Title.LabelStyle.Font = new Font("Arial", 10.0F);
            //farand_Chart.Title.FrameSize = new SizeF(200F, 20F);
            //farand_Chart.Title.Text = "Reel Cable length & Angle Relation";

            //farand_Chart.Coordinates.LabelStyle.Font = new Font("Arial", 10.0F);
            //farand_Chart.Coordinates.FrameSize = new SizeF(120F, 20F);


            //farand_Chart.Legends.LeftMargin = 10.0F;
            //farand_Chart.GraphArea.RightMargin = 200;


            //farand_Chart.Clear_All_Farand_Graphs();


            //Farand_Chart.Farand_Graph myGraph1 = new Farand_Chart.Farand_Graph();
            //myGraph1.Name = "Reel Angle";
            //myGraph1.PointStyle.Visible = false;
            //myGraph1.LineStyle.Color = Color.SkyBlue;
            //myGraph1.PointStyle.Size = 5.0F;
            //myGraph1.PointStyle.FillColor = Color.SkyBlue;
            //myGraph1.PointStyle.LineColor = Color.SkyBlue;
            //myGraph1.PointStyle.LineWidth = 1.0F;
            //myGraph1.LineStyle.Width = 1.5F;
            //farand_Chart.Add_Farand_Graph(myGraph1);

            //Farand_Chart.Farand_Graph myGraph2 = new Farand_Chart.Farand_Graph();
            //myGraph2.Name = "Reel Angle";
            //myGraph2.PointStyle.Visible = false;
            //myGraph2.LineStyle.Color = Color.Red;
            //myGraph2.PointStyle.Size = 5.0F;
            //myGraph2.PointStyle.FillColor = Color.Red;
            //myGraph2.PointStyle.LineColor = Color.Red;
            //myGraph2.PointStyle.LineWidth = 1.0F;
            //myGraph2.LineStyle.Width = 1.5F;
            //farand_Chart.Add_Farand_Graph(myGraph2);



            //farand_Chart.GraphArea.RightMargin = 150;
            //farand_Chart.Legends.Visible = true;
            //farand_Chart.Legends.FillColor = Color.FromArgb(64, 64, 64);
            //farand_Chart.Legends.LineStyle.Visible = true;
            //farand_Chart.Legends.LineStyle.Width = 1.5F;
            //farand_Chart.Legends.LineStyle.Color = Color.FromArgb(180, 180, 180);
            //farand_Chart.Legends.FrameSize = new SizeF(130.0F, 18.0F);
            //farand_Chart.Legends.LeftMargin = 10.0F;
            //farand_Chart.Legends.LabelStyle.Font = new Font("Arial", 8.0F);
            //farand_Chart.Legends.LabelStyle.Color = Color.White;

            //farand_Chart.Refresh_All();
        }

        private void Initialize_VCP()
        {
            vcp.Rx_Byte_Count = 512;
            vcp.Baud_Rate = 38400;
            vcp.Communication_Response = 0x55;
            vcp.Communication_Response_Byte_Index = 3;
            vcp.Start_Communication_Byte = 0x55;
            vcp.Start_Communication_Byte_Index = 3;
            vcp.Start_VCP_Connection = true;
            vcp.Normal_Operation_Starts += Vcp_Normal_Operation_Starts;
            vcp.Received_Data_Ready += Vcp_Received_Data_Ready;

        }

        private void Vcp_Normal_Operation_Starts(object sender, EventArgs e)
        {

        }
        private void Vcp_Received_Data_Ready(object sender, EventArgs e)
        {
            var buf = vcp.Rx_Bytes;

            Get_Pulse_Counter_Array_FromuC(buf);
            Graph_Data();
            vcp.Send_Data(buf);

        }
        private void Get_Pulse_Counter_Array_FromuC(byte[] rx_Bytes)
        {

            for (int i = 0; i < 10; i++)
            {

                position_History[i] = (Int32)(
                    (rx_Bytes[8 * i + 1]) +
                    (rx_Bytes[8 * i + 2] << 8) +
                    (rx_Bytes[8 * i + 3] << 16) +
                    (rx_Bytes[8 * i + 4] << 24)
                );

            }

        }

        private void Graph_Data()
        {
            graph_time += 0.25;
            double d_t = 0.025;
            

            for (int i = 0; i < 10; i++)
            {
                 
                chart_Position.Series[0].Points.AddXY(graph_time + (d_t * i), position_History[i]);



            }

            if (graph_time > chart_Position.ChartAreas[0].AxisX.Maximum)
            {
                chart_Position.ChartAreas[0].AxisX.Maximum += 0.1;
                chart_Position.ChartAreas[0].AxisX.Minimum += 0.1;
            }

        }


        private void button_Set_Click(object sender, EventArgs e)
        {


        }
        private void textBox_Height_Array_TextChanged(object sender, EventArgs e)
        {

        }

        private void auto_Detect_VCP_Control1_Load_1(object sender, EventArgs e)
        {

        }

        private void General_button_Manual(object sender, EventArgs e)
        {
        }

        private void button_Stop_Waver_Mode_Click(object sender, EventArgs e)
        {
        }


        private void textBox_A_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Serial_client_OnReceiving(object sender, DataStreamEventArgs e)
        {

        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void timer_UpdateUI_Tick(object sender, EventArgs e)
        {

        }
    }
}

