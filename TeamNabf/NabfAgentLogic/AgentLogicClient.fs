namespace NabfAgentLogic
    open NabfAgentLogic.AgentInterfaces;
    open JSLibrary.Data.GenericEvents;
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open System;
    open NabfAgentLogic.AgentLogic;
    open System.Threading;

    type public AgentLogicClient() = 
        
        
        [<DefaultValue>] val mutable private reEvalNeeded : bool
        [<DefaultValue>] val mutable private BeliefData : State
        [<DefaultValue>] val mutable private AvailableJobs : List<Job*Desirability>
        [<DefaultValue>] val mutable private localActions : Action List
        [<DefaultValue>] val mutable private decidedActions : List<Action*Desirability>
        
        //Parallel helpers
        let stopDeciders = new CancellationTokenSource()
        let actionDeciderLock = new Object()



        let JobCreatedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        let JobLoadedEvent = new Event<UnaryValueHandler<JobID>, UnaryValueEvent<JobID>>()
        let PerceptsLoadedEvent = new Event<EventHandler, EventArgs>()
        let EvaluationCompletedEvent = new Event<EventHandler, EventArgs>()

        member private this.ReEvaluate =
            stopDeciders.Cancel()
            this.localActions <- generateActions this.BeliefData
            let actionDecider state action =
                async
                    {
                        let desire = actionDesirability state action
                        lock actionDeciderLock (fun () -> this.decidedActions <- (action,desire) :: this.decidedActions)
                    }
                        
            List.iter (fun action -> Async.Start ((actionDecider this.BeliefData action),stopDeciders.Token)) this.localActions
                        
            ()

        interface IAgentLogic with
            member this.EvaluateJob(id) = (null,false)
            member this.HandlePercepts(iilpercepts) = 
                
                let percepts = (parseIilPercepts iilpercepts)
                match percepts with
                | a -> ()                    
                | _ ->
                    this.BeliefData <- updateState this.BeliefData percepts
                    this.reEvalNeeded <- true
                    let newJobs = generateJobs this.BeliefData (List.map (fun (job, _) -> job) this.AvailableJobs )
                    ()


            member this.GetJobs = [(1,1)]
            member this.Start = 
                while true do
                    if this.reEvalNeeded then
                        stopDeciders.Cancel()
                        this.localActions <- generateActions this.BeliefData
                        let actionDecider state action =
                            async
                                {
                                    let desire = actionDesirability state action
                                    lock actionDeciderLock (fun () -> this.decidedActions <- (action,desire) :: this.decidedActions)
                                }
                        
                        List.iter (fun action -> Async.Start ((actionDecider this.BeliefData action),stopDeciders.Token)) this.localActions
                        
                        ()
               
            member this.CurrentDecision = 
                let (bestAction,_) = lock actionDeciderLock (fun () -> List.maxBy snd this.decidedActions)
                buildIilAction bestAction


            [<CLIEvent>]
            member this.JobCreated = JobCreatedEvent.Publish
            [<CLIEvent>]
            member this.JobLoaded = JobLoadedEvent.Publish
            [<CLIEvent>]
            member this.PerceptsLoaded = PerceptsLoadedEvent.Publish
            [<CLIEvent>]
            member this.EvaluationCompleted = EvaluationCompletedEvent.Publish