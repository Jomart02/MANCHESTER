using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtokolLibrary;


using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
using UdpClient client = new UdpClient();


//ДЛЯ ПРИЕМА СООБЩЕНИЙ
var localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5005);
// начинаем прослушивание входящих сообщений
udpSocket.Bind(localIP);
Console.WriteLine("Клиент запущен - ождание КС...");

byte[] datares = new byte[2048]; // буфер для получаемых данных
//адрес, с которого пришли данные
EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
SocketFlags SF = new SocketFlags();
// получаем данные в массив data


//Параметры клиента
int N = 0;
string SYNS_C = "";
int exept = 0;
string ADDR_RT = "00101";
string SUB_ADDR = "00010";
char WR = ' ';

string ADDR_RT_Check = "00101";
string SUB_ADDR_Check = "00001";
//Задержка работы данного абонетта
int PAUSE = 1000;

//Отправка сообщений 
string message = "$GPRMC,123519.00,A,4807.038,N,01131.000,E,,,230394,,,A*6A";
//byte[] datasend = Encoding.ASCII.GetBytes(message);
string rec = string.Empty;
EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4002);



while (true) {

    var result = await udpSocket.ReceiveFromAsync(datares, SF, remoteIp);
    
    var messageres = Encoding.ASCII.GetString(datares, 0, result.ReceivedBytes);


    ReadMessageProtokol.ReadCommandWord(messageres, out N, out SYNS_C, out ADDR_RT_Check, out SUB_ADDR_Check, out WR); 
    //Если пришло нужное нам КС - то начинаем формирование ответного слова и информационного
    if (ADDR_RT_Check == ADDR_RT && WR == '1' && SUB_ADDR_Check == SUB_ADDR) {
        Console.WriteLine(N);
        rec = SendMessageProtokol.StartSend(message, SYNS_C, ADDR_RT_Check, WR.ToString(), SUB_ADDR_Check, N);

        Console.WriteLine(rec.Length);

        byte[] datasend = Encoding.ASCII.GetBytes(rec);
        await udpSocket.SendToAsync(datasend, SF, remotePoint);

        int bytes = await udpSocket.SendToAsync(datasend, SF, remotePoint);
        Console.WriteLine($"Отправлено {bytes} байт cообщения {message}");
    }

    Thread.Sleep(PAUSE);
}