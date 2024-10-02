namespace OneBella.ViewModels

open System
open System.Collections.ObjectModel
open System.Diagnostics
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks
open Avalonia.Controls
open Avalonia.Controls.Models.TreeDataGrid
open Avalonia.Threading
open LiteDB
open Microsoft.FSharp.Core
open OneBella.Core.Rop

open OneBella.Core.DbUtils
open OneBella.Infra.Log
open Microsoft.FSharp.Control
open ReactiveUI
open OneBella.UseCases

type ScriptViewModel(db: unit -> LiteDatabase, dbFile: string, name: string) as this =
    inherit ViewModelBase()

    let mutable resultDisplayTabIndex = 0
    let mutable isBusy = false
    let logSb = StringBuilder()

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

            //temp.Columns.Add(TemplateColumn<BsonItem>("value", "BsonItemValueSelector", GridLength(1, GridUnitType.Star)))
            temp.Columns.Add(TemplateColumn<BsonItem>("value", "BsonItemValueSelector", width = GridLength(4, GridUnitType.Star)))
            temp.Columns.Add(TextColumn<BsonItem, string>("type", (fun b -> b.Type), width = GridLength(1, GridUnitType.Star)))
            temp

    let querySw, queryTimer =
        let sw = Stopwatch.StartNew()
        //use a long enough interval so that it only shows up for slow queries
        let temp = new System.Timers.Timer(Interval = 250.0, AutoReset = true)

        temp.Elapsed
        |> Observable.add (fun _ -> paging.RunInfo <- sw.Elapsed.ToString("mm\:ss\.fff"))

        sw, temp

    let info (msg: string) =
        ignore <| logSb.AppendLine msg
        logInfo msg

    let err exc =
        logExc exc
        ignore <| logSb.AppendLine(exc.ToString())

    let beforeRunSql () =
        querySw.Restart()
        queryTimer.Start()
        this.IsBusy <- true

        logSb.Clear() |> ignore
        paging.RunInfo <- ""
        result.Clear()

    let afterRunSql bsonValues =
        queryTimer.Stop()
        querySw.Stop()
        this.IsBusy <- false

        Dispatcher.UIThread.Post(fun () ->
            if not (bsonValues = null) then
                let tableName = RunSql.findTableName this.Query
                for i in bsonValues do
                    result.Add(BsonItem("result", i, -1, None, tableName, db, IsExpanded = true))

                paging.CalculatePages(querySw.Elapsed)

                if (result.Count > 0) then
                    this.ResultDisplayTabIndex <- 0
                else
                    //show the Text tab. It has the error/log message
                    this.ResultDisplayTabIndex <- 1)


    let runSql (sql: String) token =
        let uc = RunSql.create querySw sql db token
        RunSql.run uc

    let stopCommand =
        let run () = //TODO.How to cancel a long running litedb query?
            this.IsBusy <- false

        ReactiveCommand.Create(run)

    let dispose () =
        source.Dispose()
        queryTimer.Dispose()


    let execute sql =
        use cs = new CancellationTokenSource()

        beforeRunSql
        |> run
        |> log (fun _ -> info $"Executing {sql}") err
        |> map (fun _ -> runSql sql cs.Token)
        |> log (fun b -> info $"Done {sql}") err
        |> tryMapErr (fun j -> Array.empty)
        |> log (fun _ -> info $"Showing query results") err
        |> finish (fun bson -> afterRunSql bson)


    let runSqlCommand =
        let run () = execute this.Query
        ReactiveCommand.Create(fun () -> ignore (Task.Run run))

    let checkpointCommand =
        let run () =
            beforeRunSql
            |> run
            |> map (fun _ -> db ())
            |> inspect (fun _ -> info $"Executing db checkpoint") err
            |> map (fun db -> Async.StartImmediate(checkpoint db))
            |> inspect (fun _ -> info $"checkpoint done") err
            |> finish (fun _ -> afterRunSql Seq.empty)

        ReactiveCommand.Create(fun () -> ignore (Task.Run run))

    let shrinkCommand =
        let run () =
            beforeRunSql
            |> run
            |> map (fun _ -> db ())
            |> inspect (fun _ -> info $"Shrinking db") err
            |> map (fun db -> Async.StartImmediate(shrink db))
            |> inspect (fun _ -> info $"shrink done") err
            |> finish (fun _ -> afterRunSql Seq.empty)

        ReactiveCommand.Create(fun () -> ignore (Task.Run run))

    let beginCommand =
        let run () = execute "BEGIN"
        ReactiveCommand.Create(fun () -> ignore (Task.Run run))

    let rollbackCommand =
        let run () = execute "ROLLBACK"
        ReactiveCommand.Create(fun () -> ignore (Task.Run run))

    let commitCommand =
        let run () = execute "COMMIT"
        ReactiveCommand.Create(fun () -> ignore (Task.Run run))

    let getQueryJson () =
        if paging.DisplaySource.Count = 1 then
            paging.DisplaySource[ 0 ].AsJson()
        elif (paging.DisplaySource.Count > 1) then
            let pageStart, pageEnd = paging.GetCurrentPageBoundaries()

            seq { pageStart..pageEnd }
            |> Seq.zip paging.DisplaySource
            |> Seq.fold (fun (acc: StringBuilder) (b, i) -> acc.AppendLine($"// ({i})").AppendLine(b.AsJson())) (StringBuilder())
            |> fun sb ->  sb.ToString()
        else
             ""

    let getTextDisplay () =
        if result.Count > 0 then
            getQueryJson ()
        else
            logSb.ToString()

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


    member x.BeginCommand = beginCommand
    member x.ShrinkCommand = shrinkCommand
    member x.RollbackCommand = rollbackCommand
    member x.CommitCommand = commitCommand
    member x.RunCommand = runSqlCommand
    member x.StopCommand = stopCommand
    member x.CheckpointCommand = checkpointCommand
    member x.Source = source

    member x.Query
        with get () = query
        and set v = x.RaiseAndSetIfChanged(&query, v) |> ignore

    member x.QueryResultText = getTextDisplay ()
    //and set (v) = this.RaiseAndSetIfChanged(&x.json, v) |> ignore

    interface IDisposable with
        member x.Dispose() = dispose ()
