using System;

namespace Cirrious.CrossCore.MvvmCrossDAL.Examples
{
    // This ca be a ServiceStack or Database implementation of an Parameter Object
    public class GetGenreRequest
    {
        public string Id {
            get;
            set;
        }

        public int Limit {
            get;
            set;
        }

        public int Depth {
            get;
            set;
        }
    }

    public class GenreDto
    {
        public string Id;
        public string Foo;

        public GenreDto()
        {
        }

        public static Genre fromJson(object data)
        {
            // this would parse data, which could be a jsonObject to your real model
            return new Genre();
        }
    }
}

