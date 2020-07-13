using Twelvedata.API.Services.Instruments;

namespace Twelvedata.API.Services.Realtime
{
    public class Subscription
    {
        public UserWebsocket Client { get; }
        public Instrument Instrument { get; }

        public Subscription(UserWebsocket client, Instrument instrument)
        {
            Client = client;
            Instrument = instrument;
        }

        public override int GetHashCode()
        {
            return new { Client, Instrument }.GetHashCode() * 17;
        }

        public override bool Equals(object other) => Equals(other as Subscription);

        public bool Equals(Subscription other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Client.Equals(other.Client) && Instrument.Equals(other.Instrument);
        }
    }
}
