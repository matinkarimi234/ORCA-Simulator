using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Motor_Trajectory_Component
{
    public partial class Motor_Trajectory_Component: UserControl
    {
        WaveParams sine_Params = new WaveParams();
        public WaveParams Sine_Params
        {
            get { return sine_Params; }
            set
            {
                sine_Params = value;
                // update UI from struct if needed
                checkBox_Enable_Sine.Checked = value.Enabled;
                numericUpDown_Sine_Frequency.Value = (decimal)value.FrequencyHz;
                numericUpDown_Sine_Amplitude.Value = value.Amplitude;
                numericUpDown_Sine_Offset.Value = value.Offset;
                numericUpDown_Sine_Phase_Deg.Value =
                    (decimal)(value.PhaseRad * 180.0 / Math.PI);
            }
        }

        public WaveParams Cosine_Params
        {
            get { return cosine_Params; }
            set
            {
                cosine_Params = value;
                checkBox_Enable_Cosine.Checked = value.Enabled;
                numericUpDown_Cosine_Frequency.Value = (decimal)value.FrequencyHz;
                numericUpDown_Cosine_Amplitude.Value = value.Amplitude;
                numericUpDown_Cosine_Offset.Value = value.Offset;
                numericUpDown_Cosine_Phase_Deg.Value =
                    (decimal)(value.PhaseRad * 180.0 / Math.PI);
            }
        }


        WaveParams cosine_Params = new WaveParams();

        NumericUpDown[] sine_Params_Numerics;
        NumericUpDown[] cosine_Params_Numerics;

        bool is_Motor_Enabled = true;
        public bool Is_Motor_Enabled
        {
            get { return is_Motor_Enabled; }
            set { is_Motor_Enabled = value; }
        }

        string motor_Name = "A";
        public string Motor_Name
        {
            get { return motor_Name; }
            set 
            {
                if (value != null)
                {
                    motor_Name = value;

                    SetControlTextSafe(checkBox_Enable, $"Enable Motor {value}");
                    SetControlTextSafe(groupBox_Motor, $"Motor {value}");
                }
                
            }
        }

        public Motor_Trajectory_Component()
        {
            InitializeComponent();

            sine_Params = new WaveParams(true, 0.05, 10000, 0, 0.0);
            cosine_Params = new WaveParams(true, 0.10, 10000, 0, 0.0);

            Sine_Params = sine_Params;     // sync UI
            Cosine_Params = cosine_Params; // sync UI

            sine_Params_Numerics = new NumericUpDown[]
            {
                numericUpDown_Sine_Frequency,
                numericUpDown_Sine_Amplitude,
                numericUpDown_Sine_Offset,
                numericUpDown_Sine_Phase_Deg
            };
            
            cosine_Params_Numerics = new NumericUpDown[]
            {
                numericUpDown_Cosine_Frequency,
                numericUpDown_Cosine_Amplitude,
                numericUpDown_Cosine_Offset,
                numericUpDown_Cosine_Phase_Deg
            };


        }

        private void Motor_Trajectory_Load(object sender, EventArgs e)
        {

        }


        public static void SetControlTextSafe(Control control, string text)
        {
            if (control == null || control.IsDisposed)
                return;

            if (control.InvokeRequired)
            {
                try
                {
                    control.Invoke((MethodInvoker)(() =>
                    {
                        if (!control.IsDisposed)
                            control.Text = text;
                    }));
                }
                catch (ObjectDisposedException)
                {
                }
            }
            else
            {
                if (!control.IsDisposed)
                    control.Text = text;
            }
        }

        public static void SetControlEnableSafe(Control control, bool is_on)
        {
            if (control == null || control.IsDisposed)
                return;

            if (control.InvokeRequired)
            {
                try
                {
                    control.Invoke((MethodInvoker)(() =>
                    {
                        if (!control.IsDisposed)
                            control.Enabled = is_on;
                    }));
                }
                catch (ObjectDisposedException)
                {
                }
            }
            else
            {
                if (!control.IsDisposed)
                    control.Enabled = is_on;
            }
        }

        private void checkBox_Enable_CheckedChanged(object sender, EventArgs e)
        {
            is_Motor_Enabled = checkBox_Enable.Checked;

            groupBox_Motor.Enabled = is_Motor_Enabled;
        }

        private void Enable_Check_Change(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            switch (checkBox.Name)
            {
                case "checkBox_Enable_Sine":

                    foreach (var num in sine_Params_Numerics)
                    {
                        SetControlEnableSafe(num, checkBox.Checked);
                    }

                    sine_Params.Enabled = checkBox.Checked;

                    break;

                case "checkBox_Enable_Cosine":

                    foreach (var num in cosine_Params_Numerics)
                    {
                        SetControlEnableSafe(num, checkBox.Checked);
                    }

                    cosine_Params.Enabled = checkBox.Checked;

                    break;

                default:
                    break;
            }
        }

        private void Numerics_Sine_Params_Changed(object sender, EventArgs e)
        {
            NumericUpDown numeric = (NumericUpDown)sender;

            switch (numeric.Name)
            {
                case "numericUpDown_Sine_Frequency":

                    sine_Params.FrequencyHz = Convert.ToDouble(numeric.Value);

                    break;

                case "numericUpDown_Sine_Amplitude":

                    sine_Params.Amplitude = Convert.ToInt32(numeric.Value);

                    break;

                case "numericUpDown_Sine_Offset":

                    sine_Params.Offset = Convert.ToInt32(numeric.Value);

                    break;

                case "numericUpDown_Sine_Phase_Deg":

                    sine_Params.PhaseRad = (Convert.ToDouble(numeric.Value) * Math.PI) / 180;

                    break;

                default:
                    break;
            }
        }

        private void Numerics_Cosine_Params_Changed(object sender, EventArgs e)
        {
            NumericUpDown numeric = (NumericUpDown)sender;

            switch (numeric.Name)
            {
                case "numericUpDown_Cosine_Frequency":

                    cosine_Params.FrequencyHz = Convert.ToDouble(numeric.Value);

                    break;

                case "numericUpDown_Cosine_Amplitude":

                    cosine_Params.Amplitude = Convert.ToInt32(numeric.Value);

                    break;

                case "numericUpDown_Cosine_Offset":

                    cosine_Params.Offset = Convert.ToInt32(numeric.Value);

                    break;

                case "numericUpDown_Cosine_Phase_Deg":

                    cosine_Params.PhaseRad = (Convert.ToDouble(numeric.Value) * Math.PI) / 180;

                    break;

                default:
                    break;
            }

        }
    }
}
