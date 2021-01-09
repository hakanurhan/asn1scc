﻿(*
* Copyright (c) 2008-2012 Semantix and (c) 2012-2015 Neuropublic
*
* This file is part of the ASN1SCC tool.
*
* Licensed under the terms of GNU General Public Licence as published by
* the Free Software Foundation.
*
*  For more informations see License.txt file
*)

module MapParamAstToNonParamAst

open System
open System.Globalization
open System.Numerics
open Antlr.Runtime.Tree
open Antlr.Runtime
open CommonTypes
open FsUtils



let getModuleByName = ParameterizedAsn1Ast.getModuleByName

let getTasByName  = ParameterizedAsn1Ast.getTasByName

type UserDefinedTypeScope = ScopeNode list

type UserDefinedVarScope = VarScopNode list


let visitModule (md:ParameterizedAsn1Ast.Asn1Module) : UserDefinedTypeScope=
    [MD md.Name.Value]

let visitTas (s:UserDefinedTypeScope) (ts:ParameterizedAsn1Ast.TypeAssignment) : UserDefinedTypeScope=
    s@[TA ts.Name.Value]

let visitTypeVas (s:UserDefinedTypeScope) (vas:ParameterizedAsn1Ast.ValueAssignment) : UserDefinedTypeScope=
    s@[VA vas.Name.Value]

let visitRefType (md:string) (ts:string) : UserDefinedTypeScope=
    [MD md; TA ts]

//let visitRevValue (md:string) (vs:string) =
//    {UserDefinedTypeScope.typeID=[MD md; VA vs]; asn1TypeName=None; asn1VarName=Some vs;varID=[]}

let visitSeqChild (s:UserDefinedTypeScope) (ch:ParameterizedAsn1Ast.ChildInfo) : UserDefinedTypeScope=
    s@[SEQ_CHILD ch.Name.Value]

let visitChoiceChild (s:UserDefinedTypeScope) (ch:ParameterizedAsn1Ast.ChildInfo) : UserDefinedTypeScope=
    s@[CH_CHILD (ch.Name.Value, ToC2 ch.Name.Value)]

let visitSeqOfChild (s:UserDefinedTypeScope) : UserDefinedTypeScope =
    s@[SQF]


let visitValueVas  (vs:ParameterizedAsn1Ast.ValueAssignment) : UserDefinedVarScope=
    [VA2 vs.Name.Value]


let visitDefaultValue () : UserDefinedVarScope = 
    [DV]

let visitNamedItemValue  (nm:ParameterizedAsn1Ast.NamedItem) : UserDefinedVarScope= 
    [NI nm.Name.Value]

let visitConstraint (s:UserDefinedVarScope) : UserDefinedVarScope= 
    s@[CON 0]

let visitSilbingConstraint (s:UserDefinedVarScope) : UserDefinedVarScope = 
    let idx, xs = 
        match s |> List.rev with
        | (CON idx)::xs  -> idx, xs
        | _              -> raise(BugErrorException "invalid call to visitSilbingConstraint")
    xs@[CON (idx+1)]


let visitValue (s:UserDefinedVarScope) :UserDefinedVarScope = 
    s @[VL 0]

let visitSilbingValue (s:UserDefinedVarScope) :UserDefinedVarScope = 
    let idx, xs = 
        match s |> List.rev with
        | (VL idx)::xs  -> idx, xs
        | _              -> raise(BugErrorException "invalid call to visitSilbingConstraint")
    xs@[VL (idx+1)]

let visitSeqOfValue (s:UserDefinedVarScope) idx :UserDefinedVarScope =
    s @[SQOV idx]

let visitSeqChildValue (s:UserDefinedVarScope) childName :UserDefinedVarScope =
    s @[SQCHILD childName ]



let rec getSequenceChildren (r:ParameterizedAsn1Ast.AstRoot) (input:list<ParameterizedAsn1Ast.SequenceChild>) : list<ParameterizedAsn1Ast.ChildInfo> =
    seq {
        for ch in input do
        match ch with
        | ParameterizedAsn1Ast.ChildInfo ch  -> yield ch
        | ParameterizedAsn1Ast.ComponentsOf (md,ts) ->
            let tas = ParameterizedAsn1Ast.getTypeAssignment r md ts
            match tas.Type.Kind with
            | ParameterizedAsn1Ast.Sequence(children)    ->
                 yield! getSequenceChildren r children
            | _                                          -> raise(SemanticError(ts.Location, "Expecting SEQUENCE type"))    
            }|> Seq.toList

