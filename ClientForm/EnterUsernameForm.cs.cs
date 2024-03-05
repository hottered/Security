using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    public partial class EnterUsernameForm : Form
    {
        private string username;
        public EnterUsernameForm()
        {
            InitializeComponent();
            username = "";
            textBox1.Text = username;
        }
        public EnterUsernameForm(string _username)
        {
            InitializeComponent();
            username = _username;
            textBox1.Text = username;

        }
        private void button1_Click(object sender, EventArgs e)
        {
            string encryptionAlgoritm = "";
            if (radioDouble.Checked)
            {
                encryptionAlgoritm = "DT";
            }
            //}else if (radioA52.Checked)
            //{
            //    encryptionAlgoritm = "A52";
            //}
            else if (radioCFB.Checked)
            {
                encryptionAlgoritm = "CFB";
            }

            if(encryptionAlgoritm == "")
            {
                MessageBox.Show("Please choose an encription algorithm");
                return;
            }
            string enteredUsername = textBox1.Text;
            if(enteredUsername == "")
            {
                MessageBox.Show("Username field cannot be empty");
                return;
            }
            string keyString = textBox2.Text;
            int[] keyIntArray;
            if (keyString.Trim() != "")
            {
                //if(encryptionAlgoritm== "A52" && keyString.Length != 8)
                //{
                //    MessageBox.Show("A52 must have a key minimum 8 chars");
                //    return;
                //}
                if (encryptionAlgoritm == "CFB" && keyString.Length != 8)
                {
                    MessageBox.Show("CFB must have a key minimum 8 chars");
                    return;
                }
                keyIntArray = keyString.Select(c => int.Parse(c.ToString())).ToArray();
            }
            else
            {
                keyIntArray = new int[0];
            }
            Form1 f1 = new Form1(enteredUsername, keyIntArray,encryptionAlgoritm);
            this.Hide();
            f1.ShowDialog();
            this.Close();
        }
    }
}
