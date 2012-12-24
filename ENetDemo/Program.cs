#region License
/*
ENet for C#
Copyright (c) 2011 James F. Bellinger <jfb@zer7.com>

Permission to use, copy, modify, and/or distribute this software for any
purpose with or without fee is hereby granted, provided that the above
copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/
#endregion

using System;
using System.Threading;

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
    class Program
    {
        static void Server()
        {
            using (ENet.Host host = new ENet.Host())
            {
                host.Create(5000, 1);
                ENet.Peer peer = new ENet.Peer();

                while (host.Service(1) >= 0)
                {
                    ENet.Event @event;

                    while (host.CheckEvents(out @event) > 0)
                    {
                        //Console.WriteLine("Server: " + @event.Type.ToString());

                        switch (@event.Type)
                        {
                            case ENet.EventType.Connect:
                                peer = @event.Peer;
                                for (int i = 0; i < 200; i++) { peer.Send((byte)i, new byte[] { 0, 0 }); }
                                break;

                            case ENet.EventType.Receive:
                                byte[] data = @event.Packet.GetBytes();
                                ushort value = BitConverter.ToUInt16(data, 0);
                                if (value % 1000 == 1) { Console.WriteLine("  Server: Ch={0} Recv={1}", @event.ChannelID, value); }
                                value++; peer.Send(@event.ChannelID, BitConverter.GetBytes(value));
                                @event.Packet.Dispose();
                                break;
                        }
                    }
                }
            }
        }

        static void Client()
        {
            using (ENet.Host host = new ENet.Host())
            {
                host.Create(null, 1);

                ENet.Address address = new ENet.Address();
                address.SetHost("127.0.0.1"); address.Port = 5000;

                ENet.Peer peer = host.Connect(address, 200, 1234);
                while (host.Service(1) >= 0)
                {
                    ENet.Event @event;
                    while (host.CheckEvents(out @event) > 0)
                    {
                        //Console.WriteLine("Client: " + @event.Type.ToString());

                        switch (@event.Type)
                        {
                            case ENet.EventType.Receive:
                                byte[] data = @event.Packet.GetBytes();
                                ushort value = BitConverter.ToUInt16(data, 0);
                                if (value % 1000 == 0) { Console.WriteLine("  Client: Ch={0} Recv={1}", @event.ChannelID, value); }
                                value++; peer.Send(@event.ChannelID, BitConverter.GetBytes(value));
                                @event.Packet.Dispose();
                                break;
                        }
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("ENet demo");
            ENet.Library.Initialize();

            Thread server = new Thread(Server); server.Start();
            Thread.Sleep(250);
            Thread client = new Thread(Client); client.Start();

            server.Join();
            client.Join();

            ENet.Library.Deinitialize();
            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }
    }
}