let rec getActualKind r kind =
    match kind with
    | ParameterizedAsn1Ast.ReferenceType(md, ts,_) -> 
        let newTas = ParameterizedAsn1Ast.getTypeAssignment r md ts
        getActualKind r newTas.Type.Kind
    | _                                            -> kind


let rec MapAsn1Value (r:ParameterizedAsn1Ast.AstRoot) (kind: ParameterizedAsn1Ast.Asn1TypeKind) (typeScope : ScopeNode list) (variableScope : VarScopNode list)  (v:ParameterizedAsn1Ast.Asn1Value) :Asn1Ast.Asn1Value =
    let rec getActualKindAndModule r kind =
        let rec getActualaux r kind modName=
            match kind with
            | ParameterizedAsn1Ast.ReferenceType(md, ts,_) -> 
                let mdl = ParameterizedAsn1Ast.getModuleByName  r md
                let newTas = ParameterizedAsn1Ast.getTypeAssignment r md ts
                getActualaux r newTas.Type.Kind (Some mdl.Name)
            | _                                            -> kind, modName
        getActualaux r kind None

    let MapAsn1ValueKind (r:ParameterizedAsn1Ast.AstRoot) (kind: ParameterizedAsn1Ast.Asn1TypeKind) (vk:ParameterizedAsn1Ast.Asn1ValueKind) :Asn1Ast.Asn1ValueKind =
        let actKind = getActualKind r kind
        match vk with
        |ParameterizedAsn1Ast.IntegerValue(v)       -> Asn1Ast.IntegerValue v
        |ParameterizedAsn1Ast.RealValue(v)          -> Asn1Ast.RealValue v
        |ParameterizedAsn1Ast.StringValue(v)        -> 
            let actKind, mdName = getActualKindAndModule r kind
            match actKind with
            | ParameterizedAsn1Ast.TimeType tmClss  -> Asn1Ast.TimeValue (CommonTypes.createTimeValueFromString tmClss v)
            | _                                     -> Asn1Ast.StringValue v
        |ParameterizedAsn1Ast.BooleanValue(v)       -> Asn1Ast.BooleanValue v
        |ParameterizedAsn1Ast.BitStringValue(v)     -> Asn1Ast.BitStringValue v
        |ParameterizedAsn1Ast.OctetStringValue v    -> Asn1Ast.OctetStringValue v
        |ParameterizedAsn1Ast.RefValue(v1,v2)       -> 
            let actKind, mdName = getActualKindAndModule r kind
            match actKind with
            | ParameterizedAsn1Ast.Enumerated(items)    ->  
                match mdName with
                | None  ->  Asn1Ast.RefValue(v1,v2)
                | Some(s)   -> Asn1Ast.RefValue(s,v2)
            | _                                         ->  Asn1Ast.RefValue(v1,v2)
        |ParameterizedAsn1Ast.SeqOfValue(vals)      -> 
            match actKind with
            | ParameterizedAsn1Ast.SequenceOf(ch)    -> Asn1Ast.SeqOfValue(vals |> List.mapi (fun idx v -> MapAsn1Value r ch.Kind typeScope (visitSeqOfValue variableScope idx) v))
            | _                                      -> raise(SemanticError(v.Location, "Expecting a SEQUENCE OF value"))
        |ParameterizedAsn1Ast.SeqValue(vals)        -> 
            match actKind with
            |ParameterizedAsn1Ast.Sequence(children) ->
                let children = getSequenceChildren r children
                let mapChildVal (nm:StringLoc, chv:ParameterizedAsn1Ast.Asn1Value) =
                    match children |> Seq.tryFind(fun ch -> ch.Name=nm) with
                    | Some(child)   -> (nm, MapAsn1Value r child.Type.Kind typeScope (visitSeqChildValue variableScope nm.Value) chv)
                    | _             -> raise(SemanticError(nm.Location, sprintf "Unknown component name '%s'" nm.Value))
                Asn1Ast.SeqValue(vals |> List.map mapChildVal)
            | _                 -> raise(SemanticError(v.Location, "Expecting a SEQUENCE value"))
        |ParameterizedAsn1Ast.ChValue(n,v)          -> 
            match actKind with
            |ParameterizedAsn1Ast.Choice(children) ->
                match children |> Seq.tryFind(fun x -> x.Name=n) with
                | Some(child)   -> Asn1Ast.ChValue(n, MapAsn1Value r child.Type.Kind typeScope (visitSeqChildValue variableScope n.Value) v)
                | None          -> raise(SemanticError(n.Location, sprintf "Unknown alternative name '%s'" n.Value))
            | _                 -> raise(SemanticError(v.Location, "Expecting a CHOICE value"))
            
        |ParameterizedAsn1Ast.NullValue             -> Asn1Ast.NullValue
        |ParameterizedAsn1Ast.ObjOrRelObjIdValue  items  -> Asn1Ast.ObjOrRelObjIdValue items

        |ParameterizedAsn1Ast.EmptyList             ->
            match actKind with
            |ParameterizedAsn1Ast.Sequence(_)       -> Asn1Ast.SeqValue []
            |ParameterizedAsn1Ast.SequenceOf(_)     -> Asn1Ast.SeqOfValue []
            | _                                     -> raise(SemanticError(v.Location, "Unexpected value"))
    {
        Asn1Ast.Asn1Value.Kind = MapAsn1ValueKind r kind v.Kind
        Location = v.Location
        id = ReferenceToValue(typeScope,variableScope)
    }




