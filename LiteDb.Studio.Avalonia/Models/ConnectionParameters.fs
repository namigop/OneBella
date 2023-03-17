namespace OneBella.Models

open Microsoft.CodeAnalysis
open LiteDB
open System
open OneBella.ConnectionSettings

type ConnectionParameters =
    {
        Id : int
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
    member x.FromStoredConnection (con : IConnectionSettings) =
        {
            Id = con.Id
            DbFile = con.DbFile
            Password = ""
            IsDirect = con.IsDirect
            IsShared = con.IsShared
            InitSizeInMB = con.InitSizeInMB
            IsReadOnly = con.IsReadOnly
            IsUpgradingFromV4 = con.IsUpgradingFromV4
            Collation = con.Collation
        }
