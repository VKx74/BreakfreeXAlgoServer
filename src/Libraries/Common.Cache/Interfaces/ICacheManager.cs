using System.Threading.Tasks;

namespace Common.Cache.Interfaces
{
    public interface ICacheManager
    {
        Task RemoveAllHash();
    }
}