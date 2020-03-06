#r "paket:
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target 
nuget Fake.JavaScript.Yarn //"
#load ".fake/build.fsx/intellisense.fsx"

#if !FAKE
#load ".fake/build.fsx/intellisense.fsx"
#r "netstandard" // Temp fix for https://github.com/dotnet/fsharp/issues/5216
#endif

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.JavaScript

module Webpack =
    let webpack (path: string) =
        let cmd = sprintf "webpack --env.entry=%s --mode=development" path 
        Yarn.exec cmd id
    let webpackDevServer (path: string) =
        let cmd = sprintf "webpack-dev-server --env.entry=%s --mode=development" path 
        Yarn.exec cmd id


Target.initEnvironment ()

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    |> Shell.cleanDirs 
)

Target.create "YarnInstall" (fun _ ->
    Yarn.install id 
)

Target.create "Setup" ignore 
Target.create "All" ignore

"Setup"
    <== [ "Clean"; "YarnInstall" ]

//// HelloWorld ////

Target.create "BuildHelloWorld" (fun _ ->
    Webpack.webpack "./src/HelloWorld/HelloWorld.fsproj"
)

Target.create "WatchHelloWorld" (fun _ ->
    Webpack.webpackDevServer "./src/HelloWorld/HelloWorld.fsproj"
)

"Setup"
    ==> "BuildHelloWorld"

"Setup"
    ==> "WatchHelloWorld"

//// SimpleCounter ////

Target.create "BuildSimpleCounter" (fun _ ->
    Webpack.webpack "./src/SimpleCounter/SimpleCounter.fsproj"
)

Target.create "WatchSimpleCounter" (fun _ ->
    Webpack.webpackDevServer "./src/SimpleCounter/SimpleCounter.fsproj"
)

"Setup"
    ==> "BuildSimpleCounter"

"Setup"
    ==> "WatchSimpleCounter"


//// Counter ////

Target.create "BuildCounter" (fun _ ->
    Webpack.webpack "./src/Counter/Counter.fsproj"
)

Target.create "WatchCounter" (fun _ ->
    Webpack.webpackDevServer "./src/Counter/Counter.fsproj"
)

"Setup"
    ==> "BuildCounter"

"Setup"
    ==> "WatchCounter"

//// InputBox ////

Target.create "BuildInputBox" (fun _ ->
    Webpack.webpack "./src/InputBox/InputBox.fsproj"
)

Target.create "WatchInputBox" (fun _ ->
    Webpack.webpackDevServer "./src/InputBox/InputBox.fsproj"
)

"Setup"
    ==> "BuildInputBox"

"Setup"
    ==> "WatchInputBox"

//// Todos ////

Target.create "BuildTodos" (fun _ ->
    Webpack.webpack "./src/Todos/Todos.fsproj"
)

Target.create "WatchTodos" (fun _ ->
    Webpack.webpackDevServer "./src/Todos/Todos.fsproj"
)

//// EditableTodos ////

Target.create "BuildEditableTodos" (fun _ ->
    Webpack.webpack "./src/EditableTodos/EditableTodos.fsproj"
)

Target.create "WatchEditableTodos" (fun _ ->
    Webpack.webpackDevServer "./src/EditableTodos/EditableTodos.fsproj"
)

"Setup"
    ==> "BuildEditableTodos"

"Setup"
    ==> "WatchEditableTodos"

//// LocalStorageEditableTodos ////

Target.create "BuildLocalStorageEditableTodos" (fun _ ->
    Webpack.webpack "./src/LocalStorageEditableTodos/LocalStorageEditableTodos.fsproj"
)

Target.create "WatchLocalStorageEditableTodos" (fun _ ->
    Webpack.webpackDevServer "./src/LocalStorageEditableTodos/LocalStorageEditableTodos.fsproj"
)

"Setup"
    ==> "BuildLocalStorageEditableTodos"

"Setup"
    ==> "WatchLocalStorageEditableTodos"

//// FilterTodos ////

Target.create "BuildFilterTodos" (fun _ ->
    Webpack.webpack "./src/FilterTodos/FilterTodos.fsproj"
)

Target.create "WatchFilterTodos" (fun _ ->
    Webpack.webpackDevServer "./src/FilterTodos/FilterTodos.fsproj"
)

"Setup"
    ==> "BuildFilterTodos"

"Setup"
    ==> "WatchFilterTodos"

//// Docs ////

Target.create "BuildDocs" (fun _ ->
    Webpack.webpack "./docs/App/App.fsproj"
)

Target.create "WatchDocs" (fun _ ->
    Webpack.webpackDevServer "./docs/App/App.fsproj"
)

"Setup"
    ==> "BuildDocs"

"Setup"
    ==> "WatchDocs"

"All"
    <== [ "BuildHelloWorld"; "BuildSimpleCounter"; "BuildCounter"; "BuildInputBox"; "BuildTodos"; "BuildEditableTodos"; "BuildLocalStorageEditableTodos"; "BuildDocs" ]

Target.runOrDefault "Setup"
