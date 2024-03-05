using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ClientForm
{

    public partial class Form1 : Form
    {
        private InstanceContext context;
        private Proxy.ChatServiceClient server;
        public EncryptionAlgoritm messageEncription;
        public string[] poruke;
        public string username;
        public string whomToSend;
        public bool showEncryptedMessage;
        public int[] key;
        public string keyString;
        public string encryptionAlgoritm;
        private byte[] fileBytes;
        public byte[] fileRecived;
        public string filePath;
        public string filePathSaving;
        private string selectedUser;
        public Form1(string _username, int[] _key, string _encryptionAlgoritm)
        {

            InitializeComponent();
            this.fileBytes = new byte[0];
            this.key = _key;
            this.encryptionAlgoritm = _encryptionAlgoritm;
            this.poruke = new string[0];
            this.fileRecived = new byte[0];
            if (encryptionAlgoritm == "A52" || encryptionAlgoritm == "CFB")
            {
                this.keyString = MakeStringOutOfInt(key);
            }
            switch (encryptionAlgoritm)
            {
                case "DT": messageEncription = new DoubleTransposition(key); break;
                //case "A52": messageEncription = new A52(keyString); break;
                case "CFB": messageEncription = new CFB(keyString, "11001100"); break;
            }

            context = new InstanceContext(new MyCallback(this));
            server = new Proxy.ChatServiceClient(context);


            this.username = _username;
            this.showEncryptedMessage = false;

            server.Join(username);

            label1.Text += username;

        }

        private string MakeStringOutOfInt(int[] key)
        {
            string result = string.Concat(key.Select(x => x.ToString()));
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string fileEncrypted = "";
            string newText = textBox1.Text;
            var fileEncriptionBytes = fileBytes;
            if (newText == "")
            {
                MessageBox.Show("Please enter a message");
                return;
            }

            string encryptedMessage = "";
            if (key.Length == 0)
            {
                //message
                encryptedMessage = newText;

                //file
                fileEncrypted = newText;
            }
            else
            {
                //for message
                encryptedMessage = messageEncription.Encrypt(newText);

                //for file
                fileEncrypted = BitConverter.ToString(fileBytes);
                fileEncrypted = messageEncription.Encrypt(fileEncrypted);
                if (messageEncription is CFB)
                {
                    fileEncrypted = CFB.MakeStringOutOfBinaryNumber(fileEncrypted);
                }
                //var fileDecrypted = messageEncription.Decrypt(fileEncrypted);
                //fileEncriptionBytes = StringToByteArray(fileEncrypted);
            }

            if (messageEncription is DoubleTransposition || key.Length == 0)
            {
                encryptedMessage = CFB.MakeBinaryNumberOutOfString(encryptedMessage);
            }
            string porukaUChatu = $"{this.username}" + "_" + $"{encryptedMessage}" + "_" + $"{whomToSend}";

            Array.Resize(ref poruke, poruke.Length + 1);
            poruke[poruke.Length - 1] = porukaUChatu;

            if (fileBytes.Length != 0)
            {

                server.SendFile(fileEncrypted, filePath, selectedUser);
                fileNameLabel.Text = "";
            }
            else
            {
                listBox1.Items.Add(newText);
                server.SendMessage(encryptedMessage, whomToSend);
            }
            textBox1.Clear();
            textBox1.Enabled = true;
        }

        internal void AddOnlineUsersToListbox(string[] users)
        {
            //foreach (var user in users)
            //{
            //    if(username != user)
            //    {
            //        listBox2.Items.Add(user);
            //    }
            //}

            HashSet<string> uniqueUsers = new HashSet<string>();

            foreach (var user in users)
            {
                // Check if the user is not already in the HashSet
                if (uniqueUsers.Add(user) && username != user)
                {
                    // If the user is unique, add it to listBox2
                    listBox2.Items.Add(user);
                }
            }
        }
        internal void AddUserToListBox(string user)
        {
            if (!listBox2.Items.Contains(user))
            {
                listBox2.Items.Add(user);

            }
        }

        public void DisplayMessage(string message)
        {
            listBox1.Items.Add(message);
        }
        internal void DisplayEncryptedMessage(string receivedMessageCoded)
        {
            listBox3.Items.Add(receivedMessageCoded);
        }
        public void DisplayFileName(string name)
        {
            string fileName = Path.GetFileName(name);
            fileNameLabel.Text = fileName;
        }
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != -1)
            {
                selectedUser = listBox2.SelectedItem.ToString();
                MessageBox.Show($"You clicked on user: {selectedUser}");
                whomToSend = selectedUser;
                listBox1.Items.Clear();

                foreach (var poruka in poruke)
                {

                    string[] parts = poruka.Split('_');


                    //var messageForOtherEncriptions = CFB.MakeStringOutOfBinaryNumber(parts[1]);

                    string decodedMessage = "";
                    if (this.key.Length == 0)
                    {
                        decodedMessage = CFB.MakeStringOutOfBinaryNumber(parts[1]);
                    }
                    else
                    {
                        if (messageEncription is CFB)
                        {
                            //var messageForCFBWhenKeyExist = CFB.MakeBinaryNumberOutOfString(parts[1]);
                            decodedMessage = messageEncription.Decrypt(parts[1]);
                        }
                        else
                        {
                            var tempStringForDT = CFB.MakeStringOutOfBinaryNumber(parts[1]);
                            decodedMessage = messageEncription.Decrypt(tempStringForDT);
                        }
                    }
                    if (parts[0] == whomToSend)
                    {
                        DisplayMessage($"{parts[0]}: {decodedMessage}");
                        continue;
                    }
                    if (parts[2] == whomToSend)
                    {
                        DisplayMessage(decodedMessage);
                        continue;
                    }
                }
            }
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.showEncryptedMessage = checkBox1.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EnterUsernameForm f0 = new EnterUsernameForm(username);
            this.Hide();
            f0.ShowDialog();
            this.Close();
        }

        public byte[] StringToByteArray(string hex)
        {
            // Remove any spaces and convert the string to uppercase
            hex = hex.Replace("-", "").ToUpper();
            hex = hex.Replace("~", "").ToUpper();

            // Check if the length of the string is odd, if so, pad with a leading zero
            if (hex.Length % 2 != 0)
            {
                hex = "0" + hex;
            }

            // Convert the hexadecimal string to a byte array
            byte[] byteArray = new byte[hex.Length / 2];
            for (int i = 0; i < byteArray.Length; i++)
            {
                byteArray[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return byteArray;
        }
        private void SaveFile(byte[] fileBytes, string fileExtension)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            // Postavite filter za odabir tipova datoteka (npr. "Text files (*.txt)|*.txt|All files (*.*)|*.*")
            saveFileDialog.Filter = $"All files (*.{fileExtension})|*.{fileExtension}";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePathSave = saveFileDialog.FileName;

                try
                {
                    // Spasi niz bajtova u datoteku
                    File.WriteAllBytes(filePathSave, fileBytes);

                    MessageBox.Show("Datoteka je uspješno sačuvana.", "Informacija", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška pri spremanju datoteke: {ex.Message}", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;

                try
                {
                    fileBytes = File.ReadAllBytes(filePath);

                    // Dodatni kod za prikazivanje imena datoteke i ekstenzije
                    string fileNameWithExtension = Path.GetFileName(filePath);
                    fileNameLabel.Text = fileNameWithExtension;

                    textBox1.Text = BitConverter.ToString(fileBytes);

                    // byte[] reversedBytesFromString = StringToByteArray(textBox1.Text);

                    string fileExtension = Path.GetExtension(filePath);

                    textBox1.Enabled = false;
                    // extensionLabel.Text = fileExtension;

                    // Sačuvaj datoteku na osnovu bajtova i ekstenzije
                    // SaveFile(fileBytes, fileExtension);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Greška pri čitanju datoteke: {ex.Message}", "Greška", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string fileExtenstion = Path.GetExtension(filePathSaving);
            SaveFile(fileRecived, fileExtenstion);
        }

        private void generateHashButton_Click(object sender, EventArgs e)
        {
            SHA256 sha = new SHA256();
            if (textBox1.Text != "")
            {
                string hash = sha.ComputeHash(textBox1.Text);
                computedHashLabel.Text = hash;
            }
            if(fileRecived.Length != 0)
            {
                string genereatedStringOutOfBytes = BitConverter.ToString(fileRecived);
                string hash = sha.ComputeHash(genereatedStringOutOfBytes);
                computedHashLabel.Text = hash;
            }

        }
    }
}
