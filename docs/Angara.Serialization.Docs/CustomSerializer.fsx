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

This tutorial describes how to create serializer for user defined type. 
We need to introduce some concepts first.

###InfoSet

InfoSet is an intermediate tree-based representation that uses limited set of .NET types. Purpose of InfoSet is to decouple object graph parsing tasks from serialized representation. 
InfoSets are represented by instances of `Angara.Serialization.InfoSet` discriminated union. Serialization is performed in two steps. On the first step an object is converted to an InfoSet using 
`ArtefactSerializer.Serialize` method as shown in the next code snippet
*)
#r "Angara.Serialization.dll"
open Angara.Serialization
let a = [| for i in 0..100 -> i*i |] // An object to serialize
let infoSet = a |> ArtefactSerializer.Serialize CoreSerializerResolver.Instance

(** Then InfoSet is converted to serialized representation. `Angara.Serialization.Json` class 
provides support for JSON representation. Method `Marshal` converts InfoSet to JSON Second
parameter is related to blob support and can be set to None for now. Blobs will be discussed in
a separate tutorial. *)

#r "Angara.Serialization.Json.dll"
let json = Json.Marshal(infoSet, None)

(** Deserialization is performed in reverse order *)
let infoSet' = Json.Unmarshal(json, None)
let a' = infoSet |> ArtefactSerializer.Deserialize CoreSerializerResolver.Instance

(** `FromObject` and `ToObject` methods of Json class are convenient shortcuts that combine two steps descibed above.
Above code snippet can be rewrited in one line *)

let a'' = Json.ToObject<obj>(json, CoreSerializerResolver.Instance)

(**
###Type IDs 

The serialization library uses textual identifier (TypeID) to differentiate types of objects in the 
serialized representation. Primary benefit of this approach is independence of serialized
representation of .NET platform. Also TypeIDs can be chosen to be shorter than .NET full type 
names especially in case of generic types.

###Resolvers

Serialization implementation needs to know TypeID and function that converts objects to InfoSets for each type being serialized. Also 
it needs to know CLR type and function to convert InfoSets to object for each type id found in serialized representation. This is reponsibility of resolvers - objects
providing `Angara.Serialization.ISerializerResolver` interface.

Singleton instance of `Angara.Serialization.CoreSerializerResolver` resolves many .NET base types: primitives, arrays, F# lists, tuples and optional types.
*)

(**
###Creating custom serializer

Let's build serializer for `Vector2d` type *)

type Vector2d = { x: double; y: double }

(**
We need to define Type ID, serialization function (converter to InfoSet) and deserialization function (converter from InfoSet) for a 
certain type to make it serializable. This is done by creating type implementing `Angara.Serialization.ISerializer<T>` interface*)

type Vector2dSerializer () =
    interface ISerializer<Vector2d> with
        member x.TypeId = "V2D" // This is Type ID for Vector2d type
        member x.Serialize _ v = 
            InfoSet.EmptyMap.AddDouble("x", v.x).AddDouble("y", v.y)
        member x.Deserialize _ v = 
            let map = InfoSet.toMap v in { x = map.["x"].ToDouble(); y = map.["y"].ToDouble() } 

(** We need to let Serialization infrastructure to know about serializer for `Vector2d`. To do this
we create library, add our serializer to library and use it in invocations of serialization methods 

`CreateDefault` method creates serializer library which has all core types in it
Libraries are basically SerializerResolver that can looks among serializers added by user.

*)

let res = SerializerLibrary.CreateDefault()
res.Register(Vector2dSerializer())

let v = { x = -1.0; y = 1.0 }
let vjson = Json.FromObject(res, v)

(** Resulting JSON is *)

(*** include-value: vjson.ToString() ***)

(** 
###Access to serialization infrastructure inside serializers

Next example describes first argument of the serializer/deserializer methods.
*)

type MapSerializer () = 
    interface ISerializer<Map<string, obj>> with
        member x.TypeId = "Map"
        member x.Serialize resolver obj = 
            obj |> Map.fold (fun infoSet key value -> 
                infoSet.AddInfoSet(key, ArtefactSerializer.Serialize resolver value)) InfoSet.EmptyMap
        member x.Deserialize resolver infoSet = 
            infoSet.ToMap() |> Map.map (fun _ value -> ArtefactSerializer.Deserialize resolver value)

let map = Map.empty<string,obj>.Add("Pi", 3.1415 :> obj).Add("Vector", { x = 10.0; y = -20.0 } :> obj)
res.Register(MapSerializer())
let mjson = Json.FromObject(res, map)

(*** include-value: mjson.ToString() ***)

