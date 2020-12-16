module Launcher.Home

open System.Diagnostics
open Bolero
open Bolero.Html
open Launcher.Programs.Models
open Launcher.Services
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Elmish
open FSharp.Data.LiteralProviders

type Page =
    | Home
    | MovingBall
    | OrgMode
    
type AppModel = { Page: Page; count: int }

type AppMsg =
    | SwitchPage of Page
    | Inc

let initModel _ = { Page = Home; count = 0 }, []

let update msg model =
    match msg with
    | SwitchPage p -> { model with Page = p }, []
    | Inc -> { model with count = model.count + 1 }, []

let view model dispatch =
    match model.Page with
    | Home -> a [ attr.href "#"; on.click (fun _ -> dispatch << SwitchPage <| OrgMode) ] [ text "Org Mode" ]
    | OrgMode ->
        concat [
            p [] [ a [ attr.href "#"; on.click (fun _ -> dispatch << SwitchPage <| Home) ] [ text "Home" ] ]
            button [ on.click (fun _ -> dispatch Inc) ] [ textf "inc: %d" model.count ]
            Node.Component(typeof<Launcher.Programs.OrgMode.Program.Component>, [], [])
        ]
    | _ -> code [] [ text "Unknown" ]

type Component() =
    inherit ProgramComponent<AppModel, AppMsg>()
    
    [<Inject>]
    member val js: IJSRuntime = null with get, set
    
    [<Inject>]
    member val fw: IFileWatcher = Unchecked.defaultof<IFileWatcher> with get, set
    
    override this.Program = Program.mkProgram initModel update view
