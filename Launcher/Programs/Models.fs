module Launcher.Programs.Models

type Foo() =
    let mutable count = 0
    member this.X () =
        let c = count
        count <- c + 1
        c