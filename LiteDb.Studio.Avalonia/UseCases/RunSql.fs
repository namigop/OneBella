module OneBella.UseCases.RunSql

open System
open System.Diagnostics
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Threading
open LiteDB
open OneBella.Core
open OneBella.Core.Rop
open OneBella.Core.DbUtils

type T =
    { Stopwatch: Stopwatch
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

    let rec read () =
        match reader.ReadLine() with
        | null -> sb
        | s when s.TrimStart().StartsWith("--") -> read ()
        | a ->
            sb.AppendLine(a) |> ignore
            read ()

    read().ToString()

let findTableName sql =
    let getPattern (actualSql: string) =
        if actualSql.Contains("INSERT") then
            Some @"INSERT\s+INTO\s+(?<table>\w+).*"
        elif actualSql.Contains("UPDATE") then
            Some @"UPDATE\s+(?<table>\w+).*"
        elif actualSql.Contains("DELETE") then
            Some @"DELETE\s+(?<table>\w+).*"
        elif actualSql.Contains("SELECT") then
            Some @"SELECT.*FROM\s+(?<table>\w+).*"
        else
            None


    sql
    |> removeComments
    |> fun s -> s.ToUpperInvariant().Trim()
    |> fun sql -> sql, getPattern sql
    |> fun (sql, o) ->
        match o with
        | None -> None
        | Some r ->
            let regex = Regex.Match(sql, r)
            if regex.Success then
                let g = regex.Groups.["table"]
                Some g.Value
            else
                None


let run (req: T) =
    let db = req.Db()
    req.Stopwatch.Restart()
    use reader = req.Query |> removeComments |> exec db
    let bsonValues = reader |> readResult req.Token |> Seq.map BVal.create |> Seq.toArray
    //|> Seq.toArray
    req.Stopwatch.Stop()
    bsonValues
