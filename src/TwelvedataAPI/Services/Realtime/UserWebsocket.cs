using System;
using System.Threading.Tasks;
using Algoserver.API.Models.WebSocket;

namespace Algoserver.API.Services.Realtime
{
    public delegate Task SendToSocketCallback(BaseMessage message);

    /// <inheritdoc />
    public class UserWebsocket : IEquatable<UserWebsocket>
    {
        public string UserId { get; set; }
        public string SocketId { get; set; }

        public SendToSocketCallback Callback { get; set; }

        public async Task SendMessage(BaseMessage message)
        {
            if (Callback != null) await Callback.Invoke(message);
        }

        public override int GetHashCode()
        {
            return SocketId.GetHashCode() * 17;
        }

        public override bool Equals(object other) => Equals(other as UserWebsocket);

        public bool Equals(UserWebsocket other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return SocketId == other.SocketId;
        }
    }
}