﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Services
{
    interface ITransaction : IDisposable
    {
        void Commit();
    }
}
