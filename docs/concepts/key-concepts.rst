Key concepts
############

This document serves as a comprehensive overview of the fundamental concepts that are crucial to grasp when working with
UVault. Its purpose is to provide guidance to users, aiding them in developing a comprehensive understanding of the
project and its functionalities. By familiarizing themselves with the concepts elucidated herein, users can navigate
UVault more effectively and leverage its features to their fullest potential.

.. _manager-concept:

Manager
*******

Within UVault, every management operation related to API's, grants, users and more is is carried out through a manager
that relies on a store as its underlying mechanism. The store acts as a repository where information can be persistently
stored. This allows for various options such as in-memory storage, database storage, or integration with external
service providers like Auth0.

The store plays a crucial role in UVault by providing a unified interface to manage user-related data. It abstracts away
the specific details of the storage implementation and allows the manager to perform operations such as fetching,
creating, modifying, and removing data without concern for the underlying storage technology.

By decoupling the management operations from the storage implementation, UVault offers flexibility and extensibility.
Developers can choose the most suitable storage option for their specific needs, whether it be an in-memory solution for
testing purposes or a robust database for production environments. Additionally, integration with external service
providers expands the possibilities for data management.

By leveraging the store mechanism, UVault enables efficient and seamless user management while accommodating a variety
of storage options to cater to diverse application requirements.

.. _store-concept:

Store
*****

In UVault, a store is considered the authoritative source of information for a specific concept, which can include
users, APIs, applications, and other relevant entities. The store functions as a repository that holds all the essential
data associated with the respective concept. It is designed to ensure that the stored information is accurate,
consistent, and up-to-date.

As the "sole source of truth," the store is responsible for maintaining the integrity and reliability of the data it
holds. It serves as a centralized location where the relevant information is stored, organized, and managed. By having a
single, authoritative store, UVault avoids data inconsistencies or discrepancies that can arise from multiple data
sources.

The store is designed to provide a reliable and efficient means of accessing and manipulating the data related to a
specific concept. It acts as a foundation for various operations, such as retrieving user details, updating API
configurations, managing application information, and more. UVault ensures that any changes made to the concept's data\
are reflected consistently across the system, maintaining data integrity.

By utilizing a store as the repository of essential information, UVault establishes a reliable and centralized source
that can be trusted for accurate and up-to-date data related to users, APIs, applications, and other relevant concepts.
This ensures the consistency and integrity of the information within the UVault system.

.. _key-concept:

Key
***

In UVault, a key is employed as a distinct identifier to uniquely identify entities within the store. These entities can
include users, APIs, applications, and other relevant elements. The specific type of key utilized may vary depending on
the implementation adopted by different stores.

For instance, in a store that operates on a database, the key implementation may employ an integer-based or a GUID-based
key to uniquely identify its entities. These keys are typically generated or assigned during the creation of the entity
and serve as a means to reference and retrieve the entity from the store.

On the other hand, when integrating with an external service provider like Auth0, a string-based key is commonly used to
fulfill the same purpose of uniquely identifying entities. Auth0 generates or assigns string-based keys that can be
utilized to identify users, APIs, applications, and other relevant entities within the UVault system.

The choice of key implementation depends on the specific requirements and characteristics of the underlying store or
service provider. UVault is designed to accommodate different key types, allowing for flexibility in integrating with
diverse storage systems or external services. By leveraging appropriate keys, UVault ensures accurate and efficient
identification and retrieval of entities from the store, enabling seamless management and interaction with users, APIs,
applications, and other entities within the UVault ecosystem.

.. _model-mapper-concept:

Model mapper
************

A model mapper refers to a transformation process that facilitates the conversion of the model representation of a
concept stored within a store to a model that can be readily understood and utilized within your codebase.

In UVault, each store is responsible for storing various concepts, such as users, APIs, applications, and others, using
a specific model representation. The stored model can differ depending on the store implementation being used. For
instance, in the case of the built-in Static store, the model stored in the store matches the model defined within your
codebase. This direct correspondence simplifies the mapping process since the models align seamlessly.

However, in other stores like the Auth0 store, the stored model may differ from the model representation in your
codebase. This discrepancy arises due to the specific requirements or conventions of the external service provider or
storage system.

The model mapper plays a crucial role in bridging this gap by converting the store model to a model that is compatible
with your codebase. It handles the necessary transformations and mappings, ensuring that the data stored in the store
can be easily understood and utilized within your application. This allows for seamless integration and interaction with
the store, irrespective of any variations between the store model and your codebase model.

By leveraging the model mapper, UVault enables the smooth synchronization and translation of data between the store and
your codebase, promoting efficient communication and utilization of concepts such as users, APIs, applications, and
others within your UVault-powered application.

.. _operation-mapper-concept:

Operation mapper
****************

An operation mapper refers to a unidirectional transformation process that converts a concept stored within a store into
a model that can be effectively utilized to carry out operations on the model.

Within UVault, each store has the flexibility to use different models when creating or updating a specific concept. This
variation in models may arise due to specific requirements, conventions, or data structures of the underlying store
implementation.

In this context, the operation mapper plays a critical role in facilitating the transformation of your model into a
format that aligns with the requirements of the desired operation. It ensures compatibility between your model and the
operation to be performed on it.

By utilizing the operation mapper, you can convert your model representation into a format that effectively aligns with
the operation's expectations. This transformation allows for seamless execution of operations on the model, such as
creating or updating a concept, within the UVault system.

The operation mapper enables UVault to work with different models and stores while ensuring interoperability and
compatibility between your model and the desired operation. It streamlines the process of interacting with the store
and carrying out operations on the model, promoting efficient data manipulation and management within your
UVault-powered application.
