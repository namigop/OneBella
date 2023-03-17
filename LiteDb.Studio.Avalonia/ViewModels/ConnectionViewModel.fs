namespace OneBella.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open System.Threading.Tasks
open LiteDB
open OneBella.Models
open ReactiveUI
open OneBella.Models.Utils
open OneBella.ConnectionSettings

type ConnectionViewModel(savedConnections: IConnectionSettings array)  as this =
    inherit ViewModelBase()

    let mutable ts = new TaskCompletionSource<ConnectionParameters>()

    let mutable error = ""


    let mutable selectedConItem2 = ConnectionItem(0, ConnectionString())

    let mutable closeFunc = fun () -> ()
    let canConnect() =
        let ok = not (String.IsNullOrEmpty selectedConItem2.DbFile) && File.Exists(selectedConItem2.DbFile)
        if not ok then
            this.Error <- "Please select a db file"
        ok
    let conItems = ObservableCollection<ConnectionItem>()
    do
        savedConnections
        |> Array.map (fun c ->
            let cstr = Unchecked.defaultof<ConnectionParameters>.FromStoredConnection (c)
            ConnectionItem(c.Id, cstr.ToConnectionString()))
        |> fun items -> ObservableCollection(items)
        |> fun items ->
            ConnectionItem(0, ConnectionString(Filename = "<New connection>"))
            |> fun c -> items.Insert(0, c)
            for d in items do conItems.Add d
            selectedConItem2 <- conItems[0]
    let connectCommand =
        let run () =
            if (canConnect()) then
                let p :ConnectionParameters= selectedConItem2.GetParameters()
                ts.SetResult(p)
                closeFunc ()
        ReactiveCommand.Create(run)

    member x.ConnectCommand = connectCommand


    member x.SelectFileTask = ts.Task


    member x.Close
        with set v = closeFunc <- v

    member x.SelectedConnectionItem
        with get () = selectedConItem2
        and set v = x.RaiseAndSetIfChanged(&selectedConItem2, v) |> ignore

    member x.ConnectionItems = conItems
    member x.CanConnect
        with get() =
            let a =  not (String.IsNullOrEmpty x.SelectedConnectionItem.DbFile)
            let b = File.Exists(x.SelectedConnectionItem.DbFile)
            a && b

    member x.Error
        with get () = error
        and set v =
            x.RaiseAndSetIfChanged(&error, v) |> ignore
            ts <- TaskCompletionSource<ConnectionParameters>()


    member x.Set(result: string[]) =
        if not (result = null) && (result.Length > 0) then
             x.SelectedConnectionItem.DbFile <- result[0]
