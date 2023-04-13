module OneBella.UseCases.UpdateField

open System.Diagnostics
open System.Reflection.PortableExecutable
open System.Threading
open LiteDB
open OneBella.Core


type T =
    { BsonId: BsonValue option
      Field: string
      TableName: string
      NewValue: BsonValue
      OldValue: BsonValue
      Db: unit -> LiteDatabase }


let create db table field id oldValue newValue =
    { Db = db
      Field = field
      TableName = table
      OldValue = oldValue
      NewValue = newValue
      BsonId = id }


let run (req: T) =
    match req.BsonId with
    | None -> false
    | Some id ->

        use cs = new CancellationTokenSource()
        let updateSql = $"UPDATE {req.TableName} SET {req.Field} = @0 WHERE _id = @1 AND {req.Field} = @2"

        let arg = BsonDocument()
        arg["0"] <- req.NewValue
        arg["1"] <- id
        arg["2"] <- req.OldValue
        let db = req.Db()
        let result = (DbUtils.execWithArg db updateSql arg) |>   DbUtils.readResult cs.Token |> Seq.head
        let updated = result.RawValue = 1
        updated
