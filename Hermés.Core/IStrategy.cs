﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Hermés.Core;

namespace Hermés.Core
{
    public interface IStrategy : IEventConsumer, IDisposable
    {
        void Initialize(Kernel kernel);
    }
}
