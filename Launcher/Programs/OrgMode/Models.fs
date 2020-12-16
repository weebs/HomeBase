module Launcher.Programs.OrgMode.Models

open System.IO
open Bolero

type Line = int * string

type Section =
    { start: Line
      contents: Line[] }

module Line =
    let isSectionStart (s: string) = s.StartsWith("*")
    let isTodo (s: string) = s.StartsWith("TODO")
    let isDone (s: string) = s.StartsWith("DONE")
    
type AppMsg =
    | DirectoryChanged of FileSystemEventArgs
    | SectionClicked of sectionNumber: int

type AppModel =
    { filePath: string
      fileLines: Line[]
      sections: {| expanded: bool
                   section: Line * Line[] |}[] }

let readLines (path: string) : Line[] =
    File.ReadAllLines(path)
    |> Array.mapi (fun i s -> i, s)
    
let readSections (lines: Line[]) =
    let starts =
        lines |> Array.filter (snd >> Line.isSectionStart)
    [|
        for i in 0..(starts.Length - 2) do
            yield lines.[fst starts.[i]], lines.[(fst starts.[i])..((fst starts.[i + 1]) - 1)]
        yield lines.[fst starts.[starts.Length - 2]], lines.[(fst starts.[starts.Length - 1])..]
    |]
    
let init =
    { filePath = ""
      fileLines = [||]
      sections = [||] }
    
let openFile (path: string) (model: AppModel) =
    let fileLines = readLines path
    let sections =
        readSections fileLines
        |> Array.map (fun s -> {| expanded = false
                                  section = s |})
    { model with
        filePath = path
        fileLines = fileLines
        sections = sections }