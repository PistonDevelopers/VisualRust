#r "FSharp.Data.dll"
#r "System.Xml.Linq.dll"
open FSharp.Data
open System.Xml
open System.Xml.Linq

// Original file command:
// heat dir gdb\i686 -o gdb.2013.i686.wxs -gg -sfrag -cg CmpGroup_gdb.2013.i686

type Fragment = XmlProvider<"gdb.2013.i686.wxs", Global=true>

let inline (!>) (x:^a) : ^b = ((^a or ^b) : (static member op_Implicit : ^a -> ^b) x)

let frag = Fragment.Load("gdb.2013.i686.wxs")

let set_id (text: string) (xelm: XElement) =
    xelm.Attribute(!> "Id").SetValue(text.Replace('-', '_'))


let set_dir_id (pars: seq<string>) (dir: Fragment.Directory) =
    set_id ("Dir_gdb_" + System.String.Join("_", pars) + "_" + dir.Name + ".2013.i686") dir.XElement

let set_cmp_id (cmp_group: Fragment.ComponentGroup) (comp: Fragment.Component) = 
    let file_name = (comp.File.Source.Split('\\') |> Seq.last) + ".2013.i686"
    let set_id (xelm: XElement) =
        set_id file_name xelm
    let old_id = comp.Id
    set_id comp.XElement
    set_id comp.File.XElement
    let cref = cmp_group.ComponentRefs |> Array.find (fun cref -> cref.Id = old_id)
    set_id cref.XElement

let rec iter_dir (cmp_group: Fragment.ComponentGroup) (pars: seq<string>) (dir: Fragment.Directory) =
    dir.Directories |> Array.iter (iter_dir cmp_group (Seq.append pars [| dir.Name |]))
    set_dir_id pars dir
    dir.Components |> Array.iter (fun cmp -> set_cmp_id cmp_group cmp)

let dir_ref = [| frag.Fragments.[0].DirectoryRef.Value.Directory |] |> Seq.iter (iter_dir frag.Fragments.[1].ComponentGroup.Value [| |])
frag.XElement.Save(__SOURCE_DIRECTORY__ + "\\edited_id.xml", SaveOptions.None)