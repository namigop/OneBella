namespace OneBella.ViewModels
open System.Collections.ObjectModel
open LiteDB
open ReactiveUI

type DbItem() =
    inherit ViewModelBase()

    let contextMenu = ObservableCollection<DbAction>()
    let children = new ObservableCollection<DbItem>()
    let mutable title = ""
    let mutable isExpanded = false
    let mutable isCollection = false

    member x.Children with get() = children

    member x.ContextMenu with get() = contextMenu
    member x.Title
        with get() = title
        and set(v) =  x.RaiseAndSetIfChanged(&title, v) |> ignore
    member x.IsExpanded
        with get() = isExpanded
        and set(v) =  x.RaiseAndSetIfChanged(&isExpanded, v) |> ignore
    member x.IsCollection
        with get() = isCollection
        and set(v) =  x.RaiseAndSetIfChanged(&isCollection, v) |> ignore

    abstract IsConnected : bool
    default x.IsConnected with get() = true
