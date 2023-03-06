namespace LiteDb.Studio.Avalonia.ViewModels
open System.Collections.ObjectModel
open ReactiveUI

type DbItem() =
    inherit ViewModelBase()

    let children = new ObservableCollection<DbItem>()
    let mutable title = ""
    let mutable isExpanded = false
    let mutable isCollection = false

    member x.Children with get() = children

    member x.Title
        with get() = title
        and set(v) =  x.RaiseAndSetIfChanged(&title, v) |> ignore
    member x.IsExpanded
        with get() = isExpanded
        and set(v) =  x.RaiseAndSetIfChanged(&isExpanded, v) |> ignore
    member x.IsCollection
        with get() = isCollection
        and set(v) =  x.RaiseAndSetIfChanged(&isCollection, v) |> ignore
