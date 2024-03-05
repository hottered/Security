using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientForm
{
    public class MyCallback : Proxy.IChatServiceCallback
    {
        private Form1 form;
        public MyCallback(Form1 form)
        {
            this.form = form;
        }
        
        public void ReciveMessage(string user, string message)
        {


            string porukaUChatu = $"{user}" + "_" + $"{message}" + "_" + $"{form.username}";

            Array.Resize(ref form.poruke, form.poruke.Length + 1);
            form.poruke[form.poruke.Length - 1] = porukaUChatu;


            var MessageToDecryptIfCFBExists = message;
            message = CFB.MakeStringOutOfBinaryNumber(message);
            string decodedMessage = "";
            if (form.key.Length == 0)
            {
                decodedMessage = message;
            }
            else
            {
                if (form.messageEncription is CFB)
                {
                    decodedMessage = form.messageEncription.Decrypt(MessageToDecryptIfCFBExists);

                }
                else
                {
                    decodedMessage = form.messageEncription.Decrypt(message);
                }
            }

            string receivedMessage = $"{user}: {decodedMessage}";
            string receivedMessageCoded = $"{user}: {message}";

            if (form.whomToSend == user || form.username == user)
            {
                form.DisplayMessage(receivedMessage);
                if (form.showEncryptedMessage)
                {
                    form.DisplayEncryptedMessage(receivedMessageCoded);
                }
            }
        }
        
        public void BroadcastOnlineUsers(string user)
        {
            form.AddUserToListBox(user);
        }
        
        public void GetAllUsersWhoAreOnline(string[] users)
        {
            form.AddOnlineUsersToListbox(users);
        }
        
        public void ReturnChatWithUsers(string[] messages)
        {
            //form.DisplayChat(messages);
            form.poruke = messages;
            //form.DisplayChat();
        }

        public void ReciveFile(string user,string fileBytes, string fileName)
        {         
            if (form.whomToSend == user || form.username == user)
            {
                string temp = "";
                byte[] bytesFileDecrypted = new byte[0];
                if (form.key.Length != 0)
                {
                    //if (form.messageEncription is MessageEncription)
                    //{
                    if(form.messageEncription is CFB)
                    {
                        string strinForCFBDecrypt = CFB.MakeBinaryNumberOutOfString(fileBytes);
                        temp = form.messageEncription.Decrypt(strinForCFBDecrypt);
                        
                    }
                    else
                    {
                        temp = form.messageEncription.Decrypt(fileBytes);
                    }
                        bytesFileDecrypted = form.StringToByteArray(temp);
                        //string fileDecryptionString = BitConverter.ToString(fileBytesRecived);
                        //fileDecryptionString = form.messageEncription.Decrypt(fileDecryptionString);
                        //fileBytesRecived = form.StringToByteArray(fileDecryptionString);
                    //}
                }
                else
                {
                    //if(form.messageEncription is MessageEncription)
                    //{
                    bytesFileDecrypted = form.StringToByteArray(fileBytes);
                    //}
                }
                form.DisplayFileName(fileName);
                form.fileRecived = bytesFileDecrypted;
                form.filePathSaving = fileName;
            }
        }
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EnterUsernameForm());

        }
    }
}
