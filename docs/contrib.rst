Contributing to UVault
======================

Few open-source projects are going to be successful without contributions and UVault is no exception to this rule.
We are deeply grateful for all contributions no matter their size.

Finding existing issues
-----------------------

Before filing a new issue, please use our `issue tracker <https://github.com/dotnet-essentials/Kwality.UVault/issues>`_
to check if it already exists.

.. note::
  If you do find an existing issue, please include your own feedback in the discussion.
  Instead of posting "me too", upvote the issue with 👍, as this better helps us prioritize popular issues and avoids
  spamming people who are subscribed to the issue.

Writing a Good Bug Report
^^^^^^^^^^^^^^^^^^^^^^^^^
Good bug reports make it easier for maintainers to verify and root cause the underlying problem.
The better a bug report, the faster the problem will be resolved.
Ideally, a bug report should contain the following information:

* A high-level description of the problem.
* A minimal reproduction, i.e. the smallest size of code/configuration required to reproduce the wrong behavior.
* A description of the expected behavior, contrasted with the actual behavior observed.
* Information on the environment: nuget version, .NET version, etc.
* Additional information, e.g. is it a regression from previous versions? are there any known workarounds?

When ready to submit a bug report, please use the
`Bug report <https://github.com/dotnet-essentials/Kwality.UVault/issues/new/choose>`_ issue template.

Contributing Changes
--------------------

UVault exposes many extension knobs to implement custom providers and for that reason, the core library very rarely
accepts extra dependencies from someone favorite provider.

In order for UVault to provide a consistent experience across the library, we generally want to review every single API
that's added, changed or deleted. Changes to the API must be proposed, discussed and approved with the ``API approved``
label in a separate issue before opening a PR.

DOs and DON'Ts
^^^^^^^^^^^^^^

Please do:

- Target your `PR <https://help.github.com/articles/using-pull-requests>`_ at the `main` branch.
- Follow the coding style already present in the project.
- Use ReSharper (CLI) for inspection(s). A GitHub action is present which runs these inspections on every commit.
  So anything reported by ReSharper (CLI) will cause your pull request to be declined.
- Ensure that your changes are covered by new or existing tests.
- Ensure that the quality of your tests is sufficient by performing mutation testing using
  `Stryker <https://stryker-mutator.io/docs/stryker-net/introduction/>`_.
- Code coverage must be non-decreasing unless approved by the authors.
- If the contribution adds a feature or fixes a bug, please update the
  `release notes <https://kwalityuvault.readthedocs.io/en/latest/release-notes.html>`_.
- If the contribution affects the documentation, please update the
  `documentation <https://kwalityuvault.readthedocs.io/en/latest/>`_.

Please do not:

- Create big PR's. Instead, file an issue and discuss and start  a discussion so we can agree on a direction before you
  invest a large amount of time. This includes ``any`` change to the public API.
- Add dependencies to your favorite packages in the ``core`` library, instead opt for another design where your favorite
  library is included in an additional project.
