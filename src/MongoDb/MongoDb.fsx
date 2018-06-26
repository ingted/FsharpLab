// Before running any code, invoke Paket to get the dependencies.
//
// You can either build the project (Ctrl + Alt + B in VS) or run
// '.paket/paket.bootstrap.exe' and then '.paket/paket.exe install'
// (if you are on a Mac or Linux, run the 'exe' files using 'mono')
//
// Once you have packages, use Alt+Enter (in VS) or Ctrl+Enter to
// run the following in F# Interactive. You can ignore the project
// (running it doesn't do anything, it just contains this script)
#r "../../packages/DnsClient/lib/net45/DnsClient.dll"
#r "../../packages/MongoDB.Bson/lib/net45/MongoDB.Bson.dll"
#r "../../packages/MongoDB.Driver.Core/lib/net45/MongoDB.Driver.Core.dll"
#r "../../packages/MongoDB.Driver/lib/net45/MongoDB.Driver.dll"
#load "..\..\packages/FsLab/FsLab.fsx"
open MongoDB.Driver
open System
open Deedle
open FSharp.Data
open XPlot.GoogleCharts
open XPlot.GoogleCharts.Deedle
open MongoDB.Bson.Serialization.Attributes


// Some composite primary key
type Id = { Id: int; Date: DateTime }

type Person(id: int, day: DateTime, name: string) =
   new () = Person(0, DateTime.Today, "")
    
   [<BsonId>] member val Id: Id = { Id = id; Date = day } with get, set
   [<BsonElement>] member val Name: string = name with get,set

   override this.ToString() =
    sprintf "Id: %A, Name: %s" this.Id this.Name

let client = MongoClient("mongodb://127.0.0.1:27017")
let db = client.GetDatabase("Test")
let persons = db.GetCollection<Person>("persons")

//persons.InsertMany([| Person(0, DateTime.Today, "Dominik"); Person(0, DateTime.Today.AddDays(1.), "Dominik tommorow"); |])
let tommorow = (DateTime.Today.AddDays(1.))
let find = persons.Find(fun p -> p.Id = { Id = 0; Date = tommorow })
find.ToList().ToArray()