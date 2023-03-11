namespace OneBella.Repo
open System
type IDbSetup =
  abstract DbFile   :string
  abstract IsDirect :bool
  abstract IsShared :bool
  abstract InitSizeInMB : int64
  abstract IsReadOnly : bool
  abstract IsUpgradingFromV4 : bool
  abstract Collation :string
    


