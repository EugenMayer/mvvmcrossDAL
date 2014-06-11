using System;
using Cirrious.MvvmCross.ViewModels;

namespace Cirrious.CrossCore.MvvmCrossDAL
{
    public abstract class BaseModel:MvxNotifyPropertyChanged
    {
        public static string UnsafePrefix = "_do_not_use_this_unsafe_prefix!!!_";

        public BaseModel() : this(UnsafePrefix + System.Guid.NewGuid())
        {
        }

        public BaseModel(string id)
        {
            _id = id;
        }

        private string _id;

        public string Id {
            get {
                return _id;
            }
            set {
                _id = value;
                RaisePropertyChanged(() => Id);
            }
        }
    }
}

