using System;
using System.Text;
using ClientLourd.Services.SocketService;
using ClientLourd.Utilities.Enums;
using MessagePack;
using MessagePack.Resolvers;

namespace ClientLourd.Models.NonBindable

{
    public class Tlv
    {
        public Tlv(SocketMessageTypes type)
        {
            Type = (byte) ((int) type);
        }

        /// <summary>
        /// Serialize the message using message pack
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public Tlv(SocketMessageTypes type, dynamic message)
        {
            Type = (byte) ((int) type);
            Value = MessagePackSerializer.Serialize(message, ContractlessStandardResolver.Options);
        }

        /// <summary>
        /// Convert the message in byte 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public Tlv(SocketMessageTypes type, string message)
        {
            Type = (byte) ((int) type);
            Value = Encoding.UTF8.GetBytes(message);
        }

        public Tlv(SocketMessageTypes type, byte[] bytes)
        {
            Type = (byte) (int) type;
            Value = bytes;
        }

        public Tlv(SocketMessageTypes type, Guid guid)
        {
            Type = (byte) ((int) type);
            byte[] bytes = guid.ToByteArray();
            // We need to change the format since the server
            // use uuid
            Array.Reverse(bytes, 0, 4);
            Array.Reverse(bytes, 4, 2);
            Array.Reverse(bytes, 6, 2);
            Value = bytes;
        }

        public byte[] GetBytes()
        {
            byte[] bytes = new Byte[1 + 2 + (Value != null ? Value.Length : 0)];
            bytes[0] = Type;

            byte[] lengthInBytes = BitConverter.GetBytes(Length);

            // Convert to big-endian
            Array.Reverse(lengthInBytes);

            lengthInBytes.CopyTo(bytes, 1);
            Value?.CopyTo(bytes, 3);

            return bytes;
        }

        public byte Type { get; private set; }

        public UInt16 Length
        {
            get
            {
                if (Value != null)
                {
                    return (UInt16) Value.Length;
                }

                return 0;
            }
        }

        public byte[] Value { get; private set; }
    }
}