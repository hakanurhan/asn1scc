﻿module Asn1AcnAst

open System.Numerics
open Antlr.Runtime.Tree
open Antlr.Runtime
open System
open FsUtils
open CommonTypes
open AcnGenericTypes






/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////// ACN PROPERTIES DEFINITION ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////// ASN1 VALUES DEFINITION    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

type IntegerValue         = IntLoc
type RealValue            = DoubleLoc
type StringValue          = StringLoc
type TimeValue            = Asn1DateTimeValueLoc
type BooleanValue         = BoolLoc
type BitStringValue       = StringLoc
type OctetStringValue     = list<ByteLoc>
type EnumValue            = StringLoc
type NullValue            = unit
type SeqOfValue           = list<Asn1Value>
and SeqValue              = list<NamedValue>
and ChValue               = NamedValue
and RefValue              = ((StringLoc*StringLoc)*Asn1Value)
and ObjectIdenfierValue   = ((ResolvedObjectIdentifierValueCompoent list)*(ObjectIdentifierValueCompoent list))

and NamedValue = {
    name        : StringLoc
    Value       : Asn1Value
}
and Asn1Value = {
    kind : Asn1ValueKind
    loc  : SrcLoc
    id   : ReferenceToValue
}

and Asn1ValueKind =
    | IntegerValue          of IntegerValue    
    | RealValue             of RealValue       
    | StringValue           of StringValue     
    | BooleanValue          of BooleanValue    
    | BitStringValue        of BitStringValue  
    | TimeValue             of TimeValue
    | OctetStringValue      of OctetStringValue
    | EnumValue             of EnumValue       
    | SeqOfValue            of SeqOfValue      
    | SeqValue              of SeqValue        
    | ChValue               of ChValue         
    | NullValue             of NullValue
    | RefValue              of RefValue   
    | ObjOrRelObjIdValue    of ObjectIdenfierValue


/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////// ASN1 CONSTRAINTS DEFINITION    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


type GenericConstraint<'v> =
    | UnionConstraint                   of GenericConstraint<'v>*GenericConstraint<'v>*bool //left,righ, virtual constraint
    | IntersectionConstraint            of GenericConstraint<'v>*GenericConstraint<'v>
    | AllExceptConstraint               of GenericConstraint<'v>
    | ExceptConstraint                  of GenericConstraint<'v>*GenericConstraint<'v>
    | RootConstraint                    of GenericConstraint<'v>
    | RootConstraint2                   of GenericConstraint<'v>*GenericConstraint<'v>
    | SingleValueConstraint             of 'v

