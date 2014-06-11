using System;

namespace Cirrious.CrossCore.MvvmCrossDAL.Examples
{
    public class ServiceClientBase
    {
        public object Get(object Options)
        {
            return new object();
            // e.g. some rest kind of call
        }
    }

    public class RestWrapper
    {
        public ServiceClientBase RestClient;

        public RestWrapper()
        {
        }
    }
}

