module LiteDb.Studio.Avalonia.Models.Utils

open System
open System.Collections.Generic
open System.Globalization

let getCultures() =
    CultureInfo.GetCultures(CultureTypes.AllCultures)
    |> Seq.map (fun x -> x.LCID)
    |> Seq.distinct
    |> Seq.filter (fun x ->  not(x = 4096))
    |> Seq.map (fun x -> CultureInfo.GetCultureInfo(x).Name)
    |> Seq.toArray

let getCompareOptions() =
   let names = (Enum.GetNames(typeof<CompareOptions>))
   [| "" |] |> Array.append names

let getPages (pageSize:int) (total:int) =
        let pages = Dictionary<int, (int*int)>()
        if (pageSize > total) then
            pages[0] <- (0, total-1)
        else
            let maxPageCount = (total / pageSize) + 1
            for i in 0..(maxPageCount-1) do
                let pageStart = i * pageSize
                if (pageStart < total) then
                    let pageEnd = pageStart + pageSize - 1
                    pages[i] <- (pageStart, if (pageEnd < total-1) then pageEnd else total-1)
        pages
