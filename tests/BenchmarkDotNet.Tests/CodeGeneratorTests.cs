using System;
using System.Linq;
using System.Reflection;
using Xunit;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Code;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Parameters;
using System.IO;

namespace BenchmarkDotNet.Tests
{
    public class CodeGeneratorTests
    {
        [Fact]
        public void AsyncVoidIsNotSupported()
        {
            var asyncVoidMethod =
                typeof(CodeGeneratorTests)
                    .GetTypeInfo()
                    .GetMethods()
                    .Single(method => method.Name == nameof(AsyncVoidMethod));

            var target = new Descriptor(typeof(CodeGeneratorTests), asyncVoidMethod);
            var benchmark = BenchmarkCase.Create(target, Job.Default, null, ManualConfig.CreateEmpty().CreateImmutableConfig());

            Assert.Throws<NotSupportedException>(() => CodeGenerator.Generate(new BuildPartition(new[] { new BenchmarkBuildInfo(benchmark, ManualConfig.CreateEmpty().CreateImmutableConfig(), 0) }, BenchmarkRunnerClean.DefaultResolver)));
        }

        [Fact]
        public void UsingStatementsInTheAutoGeneratedCodeAreProhibited()
        {
            var fineMethod =
                typeof(CodeGeneratorTests)
                    .GetTypeInfo()
                    .GetMethods()
                    .Single(method => method.Name == nameof(FineMethod));

            var target = new Descriptor(typeof(CodeGeneratorTests), fineMethod);
            var benchmark = BenchmarkCase.Create(target, Job.Default, new ParameterInstances(Array.Empty<ParameterInstance>()), ManualConfig.CreateEmpty().CreateImmutableConfig());

            var generatedSourceFile = CodeGenerator.Generate(new BuildPartition(new[] { new BenchmarkBuildInfo(benchmark, ManualConfig.CreateEmpty().CreateImmutableConfig(), 0) }, BenchmarkRunnerClean.DefaultResolver));

            using (StringReader stringReader = new StringReader(generatedSourceFile))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null && !line.StartsWith("namespace"))
                {
                    Assert.False(line.Trim().StartsWith("using"), line);
                }
            }
        }

#pragma warning disable CS1998
#pragma warning disable xUnit1013 // Public method should be marked as test
        [Benchmark]
        public async void AsyncVoidMethod() { }

        [Benchmark]
        public void FineMethod() { }
#pragma warning restore xUnit1013 // Public method should be marked as test
    }
}