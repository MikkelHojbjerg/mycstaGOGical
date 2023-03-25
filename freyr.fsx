#r "nuget: Newtonsoft.Json, 13.0.3"
#r "nuget: FSharp.Data"
open System.Diagnostics
open Newtonsoft.Json
open System
open System.IO
open FSharp
open FSharp.Data


let fetchFromA (area: string) =
    let url = urlA + (area.Replace (" ", "%20"))
    let response = Http.RequestString($"{url}", httpMethod = "POST", headers = [ "Authorization", $"Bearer {token}" ])
    printfn $"Response from A: {response}"
    response


let sendToB (data: string) =
    let response = Http.RequestString($"{urlB}", httpMethod = "POST", body = TextRequest data, headers = [ "Authorization", $"Bearer {token}" ])
    printfn $"Response from B: {response}"

