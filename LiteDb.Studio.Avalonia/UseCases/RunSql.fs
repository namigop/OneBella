module OneBella.UseCases.RunSql
open System
open System.Diagnostics
open System.Threading
open LiteDB
open OneBella.Core.Rop
open OneBella.Core.DbUtils

type T = {
      Stopwatch: Stopwatch
      Query: string
      Db: unit -> LiteDatabase
      Token: CancellationToken }

let create sw query db token =
    { Stopwatch = sw
      Query = query
      Db = db
      Token = token }

let run (req: T) =
    let go () =
        let db = req.Db()
        req.Stopwatch.Restart()
        use reader = exec db req.Query
        let bsonValues = reader |> readResult req.Token |> Seq.toArray
        req.Stopwatch.Stop()
        bsonValues
    run go
