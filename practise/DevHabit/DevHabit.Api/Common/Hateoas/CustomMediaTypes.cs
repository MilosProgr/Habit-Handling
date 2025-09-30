using System.Collections.Immutable;

namespace DevHabit.Api.Common.Hateoas;

public static class CustomMediaTypeNames
{
    public static class Application
    {
        public const string Json = "application/json;";
        public const string JsonV1 = "application/json;v=1";
        public const string JsonV2 = "application/json;v=2";
        public const string HateoasJson = "application/vnd.dev-habit.hateoas+json";
        public const string HateoasJsonV1 = "application/vnd.dev-habit.hateoas.v1+json";
        public const string HateoasJsonV2 = "application/vnd.dev-habit.hateoas.v2+json";

        public static readonly IImmutableSet<string> HateoasMediaTypes =
            ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
            HateoasJson,
            HateoasJsonV1,
            HateoasJsonV2
    );
    }
}
