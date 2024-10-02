module OneBella.Core.DbUtils

open System.IO
open System.Reflection
open System.Text
open System.Threading
open System.Threading.Tasks
open LiteDB
open System.Collections.Generic

let execWithArg (db: LiteDatabase) sql arg=
    use reader = new StringReader(sql)
    db.Execute(reader, arg)

let exec (db: LiteDatabase) sql =
    execWithArg db sql (BsonDocument())
    //use reader = new StringReader(sql)
    //db.Execute(reader, BsonDocument())

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

let dispose_hack (liteDb: LiteDatabase) =
    //this is a hack to clear the MemoryCache.  We do this only after calling
    //liteDb.Dispose because we are then sure that instance will no longer be used.
    //
    //We are looking to do the following using reflection
    //  1. Call Clear() on LiteDb.LiteEngine.DiskService.MemoryCache
    //  2. Call Clear() on LiteDb.LiteEngine.DiskService.MemoryCache._free //A ConcurrentQueue<T>
    try
        liteDb.Dispose()

        //liteDb
        //    .GetType()
        //    .GetField("_engine", BindingFlags.Instance ||| BindingFlags.NonPublic)
        //    .GetValue(liteDb)
        //|> fun engineVal ->
        //    engineVal
        //        .GetType()
        //        .GetField("_disk", BindingFlags.Instance ||| BindingFlags.NonPublic)
        //        .GetValue(engineVal)
        //|> fun dsVal ->
        //    dsVal
        //        .GetType()
        //        .GetProperty("Cache").GetValue(dsVal)
        //|> fun cacheVal ->
        //    let clear = cacheVal.GetType().GetMethod("Clear")
        //    clear.Invoke(cacheVal, null) |> ignore

        //    cacheVal
        //        .GetType()
        //        .GetField("_free", BindingFlags.Instance ||| BindingFlags.NonPublic)
        //        .GetValue(cacheVal)
        //|> fun freeVal ->
        //    let clear = freeVal.GetType().GetMethod("Clear")
        //    clear.Invoke(freeVal, null) |> ignore
    with exc ->
        ()
