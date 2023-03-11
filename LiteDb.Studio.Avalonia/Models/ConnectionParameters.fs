namespace OneBella.Models

open Microsoft.CodeAnalysis
open LiteDB
open System

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

module ConnectionParameters =
    let buildConString (p: ConnectionParameters) =
        let cs = ConnectionString()

        cs.Connection <-
            if p.IsDirect then
                ConnectionType.Direct
            else
                ConnectionType.Shared

        cs.Filename <- p.DbFile
        cs.ReadOnly <- p.IsReadOnly
        cs.Upgrade <- p.IsUpgradingFromV4
        cs.InitialSize <- p.InitSizeInMB * 1024L * 1024L

        cs.Password <-
            if String.IsNullOrWhiteSpace(p.Password) then
                null
            else
                p.Password.Trim()

        cs.Collation <-
            if String.IsNullOrWhiteSpace(p.Collation) then
                null
            else
                Collation(p.Collation)
        cs

type ConnectionParameters with
    member x.ToConnectionString() = ConnectionParameters.buildConString x
     