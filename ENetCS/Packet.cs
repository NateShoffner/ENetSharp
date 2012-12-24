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
using System.Runtime.InteropServices;

namespace ENet
{
    public unsafe struct Packet : IDisposable
    {
        Native.ENetPacket* _packet;

        public Packet(Native.ENetPacket* packet)
        {
            _packet = packet;
        }

        internal void CheckCreated()
        {
            if (_packet == null) { throw new InvalidOperationException("No native packet."); }
        }

        public void Create(byte[] data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            Create(data, 0, data.Length);
        }

        public void Create(byte[] data, int offset, int length)
        {
            Create(data, offset, length, PacketFlags.None);
        }

        public void Create(byte[] data, int offset, int length, PacketFlags flags)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (offset < 0 || length < 0 || length > data.Length - offset) { throw new ArgumentOutOfRangeException(); }
            fixed (byte* bytes = data) { Create(bytes + offset, length, flags); }
        }

        public void Create(void* data, int length, PacketFlags flags)
        {
            if (_packet != null) { throw new InvalidOperationException("Already created."); }

            _packet = Native.enet_packet_create(data, (IntPtr)length, flags);
            if (_packet == null) { throw new ENetException(0, "Packet creation call failed."); }
        }

        public void CopyTo(byte[] array)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            CopyTo(array, 0, array.Length);
        }

        public void CopyTo(byte[] array, int offset, int length)
        {
            if (array == null) { throw new ArgumentNullException("array"); }
            if (offset < 0 || length < 0 || length > array.Length - offset) { throw new ArgumentOutOfRangeException(); }

            CheckCreated();
            if (length > Length - offset) { throw new ArgumentOutOfRangeException(); }
            if (length > 0) { Marshal.Copy((IntPtr)((byte*)Data + offset), array, offset, length); }
        }

        public byte[] GetBytes()
        {
            CheckCreated();
            byte[] array = new byte[Length]; CopyTo(array); return array;
        }

        public void Dispose()
        {
            if (_packet != null)
            {
                if (_packet->referenceCount == IntPtr.Zero) { Native.enet_packet_destroy(_packet); }
                _packet = null;
            }
        }

        public void Resize(int length)
        {
            if (length < 0) { throw new ArgumentOutOfRangeException("length"); }
            CheckCreated(); int ret = Native.enet_packet_resize(_packet, (IntPtr)length);
            if (ret < 0) { throw new ENetException(ret, "Packet resize call failed."); }
        }

        public void* Data
        {
            get { CheckCreated(); return _packet->data; }
        }

        public int Length
        {
            get
            {
                CheckCreated();
                if (_packet->dataLength.ToPointer() > (void*)int.MaxValue) { throw new ENetException(0, "Packet too long!"); }
                return (int)_packet->dataLength;
            }
        }

        public Native.ENetPacket* NativeData
        {
            get { return _packet; }
            set { _packet = value; }
        }

        public bool IsSet
        {
            get { return _packet != null; }
        }
    }
}
