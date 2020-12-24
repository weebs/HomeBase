module Launcher.Programs.AudioView

open System
open Bolero.Html

let sampleToY (height: int) (f: single) =
    let half = (single height) / 2.0f
    int ((half * f) + half)

let normalizeY (height: int) (y: int) =
    height - y
    
let showWavForm (width: int) (height: int) (gain: single) (data: single[])=
    let xFactor = (data.Length / width)
    let points = [
        for i in 0..width do
            yield i, (data.[i * xFactor] * gain)
        ]
    let pointsAttr =
        points
        |> List.map (fun (a, b) -> sprintf "%d,%d" a (sampleToY height b))
        |> fun xs -> String.Join(" ", xs)
        
    div [] [
        textf "Waveform Length: %d" (data.Length / 2)
        div [] [
            svg [ attr.style (sprintf "width: %dpx; height: %dpx" width height)
                  "viewbox" => sprintf "%d %d %d %d" width height width height  ] [
                elt "polyline"
                    [ "fill" => "none"
                      "stroke" => "#0074d9"
                      "stroke-width" => "1"
                      "points" => pointsAttr ]
                    []
            ]
        ]
    ]
