using System.Threading.Tasks;

namespace Services
{
    public interface IService
    {
        Task<string> AsyncRead();
        Task AsyncWrite(string message);
        Task AsyncReload();
    }
}