and MapAsn1Optionality (r:ParameterizedAsn1Ast.AstRoot) (kind: ParameterizedAsn1Ast.Asn1TypeKind) typeScope (o:ParameterizedAsn1Ast.Asn1Optionality) :Asn1Ast.Asn1Optionality =
    match o with
    | ParameterizedAsn1Ast.AlwaysAbsent     -> Asn1Ast.AlwaysAbsent
    | ParameterizedAsn1Ast.AlwaysPresent    -> Asn1Ast.AlwaysPresent
    | ParameterizedAsn1Ast.Optional         -> Asn1Ast.Optional ({Asn1Ast.Optional.defaultValue = None})
    | ParameterizedAsn1Ast.Default(v)       -> Asn1Ast.Optional ({Asn1Ast.Optional.defaultValue = Some (MapAsn1Value r kind typeScope (visitDefaultValue ()) v)})

and MapChildInfo (r:ParameterizedAsn1Ast.AstRoot)  typeScope (isSequence) (c:ParameterizedAsn1Ast.ChildInfo) :Asn1Ast.ChildInfo =
    {
        Asn1Ast.ChildInfo.Name = c.Name
        ada_name = ToC2 c.Name.Value
        c_name = ToC2 c.Name.Value
        present_when_name = ToC2 c.Name.Value
        Type = MapAsn1Type r  (if isSequence then (visitSeqChild typeScope c) else (visitChoiceChild typeScope c)) c.Type
        Optionality = match c.Optionality with
                      |None -> None
                      |Some(x) -> Some (MapAsn1Optionality r c.Type.Kind typeScope x)
        Comments = c.Comments
    }

and MapNamedItem (r:ParameterizedAsn1Ast.AstRoot) typeScope (n:ParameterizedAsn1Ast.NamedItem) :Asn1Ast.NamedItem =
    {
        Asn1Ast.NamedItem.Name = n.Name
        c_name = ToC n.Name.Value
        ada_name = ToC n.Name.Value
        _value = match n._value with
                 | None -> None
                 | Some(x)  -> Some (MapAsn1Value r ParameterizedAsn1Ast.Integer typeScope (visitNamedItemValue n) x)
        Comments = n.Comments
    }



