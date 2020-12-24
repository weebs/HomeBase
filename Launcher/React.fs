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

