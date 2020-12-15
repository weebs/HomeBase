namespace Launcher.Services

open System
open System.Collections.Generic
open System.IO

type IFileWatcher =
    abstract CreateDirectoryWatch: string -> (FileSystemEventArgs -> unit) -> FileSystemWatcher
    
type FileWatcher() =
    let watches = Dictionary<_,_>()
    
    interface IFileWatcher with
        member this.CreateDirectoryWatch path action =
            let id = Guid.NewGuid()
            let w = new FileSystemWatcher()
            watches.Add(id, w)
            w.Path <- path
            w.Changed.Add(action)
            w.EnableRaisingEvents <- true
            w