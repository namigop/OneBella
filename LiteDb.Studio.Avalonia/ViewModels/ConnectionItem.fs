namespace OneBella.ViewModels

open System
open System.IO
open System.Threading.Tasks
open LiteDB
open OneBella.Models
open ReactiveUI
open OneBella.Models.Utils
type ConnectionItem(id, cs:ConnectionString) =
    inherit ViewModelBase()

    let mutable dbFile = cs.Filename
    let mutable password = ""
    let mutable isDirect = cs.Connection = ConnectionType.Direct
    let mutable isShared = not isDirect
    let mutable initSizeInMB = cs.InitialSize
    let mutable isReadOnly = cs.ReadOnly
    let mutable isUpgradingFromV4 = cs.Upgrade

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



    member x.CompareOptions = getCompareOptions ()
    member x.Cultures = getCultures ()

    member x.SelectedCulture
        with get () = selectedCulture
        and set v = x.RaiseAndSetIfChanged(&selectedCulture, v) |> ignore



    member x.SelectedCompareOption
        with get () = selectedCompareOption
        and set v = x.RaiseAndSetIfChanged(&selectedCompareOption, v) |> ignore

    member x.DbFile
        with get () = dbFile
        and set v =
            x.RaiseAndSetIfChanged(&dbFile, v) |> ignore


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

    member x.GetParameters() : ConnectionParameters =
        let collation =
            if not <| String.IsNullOrWhiteSpace(selectedCulture) && not <| String.IsNullOrWhiteSpace(selectedCompareOption) then
                $"{selectedCulture}/{selectedCompareOption}"
            elif not <| String.IsNullOrWhiteSpace(selectedCulture) then
                selectedCulture
            else
                ""
        {
          Id = id
          DbFile = x.DbFile
          Password = x.Password
          IsDirect = x.IsDirect
          IsShared = x.IsShared
          InitSizeInMB = x.InitSizeInMB
          IsReadOnly = x.IsReadOnly
          IsUpgradingFromV4 = x.IsUpgradingFromV4
          Collation = collation
          }
