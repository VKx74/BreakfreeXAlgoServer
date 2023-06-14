namespace Algoserver.API.Settings
{
    public class RedisSettings
    {
        public string InstanceName { get; set; }
        public int? DefaultDatabase { get; set; }
        public bool UseSSL { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int ConnectRetry { get; set; }

    }
}