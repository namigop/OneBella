namespace OneBella.ViewModels

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open System.ComponentModel
open System.Diagnostics
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks
open Avalonia.Controls
open Avalonia.Controls.Models.TreeDataGrid
open Avalonia.Threading
open LiteDB
open OneBella.Models.DbUtils
open Microsoft.FSharp.Control
open ReactiveUI

type ScriptViewModel(db: unit -> LiteDatabase, dbFile: string, name: string) as this =
    inherit ViewModelBase()

    let mutable cs = new CancellationTokenSource()
    let mutable resultDisplayTabIndex = 0
    let mutable isBusy = false
    let mutable err = ""
    let mutable json = ""
    let mutable query = "select * from DocRuleDao"
    let result = ObservableCollection<BsonItem>()
    let paging = PagingViewModel(result)
    let mutable source = Unchecked.defaultof<HierarchicalTreeDataGridSource<BsonItem>>

    do
        source <-
            let temp = new HierarchicalTreeDataGridSource<BsonItem>(paging.DisplaySource)
            temp.Columns.Add(
                HierarchicalExpanderColumn<BsonItem>(
                    //comment to prevent auto-format to 1 line
                    TemplateColumn<BsonItem>("key", "BsonItemNameSelector"),
                    (fun (x: BsonItem) -> x.Children),
                    (fun x -> x.HasChildren),
                    (fun x -> x.IsExpanded)
                )
            )
            temp.Columns.Add(TemplateColumn<BsonItem>("value", "BsonItemValueSelector", GridLength(1, GridUnitType.Star)))
            temp.Columns.Add(new TextColumn<BsonItem, string>("type", (fun b -> b.Type)))
            temp

    let beforeRunSql () =
        this.IsBusy <- true
        cs <- new CancellationTokenSource()
        json <- ""
        this.Error <- ""
        result.Clear()

    let afterRunSql bsonValues (elapsed) =
        this.IsBusy <- false
        Dispatcher.UIThread.Post(fun () ->
            for i in bsonValues do
                result.Add(BsonItem("result", i, -1, IsExpanded = true))
            if (String.IsNullOrEmpty this.Error) then
                this.ResultDisplayTabIndex <- 0
                paging.CalculatePages(elapsed)
            else
                //show the Text tab. It has the error message
                this.ResultDisplayTabIndex <- 1)



    let runSql (sql: String) =
        try
            let liteDb = db()
            if liteDb = null then
                failwith "Database is disconnected"

            use reader = exec (db()) sql
            reader |> readResult cs.Token
        with exc ->
            this.Error <- exc.ToString()
            Seq.empty

    let stopCommand =
        let run () =
            cs.Cancel()
            this.IsBusy <- false
        ReactiveCommand.Create(run)

    let execute sql =
        beforeRunSql()
        (fun () ->
            let sw = Stopwatch.StartNew()
            let bsonValues = runSql sql
            sw.Stop()
            afterRunSql bsonValues sw.Elapsed)
        |> Task.Run |> ignore

    let runCommand =
        ReactiveCommand.Create(fun () ->  execute this.Query  )

    let checkpointCommand =
        let run () = db() |> checkpoint
        ReactiveCommand.Create(fun () -> Async.StartImmediate(run ()))
    let shrinkCommand =
        let run () = db() |> shrink
        ReactiveCommand.Create(fun () -> Async.StartImmediate(run ()))
    let beginCommand =
        ReactiveCommand.Create(fun() -> execute ("BEGIN"))
        //ReactiveCommand.Create(fun () -> Async.StartImmediate(runSql ("BEGIN")))

    let rollbackCommand =
        ReactiveCommand.Create(fun() -> execute ("ROLLBACK"))
        //ReactiveCommand.Create(fun () -> Async.StartImmediate(runSql ("ROLLBACK")))

    let commitCommand =
        ReactiveCommand.Create(fun () ->(execute ("COMMIT")))

    let getQueryJson () =
        if (String.IsNullOrEmpty json) then
            if paging.DisplaySource.Count = 1 then
                json <- result.[0].AsJson()
            elif (paging.DisplaySource.Count > 1) then
                 seq{ 1..paging.DisplaySource.Count }
                 |> Seq.zip paging.DisplaySource
                 |> Seq.fold (fun (acc:StringBuilder) (b,i) -> acc.AppendLine($"// ({i})").AppendLine(b.AsJson())) (StringBuilder())
                 |> fun sb -> json <- sb.ToString()
            else
                json <- ""
        json

    let getTextDisplay() =
        if String.IsNullOrEmpty(this.Error) then
            getQueryJson()
        else
            this.Error

    member x.Header = $"{Path.GetFileName(dbFile)} - {name}"
    member x.Paging = paging
    member x.CanShowPaging = x.ResultDisplayTabIndex = 0
    member x.ResultDisplayTabIndex
        with get () = resultDisplayTabIndex
        and set v =
            let _ = x.RaiseAndSetIfChanged(&resultDisplayTabIndex, v)
            x.RaisePropertyChanged(nameof x.CanShowPaging)
            if (resultDisplayTabIndex = 1) then
                x.RaisePropertyChanged(nameof x.QueryResultText)
    member x.IsBusy
        with get () = isBusy
        and set v = x.RaiseAndSetIfChanged(&isBusy, v) |> ignore

    member x.Error
        with get () = err
        and set v = x.RaiseAndSetIfChanged(&err, v) |> ignore

    member x.BeginCommand = beginCommand
    member x.ShrinkCommand = shrinkCommand
    member x.RollbackCommand = rollbackCommand
    member x.CommitCommand = commitCommand
    member x.RunCommand = runCommand
    member x.StopCommand = stopCommand
    member x.CheckpointCommand = checkpointCommand
    member x.Source = source

    member x.Query
        with get () = query
        and set v = x.RaiseAndSetIfChanged(&query, v) |> ignore

    member x.QueryResultText = getTextDisplay ()
    //and set (v) = this.RaiseAndSetIfChanged(&x.json, v) |> ignore

    interface IDisposable with
        member x.Dispose() = source.Dispose()
