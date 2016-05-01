use std::cell::RefCell;
use std::panic;
use std::panic::{PanicInfo, UnwindSafe};

use winapi::{DWORD, UINT32};
use kernel32::RaiseException;

use super::*;

thread_local!(static PANIC_INFO: RefCell<Option<FFIPanicInfo>> = RefCell::new(None));
const RUST_PANIC_CODE: DWORD = 0x00524323; // std uses RST, we use RC#

pub fn unwindable_call<F:FnOnce() -> T + UnwindSafe, T>(f: F) -> T {
    match panic::catch_unwind(f) {
        Err(..) => {
            let info_ptr: *const usize = PANIC_INFO.with(|f| unsafe { ::std::mem::transmute::<_, *const usize>(f.borrow().as_ref()) });
            let ptr_arr = [info_ptr];
            unsafe { RaiseException(RUST_PANIC_CODE, 0, 1, ptr_arr.as_ptr() as *const _) };
            unreachable!()
        }
        Ok(x) => x
    }
}

#[repr(C)]
pub struct FFIPanicInfo {
    msg: OwnedSlice<u8>,
    file: OwnedSlice<u8>,
    line: UINT32
}

impl FFIPanicInfo {
   	pub fn set(panic: &PanicInfo) {
        PANIC_INFO.with(|pi| {
            let mut result = FFIPanicInfo {
                msg: OwnedSlice::empty(),
                file: OwnedSlice::empty(),
                line: 0
            };
            if let Some(text) = panic.payload().downcast_ref::<&'static str>() {
                result.msg = OwnedSlice::from_str(text);
            };
            if let Some(loc) = panic.location() {
                result.file = OwnedSlice::from_str(loc.file());
                result.line = loc.line();
            };
            *pi.borrow_mut() = Some(result);
        });
    }
}