and MapNamedConstraint (r:ParameterizedAsn1Ast.AstRoot) (kind: ParameterizedAsn1Ast.Asn1TypeKind) typeScope cs (x:ParameterizedAsn1Ast.NamedConstraint) :Asn1Ast.NamedConstraint =
    let childKind = match getActualKind r kind with
                    | ParameterizedAsn1Ast.Choice(children)     ->
                        match children |> Seq.tryFind(fun ch -> ch.Name = x.Name) with
                        | None      -> raise(SemanticError(x.Name.Location, sprintf "Unknown component name '%s'" x.Name.Value))
                        | Some(child)   -> child.Type.Kind

                    | ParameterizedAsn1Ast.Sequence(children)   ->
                        let children = getSequenceChildren r children
                        match children |> Seq.tryFind(fun ch -> ch.Name = x.Name) with
                        | None      -> raise(SemanticError(x.Name.Location, sprintf "Unknown component name '%s'" x.Name.Value))
                        | Some(child)   -> child.Type.Kind
                    | _                                         -> raise(SemanticError(x.Name.Location, sprintf "Unexpected constraint type" ))
    {
        Asn1Ast.NamedConstraint.Name = x.Name; 
        Mark = match x.Mark with
                | ParameterizedAsn1Ast.NoMark        -> Asn1Ast.NoMark
                | ParameterizedAsn1Ast.MarkPresent   -> Asn1Ast.MarkPresent
                | ParameterizedAsn1Ast.MarkAbsent    -> Asn1Ast.MarkAbsent
                | ParameterizedAsn1Ast.MarkOptional  -> Asn1Ast.MarkOptional
        Contraint = match x.Contraint with
                    | None  -> None
                    | Some(cc)  -> Some (MapAsn1Constraint r childKind typeScope cs cc)
    }

and MapAsn1Constraint (r:ParameterizedAsn1Ast.AstRoot) (kind: ParameterizedAsn1Ast.Asn1TypeKind) typeScope cs (c:ParameterizedAsn1Ast.Asn1Constraint) :Asn1Ast.Asn1Constraint =
    match c with
    | ParameterizedAsn1Ast.SingleValueContraint(v)          -> Asn1Ast.SingleValueContraint (MapAsn1Value r kind typeScope (visitValue cs) v)
    | ParameterizedAsn1Ast.RangeContraint(v1,v2,b1,b2)      -> 
        let vs1 = visitValue cs
        let vs2 = visitSilbingValue vs1

        Asn1Ast.RangeContraint(MapAsn1Value r kind typeScope vs1 v1, MapAsn1Value r kind typeScope vs2 v2,b1,b2)
    | ParameterizedAsn1Ast.RangeContraint_val_MAX(v,b)        -> Asn1Ast.RangeContraint_val_MAX (MapAsn1Value r kind typeScope (visitValue cs) v, b)
    | ParameterizedAsn1Ast.RangeContraint_MIN_val(v,b)        -> Asn1Ast.RangeContraint_MIN_val (MapAsn1Value r kind typeScope (visitValue cs) v, b)
    | ParameterizedAsn1Ast.TypeInclusionConstraint(s1,s2)   -> Asn1Ast.TypeInclusionConstraint(s1,s2)
    | ParameterizedAsn1Ast.SizeContraint(c)                 -> Asn1Ast.SizeContraint(MapAsn1Constraint r ParameterizedAsn1Ast.Integer typeScope (visitConstraint cs) c)
    | ParameterizedAsn1Ast.AlphabetContraint(c)             -> Asn1Ast.AlphabetContraint(MapAsn1Constraint r ParameterizedAsn1Ast.IA5String typeScope (visitConstraint cs) c)
    | ParameterizedAsn1Ast.UnionConstraint(c1,c2, b)           -> 
        let cs1 = visitConstraint cs
        let cs2 = visitSilbingConstraint cs1
        Asn1Ast.UnionConstraint(MapAsn1Constraint r kind typeScope cs1 c1, MapAsn1Constraint r kind typeScope cs2 c2, b)
    | ParameterizedAsn1Ast.IntersectionConstraint(c1,c2)    -> 
        let cs1 = visitConstraint cs
        let cs2 = visitSilbingConstraint cs1
        Asn1Ast.IntersectionConstraint(MapAsn1Constraint r kind typeScope cs1 c1, MapAsn1Constraint r kind typeScope cs2 c2)
    | ParameterizedAsn1Ast.AllExceptConstraint(c)           -> Asn1Ast.AllExceptConstraint(MapAsn1Constraint r kind typeScope (visitConstraint cs) c)
    | ParameterizedAsn1Ast.ExceptConstraint(c1,c2)          -> 
        let cs1 = visitConstraint cs
        let cs2 = visitSilbingConstraint cs1
        Asn1Ast.ExceptConstraint(MapAsn1Constraint r kind typeScope cs1 c1, MapAsn1Constraint r kind typeScope cs2 c2)
    | ParameterizedAsn1Ast.RootConstraint(c1)               -> Asn1Ast.RootConstraint(MapAsn1Constraint r kind typeScope (visitConstraint cs) c1)
    | ParameterizedAsn1Ast.RootConstraint2(c1,c2)           -> 
        let cs1 = visitConstraint cs
        let cs2 = visitSilbingConstraint cs1
        Asn1Ast.RootConstraint2(MapAsn1Constraint r kind typeScope cs2 c1, MapAsn1Constraint r kind typeScope cs2 c2)
    | ParameterizedAsn1Ast.WithComponentConstraint(c, loc)       -> 
        let akind = getActualKind r kind
        match akind with
        | ParameterizedAsn1Ast.SequenceOf(child)    ->        
            Asn1Ast.WithComponentConstraint((MapAsn1Constraint r child.Kind typeScope (visitConstraint cs) c), loc)
        | _                                         ->        raise(SemanticError(emptyLocation,"Unexpected constraint type"))
    | ParameterizedAsn1Ast.WithComponentsConstraint(ncs)    -> 
        Asn1Ast.WithComponentsConstraint(ncs|> foldMap (fun cs c -> 
                                                                let newC = MapNamedConstraint r kind typeScope (visitConstraint cs) c
                                                                let newSs = visitSilbingConstraint cs
                                                                newC, newSs) cs |> fst  )

