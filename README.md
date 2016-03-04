ENetSharp
====================

C# [ENet](http://enet.bespin.org/) Wrapper

[![Build status](https://ci.appveyor.com/api/projects/status/r1exgo7iccj8p267?svg=true)](https://ci.appveyor.com/project/NateShoffner/enetsharp)


Usage
------------

#### ENet Server
```csharp
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
```

#### ENet Client
```csharp
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
```