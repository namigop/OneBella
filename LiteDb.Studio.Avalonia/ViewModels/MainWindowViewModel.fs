namespace OneBella.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open LiteDB
open OneBella.Models
open OneBella.Models.DbUtils
open OneBella.Models.Utils
open ReactiveUI

type MainWindowViewModel()  as this=
    inherit ViewModelBase()

    let scriptTabs = ObservableCollection<ScriptViewModel>()
    let mutable db = Unchecked.defaultof<LiteDatabase>
    let mutable selectedTab = Unchecked.defaultof<ScriptViewModel>

    let openNewTab liteDb dbFile tableName=
       let scriptName = scriptTabs |> Seq.map (fun c -> c.Header)  |>getScriptName
       let tab = new ScriptViewModel(liteDb, dbFile, scriptName)
       tab.Query <- getDefaultSql tableName
       scriptTabs.Add(tab)
       this.SelectedTab <-tab


    let createTableItem liteDb dbFile tableName =
        let temp = DbItem(Title = tableName, IsCollection = true)
        let newTabAction = DbAction( (fun() -> openNewTab liteDb dbFile tableName), Header="open new tab")
        temp.ContextMenu.Add(newTabAction)

        temp


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

        let collections = getCollectionNames liteDb
        collections
        |> Seq.map (fun name -> createTableItem liteDb dbFile name)
        |> Seq.iter (fun item -> root.Children.Add item)

        if (Seq.length collections) > 0 then
            openNewTab liteDb dbFile (Seq.head collections)


        x.SelectedTab <- scriptTabs[0]
        db <- liteDb
