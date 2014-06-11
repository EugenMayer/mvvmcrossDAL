using System;
using System.Linq;
using Cirrious.CrossCore.MvvmCrossDAL;
using System.Collections.Generic;

namespace Cirrious.CrossCore.MvvmCrossDAL.Examples
{
    public class GenreService : BaseStorageService<Genre, GetGenreRequest, GetGenreRequest,GetGenreRequest>
    {
        protected readonly ServiceClientBase _restClient;

        public GenreService(RestWrapper restWrapper)
        {
            _restClient = restWrapper.RestClient;
        }

        protected override System.Collections.Generic.List<Genre> LoadElements(int limit, GetGenreRequest options)
        {
            if (limit > 0 && options.Limit <= 0) {
                options.Limit = limit;
            }
            options.Depth = 1;
            var oneResponse = _restClient.Get(options);

            List<Genre> result = new List<Genre>();
            // foreach oneResponse ..
            result.Add(GenreDto.fromJson(oneResponse));
            return result;
        }

        protected override Genre LoadElement(string id, GetGenreRequest options)
        {
            options.Id = id;
            var oneResponse = _restClient.Get(options);
            return GenreDto.fromJson(oneResponse);
        }

        protected override bool UpdateElement(Genre item, ref GetGenreRequest options)
        {
            throw new NotImplementedException();
        }

        protected override Genre CreateElement(Genre item, ref GetGenreRequest options)
        {
            throw new NotImplementedException();
        }
    }
}