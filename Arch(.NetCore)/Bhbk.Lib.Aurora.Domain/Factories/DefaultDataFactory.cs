using AutoMapper;
using Bhbk.Lib.Aurora.Data_EF6.Infrastructure;

namespace Bhbk.Lib.Aurora.Domain.Factories
{
    public class DefaultDataFactory
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public DefaultDataFactory(IUnitOfWork uow)
        {

        }

        public void Create()
        {

        }

        public void Destroy()
        {

        }
    }
}
