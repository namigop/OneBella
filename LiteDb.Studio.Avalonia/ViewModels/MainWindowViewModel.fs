namespace OneBella.ViewModels

open System.Collections.ObjectModel
open System.IO
open LiteDB
open OneBella.Core
open OneBella.Core.DbUtils
open OneBella.Models.Utils
open ReactiveUI

type MainWindowViewModel() as this =
    inherit ViewModelBase()

    let scriptTabs = ObservableCollection<ScriptViewModel>()
    let mutable selectedTab = Unchecked.defaultof<ScriptViewModel>

    let openNewTab getLiteDb dbFile tableName =
        let scriptName = scriptTabs |> Seq.map (fun c -> c.Header) |> getScriptName
        let tab = new ScriptViewModel(getLiteDb, dbFile, scriptName)
        tab.Query <- getDefaultSql tableName
        scriptTabs.Add(tab)
        this.SelectedTab <- tab


    let createTableItem getLiteDb dbFile tableName =
        let temp = DbItem(Title = tableName, IsCollection = true)

        let newTabAction =
            DbAction((fun () -> openNewTab getLiteDb dbFile tableName), Header = "open new tab")

        temp.ContextMenu.Add(newTabAction)

        temp

    let openNewTabCommand =
        let run () =
            let root: DbItem = this.DbItems |> Seq.head
            let f = root :?> DbFileItem
            openNewTab (fun () -> f.LiteDb) f.ConnectionString.Filename "<Todo>"

        ReactiveCommand.Create(run)

    member x.OpenNewTabCommand = openNewTabCommand
    member val DbItems = ObservableCollection<DbItem>()
    member x.Tabs = scriptTabs

    member x.SelectedTab
        with get () = selectedTab
        and set v = x.RaiseAndSetIfChanged(&selectedTab, v) |> ignore


    member x.Connect(con: ConnParamType) =
        let dbFile = con.DbFile
        let conString = ConnectionParameters.buildConString con

        let liteDb = getDb conString

        let name = Path.GetFileName dbFile
        let root = DbFileItem(liteDb, conString, Title = name, IsExpanded = true)

        let system = DbItem(Title = "System")
        root.Children.Add system
        let getLiteDb = fun () -> root.LiteDb

        liteDb
        |> getSystemTables
        |> Seq.map (fun doc -> DbItem(Title = doc.["name"].AsString, IsCollection = true))
        |> Seq.iter (fun i -> system.Children.Add i)

        let collections = getCollectionNames liteDb

        collections
        |> Seq.map (fun name -> createTableItem getLiteDb dbFile name)
        |> Seq.iter (fun item -> root.Children.Add item)

        if (Seq.length collections) > 0 then
            openNewTab getLiteDb dbFile (Seq.head collections)
        else
            openNewTab getLiteDb dbFile (Seq.head system.Children).Title

        x.DbItems.Add root
        x.SelectedTab <- scriptTabs[0]
