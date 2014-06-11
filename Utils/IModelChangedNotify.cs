using System;

namespace Cirrious.CrossCore.MvvmCrossDAL
{
    public interface IModelChangedNotify
    {
        event ModelChangedEventHandler ModelChanged;
    }
}

