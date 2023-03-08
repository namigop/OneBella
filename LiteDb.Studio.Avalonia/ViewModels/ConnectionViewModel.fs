namespace LiteDb.Studio.Avalonia.ViewModels

open System
open System.IO
open System.Threading.Tasks
open LiteDB
open LiteDb.Studio.Avalonia.Models
open ReactiveUI
open LiteDb.Studio.Avalonia.Models.Utils

type ConnectionViewModel() as this =
    inherit ViewModelBase()

    let ts = new TaskCompletionSource<ConnectionParameters>()
    let cs = new ConnectionString()


    let mutable dbFile = ""
    let mutable password = ""
    let mutable isDirect = true
    let mutable isShared = false
    let mutable initSizeInMB = 0L
    let mutable isReadOnly = false
    let mutable isUpgradingFromV4 = false

    let mutable selectedCulture =
        if not (cs.Collation = null) then
            getCultures () |> Array.find (fun x -> x = cs.Collation.Culture.Name)
        else
            ""

    let mutable selectedCompareOption =
        if not (cs.Collation = null) then
            getCompareOptions ()
            |> Array.find (fun x -> x = cs.Collation.SortOptions.ToString())
        else
            ""

    let mutable closeFunc = fun () -> ()

    let connectCommand =
        let run () =
            ts.SetResult(this.GetParameters())
            closeFunc ()
        ReactiveCommand.Create(run)


    member x.Close
        with set (v) = closeFunc <- v


    member x.CanConnect with get() = not (String.IsNullOrEmpty x.DbFile) && File.Exists(x.DbFile)
    member x.CompareOptions = getCompareOptions ()
    member x.Cultures = getCultures ()

    member x.SelectedCulture
        with get () = selectedCulture
        and set v = x.RaiseAndSetIfChanged(&selectedCulture, v) |> ignore

    member x.SelectedCompareOption
        with get () = selectedCompareOption
        and set v = x.RaiseAndSetIfChanged(&selectedCompareOption, v) |> ignore


    member x.ConnectCommand = connectCommand

    member x.DbFile
        with get () = dbFile
        and set v =
            x.RaiseAndSetIfChanged(&dbFile, v) |> ignore
            x.RaisePropertyChanged(nameof x.CanConnect)

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

    member x.Set(result: string[]) =
        if (result.Length > 0) then
            x.DbFile <- result[0]

    member x.GetParameters() : ConnectionParameters =
        let collation =
            if not <| String.IsNullOrWhiteSpace(selectedCulture) && not <| String.IsNullOrWhiteSpace(selectedCompareOption) then
                $"{selectedCulture}/{selectedCompareOption}"
            elif not <| String.IsNullOrWhiteSpace(selectedCulture) then
                selectedCulture
            else
                ""
        {
          DbFile = x.DbFile
          Password = x.Password
          IsDirect = x.IsDirect
          IsShared = x.IsShared
          InitSizeInMB = x.InitSizeInMB
          IsReadOnly = x.IsReadOnly
          IsUpgradingFromV4 = x.IsUpgradingFromV4
          Collation = collation
          }
