using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogPlatformCleanArchitecture.Application.ExceptionHandling
{
    public class EmailAlreadyConfirmedException : Exception
    {
        public EmailAlreadyConfirmedException(string message) : base(message) { }
    }
}
