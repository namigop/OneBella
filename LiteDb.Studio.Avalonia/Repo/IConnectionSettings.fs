module OneBella.ConnectionSettings
open System
type IConnectionSettings =
  abstract Id : int
  abstract DbFile   :string
  abstract IsDirect :bool
  abstract IsShared :bool
  abstract InitSizeInMB : int64
  abstract IsReadOnly : bool
  abstract IsUpgradingFromV4 : bool
  abstract Collation :string
    

let create id dbFile isDirect initSize isReadOnly isUpgrading collation =
  {
      new IConnectionSettings with
        member x.Id = id
        member x.DbFile = dbFile
        member x.IsDirect = isDirect
        member x.IsShared = not isDirect
        member x.InitSizeInMB = initSize
        member x. IsReadOnly = isReadOnly
        member x.IsUpgradingFromV4 = isUpgrading
        member x.Collation = collation
  }
