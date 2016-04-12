
(** 
###Serializers, Resolvers, Libraries and Typeids *)

(** The Serialization service uses textual identifier (typeid) to differentiate types of objects in the 
serializable representation. Primary benefit of this approach is independence of serializable 
representation of .NET platform. Also type ids can be chosen to be shorter than .NET full type 
names especially in case of generic types.

Current implementation needs to know type id and serialization function in order to convert an 
artefact into serializable representation and it needs deserialization function for type id 
in order to restore artefact from serializable representation.

SerializerResolvers are responsible for returning serializer for given CLR Type or typeid.

SerializerLibrary is a SerializerResolver that can looks among serializers added by user. *)

(**
###Creating custom serializer

Let's build and use serializer for `Vector2d` type *)

type Vector2d = { x: double; y: double }

(**

Serializable artefacts can be converted to `InfoSet` object 
and restored from 'InfoSet'. 

Artefacts of type 'T are serialized and deserialized by some object implementing type ISerializer<'T>. 
Serializer types has serialization and deserializationb method and property for Type ID. 
*)

type Vector2dSerializer () =
    interface ISerializer<Vector2d> with
        member x.TypeId = "V2D"
        member x.Serialize _ v = 
            InfoSet.EmptyMap.AddDouble("x", v.x).AddDouble("y", v.y)
        member x.Deserialize _ v = 
            let map = InfoSet.toMap v in { x = map.["x"].ToDouble(); y = map.["y"].ToDouble() } 

(** We need to let Serialization infrastructure to know about serializer for `Vector2d`. To do this
we create library, add our serializer to library and use it in invocations of serialization methods 

`CreateDefault` method creates serializer library which has all core types in it*)

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

