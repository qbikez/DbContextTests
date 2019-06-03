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



Context lifetime test observations:

* `TransactionScope` works between `using(DbContext)`.
* `Users.Orders.Add(new Order())` will load all Orders of a user (if it's LazyLoading). Cannot do it without loading the collection (either lazily or eagerly). It probably makes more sense to call `db.Orders.Add(new Order())`.
* If we want to update an entity retrieved from a context that was disposed, we need to reattach it to the new context:

        db.Users.Attach(user);
        db.Entry(user).State = System.Data.Entity.EntityState.Modified;

* "Offline" modification (modification of an Entity retrieved from a context that was disposed) cause problems with Relations:
  * will throw exception when checking `User.UserPreferences == null` if `UserPreferences` is LazyLoaded - can't lazy load from a disposed context
  * will not update `User.UserPreferences` in DB unless it's reattached (event if `User` is reattached)
