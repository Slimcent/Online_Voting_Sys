using AutoMapper;
using OnlineVoting.Models.Pagination;

namespace OnlineVoting.Api.Mapper
{
    public class PagedResponseProfile : Profile
    {
        public PagedResponseProfile()
        {
            CreateMap(typeof(PagedList<>), typeof(PagedResponse<>))
                .ConvertUsing(typeof(PagedListToPagedResponseConverter<,>));
        }

        public class PagedListToPagedResponseConverter<TSource, TDestination> : ITypeConverter<PagedList<TSource>, PagedResponse<TDestination>> where TDestination : class
        {
            public PagedResponse<TDestination> Convert(PagedList<TSource> source, PagedResponse<TDestination> destination, ResolutionContext context)
            {
                var list = source.ToList();
                var items = context.Mapper.Map<IEnumerable<TSource>, IEnumerable<TDestination>>(list);

                var pagedResponse = new PagedResponse<TDestination>
                {
                    MetaData = source.MetaData,
                    Items = items
                };
                return pagedResponse;
            }
        }
    }
}
