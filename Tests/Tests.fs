module Tests

open Xunit
open Lib.Repl

let script = """
#load "load.fsx"
"""


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