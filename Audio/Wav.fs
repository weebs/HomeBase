module Audio.Wav

open System

module Read =
    module string =
        let ascii (bytes: byte[]) : string =
            bytes
            |> Array.map char
            |> System.String

let readWavData (path: string) : single[] =
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
