using Common.Logic.Helpers;
using Common.Logic.Interfaces;

namespace Common.Logic.Services
{
    public class BaseService : BaseHelper, IService
    {
        public string UserId { get; set; }
    }

    public class BaseService<T> : BaseResult<T>
    {
    }
}
