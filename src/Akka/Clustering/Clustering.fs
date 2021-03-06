module Clustering
open Akka.Cluster.Sharding
open Akkling
open Akkling.Cluster.Sharding
open System.Configuration
open Akka.Cluster.Tools.Singleton
open Akkling


let config port = 
    sprintf """
            akka {
              actor {
                provider = cluster
                debug {
                    receive = on
                    autoreceive = on
                    lifecycle = on
                    event-stream = on
                    unhandled = on
                  }
                  serializers {
                    hyperion = "Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion"
                  }
                  serialization-bindings {
                    "System.Object" = hyperion
                  }
                }
                remote {
                  helios.tcp {
                    public-hostname = "localhost"
                    hostname = "localhost"
                    port = "%d"
                  }
                }
                cluster {
                    auto-down-unreachable-after = 5s
                    seed-nodes = [ "akka.tcp://test@localhost:5000" ]
                    roles = []
                }
                loglevel = INFO
                stdout-loglevel = INFO
            }""" port


type Msg =
    | Inc of int
    | Dec of int
    | GetState

[<EntryPoint>]
let main argv =
    let behavior (ctx : Actor<_>) msg = printfn "%A received %s" (ctx.Self.Path.ToStringWithAddress()) msg |> ignored


    let actor (ctx: Actor<Msg>) =
        let rec loop(state) = actor {
            let! msg = ctx.Receive()
            match msg with
            | Inc num ->
                return loop(state + num)
            | Dec num -> 
                return loop(state - num)
            | GetState ->
                ctx.Sender() <! state
                return loop(state)
        }
        loop(0)

    // spawn two separate systems with shard regions on each of them

    let system = System.create "test" (Configuration.parse(config(5000)).WithFallback(ClusterSingletonManager.DefaultConfig()))
    let fac1 = entityFactoryFor system "printer" <| props (actor)


    let system = System.create "test" (Configuration.parse(config(5001)).WithFallback(ClusterSingletonManager.DefaultConfig()))
    let fac2 = entityFactoryFor system "printer" <| props (actor)
    // wait a while before starting a second system
    System.Threading.Thread.Sleep 5000

    let entity1 = fac1.RefFor "shard-1" "entity-1"
    let john = fac2.RefFor "shard-1" "john"
    let alice = fac1.RefFor "shard-2" "alice"
    let frank = fac2.RefFor "shard-2" "frank"

    entity1 <! Inc(1)
    entity1 <! Inc(2)
    john <! Dec(1)
    alice <! Inc(312)
    frank <! Inc(2311)

    let frankState: int = frank <? GetState |> Async.RunSynchronously
    printfn "State: %d" frankState
    // check which shards have been build on the second shard region

    System.Threading.Thread.Sleep(5000)

    let printShards shardReg =
        async {
            let! (stats: ShardRegionStats) = (typed shardReg) <? GetShardRegionStats.Instance
            for kv in stats.Stats do
                printfn "\tShard '%s' has %d entities on it" kv.Key kv.Value
        } |> Async.RunSynchronously
        
    let printAllEntities shardReg =
        async {
            let! (state: CurrentShardRegionState) = (typed shardReg) <? (GetShardRegionState.Instance)
            for shard in state.Shards do
                for entity in shard.EntityIds do
                    printfn "Shard %s/%s/%s" "printer" shard.ShardId entity
        } |> Async.RunSynchronously

    printfn "Shards active on node 'localhost:5000':"
    printShards fac1.ShardRegion
    printfn "Shard entities"
    printAllEntities fac1.ShardRegion

    printfn "Shards active on node 'localhost:5001':"
    printShards fac2.ShardRegion
    printfn "Shard entities"
    printAllEntities fac2.ShardRegion
    printfn "%A" argv
    0 // return an integer exit code
