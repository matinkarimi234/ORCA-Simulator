using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Simple_Client_LAN_Control
{
    partial class Simple_Buffer_Server
    {
        private IContainer components = null;
        private System.Windows.Forms.Timer timerRxFlash;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timerRxFlash = new System.Windows.Forms.Timer(this.components);
            this.pictureBox_Status = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Status)).BeginInit();
            this.SuspendLayout();
            // 
            // timerRxFlash
            // 
            this.timerRxFlash.Tick += new System.EventHandler(this.timerRxFlash_Tick);
            // 
            // pictureBox_Status
            // 
            this.pictureBox_Status.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_Status.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox_Status.Image = global::Simple_Client_LAN_Control.Properties.Resources.Connection_Fail;
            this.pictureBox_Status.Location = new System.Drawing.Point(8, 4);
            this.pictureBox_Status.Name = "pictureBox_Status";
            this.pictureBox_Status.Size = new System.Drawing.Size(25, 25);
            this.pictureBox_Status.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_Status.TabIndex = 2;
            this.pictureBox_Status.TabStop = false;
            // 
            // Simple_Client_LAN_Control
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pictureBox_Status);
            this.Name = "Simple_Client_LAN_Control";
            this.Size = new System.Drawing.Size(40, 38);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_Status)).EndInit();
            this.ResumeLayout(false);

        }

        private PictureBox pictureBox_Status;
    }
}
