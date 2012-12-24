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

namespace ENet
{
    public unsafe struct Peer
    {
        Native.ENetPeer* _peer;

        public Peer(Native.ENetPeer* peer)
        {
            _peer = peer;
        }

        void CheckCreated()
        {
            if (_peer == null) { throw new InvalidOperationException("No native peer."); }
        }

        public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
        {
            CheckCreated(); Native.enet_peer_throttle_configure(_peer, interval, acceleration, deceleration);
        }

        public void Disconnect(uint data)
        {
            CheckCreated(); Native.enet_peer_disconnect(_peer, data);
        }

        public void DisconnectLater(uint data)
        {
            CheckCreated(); Native.enet_peer_disconnect_later(_peer, data);
        }

        public void DisconnectNow(uint data)
        {
            CheckCreated(); Native.enet_peer_disconnect_now(_peer, data);
        }

        public void Ping()
        {
            CheckCreated(); Native.enet_peer_ping(_peer);
        }

        public void Reset()
        {
            CheckCreated(); Native.enet_peer_reset(_peer);
        }

        public bool Receive(out byte channelID, out Packet packet)
        {
            CheckCreated(); Native.ENetPacket* nativePacket;
            nativePacket = Native.enet_peer_receive(_peer, out channelID);
            if (nativePacket == null) { packet = new Packet(); return false; }
            packet = new Packet(nativePacket); return true;
        }

        public bool Send(byte channelID, byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            return Send(channelID, data, 0, data.Length);
        }

        public bool Send(byte channelID, byte[] data, int offset, int length)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            bool ret; using (Packet packet = new Packet())
            {
                packet.Create(data, offset, length);
                ret = Send(channelID, packet);
            }
            return ret;
        }

        public bool Send(byte channelID, Packet packet)
        {
            CheckCreated(); packet.CheckCreated();
            return Native.enet_peer_send(_peer, channelID, packet.NativeData) >= 0;
        }

        public bool IsSet
        {
            get { return _peer != null; }
        }

        public Native.ENetPeer* NativeData
        {
            get { return _peer; }
            set { _peer = value; }
        }

        public PeerState State
        {
            get { return IsSet ? _peer->state : PeerState.Uninitialized; }
        }

        public IntPtr UserData
        {
            get { CheckCreated(); return _peer->data; }
            set { CheckCreated(); _peer->data = value; }
        }
    }
}
