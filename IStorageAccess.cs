using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cirrious.CrossCore.MvvmCrossDAL
{
    public interface IStorageAccess<TModel,TGetOptions>
    {
        event ModelChangedEventHandler ModelChanged;

        TModel Find(string id);

        TModel Find(string id, TGetOptions options);


        List<TModel> FindAll();

        List<TModel> FindAll(int limit);

        List<TModel> FindAll(TGetOptions options);

        List<TModel> FindAll(int limit, TGetOptions options);


        TModel Save(TModel data, bool forceOverride = false);

        TModel GetNewModel();

        bool clearStorage(string storageNamespace = "_default_");

        TModel AddToStorage(TModel item, bool forceOverride = false);
    }
}

