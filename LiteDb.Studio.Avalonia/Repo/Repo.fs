module OneBella.Repo

open LiteDB
open OneBella.Models
open OneBella.ConnectionSettings
let private db = new LiteDatabase(Utils.getAppDataPath() + $"/{Utils.appName}.db")

let getConnSettings() =
    let c = db.GetCollection<IConnectionSettings>()
    let _ = c.EnsureIndex(fun r -> r.Id)
    let ss = c.FindAll()
    let count = Seq.length ss
    Seq.toArray ss
let disconnect() = db.Dispose()
let saveConnSettings (settings:IConnectionSettings) =
    let c = db.GetCollection<IConnectionSettings>()
    let _ = c.EnsureIndex(fun r -> r.Id)
    c.Upsert(settings) |> ignore
    ()
