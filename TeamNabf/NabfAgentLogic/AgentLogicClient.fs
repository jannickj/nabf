namespace NabfAgentLogic
    open NabfAgentLogic.AgentInterfaces;
    open JSLibrary.Data.GenericEvents;
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;

    type AgentLogicClient() = 
        
        let JobCreatedEvent = new Event<UnaryValueHandler<IilAction>, UnaryValueEvent<IilAction>>()
        
        interface IAgentLogic with
            member this.EvaluateJob(id) = (null,false)
            member this.HandlePercepts(percepts) = ()
            member this.GetJobs = [(1,1)]
            member this.Start = ()
            member this.CurrentDecision = null
            [<CLIEvent>]
            member this.JobCreated = JobCreatedEvent.Publish