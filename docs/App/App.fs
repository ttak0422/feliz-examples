module App

open Elmish
open Elmish.React
open Fable.Core
open Fable.Core.JsInterop
open Fable.SimpleHttp
open Feliz
open Feliz.ElmishComponents
open Feliz.Markdown
open Feliz.Router



//// Types ////

type Page =
    | Home
    | HelloWorld
    | SimpleCounter
    | Counter
    | InputBox
    | Todos
    | EditableTodos
    | LocalStorageEditableTodos
    | FilterTodos
    | ManagedTodos
    | NotFound

    member this.ToSegments(): string list =
        match this with
        | Home -> []
        | HelloWorld -> [ "helloWorld" ]
        | SimpleCounter -> [ "simpleCounter" ]
        | Counter -> [ "counter" ]
        | InputBox -> [ "inputBox" ]
        | Todos -> [ "todos" ]
        | EditableTodos -> [ "editableTodos" ]
        | LocalStorageEditableTodos -> [ "localStorageEditableTodos" ]
        | FilterTodos -> [ "filterTodos" ]
        | ManagedTodos -> [ "managedTodos" ]
        | NotFound -> [ "notfound" ]

    static member All =
        [ Home; HelloWorld; SimpleCounter; Counter; InputBox; Todos; EditableTodos; LocalStorageEditableTodos; FilterTodos; ManagedTodos ]

type Model =
    { Page: Page
      ManagedTodosModel: ManagedTodos.Model }

type Msg = 
    | PageChanged of Page
    | ManagedTodosMsg of ManagedTodos.Msg

type Highlight =
    static member inline highlight (properties: IReactProperty list): ReactElement =
        Interop.reactApi.createElement (importDefault "react-highlight", createObj !!properties)

//// State ////

let parseUrl (url: string list): Page =
    match url with
    | [] -> Home
    | [ "helloWorld" ] -> HelloWorld
    | [ "simpleCounter" ] -> SimpleCounter
    | [ "counter" ] -> Counter
    | [ "inputBox" ] -> InputBox
    | [ "todos" ] -> Todos
    | [ "editableTodos" ] -> EditableTodos
    | [ "localStorageEditableTodos" ] -> LocalStorageEditableTodos
    | [ "filterTodos" ] -> FilterTodos
    | [ "managedTodos" ] -> ManagedTodos
    | _ -> NotFound

let init(): Model * Msg Cmd = 
    { Page = Home
      ManagedTodosModel = ManagedTodos.init }, []

let update (msg: Msg) (model: Model): Model * Msg Cmd =
    match msg with
    | PageChanged page -> { model with Page = page }, []
    | ManagedTodosMsg msg -> { model with ManagedTodosModel = ManagedTodos.update model.ManagedTodosModel msg }, []



//// View ---side bar--- ////

let navBar =
    Html.nav
        [ prop.className "navbar navbar-dark bg-dark"
          prop.children
              [ Html.div
                  [ prop.className "navbar-brand mb-0 h1"
                    prop.text "feliz-examples" ] ] ]

let menuItem (page: Page) (currentPage: Page): ReactElement =
    Html.anchor
        [ prop.className
            [ true, "list-group-item list-group-item-action"
              page = currentPage, "active" ]
          prop.text (page.ToString())
          prop.style [ style.overflow.hidden ]
          prop.href (sprintf "#/%s" (String.concat "/" (page.ToSegments()))) ]

let menu (currentPage: Page): ReactElement =
    Html.aside
        [ Html.ul
            [ prop.className "list-group"
              prop.children (Page.All |> List.map (fun page -> menuItem page currentPage)) ] ]



//// View ---content--- ////

module MarkdownLoader =
    type Model =
        | Initial
        | Loading
        | Failed of string
        | Loaded of string

    type Msg =
        | StartLoading of path: string
        | Loaded of Result<string, int * string>

    let init (path: string): Model * Msg Cmd = Initial, Cmd.ofMsg (StartLoading path)

    let update (msg: Msg) (model: Model): Model * Msg Cmd =
        match msg with
        | StartLoading path ->
            let loadMarkdownAsync() =
                async {
                    let! (statusCode, responseText) = Http.get path
                    if statusCode = 200
                    then return Loaded(Ok responseText)
                    else return Loaded(Error(statusCode, responseText))
                }
            Loading, Cmd.OfAsync.perform loadMarkdownAsync () id
        | Loaded(Ok content) -> Model.Loaded content, []
        | Loaded(Error(status, err)) -> Model.Loaded(sprintf "Status %d: could not load markdown" status), []

    let renderMarkdown (content: string): ReactElement =
        Html.div
            [ prop.className "container"
              prop.children
                  [ Markdown.markdown
                      [ markdown.source content
                        markdown.escapeHtml false
                        markdown.renderers
                            [ markdown.renderers.code (fun props ->
                                Html.div
                                    [ Highlight.highlight
                                        [ prop.className "fsharp"
                                          prop.text (props.value) ] ]) ] ] ] ]

    let render (model: Model) (dispatch: Msg Dispatch): ReactElement =
        match model with
        | Initial -> Html.none
        | Loading -> Html.div [ prop.text "Loading..." ]
        | Model.Loaded content -> renderMarkdown content
        | Failed err ->
            Html.h1
                [ prop.style [ style.color.crimson ]
                  prop.text err ]

    let loadMarkdown (path: string): ReactElement =
        React.elmishComponent ("LoadMarkdown", init path, update, render, key = path)

let home: ReactElement = MarkdownLoader.loadMarkdown "https://raw.githubusercontent.com/ttak0422/feliz-examples/master/README.md"

let notFound =
    Html.div
        [ prop.className "text-center"
          prop.text "not found" ]

let content (model: Model) (dispatch: Msg Dispatch): ReactElement =
    match model.Page with
    | Home -> home
    | HelloWorld -> HelloWorld.helloWorld()
    | SimpleCounter -> SimpleCounter.simpleCounter()
    | Counter -> Counter.counter()
    | InputBox -> InputBox.inputBox()
    | Todos -> Todos.todos()
    | EditableTodos -> EditableTodos.editableTodos()
    | LocalStorageEditableTodos -> LocalStorageEditableTodos.localStorageEditableTodos
    | FilterTodos -> FilterTodos.filterTodos
    | ManagedTodos -> ManagedTodos.managedTodos(model.ManagedTodosModel, ManagedTodosMsg>>dispatch)
    | NotFound -> notFound

let render (model: Model) (dispatch: Msg Dispatch): ReactElement =
    let app =
        Html.div
            [ navBar
              Html.div
                  [ prop.children
                      [ Html.div
                          [ prop.className "row"
                            prop.children
                                [ Html.div
                                    [ prop.className "col-3"
                                      prop.children [ menu model.Page ] ]
                                  Html.div
                                      [ prop.className "col-9"
                                        prop.style [ style.padding 20 ]
                                        prop.children [ content model dispatch ] ] ] ] ] ] ]
    Router.router
        [ Router.onUrlChanged
            (parseUrl
             >> PageChanged
             >> dispatch)
          Router.application app ]

Program.mkProgram init update render
|> Program.withReactBatched "feliz-app"
|> Program.run
