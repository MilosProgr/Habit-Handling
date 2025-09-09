using System.Dynamic;

namespace DevHabit.Api.Common.DataShaping;

public interface IShapedCollectionResult
{
    IReadOnlyCollection<ExpandoObject> Data { get; init; }
}
