module Launcher.Home

open System.Diagnostics
open Bolero
open Bolero.Html
open Launcher.Programs
open Launcher.Services
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Elmish
open FSharp.Data.LiteralProviders

type Page =
    | Home
    | MovingBall
    | OrgMode
    | MovingLine
    
type AppModel = { page: Page; count: int }

type AppMsg =
    | SwitchPage of Page
    | Inc

let initModel (p: ProgramComponent<_,_>) =
    let f _ = 
        async {
            let! x = p.JSRuntime.InvokeAsync<string>("prompt", "enter something").AsTask() |> Async.AwaitTask
            do! p.JSRuntime.InvokeVoidAsync("console.log", x).AsTask() |> Async.AwaitTask
        }
    { page = Home; count = 0 }, Cmd.OfAsync.perform f () (fun _ -> Inc)

let update msg model =
    match msg with
    | SwitchPage p -> { model with page = p }, []
    | Inc -> { model with count = model.count + 1 }, []

let view model dispatch =
    match model.page with
    | Home ->
        ul [] [
            li [] [
                a [ attr.href "#"
                    on.click (fun _ -> dispatch << SwitchPage <| OrgMode) ] [
                    text "Org Mode"
                ]
            ]
            li [] [
                a [ attr.href "#"
                    on.click (fun _ -> dispatch << SwitchPage <| MovingLine) ] [
                    text "Moving Line"
                ]
            ]
        ]
    | OrgMode ->
        concat [
            p [] [ a [ attr.href "#"; on.click (fun _ -> dispatch << SwitchPage <| Home) ] [ text "Home" ] ]
            button [ on.click (fun _ -> dispatch Inc) ] [ textf "inc: %d" model.count ]
            Node.Component(typeof<Programs.OrgMode.Program.Component>, [], [])
        ]
    | MovingBall -> code [] [ text "todo: moving ball" ]
    | MovingLine -> Node.Component(typeof<MovingLineSvg.Program>, [], [])
//    | _ -> code [] [ text "Unknown" ]

type IWKJSRuntime =
    inherit IJSRuntime
    
    abstract member Invoke: identifier: string -> 't

type Component() =
    inherit ProgramComponent<AppModel, AppMsg>()
    
    [<Inject>]
    member val js: IJSRuntime = null with get, set
    
    [<Inject>]
    member val fw: IFileWatcher = Unchecked.defaultof<IFileWatcher> with get, set
    
    override this.Program =
        Program.mkProgram initModel update view
