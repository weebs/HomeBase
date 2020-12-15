module Launcher.React

open Bolero
open Bolero.Html

type Atom<'t> =
    internal { mutable x: 't }
        with
        member internal this.setState(x: 't) = this.x <- x
        member this.Value with get() = this.x
        
let (!) (a: Atom<'t>) = a.Value

type Window() =
    member this.setTitle(s: string) = ()

type React() =
    let mutable effects = []
    let mutable cleanups = []
    
    member this.ForceRerender() = ()
    
    member this.useState<'t>(x: 't) =
        let atom = { x = x }
        (atom, fun t -> atom.setState(t); this.ForceRerender())
        
    member this.useEffect(f: Window -> unit) =
        effects <- f :: effects
        
    member this.useEffect(f: Window -> unit -> unit) =
        cleanups <- f :: cleanups
        
    member this.useReducer (x: 't) (f: 't -> 'msg -> 't) =
        let atom = { x = x }
        let dispatch = fun (msg: 'msg) -> atom.setState(f !atom msg); this.ForceRerender()
        atom, dispatch
   
   
type PluggableComponent(f: unit -> Node) =
    inherit Component()
    
    override this.Render() = f ()

module React =
    let functionComponent (f: React -> 'props -> Node) =
        let react = React()
        let comp = f react

        // Todo: what should React.functionComponent return? How do we get it on the DOM?
        { new Component() with
            member this.Render() = comp (Unchecked.defaultof<'props>) }
        
    module V2 =
        let functionComponent (f: 'props -> Node) =
            ()
            
type Msg =
    | Foo
    | Bar
    
let counter = React.functionComponent <| fun react ->
    let (count, setCount) = react.useState(0)
    
    react.useEffect(fun window -> window.setTitle("Hi!"))
    
    let state, dispatch = react.useReducer "hi" <| fun state -> function
        | Foo -> "foo"
        | Bar -> "bar"
    
    fun (title: string) ->
        div [] [
            h4 [] [ text title ]
            button [ on.click (fun _ -> setCount(!count + 1)) ] [
                text "Click me!"
            ]
            textf "The count is %d" !count
            
            button [ on.click (fun _ -> dispatch Foo) ] [ text "foo" ]
            button [ on.click (fun _ -> dispatch Bar) ] [ text "bar" ]
            textf "The text is %s" !state
        ]

[<AbstractClass>]
type FuncComponent() =
    inherit Component()
    
    let mutable effects = []
    let mutable cleanups = []
    
    abstract member View: unit -> Node
    
    member this.ForceRerender() = ()
    
    member this.useState<'t>(x: 't) =
        let atom = { x = x }
        (atom, fun t -> atom.setState(t); this.ForceRerender())
        
    member this.useEffect(f: unit -> unit) =
        effects <- f :: effects
        
    member this.useEffect(f: unit -> unit -> unit) =
        cleanups <- f :: cleanups
    
    override this.Render() =
        let node = this.View ()
        for effect in effects do effect ()
        node
        
//    override this.OnAfterRender

type Test(title: string) =
    inherit FuncComponent()
    
    let (count, setCount) = base.useState(0)
    
    do
        base.useEffect(fun window -> ())
    
    override this.View() =
        div [] [
            h4 [] [ text title ]
            button [ on.click (fun _ -> setCount(!count + 1)) ] [
                text "Click me!"
            ]
            textf "The count is %d" !count
        ]


type Lens<'x, 't> = 'x -> 't * 'x -> 't -> 'x

open System

type DecayingCircle = { x: int; y: int; tDecay: int; size: int }

let tDecay =
    (fun (x: DecayingCircle) -> x.tDecay),
    (fun (x: DecayingCircle) (i: int) -> { x with tDecay = i })

let y =
    (fun (x: DecayingCircle) -> x.y),
    (fun (x: DecayingCircle) (i: int) -> { x with y = i })

let x =
    (fun (x: DecayingCircle) -> x.x),
    (fun (x: DecayingCircle) (i: int) -> { x with x = i })

type LiveDecayingCircle = { id: Guid; tStart: int; circle: DecayingCircle }

let id =
    (fun (x: LiveDecayingCircle) -> x.id), (fun (x: LiveDecayingCircle) (value: Guid) -> { x with id = value })

let tStart =
    (fun (x: LiveDecayingCircle) -> x.tStart),
    (fun (x: LiveDecayingCircle) (value: int) -> { x with tStart = value })

let circle =
    (fun (x: LiveDecayingCircle) -> x.circle),
    (fun (x: LiveDecayingCircle) (value: DecayingCircle) -> { x with circle = value })
    
    
let inline (>->) ((a,b): ('ldc -> 'dc) * ('ldc -> 'dc -> 'ldc)) ((c,d): ('dc -> 'st) * ('dc -> 'st -> 'dc)) =
    let setl ldc st =
        let dc = a ldc
        let dc' = d dc st
        b ldc dc'
    a >> c, setl
    
    
let inline (|>|) ((a,b): ('ldc -> 'dc) * ('ldc -> 'dc -> 'ldc)) ((c,d): ('dc -> 'st) * ('dc -> 'st -> 'dc)) =
    let setl ldc st =
        let dc = a ldc
        let dc' = d dc st
        b ldc dc'
    a >> c, setl
    
let inline getl ((a,_): ('ldc -> 'dc) * ('ldc -> 'dc -> 'ldc)) (ldc: 'ldc) = a ldc

let inline setl ((_,b): ('ldc -> 'dc) * ('ldc -> 'dc -> 'ldc)) (dc: 'dc) (ldc: 'ldc) = b ldc dc

let (|.) a b c = setl b c a
let (|@) a b = getl b a

let c = { x = 0; y = 0; tDecay = 1; size = 20 }
let lc = { id = Guid.NewGuid(); tStart = 0; circle = c }

let foo = lc |> getl circle |> setl tDecay 5
let bar = lc |> setl (circle >-> tDecay) 5

let moo =
      lc
      |. (circle |>| tDecay) <| 5
      |. (circle |>| y) <| 10
      |@ circle |. x <| 10