module SimpleCounter

open Feliz

let simpleCounter =
    React.functionComponent
        ("SimpleCounter",
         (fun () ->
             let (count, setCount) = React.useState (0)
             Html.div
                 [ prop.className "text-center"
                   prop.children
                       [ Html.text count 
                         Html.button
                           [ prop.className "btn btn-primary ml-2"
                             prop.onClick (fun _ -> setCount (count + 1)) 
                             prop.text "+" ]
                         Html.button
                             [ prop.className "btn btn-primary ml-2"
                               prop.onClick (fun _ -> setCount (count - 1)) 
                               prop.text "-" ] ] ]))

open Browser

ReactDOM.render (simpleCounter, document.getElementById "feliz-app")
