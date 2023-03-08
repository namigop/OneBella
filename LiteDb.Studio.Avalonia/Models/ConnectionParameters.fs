namespace LiteDb.Studio.Avalonia.Models

open Microsoft.CodeAnalysis

type ConnectionParameters =
    {
        DbFile   :string
        Password :string
        IsDirect :bool
        IsShared :bool
        InitSizeInMB : int64
        IsReadOnly : bool
        IsUpgradingFromV4 : bool
        Collation :string
    }
