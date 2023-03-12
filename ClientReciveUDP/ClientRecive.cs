using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtokolLibrary;


using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
using UdpClient client = new UdpClient();


//ДЛЯ ПРИЕМА СООБЩЕНИЙ
var localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5006);
// начинаем прослушивание входящих сообщений
udpSocket.Bind(localIP);
Console.WriteLine("Клиент запущен - ождание КС...");

byte[] datares = new byte[1024]; // буфер для получаемых данных
//адрес, с которого пришли данные
EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
SocketFlags SF = new SocketFlags();
// получаем данные в массив data


//Переменные для работы по манчестерскому коду 
int N = 0;
string SYNS_C = "";
int exept = 0;
char WR = ' ';
string ADDR_RT = "10101";//21
string SUB_ADDR = "00010";
//Для проверки
string ADDR_RT_Check = "10111";
string SUB_ADDR_Check = "00001";
//Задержка работы данного абонетта
int PAUSE = 500;

string ResponseWord = "";

//Отправка сообщений 
string rec = string.Empty;
EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4002);



while (true) {
    

    //Асинхронный прием сообщений 
    var result = await udpSocket.ReceiveFromAsync(datares, SF, remoteIp);
    var messageres = Encoding.ASCII.GetString(datares, 0, result.ReceivedBytes);

    //выделяем командное слово из принятого сообщения
    var COM_WORD = messageres.Substring(0, 20);
    //Проверка командного слова 
    ReadMessageProtokol.ReadCommandWord(COM_WORD, out N, out SYNS_C, out ADDR_RT_Check, out SUB_ADDR_Check, out WR);
    
    //Если пришло нужное нам КС - то начинаем формирование ответного слова и чтения сообщения 
    if (ADDR_RT_Check == ADDR_RT && WR == '0' && SUB_ADDR_Check == SUB_ADDR) {
        
        Console.WriteLine(ReadMessageProtokol.ReadInformationWord(messageres,out ResponseWord));
        
        rec = SendMessageProtokol.StartSend(messageres, SYNS_C, SUB_ADDR, "2", ADDR_RT, N);
        
        
        byte[] datasend = Encoding.ASCII.GetBytes(rec);

        int bytes = await udpSocket.SendToAsync(datasend, SF, remotePoint);

        //Console.WriteLine($"Отправлено {bytes} байт cообщения {message}");
        Console.WriteLine("Отправлено ответное слово ");
    }

    //Задержка работы клиента 
    Thread.Sleep(PAUSE);
}