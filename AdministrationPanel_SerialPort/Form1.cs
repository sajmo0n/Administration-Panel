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
        string Password;
        char SingleSign;
        bool GetIDCards;
        bool AddingNewUser;
        byte[] IDCards = new byte[110];
        byte[] BufferDeleteUserID = new byte[5];
        byte[] BufferUnlockUserID = new byte[5];
        byte[] NewPassword = new byte[4];
        byte[] BufferChangePasswordID = new byte[5];
        int SelectedIDCard;
        
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

            chBoxBytes.Checked = true;
            chBoxASCII.Checked = false;

            GetIDCards = false;
            AddingNewUser = false;

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
            this.Invoke(new EventHandler(ShowData));
        }

        private void ShowData(object sender, EventArgs e)
        {
            if(AddingNewUser)
            {
                int BytesToRead = serialPort1.BytesToRead;
                byte[] TempIDCardsBuffer = new byte[serialPort1.BytesToRead];
                serialPort1.Read(TempIDCardsBuffer, 0, serialPort1.BytesToRead);
                string TempCounter = lstBoxIDCards.Items.Count.ToString();
                int CounterItemsOnList = Int32.Parse(TempCounter);
                TempIDCardsBuffer.CopyTo(IDCards, CounterItemsOnList*11);
                byte[] TempIDCard = new byte[5];
                Array.Copy(TempIDCardsBuffer, 4 , TempIDCard, 0, 5);
                lstBoxIDCards.Items.Add(string.Join(" ", TempIDCard.Select(b => b.ToString())));
                AddingNewUser = false;
            }
            if(GetIDCards)
            {
                byte[] TempConfirmation = new byte[5];
                serialPort1.Read(TempConfirmation, 0, 5);
                if (TempConfirmation[3] == 20)
                {
                    int BytesToRead = serialPort1.BytesToRead;
                    byte[] TempIDCardsBuffer = new byte[serialPort1.BytesToRead];
                    serialPort1.Read(TempIDCardsBuffer, 0, serialPort1.BytesToRead);
                    TempIDCardsBuffer.CopyTo(IDCards, 0);

                    for (int i = 0; i < BytesToRead / 11; i++)
                    {
                        byte[] TempIDCard = new byte[5];
                        Array.Copy(TempIDCardsBuffer, ((i * 11) + 4), TempIDCard, 0, 5);
                        lstBoxIDCards.Items.Add(string.Join(" ", TempIDCard.Select(b => b.ToString())));
                    }
                    GetIDCards = false;
                }
                else
                {
                    MessageBox.Show("Blad, niepowodzenie operacji. Zaloguj sie ponownie.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    GetIDCards = false;
                }
            }
            else if(chBoxAlwaysUpdate.Checked)
            {
                if(chBoxBytes.Checked)
                {
                    byte[] TempDataOut = new byte[serialPort1.BytesToRead];
                    serialPort1.Read(TempDataOut, 0, serialPort1.BytesToRead);
                    tBoxDataIn.Text = string.Join(" ", TempDataOut.Select(b => b.ToString()));
                }
                else
                {
                    DataIn = serialPort1.ReadExisting();
                    tBoxDataIn.Text = DataIn;
                }
            }
            else if(chBoxAddToOldData.Checked)
            {
                if (chBoxBytes.Checked)
                {
                    byte[] TempDataOut = new byte[serialPort1.BytesToRead];
                    serialPort1.Read(TempDataOut, 0, serialPort1.BytesToRead);
                    tBoxDataIn.Text = string.Join(" ", TempDataOut.Select(b => b.ToString()));
                }
                else
                {
                    DataIn = serialPort1.ReadExisting();
                    tBoxDataIn.Text += DataIn;
                }
                
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

        private void btnClearIDCardsList_Click(object sender, EventArgs e)
        {
            lstBoxIDCards.Items.Clear();
            
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
               byte[] BufferAddUser = {255, 254, 5, 10, 0, 0, 0, 0, 0};
                Password = tBoxSendDataOut.Text;
                /* Commented below its option to send password as bytes not as ASCII */
                //for (int i = 0; i < Password.Length; i++)
                //{
                //    SingleSign = Password[i];
                //    BufferAddUser[i + 4] = Byte.Parse(SingleSign.ToString());  
                //}
                byte[] TempPassword = Encoding.ASCII.GetBytes(Password);
                Array.Copy(TempPassword, 0, BufferAddUser, 4, 4);
                BufferAddUser[BufferAddUser.Length - 1] = 0;
                serialPort1.Write(BufferAddUser, 0, BufferAddUser.Length);
                AddingNewUser = true;
            }
        }

        private void btnLogIn_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                GetIDCards = true;
                byte[] BufferAddUser = { 255, 254, 5, 14, 0, 0, 0, 0, 0 };
                Password = tBoxSendDataOut.Text;
                for (int i = 0; i < Password.Length; i++)
                {
                    SingleSign = Password[i];
                    BufferAddUser[i + 4] = Byte.Parse(SingleSign.ToString());
                }
                BufferAddUser[BufferAddUser.Length - 1] = 0;
                serialPort1.Write(BufferAddUser, 0, BufferAddUser.Length);
            }
        }

        private void chBoxBytes_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxBytes.Checked)
            {
                chBoxBytes.Checked = true;
                chBoxASCII.Checked = false;
            }
            else
            {
                chBoxASCII.Checked = true;
            }
        }

        private void chBoxASCII_CheckedChanged(object sender, EventArgs e)
        {
            if (chBoxASCII.Checked)
            {
                chBoxBytes.Checked = false;
                chBoxASCII.Checked = true;
            }
            else
            {
                chBoxBytes.Checked = true;
            }
        }

        private void lstBoxIDCards_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIDCard = lstBoxIDCards.SelectedIndex;
            
        }

        private void btnDeleteIDCard_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                byte[] BufferDeleteUser = {255, 254, 6, 11, 0, 0, 0, 0, 0, 0};
                for (int i = 0; i < 10; i++)
                {
                    if (i == SelectedIDCard)
                    {
                        Array.Copy(IDCards, ((i * 11) + 4), BufferDeleteUserID, 0, 5);
                    }
                }
                Array.Copy(BufferDeleteUserID, 0, BufferDeleteUser, 4, 5);
                BufferDeleteUser[BufferDeleteUser.Length - 1] = 0;
                serialPort1.Write(BufferDeleteUser, 0, BufferDeleteUser.Length);
                lstBoxIDCards.Items.RemoveAt(SelectedIDCard);
            }
        }

        private void btnUnlockIDCard_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                byte[] BufferUnlockUser = { 255, 254, 6, 12, 0, 0, 0, 0, 0, 0 };
                for (int i = 0; i < 10; i++)
                {
                    if (i == SelectedIDCard)
                    {
                        Array.Copy(IDCards, ((i * 11) + 4), BufferUnlockUserID, 0, 5);
                    }
                }
                Array.Copy(BufferUnlockUserID, 0, BufferUnlockUser, 4, 5);
                BufferUnlockUser[BufferUnlockUser.Length - 1] = 0;
                serialPort1.Write(BufferUnlockUser, 0, BufferUnlockUser.Length);
            }
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            //char SinglePasswordSign;
            if (serialPort1.IsOpen)
            {
                DataOut = tBoxSendDataOut.Text;
                //for (int i = 0; i < 4; i++)
                //{
                //    SinglePasswordSign = DataOut[i];
                //    NewPassword[i] = Byte.Parse(SinglePasswordSign.ToString());
                //}
                NewPassword = Encoding.ASCII.GetBytes(DataOut);

                byte[] BufferChangePassword = { 255, 254, 10, 13, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int i = 0; i < 10; i++)
                {
                    if (i == SelectedIDCard)
                    {
                        Array.Copy(IDCards, ((i * 11) + 4), BufferChangePasswordID, 0, 5);
                    }
                }
                Array.Copy(BufferChangePasswordID, 0, BufferChangePassword, 4, 5);
                Array.Copy(NewPassword, 0, BufferChangePassword, 9, 4);
                BufferChangePassword[BufferChangePassword.Length - 1] = 0;
                serialPort1.Write(BufferChangePassword, 0, BufferChangePassword.Length);
                
            }
        }
    }
}
