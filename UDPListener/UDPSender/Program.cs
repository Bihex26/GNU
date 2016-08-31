using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDPSender
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var done = false;
                Socket sendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPAddress desAddress = IPAddress.Parse("192.168.2.255");
                IPEndPoint sendingEndPoint = new IPEndPoint(desAddress, 5000);

                Console.WriteLine("Enter text to broadcast via UDP.");
                Console.WriteLine("Enter a blank line to exit the program.");

                while (!done)
                {
                    Console.WriteLine("Enter text to send, blank line to quit");
                    string textToSend = Console.ReadLine();
                    if (textToSend != null && textToSend.Length == 0)
                    {
                        done = true;
                    }
                    else

                    {
                        // the socket object must have an array of bytes to send.

                        // this loads the string entered by the user into an array of bytes.

                        byte[] sendBuffer = Encoding.ASCII.GetBytes(textToSend);

                        // Remind the user of where this is going.

                        Console.WriteLine("sending to address: {0} port: {1}",
                        sendingEndPoint.Address,
                        sendingEndPoint.Port);
                        try
                        {
                            sendingSocket.SendTo(sendBuffer, sendingEndPoint);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(" Exception {0}", ex.Message);
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
