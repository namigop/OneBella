namespace OneBella.ViewModels

open System.Collections.Generic
open LiteDB
open OneBella.Core
open ReactiveUI

type BsonItem(name: string, bVal: BValType, index: int) =
    inherit ReactiveObject()

    let typeInfo =
        match bVal with
        | Document d ->
            let s = if d.Count > 1 then "fields" else "field"
            d.Type, $"( {d.Count} {s} )"
        | Array d ->
            let s = if d.Count > 1 then "items" else "item"
            d.Type, $"( {d.Count} {s} )"
        | Nil d -> d.Type, $"<null>"
        | Bytes d -> d.Type, $"{d.SizeKB} KB"
        | Bool d -> d.Type, $"{d.Value}"
        | Decimal d -> d.Type, $"{d.Value}"
        | Double d -> d.Type, $"{d.Value}"
        | Guid d -> d.Type, $"{d.Value}"
        | Int d -> d.Type, $"{d.Value}"
        | Long d -> d.Type, $"{d.Value}"
        | String d -> d.Type, $"{d.Value}"
        | DateTime d -> d.Type, $"{d.Value}"

    let children =
        match bVal with
        | Document d -> d.Value |> Seq.map (fun kvp -> BsonItem(kvp.Key, (BVal.create kvp.Value), -1))
        | Array d ->
            d.Value
            |> Seq.zip (seq { 0 .. d.Count - 1 })
            |> Seq.map (fun (i, b) -> BsonItem($"[{i}]", (BVal.create b), i))
        | _ -> Seq.empty




    let mutable isExpanded =
        match bVal with
        | Array d -> true
        | _ -> false

    member x.HasChildren =
        match bVal with
        | Document d -> true
        | Array d -> true
        | _ -> false

    member x.IsExpanded
        with get () = isExpanded
        and set v = x.RaiseAndSetIfChanged(&isExpanded, v) |> ignore

    member x.Children = children

    member x.AsJson() =
        let temp = List<BsonValue>()
        temp.Add(BVal.getRawValue bVal)
        DbUtils.toJson temp

    member x.Value =
        let _, v = typeInfo
        v

    member x.Type =
        let t, _ = typeInfo
        t

    member x.Name =
        match bVal with
        | Document d ->
            if (index > -1) then
                let ok, id = d.Value.TryGetValue("_id")
                if ok then $"[{index}] ( id = {id} )" else $"[{index}]"
            else
                name
        | _ -> name
