# feliz-examples

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