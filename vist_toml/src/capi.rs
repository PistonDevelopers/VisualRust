use std::panic as std_panic;
use std::ptr;
use std::slice;

use toml_document::{Document, ParserError};
use winapi::INT32;

use super::*;
use panic::*;

type Boolean = u8;

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
            error: OwnedSlice::from_string(err.desc)
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
                result: OwnedSlice::from_string(string),
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
                result: OwnedSlice::from_slice(&vec, |s| OwnedSlice::from_string(*s)),
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
            path: OwnedSlice::from_string(&*e.path),
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

impl MultiQueryResult<OwnedSlice<OwnedOutputTarget>> {
    fn from_output_targets(r: Result<Vec<OutputTarget>, Vec<PathError>>)
                         -> MultiQueryResult<OwnedSlice<OwnedOutputTarget>> {
        match r {
            Ok(targets) => MultiQueryResult {
                result: OwnedSlice::from_slice(&targets, OwnedOutputTarget::from),
                errors: OwnedSlice::empty(),
            },
            Err(errors) => MultiQueryResult {
                result: OwnedSlice::empty(),
                errors: OwnedSlice::from_slice(&errors, PathErrorFFI::new),
            }
        }
    }
}

#[repr(u8)]
#[derive(Copy, Clone)]
pub enum Trilean {
    False = 0,
    True = 1,
    Unknown = 2,
}

impl Trilean {
    fn new(maybe: Option<bool>) -> Trilean {
        match maybe {
            Some(false) => Trilean::False,
            Some(true) => Trilean::True,
            None => Trilean::Unknown,
        }
    }

    fn to_bool(self) -> Option<bool> {
        match self {
            Trilean::False => Some(false),
            Trilean::True => Some(true),
            Trilean::Unknown => None,
        }
    }
}

#[repr(C)]
pub struct OwnedOutputTarget {
    handle: usize,
    kind: OwnedSlice<u8>,
    name: OwnedSlice<u8>,
    path: OwnedSlice<u8>,
    test: Trilean,
    doctest: Trilean,
    bench: Trilean,
    doc: Trilean,
    plugin: Trilean,
    harness: Trilean
}

impl OwnedOutputTarget {
    fn from(t: &OutputTarget) -> OwnedOutputTarget {
        OwnedOutputTarget {
            handle: t.handle,
            kind: OwnedSlice::from_string(t.kind),
            name: OwnedSlice::from_str_opt(t.name),
            path: OwnedSlice::from_str_opt(t.path),
            test: Trilean::new(t.test),
            doctest: Trilean::new(t.doctest),
            bench: Trilean::new(t.bench),
            doc: Trilean::new(t.doc),
            plugin: Trilean::new(t.plugin),
            harness: Trilean::new(t.harness),
        }
    }
}

#[repr(C)]
pub struct BorrowedOutputTarget<'a> {
    handle: usize,
    kind: BorrowedSlice<'a, u8>,
    name: BorrowedSlice<'a, u8>,
    path: BorrowedSlice<'a, u8>,
    test: Trilean,
    doctest: Trilean,
    bench: Trilean,
    doc: Trilean,
    plugin: Trilean,
    harness: Trilean
}

impl<'a> BorrowedOutputTarget<'a> {
    fn to_target(&'a self) -> OutputTarget<'a> {
        OutputTarget {
            handle: self.handle,
            kind: self.kind.as_str(),
            name: self.name.as_str_opt(),
            path: self.path.as_str_opt(),
            test: self.test.to_bool(),
            doctest: self.doctest.to_bool(),
            bench: self.bench.to_bool(),
            doc: self.doc.to_bool(),
            plugin: self.plugin.to_bool(),
            harness: self.harness.to_bool(),
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
pub extern "C" fn free_output_targets_result(r: MultiQueryResult<OwnedSlice<OwnedOutputTarget>>) {
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
                                     -> MultiQueryResult<OwnedSlice<OwnedOutputTarget>> {
    unwindable_call(move || {
        let targets = unsafe { &*manifest }.get_output_targets();
        MultiQueryResult::from_output_targets(targets)
    })
}

#[no_mangle]
#[allow(no_mangle_generic_items)]
pub extern "C" fn add_output_target<'a>(manifest: *mut Manifest,
                                        raw_target: BorrowedOutputTarget<'a>)
                                        -> usize {
    unwindable_call(move || {
        let manifest = unsafe { &mut*manifest };
        let target = raw_target.to_target();
        manifest.add_output_target(target)
    })
}

#[no_mangle]
#[allow(no_mangle_generic_items)]
pub extern "C" fn set_output_target<'a>(manifest: *mut Manifest,
                                        raw_target: BorrowedOutputTarget<'a>)
                                        -> usize {
    unwindable_call(move || {
        let manifest = unsafe { &mut*manifest };
        let target = raw_target.to_target();
        manifest.set_output_target(target).unwrap_or(0)
    })
}

#[no_mangle]
#[allow(no_mangle_generic_items)]
pub extern "C" fn remove_output_target<'a>(manifest: *mut Manifest,
                                           handle: usize,
                                           kind: BorrowedSlice<'a, u8>) {
    unwindable_call(move || {
        let manifest = unsafe { &mut*manifest };
        manifest.remove_output_target(handle, kind.as_str())
    })
}

#[no_mangle]
pub extern "C" fn manifest_to_string(manifest: *mut Manifest) -> OwnedSlice<u8> {
    unwindable_call(move || {
        let manifest = unsafe { &*manifest };
        OwnedSlice::from_string(manifest.to_string())
    })
}