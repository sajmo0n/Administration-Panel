using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace AdministrationPanel_SerialPort
{
    public partial class Form1 : Form
    {
        string DataOut;
        string DataIn;
        string SendWith;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cBoxCOMPort.Items.AddRange(ports);

            chBoxWrite.Checked = true;
            chBoxWrite.Checked = false;
            SendWith = "Write";

            btnOpen.Enabled = true;
            btnClose.Enabled = false;

            chBoxAlwaysUpdate.Checked = false;
            chBoxAddToOldData.Checked = true;

        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cBoxCOMPort.Text;
                serialPort1.BaudRate = Convert.ToInt32(cBoxBaudRate.Text);
                serialPort1.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), cBoxParityBits.Text);

                serialPort1.Open();
                progressBar1.Value = 100;
                btnOpen.Enabled = false;
                btnClose.Enabled = true;
                lblStatusCom.Text = "ON";
            }
            catch(Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatusCom.Text = "OFF";
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                serialPort1.Close();
                progressBar1.Value = 0;
                btnOpen.Enabled = true;
                btnClose.Enabled = false;
                lblStatusCom.Text = "OFF";
            }
            
        }

        private void btnSendData_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                DataOut = tBoxSendDataOut.Text;
                if(SendWith == "WriteLine")
                {
                    serialPort1.WriteLine(DataOut);
                }
                else if(SendWith == "Write")
                {
                    serialPort1.Write(DataOut);
                }
            }
        }

        private void btnClearDataOut_Click(object sender, EventArgs e)
        {
            if(tBoxSendDataOut.Text != "")
            {
                tBoxSendDataOut.Text = "";
            }
        }

        private void chBoxWriteLine_CheckedChanged(object sender, EventArgs e)
        {
            if(chBoxWriteLine.Checked)
            {
                SendWith = "WriteLine";
                chBoxWrite.Checked = false;
                chBoxWriteLine.Checked = true;
            }
        }

        private void chBoxWrite_CheckedChanged(object sender, EventArgs e)
        {
            if(chBoxWrite.Checked)
            {
                SendWith = "Write";
                chBoxWrite.Checked = true;
                chBoxWriteLine.Checked = false;
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataIn = serialPort1.ReadExisting();
            this.Invoke(new EventHandler(ShowData));
        }

        private void ShowData(object sender, EventArgs e)
        {
            if(chBoxAlwaysUpdate.Checked)
            {
                tBoxDataIn.Text = DataIn;
            }
            else if(chBoxAddToOldData.Checked)
            {
                tBoxDataIn.Text += DataIn;
            }

            
        }

        private void chBoxAlwaysUpdate_CheckedChanged(object sender, EventArgs e)
        {
            if(chBoxAlwaysUpdate.Checked)
            {
                chBoxAlwaysUpdate.Checked = true;
                chBoxAddToOldData.Checked = false;
            }
            else
            {
                chBoxAddToOldData.Checked = true;
            }
        }

        private void chBoxAddToOldData_CheckedChanged(object sender, EventArgs e)
        {
            if(chBoxAddToOldData.Checked)
            {
                chBoxAlwaysUpdate.Checked = false;
                chBoxAddToOldData.Checked = true;
            }
            else
            {
                chBoxAlwaysUpdate.Checked = true;
            }
        }

        private void btnClearDataIn_Click(object sender, EventArgs e)
        {
            if (tBoxDataIn.Text != "")
            {
                tBoxDataIn.Text = "";
            }
        }

        private void btnReadIDCards_Click(object sender, EventArgs e)
        {
            lstBoxIDCards.Items.Add("ID Card 1");
            lstBoxIDCards.Items.Add("ID Card 2");
            lstBoxIDCards.Items.Add("ID Card 3");
            lstBoxIDCards.Items.Add("ID Card 4");
            lstBoxIDCards.Items.Add("ID Card 5");
            lstBoxIDCards.Items.Add("ID Card 6");
        }

        private void btnClearIDCardsList_Click(object sender, EventArgs e)
        {
            lstBoxIDCards.Items.Clear();
        }
    }
}
