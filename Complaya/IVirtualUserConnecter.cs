public interface IVirtualUserConnecter
{
    bool Connect();
    bool SendInput(object input);
    bool Disconnect();


}