and MapAsn1Type (r:ParameterizedAsn1Ast.AstRoot) typeScope (t:ParameterizedAsn1Ast.Asn1Type) :Asn1Ast.Asn1Type =
    let aux kind : Asn1Ast.Asn1Type=
        {
            Asn1Ast.Asn1Type.Kind = kind
            Constraints = 
                t.Constraints |> 
                foldMap (fun ss c -> 
                   let newC = MapAsn1Constraint r t.Kind typeScope ss c
                   let newSs = visitSilbingConstraint ss
                   newC, newSs) (visitConstraint []) |> fst
            Location = t.Location
            parameterizedTypeInstance = t.parameterizedTypeInstance
            acnInfo = t.acnInfo
            encodeAsContainedInOctetString = t.encodeAsContainedInOctetString
        }        
    match t.Kind with
    | ParameterizedAsn1Ast.Integer          -> aux Asn1Ast.Integer
    | ParameterizedAsn1Ast.Real             -> aux Asn1Ast.Real
    | ParameterizedAsn1Ast.IA5String        -> aux Asn1Ast.IA5String
    | ParameterizedAsn1Ast.NumericString    -> aux Asn1Ast.NumericString
    | ParameterizedAsn1Ast.OctetString      -> aux Asn1Ast.OctetString
    | ParameterizedAsn1Ast.TimeType cl        -> aux (Asn1Ast.TimeType cl)
    | ParameterizedAsn1Ast.NullType         -> aux Asn1Ast.NullType
    | ParameterizedAsn1Ast.BitString        -> aux Asn1Ast.BitString
    | ParameterizedAsn1Ast.Boolean          -> aux Asn1Ast.Boolean
    | ParameterizedAsn1Ast.ObjectIdentifier         -> aux Asn1Ast.ObjectIdentifier
    | ParameterizedAsn1Ast.RelativeObjectIdentifier -> aux Asn1Ast.RelativeObjectIdentifier
    | ParameterizedAsn1Ast.Enumerated(items)-> aux (Asn1Ast.Enumerated(items |> List.map (MapNamedItem r typeScope)))
    | ParameterizedAsn1Ast.SequenceOf(child)-> aux (Asn1Ast.SequenceOf(MapAsn1Type r (visitSeqOfChild typeScope) child))
    | ParameterizedAsn1Ast.Sequence(children)   -> 
        let children = getSequenceChildren r children
        aux (Asn1Ast.Sequence(children |> List.map (MapChildInfo r typeScope true) ))
    | ParameterizedAsn1Ast.Choice(children)     -> aux (Asn1Ast.Choice(children |> List.map (MapChildInfo r typeScope false) ))
    | ParameterizedAsn1Ast.ReferenceType(mdName,ts, args)   ->
        match args with
        | []    ->  aux (Asn1Ast.ReferenceType({Asn1Ast.ReferenceType.modName = mdName; tasName = ts; tabularized = false}))
        | _     ->  raise(BugErrorException "")

    

