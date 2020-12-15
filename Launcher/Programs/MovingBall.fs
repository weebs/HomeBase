module Launcher.MovingBall

open System
open Elmish
open Bolero.Html
open Bolero
open Microsoft.AspNetCore.Components.Web
open Myriad.Plugins

let circle = elt "circle"

type attr =
    static member stroke (s: string) = "stroke" => s

let viewCircle (x: int) (y: int) (radius: int) =
    circle [ "cx" => x; "cy" => y; "r" => radius; "stroke" => "green"; "fill" => "yellow" ] []

type DecayingCircle = { x: int; y: int; tDecay: uint; size: uint }

[<Generator.Lenses>]
type LiveDecayingCircle = { id: Guid; tStart: uint; circle: DecayingCircle }

let dcSize deltas dc =
    (int)dc.size - (int)(dc.tDecay * deltas)

let viewDecayingCircle (deltas: uint) (dc: DecayingCircle) : Node =
    let size = System.Math.Max(0, dcSize deltas dc)
    viewCircle (dc.x + int (deltas * 2u * (1u + (uint size / 44u)))) dc.y size
    
let hasDecayed tNow ldc =
    dcSize (tNow - ldc.tStart) ldc.circle <= 0
    
type Model =
    { count: int
      mutable tNow: uint
      random: int
      sum: int
      circles: Map<Guid, LiveDecayingCircle>
      mutable renders: int }
    
let cleanup (model: Model) =
    { model with
        sum = model.random * model.count * 10
        random = model.random * -1 
        circles = model.circles |> Map.filter (fun _id ldc -> not <| hasDecayed model.tNow ldc) }
    
let tick (model: Model) =
    model.tNow <- model.tNow + 1u
    cleanup model
    
let addDecayingCircle (dc: DecayingCircle) model =
    let g = Guid.NewGuid()
    let circles = model.circles.Add(g, { id = g; tStart = model.tNow; circle = dc })
    { model with circles = circles }
    
type Message =
    | IncrementClicked
    | DelayedIncrementClicked
    | DelayFinished
    | SvgClicked of MouseEventArgs
    
let update (message: Message) (model: Model) =
    match message with
    | IncrementClicked ->
        model.renders <- 0
        { model with count = model.count + 1 }, []
    | DelayedIncrementClicked ->
        let sub dispatch =
            async {
                for _ in 0..14400 do
                    do! Async.Sleep 7
                    dispatch DelayFinished
            } |> Async.StartImmediate
        model, Cmd.ofSub sub
    | DelayFinished ->
        model |> tick, []
    | SvgClicked e ->
        model |> addDecayingCircle { x = int e.ClientX; y = int e.ClientY - 400; tDecay = 1u; size = uint <| Math.Max(44., (e.ClientX / 4.) - 44.) }, []

let viewTable (model: Model) =        
    table [ on.click (fun _ -> printfn "click" ) ] [
        forEach [0..19]
        <| fun y ->
            tr [] [
                forEach [0..20]
                <| fun x ->
                    let index = (y * 20) + x
                    td [] [ code [] [ textf "[ %d ]" <| index + model.sum ] ]
            ]
    ]
        
let view (model: Model) (dispatch: Message -> unit) =
    model.renders <- model.renders + 1
    div [] [
        button [ on.click (fun _ -> dispatch IncrementClicked) ] [
            text "Increment"
        ]
        // 0 - 20
        // 21 - 40
        button [ on.click (fun _ -> dispatch DelayedIncrementClicked) ] [
            text "Increment Delayed"
        ]
        p [] [ code [] [ textf "Renders %d" model.renders ] ]
        svg [ attr.width "1080"; attr.height" 200"; on.click (dispatch << SvgClicked) ] [
//            elt "circle" [ "cx" => 20 + model.renders; "cy" => 50; "r" => 20; "stroke" => "green"; "fill" => "yellow" ] []
            forEach model.circles <| fun kv ->
                viewDecayingCircle (model.tNow - kv.Value.tStart) kv.Value.circle
        ]
    ]

let initModel _ = { count = 20; random = -1; sum = 0; renders = 0; circles = Map.empty; tNow = 0u }, []

type App() =
    inherit ProgramComponent<Model, Message>()
    
    override this.Program = Program.mkProgram initModel update view

    
type SimpleApp() =
    inherit Component()
    let mutable count = 0
    let mutable s = ""
    
    override this.Render() =
        let files = System.IO.Directory.GetFiles("/home/dave/Documents")
        
        div [] [
            ul [] [
                forEach files <| fun f ->
                    li [] [ text f ]
            ]
            p [] [ textf "%d" count ]
            button [ on.click (fun _ -> count <- count + 1) ] [ text "+" ]
            p [] []
            input [ on.input (fun e -> s <- unbox e.Value) ]
            p [] [ code [] [ textf "text: %s" s ] ]
//            Node.Component(typeof<App>, [], [])
            comp<App> [] []
        ]