# feliz-examples

[![Build Status](https://dev.azure.com/ttak0422/feliz-example/_apis/build/status/ttak0422.feliz-examples?branchName=master)](https://dev.azure.com/ttak0422/feliz-example/_build/latest?definitionId=7&branchName=master) [![Open in Gitpod](https://gitpod.io/button/open-in-gitpod.svg)](https://gitpod.io/#https://github.com/ttak0422/feliz-examples)

A hybrid approach using components.

When managing data as in **ManagedTodos**, manage it in the same way as before.

To manage only pages and **ManagedTodos**, this application is implemented with the following small **Model** and **Msg**.

```fsharp
type Model =
    { Page: Page
      ManagedTodosModel: ManagedTodos.Model }

type Msg = 
    | PageChanged of Page
    | ManagedTodosMsg of ManagedTodos.Msg
```