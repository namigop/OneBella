namespace OneBella.ViewModels

open System
open System.Collections.Generic
open LiteDB
open OneBella.Models
open ReactiveUI

type BsonItem(name: string, bVal: BsonValue, index :int) =
    inherit ReactiveObject()
    let typeInfo =
        if bVal.IsDocument then
            let keyCount = bVal.AsDocument.Keys.Count
            let s = if keyCount > 1 then "fields" else "field"
            "document", $"( {keyCount} {s} )"
        elif bVal.IsArray then "array", $"( {bVal.AsArray.Count} items )"
        elif bVal.IsNull then "", $"<null>"
        elif bVal.IsBinary then
            let size = Convert.ToDouble(bVal.AsBinary.LongLength)/1024.0
            "bin", $"{size} KB"
        elif bVal.IsBoolean then $"bool", $"{bVal.AsBoolean}"
        elif bVal.IsDecimal then $"decimal",$"{bVal.AsDecimal}"
        elif bVal.IsDouble then $"double",$"{bVal.AsDouble}"
        elif bVal.IsGuid then $"guid",$"{bVal.AsGuid}"
        elif bVal.IsInt32 then $"int",$"{bVal.AsInt32}"
        elif bVal.IsInt64 then $"long",$"{bVal.AsInt64}"
        elif bVal.IsNumber then $"number",$"{bVal.AsDouble}"
        elif bVal.IsString then $"string",$"{bVal.AsString}"
        elif bVal.IsDateTime then $"datetime",$"{bVal.AsDateTime}"
        else
            "",""
    let children =
        if bVal.IsDocument then
            bVal.AsDocument
            |> Seq.map (fun kvp -> BsonItem(kvp.Key, kvp.Value, -1))
        elif bVal.IsArray then
            bVal.AsArray
            |> Seq.zip (seq{0..bVal.AsArray.Count-1})
            |> Seq.map (fun (i, b) -> BsonItem($"[{i}]", b, i))
        else
            Seq.empty

    let mutable isExpanded = bVal.IsArray
    member x.HasChildren with get() = bVal.IsDocument || bVal.IsArray
    member x.IsExpanded
        with get() = isExpanded
        and set v = x.RaiseAndSetIfChanged(&isExpanded, v) |> ignore
    member x.Children with get() = children
    member x.AsJson() =
        let temp =new List<BsonValue>()
        temp.Add bVal
        DbUtils.toJson temp
    member x.Value with get() =
        let _,v = typeInfo
        v
    member x.Type with get() =
        let t, _= typeInfo
        t
    member x.Name with get() =
        if (bVal.IsDocument && index > -1) then
            let ok, id = bVal.AsDocument.TryGetValue("_id")
            if ok then $"[{index}] ( id = {id} )" else $"[{index}]"
        else
            name
