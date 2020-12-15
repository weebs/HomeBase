module Launcher.Programs.OrgMode.Program

open Bolero
open Bolero.Html
open Launcher.Programs.Models
open Launcher.Services
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Models
open Elmish
open FSharp.Data.LiteralProviders

let initModel (fw: IFileWatcher) (pg: ProgramComponent<_,_>) =
//    pg.JSRuntime.InvokeAsync<unit>("console.log", "hi") |> ignore
//    pg.JSRuntime.InvokeAsync<unit>("alert", "hi") |> ignore
//    pg.JSRuntime.InvokeAsync<unit>("eval", System.IO.File.ReadAllText("/home/dave/code/BoleroWebWindow/Launcher/Javascript/common.js")) |> ignore
    pg.JSRuntime.InvokeAsync<unit>("eval", TextFile.Javascript.``common.js``.Text) |> ignore
    let sub dispatch =
        fw.CreateDirectoryWatch "/home/dave/Documents" (dispatch << AppMsg.DirectoryChanged) |> ignore
    { filePath = "/home/dave/Documents/main.org"
      fileLines = System.IO.File.ReadAllLines("/home/dave/Documents/main.org") }, Cmd.ofSub sub

let lineStyle (line: string) : Attr =
    if line.StartsWith("*") then
        attr.style "font-weight: bold"
    elif line.StartsWith("TODO") then
        attr.style "color: green"
    else
        attr.style ""
        
let viewLine (line: string) =
    match line.Split(char " ").[0] with
    | "TODO" ->
        span [] [
            code [ attr.style "color: red" ] [ text "TODO" ]
            code [] [ text <| line.Substring(4) ]
        ]
    | "DONE" ->
        span [] [
            code [ attr.style "color: green" ] [ text "DONE" ]
            code [] [ text <| line.Substring(4) ]
        ]
        
    | _ ->
        code [ lineStyle line ] [ text line ]
        
let view (model: AppModel) _ =
    // todo: show a sidebar of outline (* ** *** **** etc)
    div [] [
        forEach model.fileLines <| fun line ->
            p [] [
                viewLine line
            ]
    ]
    
let update (msg: AppMsg) (model: AppModel) =
    match msg with
    | AppMsg.DirectoryChanged e ->
        if e.FullPath = model.filePath then
            { model with fileLines = System.IO.File.ReadAllLines(model.filePath) }, []
        else
            model, []

type Component() =
    inherit ProgramComponent<AppModel, AppMsg>()
    
    [<Inject>]
    member val js: IJSRuntime = null with get, set
    
    [<Inject>]
    member val fw: IFileWatcher = Unchecked.defaultof<IFileWatcher> with get, set
    
    override this.Program = Program.mkProgram (initModel this.fw) update view
