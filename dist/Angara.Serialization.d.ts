declare module Angara {
    class Guid {
        private id;
        private static emptyGuid;
        ToString(): string;
        constructor(id: string);
        static Empty(): Guid;
        static NewGuid(): Guid;
        private static s4();
    }
    enum InfoSetType {
        Null = 0,
        Bool = 1,
        Int = 2,
        Double = 3,
        String = 4,
        Raw = 5,
        Artefact = 6,
        Map = 7,
        DateTime = 8,
        Guid = 9,
        BoolArray = 10,
        IntArray = 11,
        DoubleArray = 12,
        StringArray = 13,
        DateTimeArray = 14,
        Seq = 15,
    }
    var TypeIdPropertyName: string;
    class InfoSet {
        private t;
        private v;
        constructor(t: InfoSetType, v: any);
        Type: InfoSetType;
        static EmptyMap: InfoSet;
        AddInfoSet(p: string, i: InfoSet): this;
        AddInt(p: string, n: number): this;
        AddString(p: string, s: string): this;
        AddBool(p: string, b: boolean): this;
        AddDateTime(p: string, d: Date): this;
        AddDouble(p: string, n: number): this;
        AddGuid(p: string, g: Guid): this;
        AddIntArray(p: string, inta: Array<number>): this;
        AddStringArray(p: string, sa: Array<string>): this;
        AddBoolArray(p: string, ba: Array<boolean>): this;
        AddDateTimeArray(p: string, da: Array<Date>): this;
        AddDoubleArray(p: string, da: Array<number> | Float64Array): this;
        AddSeq(p: string, s: Array<InfoSet>): this;
        ToNull(): any;
        ToBool(): boolean;
        ToInt(): number;
        ToDouble(): number;
        ToString(): string;
        ToRaw(): any;
        ToGuid(): Guid;
        ToMap(): {
            [key: string]: InfoSet;
        };
        ToArtefact(): {
            TypeId: string;
            Content: InfoSet;
        };
        ToDateTime(): Date;
        ToBoolArray(): boolean[];
        ToIntArray(): number[] | Int32Array;
        ToDoubleArray(): number[] | Float64Array;
        ToStringArray(): string[];
        ToDateTimeArray(): Date[];
        ToSeq(): InfoSet[];
        IsBool: boolean;
        IsInt: boolean;
        IsDouble: boolean;
        IsString: boolean;
        IsRaw: boolean;
        IsGuid: boolean;
        IsArtefact: boolean;
        IsMap: boolean;
        IsDateTime: boolean;
        IsBoolArray: boolean;
        IsIntArray: boolean;
        IsDoubleArray: boolean;
        IsStringArray: boolean;
        IsDateTimeArray: boolean;
        IsSeq: boolean;
        IsNull: boolean;
        static Null(): InfoSet;
        static Bool(b: boolean): InfoSet;
        static Int(n: number): InfoSet;
        static Double(n: number): InfoSet;
        static DoubleArray(d: number[] | Float64Array): InfoSet;
        static String(s: string): InfoSet;
        static Raw(r: any): InfoSet;
        static Guid(g: Guid): InfoSet;
        static Artefact(typeId: string, content: InfoSet): InfoSet;
        static Map(m: {
            [p: string]: InfoSet;
        }): InfoSet;
        static DateTime(d: Date): InfoSet;
        static BoolArray(b: boolean[] | Int8Array): InfoSet;
        static IntArray(n: number[] | Int32Array): InfoSet;
        static StringArray(s: Array<string>): InfoSet;
        static DateTimeArray(d: Array<Date>): InfoSet;
        static Seq(s: Array<InfoSet>): InfoSet;
        private static EncodeNameAndType(n, t);
        private static DecodeNameAndType(s);
        private static Encode(i);
        static Marshal(i: InfoSet): any;
        static Unmarshal(t: any): InfoSet;
        private static DecodeMap(json);
        private static Decode(t);
        static Deserialize(is: Angara.InfoSet): any;
    }
}
