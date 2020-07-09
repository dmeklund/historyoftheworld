using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ParseWiki.Extractors;
using ParseWiki.Sinks;
using ParseWiki.Sources;

namespace ParseWiki.Processors
{
    internal class WrappedOutput<T>
    {
        internal int Id { get; }
        internal T Value { get; }

        internal WrappedOutput(int id, T value)
        {
            Id = id;
            Value = value;
        }
            
    }
    public class DataflowProcessor<T1,T2> : Processor<T1,T2> where T1 : IWithId
    {
        private bool _cancelled = false;
        public DataflowProcessor(ISource<T1> source, IExtractor<T1, T2> extractor, ISink<T2> sink) : base(source, extractor, sink)
        {
        }

        

        private class AsyncEnumerable<T> : IEnumerable<WrappedOutput<T>>
        {
            private IAsyncEnumerable<T> _enumer;
            private int _id;
            internal AsyncEnumerable(int id, IAsyncEnumerable<T> enumer)
            {
                _enumer = enumer;
                _id = id;
            }
            public IEnumerator<WrappedOutput<T>> GetEnumerator()
            {
                return new AsyncEnumerator<T>(_id, _enumer.GetAsyncEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new AsyncEnumerator<T>(_id, _enumer.GetAsyncEnumerator());
            }
        }

        private class AsyncEnumerator<T> : IEnumerator<WrappedOutput<T>>
        {
            private IAsyncEnumerator<T> _enumer;
            private int _id;
            internal AsyncEnumerator(int id, IAsyncEnumerator<T> enumer)
            {
                _enumer = enumer;
                _id = id;
            }
            public bool MoveNext()
            {
                var result = _enumer.MoveNextAsync();
                result.AsTask().Wait();
                return result.Result;
            }

            public void Reset()
            {
            }

            public WrappedOutput<T> Current
            {
                get
                {
                    return new WrappedOutput<T>(_id, _enumer.Current);
                }
            }

            object? IEnumerator.Current => Current;

            public void Dispose()
            {
                _enumer.DisposeAsync().AsTask().Wait();
            }
        }

        internal override void Cancel()
        {
            Console.WriteLine("Waiting for running tasks to complete...");
            _cancelled = true;
        }

        internal override async Task Process()
        {
            var actionBlock = new ActionBlock<T1>(
                async input =>
                {
                    try
                    {
                        await foreach (var output in Extractor.Extract(input))
                        {
                            await Sink.Save(input.Id, output);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 16,
                    BoundedCapacity = 16
                }
            );
            int lastId = -1;
            await foreach (var input in Source.FetchAll())
            {
                // this should not be necessary with the dataflow block model
                // while (GC.GetTotalMemory(false) > 1.5e9)
                // {
                //     await Task.Delay(1000);
                // }
                Console.WriteLine($"Processing {input}");
                await actionBlock.SendAsync(input);
                lastId = input.Id;
                if (_cancelled)
                {
                    break;
                }
            }
            actionBlock.Complete();
            Console.WriteLine($"Dataflow submission finished: awaiting {actionBlock.InputCount} items in the queue");
            await actionBlock.Completion;
            Console.WriteLine($"Process completed (canceled? {_cancelled}). Last id: {lastId}");
        }
    }
}
