# TaskRunner
A .NET Core project that demostrates a non-blocking asynchronous task queue.

  * Actions are executed sequentially, one at a time.
  * Actions are executed in the order that they were added to the class.
  * Actions need not necessarily be added all at the same time.
  * Actions may execute on another thread.
