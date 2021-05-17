using appointment_checker.models;

namespace appointment_checker.services
{
    public interface INotifier
    {
        void Notify(Status status, string body);
    }
}