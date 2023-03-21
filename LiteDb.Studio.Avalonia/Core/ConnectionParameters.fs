namespace OneBella.Core

open LiteDB
open System

type ConnParamType() =

    member val Id = 0 with get, set
    member val DbFile = "" with get, set
    member val Password = "" with get, set
    member val IsDirect = false with get, set
    member val IsShared = true with get, set
    member val InitSizeInMB = 0L with get, set
    member val IsReadOnly = true with get, set
    member val IsUpgradingFromV4 = false with get, set
    member val Collation = "" with get, set



module ConnectionParameters =

    let buildConString (p: ConnParamType) =
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
