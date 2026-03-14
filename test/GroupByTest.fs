module Tests.GroupBy

open Fable.Reactive
open Expecto
open Tests.Utils

exception MyError of string

[<Tests>]
let tests = testList "GroupBy Tests" [

    testAsync "Test groupby empty" {
        // Arrange
        let xs = Reactive.empty<int> ()
                |> Reactive.groupBy (fun _ -> 42)
                |> Reactive.flatMap id
        let obv = TestObserver<int>()

        // Act
        let! sub = xs.SubscribeAsync obv

        try
            do! obv.AwaitIgnore ()
        with
        | _ -> ()

        // Assert
        Expect.equal obv.Notifications.Count 1 "Should be equal"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnCompleted ]
        Expect.equal actual expected "Should be equal"
    }

    testAsync "Test groupby error" {
        // Arrange
        let error = MyError "error"
        let xs = Reactive.fail<int> error
                |> Reactive.groupBy (fun _ -> 42)
                |> Reactive.flatMap id
        let obv = TestObserver<int>()

        // Act
        let! sub = xs.SubscribeAsync obv

        try
            do! obv.AwaitIgnore ()
        with
        | _ -> ()

        // Assert
        Expect.equal obv.Notifications.Count 1 "Should be equal"
        let actual = obv.Notifications |> Seq.toList
        let expected : Notification<int> list = [ OnError error ]
        Expect.equal actual expected "Should be equal"
    }

    testAsync "Test groupby 2 groups" {
        // Arrange
        let xs = Reactive.ofSeq [1; 2; 3; 4; 5; 6]
                |> Reactive.groupBy (fun x -> x % 2)
                |> Reactive.flatMap (fun x -> x |> Reactive.min)
        let obv = TestObserver<int> ()

        // Act
        let! sub = xs.SubscribeAsync obv

        try
            do! obv.AwaitIgnore ()
        with
        | _ -> ()

        // Assert
        Expect.equal obv.Notifications.Count 3 "Should be equal"
        let actual = obv.Notifications |> Seq.toList
        let expected = [
            [ OnNext 1; OnNext 2; OnCompleted ]
            [ OnNext 2; OnNext 1; OnCompleted ]
        ]
        Expect.contains expected actual  "Should be equal"
    }

    testAsync "Test groupby cancel" {
        // Arrange
        let xs = Reactive.ofSeq [1; 2; 3; 4; 5; 6]
                |> Reactive.groupBy (fun x -> x % 2)
                |> Reactive.flatMap id
        let obv = TestObserver<int> ()

        // Act
        let! sub = xs.SubscribeAsync obv
        do! sub.DisposeAsync ()

        // Assert
        Expect.isLessThan obv.Notifications.Count 8 "Should be less"
    }
]