using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbContextTests.Services
{
    interface ITransactionFactory 
    {
        ITransaction GetTransaction();
    }
}
