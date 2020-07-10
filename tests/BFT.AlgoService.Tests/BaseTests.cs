using Common.API.Interfaces;
using Newtonsoft.Json;
using RestEase;
using System;
using Xunit;
using Xunit.Abstractions;

namespace BFT.AlgoService.Tests
{
    public class BaseTests : IClassFixture<TestFixture>
    {
        protected readonly ITestOutputHelper Helper;

        public BaseTests(ITestOutputHelper helper)
        {
            Helper = helper;
        }

        public bool IsSuccessResponse<T>(Response<T> response)
        {
            return response.ResponseMessage.IsSuccessStatusCode;
        }

        public void CheckSuccessResponse<T>(Response<T> response)
        {
            Assert.NotNull(response);

            if (!response.ResponseMessage.IsSuccessStatusCode)
            {
                Helper.WriteLine(response.StringContent);
                Console.WriteLine(response.StringContent);
            }
            Assert.True(response.ResponseMessage.IsSuccessStatusCode);
        }

        public void CheckFailResponse<T>(Response<T> response)
        {
            Assert.False(response.ResponseMessage.IsSuccessStatusCode);

            var content = JsonConvert.DeserializeObject<IMessage>(response.StringContent);

            Assert.NotNull(content);
            Assert.NotNull(content.Error);
            Assert.Null(content.Data);
        }

        public T ExtractObject<T>(Response<T> response) where T : class
        {
            CheckSuccessResponse(response);
            var content = response.GetContent();
            Assert.NotNull(content);
            return content;
        }

        public bool TryExtractObject<T, RT>(Response<RT> response, out T data) where T : class
        {
            try
            {
                data = JsonConvert.DeserializeObject<T>(response.StringContent);
                return true;
            }
            catch
            {
                data = null;
                return false;
            }
        }

        public T GetData<T>(TestMessage message)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(message.Data));
        }
    }
}
