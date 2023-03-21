module OneBella.Models.Utils

open System
open System.Collections.Generic
open System.Globalization
open System.Runtime.InteropServices
open System.IO

let appName = "OneBella"

let getCultures () =
    CultureInfo.GetCultures(CultureTypes.AllCultures)
    |> Seq.map (fun x -> x.LCID)
    |> Seq.distinct
    |> Seq.filter (fun x -> not (x = 4096))
    |> Seq.map (fun x -> CultureInfo.GetCultureInfo(x).Name)
    |> Seq.toArray

let getCompareOptions () =
    let names = Enum.GetNames(typeof<CompareOptions>)
    [| "" |] |> Array.append names

let getPages (pageSize: int) (total: int) =
    let pages = Dictionary<int, int * int>()

    if (pageSize > total) then
        pages[0] <- (0, total - 1)
    else
        let maxPageCount = (total / pageSize) + 1

        for i in 0 .. (maxPageCount - 1) do
            let pageStart = i * pageSize

            if (pageStart < total) then
                let pageEnd = pageStart + pageSize - 1
                pages[i] <- (pageStart, (if (pageEnd < total - 1) then pageEnd else total - 1))

    pages

let getDefaultSql (tableName: string) =
    $@"SELECT * FROM {tableName} LIMIT 100

--  UPDATE {tableName}
--  SET <key0> = <exprValue0> [,<keyN> = <exprValueN>] | <newDoc>
--  [ WHERE <filterExpr> ]

--  INSERT INTO {tableName}[: {{autoIdType}}]
--  VALUES {{doc0}} [, {{docN}}]

-- DELETE {tableName} WHERE <filterExpr>
"

let getScriptName (names: seq<string>) =
    if (Seq.length names) = 0 then
        "Script 1"
    else
        names
        |> Seq.map (fun c -> c.Split(" ") |> Seq.last)
        |> Seq.map (fun c -> Convert.ToInt32 c)
        |> Seq.max
        |> (fun n -> $"Script {n + 1}")

let isWindows () =
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

let isMac () =
    RuntimeInformation.IsOSPlatform(OSPlatform.OSX)

let isLinux () =
    RuntimeInformation.IsOSPlatform(OSPlatform.Linux)

let getAppDataPath () =
    let get () =
        if isWindows () then
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            |> fun p -> Path.Combine(p, appName)
        elif isMac () then
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            |> fun p -> Path.Combine(p, appName)
        else
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            |> fun p -> Path.Combine(p, appName)

    get () |> (fun d -> Directory.CreateDirectory d) |> (fun d -> d.FullName)

let getTempPath () =
    Path.Combine(getAppDataPath (), "Temp")
    |> fun d -> Directory.CreateDirectory d
    |> fun d -> d.FullName
