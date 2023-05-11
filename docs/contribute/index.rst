Contributing to UVault
======================

Contributions to open-source projects are essential for their success, and UVault is no exception to this rule.
We appreciate all contributions, regardless of their size.

Finding existing issues
-----------------------

Prior to submitting a new issue, we kindly request that you check our
`issue tracker <https://github.com/dotnet-essentials/Kwality.UVault/issues>`_  to ensure that a similar issue has not
already been reported.

.. note::
  Should you discover an issue that has already been reported, we encourage you to participate in the ongoing discussion
  by sharing your own feedback. Rather than posting a simple "me too" comment, please upvote the issue using the 👍
  reaction. This approach assists us in prioritizing popular issues and prevents the need to repeatedly notify
  individuals subscribed to the issue thread.

Writing a Good Bug Report
^^^^^^^^^^^^^^^^^^^^^^^^^
Effective bug reports play a crucial role in assisting maintainers with verifying and identifying the root cause of an
issue. The quality of a bug report directly impacts the speed with which a problem can be resolved.
Ideally, a bug report should include the following details:

* A brief overview of the problem.
* A minimal reproduction scenario, consisting of the smallest possible code or configuration required to recreate the
  undesired behavior.
* A comparison of the expected versus actual behavior.
* Information about the environment, including the NuGet version, .NET version, and any other relevant system details.
* Additional information, such as whether the issue is a regression from previous versions or if there are known
  workarounds.

To submit a bug report, please use the
`Bug report <https://github.com/dotnet-essentials/Kwality.UVault/issues/new/choose>`_ issue template.

Contributing Changes
--------------------

The UVault core library has a strict policy of minimizing dependencies and rarely accepts changes that introduce
additional ones. If it is necessary to add an extra dependency, please consider designing a solution where it can be
incorporated into a separate project instead of the core library.

To ensure a uniform experience throughout the library, all new APIs, modifications, and deletions must undergo review.
Before submitting a pull request, changes to the API must be proposed, discussed, and approved using the
``API approved`` label in a separate issue.

DOs and DON'Ts
^^^^^^^^^^^^^^

When submitting a pull request, please adhere to the following guidelines:

- Target the `main`` branch with your `pull request <https://help.github.com/articles/using-pull-requests>`_.
- Follow the coding style established in the existing project.
- Follow the coding style established in the existing project.
  Any issues detected by ReSharper (CLI) will result in the rejection of your pull request.
- Verify that your changes are covered by either new or pre-existing tests.
- Ensure that your test quality is adequate by performing mutation testing with
  `Stryker <https://stryker-mutator.io/docs/stryker-net/introduction/>`_.
- Code coverage should not decrease unless it has been approved by the authors.
- If your contribution affects the documentation, please update the
- `documentation <https://kwalityuvault.readthedocs.io/en/latest/>`_.

To ensure that your pull request is well-received, please avoid the following:

- Do not create excessively large pull requests. Instead, open an issue to begin a discussion and come to an agreement
  on the direction of the proposed changes before investing a substantial amount of time. This includes any
  modifications to the public API.

Validate your work locally
^^^^^^^^^^^^^^^^^^^^^^^^^^

To ensure that your PR does not break any existing functionality, it is recommended that you run the test suite locally
before submitting it.

Since UVault closely integrates with Auth0, it is necessary to have a (free) Auth0 account before proceeding.
After creating an account, the following steps need to be performed:

1. Change the value of the `Default Directory` to ``Username-Password-Authentication`` inside your tenant's settings.
2. Create a `Regular Web Application` application in your tenant and enable the ``Password`` grant type.
3. Create an API in your tenant or use the default ``Auth0 Management API``.
4. Create a user in your tenant that uses the ``Username-Password-Authentication`` connection.

Once your Auth0 account is configured, set the following environment variables:

- ``AUTH0_AUDIENCE``: The API audience of the API in your tenant.
- ``AUTH0_CLIENT_ID``: The Client ID of the application in your tenant.
- ``AUTH0_CLIENT_SECRET``: The Client Secret of the application in your tenant.
- ``AUTH0_TOKEN_ENDPOINT``: The endpoint of your Auth0 tenant used to authenticate.
- ``AUTH0_USERNAME``: The username of the created Auth0 user.
- ``AUTH0_PASSWORD``: The password of the created Auth0 user.
- ``AUTH0_VALID_AUDIENCE``: The audience that's supported by your Auth0 tenant.
- ``AUTH0_VALID_ISSUER``: The issuer that's supported by your Auth0 tenant.
