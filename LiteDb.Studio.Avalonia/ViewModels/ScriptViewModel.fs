namespace LiteDb.Studio.Avalonia.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open Avalonia.Controls
open Avalonia.Controls.Models.TreeDataGrid
open LiteDB
open LiteDb.Studio.Avalonia.Models.DbUtils
open ReactiveUI

type ScriptViewModel(db:LiteDatabase, dbFile:string, name:string) as this =
    inherit ViewModelBase()
    let mutable json = ""
    let mutable query ="select * from DocRuleDao"
    let result = ObservableCollection<BsonItem>()
    let mutable source = Unchecked.defaultof<HierarchicalTreeDataGridSource<BsonItem>>

    do source <-
            let temp = new HierarchicalTreeDataGridSource<BsonItem>(result)
            temp.Columns.Add(
                HierarchicalExpanderColumn<BsonItem>(
                    TemplateColumn<BsonItem>("key", "BsonItemNameSelector"),
                    (fun (x: BsonItem) -> x.Children),
                    (fun x -> x.HasChildren),
                    (fun x -> x.IsExpanded))
            )

            temp.Columns.Add(TemplateColumn<BsonItem>("value", "BsonItemValueSelector", GridLength(1, GridUnitType.Star)))
            temp.Columns.Add(new TextColumn<BsonItem, string>("type", (fun b -> b.Type)))
            temp

    let runCommand =
        let run () =
            (exec db this.Query)
            |> readResult
            |> fun r ->
                json <- ""
                result.Clear()
                r |> Seq.iter (fun i -> result.Add(BsonItem("result", i, -1, IsExpanded = true)))
                this.RaisePropertyChanged("QueryResultJson")
                //toJson (r)
            //|> (fun j -> this.QueryResultJson <- j)

        ReactiveCommand.Create(Action(run))

    let checkpointCommand =
        let run () =
            checkpoint db
            shrink db
        ReactiveCommand.Create(Action(run))

    let getQueryJson() =
        if (String.IsNullOrEmpty json) then
            if result.Count > 0 then
                json <- result.[0].AsJson()
            else
                json <- ""
        json

    member x.Header = $"{Path.GetFileName(dbFile)} - {name}"
    member x.RunCommand = runCommand
    member x.CheckpointCommand = checkpointCommand
    member x.Source: HierarchicalTreeDataGridSource<BsonItem> = source

    member x.Query
        with get () = query
        and set v = x.RaiseAndSetIfChanged(&query, v) |> ignore

    member x.QueryResultJson
        with get () = getQueryJson()
        //and set (v) = this.RaiseAndSetIfChanged(&x.json, v) |> ignore

    interface IDisposable with
        member x.Dispose() =
            source.Dispose()
