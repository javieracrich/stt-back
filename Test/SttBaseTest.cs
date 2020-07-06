using AutoMapper;
using Services;
using Xunit;

// https://dev.to/cesarcodes/quick-fix-for-mapper-already-initialized-error-in-automapper-xunit-tests-6k8
[assembly: CollectionBehavior(MaxParallelThreads = 1)]
namespace Test
{
    public class SttBaseTest
    {
        protected SttBaseTest()
        {
            Mapper.Reset();
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new AutomapperProfile());
                cfg.AllowNullCollections = true;
            });
        }
    }
}