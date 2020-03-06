module Todos

open Feliz
open Fable.Core
open Fable.Core.JsInterop

type Model =
    { Text: string
      Todos: string list }

type Msg =
    | UpdateText of string
    | AddTodo
    | RemoveTodo of int

let init: Model =
    { Text = ""
      Todos = [] }

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

let viewTodo (idx: int) (todo: string) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "card"
          prop.children
              [ Html.div
                  [ prop.className "card-block"
                    prop.children
                        [ Html.text todo
                          Html.span
                              [ prop.onClick (fun _ -> dispatch (RemoveTodo idx))
                                prop.className "float-right"
                                prop.children [ Html.text "✖︎" ] ] ] ] ] ]

let todos =
    React.functionComponent
        ("todos",
         (fun () ->
             let model, dispatch = React.useReducer (update, init)

             Html.div
                 [ prop.className "col-12 col-sm-6 offset-sm-3"
                   prop.children
                       [ Html.div
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
                                                 prop.valueOrDefault model.Text
                                                 prop.autoFocus true
                                                 prop.className "form-control"
                                                 prop.placeholder "Enter a todo"
                                                 prop.onKeyDown (fun e ->
                                                     // OnEnter
                                                     if e.keyCode = 13. && model.Text.Length > 0 then dispatch AddTodo) ] ] ]
                                   Html.div
                                       [ prop.className "col-3"
                                         prop.children
                                             [ Html.button
                                                 [ prop.className "btn btn-primary form-control"
                                                   prop.onClick (fun _ -> dispatch AddTodo)
                                                   prop.children [ Html.text "+" ] ] ] ] ] ]
                         Html.div
                             [ prop.children
                                 (model.Todos
                                  |> List.indexed
                                  |> List.map (fun (idx, x) -> viewTodo idx x dispatch)) ] ] ]))

open Browser.Dom

ReactDOM.render (todos, document.getElementById "feliz-app")
