(*** hide ***)

#I __SOURCE_DIRECTORY__
#I "./bin/Debug"
#r "Newtonsoft.Json.dll"
#r "bin/Debug/Angara.Serialization.dll"
#r "bin/Debug/Angara.Serialization.Json.dll"
#r "bin/Debug/Newtonsoft.Json.dll"

(** 
Building serializer for a user type
===================================

This tutorial describes how to create and use serializer for user defined type. 
We need to introduce some concepts first.

###InfoSet

InfoSet is an intermediate representation of objects being serialized. InfoSet is a tree-based data structure that 
uses limited set of .NET types. Purpose of InfoSet is to decouple serialization format from object graph traversing. 
InfoSets are represented by instances of `Angara.Serialization.InfoSet` discriminated union. Here are some examples
how to create InfoSets for primitive types *)

let i1 = InfoSet.Int 1
let i2 = InfoSet.String "Hello, World!"

(** InfoSets can store arrays of primitive types and sequences of InfoSets *)

let i3 = InfoSet.UInt64Array [| 0UL; 1UL; 9223372036854775807UL |]
let i4 = InfoSet.Seq [ i1; i2; i3 ]

(** InfoSets can store dictionaries of InfoSets*)

let i5 = InfoSet.Map(Map.empty<string, InfoSet>.Add("intValue", i1).Add("sequence", i4))

(** The same InfoSet can be constructed with less code using shortcut methods  *)

let i5' = InfoSet.EmptyMap.AddInt("intValue", 1).AddSeq("sequence", [ i1; i2; i3 ]) 

(** Serialization is actually performed in two steps. On the first step object is converted to InfoSet using 
`ArtefactSerializer.Serialize` method as shown in the next code snippet *)
#r "Angara.Serialization.dll"
open Angara.Serialization
let a = [| for i in 0..100 -> i*i |] // An object to serialize
let infoSet = a |> ArtefactSerializer.Serialize CoreSerializerResolver.Instance

(** Then InfoSet is converted to serialized representation. `Angara.Serialization.Json` class 
provides support for JSON representation. Method `Marshal` converts InfoSet to JSON. Second
parameter is related to blob support and can be set to None for now. Blobs will be discussed in
a separate tutorial. *)

#r "Angara.Serialization.Json.dll"
let json = Json.Marshal(infoSet, None)

(** Deserialization is performed in reverse order. Serialized format is parsed into InfoSet by `Unmarshal' method, 
then InfoSet is converted to CLR object *)
let infoSet' = Json.Unmarshal(json, None)
let a' = infoSet |> ArtefactSerializer.Deserialize CoreSerializerResolver.Instance

(** `FromObject` and `ToObject` methods of `Json` class are convenient shortcuts that combine 
these two steps. Code snippet above can be rewrited in one line *)

let a'' = Json.ToObject<obj>(json, CoreSerializerResolver.Instance)

(**
###Type IDs 

The serialization library uses textual identifier (TypeID) to differentiate types of objects in the 
serialized representation. Primary benefit of this approach is independence of serialized
representation of .NET platform. Also TypeIDs can be chosen to be shorter than .NET full type 
names especially in case of generic types.

###Resolvers

Serialization process needs to know TypeID and function that converts object to InfoSets for each type being serialized. 
Deserialization process need to know CLR type and function to convert InfoSets to object for each Type ID found in serialized 
representation. Two way mapping of CLR types and TypeIDs with converter functions is a reponsibility of resolvers - 
objects that implement `Angara.Serialization.ISerializerResolver` interface.

Serialization library came with built-in resolver for primitives, arrays, F# lists, tuples and optional types. 
It is available as singleton by `Angara.Serialization.CoreSerializerResolver.Instance` property. 
*)

(**
###Creating custom serializer

Let's build serializer for `Vector2d` type *)

type Vector2d = { x: int; y: int }

(**
We need to define TypeID, serializer function that converts instances of Vector2d to InfoSet and deserializer function 
that converts InfoSets to Vector2d instances. This is done by creating serializer type - a type that implements 
`Angara.Serialization.ISerializer<Vector2d>` interface *)

type Vector2dSerializer () =
    interface ISerializer<Vector2d> with
        member x.TypeId = "V2D" // This is Type ID for Vector2d type
        member x.Serialize _ v = 
            InfoSet.EmptyMap.AddInt("x", v.x).AddInt("y", v.y)
        member x.Deserialize _ i = 
            let map = InfoSet.toMap i in { x = map.["x"].ToInt(); y = map.["y"].ToInt() } 

(** Design of `Angara.Serialization` library separates serialization responsibility from other responsibilities of an object.
This allows us to make any object serializable without modifying its code or creating adapter types. However this means that
we need to let serialization infrastructure know about our custom serializers.

To do this we can create SerializerLibrary - a resolver that is extensible with custom serializers. 
`SerializerLibrary.CreateDefault` method creates resolver that knows about all types supported by `CoreInstanceResolver` 
and allows to register serializers for user defined type.
*)

let lib = SerializerLibrary.CreateDefault()
lib.Register(Vector2dSerializer())

(** Then we pass our custom library to serialization infrastructure when performing serialization or deserialization *)
let v = { x = -1; y = 10 }
let vjson = Json.FromObject(lib, v)
let v' = Json.ToObject<Vector2d>(vjson, lib)

(** Notice `V2D` TypeID in the generated JSON representation*)

(*** include-value: vjson.ToString() ***)

