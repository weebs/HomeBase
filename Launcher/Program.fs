module Program
// Learn more about F# at http://fsharp.org

open Elmish
open System
open Launcher
open Launcher.Programs.Models
open Launcher.Services
open WebWindows.Blazor
open Bolero.Html
open Microsoft.Extensions.DependencyInjection
open Bolero
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Logging.Console
open WebWindows.Blazor
open FSharp.Data.LiteralProviders

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
type Startup() =

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    static member val _T = typeof<Home.Component> with get, set
    member this.ConfigureServices(services: IServiceCollection) =
        services
            .Remove(ServiceDescriptor(typeof<ILoggerFactory>, typeof<ConsoleLoggerProvider>))
        |> ignore
        services
            .AddSingleton<ILoggerFactory>(new Factory())
            .AddSingleton<Foo>(Foo())
            .AddSingleton<IFileWatcher>(FileWatcher())
        |> ignore
    member this.Configure(app: DesktopApplicationBuilder) =
        app.AddComponent(Startup._T, "app")
//        app.AddComponent<Purple.Burps.Client.Main.MyApp>("app")
//        app.AddComponent<MovingBall.SimpleApp>("app")


[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"
//    ComponentsDesktop.Run<Startup>("Purple", "wwwroot/index.html")

    ComponentsDesktop.RunWithStringContent<Startup>("Purple", TextFile.wwwroot.``index.html``.Text)
    0 // return an integer exit code
