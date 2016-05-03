use std::panic as std_panic;
use std::ptr;
use std::slice;

use toml_document::{Document, ParserError};
use winapi::{BOOL, INT32};

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
}

impl QueryResult<OwnedSlice<OwnedSlice<u8>>> {
    fn from_string_array_result(r: Result<Vec<&str>, QueryError>)
                                -> QueryResult<OwnedSlice<OwnedSlice<u8>>> {
        match r {
            Ok(vec) => QueryResult {
                result: OwnedSlice::from_slice(&vec, |s| OwnedSlice::from_str(s)),
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

#[repr(C)]
pub struct PathErrorFFI {
    path: OwnedSlice<u8>,
    expected: BorrowedSlice<'static, u8>,
    got: BorrowedSlice<'static, u8>
}

impl PathErrorFFI {
    fn new(e: &PathError) -> PathErrorFFI {
        PathErrorFFI {
            path: OwnedSlice::from_str(&e.path),
            expected: BorrowedSlice::from_static(e.expected),
            got: BorrowedSlice::from_static(e.got)
        }
    }
}

#[repr(C)]
pub struct MultiQueryResult<T> {
    result: T,
    errors: OwnedSlice<PathErrorFFI>
}

impl MultiQueryResult<OwnedSlice<RawDependency>> {
    fn from_dependencies(r: Result<Vec<Dependency>, Vec<PathError>>)
                         -> MultiQueryResult<OwnedSlice<RawDependency>> {
        match r {
            Ok(deps) => MultiQueryResult {
                result: OwnedSlice::from_slice(&deps, RawDependency::from),
                errors: OwnedSlice::empty(),
            },
            Err(errors) => MultiQueryResult {
                result: OwnedSlice::empty(),
                errors: OwnedSlice::from_slice(&errors, PathErrorFFI::new),
            }
        }
    }
}

impl MultiQueryResult<OwnedSlice<OutputTargetFFI>> {
    fn from_output_targets(r: Result<Vec<OutputTarget>, Vec<PathError>>)
                         -> MultiQueryResult<OwnedSlice<OutputTargetFFI>> {
        match r {
            Ok(targets) => MultiQueryResult {
                result: OwnedSlice::from_slice(&targets, OutputTargetFFI::from),
                errors: OwnedSlice::empty(),
            },
            Err(errors) => MultiQueryResult {
                result: OwnedSlice::empty(),
                errors: OwnedSlice::from_slice(&errors, PathErrorFFI::new),
            }
        }
    }
}

#[repr(C)]
pub struct OutputTargetFFI {
    name: OwnedSlice<u8>,
    path: OwnedSlice<u8>,
    test: BOOL,
    doctest: BOOL,
    bench: BOOL,
    doc: BOOL,
    plugin: BOOL,
    harness: BOOL
}

impl OutputTargetFFI {
    fn from(t: &OutputTarget) -> OutputTargetFFI {
        OutputTargetFFI {
            name: OwnedSlice::from_str_opt(t.name),
            path: OwnedSlice::from_str_opt(t.path),
            test: t.test as BOOL,
            doctest: t.doctest as BOOL,
            bench: t.bench as BOOL,
            doc: t.doc as BOOL,
            plugin: t.plugin as BOOL,
            harness: t.harness as BOOL,
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
pub extern "C" fn free_dependencies_result(r: MultiQueryResult<OwnedSlice<RawDependency>>) {
    drop(r)
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

#[no_mangle]
pub extern "C" fn get_dependencies(manifest: *mut Manifest)
                                   -> MultiQueryResult<OwnedSlice<RawDependency>> {
    unwindable_call(move || {
        let dependencies = unsafe { &*manifest }.get_dependencies();
        MultiQueryResult::from_dependencies(dependencies)
    })
}

#[no_mangle]
pub extern "C" fn get_output_targets(manifest: *mut Manifest)
                                     -> MultiQueryResult<OwnedSlice<OutputTargetFFI>> {
    unwindable_call(move || {
        let targets = unsafe { &*manifest }.get_output_targets();
        MultiQueryResult::from_output_targets(targets)
    })
}