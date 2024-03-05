using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using System.ServiceModel.MsmqIntegration;
using System.Messaging;
using System.IO;


namespace TestServer
{

    [ServiceContract]
    public interface IChatClient
    {
        [OperationContract(IsOneWay = true)]
        void ReciveMessage(string user, string message);
        
        [OperationContract(IsOneWay = true)]
        void ReciveFile(string user, string fileBytes,string fileName);

        [OperationContract(IsOneWay = true)]
        void BroadcastOnlineUsers(string user);

        [OperationContract(IsOneWay = true)]
        void GetAllUsersWhoAreOnline(string[] users);

        [OperationContract(IsOneWay = true)]
        void ReturnChatWithUsers(string[] messages);
    }

    [ServiceContract(CallbackContract = typeof(IChatClient))]
    public interface IChatService
    {
        [OperationContract(IsOneWay = true)]
        void Join(string message);
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, string userToSend);    
        [OperationContract(IsOneWay = true)]
        void SendFile(string fileBytes, string fileName, string userToSend);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        Dictionary<IChatClient, string> _users = new Dictionary<IChatClient, string>();
        static object myLock = new object();
        
        public void Join(string username)
        {
            var connection = OperationContext.Current.GetCallbackChannel<IChatClient>();

            connection.GetAllUsersWhoAreOnline(_users.Select(x => x.Value).ToArray());

            foreach (var con in _users)
            {
                con.Key.BroadcastOnlineUsers(username);
            }
            Console.WriteLine(username);
            _users[connection] = username;

            string[] usersChat = ReadMessages("messages.txt", username);

            connection.ReturnChatWithUsers(usersChat);

        }

        public void SendMessage(string message, string userToSend)
        {
            var connection = OperationContext.Current.GetCallbackChannel<IChatClient>();
            string user;

            if (!_users.TryGetValue(connection, out user))
            {
                return;
            }

            var userToSendConnection = _users.LastOrDefault(x => x.Value == userToSend).Key;
            if (userToSendConnection == null)
            {
                return;
            }

            lock (myLock)
            {
                using (StreamWriter sw = new StreamWriter("messages.txt", true))
                {
                    sw.WriteLine(user + "_" + message + "_" + userToSend);
                    sw.Flush();
                }
            }
            userToSendConnection.ReciveMessage(user, message);
        }

        public void SendFile(string fileBytes, string fileName, string userToSend)
        {
            var connection = OperationContext.Current.GetCallbackChannel<IChatClient>();
            string user;

            if (!_users.TryGetValue(connection, out user))
            {
                return;
            }

            var userToSendConnection = _users.LastOrDefault(x => x.Value == userToSend).Key;
            if (userToSendConnection == null)
            {
                return;
            }

            userToSendConnection.ReciveFile(user, fileBytes,fileName);

        }

        static string[] ReadMessages(string fileName, string sender)
        {
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
            }
            using (StreamReader sr = new StreamReader(fileName))
            {
                var filteredMessages = new List<string>();
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    string[] parts = line.Split('_');
                    if (parts[0] == sender || parts[2] == sender)
                    {
                      
                        filteredMessages.Add(line);
                    }
                }
                return filteredMessages.ToArray();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(ChatService));
            host.Open();
            Console.WriteLine("Welcome to the encription server");
            Console.ReadLine();
            host.Close();
        }
    }
}
