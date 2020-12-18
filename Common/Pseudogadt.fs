module Pseudogadt

// open System

module Response =
    type T = private Response of obj
    let internal create o = Response o
    let internal get (Response o) = o
    
type Response = Response.T

module Return =
    type T<'a> = private Return of 'a
    let internal create t = Return t
    let value (_: T<'a>) (value: 'a) = Response.create value
    
type Return<'a> = ('a -> Response.T)

module Request = 
    type T<'input, 'ret> = private Request of 'input * 'ret
    let internal create input = Request (input, Unchecked.defaultof<'ret>)
    
type Router<'a> = ('a -> Response.T)

type Msg =
    | A of int * Return<string>
    | B of {| a: int; b: int |} * Return<decimal>

let inline get (router: Router<'a>) (request: ('input * Return<'ret>) -> 'a) (input: 'input) : 'ret =
    let f ret = Response.create ret
    let msg = request (input, f)
    let response = router msg
    (Response.get response) :?> 'ret
    
let router (msg: Msg) : Response.T =
    match msg with
    | A (i, ret) ->
        [ 0..i ] |> sprintf "%A" |> ret
    | B (r, ret) ->
        (decimal r.a) * (decimal r.b) |> ret

let main argv =
    printfn "Hello World from F#!"
    let s = get router Msg.A 5
    let z = get router Msg.B {| a = 5; b = 22 |}
    printfn "s is %s" s
    printfn "z is %M" z
    0 // return an integer exit code
