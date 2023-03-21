module OneBella.Repo

open LiteDB
open OneBella.Models

let private db = new LiteDatabase(Utils.getAppDataPath () + $"/{Utils.appName}.db")

let getDb () = db

let disconnect () = db.Dispose()
