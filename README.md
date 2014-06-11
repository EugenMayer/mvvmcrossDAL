A data acceess layer implementation for MvvmCross which matches the ember-data approach.
In the end, this is what make the MvvmCross 2 way-bindings really work, since you do not create copies of enities and do not update on copies.
It is a centralized storage-layer implementation which is not there for persistence but rather for keeping object and the references in a single-point-of-truth

 - ensures that remotely fetched entities keep the same reference ( in terms of c# reference ) in the storage. This way bindings work across different views
 - same entities always have same references
 - ensures updates are merged in the DAL and updates the references
 - ensure that nested entities work out ( using namespaces for nesting )
