module Launcher.LensTest

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
      |@ circle
      |. x <| 10
      
let moo2 =
    lc
    |> setl (circle |>| tDecay) 5
    |> setl (circle |>| y) 10
    |> getl circle
    |> setl x 10
