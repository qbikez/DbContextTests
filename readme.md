TransactionScope perf test results:

| Average of ElapsedMs |                    | ContextLifetime |                |
|----------------------|--------------------|-----------------|----------------|
| TransactionType      | TransactionOutcome | PerQuery        | PerTransaction |
| DatabaseTransaction  | COMMIT             |                 | 5 424          |
|                      | ROLLBACK           |                 | 3 447          |
| None                 | COMMIT             |                 | 3 652          |
| TransactionScope     | COMMIT             | 7 105           | 3 988          |
|                      | ROLLBACK           | 5 111           | 3 564          |

UsingContext perf test results:

| Average of ElapsedMs                     | ContextLifetime |                |
|------------------------------------------|-----------------|----------------|
|                                          | PerQuery        | PerTransaction |
| get_and_update_multiple_context_reattach | 2 751           |                |
| get_and_update_single_context            |                 | 2 392          |
| get_and_update_multiple_context_retrieve | 3 351           |                |
| get_and_add_to_dbset_multiple_contexts   | 3 087           |                |
| get_and_add_to_dbset_single_context      |                 | 2 858          |