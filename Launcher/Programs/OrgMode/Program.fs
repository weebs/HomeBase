module Launcher.Programs.OrgMode.Program

open Bolero
open Bolero.Html
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Models
open Elmish
open FSharp.Data.LiteralProviders

let initModel (pg: ProgramComponent<_,_>) =
//    pg.JSRuntime.InvokeAsync<unit>("console.log", "hi") |> ignore
//    pg.JSRuntime.InvokeAsync<unit>("alert", "hi") |> ignore
//    pg.JSRuntime.InvokeAsync<unit>("eval", System.IO.File.ReadAllText("/home/dave/code/BoleroWebWindow/Launcher/Javascript/common.js")) |> ignore
    pg.JSRuntime.InvokeAsync<unit>("eval", TextFile.Javascript.``common.js``.Text) |> ignore
    { filePath = "/home/dave/Documents/main.org" }, []

let view (model: AppModel) _ =
    div [] [
        forEach (System.IO.File.ReadAllLines(model.filePath)) <| fun line ->
            p [] [
                code [] [ text line ]
            ]
    ]
    
let update msg model = model, []

type Component() =
    inherit ProgramComponent<AppModel, AppMsg>()
    
    [<Inject>]
    member val js: IJSRuntime = null with get, set
    
    override this.Program = Program.mkProgram initModel update view
