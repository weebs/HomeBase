module Launcher.Programs.MovingLineSvg

open System
open Elmish
open Bolero.Html
open Bolero
open Microsoft.AspNetCore.Components.Web
open Myriad.Plugins

type Status = { stopRequested: bool; running: bool }
type Timer = { time: int }
type Model = { timer: Timer
               status: Status }
module Model =
    let incTime model = { model with timer = { model.timer with time = model.timer.time + 2 } }

type Msg =
    | Tick
    | StopClicked
    | StartClicked
    
let initModel _ = { timer = { time = 0 }; status = { stopRequested = false; running = false } }, Cmd.ofMsg StartClicked

let delayedMsg (msg: Msg) (ms: int) =
    fun (dispatch: Dispatch<Msg>) ->
        async {
            do! Async.Sleep ms
            dispatch msg
        } |> Async.StartImmediate
    
let update (msg: Msg) (model: Model) =
    match msg with
    | Tick ->
        if model.status.stopRequested then
            { model with
                timer = { time = 0 }
                status = {
                    stopRequested = false
                    running = false } }, []
        else
            model |> Model.incTime, Cmd.ofSub (delayedMsg Tick 7)
    | StartClicked ->
        { model with status = { model.status with running = true } }, Cmd.ofMsg Tick
    | StopClicked ->
        { model with
            status = {
                model.status with stopRequested = true } }, []

let mutable stopCount = 0
let stop () =
    stopCount <- stopCount + 1
    printfn "Stopping: %d" stopCount
    
let viewStatusPanel model dispatch =
    div [ attr.key "statusPanel" ] [
        cond model.running <| function
        | true ->
            button [ on.click (fun _ -> stop (); dispatch StopClicked); attr.key "stopButton" ] [
                text "Stop"
            ]
        | false ->
            button [ on.click (fun _ -> dispatch StartClicked) ] [
                text "Start"
            ]
    ]

type StatusPanel() =
    inherit ElmishComponent<Status, Msg>()
    override this.View model dispatch =
        viewStatusPanel model dispatch
        
let drawLines (time: int) =
    forEach [ 0..43 ] <| fun i ->
//        let stroke = sprintf "#%d%d%d" ((i * 2) + 10 + (time / 10)) ((i * 2) + 10) (77 + (time / 10))
        let stroke = sprintf "rgb(%d,%d,%d)" (255 - time) time (200 - (i * 4))
        let stroke2 = sprintf "rgb(%d,%d,%d)" time (255 - time) (77 + (i * 2))
        let height = ((float i) / 43.) * 80. |> int
        let height2 = ((float i) / 43.) * float time |> int
        concat [
            elt "line" [ "x1" => time + (i * 10) + 5
                         "y1" => time
                         "x2" => time + (i * 10) + 5
                         "y2" => height
                         "stroke" => stroke ] []
            elt "line" [ "x1" => time + (i * 10)
                         "y1" => 100
                         "x2" => time + (i * 10)
                         "y2" => 180
                         "stroke" => stroke2 ] []
        ]
        
    
type Graph() =
    inherit ElmishComponent<Timer, unit>()
    override this.View model dispatch =
        let t =
            if (model.time / 200) % 2 = 0 then
                model.time % 200
            else
                200 - (model.time % 200)
        concat [
            p [] [ code [] [ textf "%d" model.time ] ]
            svg [ attr.style "width: 100vw; height: 100vh;"; "viewbox" => "0 0 800 800" ] [
                drawLines t
            ]
        ]
        
    
let view model dispatch =
    div [ attr.style "width: 100vw; height: 100vh;" ] [
        ecomp<StatusPanel,_,_> [] model.status dispatch
//        viewStatusPanel model.status dispatch
        ecomp<Graph,_,_> [] model.timer ignore
    ]
    
type Program() =
    inherit ProgramComponent<Model, Msg>()
    
    override this.Program = Program.mkProgram initModel update view