using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace BFT.AlgoService.Tests
{
    public class AlgoControllerTests : BaseTests
    {
        private readonly TestFixture _testFixture;

        public AlgoControllerTests(TestFixture testFixture, ITestOutputHelper helper) : base(helper)
        {
            _testFixture = testFixture;
        }
        
        [Fact]
        public async Task BehaviourTest()
        {
            throw new NotImplementedException();
        }
    }
}
