#region

using System;
using System.Threading;
using ENet;

#endregion

namespace ENetDemo
{
    // The general arrangement is as follows:
    //   Create a Host. For a client, the peer count is 1. If it's a client, you don't
    //    want someone to be able to connect to it, so the Address? parameter is null.
    //   Connect if you're a client.
    //   Service() checks for new notifications to read (Connect, Disconnect, Receive).
    //    It returns 0 if there's nothing to read, 1 if there is, and -1 if an error
    //    has occured.
    //   CheckEvents() pulls a notification from the queue. It returns 0 if there are
    //    no more, 1 if it got one, and -1 if an error occured.
    // Send and receive packets. One thing to keep in mind is, after reading a packet
    //  from a Receive notification, you need to Dispose of it. Otherwise you'll get
    //  a memory leak. Also, if on Peer.Send you give it a Packet instead of a byte
    //  array, be sure to Dispose of the packet afterwards. Host.Broadcast does
    //  disposal automatically.
    //
    // Also, the ENet.dll included with this download is 32-bit. So, if you use it instead
    //  of a version you compile yourself, make sure to set Platform to x86 in Build.
    internal class Program
    {
        private static void Server()
        {
            using (var host = new Host())
            {
                host.Create(5000, 1);
                var peer = new Peer();

                while (host.Service(1) >= 0)
                {
                    Event @event;

                    while (host.CheckEvents(out @event) > 0)
                    {
                        //Console.WriteLine("Server: " + @event.Type.ToString());

                        switch (@event.Type)
                        {
                            case EventType.Connect:
                                peer = @event.Peer;
                                for (var i = 0; i < 200; i++)
                                {
                                    peer.Send((byte) i, new byte[] {0, 0});
                                }
                                break;

                            case EventType.Receive:
                                var data = @event.Packet.GetBytes();
                                var value = BitConverter.ToUInt16(data, 0);
                                if (value%1000 == 1)
                                {
                                    Console.WriteLine("  Server: Ch={0} Recv={1}", @event.ChannelID, value);
                                }
                                value++;
                                peer.Send(@event.ChannelID, BitConverter.GetBytes(value));
                                @event.Packet.Dispose();
                                break;
                        }
                    }
                }
            }
        }

        private static void Client()
        {
            using (var host = new Host())
            {
                host.Create(null, 1);

                var address = new Address();
                address.SetHost("127.0.0.1");
                address.Port = 5000;

                var peer = host.Connect(address, 200, 1234);
                while (host.Service(1) >= 0)
                {
                    Event @event;
                    while (host.CheckEvents(out @event) > 0)
                    {
                        //Console.WriteLine("Client: " + @event.Type.ToString());

                        switch (@event.Type)
                        {
                            case EventType.Receive:
                                var data = @event.Packet.GetBytes();
                                var value = BitConverter.ToUInt16(data, 0);
                                if (value%1000 == 0)
                                {
                                    Console.WriteLine("  Client: Ch={0} Recv={1}", @event.ChannelID, value);
                                }
                                value++;
                                peer.Send(@event.ChannelID, BitConverter.GetBytes(value));
                                @event.Packet.Dispose();
                                break;
                        }
                    }
                }
            }
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("ENet demo");
            Library.Initialize();

            var server = new Thread(Server);
            server.Start();
            Thread.Sleep(250);
            var client = new Thread(Client);
            client.Start();

            server.Join();
            client.Join();

            Library.Deinitialize();
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
    }
}