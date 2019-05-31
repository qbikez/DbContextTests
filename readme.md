TransactionScope perf test results:

| test name                                                | loop count | elapsed          |
|----------------------------------------------------------|------------|------------------|
| commit_transaction_in_multiple_same_contexts_perf        | 1000       | 00:00:06.1243420 |
| commit_transaction_in_single_context_perf                | 1000       | 00:00:04.4680017 |
| rollback_transaction_in_multiple_different_contexts_perf | 1000       | 00:00:08.3109921 |
| rollback_transaction_in_multiple_same_contexts_perf      | 1000       | 00:00:05.2899681 |
| rollback_transaction_in_single_context_perf              | 1000       | 00:00:04.3460997 |