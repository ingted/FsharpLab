module Clustering
open Akka.Cluster.Sharding
open Akkling
open Akkling.Cluster.Sharding
open System.Configuration
open Akka.Cluster.Tools.Singleton


[<EntryPoint>]
let main argv =
    let behavior (ctx : Actor<_>) msg = printfn "%A received %s" (ctx.Self.Path.ToStringWithAddress()) msg |> ignored

    // spawn two separate systems with shard regions on each of them

    let system = System.create "test" (Configuration.load().WithFallback(ClusterSingletonManager.DefaultConfig()))
    let fac1 = entityFactoryFor system "printer" <| props (actorOf2 behavior)

    // wait a while before starting a second system
    System.Threading.Thread.Sleep 5000

    let entity1 = fac1.RefFor "shard-1" "entity-1"
    let john = fac1.RefFor "shard-2" "john"
    let alice = fac1.RefFor "shard-3" "alice"
    let frank = fac1.RefFor "shard-4" "frank"

    entity1 <! "hello"
    entity1 <! " world"
    john <! "hello John"
    alice <! "hello Alice"
    frank <! "hello Frank"

    // check which shards have been build on the second shard region

    System.Threading.Thread.Sleep(5000)

    let printShards shardReg =
        async {
            let! (stats: ShardRegionStats) = (typed shardReg) <? GetShardRegionStats.Instance
            for kv in stats.Stats do
                printfn "\tShard '%s' has %d entities on it" kv.Key kv.Value
        } |> Async.RunSynchronously

    printfn "Shards active on node 'localhost:5000':"
    printShards fac1.ShardRegion
    printfn "%A" argv
    0 // return an integer exit code
