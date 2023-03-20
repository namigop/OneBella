module OneBella.Core.Rop

open System

type Res<'T>(v: 'T, err: Exception) =
    member x.Value = v
    member x.Error = err
    member x.IsValid = err = null

let ok v = Res(v, Unchecked.defaultof<Exception>)
let failed (exc: Exception) = Res<'a>(Unchecked.defaultof<'a>, exc)

let bind fn (res: Res<'S>) =
    if res.IsValid then
        try
            fn (res.Value)
        with exc ->
            failed exc
    else
        failed res.Error


let run1 (fn: 'S -> 'T) (arg1: 'S) =
    try
        arg1 |> fn |> ok
    with exc ->
        failed exc

let run (fn: unit -> 'T) = run1 fn Unchecked.defaultof<unit>

let run2 (fn: 'a -> 'b -> 'c) (arg1: 'a) arg2 =
    run1 (fun (a, b) -> fn a b) (arg1, arg2)

let run3 (fn: 'a -> 'b -> 'c -> 'd) arg1 arg2 arg3 =
    run1 (fun (a, b, c) -> fn a b c) (arg1, arg2, arg3)

let inspect onOk onFailed (res: Res<'S>) =
    try
        if res.IsValid then
            onOk res.Value
            res
        else
            onFailed res.Error
            res
    with exc ->
        failed exc

let log = inspect

let map (fn: 'S -> 'T) (res: Res<'S>) =
    if res.IsValid then
        try
            res.Value |> fn |> ok
        with exc ->
            failed exc
    else
        failed res.Error

let tryMapErr fn (res: Res<'S>) =
    if not res.IsValid then
        try
            res.Error |> fn |> ok
        with exc ->
            failed exc
    else
        res

let finish f r = ignore (map f r)
