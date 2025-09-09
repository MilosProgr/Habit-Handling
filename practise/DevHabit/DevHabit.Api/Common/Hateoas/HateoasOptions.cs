using DevHabit.Api.Common.DataShaping;
using DevHabit.Api.DTOs.Common;

namespace DevHabit.Api.Common.Hateoas;

public class HateoasOptions<T>
{
    /// <summary>
    /// Funkcija koja kreira linkove za pojedinačni item
    /// </summary>
    public Func<T, IEnumerable<LinkDto>>? ItemLinksFactory { get; set; }

    /// <summary>
    /// Funkcija koja kreira linkove za kolekciju
    /// </summary>
    public Func<ShapedPaginationResult<T>, IEnumerable<LinkDto>>? CollectionLinksFactory { get; set; }

    /// <summary>
    /// Accept header iz HTTP zahteva
    /// </summary>
    public string? AcceptHeader { get; set; }
}
