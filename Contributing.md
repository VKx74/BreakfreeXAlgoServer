# Contribute to Fintatech.TDS Community Wizard - Storage API

Thank you for your interest in contributing to Storage API. This guide details how to contribute to Storage.

## Issue tracker

When submitting an issue please conform to the guidelines listed below. 
Not all issues will be addressed and your issue is more likely to
be addressed if you submit a merge request which partially or fully solves the issue.

If it happens that you know the solution to an existing bug, please first
open the issue in order to keep track of it and then open the relevant merge
request that potentially fixes it. 

Search the issue tracker for similar entries before submitting your own, 
there's a good chance somebody else had the same issue or feature proposal.

### Feature proposals

To create a feature proposal for Storage, open an issue on the issue tracker.
In order to help track the feature proposals, we have created a ~enhancement label. 
Please keep feature proposals as small and simple as possible, complex ones
might be edited to make them small and simple.

For changes in the user interface, it can be helpful to create a mockup first.
If you want to create something yourself, consider opening an issue with ~discussion label first to
discuss whether it is interesting to include this in Storage.

### Bug reports

To submit bug report open an issue on the issue tracker.
In order to help track the bugs, we have created a ~bug label.
Please include steps to reproduce the bug, actual and expected behaviors. 
It would be good to paste relevant screenshots or even attach a video in case of tricky bugs.


## Workflow for the development team

- Master assigns issue to the developer.
- Developer creates new branch 'Issue_#N' in the repository, where N is the issue number.
- Developer adds ~"in progress" label to the issue when he/she starts working on it.
- When issue is done developer removes ~"in progress" label, adds ~"code review" label and assigns issue to the reviewer.
- Reviewer verifies developer's work.
  - If something is wrong (bad code, incorrect implementation and so on) reviewer removes ~"code review" label, 
    adds ~"code review failure" label and assigns issue back to the developer.
  - If everything is fine reviewer removes ~"code review" label, adds ~"QA review" label and assigns issue to the QA.
- QA checks if issue implementation conforms the specification.
  - In case of any issues QA removes ~"QA review" label, adds ~"QA review failure" label and assigns issue back to the developer.
  - If everything is fine QA removes ~"QA review" label, adds ~verified label and assigns issue to the master.
- Master merges issue branch into the develop, removes ~verified label, adds ~merged label and assigns issue to the QA.
- QA verifies if everything works properly after merging.
  - In case of any issues QA removes ~merged label, adds ~"QA review failure" label and assigns back to the master.
  - If everything is fine QA adds ~done label and closes the issue.

## Code of conduct

As contributors and maintainers of this project, we pledge to respect all people who contribute through 
reporting issues, posting feature requests, updating documentation, submitting pull requests or patches, and other activities.

We are committed to making participation in this project a harassment-free experience for everyone, 
regardless of level of experience, gender, gender identity and expression, sexual orientation, 
disability, personal appearance, body size, race, ethnicity, age, or religion.

Examples of unacceptable behavior by participants include the use of sexual language or imagery, 
derogatory comments or personal attacks, trolling, public or private harassment, insults, or other unprofessional conduct.

We have the right and responsibility to remove, edit, or reject comments, commits, code, wiki edits, 
issues, and other contributions that are not aligned to this Code of Conduct. 
Project maintainers who do not follow the Code of Conduct may be removed from the project team.