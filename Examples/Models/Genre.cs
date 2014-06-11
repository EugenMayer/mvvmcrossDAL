using System;

namespace Cirrious.CrossCore.MvvmCrossDAL.Examples
{
    public class Genre : BaseModel
    {
        private string _foo;

        public Genre() : base()
        {
        }

        public Genre(GenreDto args) : base(args.Id)
        {
            _foo = args.Foo;
        }


    }
}

