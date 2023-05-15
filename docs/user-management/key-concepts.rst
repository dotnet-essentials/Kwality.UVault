Key concepts
============

Every operation associated with the User Management module is executed through a manager, which, in turn, utilizes a
store as its underlying mechanism. The store, therefore, serves as a repository where user information can be persisted,
such as in-memory, a database, or an external service provider like Auth0.

UVault's User Management components comes equipped with two pre-built stores:

- The default, `Static` store, which stores users in a collection stored in memory. `(Note that this isn't thread-safe).`
- An `Auth0` store, which stores users in Auth0.

It is also possible to develop a custom store if any the built-in stores do not adequately meet your requirements.
For additional details, see :ref:`create your own store <user_management_custom-store>` for more information.

.. _user_management_understanding-keys:

Understanding keys
------------------

To store a user in a particular store, it is necessary to uniquely identify each user with a key.
Depending on the store used by the manager, the type of key employed may differ. For instance, a database may employ
an integer-based key, while an external service provider such as Auth0 may use a string-based key.

The package comes with two pre-built key types, an integer-based key, and a string-based key.
However, it is feasible to create a custom key by developing a class that implements the
`IEquatable<T> <https://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1?view=net-7.0>`_ interface.

.. _user_management_understanding-operation-mappers:

Understanding operation mappers
-------------------------------

Certain stores may use a distinct model for creating and/or updating a user.
To support this functionality, the operations necessitate an `IUserOperationMapper <https://github.com/dotnet-essentials/Kwality.UVault/blob/feature/3-add-user-management/app/Kwality.UVault.User.Management/Operations/Mappers/Abstractions/IUser.Operation.Mapper.cs>`_
to transform your model to a model that the used store does understand.
