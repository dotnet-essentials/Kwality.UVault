Key concepts
############

This document provides an overview of the essential concepts that must be comprehended while utilizing UVault.
It is designed to guide the users towards a better understanding of the project and its functionalities.

.. _manager-concept:

Manager
*******

Each user management operation is executed through a manager, which, in turn, utilizes a store as it's underlying
mechanism. The store, therefore, serves as a repository where information can be persisted, such as in-memory, a
database, or an external service provider like Auth0.

.. _store-concept:

Store
*****

A store is regarded as the sole source of truth for a given concept, including users, APIs, applications, and others.
The store acts as a repository that contains all the essential information related to the respective concept, ensuring
that the data is accurate, consistent, and up-to-date.

.. _key-concept:

Key
***

A key serves as a unique identifier that is used to identify an entity from the store, including users, APIs,
applications, and other entities. The key may vary depending on the implementation used by different stores.
For instance, a store that operates on a database may utilize an integer-based or a GUID-based key to identify its
entities. On the other hand, an external service provider such as Auth0 may use a string-based key for the same purpose.

.. _model-mapper-concept:

Model mapper
************

A model mapper refers to a one-way transformation that converts the model representation of a concept stored within a
store to a model that can be easily understood by your code.

Each store may store a concept such as users, APIs, applications, and others using a specific model. In some stores,
like the built-in `Static` store, the stored model matches the model defined in your codebase. In contrast, for other
stores, like the Auth0 store, the stored model might differ from the representation in your codebase.
The model mapper plays a crucial role in converting the store model to a model that is compatible with your codebase.

.. _operation-mapper-concept:

Operation mapper
****************

An operation mapper is a one-way transformation that converts a concept stored within a store into a model that can be
used to carry out operations on the model.

Each store may use different models to create or update a particular concept.
In this context, the operation mapper plays a crucial role in transforming your model into a format that is compatible
with the operation you wish to perform.
