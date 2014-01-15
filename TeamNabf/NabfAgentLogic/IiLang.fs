namespace IiLang
    module IiLangHandler =
        open JSLibrary.IiLang
        open JSLibrary.IiLang.Parameters
        open JSLibrary.IiLang.DataContainers
        open System.Xml.Serialization

        type Element = ParameterList of Element list
                     | Function      of (string * Element list)
                     | Numeral       of float
                     | Identifier    of string

        type Data = Percept of (string * Element list)
                  | Action  of (string * Element list)

        type PerceptCollection = Data list

        let rec buildIil (data : Element) : IilParameter = 
            let unpackParameters ls = List.map buildIil ls |> List.toSeq<IilParameter>
            match data with
            | ParameterList ls ->
                new IilParameterList (unpackParameters ls) 
                :> IilParameter
            | Function (name, ls) -> 
                new IilFunction (name, unpackParameters ls)
                :> IilParameter
            | Numeral num -> 
                new IilNumeral (num) 
                :> IilParameter
            | Identifier str -> 
                new IilIdentifier (str) 
                :> IilParameter

        let rec evalIil (iil : IilParameter) = 
            match iil with
            | :? Parameters.IilParameterList as paramlist 
                -> ParameterList (List.ofSeq paramlist.Parameters |> List.map evalIil)
            | :? Parameters.IilFunction as func           
                -> Function (func.Name, List.ofSeq func.Parameters |> List.map evalIil)
            | :? Parameters.IilIdentifier as id           
                -> Identifier id.Value
            | :? Parameters.IilNumeral as num             
                -> Numeral num.Value
            | _ -> failwith "the object %O is not a recognized iilang element (IilElement)"

        let parsePercept (iilPercept : IilPercept) = 
            Percept <| (iilPercept.Name, List.ofSeq iilPercept.Parameters |> List.map evalIil) 

   