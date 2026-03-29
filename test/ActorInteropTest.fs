module Tests.ActorInterop

open Fable.Actor
open Fable.Reactive

open Expecto
open Tests.Utils

[<Tests>]
let tests =
    testList
        "ActorInterop Tests"
        [ testAsync "subscribeActor receives all notifications" {
              let xs = fromNotification [ OnNext 1; OnNext 2; OnNext 3; OnCompleted ]
              let received = System.Collections.Generic.List<Notification<int>>()
              let tcs = System.Threading.Tasks.TaskCompletionSource<unit>()

              let actor =
                  Actor.spawn (fun inbox ->
                      let rec loop () =
                          actor {
                              let! msg = inbox.Receive()
                              received.Add msg

                              match msg with
                              | OnCompleted -> tcs.SetResult()
                              | OnError _ -> tcs.SetResult()
                              | _ -> ()

                              return! loop ()
                          }

                      loop ())

              let! _sub = Reactive.subscribeActor actor xs
              do! Async.AwaitTask tcs.Task

              let actual = received |> Seq.toList
              let expected = [ OnNext 1; OnNext 2; OnNext 3; OnCompleted ]
              Expect.equal actual expected "Actor should receive all notifications"
              Actor.kill actor
          }

          testAsync "flatMapActor emits transformed values downstream" {
              let xs = fromNotification [ OnNext 1; OnNext 2; OnNext 3; OnCompleted ]

              let doubled =
                  xs
                  |> Reactive.flatMapActor (fun emit inbox ->
                      let rec loop () =
                          actor {
                              let! x = inbox.Receive()
                              emit (x * 2)
                              return! loop ()
                          }

                      loop ())

              let obv = TestObserver<int>()
              let! _sub = doubled.SubscribeAsync obv
              // Fire-and-forget emit is async, give actor time to process
              do! obv.AwaitIgnore()
              do! Async.Sleep 100

              let actual =
                  obv.Notifications
                  |> Seq.choose (function
                      | OnNext x -> Some x
                      | _ -> None)
                  |> Seq.toList

              Expect.equal (actual |> List.sort) [ 2; 4; 6 ] "Should emit doubled values"
          }

          testAsync "mapActor applies stateful transform with backpressure" {
              let xs = fromNotification [ OnNext 10; OnNext 20; OnNext 30; OnCompleted ]

              let withRunningSum =
                  xs |> Reactive.mapActor (fun sum x -> sum + x, sum + x) 0

              let obv = TestObserver<int>()
              let! _sub = withRunningSum.SubscribeAsync obv
              let! _ = obv.Await()

              let actual =
                  obv.Notifications
                  |> Seq.choose (function
                      | OnNext x -> Some x
                      | _ -> None)
                  |> Seq.toList

              Expect.equal actual [ 10; 30; 60 ] "Should emit running sums"
          }

          testAsync "ofActor creates observable from actor emissions" {
              let actor, obs =
                  Reactive.ofActor (fun emit _inbox ->
                      actor {
                          emit 42
                          do! Async.Sleep 10
                          emit 43
                      })

              let obv = TestObserver<int>()
              let! _sub = obs.SubscribeAsync obv
              // Give the actor time to emit
              do! Async.Sleep 200

              let actual =
                  obv.Notifications
                  |> Seq.choose (function
                      | OnNext x -> Some x
                      | _ -> None)
                  |> Seq.toList

              Expect.equal actual [ 42; 43 ] "Should emit actor values"
              Actor.kill actor
          } ]
