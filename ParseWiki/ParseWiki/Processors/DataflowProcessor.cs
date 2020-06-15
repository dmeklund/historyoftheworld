using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ParseWiki.Extractors;
using ParseWiki.Sinks;
using ParseWiki.Sources;

namespace ParseWiki.Processors
{
    public class DataflowProcessor<T1,T2> : Processor<T1,T2> where T1 : IWithId
    {
        public DataflowProcessor(ISource<T1> source, IExtractor<T1, T2> extractor, ISink<T2> sink) : base(source, extractor, sink)
        {
        }

        private class WrappedOutput
        {
            internal int Id { get; }
            internal T2 Value { get; }

            internal WrappedOutput(int id, T2 value)
            {
                Id = id;
                Value = value;
            }
            
        }

        internal override async Task Process()
        {
            var extractBlock = new TransformBlock<T1, WrappedOutput>(
                async input => new WrappedOutput(input.Id, await Extractor.Extract(input))
            );
            var sinkBlock = new ActionBlock<WrappedOutput>(
                async output => await Sink.Save(output.Id, output.Value),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 32
                }
            );
            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };
            extractBlock.LinkTo(sinkBlock, linkOptions);
            await foreach (var input in Source.FetchAll())
            {
                // this should not be necessary with the dataflow block model
                // while (GC.GetTotalMemory(false) > 1.5e9)
                // {
                //     await Task.Delay(1000);
                // }
                await extractBlock.SendAsync(input);
            }
            extractBlock.Complete();
            await extractBlock.Completion;
        }
    }
}