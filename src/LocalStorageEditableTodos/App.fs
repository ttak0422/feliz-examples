module LocalStorageEditableTodos

open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Feliz
open Feliz.ElmishComponents
open Elmish
open Thoth.Json

type TodoEdit =
    { Index: int
      Text: string }

type Todo =
    { Text: string
      Completed: bool }

type Model =
    { Text: string
      Todos: Todo list
      Editing: TodoEdit option }

type Msg =
    | UpdateText of string
    | AddTodo
    | RemoveTodo of int
    | Edit of int * string
    | SaveEdit of int * string
    | ToggleTodo of int
    | Failure of string

[<LiteralAttribute>]
let KEY_LOCAL_STORAGE_EDITABLE_TODOS = "LOCAL_STORAGE_EDITABLE_TODOS"

let loadTodos(): Todo list =
    Browser.WebStorage.localStorage.getItem KEY_LOCAL_STORAGE_EDITABLE_TODOS
    |> unbox
    |> Decode.fromString (Decode.Auto.generateDecoder<Todo list>())
    |> function
    | Ok xs -> xs
    | _ -> []

let saveTodos (todos: Todo list): Msg Cmd =
    let save (todos: Todo list) =
        Encode.Auto.toString (0, todos)
        |> fun json -> (KEY_LOCAL_STORAGE_EDITABLE_TODOS, json)
        |> Browser.WebStorage.localStorage.setItem
    Cmd.OfFunc.attempt save todos (string >> Failure)

let init: Model * Msg Cmd =
    { Text = ""
      Todos = loadTodos()
      Editing = None }, []

let update (msg: Msg) (model: Model): Model * Msg Cmd =
    match msg with
    | UpdateText text -> { model with Text = text }, []
    | AddTodo ->
        let todos' =
            model.Todos @ [ { Text = model.Text
                              Completed = false } ]
        { model with
              Text = ""
              Todos = todos' }, saveTodos todos'
    | RemoveTodo idx ->
        let init = List.take idx model.Todos
        let tail = List.skip (idx + 1) model.Todos
        let todos' = init @ tail
        { model with Todos = todos' }, saveTodos todos'
    | Edit(idx, todo) ->
        { model with
              Editing =
                  Some
                      { Index = idx
                        Text = todo } }, []
    | SaveEdit(idx, todoText) ->
        let todos' =
            model.Todos
            |> List.indexed
            |> List.map (fun (i, x) ->
                if i = idx then { x with Text = todoText } else x)
        { model with
              Editing = None
              Todos = todos' }, saveTodos todos'
    | ToggleTodo idx ->
        let todos' =
            model.Todos
            |> List.indexed
            |> List.map (fun (i, x) ->
                if i = idx then { x with Completed = not x.Completed } else x)
        { model with Todos = todos' }, saveTodos todos'
    | Failure err ->
        JS.console.error err
        model, []

let todoForm (formText: string) (dispatch: Msg -> unit): ReactElement =
    Html.div
        [ prop.className "row"
          prop.children
              [ Html.div
                  [ prop.className "col-9"
                    prop.children
                        [ Html.input
                            [ prop.onInput (fun e ->
                                !!e.target?value
                                |> UpdateText
                                |> dispatch)
                              prop.valueOrDefault formText
                              prop.autoFocus true
                              prop.className "form-control"
                              prop.placeholder "Enter a todo"
                              prop.onKeyDown (fun e ->
                                  // OnEnter
                                  if e.keyCode = 13. && formText.Length > 0 then dispatch AddTodo) ] ] ]
                Html.div
                    [ prop.className "col-3"
                      prop.children
                          [ Html.button
                              [ prop.className "btn btn-primary form-control"
                                prop.onClick (fun _ -> dispatch AddTodo)
                                prop.text "+" ] ] ] ] ]


let viewNormalTodo (idx: int) (todo: Todo) (dispatch: Msg -> unit): ReactElement =
    Html.div
        [ prop.className "card"
          prop.children
              [ Html.div
                  [ prop.className "card-block"
                    prop.children
                        [ Html.input
                            [ prop.onClick (fun _ -> dispatch <| ToggleTodo idx)
                              prop.type' "checkbox"
                              prop.defaultChecked todo.Completed
                              prop.className "mr-3" ]
                          Html.span
                              [ prop.onDoubleClick (fun _ -> dispatch <| Edit(idx, todo.Text))
                                prop.style [ todo.Completed, [ style.textDecoration textDecorationLine.lineThrough ] ]
                                prop.text todo.Text ]
                          Html.span
                              [ prop.onClick (fun _ -> dispatch (RemoveTodo idx))
                                prop.className "float-right"
                                prop.text "✖︎" ] ] ] ] ]

let viewEditTodo (idx: int) (edit: TodoEdit) (dispatch: Msg -> unit): ReactElement =
    Html.div
        [ prop.className "card"
          prop.children
              [ Html.div
                  [ prop.className "card-block"
                    prop.children
                        [ Html.div
                            [ prop.onKeyDown (fun e ->
                                if e.keyCode = 13. && edit.Text.Length > 0 then
                                    dispatch <| SaveEdit(edit.Index, edit.Text))
                              prop.valueOrDefault edit.Text
                              prop.children
                                  [ Html.input
                                      [ prop.onInput (fun e ->
                                          !!e.target?value
                                          |> fun x ->
                                              (idx, x)
                                              |> Edit
                                              |> dispatch)
                                        prop.valueOrDefault edit.Text
                                        prop.className "form-control" ] ] ] ] ] ] ]

let todoList (todos: Todo list) (edit: TodoEdit option) (dispatch: Msg -> unit) =
    Html.div
        [ prop.children
            (match edit with
             | None ->
                 todos
                 |> List.indexed
                 |> List.map (fun (idx, x) -> viewNormalTodo idx x dispatch)
             | Some(edit) ->
                 todos
                 |> List.indexed
                 |> List.map
                     (fun (i, x) ->
                         if i = edit.Index then viewEditTodo i edit dispatch else viewNormalTodo i x dispatch)) ]


let render (model: Model) (dispatch: Msg Dispatch): ReactElement =
    Html.div
        [ prop.className "col-12 col-sm-6 offset-sm-3"
          prop.children
              [ todoForm model.Text dispatch
                todoList model.Todos model.Editing dispatch ] ]

let localStorageEditableTodos = React.elmishComponent ("LocalStorageEditableTodos", init, update, render)

open Browser

ReactDom.render (localStorageEditableTodos, document.getElementById "feliz-app")
