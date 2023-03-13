using System;
using System.Net;
using System.Net.Sockets;
//using System.Reflection.PortableExecutable;
//using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using ProtokolLibraly;
using ProtokolLibrary;

namespace UDPMagistral {
    class ClientController {

        private static IPAddress remoteIPAddress;
        private static int remotePort;
        private const int localPort = 4002;

        //Обьявление клиента 
        private static Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static SocketFlags SF = new SocketFlags();
        // private static UdpClient sender = new UdpClient();
        private static IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4002);

        //Для получаения сообщения по протоколу конроллера 
        private static string PROTOCOL_MESSAGE = "$PPPP,000000.00,000000,0000.0000,N,00000.0000,E,000.0,N,00.0,K,000.0,*5E"; //-стандартное сообщение 
        private static ProtokolMessage MESSAGE = new ProtokolMessage();

        //[STAThread]
        static void Main(string[] args) {
            try {

                Dictionary < int , string  >  Port_ADDR = new Dictionary < int , string >();
                //Порты и соответствующие адреса абонентов 
                Port_ADDR.Add(5000, "00011");
                Port_ADDR.Add(5005, "00101");
                Port_ADDR.Add(5006, "10101");
                Port_ADDR.Add(5007, "10110");
                Port_ADDR.Add(4000, "00001");// - главный порт сервера (При его наличии не нужно знать другие порты - только адресса )

                int[] recivePort = { 5000, 5005 , 4000 };//От кого принимаем
                int[] sendPort = { 5006, 5007 };//Кому отправляем

                //Параметры контроллера 
                int N = 20;//Количество отправляемых ИС в пакете 
                string WR = "";//Контроль отправки 
                string ADDR_RT = "00010";//1 - адрес контроллера
                //Командное слово для отправления клиентам 
                string ComWord = "";
               
                // начинаем прослушивание входящих сообщений
                udpSocket.Bind(localIP);


                //"Укажите удаленный порт по стандарту 
                remotePort = 5000;
                //Укажите удаленный IP-адрес"
                remoteIPAddress = IPAddress.Parse("127.0.0.1");

                // Создаем поток для прослушивания
                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();

                // Отправка сообщений в асинхронном режиме
                int i = 0, j=0;
                while (true) {
                    

                    if (i == recivePort.Length) { i = 0; }
                    remotePort = recivePort[i];
                    ComWord = MessageProtokol.CommandWord(  Port_ADDR [recivePort[i] ]  , ADDR_RT , "1" , N ,  "001");

                    SendToSources(ComWord);
                    Thread.Sleep(100);
                    i++;

                    if (j == sendPort.Length) { j = 0; }
                    remotePort = sendPort[j];
                    ComWord = MessageProtokol.CommandWord( Port_ADDR[ sendPort[j] ], ADDR_RT, "0", N, "001");

                    SendToReceivers(ComWord);
                    Thread.Sleep(100);
                    j++;

                }
            } catch (Exception ex) {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
        /// <summary>
        /// Метод для отправки сообщений к источникам 
        /// </summary>
        /// <param name="datagram">Сформированное командное слово</param>
        private static void SendToSources(string Command_Word) {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();

            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            try {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.ASCII.GetBytes(Command_Word);

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

        /// <summary>
        ///Метод для отправки приемникам 
        /// </summary>
        /// <param name="Command_Word"></param>
        private static void SendToReceivers(string Command_Word) {
            
            string message = PROTOCOL_MESSAGE;
            UdpClient sender = new UdpClient();
            int N = 0;
            string SYNS_C = "";
            string ADDR_T = "";
            string SUB_ADDR = "";
            char WR = '0';
            
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

        /// <summary>
        /// Метод для приема ОС и ИС от абонентов в отдельном потоке 
        /// </summary>
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
                    byte[] receiveBytes = new byte[2048];
                    for (int i =0; i< receiveBytes.Length;  i++) receiveBytes[i] = 0;
                    
                    //Получение данных через сокет 
                    udpSocket.ReceiveFrom(receiveBytes, ref RemoteIpEndPoint);

                    // Преобразуем и отображаем данные
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    string NMEAmes = Receive.GetMessageConnector(returnData);

                    Console.WriteLine( Receive.GetMessageConnector(returnData) );

                    PROTOCOL_MESSAGE = MESSAGE.GetMessage(NMEAmes);
                    
                    Console.WriteLine("===" + PROTOCOL_MESSAGE);

                } catch (Exception ex) {
                    //Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
                }
            }
        }
    }
}


