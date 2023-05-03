Updating the documentation
==========================

This document describes how UVault uses Docker to build the documentation.

.. warning::
   The commands that are listed on this page **must** be executed from within the ``docs/`` folder.

Docker image
------------

You can build the Docker image that's used for building the documentation using the following PowerShell command.

.. code-block:: console

    $ docker image build -t uvault/docs .

This command generates a Docker image named `uvault/docs` which can be used to build to project's documentation.

Building the documentation
--------------------------

With the Docker image created, the following PowerShell command to generate an HTML version of UVault's documentation.
Once this command completes, the output can be found in the ``/docs/_build/`` folder.

.. code-block:: console

    $ docker run -it --rm -v $PWD/:/docs uvault/docs make html
