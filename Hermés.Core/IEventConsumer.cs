﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermés.Core
{
    public interface IEventConsumer
    {
        void DispatchEvent(Event e);
    }
}
