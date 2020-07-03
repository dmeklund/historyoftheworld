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

        internal override async Task Process()
        {
            // TransformManyBlock does not inherently support async enumerables
            // so we have to translate to a list by hand.
            // https://github.com/dotnet/runtime/issues/30863
            // var extractBlock = new TransformManyBlock<T1, WrappedOutput<T2>>(
            //     input =>
            //     {
            //         try
            //         {
            //             return new AsyncEnumerable<T2>(input.Id, Extractor.Extract(input));
            //         }
            //         catch (Exception e)
            //         {
            //             Console.WriteLine(e);
            //             throw;
            //         }
            //         // await foreach (var result in Extractor.Extract(input))
            //         // {
            //         //     resultList.Add(new WrappedOutput(input.Id, result));
            //         // }
            //         // return resultList;
            //     },
            //     new ExecutionDataflowBlockOptions()
            //     {
            //         MaxDegreeOfParallelism = 32
            //     }
            // );
            // var sinkBlock = new ActionBlock<WrappedOutput<T2>>(
            //     async output =>
            //     {
            //         try
            //         {
            //             await Sink.Save(output.Id, output.Value);
            //         }
            //         catch (Exception e)
            //         {
            //             Console.WriteLine(e);
            //             throw;
            //         }
            //     },
            //     new ExecutionDataflowBlockOptions
            //     {
            //         MaxDegreeOfParallelism = 32
            //     }
            // );
            // var linkOptions = new DataflowLinkOptions
            // {
            //     PropagateCompletion = true
            // };
            // extractBlock.LinkTo(sinkBlock, linkOptions);
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
                    MaxDegreeOfParallelism = 32,
                    BoundedCapacity = 32
                }
            );
            await foreach (var input in Source.FetchAll())
            {
                // this should not be necessary with the dataflow block model
                // while (GC.GetTotalMemory(false) > 1.5e9)
                // {
                //     await Task.Delay(1000);
                // }
                await actionBlock.SendAsync(input);
            }
            actionBlock.Complete();
            // while (!extractBlock.Completion.IsCompleted)
            // {
            //     await Task.Delay(1000);
            // }
            // Task.WaitAll(actionBlock.Completion, sinkBlock.Completion);
            await actionBlock.Completion;
        }
    }
}