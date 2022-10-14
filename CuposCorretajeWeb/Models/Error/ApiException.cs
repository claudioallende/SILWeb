using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuposCorretajeWeb.Models.Error
{
    public class ApiException : System.Exception
    {
        public ApiException(string message)
            : base(message)
        { }
    }
}