namespace OneBella.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open System.Threading.Tasks
open LiteDB
open OneBella.Core
open ReactiveUI

type ConnectionViewModel(savedConnections: ConnParamType array) as this =
    inherit ViewModelBase()

    let mutable ts = new TaskCompletionSource<ConnParamType>()
    let mutable error = ""
    let mutable closeFunc = fun () -> ()
    let conItems = ObservableCollection<ConnectionItem>()
    let mutable selectedConItem2 = ConnectionItem(0, ConnectionString(), conItems)

    let canConnect () =
        let ok =
            not (String.IsNullOrEmpty selectedConItem2.DbFile)
            && File.Exists(selectedConItem2.DbFile)
        if not ok then
            this.Error <- "Please select a db file"
        ok

    do
        savedConnections
        |> Array.map (fun c ->
            let cstr = ConnectionParameters.buildConString c
            ConnectionItem(c.Id, cstr, conItems))
        |> fun items -> ObservableCollection(items)
        |> fun items ->
            ConnectionItem(0, ConnectionString(Filename = "<Select a db file>"), conItems, CanDelete = false)
            |> fun c -> items.Insert(0, c)

            for d in items do
                if (File.Exists d.DbFile) then
                    conItems.Add d

            if conItems.Count > 0 then
                selectedConItem2 <- conItems[0]


    let connectCommand =
        let run () =
            if (canConnect ()) then

                selectedConItem2.GetParameters() |> ts.SetResult |> closeFunc

        ReactiveCommand.Create(run)

    member x.ConnectCommand = connectCommand


    member x.SelectFileTask = ts.Task


    member x.Close
        with set v = closeFunc <- v

    member x.SelectedConnectionItem
        with get () = selectedConItem2
        and set v = x.RaiseAndSetIfChanged(&selectedConItem2, v) |> ignore

    member x.ConnectionItems = conItems

    member x.CanConnect =
        let a = not (String.IsNullOrEmpty x.SelectedConnectionItem.DbFile)
        let b = File.Exists(x.SelectedConnectionItem.DbFile)
        a && b

    member x.Error
        with get () = error
        and set v =
            x.RaiseAndSetIfChanged(&error, v) |> ignore
            ts <- TaskCompletionSource<ConnParamType>()


    member x.Set(result: string[]) =
        if not (result = null) && (result.Length > 0) then
            conItems
            |> Seq.tryFind (fun d -> d.DbFile = result[0])
            |> fun e ->
                match e with
                | Some (d) -> x.SelectedConnectionItem <- d
                | None ->
                    let connParams = x.SelectedConnectionItem.GetParameters()
                    connParams.DbFile <- result[0]
                    let cs = ConnectionParameters.buildConString connParams
                    let c = ConnectionItem(0, cs, conItems)
                    conItems.Add c
                    x.SelectedConnectionItem <- c
