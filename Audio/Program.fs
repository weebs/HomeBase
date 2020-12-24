module Audio.Program
// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

// Define a function to construct a message to print
open System
open System.Net.Http.Headers
open SoundIOSharp

let from whom =
    sprintf "from %s" whom
    
let wavData = Wav.readWavData "/home/dave/Downloads/Glitch_5.wav"

let mutable wavPos = 0
    
module libsoundio =
    let devices (api: SoundIO) =
        [| 0..(api.OutputDeviceCount - 1) |]
        |> Array.map api.GetOutputDevice
        
let inline write_sample (ptr: IntPtr) (sample: float) =
    let buf: nativeptr<single> = NativeInterop.NativePtr.ofNativeInt ptr
    NativeInterop.NativePtr.write buf (single sample)
    
let mutable seconds_offset = 0.
    
let write_sine_callback (outStream: SoundIOOutStream) (min: int) (max: int) : unit =
    printfn "Min: %d, Max: %d" min max
    let floatSampleRate = outStream.SampleRate
    let secondsPerFrame = 1.0 / float floatSampleRate
    let mutable framesLeft = max
    let layout = outStream.Layout
    while framesLeft > 0 do
        let mutable frameCount = framesLeft
        let areas = outStream.BeginWrite(&frameCount)
        printfn "Frame Count: %d" frameCount
        if frameCount > 0 then
            let pitch = 220.0
            let radiansPerSecond = pitch * 2.0 * Math.PI
            let mutable frame = 0
            while frame < frameCount do
                let sample = Math.Sin((seconds_offset + float (frame + 1)) * secondsPerFrame * radiansPerSecond)
//                printfn "%f" sample
                let mutable channel = 0
                while channel < layout.ChannelCount do
                    let area = areas.GetArea(channel)
                    let ptr = IntPtr.Add(area.Pointer, + (area.Step * frame))
                    write_sample ptr (float sample)
//                    area.IncPointer(area.Step)
                    channel <- channel + 1
                frame <- frame + 1
            outStream.EndWrite()
            seconds_offset <- Math.IEEERemainder(seconds_offset + secondsPerFrame + float frameCount, 1.0)
            framesLeft <- framesLeft - frameCount
        else
            printfn "done"
            framesLeft <- 0
        printfn "done"
    
let write_callback (outStream: SoundIOOutStream) (min: int) (max: int) : unit =
    let secondsPerFrame = 1.0 / float outStream.SampleRate
    let mutable framesLeft = max
    let mutable frameCount = max
    while frameCount > 0 && framesLeft > 0 do
        let results = outStream.BeginWrite(&frameCount)
        if frameCount <> 0 then
            let pitch = 440.0
            let radiansPerSecond = pitch * 2.0 * Math.PI
            for frame in 0..(frameCount - 1) do
                // todo: seconds_offset?
                for channel in 0..(outStream.Layout.ChannelCount - 1) do
                    let sample = Math.Sin(float frame * secondsPerFrame * radiansPerSecond)
                    let sample =
                        if wavPos < wavData.Length then
                            wavData.[wavPos]
                        else
                            0.f
                    if wavPos = wavData.Length * 2 
                    then wavPos <- 0
                    else wavPos <- wavPos + 1
                    let area = results.GetArea(channel)
                    write_sample area.Pointer (float sample)
                    area.IncPointer(area.Step)
            outStream.EndWrite()
            framesLeft <- framesLeft - frameCount
            frameCount <- framesLeft
                
let underflow_callback outStream =
    printfn "underflow"

[<EntryPoint>]
let main argv =
    let message = from "F#" // Call the function
    printfn "Hello world %s" message
    
    use api = new SoundIO()
    api.Connect()
    api.FlushEvents()
    let devices = libsoundio.devices api
    let defaultDevice = api.GetOutputDevice(api.DefaultOutputDeviceIndex)
    let outStream = defaultDevice.CreateOutStream()
//    outStream.WriteCallback <- Action<_,_>(write_callback outStream)
    outStream.WriteCallback <- Action<_,_>(write_sine_callback outStream)
    outStream.UnderflowCallback <- Action(fun _ -> underflow_callback outStream)
    if defaultDevice.SupportsFormat SoundIODevice.Float32NE then
        outStream.SoftwareLatency <- 2.
        outStream.Format <- SoundIODevice.Float32NE
        outStream.Open()
        outStream.Start()
        api.FlushEvents()
        let rec loop k =
            match string k with
            | "q" ->
                outStream.Dispose()
                defaultDevice.RemoveReference()
                api.Dispose()
                0
            | _ ->
                api.FlushEvents()
                loop (Console.ReadKey())
        loop (Console.ReadKey())
    else
        failwith "todo: implement other formats besides Float32NE"