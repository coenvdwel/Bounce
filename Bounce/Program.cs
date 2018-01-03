using System;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Collections.Generic;

namespace Bounce
{
  class Program
  {
    static Socket sokkie = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

    static void Main(string[] args)
    {
      Console.Write("Initializing... ");

      sokkie.Bind(new IPEndPoint(IPAddress.Any, 8080));
      sokkie.Listen(128);
      sokkie.BeginAccept(null, 0, OnAccept, null);

      Console.WriteLine("Ready");
      Console.WriteLine("Type + <enter> to broadcast.");
      Console.WriteLine();

      string a;
      do
      {
        Broadcast(a = Console.ReadLine());
      } while (!String.IsNullOrEmpty(a));
    }

    private static void Broadcast(string s)
    {
      // TODO
      // Probably iterate some list of clients, to send message s using Socket.Send

      Console.WriteLine("- Sent broadcast '" + s + "' to 123 clients.");
    }

    private static void OnMessage(IAsyncResult result)
    {
      // TODO
      // Make sure this method is properly called from the client
      // And then it's said to be masked too, so I need to decypher it in order to understand the message

      Console.WriteLine("- Received message 123 from client 456.");
    }

    static SHA1 sha1 = SHA1CryptoServiceProvider.Create();
    private static void OnAccept(IAsyncResult result)
    {
      try
      {
        Console.Write("- New connection, validating... ");

        byte[] buffer = new byte[1024];
        var client = sokkie.EndAccept(result);
        var i = client.Receive(buffer);
        var headerResponse = (System.Text.Encoding.UTF8.GetString(buffer)).Substring(0, i);

        var key = Convert.ToBase64String(sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(headerResponse.Replace("ey:", "`")
                  .Split('`')[1]
                  .Replace("\r", "").Split('\n')[0]
                  .Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));

        var response = "HTTP/1.1 101 Switching Protocols" + "\r\n"
             + "Upgrade: websocket" + "\r\n"
             + "Connection: Upgrade" + "\r\n"
             + "Sec-WebSocket-Accept: " + key + "\r\n" + "\r\n";

        client.Send(System.Text.Encoding.UTF8.GetBytes(response));
        Console.WriteLine("Connected!");

        // TODO
        // Now, I'm connected to this new client - i probably have to store his connection somewhere so I can broadcast to him later
        // Also, I need to listen to this client, in case he's sending me messages... But I can't, because the port is in use?
      }
      catch (Exception ex)
      {
        throw ex;
      }
      finally
      {
        if (sokkie != null && sokkie.IsBound)
        {
          sokkie.BeginAccept(null, 0, OnAccept, null);
        }
      }
    }
  }
}