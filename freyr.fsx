#r "nuget: FSharp.Data"
open System.Diagnostics
open System.Text.Json
open System
open System.IO
open FSharp.Data
#load "valhalla.fsx"
open Valhalla

//Gets data from a
let fetchFromA (area: string) =
    //Defines url and adds a area code
    let url = heimdal.url_A + (area.Replace (" ", "%20"))
    //Sends requests
    Http.RequestString($"{url}", httpMethod = "POST", headers = [ "Authorization", $"Bearer {heimdal.token}" ])

//Sends data to b
let sendToB (data: string) =
    //Defines the data obtained from fetch and pushes it to b
    let response = Http.RequestString($"{heimdal.url_B}", httpMethod = "POST", body = TextRequest data, headers = [ "Authorization", $"Bearer {heimdal.token}" ])
    //prints data
    printfn $"Response from B: {response}"


let mutable lastJson = ""
let run () =
        for area in heimdal.areas do
            let response = fetchFromA area
            let json = (thor.toJson (response, area))
            if json <> lastJson then
                sendToB json
                lastJson <- json
                printfn "changed"
                printfn "%s\n\n%s" json lastJson
            else
                printfn "not changed"
