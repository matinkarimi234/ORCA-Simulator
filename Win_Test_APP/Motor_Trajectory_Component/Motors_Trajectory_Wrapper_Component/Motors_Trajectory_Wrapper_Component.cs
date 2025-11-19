using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Motor_Trajectory_Component;

namespace Motors_Trajectory_Wrapper_Component
{
    public partial class Motors_Trajectory_Wrapper_Component: UserControl
    {


        public Motors_Trajectory_Wrapper_Component()
        {
            InitializeComponent();
        }

        private void button_Generate_File_Click(object sender, EventArgs e)
        {
            int batchMs = (int)numericUpDown_BatchMs.Value;
            double periodSeconds = (double)numericUpDown_Period_Sec.Value;

            // 1) Get wave parameters from each GUI control
            var sineA = motor_Trajectory_A.Sine_Params;
            var cosA= motor_Trajectory_A.Cosine_Params;

            var sineB = motor_Trajectory_B.Sine_Params;
            var cosB = motor_Trajectory_B.Cosine_Params;

            var sineC = motor_Trajectory_C.Sine_Params;
            var cosC = motor_Trajectory_C.Cosine_Params;

            // 2) Build MotorTrajectory objects
            var motorA = new MotorTrajectory(sineA, cosA, batchMs, periodSeconds);
            var motorB = new MotorTrajectory(sineB, cosB, batchMs, periodSeconds);
            var motorC = new MotorTrajectory(sineC, cosC, batchMs, periodSeconds);

            bool motorA_En = motor_Trajectory_A.Is_Motor_Enabled;
            bool motorB_En = motor_Trajectory_B.Is_Motor_Enabled;
            bool motorC_En = motor_Trajectory_C.Is_Motor_Enabled;

            // 3) Wrap into 3-motor frame generator
            var wrapper = new ThreeMotorTrajectoryWrapper(motorA, motorA_En, motorB, motorB_En, motorC, motorC_En);

            // 4) Save to file
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.FileName = "three_motor_trajectory.txt";

                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    wrapper.WritePrettyFile(sfd.FileName);
                    MessageBox.Show(this,
                        "File generated:\n" + sfd.FileName,
                        "Done",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }
    }
}
