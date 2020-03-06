module InputBox

open Feliz
open Fable.Core
open Fable.Core.JsInterop

type Model =
    { Text: string }

type Msg = UpdateText of string

let init: Model = { Text = "" }

let update (model: Model): Msg -> Model =
    function
    | UpdateText text -> { model with Text = text }

let inputBox =
    React.functionComponent
        ("InputBox",
         (fun () ->
             let (model, dispatch) = React.useReducer (update, init)
             Html.div
                 [ prop.className "text-center"
                   prop.children
                       [ Html.input
                           [ prop.onInput (fun e ->
                               !!e.target?value
                               |> UpdateText
                               |> dispatch)
                             prop.valueOrDefault model.Text ]
                         Html.div [ Html.text model.Text ] ] ]))

open Browser.Dom

ReactDOM.render (inputBox, document.getElementById "feliz-app")
