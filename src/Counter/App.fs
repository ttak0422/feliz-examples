module Counter

open Feliz

type Model =
    { Counters: int list }

type Msg =
    | Increment of int
    | Decrement of int
    | Remove of int
    | AddCounter

let init: Model = { Counters = [ 0 ] }

let updateAt (idx: int) (value: 'a): 'a list -> 'a list =
    List.indexed
    >> List.map (fun (i, v) ->
        if i = idx then value else v)

let removeAt (idx: int): 'a list -> 'a list =
    List.indexed
    >> List.where (fun (i, _) -> i <> idx)
    >> List.map (fun (_, v) -> v)

let update (model: Model): Msg -> Model =
    function
    | Increment idx -> { model with Counters = model.Counters |> updateAt idx (model.Counters.[idx] + 1) }
    | Decrement idx -> { model with Counters = model.Counters |> updateAt idx (model.Counters.[idx] - 1) }
    | Remove idx -> { model with Counters = model.Counters |> removeAt idx }
    | AddCounter -> { model with Counters = model.Counters @ [ 0 ] }

let counter =
    React.functionComponent
        ("Counter",
         (fun () ->
             let (model, dispatch) = React.useReducer (update, init)

             let counterView (idx: int, count: int): ReactElement =
                 Html.div
                     [ prop.className "mb-2"
                       // 動作しない
                       prop.children
                           [ Html.text count 
                             Html.button
                                 [ prop.className "btn btn-primary ml-2"
                                   prop.onClick (fun _ -> dispatch <| Increment idx)
                                   prop.text "+" ]
                             Html.button
                                 [ prop.className "btn btn-primary ml-2"
                                   prop.onClick (fun _ -> dispatch <| Decrement idx)
                                   prop.text "-" ]
                             Html.button
                                 [ prop.className "btn btn-primary ml-2"
                                   prop.onClick (fun _ -> dispatch <| Remove idx)
                                   prop.text "X" ] ] ]

             Html.div
                 [ prop.className "text-center"
                   prop.children
                       [ Html.div
                           [ prop.className "mb-2"
                             prop.children
                                 [ Html.button
                                     [ prop.className "btn btn-primary"
                                       prop.onClick (fun _ -> dispatch AddCounter)
                                       prop.text "Add Counter" ] ] ]
                         Html.div
                             (model.Counters
                              |> List.indexed
                              |> List.map counterView) ] ]))

open Browser.Dom

ReactDOM.render (counter, document.getElementById "feliz-app")
