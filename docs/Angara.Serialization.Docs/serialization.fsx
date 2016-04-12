(*** hide ***)

#I __SOURCE_DIRECTORY__
#I "./bin/Debug"
#r "Newtonsoft.Json.dll"
#r "bin/Debug/Angara.Serialization.dll"
#r "bin/Debug/Angara.Serialization.Json.dll"
#r "bin/Debug/Newtonsoft.Json.dll"

(** 
Angara.Serialization
====================

A library that facilitates serialization on both .NET and JavaScript platforms and communication between them.  
The library contains built-in serializers for a wide range of types and can easily be extended with serializers for user-defined types.
Serialization formats are pluggable. The shipped implementation is based on JSON.

Example
-------

Here is a simple example how object can be converted to JSON format and restored back.

Let's prepare a tuple of double array, integer array and string value.
*)

open System
let x = [| 0.0; Math.PI / 6.0; 3.0 * Math.PI / 2.0  |]
let y = x |> Array.map (fun x -> sign(sin(x)))
let t = "sign(sin(x))", x, y

(**
Next lines of code converts tuple `t` to a JSON format. Passing `CoreSerializerResolver.Instance` 
as a first argument means that built-in serializers will be used.
*)

#r "Angara.Serialization.dll"
#r "Angara.Serialization.Json.dll"
open Angara.Serialization

let json = Json.FromObject(CoreSerializerResolver.Instance, t)

(** Text representation of resulting JSON is shown below. This JSON can be persisted to a file or transferred
over network. 
*)
(***include-value:json.ToString()***)

(** We introduce special conventions to embed type information into serialized representation.
Numeric arrays are converted to [base64](https://tools.ietf.org/html/rfc3548#page-4) representation
in order to save their exact values and reduce serialized representation size. `Angara.Serialization` library
has JavaScript component that parses and generates JSON conforming to these conventions.
 *)

(** Information about types is included into serialized representation. This allows to restore object
without specifying it's exact type. Next snippet of code restores object from JSON and prints its type*)

(*** define-output:ttype ***)
let o = Json.ToObject<obj>(json, CoreSerializerResolver.Instance)
printfn "%A" (o.GetType())

(** Object is correctly restored as a tuple of two arrays and a string*)
(*** include-output:ttype ***)

(** It is more convenient to specify deserialized object type if you know it. Next single line
of code restores tuple *)
let s',x',y' = 
    Json.ToObject<string * double[] * int[]>(json, CoreSerializerResolver.Instance)

(** Value of restored array `y'` is *)
(*** include-value:y' ***)

