// Learn more about F# at http://fsharp.org

open Elmish
open System
open WebWindows.Blazor
open Bolero.Html
open Microsoft.Extensions.DependencyInjection
open Bolero
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Console
open WebWindows.Blazor

type D() =
    interface IDisposable with
        member this.Dispose() = ()
        
type Logger() =
    interface Microsoft.Extensions.Logging.ILogger with
        member this.IsEnabled(level) = true
        member this.Log(level, eventId, state, ex, formatter) = ()
        member this.BeginScope(state) = new D() :> IDisposable
    
        
type Factory() =
    interface Microsoft.Extensions.Logging.ILoggerFactory with
        member this.AddProvider(p) = ()
        member this.CreateLogger(name) = Logger() :> ILogger
    interface IDisposable with
        member this.Dispose() = ()
        
type Model = { count: int; random: int; xs: int[]; mutable renders: int }
type Message =
    | IncrementClicked
    | DelayedIncrementClicked
    | DelayFinished
    
    
let update (message: Message) (model: Model) =
    match message with
    | IncrementClicked -> { model with count = model.count + 1 }, []
    | DelayedIncrementClicked ->
        let sub dispatch =
            async {
                for i in 0..144 do
                    do! Async.Sleep 17
                    dispatch DelayFinished
            } |> Async.StartImmediate
        model, Cmd.ofSub sub
    | DelayFinished ->
        for i in 0..(model.xs.Length - 1) do
            model.xs.[i] <- model.xs.[i] + (model.random * model.count * 10)
        { model with random = model.random * -1 }, []
        
        
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
        table [ on.click (fun _ -> printfn "click" ) ] [
            forEach [0..19]
            <| fun y ->
                tr [] [
                    forEach [0..20]
                    <| fun x ->
                        empty
//                        let index = (y * 20) + x
//                        td [] [ code [] [ textf "[ %d ]" model.xs.[index] ] ]
                ]
        ]
    ]

let initModel _ = { count = 20; random = -1; xs = [| for i in 0..400 do yield i |]; renders = 0 }, []

type App() =
    inherit ProgramComponent<Model, Message>()
    
    override this.Program = Program.mkProgram initModel update view
        
type SimpleApp() =
    inherit Component()
    let mutable count = 0
    let mutable s = ""
    
    override this.Render() =
        let files = System.IO.Directory.GetFiles("/home/dave/Documents")
        
        Html.div [] [
            ul [] [
                forEach files <| fun f ->
                    li [] [ text f ]
            ]
            p [] [ textf "%d" count ]
            button [ on.click (fun _ -> count <- count + 1) ] [ text "+" ]
            p [] []
            input [ on.input (fun e -> s <- unbox e.Value) ]
            p [] [ code [] [ textf "text: %s" s ] ]
            Node.Component(typeof<App>, [], [])
        ]

type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices(services: IServiceCollection) =
        services
            .Remove(ServiceDescriptor(typeof<ILoggerFactory>, typeof<ConsoleLoggerProvider>))
        |> ignore
        services
            .AddSingleton<ILoggerFactory>(new Factory())
        |> ignore
    member this.Configure(app: DesktopApplicationBuilder) =
//        app.AddComponent<Purple.Burps.Client.Main.MyApp>("app")
        app.AddComponent<SimpleApp>("app")


[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
    ComponentsDesktop.Run<Startup>("Purple", "wwwroot/index.html")
    0 // return an integer exit code
