using System.Threading;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Algoserver.API.Models.REST;
using Newtonsoft.Json;
using Algoserver.API.Helpers;

namespace Algoserver.API.Controllers
{
    public abstract class AlgoControllerBase : Controller
    {
        [NonAction]
        public Task<JsonResult> ToEncryptedResponse(object data, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                var res = JsonConvert.SerializeObject(data);
                var encryptedRes = EncryptionHelper.Encrypt(res);
                return Json(new EncryptedResponse
                {
                    data = encryptedRes
                });
            }, token);
        }
    }
}
