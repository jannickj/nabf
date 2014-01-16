module AgentLogicClientTest
    open System
    open NUnit.Framework
    open NabfAgentLogic.AgentLogic
    open NabfAgentLogic.AgentInterfaces;
    open JSLibrary.Data.GenericEvents;
    open JSLibrary.IiLang;
    open JSLibrary.IiLang.DataContainers;
    open System.Threading;
    open System.Linq;

    [<TestFixture>]
    type AgentLogicClientTest() = 
        
        [<Test>]
        member this.EvaluateState_twoDecisions_ConcludesFirst() = 
            let tree = 
                Options []
                    

            Assert.Fail
