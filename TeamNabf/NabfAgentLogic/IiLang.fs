namespace IiLang
    module IiLangHandler =
        open JSLibrary.IiLang
        open JSLibrary.IiLang.Parameters
        open System.Xml.Serialization

        type Element = ParameterList of Element list
                     | Function      of (string * Element list)
                     | Numeral       of float
                     | Identifier    of string

        type Data = Percept of (string * Element)
                  | Action  of (string * Element)

        type PerceptCollection = Data list

        let rec buildIil (data : Element) : IilElement = 
            match data with
            | ParameterList ls -> 
                new IilParameterList (List.map buildIil ls |> List.toArray) 
                :> IilElement
            | Function (name, ls) -> 
                new IilFunction (name, List.map buildIil ls |> List.toArray) 
                :> IilElement
            | Numeral num -> 
                new IilNumeral (num) 
                :> IilElement
            | Identifier str -> 
                new IilIdentifier (str) 
                :> IilElement

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

   