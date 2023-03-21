module OneBella.UseCases.StoredConnUseCase

open System
open LiteDB
open OneBella.Core

type T = { Db: unit -> LiteDatabase }

let create db = { Db = db }

let loadById  id  (req:T)=
    let c = req.Db().GetCollection<ConnParamType>()
    let _ = c.EnsureIndex(fun c -> c.Id)
    c.FindById id
let loadAll req =
    let c = req.Db().GetCollection<ConnParamType>()
    let _ = c.EnsureIndex(fun c -> c.Id)
    c.FindAll()
let save  (settings:ConnParamType) (req:T) =
    let c = req.Db().GetCollection<ConnParamType>()
    let _ = c.EnsureIndex(fun r -> r.Id)
    c.Upsert(settings) |> ignore
    ()

let deleteById (id:int) (req:T) =
    let c = req.Db().GetCollection<ConnParamType>()
    let _ = c.EnsureIndex(fun r -> r.Id)
    c.Delete id |> ignore
