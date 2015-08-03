#r "FSharp.Data.dll"
#r "System.Xml.Linq.dll"
open FSharp.Data
open System.Xml
open System.Xml.Linq

type Fragment = XmlProvider<"gdb.2013.i686.wxs", Global=true>

let inline (!>) (x:^a) : ^b = ((^a or ^b) : (static member op_Implicit : ^a -> ^b) x)

let frag = Fragment.Load("gdb.2013.i686.wxs")

let set_cmp_guid (comp: Fragment.Component) =
    comp.XElement.Attribute(!> "Guid").SetValue(System.Guid.NewGuid().ToString("B"))

let rec iter_dir (dir: Fragment.Directory) =
    dir.Directories |> Array.iter iter_dir
    dir.Components |> Array.iter set_cmp_guid

let dir_ref = [| frag.Fragments.[0].DirectoryRef.Value.Directory |] |> Seq.iter iter_dir 
frag.XElement.Save(__SOURCE_DIRECTORY__ + "\\edited_guid.xml", SaveOptions.None)