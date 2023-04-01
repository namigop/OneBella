module OneBella.UseCases.RunSql
open System
open System.Diagnostics
open System.IO
open System.Text
open System.Threading
open LiteDB
open OneBella.Core
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

let removeComments sql =
    use reader = new StringReader(sql)
    let sb = StringBuilder()
    let rec read() =
        match reader.ReadLine() with
        | null -> sb
        | s when s.TrimStart().StartsWith("--") -> read()
        | a ->
            sb.AppendLine(a) |> ignore
            read()

    read().ToString()

let run (req: T) =
    let go () =
        let db = req.Db()
        req.Stopwatch.Restart()
        use reader =  req.Query |> removeComments |>  exec db
        let bsonValues =
            reader
            |> readResult req.Token
            |> Seq.map BVal.create
            //|> Seq.toArray
        req.Stopwatch.Stop()
        bsonValues
    run go
