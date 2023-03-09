namespace LiteDb.Studio.Avalonia.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open LiteDB
open LiteDb.Studio.Avalonia.Models
open LiteDb.Studio.Avalonia.Models.DbUtils
open ReactiveUI

type MainWindowViewModel() =
    inherit ViewModelBase()

    let scriptTabs = ObservableCollection<ScriptViewModel>()
    let mutable db = Unchecked.defaultof<LiteDatabase>
    let mutable selectedTab = Unchecked.defaultof<ScriptViewModel>

    member val DbItems = new ObservableCollection<DbItem>()
    member x.Tabs with get() = scriptTabs
    member x.SelectedTab
        with get() = selectedTab
        and set v = x.RaiseAndSetIfChanged(&selectedTab, v) |> ignore


    member x.Connect(con: ConnectionParameters) =
        let dbFile = con.DbFile
        let name = Path.GetFileName dbFile
        let root = DbItem(Title = name, IsExpanded = true)
        x.DbItems.Add root
        let system = DbItem(Title = "System")
        root.Children.Add system

        let liteDb = con |> buildConString |> getDb

        liteDb
        |> getSystemTables
        |> Seq.map (fun doc -> DbItem(Title = doc.["name"].AsString, IsCollection = true))
        |> Seq.iter (fun i -> system.Children.Add i)

        liteDb
        |> getCollectionNames
        |> Seq.map (fun n -> DbItem(Title = n, IsCollection = true))
        |> Seq.iter (fun n -> root.Children.Add n)

        scriptTabs.Add(new ScriptViewModel(liteDb, dbFile, "Script 1"))

        x.SelectedTab <- scriptTabs[0]
        db <- liteDb
