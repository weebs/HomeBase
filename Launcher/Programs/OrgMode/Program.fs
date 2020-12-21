module Launcher.Programs.OrgMode.Program

open Bolero
open Bolero.Html
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
    init |> openFile "/home/dave/Documents/main.org", Cmd.ofSub sub

//let lineStyle (line: string) : Attr =
//    if Line.isSectionStart line then
//        attr.style "font-weight: bold"
//    elif Line.isTodo line then
//        attr.style "color: green"
//    else
//        attr.style ""
        
let viewLine (line: string) =
    if Line.isTodo line then
        span [] [
            code [ attr.style "color: red" ] [ text "TODO" ]
            code [] [ text <| line.Substring(4) ]
        ]
    elif Line.isDone line then
        span [] [
            code [ attr.style "color: green" ] [ text "DONE" ]
            code [] [ text <| line.Substring(4) ]
        ]
    elif Line.isSectionStart line then
        span [] [
            code [ attr.style "font-weight: bold" ] [ text line ]
        ]
    else
        span [] [ code [] [ text line ] ]
        
let viewSections (model: AppModel) (dispatch: Dispatch<AppMsg>) =
    // todo: show a sidebar of outline (* ** *** **** etc)
    table [] [
        forEach (Array.mapi (fun i v -> i, v) model.sections) <| fun (index, section) ->
            cond section.expanded <| function
            | true ->
                concat [
                    tr [ on.click (fun _ -> dispatch <| SectionClicked index) ] [
                        td [] [ code [] [ textf "%d" (fst (fst section.section)) ] ]
                        td [] [ viewLine (snd (fst section.section)) ]
                    ]
                    forEach (Array.skip 1 (snd section.section)) <| fun (i, line) ->
                        tr [] [
                            td [] [ code [] [ textf "%d" i ] ]
                            td [] [ viewLine line ]
                        ]
                ]
            | false ->
                tr [ on.click (fun _ -> dispatch <| SectionClicked index) ] [
                    td [] [ code [] [ textf "%d" (fst (fst section.section)) ] ]
                    td [] [ viewLine (snd (fst section.section)) ]
                ]
    ]
    
let view (model: AppModel) (dispatch: Dispatch<AppMsg>) =
    div [] [
        viewSections model dispatch
    ]
    
let update (msg: AppMsg) (model: AppModel) =
    match msg with
    | AppMsg.DirectoryChanged e ->
        if e.FullPath = model.filePath then
            model |> openFile model.filePath, []
        else
            model, []
    | AppMsg.SectionClicked sectionIndex ->
        let s = model.sections.[sectionIndex]
        model.sections.[sectionIndex] <- {| s with expanded = not s.expanded |}
        { model with sections = model.sections }, []

type Component() =
    inherit ProgramComponent<AppModel, AppMsg>()
    
    [<Inject>]
    member val js: IJSRuntime = null with get, set
    
    [<Inject>]
    member val fw: IFileWatcher = Unchecked.defaultof<IFileWatcher> with get, set
    
    override this.Program = Program.mkProgram (initModel this.fw) update view
