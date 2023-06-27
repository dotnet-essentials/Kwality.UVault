Updating the documentation
##########################

This technical document presents a systematic procedure for constructing and locally accessing the documentation,
delineating each step in a sequential manner.

.. warning::
   All the commands listed in this document should be executed within the ``docs/`` folder.

Docker image
************


To initiate the construction of the Docker image utilized in the documentation building process, execute the subsequent
PowerShell command:

.. code-block:: console

    $ docker image build -t uvault/docs .

The subsequent command generates a Docker image named `uvault/docs`, which can be employed for the purpose of constructing
the documentation for the project.

Building the documentation
**************************

Subsequent to the creation of the Docker image, you may utilize the ensuing PowerShell command to generate an HTML
rendition of the UVault documentation. Once the process concludes, the resulting output will be accessible within the
`/docs/_build/` directory.

.. code-block:: console

    $ docker run -it --rm -v $PWD/:/docs uvault/docs make html
