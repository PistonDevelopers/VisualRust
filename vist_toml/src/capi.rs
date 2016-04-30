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
    pub error: OwnedSlice<u8>
}

impl ParseResult {
    fn success(doc: Document) -> ParseResult {
        ParseResult {
            manifest: Box::into_raw(Box::new(Manifest::new(doc))),
            error: OwnedSlice::<u8>::empty()
        }
    }

    fn error(err: ParserError) -> ParseResult {
        ParseResult {
            manifest: ptr::null_mut(),
            error: OwnedSlice::from_str(&err.desc)
        }
    }
}

#[repr(C)]
pub struct QueryResult<T> {
    result: T,
    error: QueryErrorFFI
}

impl QueryResult<OwnedSlice<u8>> {
    fn from_string_result(r: Result<&str, QueryError>) -> QueryResult<OwnedSlice<u8>> {
        match r {
            Ok(string) => QueryResult {
                result: OwnedSlice::from_str(string),
                error: QueryErrorFFI::empty()
            },
            Err(QueryError::Vacant{ depth }) => QueryResult {
                result: OwnedSlice::empty(),
                error: QueryErrorFFI::vacant(depth)
            },
            Err(QueryError::Conflict{ depth, kind }) => QueryResult {
                result: OwnedSlice::empty(),
                error: QueryErrorFFI::conflict(depth, kind)
            },
        }
    }

    fn from_string_array_result(r: Result<Vec<&str>, QueryError>)
                                -> QueryResult<OwnedSlice<OwnedSlice<u8>>> {
        match r {
            Ok(vec) => QueryResult {
                result: OwnedSlice::from_str_slice(&vec),
                error: QueryErrorFFI::empty()
            },
            Err(QueryError::Vacant{ depth }) => QueryResult {
                result: OwnedSlice::empty(),
                error: QueryErrorFFI::vacant(depth)
            },
            Err(QueryError::Conflict{ depth, kind }) => QueryResult {
                result: OwnedSlice::empty(),
                error: QueryErrorFFI::conflict(depth, kind)
            },
        }
    }
}

#[repr(C)]
pub struct QueryErrorFFI {
    depth: INT32,
    found_entry: BorrowedSlice<'static, u8>
}

impl QueryErrorFFI {
    fn empty() -> QueryErrorFFI {
        QueryErrorFFI {
            depth: 0,
            found_entry: BorrowedSlice::empty()
        }
    }

    fn vacant(depth: usize) -> QueryErrorFFI {
        QueryErrorFFI {
            depth: depth as INT32,
            found_entry: BorrowedSlice::empty()
        }
    }

    fn conflict(depth: usize, kind: &'static str) -> QueryErrorFFI {
        QueryErrorFFI {
            depth: depth as INT32,
            found_entry: BorrowedSlice::from_static(kind)
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
pub extern "C" fn free_strbox(s: OwnedSlice<u8>) {
    drop(s)
}

#[no_mangle]
pub extern "C" fn free_strbox_array(s: OwnedSlice<OwnedSlice<u8>>) {
    drop(s)
}

#[no_mangle]
#[allow(no_mangle_generic_items)]
pub extern "C" fn get_string<'a>(manifest: *mut Manifest,
                                 path: BorrowedSlice<'a, BorrowedSlice<'a, u8>>)
                                 -> QueryResult<OwnedSlice<u8>> {
    unwindable_call(move || {
        let vec = path.as_str_vec();
        let value = unsafe { &*manifest }.get_string(&vec);
        QueryResult::from_string_result(value)
    })
}

#[no_mangle]
pub extern "C" fn set_string(_: *mut Manifest,
                             _: RawSlice<RawSlice<u8>>)
                             -> QueryResult<BOOL> {
    unwindable_call(move || {
        unimplemented!()
    })
}

#[no_mangle]
#[allow(no_mangle_generic_items)]
pub extern "C" fn get_string_array<'a>(manifest: *mut Manifest,
                                       path: BorrowedSlice<'a, BorrowedSlice<'a, u8>>)
                                       -> QueryResult<OwnedSlice<OwnedSlice<u8>>> {
    unwindable_call(move || {
        let vec = path.as_str_vec();
        let value = unsafe { &*manifest }.get_string_array(&vec);
        QueryResult::from_string_array_result(value)
    })
}