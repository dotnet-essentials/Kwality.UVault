Updating the documentation
##########################

This technical document details how UVault employs Docker for building its documentation.

.. warning::
   All the commands listed in this document should be executed within the ``docs/`` folder.

Docker image
************

To build the Docker image used for building the documentation, run the following PowerShell command:

.. code-block:: console

    $ docker image build -t uvault/docs .

The following command generates a Docker image with the name `uvault/docs` that can be used to build the documentation
of the project.

Building the documentation
**************************

After creating the Docker image, you can use the following PowerShell command to generate an HTML version of the UVault
documentation. Upon completion, the output will be located in the  ``/docs/_build/`` folder.

.. code-block:: console

    $ docker run -it --rm -v $PWD/:/docs uvault/docs make html
