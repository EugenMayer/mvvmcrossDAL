using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cirrious.CrossCore.MvvmCrossDAL
{
    public abstract class BaseStorageService<TModel, TGetOptions, TCreateOptions,TUpdateOptions>: IStorageAccess<TModel,TGetOptions>
        where TModel:BaseModel, new()
        where TGetOptions: new()
        where TUpdateOptions:TGetOptions, new()
        where TCreateOptions:TGetOptions, new()
    {
        // the storage is 2 dimensional, since we need a namespace in case you dont load resources from a simple ressource like myressource
        // but rather from myressource/1/tickets ... so its a touple. In this case the IDs of the resources are local, not super global
        // and we would have conflicts in the storage since id:1 would map on 2 different ressources (would be overriden)
        protected Dictionary<string, Dictionary<string, TModel>> _storage = new Dictionary<string, Dictionary<string, TModel>>();

        public event ModelChangedEventHandler ModelChanged;

        protected abstract List<TModel> LoadElements(int limit, TGetOptions options);

        protected abstract TModel LoadElement(string id, TGetOptions options);

        /**
         * Must return true, if the item could be created in the backend
         * */
        protected abstract bool UpdateElement(TModel item, ref TUpdateOptions options);

        //protected abstract bool DeleteElement(TModel item, ref TDeleteOptions options);
        protected bool DeleteElement(TModel item, TGetOptions options)
        {
            // TODO: make this method abstract and let all services implement it specificaly
            return true;
        }

        /**
         *
         * Must return the ID of the newly created model or 0 if it failed
         * */
        protected abstract TModel CreateElement(TModel item, ref TCreateOptions options);

        protected void TriggerModelChanged(object newModel, ModelChangeOperation operation)
        {
            OnModelChanged(new ModelChangedEventArgs(newModel, operation));
        }

        private void OnModelChanged(ModelChangedEventArgs e)
        {
            if (ModelChanged != null) {
                ModelChanged(this, e);
            }
        }

        public virtual TModel Find(string id)
        {
            return Find(id, new TGetOptions());
        }

        public TModel Find(string id, TGetOptions options)
        {
            TModel element;
            element = LoadElement(id, options);
            return AddToStorage(element, StorageInitializeNamespace(options));
        }

        public virtual List<TModel> FindAll()
        {
            return FindAll(30, new TGetOptions());
        }

        public virtual List<TModel> FindAll(TGetOptions options)
        {
            return FindAll(0, options);
        }

        public virtual List<TModel> FindAll(int limit)
        {
            return FindAll(limit, new TGetOptions());
        }

        public List<TModel> FindAll(int limit, TGetOptions options)
        {
            var storageNamespace = StorageInitializeNamespace(options);
            foreach (var element in LoadElements(limit,options)) {
                AddToStorage(element, storageNamespace);
            }

            return StorageReturnAll(storageNamespace);
        }

        public TModel Save(TModel item)
        {
            if (IsUnsavedModel(item)) {
                TCreateOptions options = new TCreateOptions();
                // our ID is prefixed with _unsavePrefix if this is an unsaved model. Since we want to ensure the backend
                // does not create the Model using this id, we set it to null, so it gets autogenerated
                item.Id = "";
                TModel savedItem = CreateElement(item, ref options);
                if (ValidateModelId(savedItem)) { // error case
                    return AddToStorage(savedItem, StorageInitializeNamespace(options));
                }
                // else something is wrong with the model ID,
                return default(TModel);
            }
            else {
                TUpdateOptions options = new TUpdateOptions();

                if (UpdateElement(item, ref options)) {
                    if (!ValidateModelId(item)) { // invalid id in the model
                        // TODO: maybe even throw an exception here -should never happen
                        return default(TModel);
                    }

                    return AddToStorage(item, StorageInitializeNamespace(options));
                }
            }
            // If the backend was able to update it, update it to in our storage
            return default(TModel);
        }

        public bool clearStorage(string storageNamespace = "_default_")
        {
            if (_storage.ContainsKey(storageNamespace)) {
                foreach (var element in _storage[storageNamespace]) {
                    DeleteElement(element.Value, new TGetOptions());
                }
                _storage[storageNamespace].Clear();
                TriggerModelChanged(null, ModelChangeOperation.DeleteAll);
            }
            return true;
        }

        public bool IsUnsavedModel(TModel item)
        {
            if (item.Id.StartsWith(BaseModel.UnsafePrefix)) {
                return true;
            }
            // else
            return false;
        }

        public TModel GetNewModel()
        {
            var model = new TModel();
            model.Id = GetNextUnsavedKey();
            return model;
        }

        private string GetNextUnsavedKey()
        {
            var uuid = System.Guid.NewGuid();
            return BaseModel.UnsafePrefix + uuid;
        }

        /**
         *  return true if the id of the model is a real id (valid id)
         */
        private bool ValidateModelId(TModel item)
        {
            return !(item.Id == "" || item.Id.StartsWith(BaseModel.UnsafePrefix));
        }

        /**
         * Default implementation of the Namespace, which is just one bucket for all entities
         * Override this if you handle nested ressource with a touple as idintifier
         */
        protected virtual string GetNameSpace(TGetOptions options)
        {
            // the storage is 2 dimensional, since we need a namespace in case you dont load resources from a simple ressource like myressource
            // but rather from myressource/1/tickets ... so its a touple. In this case the IDs of the resources are local, not super global
            // and we would have conflicts in the storage since id:1 would map on 2 different ressources (would be overriden)

            // return a specific string to identify your "namespace", in many cases this is just concatinating all the foreign keys you have in your path   
            return "_default_";
        }

        protected virtual string GetNameSpaceFromModel(TModel item)
        {
            return "_default_";
        }

        public TModel AddToStorage(TModel item)
        {
            return AddToStorage(item, GetNameSpaceFromModel(item));
        }

        /**
         * Implement this if you handle nested resources and your model includes other models
         * here, you would iterate over all those nested ressources and and inform the respective services for the model
         * using again AddToStorage, that a model of their type has been loaded
         * 
         * Basically these deals with the problem that sometimes responses of Servces of type A include models of Type B,C,D
         * and we wont to properly inform the Service of B, C,D that we loaded such a element
         * 
         * @see CartService
         */
        protected virtual TModel AddToStorage(TModel item, string storageNamespace)
        {
            StorageInitializeNamespace(storageNamespace);
            return StorageAdd(item.Id, item, storageNamespace);
        }

        protected void DeleteFromStorage(TModel item)
        {
            string storageNamespace = GetNameSpaceFromModel(item);
            if (!_storage.ContainsKey(storageNamespace)) {
                // if the namespace does not even exists, the item cannot exist either
                return;
            }
            StorageRemove(item.Id, item, storageNamespace);
        }

        private TModel StorageAdd(string id, TModel item, string storageNamespace = "_default_")
        {
            // This is a merge
            if (_storage[storageNamespace].ContainsKey(id)) { // the item was in the storage already. Should be in 100% of all cases, since it hase been loaded before
                var properties = item.GetType().GetRuntimeProperties();
                foreach (var prop in properties) {
                    if (prop.CanWrite) {
                        // This is a more complex merge were we want to avoid calling the setters to often ( due to the events bound to them .. RaisePropertyChanged)
                        // But also we dont want to "Downgrade" fully populated models in the storage with updated, but only partially loaded models
                        // specificly this meens, if values in the model, which is about to be merged with the current instance in the storage, are null
                        // we assume that this values are not populated and not really "empty in the backend". In the case that we load a model initially
                        // the value will be null anyway, since this is the default. We just do not support the edge case where a field is first not null, and then later on is updated to null again, instead of an empty list or similar
                        // TODO: the edge-case can lead to errors and needs to be handled somehow later on. We basically currently cannot differ between a null for "empty in the backend" and null "not part of the response in the DTO"
                        if (prop.GetValue(item) != null && // this protects us for invalidating models which have been first loaded fully and then are updated by partial populated models ( often when stuff is nested, fields are skipped )
                            (prop.GetValue(_storage[storageNamespace][item.Id]) == null || // If the current value is null, skip the value comparision - we set anyway
                            !prop.GetValue(_storage[storageNamespace][item.Id]).Equals(prop.GetValue(item)))) { // only update the value if the value actually has been changed
                            prop.SetValue(_storage[storageNamespace][item.Id], prop.GetValue(item));
                        }
                    }
                }
                TriggerModelChanged(item, ModelChangeOperation.Update);
                return _storage[storageNamespace][id];
            }
            // else item was not in the storage befor, put it in there
            _storage[storageNamespace].Add(item.Id, item);
            TriggerModelChanged(item, ModelChangeOperation.Add);
            return item;
        }

        private void StorageRemove(string id, TModel item, string storageNamespace = "_default_")
        {
            if (_storage[storageNamespace].ContainsKey(id)) { // the item was in the storage already. Should be in 100% of all cases, since it hase been loaded before
                _storage[storageNamespace].Remove(id);
            }
            TriggerModelChanged(item, ModelChangeOperation.Delete);
        }

        private List<TModel> StorageReturnAll(string storageNamespace = "_default_")
        {
            return _storage[storageNamespace].Values.ToList();
        }

        private string StorageInitializeNamespace(TGetOptions options)
        {
            return StorageInitializeNamespace(GetNameSpace(options));
        }

        private string StorageInitializeNamespace(string storageNamespace)
        {
            if (!_storage.ContainsKey(storageNamespace)) {
                _storage.Add(storageNamespace, new Dictionary<string,TModel>());  
            }

            return storageNamespace;
        }
    }
}
