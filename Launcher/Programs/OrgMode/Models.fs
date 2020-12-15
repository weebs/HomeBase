module Launcher.Programs.OrgMode.Models

open System.IO
open Bolero

type AppMsg =
    | DirectoryChanged of FileSystemEventArgs

type AppModel =
    { filePath: string
      fileLines: string[] }