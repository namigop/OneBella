module OneBella.Models.DbUtils

open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks
open LiteDB
open System
open System.Collections.Generic

let exec (db: LiteDatabase) sql =
    use reader = new StringReader(sql)
    db.Execute(reader, BsonDocument())

let checkpoint (db: LiteDatabase) =
    async { do! db.Checkpoint |> Task.Run |> Async.AwaitTask }

let shrink (db: LiteDatabase) =
    async {
        let! _ = db.Rebuild |> Task.Run |> Async.AwaitTask
        ()
    }

let readResult (token: CancellationToken) (reader2: IBsonDataReader) =
    seq {
        while (not <| token.IsCancellationRequested && reader2.Read()) do
            yield reader2.Current
    }

let toJson (result: List<BsonValue>) =
    let mutable index = 0
    let sb = StringBuilder()
    use writer = new StringWriter(sb)
    let json = JsonWriter(writer, Pretty = true, Indent = 2)

    for value in result do
        if result.Count > 1 then
            index <- index + 1
            sb.AppendLine($"// {index}") |> ignore

        json.Serialize(value)
        sb.AppendLine() |> ignore

    sb.ToString()


let getDb (conStr: ConnectionString) = new LiteDatabase(conStr)

let getSystemTables (db: LiteDatabase) =
    db
        .GetCollection("$cols")
        .Query()
        .Where("type = 'system'")
        .OrderBy("name")
        .ToDocuments()

let getCollectionNames (db: LiteDatabase) = db.GetCollectionNames() |> Seq.sort

