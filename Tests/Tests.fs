module Tests

open Xunit
open Lib.Repl
open System

let script = """
#load "load.fsx"
"""

module Read =
    module int =

        open System

        let littleEndian (bytes: byte[])  =
            BitConverter.ToUInt32(bytes, 0)
//            (uint bytes.[0] |||
//             (uint bytes.[1]) <<< 8u |||
//             (uint bytes.[2]) <<< 16u  |||
//             (uint bytes.[3]) <<< 24u)
        let bigEndian (bytes: byte[]) : int =
            (bytes.[0] <<< 24 |||
             bytes.[1] <<< 16 |||
             bytes.[2] <<< 8 |||
             bytes.[3]) |> int
    module string =
        let ascii (bytes: byte[]) : string =
            bytes
            |> Array.map char
            |> System.String

[<Fact>]
let readWav2 () =
    let path = "/home/dave/Downloads/Glitch_5.wav"
    let bytes = System.IO.File.ReadAllBytes(path)
    let header = bytes.[0..43]
    let chunkId = Read.string.ascii header.[0..3]
    let chunkSize = BitConverter.ToInt32(header, 4) // header.[4..7]
    let format = Read.string.ascii header.[8..11]
    let fmt = Read.string.ascii header.[12..15]
    let lenFormatData = BitConverter.ToInt32(header, 16)
    let format = header.[20..21]
    let numChannels = BitConverter.ToInt16(header, 22)
    let sampleRate = BitConverter.ToInt32(header, 24)
    let ``(Sample Rate * BitsPerSample * Channels) / 8.`` = header.[28..31]
    let byteRate = BitConverter.ToInt32(header, 28)
    let ``more stuff idk`` = header.[32..33]
    let fmtBlockAlign = BitConverter.ToInt16(header, 32)
    let bitDepth = BitConverter.ToInt16(header, 34)
    let bitsPerSample = BitConverter.ToInt16(header, 34) // header.[34..35]
    // todo: data header isn't guaranteed to be at this position
    let dataHeader = Read.string.ascii header.[36..39]
    let fileSize2 = BitConverter.ToInt32(header, 40) //header.[40..43]
    let data = bytes.[44..]
    let floats: single[] = Array.init (data.Length / 4) (fun _ -> 0.f)
    Buffer.BlockCopy(data, 0, floats, 0, data.Length)
    floats
    
[<Fact>]
let readWav () =
    let path = "/home/dave/Downloads/Glitch_5.wav"
    let bytes = System.IO.File.ReadAllBytes(path)
    let header = bytes.[0..43]
    let riff = header.[0..3]
    let fileSize = header.[4..7]
    let fileType = header.[8..11]
    let fmt = header.[12..15]
    let lenFormatData = header.[16..19]
    let format = header.[20..21]
    let numChannels = header.[22..23]
    let sampleRate = header.[24..27]
    let ``(Sample Rate * BitsPerSample * Channels) / 8.`` = header.[28..31]
    let ``more stuff idk`` = header.[32..33]
    let bitsPerSample = header.[34..35]
    let dataHeader = header.[36..39]
    let fileSize2 = header.[40..43]
    let data = bytes.[44..]
    Assert.True(true)


[<Fact>]
let ``Script load: relative path translated to absolute path`` () =
    let sourceFilePath = "/home/user/projects/foo/src/mvu.fsx"
    let sourceDir = System.IO.Path.GetDirectoryName(sourceFilePath)
    let lines = script.Split(char "\n")
    let loadDirectives = lines |> Array.filter Parse.isLoadDirective
    let updatedDirectives =
        loadDirectives
        |> Array.map (Translate.scriptLoad sourceDir)
    Assert.True(true)