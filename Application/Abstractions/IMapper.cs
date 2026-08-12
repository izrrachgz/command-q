namespace Application.Abstractions
{
    /// <summary>
    /// Interface contract for mapping between source and destination types. This is typically used to convert between
    /// entities and dto models, ensuring a clear separation of concerns and facilitating data transformation in the application.
    /// </summary>
    /// <typeparam name="TSource">The type of the source entity.</typeparam>
    /// <typeparam name="TDestination">The type of the destination model.</typeparam>
    public partial interface IMapper<TSource, TDestination> where TSource : class where TDestination : class
    {
        TDestination Map(TSource entity);

        TSource Map(TDestination model);
    }
}
