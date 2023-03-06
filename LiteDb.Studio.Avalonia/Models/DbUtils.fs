module LiteDb.Studio.Avalonia.Models.DbUtils

open System.IO
open System.Text
open LiteDB
open System.Collections.Generic

let exec (db:LiteDatabase) sql : IBsonDataReader =
    use reader = new StringReader(sql)
    db.Execute(reader, BsonDocument())

let checkpoint (db:LiteDatabase) =
    db.Checkpoint()
let shrink (db:LiteDatabase) =
    db.Rebuild() |> ignore

let readResult (reader2:IBsonDataReader) =
    let rec read (reader:IBsonDataReader) (result:List<BsonValue>) =
        let ok = reader.Read()
        if (ok) then
            result.Add reader.Current
            read reader result
        else
            result

    let res = List<BsonValue>()
    read reader2 res

let toJson (result : List<BsonValue>) =
    let mutable index = 0
    let sb = StringBuilder()
    use writer = new StringWriter(sb)
    let json = JsonWriter(writer, Pretty = true, Indent =2)
    for value in result do
        if result.Count > 1 then
            index <- index + 1
            sb.AppendLine($"/* {index} */")  |> ignore
        json.Serialize(value)
        sb.AppendLine() |> ignore
    sb.ToString()


let getDb (conStr:string) = new LiteDatabase(conStr)
let getSystemTables (db:LiteDatabase) =  db.GetCollection("$cols").Query().Where("type = 'system'").OrderBy("name").ToDocuments();
let getCollectioNames (db:LiteDatabase) =  db.GetCollectionNames() |> Seq.sort
