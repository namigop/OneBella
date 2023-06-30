namespace OneBella.Core

open System
open LiteDB


type BObjectId =
    { Type: string
      Value: ObjectId
      Raw: BsonValue }

type BDoc =
    { Type: string
      Count: int
      Value: BsonDocument
      Raw: BsonValue }

type BArray =
    { Type: string
      Count: int
      Value: BsonArray
      Raw: BsonValue }

type BNull =
    { Type: string
      Value: BsonValue
      Raw: BsonValue }

type BBytes =
    { Type: string
      SizeKB: double
      Value: byte array
      Raw: BsonValue }

type BBool =
    { Type: string
      Value: bool
      Raw: BsonValue }

type BDecimal =
    { Type: string
      Value: decimal
      Raw: BsonValue }

type BDouble =
    { Type: string
      Value: double
      Raw: BsonValue }

type BGuid =
    { Type: string
      Value: Guid
      Raw: BsonValue }

type BInt =
    { Type: string
      Value: int
      Raw: BsonValue }

type BLong =
    { Type: string
      Value: int64
      Raw: BsonValue }

type BString =
    { Type: string
      Value: string
      Raw: BsonValue }

type BDateTime =
    { Type: string
      Value: DateTime
      Raw: BsonValue }

type BValType =
    | Document of BDoc
    | Array    of BArray
    | Bytes    of BBytes
    | Bool     of BBool
    | Decimal  of BDecimal
    | Double   of BDouble
    | Guid     of BGuid
    | Int      of BInt
    | Long     of BLong
    | String   of BString
    | DateTime of BDateTime
    | Nil      of BNull
    | ObjectId of BObjectId

module BVal =


    let getRawValue (bValType: BValType) =
        match bValType with
        | Document d -> d.Raw
        | Array d    -> d.Raw
        | Bytes d    -> d.Raw
        | Bool d     -> d.Raw
        | Decimal d  -> d.Raw
        | Double d   -> d.Raw
        | Guid d     -> d.Raw
        | Int d      -> d.Raw
        | Long d     -> d.Raw
        | String d   -> d.Raw
        | DateTime d -> d.Raw
        | Nil d      -> d.Raw
        | ObjectId d -> d.Raw
    let isObjectId (bValType: BValType) =
        bValType |> getRawValue |> fun d -> d.IsObjectId

    let findObjectId (bValType: BValType) =
        match bValType with
        | Document d ->
            d.Value
            |> Seq.tryFind (fun kv ->
                let bb =kv.Key = "_id"
                bb )
            |> fun b ->
                match b with
                    | Some (v) -> Some v.Value
                    | None -> None
        | _ -> None

    let createBDoc (bVal: BsonValue) : BDoc =
        { Type = "document"
          Count = bVal.AsDocument.Keys.Count
          Value = bVal.AsDocument
          Raw = bVal }

    let createBArray (bVal: BsonValue) : BArray =
        { Type = "array"
          Count = bVal.AsArray.Count
          Value = bVal.AsArray
          Raw = bVal }

    let createBBytes (bVal: BsonValue) =
        { Type = "bytes"
          SizeKB = Convert.ToDouble(bVal.AsBinary.LongLength) / 1024.0
          Value = bVal.AsBinary
          Raw = bVal }
    let createBObjectId (bVal: BsonValue) : BObjectId=
        { Type = "objectId"
          Value = bVal.AsObjectId
          Raw = bVal }

    let createBBool (bVal: BsonValue) : BBool =
        { Type = "bool"
          Value = bVal.AsBoolean
          Raw = bVal }

    let createBDecimal (bVal: BsonValue) : BDecimal =
        { Type = "decimal"
          Value = bVal.AsDecimal
          Raw = bVal }

    let createDouble (bVal: BsonValue) : BDouble =
        { Type = "double"
          Value = bVal.AsDouble
          Raw = bVal }

    let createBGuid (bVal: BsonValue) : BGuid =
        { Type = "guid"
          Value = bVal.AsGuid
          Raw = bVal }

    let createBInt (bVal: BsonValue) : BInt =
        { Type = "int"
          Value = bVal.AsInt32
          Raw = bVal }

    let createBLong (bVal: BsonValue) : BLong =
        { Type = "long"
          Value = bVal.AsInt64
          Raw = bVal }

    let createBDouble (bVal: BsonValue) : BDouble =
        { Type = "double"
          Value = bVal.AsDouble
          Raw = bVal }

    let createBString (bVal: BsonValue) : BString =
        { Type = "string"
          Value = bVal.AsString
          Raw = bVal }

    let createBDateTime (bVal: BsonValue) : BDateTime =
        { Type = "dateTime"
          Value = bVal.AsDateTime
          Raw = bVal }

    let createBNull (bVal: BsonValue) : BNull = { Type = ""; Value = bVal; Raw = bVal }

    let createBsonValue (typ:BValType) (target:string) =
         match typ with
            | String   _ -> BsonValue(target)
            | DateTime _ -> target |> System.DateTime.Parse |> BsonValue
            | Bool     _ -> target |> Convert.ToBoolean |> BsonValue
            | Decimal  _ -> target |> Convert.ToDecimal |> BsonValue
            | Double   _ -> target |> Convert.ToDouble |> BsonValue
            | Long     _ -> target |> Convert.ToInt64 |> BsonValue
            | Int      _ -> target |> Convert.ToInt32 |> BsonValue
            | Guid     _ -> target |>  Guid.Parse |> BsonValue
            | _ -> JsonSerializer.Deserialize target

    let create (bVal: BsonValue) : BValType =
        if bVal.IsDocument then
            bVal |> createBDoc |> Document
        elif bVal.IsArray then
            bVal |> createBArray |> Array
        elif bVal.IsNull then
            bVal |> createBNull |> Nil
        elif bVal.IsBinary then
            bVal |> createBBytes |> Bytes
        elif bVal.IsBoolean then
            bVal |> createBBool |> Bool
        elif bVal.IsDecimal then
            bVal |> createBDecimal |> Decimal
        elif bVal.IsDouble then
            bVal |> createBDouble |> Double
        elif bVal.IsGuid then
            bVal |> createBGuid |> Guid
        elif bVal.IsInt32 then
            bVal |> createBInt |> Int
        elif bVal.IsInt64 then
            bVal |> createBLong |> Long
        //elif bVal.IsNumber then $"number",$"{bVal.AsDouble}"
        elif bVal.IsString then
            bVal |> createBString |> String
        elif bVal.IsDateTime then
            bVal |> createBDateTime |> DateTime
        elif bVal.IsObjectId then
            bVal |> createBObjectId |> ObjectId
        else
            failwith "unsupported type"
