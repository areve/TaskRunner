using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TaskRunner
{
    public class TaskRunner : Queue<Func<Task>>
    {
        private bool _isRunningContinuously;

        public async Task ExecuteAll()
        {
            while (Count != 0)
            {
                var func = Dequeue();
                await func();
            }

            await Task.Yield();
        }

        public async Task ExecuteContinuously()
        {
            _isRunningContinuously = true;
            while (_isRunningContinuously)
            {
                await ExecuteAll();
            }

            await Task.Yield();
        }

        public void StopWhenQueueIsEmpty()
        {
            _isRunningContinuously = false;
        }
    }
}
