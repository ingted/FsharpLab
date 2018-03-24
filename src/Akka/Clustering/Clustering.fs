module Clustering
open Akka.Cluster.Sharding
open Akkling
open Akkling.Cluster.Sharding
open System.Configuration
open Akka.Cluster.Tools.Singleton
open Akkling.Cluster.Sharding
open Akka.Actor
open Akkling.Cluster.Sharding

type ShardEnvelope = { EntityId: string; Payload: obj }

type MessageExtractor<'Envelope, 'Message>(maximumShardQuantity: int) =
    inherit HashCodeMessageExtractor(maximumShardQuantity)

    override this.EntityId(message) =   
        let envelope = (message :?> ShardEnvelope)
        envelope.EntityId
    
    override this.EntityMessage(message) =
        let envelope = (message :?> ShardEnvelope)
        envelope.Payload    

let entityFactoryFor (system: ActorSystem) (name: string) (props: Props<'Message>) : EntityFac<'Message> =
    let clusterSharding = ClusterSharding.Get(system)
    let adjustedProps = props
    let shardRegion = clusterSharding.Start(name, adjustedProps.ToProps(), ClusterShardingSettings.Create(system), new MessageExtractor<_,_>(10))
    { ShardRegion = shardRegion; TypeName = name }
            

[<EntryPoint>]
let main argv =
    let behavior (ctx : Actor<_>) msg = printfn "%A received %s" (ctx.Self.Path.ToStringWithAddress()) msg |> ignored

    // spawn two separate systems with shard regions on each of them

    let system = System.create "test" (Configuration.load().WithFallback(ClusterSingletonManager.DefaultConfig()))
    let fac1 = entityFactoryFor system "printer" <| props (actorOf2 behavior)

    // wait a while before starting a second system
    System.Threading.Thread.Sleep 5000

    let entity1 = fac1.ShardRegion.Tell({ EntityId = "john"; Payload = "Hello" })


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
