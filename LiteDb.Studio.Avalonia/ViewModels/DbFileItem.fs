namespace OneBella.ViewModels

open System.Collections.Concurrent
open System.Reflection
open LiteDB
open OneBella.Core
open ReactiveUI


type DbFileItem(db: LiteDatabase, conString: ConnectionString) as this =
    inherit DbItem()
    let mutable liteDb = db

    let disconnect () =
        if not (liteDb = null) then
            liteDb.Checkpoint()
            DbUtils.dispose_hack liteDb
            liteDb <- null
            this.RaisePropertyChanged(nameof this.IsConnected)

    let connect () =
        if (liteDb = null) then
            liteDb <- new LiteDatabase(conString)

            this.RaisePropertyChanged(nameof this.IsConnected)

    do
        let connect = DbAction(connect, Header = "connect")
        let disconnect = DbAction(disconnect, Header = "disconnect")
        this.ContextMenu.Add connect
        this.ContextMenu.Add disconnect

    member x.LiteDb with get() = liteDb
    override x.IsConnected = not (liteDb = null)
    override x.Disconnect() = disconnect()
    member x.ConnectionString = conString
