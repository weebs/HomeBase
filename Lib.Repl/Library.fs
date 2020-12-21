module Lib.Repl

module Models =
    type RelativePath = string
    type AbsolutePath = string
    type Path = string

open Models

module Translate =
    let hello name =
        printfn "Ola %s" name
    
    let scriptLoad (basePath: AbsolutePath) (loadLine: string) : string =
        let line = loadLine.Trim()
        let start = line.IndexOf("#load")
        let pathStart = line.Substring(start + 1, line.Length - start - 1).IndexOf("\"")
        let pathEnd = line.Substring(pathStart + 1, line.Length - pathStart - 1).IndexOf("\"")
        sprintf "#load \"%s\"" <| line.Substring(pathStart, pathEnd - pathStart)
        
module Parse =
    let isDirective (line: string) = line.StartsWith("#")
    let isLoadDirective (line: string) = line.StartsWith("#load")
    
