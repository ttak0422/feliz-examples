module EditableTodos

open Feliz
open Fable.Core
open Fable.Core.JsInterop
open Fable.React

type TodoEdit =
    { Index: int
      Text: string }

type Model =
    { Text: string
      Todos: string list
      Editing: TodoEdit option }

type Msg =
    | UpdateText of string
    | AddTodo
    | RemoveTodo of int
    | Edit of int * string
    | SaveEdit of int * string

let init: Model =
    { Text = ""
      Todos = []
      Editing = None }

let updateAt (idx: int) (value: 'a): 'a list -> 'a list =
    List.indexed
    >> List.map (fun (i, x) ->
        if i = idx then value else x)

let update (model: Model): Msg -> Model =
    function
    | UpdateText text -> { model with Text = text }
    | AddTodo ->
        { model with
              Text = ""
              Todos = model.Todos @ [ model.Text ] }
    | RemoveTodo idx ->
        let init = List.take idx model.Todos
        let tail = List.skip (idx + 1) model.Todos
        { model with Todos = init @ tail }
    | Edit(idx, todo) ->
        { model with
              Editing =
                  Some
                      { Index = idx
                        Text = todo } }
    | SaveEdit(idx, todo) ->
        { model with
              Editing = None
              Todos = model.Todos |> updateAt idx todo }

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


let viewNormalTodo (idx: int) (todo: string) (dispatch: Msg -> unit): ReactElement =
    Html.div
        [ prop.className "card"
          prop.children
              [ Html.div
                  [ prop.className "card-block"
                    prop.children
                        [ Html.span
                            [ prop.onDoubleClick (fun _ -> dispatch <| Edit(idx, todo))
                              prop.text todo ]
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
                                              (edit.Index, x)
                                              |> Edit
                                              |> dispatch)
                                        prop.valueOrDefault edit.Text
                                        prop.className "form-control" ] ] ] ] ] ] ]

let todoList (todos: string list) (edit: TodoEdit option) (dispatch: Msg -> unit) =
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


let editableTodos =
    React.functionComponent
        ("EditableTodos",
         (fun () ->
             let model, dispatch = React.useReducer (update, init)
             Html.div
                 [ prop.className "col-12 col-sm-6 offset-sm-3"
                   prop.children
                       [ todoForm model.Text dispatch
                         todoList model.Todos model.Editing dispatch ] ]))

open Browser.Dom

ReactDOM.render (editableTodos, document.getElementById "feliz-app")
