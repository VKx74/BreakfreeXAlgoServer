using BFT.AlgoService.API;
using Microsoft.AspNetCore.TestHost;
using RestEase;

namespace BFT.AlgoService.Tests
{
    public class TestFixture
    {
        public IAlgoServiceAPI Client;

        public TestFixture()
        {
            var host = Program.BuildWebHost(new string[0]);

            var server = new TestServer(host);

            Client = new RestClient(server.CreateClient()).For<IAlgoServiceAPI>();
        }
        
    }
}
