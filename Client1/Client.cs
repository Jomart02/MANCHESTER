﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtokolLibrary;


using var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
using UdpClient client = new UdpClient();


//ДЛЯ ПРИЕМА СООБЩЕНИЙ
var localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
// начинаем прослушивание входящих сообщений
udpSocket.Bind(localIP);
Console.WriteLine("Клиент запущен - ождание КС...");

byte[] datares = new byte[512]; // буфер для получаемых данных
//адрес, с которого пришли данные
EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
SocketFlags SF = new SocketFlags();
// получаем данные в массив data


//Параметры клиента 
int N = 0;
string SYNS_C = "";
int exept = 0;
string ADDR_RT = "00011";
string SUB_ADDR = "00010";
char WR = ' ';

string ADDR_RT_Check = "00010";
string SUB_ADDR_Check = "00001";


//Отправка сообщений 
string message = "$$GLGSV,3,1,09,741,73,25,326,,0*";
//byte[] datasend = Encoding.ASCII.GetBytes(message);
string rec = string.Empty;
EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5555);


while (true) {

    var result = await udpSocket.ReceiveFromAsync(datares, SF, remoteIp);
    var messageres = Encoding.ASCII.GetString(datares, 0, result.ReceivedBytes);

    
    ReadMessageProtokol.ReadCommandWord(messageres, out N, out SYNS_C, out ADDR_RT_Check, out SUB_ADDR_Check, out WR);
    Console.WriteLine(ADDR_RT_Check);
    if (ADDR_RT_Check == ADDR_RT && WR == '1' && SUB_ADDR_Check == SUB_ADDR) {
        
        rec = SendMessageProtokol.StartSend(message, SYNS_C, SUB_ADDR_Check, WR.ToString() , ADDR_RT_Check, N);

        byte[] datasend = Encoding.ASCII.GetBytes(rec);
        
        int bytes = await udpSocket.SendToAsync(datasend, SF, remotePoint);
        Console.WriteLine($"Отправлено {bytes} байт cообщения {message}");
    }

    Thread.Sleep(3000);
}