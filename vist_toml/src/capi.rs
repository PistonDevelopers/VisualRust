use std::marker::PhantomData;
use std::panic as std_panic;
use std::ptr;
use std::slice;

use toml_document::{Document, ParserError};
use winapi::{INT32, BOOL};

use super::*;
use panic::*;

#[repr(C)]
pub struct ParseResult {
    pub manifest: *mut Manifest,
    pub error: OwnedStrBox
}

impl ParseResult {
    fn success(doc: Document) -> ParseResult {
        ParseResult {
            manifest: Box::into_raw(Box::new(Manifest::new(doc))),
            error: OwnedStrBox::empty()
        }
    }

    fn error(err: ParserError) -> ParseResult {
        ParseResult {
            manifest: ptr::null_mut(),
            error: OwnedStrBox::new(&err.desc)
        }
    }
}

#[repr(C)]
pub struct PathMismatch {
    pub path: StrBox,
    pub expected: StrBox,
    pub got: StrBox
}

pub struct BorrowedStrBox<'a> {
    data: StrBox,
    marker: PhantomData<&'a StrBox>
}

impl<'a> BorrowedStrBox<'a> {
    fn empty() -> BorrowedStrBox<'static> {
        BorrowedStrBox {
            data: StrBox::empty(),
            marker: PhantomData
        }
    }

    fn from_static(string: &'static str) -> BorrowedStrBox<'static> {
        BorrowedStrBox {
            data: StrBox {
                buff: string.as_ptr() as *mut _,
                len: string.len() as INT32
            },
            marker: PhantomData
        }
    }

    fn as_str(&'a self) -> &'a str {
        self.data.as_str()
    }
}

impl Drop for OwnedStrBox {
    fn drop(&mut self) {
        let this = &mut self.data;
        if this.buff != ptr::null_mut() {
            let slice = unsafe { slice::from_raw_parts_mut(this.buff, this.len as usize) };
            drop(unsafe { Box::<[u8]>::from_raw(slice) });
            this.buff = ptr::null_mut();
            this.len = 0;
        }
    }
}

#[repr(C)]
pub struct RawSlice<T> {
    arr: *mut T,
    len: INT32
}

#[repr(C)]
pub struct QueryResult<T> {
    result: T,
    error: QueryErrorFFI
}

impl QueryResult<OwnedStrBox> {
    fn from_string_result(r: Result<&str, QueryError>) -> QueryResult<OwnedStrBox> {
        match r {
            Ok(string) => QueryResult {
                result: OwnedStrBox::new(string),
                error: QueryErrorFFI::empty()
            },
            Err(QueryError::Vacant{ depth }) => QueryResult {
                result: OwnedStrBox::empty(),
                error: QueryErrorFFI::vacant(depth)
            },
            Err(QueryError::Conflict{ depth, kind }) => QueryResult {
                result: OwnedStrBox::empty(),
                error: QueryErrorFFI::conflict(depth, kind)
            },
        }
    }
}

#[repr(C)]
pub struct QueryErrorFFI {
    depth: INT32,
    found_entry: BorrowedStrBox<'static>
}

impl QueryErrorFFI {
    fn empty() -> QueryErrorFFI {
        QueryErrorFFI {
            depth: 0,
            found_entry: BorrowedStrBox::empty()
        }
    }

    fn vacant(depth: usize) -> QueryErrorFFI {
        QueryErrorFFI {
            depth: depth as INT32,
            found_entry: BorrowedStrBox::empty()
        }
    }

    fn conflict(depth: usize, kind: &'static str) -> QueryErrorFFI {
        QueryErrorFFI {
            depth: depth as INT32,
            found_entry: BorrowedStrBox::from_static(kind)
        }
    }
}

#[no_mangle]
pub extern "C" fn global_init() {
    std_panic::set_hook(Box::new(FFIPanicInfo::set));
}

#[no_mangle]
pub extern "C" fn load_from_utf16(data: *const u16, len: INT32) -> ParseResult {
    unwindable_call(|| {
        let raw_text = unsafe { slice::from_raw_parts(data, len as usize) };
        let utf8_text = String::from_utf16_lossy(raw_text);
        let doc_result = Document::parse(&utf8_text);
        match doc_result {
            Ok(doc) => ParseResult::success(doc),
            Err(parse_err) => ParseResult::error(parse_err)
        }
    })
}

#[no_mangle]
pub extern "C" fn free_manifest(manifest: *mut Manifest) {
    unwindable_call(|| {
        drop(unsafe { Box::from_raw(manifest) });
    })
}

#[no_mangle]
pub extern "C" fn free_strbox(s: OwnedStrBox) {
    drop(s)
}

#[no_mangle]
pub extern "C" fn get_string(manifest: *mut Manifest,
                             path: RawSlice<BorrowedStrBox>)
                             -> QueryResult<OwnedStrBox> {
    unwindable_call(move || {
        let mut vec = Vec::with_capacity(path.len as usize);
        for i in 0..(path.len as isize) {
            vec.push(unsafe { &*path.arr.offset(i) }.as_str());
        }
        let value = unsafe { &*manifest }.get_string(&vec);
        QueryResult::from_string_result(value)
    })
}

#[no_mangle]
pub extern "C" fn set_string(_: *mut Manifest,
                             _: RawSlice<BorrowedStrBox>)
                             -> QueryResult<BOOL> {
    unwindable_call(move || {
        unimplemented!()
    })
}