module HelloWorld

open Feliz

let helloWorld = React.functionComponent(fun () ->
    Html.div [
        prop.className "text-center"
        prop.text "Hello, World"
    ])

open Browser.Dom

ReactDOM.render(helloWorld, document.getElementById "feliz-app")