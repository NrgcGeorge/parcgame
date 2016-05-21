using PlaneCrash;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Wrapper;

namespace PlaneCrush
{
    class HandleClient
    {
        private TcpClient clientSocket;
        private string clientName;

        public HandleClient(TcpClient clientSocket, string clientName)
        {
            this.clientSocket = clientSocket;
            this.clientName = clientName;

            Thread ctThread = new Thread(listenToClient);
            ctThread.Start();
        }

        private void listenToClient()
        {
            while (true) {
                NetworkStream networkStream = clientSocket.GetStream();
                Byte[] bytesFrom = new byte[clientSocket.Available];
                networkStream.Read(bytesFrom, 0, clientSocket.Available);
            
                if (bytesFrom.Length > 0) {
                    MessageWrapper message = ByteArrayToObject(bytesFrom);

                    if (Server.readyPlayers < 2)
                    {
                        if (message.PlanesReady)
                        {
                            Server.readyPlayers++;
                            msg("Client - " + clientName + ": READY");
                        }
                    }
                    else if (message.Phase != MessageWrapper.Phases.ACKNOWLEDGE)
                    {
                        try
                        {
                            msg("Phase: " + message.Phase);
                            Server.broadcastMsg(clientName, bytesFrom);
                        }
                        catch (Exception) { }
                    }

                    if (message.Phase == MessageWrapper.Phases.ACKNOWLEDGE && Server.readyPlayers == 2)
                    {
                        Server.sendActivePlayerMsg();
                    }
                }
            }
        }

        private void msg(String ms)
        {
            ms.Trim();
            Console.WriteLine(">> " + ms);
        }

        private MessageWrapper ByteArrayToObject(byte[] arrBytes)
        {
            MessageWrapper ReturnValue;
            using (var _MemoryStream = new MemoryStream(arrBytes))
            {
                IFormatter _BinaryFormatter = new BinaryFormatter();
                ReturnValue = (MessageWrapper)_BinaryFormatter.Deserialize(_MemoryStream);
            }
            return ReturnValue;
        }

        public byte[] ObjectToByteArray(Message obj)
        {
            byte[] bytes;
            using (var _MemoryStream = new MemoryStream())
            {
                IFormatter _BinaryFormatter = new BinaryFormatter();
                _BinaryFormatter.Serialize(_MemoryStream, obj);
                bytes = _MemoryStream.ToArray();
            }
            return bytes;
        }
    }
}
