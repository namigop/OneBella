namespace OneBella.ViewModels

open ReactiveUI

type DbAction(run: unit -> unit) =
    inherit ViewModelBase()
    let mutable header = ""

    let cmd = ReactiveCommand.Create run

    member x.Command = cmd

    member x.Header
        with get () = header
        and set v = x.RaiseAndSetIfChanged(&header, v) |> ignore