let MapTypeAssignment (r:ParameterizedAsn1Ast.AstRoot) (m:ParameterizedAsn1Ast.Asn1Module) (tas:ParameterizedAsn1Ast.TypeAssignment) :Asn1Ast.TypeAssignment =
    {
        Asn1Ast.TypeAssignment.Name = tas.Name
        Type = MapAsn1Type r ([MD m.Name.Value; TA tas.Name.Value]) tas.Type
        c_name = ToC2 tas.Name.Value
        ada_name = ToC2 tas.Name.Value
        Comments = tas.Comments
        acnInfo = tas.acnInfo
    }

let MapValueAssignment (r:ParameterizedAsn1Ast.AstRoot) (m:ParameterizedAsn1Ast.Asn1Module) (vas:ParameterizedAsn1Ast.ValueAssignment) :Asn1Ast.ValueAssignment =
    let typeScope =
        match vas.Type.Kind with
        | ParameterizedAsn1Ast.ReferenceType(md,ts,_)  -> [MD md.Value; TA ts.Value]
        | _                                            -> []//raise (SemanticError (vas.Name.Location, "Unnamed types are not currently supported."))
    let varScope = [VA2 vas.Name.Value]
    {
        Asn1Ast.ValueAssignment.Name = vas.Name
        Type = MapAsn1Type r typeScope vas.Type
        Value = MapAsn1Value r  vas.Type.Kind typeScope varScope vas.Value
        //Scope = 
        //    match vas.Scope with
        //    | ParameterizedAsn1Ast.GlobalScope      ->  Asn1Ast.GlobalScope
        //    | ParameterizedAsn1Ast.TypeScope(m,t)   ->  Asn1Ast.TypeScope(m,t)
        c_name = vas.c_name
        ada_name = vas.ada_name
    }


let MapModule (r:ParameterizedAsn1Ast.AstRoot) (m:ParameterizedAsn1Ast.Asn1Module) :Asn1Ast.Asn1Module =
    let DoImportedModule (x:ParameterizedAsn1Ast.ImportedModule) : Asn1Ast.ImportedModule =
        { Asn1Ast.ImportedModule.Name = x.Name; Types = x.Types; Values = x.Values}
    {
        Asn1Ast.Asn1Module.Name = m.Name
        TypeAssignments = m.TypeAssignments |> List.filter(fun x -> x.Parameters.Length = 0) |> List.map (MapTypeAssignment r m)
        ValueAssignments = m.ValueAssignments |> List.map (MapValueAssignment r m)
        Imports = m.Imports |> List.map DoImportedModule
        Exports  = match m.Exports with
                   | ParameterizedAsn1Ast.All               -> Asn1Ast.All
                   | ParameterizedAsn1Ast.OnlySome(lst)     -> Asn1Ast.OnlySome(lst)
        Comments = m.Comments
    }

let MapFile (r:ParameterizedAsn1Ast.AstRoot) (f:ParameterizedAsn1Ast.Asn1File) :Asn1Ast.Asn1File =
    {
        Asn1Ast.Asn1File.FileName = f.FileName
        Tokens = f.Tokens
        Modules = f.Modules |> List.map (MapModule r)
    }



let DoWork (r:ParameterizedAsn1Ast.AstRoot) : Asn1Ast.AstRoot =
    let r = RemoveParamterizedTypes.DoWork r
    {
        Asn1Ast.AstRoot.Files = r.Files |> List.map (MapFile r)
        args = r.args
    
    }
    