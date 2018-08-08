using System.Threading.Tasks;

public interface IVirtualUserConnector
{
    bool Connect();
    Task<bool> SendInput(object input);
    bool Disconnect();


}

public class VirtualUserConnector : IVirtualUserConnector
{
    public bool Connect()
    {
        return true;
    }

    public bool Disconnect()
    {
        return true;
    }

    public Task<bool> SendInput(object input)
    {
        return new Task<bool>(() => true);
    }
}