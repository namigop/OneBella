namespace OneBella.ViewModels

open System.Collections.Generic
open LiteDB
open OneBella.Core
open OneBella.Core.Rop
open OneBella.UseCases
open ReactiveUI

type BsonItem(name: string, bVal: BValType, index: int, parent: Option<BsonItem>, tableName:string option, getLiteDb : unit -> LiteDatabase) as this =
    inherit ReactiveObject()

    let typeInfo =
        match bVal with
        | Document d ->
            let s = if d.Count > 1 then "fields" else "field"
            d.Type, $"( {d.Count} {s} )"
        | Array d ->
            let s = if d.Count > 1 then "items" else "item"
            d.Type, $"( {d.Count} {s} )"
        | Nil      d -> d.Type, $"<null>"
        | Bytes    d -> d.Type, $"{d.SizeKB} KB"
        | Bool     d -> d.Type, $"{d.Value}"
        | Decimal  d -> d.Type, $"{d.Value}"
        | Double   d -> d.Type, $"{d.Value}"
        | Guid     d -> d.Type, $"{d.Value}"
        | Int      d -> d.Type, $"{d.Value}"
        | Long     d -> d.Type, $"{d.Value}"
        | String   d -> d.Type, $"{d.Value}"
        | DateTime d -> d.Type, $"{d.Value}"
        | ObjectId d -> d.Type, $"{d.Value}"

    let children =
        match bVal with
        | Document d ->
            d.Value
            |> Seq.map (fun kvp -> BsonItem(kvp.Key, (BVal.create kvp.Value), -1, Some(this), tableName, getLiteDb))
        | Array d ->
            d.Value
            |> Seq.zip (seq { 0 .. d.Count - 1 })
            |> Seq.map (fun (i, b) -> BsonItem($"[{i}]", (BVal.create b), i, Some(this), tableName,getLiteDb))
        | _ -> Seq.empty


    let mutable isEditable =
        match parent with
        | Some p ->
            not (name = "_id")
            && p.Type = "document"
            && (BVal.findObjectId p.BsonValue).IsSome
            && match bVal with
               | String  _ -> true
               | Bool    _ -> true
               | Decimal _ -> true
               | Double  _ -> true
               | Long    _ -> true
               | Int     _ -> true
               |         _ -> false
        | None -> false

    let mutable canShowEditor = false

    let mutable isExpanded =
        match bVal with
        | Array d -> true
        | _ -> false

    let mutable value =
        let _, v = typeInfo
        v
    let mutable storedValue = value
    let canCommit () =
        isEditable && not (storedValue = value)

    let runUpdateSql() =
        let id = BVal.findObjectId parent.Value.BsonValue
        let sv = BVal.createBsonValue bVal storedValue
        let newValue = BVal.createBsonValue bVal value
        let updateUseCase = UpdateField.create getLiteDb tableName.Value name id sv newValue
        let updated = UpdateField.run updateUseCase
        if updated then
            storedValue <- value

    let editCommand =
        let onUpdateOk dbUpdated = storedValue <- value
        let run () =
            if (this.CanCommitChange) then
                runUpdateSql
                |> Rop.run
                |> Rop.inspect onUpdateOk (fun exc -> ())
                |> Rop.finish (fun _ -> ())
            this.RaisePropertyChanged(nameof this.CanCommitChange)
            this.CanShowEditor <- not (canShowEditor)
        ReactiveCommand.Create run

    member x.CanCommitChange =
        let v = canCommit ()
        v

    member x.HasChildren =
        match bVal with
        | Document d -> true
        | Array d -> true
        | _ -> false

    member x.EditCommand = editCommand

    member x.BsonValue = bVal

    member x.IsExpanded
        with get () = isExpanded
        and set v = x.RaiseAndSetIfChanged(&isExpanded, v) |> ignore

    member x.Children = children

    member x.AsJson() =
        let temp = List<BsonValue>()
        temp.Add(BVal.getRawValue bVal)
        DbUtils.toJson temp

    member x.Value
        with get () = value
        and set v =
            x.RaiseAndSetIfChanged(&value, v) |> ignore
            x.RaisePropertyChanged(nameof x.CanCommitChange)

    member x.IsEditable
        with get () = isEditable
        and set v = x.RaiseAndSetIfChanged(&isEditable, v) |> ignore
    member x.CanShowEditor
        with get () = canShowEditor
        and set v = x.RaiseAndSetIfChanged(&canShowEditor, v) |> ignore

    member x.Type =
        let t, _ = typeInfo
        t

    member x.Parent =
        match parent with
        | Some p -> p
        | None -> Unchecked.defaultof<BsonItem>

    member x.Name =
        match bVal with
        | Document d ->
            if (index > -1) then
                let ok, id = d.Value.TryGetValue("_id")
                if ok then $"[{index}] ( id = {id} )" else $"[{index}]"
            else
                name
        | _ -> name
