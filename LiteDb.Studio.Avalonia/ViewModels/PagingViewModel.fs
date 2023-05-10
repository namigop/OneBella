namespace OneBella.ViewModels

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open OneBella.Models.Utils
open ReactiveUI

type PagingViewModel(source: ObservableCollection<BsonItem>) =
    inherit ViewModelBase()

    //let mutable elapsed = TimeSpan.FromSeconds(0)
    let mutable tempSource = source
    let pages = Dictionary<int, int * int>()

    let displaySource =
        let temp = ObservableCollection<BsonItem>()
        //temp.CollectionChanged |> Observable.add (fun ds -> Console.WriteLine(ds.Action))
        temp

    let mutable pageSize = 50
    let mutable runInfo = ""
    let mutable currentPage = 0

    let showPage pageNumber =
        if (pages.ContainsKey pageNumber) then
            currentPage <- pageNumber
            displaySource.Clear()
            let pageStart, pageEnd = pages[pageNumber]

            for i in pageStart..pageEnd do
                displaySource.Add tempSource.[i]

        if displaySource.Count = 1 then
            displaySource.[0].IsExpanded <- true

    let startPageCommand =
        let run () =
            currentPage <- 0
            showPage currentPage

        ReactiveCommand.Create(run)

    let nextPageCommand =
        let run () =
            if (pages.ContainsKey(currentPage + 1)) then
                currentPage <- currentPage + 1
                showPage currentPage

        ReactiveCommand.Create(run)

    let backPageCommand =
        let run () =
            if (pages.ContainsKey(currentPage - 1)) then
                currentPage <- currentPage - 1
                showPage currentPage

        ReactiveCommand.Create(run)

    let endPageCommand =
        let run () =
            currentPage <- pages.Keys |> Seq.max
            showPage currentPage

        ReactiveCommand.Create(run)

    let tryFlatten (queryResult: ObservableCollection<BsonItem>) =
        if (queryResult.Count = 0) then
            false, Seq.empty
        else
            //if the result of the query is a single key with an array, we'll flatten it
            //  - this is the case for SELECT statements
            let hasSingleDocResult = queryResult.Count = 1 && queryResult[0].Type = "document"

            let documentHasSingleArrayChild =
                Seq.length queryResult[0].Children = 1
                && (queryResult[0].Children |> Seq.head).Type = "array"

            if hasSingleDocResult && documentHasSingleArrayChild then
                let docs = queryResult[0].Children |> Seq.head |> (fun f -> f.Children)
                (Seq.length docs) > 0 , docs
            else
                false, Seq.empty

    member x.DisplaySource = displaySource
    member x.EndPageCommand = endPageCommand
    member x.StartPageCommand = startPageCommand
    member x.BackPageCommand = backPageCommand
    member x.NextPageCommand = nextPageCommand

    member x.GetCurrentPageBoundaries() =
         let pageStart, pageEnd = pages[currentPage]
         (pageStart, pageEnd)

    member x.PageSize
        with get () = pageSize
        and set v = x.RaiseAndSetIfChanged(&pageSize, v) |> ignore

    member x.RunInfo
        with get () = runInfo
        and set v = x.RaiseAndSetIfChanged(&runInfo, v) |> ignore


    member x.CalculatePages(elapsed: TimeSpan) =
        pages.Clear()
        displaySource.Clear()

        let ok, flattened = tryFlatten source

        if ok then
            tempSource.Clear()
            flattened |> Seq.iter (fun i -> tempSource.Add i)

        if (tempSource.Count > 0) then
            for kvp in getPages x.PageSize tempSource.Count do
                pages[kvp.Key] <- kvp.Value

            showPage 0

        let d = if tempSource.Count = 1 then "document" else "documents"
        x.RunInfo <- $"{tempSource.Count} {d} : " + elapsed.ToString("mm\:ss\.fff")
