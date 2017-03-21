using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace TaskRunner.Test
{
    public class TaskRunnerTest
    {
        private List<string> _results = new List<string>();
        private TaskRunner _queue;

        public TaskRunnerTest()
        {
            _queue = new TaskRunner();
        }

        [Fact]
        public void When_there_are_no_actions_Then_the_count_is_zero()
        {
            Assert.Equal(0, _queue.Count);
        }

        [Fact]
        public void When_actions_are_added_to_the_queue_Then_the_count_increases()
        {
            _queue.Enqueue(() => AddResult("test1"));
            Assert.Equal(1, _queue.Count);
            Assert.Equal(1, _queue.Count);
        }

        [Fact]
        public async Task When_one_action_in_the_queue_is_executed_Then_the_count_changes_to_zero()
        {
            _queue.Enqueue(() => AddResult("test1"));
            await _queue.ExecuteAll();

            Assert.Equal(0, _queue.Count);
        }

        [Fact]
        public async Task When_one_action_in_the_queue_is_executed_Then_the_action_is_completed()
        {
            _queue.Enqueue(() => AddResult("test1"));
            await _queue.ExecuteAll();

            Assert.Equal(1, _results.Count);
            Assert.Equal("test1", _results[0]);
        }

        [Fact]
        public async Task When_slow_then_fast_actions_in_the_queue_are_executed_Then_the_actions_complete_in_order()
        {
            _queue.Enqueue(async () =>
            {
                await Task.Delay(200);
                _results.Add("test1");
            });
            _queue.Enqueue(() => AddResult("test2"));
            await _queue.ExecuteAll();

            Assert.Equal(2, _results.Count);
            Assert.Equal("test1", _results[0]);
            Assert.Equal("test2", _results[1]);
        }

        [Fact]
        public async Task When_a_slow_action_in_the_queue_is_executed_Then_other_actions_can_happen_synchronously()
        {
            _queue.Enqueue(async () =>
            {
                _results.Add("task_start");
                await Task.Delay(200);
                _results.Add("task_end");
            });

            _results.Add("event_before_task_starts");
            Task execute = _queue.ExecuteAll();
            await Task.Delay(100);
            _results.Add("event_during_task_execution");
            await Task.Delay(200);
            _results.Add("event_after_task_completed");
            execute.Wait();

            string[] expected = {
                "event_before_task_starts",
                "task_start",
                "event_during_task_execution",
                "task_end",
                "event_after_task_completed"
            };
            Assert.Equal(string.Join(",", expected), string.Join(",", _results));
        }

        [Fact]
        public async Task When_the_queue_is_executing_continuously_and_actions_are_added_Then_the_actions_appear_in_the_correct_sequence()
        {
            Task executeContinuously = _queue.ExecuteContinuously();
            _results.Add("event_after_queue_starts");
            _queue.Enqueue(() => AddResult("task1"));
            _queue.Enqueue(async () =>
            {
                _results.Add("task2_start");
                await Task.Delay(200);
                _results.Add("task2_end");
            });
            _queue.Enqueue(() => AddResult("task3"));
            await Task.Delay(100);
            _results.Add("event_while_task2_will_be_executing");
            await Task.Delay(200);
            _results.Add("event_after_task3_will_be_finished");

            _queue.StopWhenQueueIsEmpty();
            executeContinuously.Wait();

            string[] expected = {
                "event_after_queue_starts",
                "task1",
                "task2_start",
                "event_while_task2_will_be_executing",
                "task2_end",
                "task3",
                "event_after_task3_will_be_finished"
            };
            Assert.Equal(string.Join(",", expected), string.Join(",", _results));
        }

        public async Task When_the_queue_is_executing_continuously_and_actions_are_running_when_top_is_called_Then_the_actions_will_complete()
        {
            Task executeContinuously = _queue.ExecuteContinuously();
            _queue.Enqueue(async () =>
            {
                _results.Add("task_start");
                await Task.Delay(200);
                _results.Add("task_end");
            });
            await Task.Delay(100);
            _results.Add("event_while_task_is_executing");
            _queue.StopWhenQueueIsEmpty();
            _results.Add("event_after_stop_called");
            executeContinuously.Wait();

            string[] expected = {
                "task_start",
                "event_while_task_is_executing",
                "event_after_stop_called",
                "task_end"
            };
            Assert.Equal(string.Join(",", expected), string.Join(",", _results));
        }

        private async Task AddResult(string text)
        {
            _results.Add(text);
            await Task.Yield();
        }
    }
}
