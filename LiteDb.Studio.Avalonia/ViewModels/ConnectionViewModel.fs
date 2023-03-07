namespace LiteDb.Studio.Avalonia.ViewModels

open System.Threading.Tasks
open LiteDb.Studio.Avalonia.Models
open ReactiveUI

type ConnectionViewModel() as this=
    inherit ViewModelBase()

    let ts = new TaskCompletionSource<ConnectionParameters>()
    let mutable dbFile = ""
    let mutable password = ""
    let mutable isDirect = true
    let mutable isShared = false
    let mutable initSizeInMB = 0
    let mutable isReadOnly = false
    let mutable isUpgradingFromV4 = false

    let mutable closeFunc = fun () -> ()
    let connectCommand =
        let run () =
            ts.SetResult(this.GetParameters())
            closeFunc()

        ReactiveCommand.Create(run)


    member x.Close
        with set (v: unit -> unit) = closeFunc <-v
    member x.ConnectCommand with get() = connectCommand
    member x.DbFile
        with get () = dbFile
        and set v = x.RaiseAndSetIfChanged(&dbFile, v) |> ignore

    member x.Password
        with get () = password
        and set v = x.RaiseAndSetIfChanged(&password, v) |> ignore

    member x.IsDirect
        with get () = isDirect
        and set v = x.RaiseAndSetIfChanged(&isDirect, v) |> ignore

    member x.IsShared
        with get () = isShared
        and set v = x.RaiseAndSetIfChanged(&isShared, v) |> ignore

    member x.IsReadOnly
        with get () = isReadOnly
        and set v = x.RaiseAndSetIfChanged(&isReadOnly, v) |> ignore

    member x.InitSizeInMB
        with get () = initSizeInMB
        and set v = x.RaiseAndSetIfChanged(&initSizeInMB, v) |> ignore

    member x.IsUpgradingFromV4
        with get () = isUpgradingFromV4
        and set v = x.RaiseAndSetIfChanged(&isUpgradingFromV4, v) |> ignore

    member x.SelectFileTask = ts.Task

    member x.Set(result:string[]) =
        if (result.Length > 0) then
            x.DbFile <- result[0]

    member x.GetParameters() : ConnectionParameters =
        {
          DbFile = x.DbFile
          Password = x.Password
          IsDirect = x.IsDirect
          IsShared = x.IsShared
          InitSizeInMB = x.InitSizeInMB
          IsReadOnly = x.IsReadOnly
          IsUpgradingFromV4 = x.IsUpgradingFromV4
        }
