#![feature(panic_handler)]
#![feature(unsafe_no_drop_flag)]

extern crate toml_document;
extern crate winapi;
extern crate kernel32;

use std::mem;
use std::ptr;
use std::slice;
use std::str;

use toml_document::{Document, EntryRef};
use winapi::INT32;

mod panic;
pub mod capi;

fn entry_kind(e: EntryRef) -> &'static str {
    match e {
        EntryRef::String(..) => "string",
        EntryRef::Integer(..) => "integer",
        EntryRef::Float(..) => "float",
        EntryRef::Boolean(..) => "boolean",
        EntryRef::Datetime(..) => "datetime",
        EntryRef::Array(..) => "array",
        EntryRef::Table(..) => "table",
    }
}

pub struct Manifest {
    doc: Document
}

// Set functions:
// * if no table exists, create new top-level one
// * overwrites value, even if it has a value but wrong type 
impl Manifest {
    pub fn new(doc: Document) -> Manifest {
        Manifest { doc: doc }
    }

    pub fn get_string<'a>(&'a self, path: &'a [&'a str]) -> Result<&'a str, QueryError> {
        match Manifest::lookup(&self.doc, path) {
            Ok(EntryRef::String(value)) => Ok(value.get()),
            Ok(entry) => Err(QueryError::Conflict { depth: path.len(), kind: entry_kind(entry) }),
            Err(err) => Err(err)
        }
    }

    // It's the caller responsibility to make sure we are not
    // setting value on a conflicting path, eg. for
    //   [[a]]
    //   b = "c"
    // `set_string(&["a", "b"], "c")` will simply panic
    pub fn set_string<'a>(&'a mut self, _: &'a [&'a str], _: &'a str) -> bool {
        unimplemented!()
    }

    fn lookup<'a>(doc: &'a Document,path: &'a [&'a str]) -> Result<EntryRef<'a>, QueryError> {
        fn lookup_inner<'a>(entry: EntryRef<'a>,
                            path: &'a [&'a str],
                            depth: usize)
                            -> Result<EntryRef<'a>, QueryError> {
            if path.len() == 0 {
                Ok(entry)
            } else {
                match entry {
                    EntryRef::Table(table) => {
                        table.get(path[0])
                             .map_or_else(|| Err(QueryError::Vacant{ depth: depth }),
                                          |e| lookup_inner(e, &path[1..], depth + 1))
                    }
                    _ => {
                        Err(QueryError::Conflict { depth: depth, kind: entry_kind(entry) })
                    }
                }
            }
        }
        doc.get(path[0])
           .map_or(Err(QueryError::Vacant{ depth: 0 }),
                   |entry| lookup_inner(entry, &path[1..], 0))
    }
}
impl std::panic::RefUnwindSafe for Manifest { }

pub enum QueryError {
    Vacant{ depth: usize },
    Conflict{ depth: usize, kind: &'static str }
}


#[repr(C)]
pub struct StrBox {
    buff: *mut u8,
    len: INT32
}

impl StrBox {
    fn new(s: &str) -> StrBox {
        let mut text = s.to_string().into_bytes().into_boxed_slice();
        let result = StrBox {
            buff: text.as_mut_ptr(),
            len: text.len() as INT32
        };
        mem::forget(text);
        result
    }

    fn empty() -> StrBox {
        StrBox {
            buff: ptr::null_mut(),
            len: 0
        }
    }

    fn as_str(&self) -> &str {
        unsafe {
            let slice = slice::from_raw_parts(self.buff, self.len as usize);
            str::from_utf8_unchecked(slice)
        }
    }
}

#[repr(C)]
#[unsafe_no_drop_flag]
pub struct OwnedStrBox {
    data: StrBox
}

impl OwnedStrBox {
    fn new(s: &str) -> OwnedStrBox {
        OwnedStrBox {
            data: StrBox::new(s)
        }
    }

    fn empty() -> OwnedStrBox {
        OwnedStrBox {
            data: StrBox::empty()
        }
    }
}