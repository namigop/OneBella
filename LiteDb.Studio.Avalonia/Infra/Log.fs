module OneBella.Infra.Log

open System

type LogType =
    | Info
    | Warn
    | Debug
    | Error

let private write (msg: string) = Console.WriteLine msg

let private log (l: LogType) msg =
    match l with
    | LogType.Info -> write $"INFO: {msg}"
    | LogType.Warn -> write $"WARN: {msg}"
    | LogType.Error -> write $"ERROR: {msg}"
    | LogType.Debug -> Diagnostics.Debug.WriteLine msg

let logInfo = log LogType.Info
let logError = log LogType.Error
let logExc (exc: Exception) = exc.ToString() |> logError
let logWarn = log LogType.Warn
let logDebug = log LogType.Debug
