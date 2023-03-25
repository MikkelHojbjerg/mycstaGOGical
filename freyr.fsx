#r "nuget: Newtonsoft.Json, 13.0.3"
#r "nuget: FSharp.Data"
open System.Diagnostics
open Newtonsoft.Json
open System
open System.IO
open FSharp
open FSharp.Data
#load "valhalla.fsx"
open Valhalla


        

let fetchFromA (area: string) =
    let url = heimdall.urlA + (area.Replace (" ", "%20"))
    let response = Http.RequestString($"{url}", httpMethod = "POST", headers = [ "Authorization", $"Bearer {heimdall.token}" ])
    printfn $"Response from A: {response}"
    response

let fetchFromATime (area: string, backToTheFuture: string) =
    let url = heimdall.urlA + (area.Replace (" ", "%20"))
    let response = Http.RequestString($"{url}", query = ["time= ", $"{backToTheFuture}"], httpMethod = "POST", headers = [ "Authorization", $"Bearer {heimdall.token}" ])
    printfn $"Response from A: {response}"
    response

let sendToB (data: string) =
    let response = Http.RequestString($"{heimdall.urlB}", httpMethod = "POST", body = TextRequest data, headers = [ "Authorization", $"Bearer {heimdall.token}" ])
    printfn $"Response from B: {response}"

let backToTheFuture (area: string) =
    let mutable temp = new DateTime (2023, 03, 25, 11, 00, 00)
    while true do   
        temp <- temp.Subtract (new TimeSpan ( 1, 0, 0))
        let respond = fetchFromATime(area, urd.toIsoStringNoSec(temp))
        printfn $"{respond}"

let mutable lastUpdated = Map.empty
let run (area: string) =
    let response = fetchFromA area
    let (json, updated) = (kvasir.toJson (response, area))
    let changed = if (lastUpdated.ContainsKey area) then (updated <>  lastUpdated.[area]) else true

    if changed then
        sendToB json
        lastUpdated <- Map.add area updated lastUpdated
        printfn "changed"
    else 
        printfn "not changed"
    
    (changed, updated)


mimir.runAll (heimdall.areas, run)

