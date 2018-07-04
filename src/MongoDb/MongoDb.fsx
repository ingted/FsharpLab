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
open Bogus
#r "../../packages/MongoDB.Bson/lib/net45/MongoDB.Bson.dll"
#r "../../packages/MongoDB.Driver.Core/lib/net45/MongoDB.Driver.Core.dll"
#r "../../packages/MongoDB.Driver/lib/net45/MongoDB.Driver.dll"
#r "../../packages/Bogus/lib/net40/Bogus.dll"
#r "../../packages/NodaTime/lib/net45/NodaTime.dll"

open MongoDB.Driver
open System
open FSharp.Data
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Bson
open NodaTime.Extensions
open NodaTime

[<CLIMutable>]
type Alarm = { [<BsonId>] Id: ObjectId; UserId: int; AlarmTime: int64 }

let client = MongoClient("mongodb://127.0.0.1:27017")
let db = client.GetDatabase("SmartAlarm")
let alarms = db.GetCollection<Alarm>("alarms")

let generateFakeAlarms(q: int) =
    let f = Faker<Alarm>()
                .CustomInstantiator(fun f -> 
                                { Id = ObjectId.GenerateNewId()
                                  UserId = f.Random.Number(0, 1000) 
                                  AlarmTime = f.Date.Future().ToEpoch()}
                            )
    f.Generate(q)
let als = generateFakeAlarms(10000).ToArray()
alarms.InsertMany(als)
// let tommorow = (DateTime.Today.AddDays(1.))
// let find = persons.Find(fun p -> p.Id = { Id = 0; Date = DateTime.Today })
// find.ToList().ToArray()