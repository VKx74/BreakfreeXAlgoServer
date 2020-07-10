using RestEase;
using System.Threading.Tasks;

namespace BFT.AlgoService.Tests
{
    public interface IAlgoServiceAPI
    {
        [Get("api/v1/Hello")]
        Task<Response<TestMessage>> SayHelloAsync();
    }
}
