#[]
fn foo() {}

# [ path = "task_files" ]
mod task {
    // Load the `local_data` module from `task_files/tls.rs`
    # [ path = "tls.rs"]
    mod local_data;
}