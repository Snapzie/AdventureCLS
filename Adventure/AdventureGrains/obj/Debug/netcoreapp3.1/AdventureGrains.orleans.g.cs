// <auto-generated />
#if !EXCLUDE_CODEGEN
#pragma warning disable 162
#pragma warning disable 219
#pragma warning disable 414
#pragma warning disable 618
#pragma warning disable 649
#pragma warning disable 693
#pragma warning disable 1591
#pragma warning disable 1998
[assembly: global::Orleans.Metadata.FeaturePopulatorAttribute(typeof(OrleansGeneratedCode.OrleansCodeGen120a0c9883FeaturePopulator))]
[assembly: global::Orleans.CodeGeneration.OrleansCodeGenerationTargetAttribute(@"AdventureGrains, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")]
namespace OrleansGeneratedCodeCB84832B
{
    using global::Orleans;
    using global::System.Reflection;
}

namespace OrleansGeneratedCode
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute(@"Orleans-CodeGenerator", @"2.0.0.0")]
    internal sealed class OrleansCodeGen120a0c9883FeaturePopulator : global::Orleans.Metadata.IFeaturePopulator<global::Orleans.Metadata.GrainInterfaceFeature>, global::Orleans.Metadata.IFeaturePopulator<global::Orleans.Metadata.GrainClassFeature>, global::Orleans.Metadata.IFeaturePopulator<global::Orleans.Serialization.SerializerFeature>
    {
        public void Populate(global::Orleans.Metadata.GrainInterfaceFeature feature)
        {
        }

        public void Populate(global::Orleans.Metadata.GrainClassFeature feature)
        {
            feature.Classes.Add(new global::Orleans.Metadata.GrainClassMetadata(typeof(global::AdventureGrains.BossGrain)));
            feature.Classes.Add(new global::Orleans.Metadata.GrainClassMetadata(typeof(global::AdventureGrains.MonsterGrain)));
            feature.Classes.Add(new global::Orleans.Metadata.GrainClassMetadata(typeof(global::AdventureGrains.PlayerGrain)));
            feature.Classes.Add(new global::Orleans.Metadata.GrainClassMetadata(typeof(global::AdventureGrains.RoomGrain)));
        }

        public void Populate(global::Orleans.Serialization.SerializerFeature feature)
        {
            feature.AddKnownType(@"Interop,System.Runtime.Extensions", @"Interop");
            feature.AddKnownType(@"Interop+Sys,System.Runtime.Extensions", @"Sys");
            feature.AddKnownType(@"FxResources.System.Runtime.Extensions.SR,System.Runtime.Extensions", @"FxResources.System.Runtime.Extensions.SR");
            feature.AddKnownType(@"System.AppDomainUnloadedException,System.Runtime.Extensions", @"AppDomainUnloadedException");
            feature.AddKnownType(@"System.ApplicationId,System.Runtime.Extensions", @"ApplicationId");
            feature.AddKnownType(@"System.ContextBoundObject,System.Runtime.Extensions", @"ContextBoundObject");
            feature.AddKnownType(@"System.ContextMarshalException,System.Runtime.Extensions", @"ContextMarshalException");
            feature.AddKnownType(@"System.ContextStaticAttribute,System.Runtime.Extensions", @"ContextStaticAttribute");
            feature.AddKnownType(@"System.LoaderOptimization,System.Runtime.Extensions", @"LoaderOptimization");
            feature.AddKnownType(@"System.LoaderOptimizationAttribute,System.Runtime.Extensions", @"LoaderOptimizationAttribute");
            feature.AddKnownType(@"System.StringNormalizationExtensions,System.Runtime.Extensions", @"StringNormalizationExtensions");
            feature.AddKnownType(@"System.SR,System.Runtime.Extensions", @"SR");
            feature.AddKnownType(@"System.Text.ValueStringBuilder,System.Runtime.Extensions", @"ValueStringBuilder");
            feature.AddKnownType(@"System.Threading.Tasks.TaskToApm,System.Runtime.Extensions", @"TaskToApm");
            feature.AddKnownType(@"System.Security.Permissions.CodeAccessSecurityAttribute,System.Runtime.Extensions", @"CodeAccessSecurityAttribute");
            feature.AddKnownType(@"System.Security.Permissions.SecurityAttribute,System.Runtime.Extensions", @"SecurityAttribute");
            feature.AddKnownType(@"System.Security.Permissions.SecurityAction,System.Runtime.Extensions", @"SecurityAction");
            feature.AddKnownType(@"System.Security.Permissions.SecurityPermissionAttribute,System.Runtime.Extensions", @"SecurityPermissionAttribute");
            feature.AddKnownType(@"System.Security.Permissions.SecurityPermissionFlag,System.Runtime.Extensions", @"SecurityPermissionFlag");
            feature.AddKnownType(@"System.Runtime.ProfileOptimization,System.Runtime.Extensions", @"ProfileOptimization");
            feature.AddKnownType(@"System.Runtime.Versioning.FrameworkName,System.Runtime.Extensions", @"FrameworkName");
            feature.AddKnownType(@"System.Runtime.Versioning.ComponentGuaranteesAttribute,System.Runtime.Extensions", @"ComponentGuaranteesAttribute");
            feature.AddKnownType(@"System.Runtime.Versioning.ResourceConsumptionAttribute,System.Runtime.Extensions", @"ResourceConsumptionAttribute");
            feature.AddKnownType(@"System.Runtime.Versioning.ComponentGuaranteesOptions,System.Runtime.Extensions", @"ComponentGuaranteesOptions");
            feature.AddKnownType(@"System.Runtime.Versioning.ResourceExposureAttribute,System.Runtime.Extensions", @"ResourceExposureAttribute");
            feature.AddKnownType(@"System.Runtime.Versioning.ResourceScope,System.Runtime.Extensions", @"ResourceScope");
            feature.AddKnownType(@"System.Runtime.Versioning.SxSRequirements,System.Runtime.Extensions", @"SxSRequirements");
            feature.AddKnownType(@"System.Runtime.Versioning.VersioningHelper,System.Runtime.Extensions", @"VersioningHelper");
            feature.AddKnownType(@"System.Runtime.CompilerServices.SwitchExpressionException,System.Runtime.Extensions", @"SwitchExpressionException");
            feature.AddKnownType(@"System.Reflection.AssemblyNameProxy,System.Runtime.Extensions", @"AssemblyNameProxy");
            feature.AddKnownType(@"System.Net.WebUtility,System.Runtime.Extensions", @"WebUtility");
            feature.AddKnownType(@"System.IO.StringReader,System.Runtime.Extensions", @"StringReader");
            feature.AddKnownType(@"System.IO.StringWriter,System.Runtime.Extensions", @"StringWriter");
            feature.AddKnownType(@"System.IO.BufferedStream,System.Runtime.Extensions", @"BufferedStream");
            feature.AddKnownType(@"System.IO.InvalidDataException,System.Runtime.Extensions", @"InvalidDataException");
            feature.AddKnownType(@"System.IO.StreamHelpers,System.Runtime.Extensions", @"StreamHelpers");
            feature.AddKnownType(@"System.Diagnostics.Stopwatch,System.Runtime.Extensions", @"Stopwatch");
            feature.AddKnownType(@"System.CodeDom.Compiler.IndentedTextWriter,System.Runtime.Extensions", @"IndentedTextWriter");
            feature.AddKnownType(@"FxResources.System.Linq.SR,System.Linq", @"FxResources.System.Linq.SR");
            feature.AddKnownType(@"System.SR,System.Linq", @"SR");
            feature.AddKnownType(@"System.Collections.Generic.LargeArrayBuilder`1,System.Linq", @"LargeArrayBuilder`1'1");
            feature.AddKnownType(@"System.Collections.Generic.ArrayBuilder`1,System.Linq", @"ArrayBuilder`1'1");
            feature.AddKnownType(@"System.Collections.Generic.EnumerableHelpers,System.Linq", @"EnumerableHelpers");
            feature.AddKnownType(@"System.Collections.Generic.CopyPosition,System.Linq", @"CopyPosition");
            feature.AddKnownType(@"System.Collections.Generic.Marker,System.Linq", @"Marker");
            feature.AddKnownType(@"System.Collections.Generic.SparseArrayBuilder`1,System.Linq", @"SparseArrayBuilder`1'1");
            feature.AddKnownType(@"System.Linq.Enumerable,System.Linq", @"Enumerable");
            feature.AddKnownType(@"System.Linq.Enumerable+Iterator`1,System.Linq", @"Iterator`1'1");
            feature.AddKnownType(@"System.Linq.IIListProvider`1,System.Linq", @"IIListProvider`1'1");
            feature.AddKnownType(@"System.Linq.IPartition`1,System.Linq", @"IPartition`1'1");
            feature.AddKnownType(@"System.Linq.Enumerable+WhereArrayIterator`1,System.Linq", @"WhereArrayIterator`1'1");
            feature.AddKnownType(@"System.Linq.GroupedResultEnumerable`4,System.Linq", @"GroupedResultEnumerable`4'4");
            feature.AddKnownType(@"System.Linq.GroupedResultEnumerable`3,System.Linq", @"GroupedResultEnumerable`3'3");
            feature.AddKnownType(@"System.Linq.GroupedEnumerable`3,System.Linq", @"GroupedEnumerable`3'3");
            feature.AddKnownType(@"System.Linq.IGrouping`2,System.Linq", @"IGrouping`2'2");
            feature.AddKnownType(@"System.Linq.GroupedEnumerable`2,System.Linq", @"GroupedEnumerable`2'2");
            feature.AddKnownType(@"System.Linq.Lookup`2,System.Linq", @"Lookup`2'2");
            feature.AddKnownType(@"System.Linq.ILookup`2,System.Linq", @"ILookup`2'2");
            feature.AddKnownType(@"System.Linq.OrderedEnumerable`1,System.Linq", @"OrderedEnumerable`1'1");
            feature.AddKnownType(@"System.Linq.IOrderedEnumerable`1,System.Linq", @"IOrderedEnumerable`1'1");
            feature.AddKnownType(@"System.Linq.EmptyPartition`1,System.Linq", @"EmptyPartition`1'1");
            feature.AddKnownType(@"System.Linq.OrderedPartition`1,System.Linq", @"OrderedPartition`1'1");
            feature.AddKnownType(@"System.Linq.Buffer`1,System.Linq", @"Buffer`1'1");
            feature.AddKnownType(@"System.Linq.SystemCore_EnumerableDebugView`1,System.Linq", @"SystemCore_EnumerableDebugView`1'1");
            feature.AddKnownType(@"System.Linq.SystemCore_EnumerableDebugViewEmptyException,System.Linq", @"SystemCore_EnumerableDebugViewEmptyException");
            feature.AddKnownType(@"System.Linq.SystemCore_EnumerableDebugView,System.Linq", @"SystemCore_EnumerableDebugView");
            feature.AddKnownType(@"System.Linq.SystemLinq_GroupingDebugView`2,System.Linq", @"SystemLinq_GroupingDebugView`2'2");
            feature.AddKnownType(@"System.Linq.SystemLinq_LookupDebugView`2,System.Linq", @"SystemLinq_LookupDebugView`2'2");
            feature.AddKnownType(@"System.Linq.Grouping`2,System.Linq", @"Grouping`2'2");
            feature.AddKnownType(@"System.Linq.OrderedEnumerable`2,System.Linq", @"OrderedEnumerable`2'2");
            feature.AddKnownType(@"System.Linq.CachingComparer`1,System.Linq", @"CachingComparer`1'1");
            feature.AddKnownType(@"System.Linq.CachingComparer`2,System.Linq", @"CachingComparer`2'2");
            feature.AddKnownType(@"System.Linq.CachingComparerWithChild`2,System.Linq", @"CachingComparerWithChild`2'2");
            feature.AddKnownType(@"System.Linq.EnumerableSorter`1,System.Linq", @"EnumerableSorter`1'1");
            feature.AddKnownType(@"System.Linq.EnumerableSorter`2,System.Linq", @"EnumerableSorter`2'2");
            feature.AddKnownType(@"System.Linq.Set`1,System.Linq", @"Set`1'1");
            feature.AddKnownType(@"System.Linq.SingleLinkedNode`1,System.Linq", @"SingleLinkedNode`1'1");
            feature.AddKnownType(@"System.Linq.ThrowHelper,System.Linq", @"ThrowHelper");
            feature.AddKnownType(@"System.Linq.ExceptionArgument,System.Linq", @"ExceptionArgument");
            feature.AddKnownType(@"System.Linq.Utilities,System.Linq", @"Utilities");
            feature.AddKnownType(@"AdventureGrains.BlizzardWeather,AdventureGrains", @"AdventureGrains.BlizzardWeather");
            feature.AddKnownType(@"AdventureGrains.BossGrain,AdventureGrains", @"AdventureGrains.BossGrain");
            feature.AddKnownType(@"AdventureGrains.CloudyWeather,AdventureGrains", @"AdventureGrains.CloudyWeather");
            feature.AddKnownType(@"AdventureGrains.MonsterGrain,AdventureGrains", @"AdventureGrains.MonsterGrain");
            feature.AddKnownType(@"AdventureGrains.NightWeather,AdventureGrains", @"AdventureGrains.NightWeather");
            feature.AddKnownType(@"AdventureGrains.PlayerGrain,AdventureGrains", @"AdventureGrains.PlayerGrain");
            feature.AddKnownType(@"AdventureGrains.RoomGrain,AdventureGrains", @"AdventureGrains.RoomGrain");
            feature.AddKnownType(@"AdventureGrains.SunnyWeather,AdventureGrains", @"AdventureGrains.SunnyWeather");
        }
    }
}
#pragma warning restore 162
#pragma warning restore 219
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 649
#pragma warning restore 693
#pragma warning restore 1591
#pragma warning restore 1998
#endif
