# Contributing

The project maintainers maintain guidelines for contributing to the project. A team member will be happy to explain why a guideline is defined as it is.

General contribution guidance is included in this document.

## Up for Grabs

Project maintainers mark the most straightforward issues as "up for grabs". This set of issues is the place to start if you are interested in contributing but new to the codebase.

## Contribution "Bar"

Project maintainers will merge changes that improve the product significantly and broadly and that align with the project mission.

Maintainers will not merge changes that have narrowly-defined benefits, due to compatibility risk. Our goal is to keep they project broad and extensible, rather than specific and too opinionated. You are welcome to create your own project/library that extends ours. If you are unsure, please open an issue and we will be happy to discuss.

We may revert changes if they are found to be breaking.

Contributions must also satisfy the other published guidelines defined in this document.

## DOs and DON'Ts

Please do:

* **DO** follow our [coding style](##-C#-Coding-Style) (C# code-specific)
* **DO** give priority to the current style of the project or file you're changing even if it diverges from the general guidelines.
* **DO** include tests when adding new features. When fixing bugs, start with adding a test that highlights how the current behavior is broken. Changes without corresponding tests will be rejected.
* **DO** include documentation when adding new features. Changes without corresponding documentation will be rejected.
* **DO** keep the discussions focused. When a new or related topic comes up it's often better to create new issue than to side track the discussion.
* **DO** blog and tweet (or whatever) about your contributions, frequently!

Please do not:

* **DON'T** make PRs for style changes.
* **DON'T** surprise us with big pull requests. Instead, file an issue and start a discussion so we can agree on a direction before you invest a large amount of time.
* **DON'T** commit code that you didn't write. If you find code that you think is a good fit to add to the project, file an issue and start a discussion before proceeding.
* **DON'T** submit PRs that alter licensing related files or headers. If you believe there's a problem with them, file an issue and we'll be happy to discuss it.
* **DON'T** add API additions without filing an issue and discussing with us first.

## Commit Messages

Please format commit messages as follows (based on [A Note About Git Commit Messages](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html)):

```text
Summarize change in 50 characters or less

Provide more detail after the first line. Leave one blank line below the
summary and wrap all lines at 72 characters or less.

If the change fixes an issue, leave another blank line after the final
paragraph and indicate which issue is fixed in the specific format
below.

Fix #42
```

Do your best to factor commits appropriately, not too large with unrelated things in the same commit, and not too small with the same small change applied N times in N different commits.

## File Headers

The following file header is the used. Please use it for new files.

```cs
// This file is licensed under the MIT license.
// See the LICENSE file in the project root for more information.
```

## Copying Files from Other Projects

In some cases, we use files from other projects, typically where a binary distribution does not exist or would be inconvenient.

The following rules must be followed for PRs that include files from another project:

* The license of the file is [permissive](https://en.wikipedia.org/wiki/Permissive_free_software_licence). Ideally, MIT.
* The license of the file is left in-tact.
* The contribution is correctly attributed in the [3rd party notices](./THIRD-PARTY-NOTICES.TXT) file in the repository, as needed.

## Porting Files from Other Projects

There are many good algorithms implemented in other languages that would benefit the project. The rules for porting a Java file to C# , for example, are the same as would be used for copying the same file, as described above.

[Clean-room](https://en.wikipedia.org/wiki/Clean_room_design) implementations of existing algorithms that are not permissively licensed will generally not be accepted. If you want to create or nominate such an implementation, please create an issue to discuss the idea.

## C# Coding Style

For non code files (xml, etc), our current best guidance is consistency. When editing files, keep new code and changes consistent with the style in the files. For new files, it should conform to the style for that component. If there is a completely new component, anything that is reasonably broadly accepted is fine.

The general rule we follow is "use Visual Studio defaults".

1. We use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces, where each brace begins on a new line. A single line statement block can go without braces but the block must be properly indented on its own line and must not be nested in other statement blocks that use braces. One exception is that a `using` statement is permitted to be nested within another `using` statement by starting on the following line at the same indentation level, even if the nested `using` contains a controlled block.
2. We use four spaces of indentation (no tabs).
3. We use `camelCase` for internal and private fields and use `readonly` where possible. Prefix internal and private static fields with `_`. When used on static fields, `readonly` should come after `static` (e.g. `static readonly` not `readonly static`).  Public fields should be used sparingly and only if they are `readonly`.
4. We use `this.` when necessary.
5. We always specify the visibility, even if it's the default (e.g.
   `private string foo` not `string foo`). This is to explicitly express intent. Visibility should be the first modifier (e.g. `public abstract` not `abstract public`).
6. Namespace imports should be specified at the top of the file, *outside* of
   `namespace` declarations, and should be sorted alphabetically, with the exception of `System.*` namespaces, which are to be placed on top of all others.
7. Avoid more than one empty line at any time. For example, do not have two blank lines between members of a type.
8. Avoid spurious free spaces.
   For example avoid `if (someVar == 0)...`, where the dots mark the spurious free spaces.
   Consider enabling "View White Space (Ctrl+E, S)" if using Visual Studio to aid detection.
9. If a file happens to differ in style from these guidelines (e.g. private members are named `m_member` rather than `member`), the existing style in that file takes precedence.
10. We only use `var` when it's obvious what the variable type is (e.g. `var stream = new FileStream(...)` not `var stream = OpenStandardInput()`).
11. We use language keywords instead of BCL types (e.g. `int, string, float` instead of `Int32, String, Single`, etc) for both type references as well as method calls (e.g. `int.Parse` instead of `Int32.Parse`).
12. We use CAPITALIZED_SNAKE_CASING to name all our constant local variables and fields. The only exception is for interop code where the constant value should exactly match the name and value of the code you are calling via interop.
13. We use `nameof(...)` instead of `"..."` whenever possible and relevant.
14. Fields should be specified at the top within type declarations.
15. When including non-ASCII characters in the source code use Unicode escape sequences (\uXXXX) instead of literal characters. Literal non-ASCII characters occasionally get garbled by a tool or editor.
16. All public members must have XML comments documentation. We use [DocFX](https://dotnet.github.io/docfx/) to generate the documentation, so you may use [DocFX Markdown](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html) in the these comments. Undocumented public members will be rejected.

An [EditorConfig](https://editorconfig.org "EditorConfig homepage") file (`.editorconfig`) has been provided at the root of the repository, enabling C# auto-formatting conforming to the above guidelines.
