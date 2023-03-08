module LiteDb.Studio.Avalonia.Models.Utils

open System
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
