using System;

namespace SystemClockSetterNTP.Services
{
    public interface IInternetService
    {
        void CheckInternetConnectionPerdiodically();
        bool IsInternetConnectionAvailable();

        event EventHandler InternetConnectionAvailable;
    }
}
