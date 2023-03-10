using System;
using System.Net;
using System.Net.Sockets;
//using System.Reflection.PortableExecutable;
//using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

using ProtokolLibrary;

namespace UDPMagistral {
    class ClientController {

        private static IPAddress remoteIPAddress;
        private static int remotePort;
        private  const int localPort = 4002;

        //Обьявление клиента 
        private static Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static SocketFlags SF = new SocketFlags();
        // private static UdpClient sender = new UdpClient();
        private static IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4002);

        //[STAThread]
        static void Main(string[] args) {
            try {

                Dictionary < int , string  >  Port_ADDR = new Dictionary < int , string >();

                Port_ADDR.Add(5000, "00011");
                Port_ADDR.Add(5005, "00101");
                Port_ADDR.Add(5006, "10101");
                Port_ADDR.Add(5007, "10110");
                Port_ADDR.Add(4000, "00001");

                int[] recivePort = { 5000, 5005 , 4000 };
                /*int[] sendPort =   { 5006, 5007 };*/
                //int[] recivePort = { 4000 };
                int[] sendPort = { 5006, 5007 };

                //Параметры контроллера 
                int N = 50;
                
                int exept = 0;
                string WR = "";
                string ADDR_RT = "00010";//1

                string ComWord = "";
                // Получаем данные, необходимые для соединения
                //"Укажите локальный порт");

                //var localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4002);
                // начинаем прослушивание входящих сообщений
                udpSocket.Bind(localIP);


                //"Укажите удаленный порт по стандарту 
                remotePort = 5000;
                

                //Укажите удаленный IP-адрес"
                remoteIPAddress = IPAddress.Parse("127.0.0.1");

               /* //клиент для проверки =================
                UdpClient receivingcheck = new UdpClient(localPort);
                IPEndPoint RemoteIpEndPoint = null;
                //=====================================*/

                // Создаем поток для прослушивания
                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();

                //"00100001101101111101"

                int i = 0, j=0;
                while (true) {
                    

                    if (i == recivePort.Length) { i = 0; }
                    remotePort = recivePort[i];
                    ComWord = MessageProtokol.CommandWord(  Port_ADDR [recivePort[i] ]  , ADDR_RT , "1" , N ,  "001");
                    
                    Send(ComWord);
                    Thread.Sleep(100);
                    i++;

                    if (j == sendPort.Length) { j = 0; }
                    remotePort = sendPort[j];
                    ComWord = MessageProtokol.CommandWord( Port_ADDR[ sendPort[j] ], ADDR_RT, "0", N, "001");
                    
                    SendA(ComWord);

                   // j++;
                }
            } catch (Exception ex) {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        private static void Send(string datagram) {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();

            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            try {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.ASCII.GetBytes(datagram);

                // Отправляем данные
                //sender.Send(bytes, bytes.Length, endPoint);
                udpSocket.SendTo(bytes, endPoint);
            } catch (ArgumentOutOfRangeException ex) {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            } catch (Exception ex) {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            } 
           /* finally {
                // Закрыть соединение
                sender.Close();
            }*/
        }
        
        private static void SendA(string Command_Word) {
            // Создаем UdpClient
            
            string message = "$GNGLL,03740.69200,E,102030.000,A,A*1A";
            UdpClient sender = new UdpClient();
            int N = 40;
            string SYNS_C = "";
            string ADDR_T = "";
            string SUB_ADDR = "";
            char WR = '0';
            int exept = 0;

            ReadMessageProtokol.ReadCommandWord(Command_Word, out N, out SYNS_C,out ADDR_T,out SUB_ADDR,out WR);
            
            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);
            string rec = "";
            try {
                // Преобразуем данные в массив байтов
                // byte[] bytes = Encoding.ASCII.GetBytes(datagram);
                rec = SendMessageProtokol.StartSend( message, SYNS_C, ADDR_T, WR.ToString(), SUB_ADDR, N);//Неправильно сделано 
                
                // Отправляем данные
                byte[] bytes = Encoding.ASCII.GetBytes(rec);
                udpSocket.SendTo(bytes, endPoint);
                Thread.Sleep(200);
            } catch (Exception ex) {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            /*finally {
                // Закрыть соединение
                Thread.Sleep(200);
                sender.Close();
            }*/
        }


        public static void Receiver() {

            // Создаем UdpClient для чтения входящих данных
            //UdpClient receivingUdpClient = new UdpClient(localPort);

            //IPEndPoint iep = new IPEndPoint(IPAddress.Any, 0);
            EndPoint RemoteIpEndPoint = (EndPoint)localIP;


            //Для проверки ответного слова
            string A = "0" , B = "0", C = "0", X = "0", D = "0", E = "0", F = "0", G = "0", H = "0";
            string ResponseWord = "";
            

            Console.WriteLine("\n-----------Получение сообщений-----------");

            while (true) {
                // Ожидание дейтаграммы
                //byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
                try {
                    byte[] receiveBytes = new byte[1024];
                    for (int i =0; i< receiveBytes.Length;  i++) receiveBytes[i] = 0;
                    
                    //Получение данных через сокет 
                    udpSocket.ReceiveFrom(receiveBytes, ref RemoteIpEndPoint);

                    // Преобразуем и отображаем данные
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    string NMEAmes = "";

                    NMEAmes += ReceiveConnector.GetMessage(returnData);

                } catch (Exception ex) {
                    //Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                }
            }
        }
    }
}


