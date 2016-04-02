using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                String dataFromClient;

                networkStream.Read(bytesFrom, 0, clientSocket.Available);
                dataFromClient = Encoding.ASCII.GetString(bytesFrom);

                msg("Client - " + clientName + ": " + dataFromClient);
                Server.broadcastMsg(clientName, dataFromClient);
            }
        }

        private void msg(String ms)
        {
            ms.Trim();
            Console.WriteLine(">> " + ms);
        }
    }
}
