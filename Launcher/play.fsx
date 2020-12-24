#load "projdef.fsx"

open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Bolero
open Bolero.Html
open Elmish
open System
open WebWindows.Blazor
    
let initModel _ = (), []
let update _ _ = (), []
let view _ _ =
    empty
    
type Component() =
    inherit ProgramComponent<unit, unit>()
    
    [<Inject>]
    member val js: IJSRuntime = null with get, set
    
    override this.Program =
        Program.mkProgram initModel update view

Program.Startup._T <- typeof<Component>
ComponentsDesktop.Run<Program.Startup>("Purple", "/home/dave/code/HomeBase/Launcher/wwwroot/index.html")
