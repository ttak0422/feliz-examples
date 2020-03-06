module FilterTodos

open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Feliz
open Feliz.ElmishComponents
open Elmish
open Thoth.Json

type TodoEdit =
    { Id: int
      Text: string }

type Todo =
    { Id: int
      Text: string
      Completed: bool }

type Filter =
    | All
    | InComplete
    | Completed

type Model =
    { Text: string
      Todos: Todo list
      Editing: TodoEdit option
      Filter: Filter }

type Msg =
    | UpdateText of string
    | GenerateTodoId
    | AddTodo of int
    | RemoveTodo of int
    | Edit of int * string
    | SaveEdit of int * string
    | ToggleTodo of int
    | SetFilter of Filter
    | Failure of string

[<LiteralAttribute>]
let KEY_FILTER_TODOS = "KEY_FILTER_TODOS"

let loadTodos(): Todo list =
    Browser.WebStorage.localStorage.getItem KEY_FILTER_TODOS
    |> unbox
    |> Decode.fromString (Decode.Auto.generateDecoder<Todo list>())
    |> function
    | Ok xs -> xs
    | _ -> []

let saveTodos (todos: Todo list): Msg Cmd =
    let save (todos: Todo list) =
        Encode.Auto.toString (0, todos)
        |> fun json -> (KEY_FILTER_TODOS, json)
        |> Browser.WebStorage.localStorage.setItem
    Cmd.OfFunc.attempt save todos (string >> Failure)

let generateId: unit -> Msg Cmd =
    let r = System.Random()
    fun () -> AddTodo(r.Next(System.Int32.MinValue, System.Int32.MaxValue)) |> Cmd.ofMsg

let init: Model * Msg Cmd =
    { Text = ""
      Todos = loadTodos()
      Editing = None
      Filter = All }, []

let update (msg: Msg) (model: Model): Model * Msg Cmd =
    match msg with
    | UpdateText text -> { model with Text = text }, []
    | AddTodo id ->
        let todos' =
            model.Todos @ [ { Id = id
                              Text = model.Text
                              Completed = false } ]
        { model with
              Text = ""
              Todos = todos' }, saveTodos todos'
    | RemoveTodo id ->
        let todos' = model.Todos |> List.filter (fun x -> x.Id <> id)
        { model with Todos = todos' }, saveTodos todos'
    | Edit(id, todo) ->
        { model with
              Editing =
                  Some
                      { Id = id
                        Text = todo } }, []
    | SaveEdit(id, todoText) ->
        let todos' =
            model.Todos
            |> List.map (fun x ->
                if x.Id = id then { x with Text = todoText } else x)
        { model with
              Editing = None
              Todos = todos' }, saveTodos todos'
    | ToggleTodo id ->
        let todos' =
            model.Todos
            |> List.map (fun x ->
                if x.Id = id then { x with Completed = not x.Completed } else x)
        { model with Todos = todos' }, saveTodos todos'
    | Failure err ->
        JS.console.error err
        model, []
    | GenerateTodoId -> model, generateId()
    | SetFilter filter -> { model with Filter = filter }, []

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
                                  if e.keyCode = 13. && formText.Length > 0 then dispatch GenerateTodoId) ] ] ]
                Html.div
                    [ prop.className "col-3"
                      prop.children
                          [ Html.button
                              [ prop.className "btn btn-primary form-control"
                                prop.onClick (fun _ -> dispatch GenerateTodoId)
                                prop.text "+" ] ] ] ] ]

let viewFilter (filter: Filter) (isFilter: bool) (filterText: string) (dispatch: Msg Dispatch): ReactElement =
    if isFilter then
        Html.span
            [ prop.className "mr-3"
              prop.text filterText ]
    else
        Html.span
            [ prop.className "text-primary mr-3"
              prop.text filterText
              prop.onClick (fun _ -> dispatch <| SetFilter filter)
              prop.style [ style.cursor "pointer" ] ]

let viewFilters (filter: Filter) (dispatch: Msg Dispatch): ReactElement =
    Html.div
        [ prop.children
            [ viewFilter All (filter = All) (All.ToString()) dispatch
              viewFilter InComplete (filter = InComplete) (InComplete.ToString()) dispatch
              viewFilter Completed (filter = Completed) (Completed.ToString()) dispatch ] ]

let viewNormalTodo (todo: Todo) (dispatch: Msg -> unit): ReactElement =
    Html.div
        [ prop.className "card"
          prop.children
              [ Html.div
                  [ prop.className "card-block"
                    prop.children
                        [ Html.input
                            [ prop.onClick (fun _ -> dispatch <| ToggleTodo todo.Id)
                              prop.type' "checkbox"
                              prop.defaultChecked todo.Completed
                              prop.className "mr-3" ]
                          Html.span
                              [ prop.onDoubleClick (fun _ -> dispatch <| Edit(todo.Id, todo.Text))
                                prop.style [ todo.Completed, [ style.textDecoration textDecorationLine.lineThrough ] ]
                                prop.text todo.Text ]
                          Html.span
                              [ prop.onClick (fun _ -> dispatch (RemoveTodo todo.Id))
                                prop.className "float-right"
                                prop.text "✖︎" ] ] ] ] ]

let viewEditTodo (edit: TodoEdit) (dispatch: Msg -> unit): ReactElement =
    Html.div
        [ prop.className "card"
          prop.children
              [ Html.div
                  [ prop.className "card-block"
                    prop.children
                        [ Html.div
                            [ prop.onKeyDown (fun e ->
                                if e.keyCode = 13. && edit.Text.Length > 0 then dispatch <| SaveEdit(edit.Id, edit.Text))
                              prop.valueOrDefault edit.Text
                              prop.children
                                  [ Html.input
                                      [ prop.onInput (fun e ->
                                          !!e.target?value
                                          |> fun x ->
                                              (edit.Id, x)
                                              |> Edit
                                              |> dispatch)
                                        prop.valueOrDefault edit.Text
                                        prop.className "form-control" ] ] ] ] ] ] ]

let todoList (todos: Todo list) (edit: TodoEdit option) (filter: Filter) (dispatch: Msg -> unit) =
    Html.div
        [ prop.children
            (todos
             |> List.filter (fun x ->
                 match filter with
                 | All -> true
                 | InComplete -> (not x.Completed)
                 | Completed -> x.Completed)
             |> (match edit with
                 | None -> List.map (fun x -> viewNormalTodo x dispatch)
                 | Some(edit) ->
                     List.map
                         (fun x -> if x.Id = edit.Id then viewEditTodo edit dispatch else viewNormalTodo x dispatch))) ]


let render (model: Model) (dispatch: Msg Dispatch): ReactElement =
    Html.div
        [ prop.className "col-12 col-sm-6 offset-sm-3"
          prop.children
              [ todoForm model.Text dispatch
                viewFilters model.Filter dispatch
                todoList model.Todos model.Editing model.Filter dispatch ] ]

let filterTodos = React.elmishComponent ("FilterTodos", init, update, render)

open Browser

ReactDom.render (filterTodos, document.getElementById "feliz-app")
