TransactionScope perf test results:

| test name                                                | elapsed          |
|----------------------------------------------------------|------------------|
| commit_transaction_in_multiple_same_contexts_perf        | 00:00:06.0662057 |
| commit_transaction_in_single_context_perf                | 00:00:04.4063021 |
| rollback_transaction_in_multiple_different_contexts_perf | 00:00:07.4964980 |
| rollback_transaction_in_multiple_same_contexts_perf      | 00:00:05.1293657 |
| rollback_transaction_in_single_context_perf              | 00:00:03.9618454 |