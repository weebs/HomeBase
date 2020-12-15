module Launcher.Programs.OrgMode.Models

open Bolero

type AppMsg = unit

type AppModel =
    { filePath: string }