type RangeTypeConstraint<'v1,'v2>  = 
    | RangeUnionConstraint               of RangeTypeConstraint<'v1,'v2>*RangeTypeConstraint<'v1,'v2>*bool //left,righ, virtual constraint
    | RangeIntersectionConstraint        of RangeTypeConstraint<'v1,'v2>*RangeTypeConstraint<'v1,'v2>
    | RangeAllExceptConstraint           of RangeTypeConstraint<'v1,'v2>
    | RangeExceptConstraint              of RangeTypeConstraint<'v1,'v2>*RangeTypeConstraint<'v1,'v2>
    | RangeRootConstraint                of RangeTypeConstraint<'v1,'v2>
    | RangeRootConstraint2               of RangeTypeConstraint<'v1,'v2>*RangeTypeConstraint<'v1,'v2>
    | RangeSingleValueConstraint         of 'v2
    | RangeContraint                     of ('v1) *('v1)*bool*bool    //min, max, InclusiveMin(=true), InclusiveMax(=true)
    | RangeContraint_val_MAX             of ('v1) *bool            //min, InclusiveMin(=true)
    | RangeContraint_MIN_val             of ('v1) *bool            //max, InclusiveMax(=true)

type IntegerTypeConstraint  = RangeTypeConstraint<BigInteger, BigInteger>
type PosIntTypeConstraint   = RangeTypeConstraint<UInt32, UInt32>
type CharTypeConstraint     = RangeTypeConstraint<char, string>
    
type RealTypeConstraint     = RangeTypeConstraint<double, double>


type SizableTypeConstraint<'v>  = 
    | SizeUnionConstraint               of SizableTypeConstraint<'v>*SizableTypeConstraint<'v>*bool //left,righ, virtual constraint
    | SizeIntersectionConstraint        of SizableTypeConstraint<'v>*SizableTypeConstraint<'v>
    | SizeAllExceptConstraint           of SizableTypeConstraint<'v>
    | SizeExceptConstraint              of SizableTypeConstraint<'v>*SizableTypeConstraint<'v>
    | SizeRootConstraint                of SizableTypeConstraint<'v>
    | SizeRootConstraint2               of SizableTypeConstraint<'v>*SizableTypeConstraint<'v>
    | SizeSingleValueConstraint         of 'v
    | SizeContraint                     of PosIntTypeConstraint               

type IA5StringConstraint = 
    | StrUnionConstraint               of IA5StringConstraint*IA5StringConstraint*bool //left,righ, virtual constraint
    | StrIntersectionConstraint        of IA5StringConstraint*IA5StringConstraint
    | StrAllExceptConstraint           of IA5StringConstraint
    | StrExceptConstraint              of IA5StringConstraint*IA5StringConstraint
    | StrRootConstraint                of IA5StringConstraint
    | StrRootConstraint2               of IA5StringConstraint*IA5StringConstraint
    | StrSingleValueConstraint         of string
    | StrSizeContraint                 of PosIntTypeConstraint               
    | AlphabetContraint                of CharTypeConstraint           


type OctetStringConstraint  =    SizableTypeConstraint<OctetStringValue*(ReferenceToValue*SrcLoc)>
type BitStringConstraint    =    SizableTypeConstraint<BitStringValue*(ReferenceToValue*SrcLoc)>
type BoolConstraint         =    GenericConstraint<bool>
type EnumConstraint         =    GenericConstraint<string>
type ObjectIdConstraint     =    GenericConstraint<ObjectIdenfierValue>
type TimeConstraint         =    GenericConstraint<Asn1DateTimeValue>


//type SequenceOfConstraint   =     SizableTypeConstraint<SeqOfValue>
//type SequenceConstraint     =     GenericConstraint<SeqValue>

type SeqOrChoiceConstraint<'v> =
    | SeqOrChUnionConstraint                   of SeqOrChoiceConstraint<'v>*SeqOrChoiceConstraint<'v>*bool //left,righ, virtual constraint
    | SeqOrChIntersectionConstraint            of SeqOrChoiceConstraint<'v>*SeqOrChoiceConstraint<'v>
    | SeqOrChAllExceptConstraint               of SeqOrChoiceConstraint<'v>
    | SeqOrChExceptConstraint                  of SeqOrChoiceConstraint<'v>*SeqOrChoiceConstraint<'v>
    | SeqOrChRootConstraint                    of SeqOrChoiceConstraint<'v>
    | SeqOrChRootConstraint2                   of SeqOrChoiceConstraint<'v>*SeqOrChoiceConstraint<'v>
    | SeqOrChSingleValueConstraint             of 'v
    | SeqOrChWithComponentsConstraint          of NamedConstraint list       


and SeqConstraint = SeqOrChoiceConstraint<SeqValue>

and ChoiceConstraint       =     SeqOrChoiceConstraint<ChValue>

and SequenceOfConstraint   =  
    | SeqOfSizeUnionConstraint               of SequenceOfConstraint*SequenceOfConstraint*bool //left,righ, virtual constraint
    | SeqOfSizeIntersectionConstraint        of SequenceOfConstraint*SequenceOfConstraint
    | SeqOfSizeAllExceptConstraint           of SequenceOfConstraint
    | SeqOfSizeExceptConstraint              of SequenceOfConstraint*SequenceOfConstraint
    | SeqOfSizeRootConstraint                of SequenceOfConstraint
    | SeqOfSizeRootConstraint2               of SequenceOfConstraint*SequenceOfConstraint
    | SeqOfSizeSingleValueConstraint         of SeqOfValue
    | SeqOfSizeContraint                     of PosIntTypeConstraint               
    | SeqOfSeqWithComponentConstraint        of AnyConstraint*SrcLoc
    
and AnyConstraint =
    | IntegerTypeConstraint of IntegerTypeConstraint
    | IA5StringConstraint   of IA5StringConstraint   
    | RealTypeConstraint    of RealTypeConstraint   
    | OctetStringConstraint of OctetStringConstraint
    | BitStringConstraint   of BitStringConstraint
    | BoolConstraint        of BoolConstraint    
    | EnumConstraint        of EnumConstraint    
    | ObjectIdConstraint    of ObjectIdConstraint
    | SequenceOfConstraint  of SequenceOfConstraint
    | SeqConstraint         of SeqConstraint
    | ChoiceConstraint      of ChoiceConstraint
    | NullConstraint        
    | TimeConstraint        of TimeConstraint
    

and NamedConstraint = {
    Name: StringLoc
    Contraint:AnyConstraint option
    Mark:Asn1Ast.NamedConstraintMark
}


type NamedItem = {
    Name:StringLoc
    c_name:string
    ada_name:string
    definitionValue : BigInteger          // the value in the header file
    
    // the value encoded by ACN. It can (a) the named item index (i.e. like uper), (b) The definition value, (c) The redefined value from acn properties
    acnEncodeValue  : BigInteger                
    Comments: string array
}

type Optional = {
    defaultValue        : Asn1Value option
    acnPresentWhen      : PresenceWhenBool option
}

type Asn1Optionality = 
    | AlwaysAbsent
    | AlwaysPresent
    | Optional          of Optional

type Asn1ChoiceOptionality = 
    | ChoiceAlwaysAbsent
    | ChoiceAlwaysPresent

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////// ACN ENCODING CLASSES    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

type IntEncodingClass =
    |Integer_uPER
    |PositiveInteger_ConstSize_8
    |PositiveInteger_ConstSize_big_endian_16
    |PositiveInteger_ConstSize_little_endian_16
    |PositiveInteger_ConstSize_big_endian_32
    |PositiveInteger_ConstSize_little_endian_32
    |PositiveInteger_ConstSize_big_endian_64
    |PositiveInteger_ConstSize_little_endian_64
    |PositiveInteger_ConstSize of BigInteger
    |TwosComplement_ConstSize_8
    |TwosComplement_ConstSize_big_endian_16
    |TwosComplement_ConstSize_little_endian_16
    |TwosComplement_ConstSize_big_endian_32
    |TwosComplement_ConstSize_little_endian_32
    |TwosComplement_ConstSize_big_endian_64
    |TwosComplement_ConstSize_little_endian_64
    |TwosComplement_ConstSize of BigInteger
    |ASCII_ConstSize of BigInteger
    |ASCII_VarSize_NullTerminated of byte list
    |ASCII_UINT_ConstSize of BigInteger
    |ASCII_UINT_VarSize_NullTerminated of byte  list
    |BCD_ConstSize of BigInteger
    |BCD_VarSize_NullTerminated of byte  list


type RealEncodingClass =
    | Real_uPER
    | Real_IEEE754_32_big_endian
    | Real_IEEE754_64_big_endian
    | Real_IEEE754_32_little_endian
    | Real_IEEE754_64_little_endian

type StringAcnEncodingClass =
    | Acn_Enc_String_uPER                                   of BigInteger                          //char size in bits, as in uper 
    | Acn_Enc_String_uPER_Ascii                             of BigInteger                          //char size in bits, as in uper but with charset (0..255)
    | Acn_Enc_String_Ascii_Null_Teminated                   of BigInteger*(byte  list)             //char size in bits, byte = the null character
    | Acn_Enc_String_Ascii_External_Field_Determinant       of BigInteger*RelativePath             //char size in bits, encode ascii, size is provided by an external length determinant
    | Acn_Enc_String_CharIndex_External_Field_Determinant   of BigInteger*RelativePath             //char size in bits, encode char index, size is provided by an external length determinant

type SizeableAcnEncodingClass =
    | SZ_EC_uPER              
    | SZ_EC_ExternalField    of RelativePath
    | SZ_EC_TerminationPattern of BitStringValue

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////// FRONT END TYPE DEFINITIONS   /////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
////// ASN1 WITH ACN INFORMATION  DEFINITION    /////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




type BigIntegerUperRange = uperRange<BigInteger>
type DoubleUperRange = uperRange<Double>
type UInt32UperRange = uperRange<uint32>

type Integer = {
    acnProperties       : IntegerAcnProperties
    cons                : IntegerTypeConstraint list
    withcons            : IntegerTypeConstraint list
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger
    uperRange           : BigIntegerUperRange

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : IntEncodingClass
    isUnsigned          : bool
    typeDef             : Map<ProgrammingLanguage, FE_PrimitiveTypeDefinition>

}

type Real = {
    acnProperties       : RealAcnProperties
    cons                : RealTypeConstraint list
    withcons            : RealTypeConstraint list
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger
    uperRange           : DoubleUperRange

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : RealEncodingClass

    typeDef             : Map<ProgrammingLanguage, FE_PrimitiveTypeDefinition>
}

type StringType = {
    acnProperties       : StringAcnProperties
    cons                : IA5StringConstraint list
    withcons            : IA5StringConstraint list

    minSize             : SIZE
    maxSize             : SIZE
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger
    uperCharSet         : char array

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : StringAcnEncodingClass
    isNumeric           : bool
    typeDef             : Map<ProgrammingLanguage, FE_StringTypeDefinition>

}


type OctetString = {
    acnProperties       : SizeableAcnProperties
    cons                : OctetStringConstraint list
    withcons            : OctetStringConstraint list
    minSize             : SIZE
    maxSize             : SIZE
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : SizeableAcnEncodingClass
    typeDef             : Map<ProgrammingLanguage, FE_SizeableTypeDefinition>

}

type BitString = {
    acnProperties   : SizeableAcnProperties
    cons                : BitStringConstraint list
    withcons            : BitStringConstraint list
    minSize             : SIZE
    maxSize             : SIZE
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : SizeableAcnEncodingClass
    typeDef             : Map<ProgrammingLanguage, FE_SizeableTypeDefinition>
    namedBitList     : NamedBit1 list
}

type TimeType = {
    timeClass       :   TimeTypeClass
    cons                : TimeConstraint list
    withcons            : TimeConstraint list
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    typeDef             : Map<ProgrammingLanguage, FE_PrimitiveTypeDefinition>
}

type NullType = {
    acnProperties       : NullTypeAcnProperties
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    typeDef             : Map<ProgrammingLanguage, FE_PrimitiveTypeDefinition>

}

type Boolean = {    
    acnProperties       : BooleanAcnProperties
    cons                : BoolConstraint list
    withcons            : BoolConstraint list
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger
    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    typeDef             : Map<ProgrammingLanguage, FE_PrimitiveTypeDefinition>
}

type ObjectIdentifier = {    
    acnProperties       : ObjectIdTypeAcnProperties
    cons                : ObjectIdConstraint list
    withcons            : ObjectIdConstraint list
    relativeObjectId    : bool
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger
    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    typeDef             : Map<ProgrammingLanguage, FE_PrimitiveTypeDefinition>
}


type Enumerated = {
    items               : NamedItem list
    acnProperties       : IntegerAcnProperties
    cons                : EnumConstraint list
    withcons            : EnumConstraint list
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger
    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : IntEncodingClass
    encodeValues        : bool
    userDefinedValues   : bool      //if true, the user has associated at least one item with a value
    typeDef             : Map<ProgrammingLanguage, FE_EnumeratedTypeDefinition>

}

type AcnReferenceToEnumerated = {
    modName             : StringLoc
    tasName             : StringLoc
    enumerated          : Enumerated
    acnAligment         : AcnAligment option
}


type AcnReferenceToIA5String = {
    modName             : StringLoc
    tasName             : StringLoc
    str                 : StringType
    acnAligment         : AcnAligment option
}

type AcnInteger = {
    acnProperties       : IntegerAcnProperties
    cons                : IntegerTypeConstraint list
    withcons            : IntegerTypeConstraint list
    acnAligment         : AcnAligment option
    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : IntEncodingClass
    Location            : SrcLoc //Line no, Char pos
    uperRange           : BigIntegerUperRange
    isUnsigned          : bool
    checkIntHasEnoughSpace  : BigInteger -> BigInteger -> unit
    inheritInfo          : InheritanceInfo option
}

type AcnBoolean = {
    acnProperties       : BooleanAcnProperties
    acnAligment         : AcnAligment option
    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    Location            : SrcLoc //Line no, Char pos
}

type AcnNullType = {
    acnProperties       : NullTypeAcnProperties
    acnAligment         : AcnAligment option
    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    Location            : SrcLoc //Line no, Char pos
}

type  AcnInsertedType = 
    | AcnInteger                of AcnInteger
    | AcnNullType               of AcnNullType
    | AcnBoolean                of AcnBoolean
    | AcnReferenceToEnumerated  of AcnReferenceToEnumerated
    | AcnReferenceToIA5String   of AcnReferenceToIA5String
with
    member this.AsString =
        match this with
        | AcnInteger  _                 -> "INTEGER"
        | AcnNullType _                 -> "NULL"
        | AcnBoolean  _                 -> "BOOLEAN"
        | AcnReferenceToEnumerated o    -> sprintf "%s.%s" o.modName.Value o.tasName.Value
        | AcnReferenceToIA5String  o    -> sprintf "%s.%s" o.modName.Value o.tasName.Value
    member this.acnAligment =
        match this with
        | AcnInteger  o                 -> o.acnAligment
        | AcnNullType o                 -> o.acnAligment
        | AcnBoolean  o                 -> o.acnAligment
        | AcnReferenceToEnumerated o    -> o.acnAligment
        | AcnReferenceToIA5String  o    -> o.acnAligment
    member this.savePosition  =
        match this with            
        | AcnInteger  a                 -> false
        | AcnBoolean  a                 -> false
        | AcnNullType a                 -> a.acnProperties.savePosition 
        | AcnReferenceToEnumerated a    -> false
        | AcnReferenceToIA5String a     -> false




type Asn1Type = {
    id              : ReferenceToType
    parameterizedTypeInstance : bool
    Kind            : Asn1TypeKind
    acnAligment     : AcnAligment option
    acnParameters   : AcnParameter list
    Location        : SrcLoc //Line no, Char pos
    acnLocation     : SrcLoc option

    /// Indicates that this type
    /// is a subclass (or inherits) from referencType
    /// (i.e. this type resolves the reference type)
    inheritInfo     : InheritanceInfo option

    /// it indicates that this type is directly under a type assignment.
    typeAssignmentInfo  : AssignmentInfo option

}


and Asn1TypeKind =
    | Integer           of Integer
    | Real              of Real
    | IA5String         of StringType
    | NumericString     of StringType
    | OctetString       of OctetString
    | NullType          of NullType
    | TimeType          of TimeType
    | BitString         of BitString
    | Boolean           of Boolean
    | Enumerated        of Enumerated
    | SequenceOf        of SequenceOf
    | Sequence          of Sequence
    | Choice            of Choice
    | ObjectIdentifier  of ObjectIdentifier
    | ReferenceType     of ReferenceType


and SequenceOf = {
    child           : Asn1Type
    acnProperties   : SizeableAcnProperties
    cons                : SequenceOfConstraint list
    withcons            : SequenceOfConstraint list
    minSize             : SIZE
    maxSize             : SIZE
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnEncodingClass    : SizeableAcnEncodingClass
    typeDef             : Map<ProgrammingLanguage, FE_SizeableTypeDefinition>

}

and Sequence = {
    children                : SeqChildInfo list
    acnProperties           : SequenceAcnProperties
    cons                    : SeqConstraint list
    withcons                : SeqConstraint list
    uperMaxSizeInBits       : BigInteger
    uperMinSizeInBits       : BigInteger

    acnMaxSizeInBits        : BigInteger
    acnMinSizeInBits        : BigInteger
    typeDef                 : Map<ProgrammingLanguage, FE_SequenceTypeDefinition>
}

and AcnChild = {
    Name                        : StringLoc
    id                          : ReferenceToType
    Type                        : AcnInsertedType
    Comments                    : string array
}

and SeqChildInfo = 
    | Asn1Child of Asn1Child
    | AcnChild  of AcnChild


and Asn1Child = {
    Name                        : StringLoc
    _c_name                     : string
    _ada_name                   : string                     
    Type                        : Asn1Type
    Optionality                 : Asn1Optionality option
    asn1Comments                : string list
    acnComments                 : string list
}
with
    member this.Comments = this.asn1Comments@this.acnComments




and Choice = {
    children            : ChChildInfo list
    acnProperties       : ChoiceAcnProperties
    cons                : ChoiceConstraint list
    withcons            : ChoiceConstraint list
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger

    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    acnLoc              : SrcLoc option
    typeDef             : Map<ProgrammingLanguage, FE_ChoiceTypeDefinition>

}

and ChChildInfo = {
    Name                        : StringLoc
    _c_name                     : string
    _ada_name                   : string                     
    present_when_name           : string // Does not contain the "_PRESENT". Not to be used directly by backends.
    Type                        : Asn1Type
    acnPresentWhenConditions    : AcnPresentWhenConditionChoiceChild list
    asn1Comments                : string list
    acnComments                 : string list
    Optionality                 : Asn1ChoiceOptionality option
}
with
    member this.Comments = this.asn1Comments@this.acnComments


and EncodeWithinOctetOrBitStringProperties = {
    minSize             : SIZE
    maxSize             : SIZE
    acnEncodingClass    : SizeableAcnEncodingClass
    octOrBitStr         : ContainedInOctOrBitString
}

and ReferenceType = {
    modName     : StringLoc
    tasName     : StringLoc
    tabularized : bool
    acnArguments: RelativePath list
    resolvedType: Asn1Type
    hasConstraints : bool
    typeDef             : Map<ProgrammingLanguage, FE_TypeDefinition>
    uperMaxSizeInBits   : BigInteger
    uperMinSizeInBits   : BigInteger
    acnMaxSizeInBits    : BigInteger
    acnMinSizeInBits    : BigInteger
    encodingOptions        : EncodeWithinOctetOrBitStringProperties option
}


type TypeAssignment = {
    Name:StringLoc
    c_name:string
    ada_name:string
    Type:Asn1Type
    asn1Comments: string list
    acnComments : string list
}
with
    member this.Comments = this.asn1Comments@this.acnComments

type ValueAssignment = {
    Name:StringLoc
    c_name:string
    ada_name:string
    Type:Asn1Type
    Value:Asn1Value
}


type Asn1Module = {
    Name : StringLoc
    TypeAssignments : list<TypeAssignment>
    ValueAssignments : list<ValueAssignment>
    Imports : list<Asn1Ast.ImportedModule>
    Exports : Asn1Ast.Exports
    Comments : string array
}

type Asn1File = {
    FileName:string;
    Tokens: IToken array
    Modules : list<Asn1Module>
}

type AstRoot = {
    Files: list<Asn1File>
    acnConstants : Map<string, BigInteger>
    args:CommandLineSettings
    acnParseResults:CommonTypes.AntlrParserResult list //used in ICDs to regenerate with collors the initial ACN input
    stg : AbstractMacros.StgMacros  
}



type ReferenceToEnumerated = {
    modName : string
    tasName : string
    enm     : Enumerated
}

type AcnDependencyKind = 
    | AcnDepIA5StringSizeDeterminant  of (SIZE*SIZE*StringAcnProperties)                // The asn1Type has a size dependency in IA5String etc
    | AcnDepSizeDeterminant     of (SIZE*SIZE*SizeableAcnProperties)               // The asn1Type has a size dependency a SEQUENCE OF, BIT STRINT, OCTET STRING etc
    | AcnDepSizeDeterminant_bit_oct_str_containt     of ReferenceType             // The asn1Type has a size dependency a BIT STRINT, OCTET STRING containing another type
    | AcnDepRefTypeArgument       of AcnParameter        // string is the param name
    | AcnDepPresenceBool                     // points to a SEQEUNCE or Choice child
    | AcnDepPresence              of (RelativePath*Choice)
    | AcnDepPresenceStr           of (RelativePath*Choice*StringType)
    | AcnDepChoiceDeteterminant   of (ReferenceToEnumerated*Choice)           // points to Enumerated type acting as CHOICE determinant.

type Determinant =
    | AcnChildDeterminant       of AcnChild
    | AcnParameterDeterminant   of AcnParameter
    with 
        member this.id = 
            match this with
            | AcnChildDeterminant       c  -> c.id
            | AcnParameterDeterminant   p  -> p.id

//The following type expresses the dependencies that exists between ASN.1 types and ACNs types and parameters
type AcnDependency = {
    asn1Type        : ReferenceToType      // an ASN.1 type that its decoding depends on the determinant
    determinant     : Determinant          // an ACN inserted type or an ACN parameter that acts as determinant
    dependencyKind  : AcnDependencyKind
}

type AcnInsertedFieldDependencies = {
    acnDependencies                         : AcnDependency list
}



type Asn1AcnMergeState = {
    args:CommandLineSettings    
    allocatedTypeNames          : (ProgrammingLanguage*string*string)  list     //language, program unit, type definition name
    allocatedFE_TypeDefinition  : Map<(ProgrammingLanguage*ReferenceToType), FE_TypeDefinition>
    temporaryTypesAllocation    : Map<(ProgrammingLanguage*ReferenceToType), string>
}
