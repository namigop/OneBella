namespace OneBella.ViewModels

open LiteDB
open ReactiveUI


type DbFileItem(db: LiteDatabase, conString: ConnectionString) as this =
    inherit DbItem()
    let mutable liteDb = db

    let disconnect () =
        if not (liteDb = null) then
            liteDb.Dispose()
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

    member x.LiteDb = liteDb
    override x.IsConnected = not (liteDb = null)

    override x.Disconnect() = liteDb.Dispose()
    member x.ConnectionString = conString
