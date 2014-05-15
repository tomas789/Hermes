using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core
{
    interface IBroker : IEventConsumer, IDisposable
    {
        void Initialize(Portfolio portfolio);
    }
}
