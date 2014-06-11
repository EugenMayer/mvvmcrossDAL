using System;

namespace Cirrious.CrossCore.MvvmCrossDAL
{
    public delegate void ModelChangedEventHandler(object sender,ModelChangedEventArgs args);
    public enum ModelChangeOperation
    {
        Add,
        New,
        Delete
    }

    public class ModelChangedEventArgs: EventArgs
    {
        private  object _newModel;
        private ModelChangeOperation _operation;

        public ModelChangedEventArgs(object newModel, ModelChangeOperation operation)
        {
            _newModel = newModel;
            _operation = operation;
        }

        public object NewModel {
            get {
                return _newModel;
            }

        }

        public ModelChangeOperation Operation {
            get {
                return _operation;
            }
        }
    }
}

