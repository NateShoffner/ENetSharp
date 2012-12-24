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

namespace ENet
{
    public unsafe struct Event
    {
        internal Native.ENetEvent _event;

        public Event(Native.ENetEvent @event)
        {
            _event = @event;
        }

        public byte ChannelID
        {
            get { return _event.channelID; }
        }

        public uint Data
        {
            get { return _event.data; }
        }

        public Native.ENetEvent NativeData
        {
            get { return _event; }
            set { _event = value; }
        }

        public Packet Packet
        {
            get { return new Packet(_event.packet); }
        }

        public Peer Peer
        {
            get { return new Peer(_event.peer); }
        }

        public EventType Type
        {
            get { return _event.type; }
        }
    }
}
