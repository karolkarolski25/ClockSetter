using System;
using System.Collections.Generic;
using System.Text;

namespace SystemClockSetterNTP.Services
{
    public interface IInternetService
    {
        bool IsInternetConnectionAvailable();
    }
}
